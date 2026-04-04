using Flightr.Api.Contracts;
using Flightr.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Flightr.Api.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterAccountRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            PilotName = request.PilotName,
            LicenseNumber = request.LicenseNumber,
            LicenseExpirationDate = request.LicenseExpirationDate,
            LicenseGoal = request.LicenseGoal
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (result.Succeeded)
        {
            return Ok();
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }

        return ValidationProblem(ModelState);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Unauthorized(new LoginErrorResponse("AccountNotFound", "No account found with this email address."));
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            return Unauthorized(new LoginErrorResponse("InvalidPassword", "Invalid email or password."));
        }

        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            return StatusCode(500, new LoginErrorResponse("ConfigurationError", "Authentication is not configured."));
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.PilotName),
            new(ClaimTypes.Email, user.Email ?? request.Email)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new LoginResponse(
            accessToken,
            user.PilotName,
            user.Email ?? request.Email));
    }
}
