using Flightr.Api.Contracts;
using Flightr.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
    private readonly Services.IEmailSender? _emailSender;

    public AccountController(UserManager<ApplicationUser> userManager, IConfiguration configuration, Services.IEmailSender? emailSender = null)
    {
        _userManager = userManager;
        _configuration = configuration;
        _emailSender = emailSender;
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

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var email = request.Email.Trim();
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return BadRequest(new LoginErrorResponse(
                "AccountNotFound",
                "No account exists for that email address."));
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Build reset link pointing to the web app's reset page.
        // Detect Codespace URL if running in GitHub Codespaces, otherwise use config
        var frontendUrl = GetFrontendUrl();
        var encodedToken = System.Net.WebUtility.UrlEncode(token);
        var encodedEmail = System.Net.WebUtility.UrlEncode(email);
        var resetLink = $"{frontendUrl.TrimEnd('/')}/Account/ResetPassword?email={encodedEmail}&token={encodedToken}";
        
        // Log the generated reset link for debugging
        var logger = HttpContext.RequestServices.GetService(typeof(ILogger<AccountController>)) as ILogger<AccountController>;
        logger?.LogInformation("Generated reset link: {ResetLink}", resetLink);

        // Send email if an email sender is available. If not configured, the SmtpEmailSender will log instead.
        try
        {
            if (_emailSender != null)
            {
                var subject = "Flightr Password Reset";
                var body = $"<p>You requested a password reset. Click the link below to reset your password:</p><p><a href=\"{resetLink}\">Reset your password</a></p>";
                await _emailSender.SendEmailAsync(email, subject, body);
            }
        }
        catch
        {
            // Do not reveal email failures to the client; still return success message.
        }

        return Ok(new ForgotPasswordResponse(
            $"If an account exists for {email}, a password reset email has been sent.",
            null));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return BadRequest(new LoginErrorResponse("AccountNotFound", "No account found for that email address."));
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (result.Succeeded)
        {
            return Ok(new { message = "Password reset successful." });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }

        return ValidationProblem(ModelState);
    }

    private string GetFrontendUrl()
    {
        // Check if running in GitHub Codespaces
        if (Environment.GetEnvironmentVariable("CODESPACES") == "true")
        {
            var codespaceName = Environment.GetEnvironmentVariable("CODESPACE_NAME");
            var domain = Environment.GetEnvironmentVariable("GITHUB_CODESPACES_PORT_FORWARDING_DOMAIN");
            if (!string.IsNullOrWhiteSpace(codespaceName) && !string.IsNullOrWhiteSpace(domain))
            {
                return $"https://{codespaceName}-5170.{domain}";
            }
        }

        // Fallback to configuration
        return _configuration["Frontend:Url"] ?? "http://localhost:5170";
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(new GetProfileResponse(
            user.PilotName,
            user.LicenseNumber,
            user.LicenseExpirationDate,
            user.LicenseGoal));
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized();
        }

        user.PilotName = request.PilotName;
        user.LicenseNumber = request.LicenseNumber;
        user.LicenseExpirationDate = request.LicenseExpirationDate;
        user.LicenseGoal = request.LicenseGoal;

        var result = await _userManager.UpdateAsync(user);
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
}
