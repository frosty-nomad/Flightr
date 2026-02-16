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

        var response = await _client.PostAsJsonAsync("login", new
        {
            email = Input.Email,
            password = Input.Password
        });

        if (response.IsSuccessStatusCode)
        {
            var accessToken = await ReadAccessTokenAsync(response);
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, Input.Email),
                new(ClaimTypes.Email, Input.Email)
            };

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                claims.Add(new Claim("access_token", accessToken));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
                });

            TempData["StatusMessage"] = "Signed in successfully.";
            return RedirectToPage("/Index");
        }

        var errorBody = await response.Content.ReadAsStringAsync();
        var message = string.IsNullOrWhiteSpace(errorBody)
            ? "Login failed. Check your credentials and try again."
            : "Login failed: " + errorBody;

        ModelState.AddModelError(string.Empty, message);
        return Page();
    }

    private static async Task<string?> ReadAccessTokenAsync(HttpResponseMessage response)
    {
        try
        {
            using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);
            if (document.RootElement.TryGetProperty("accessToken", out var tokenProperty))
            {
                return tokenProperty.GetString();
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }
}
