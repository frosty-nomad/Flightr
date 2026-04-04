using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Flightr.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly HttpClient _client;

    public LoginModel(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("Api");
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? AccountNotFoundMessage { get; set; }
    public string? LoginErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var response = await _client.PostAsJsonAsync("api/account/login", new
        {
            email = Input.Email,
            password = Input.Password
        });

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var (accessToken, pilotName, email) = ReadLoginSuccess(responseBody);
            var displayName = string.IsNullOrWhiteSpace(pilotName) ? Input.Email : pilotName;
            var userEmail = string.IsNullOrWhiteSpace(email) ? Input.Email : email;
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, displayName),
                new(ClaimTypes.Email, userEmail)
            };

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                claims.Add(new Claim("access_token", accessToken));
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name,
                ClaimTypes.Role);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
                });

            return RedirectToPage("/FlightLogs/Index");
        }

        var errorBody = await response.Content.ReadAsStringAsync();
        var (errorCode, _) = ReadLoginError(errorBody);
        
        if (errorCode == "AccountNotFound")
        {
            AccountNotFoundMessage = $"No account found for {Input.Email}. ";
            LoginErrorMessage = "Account not found. Please create an account to get started.";
        }
        else if (errorCode == "InvalidPassword" || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            LoginErrorMessage = "Incorrect email or password. Please try again.";
        }
        else
        {
            LoginErrorMessage = string.IsNullOrWhiteSpace(errorBody)
                ? "We couldn't sign you in right now. Please try again in a moment."
                : "We couldn't sign you in right now. Please try again.";
        }

        return Page();
    }

    private static (string? AccessToken, string? PilotName, string? Email) ReadLoginSuccess(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;
            var accessToken = TryGetStringProperty(root, "accessToken", "AccessToken");
            var pilotName = TryGetStringProperty(root, "pilotName", "PilotName");
            var email = TryGetStringProperty(root, "email", "Email");

            return (accessToken, pilotName, email);
        }
        catch (JsonException)
        {
        }

        return (null, null, null);
    }

    private static string? TryGetStringProperty(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (element.TryGetProperty(name, out var property))
            {
                return property.GetString();
            }
        }

        return null;
    }

    private static (string? ErrorCode, string? Message) ReadLoginError(string errorBody)
    {
        try
        {
            using var document = JsonDocument.Parse(errorBody);
            if (document.RootElement.TryGetProperty("errorCode", out var errorCodeProperty))
            {
                var message = document.RootElement.TryGetProperty("message", out var messageProperty)
                    ? messageProperty.GetString()
                    : null;
                return (errorCodeProperty.GetString(), message);
            }
        }
        catch (JsonException)
        {
        }

        return (null, null);
    }
}
