using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Flightr.Web.Pages.Account;

public class ResetPasswordModel : PageModel
{
    private readonly HttpClient _client;

    public ResetPasswordModel(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("Api");
    }

    [BindProperty(SupportsGet = true)]
    public InputModel Input { get; set; } = new();

    public string? StatusMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public IActionResult OnGet()
    {
        if (string.IsNullOrWhiteSpace(Input.Email) || string.IsNullOrWhiteSpace(Input.Token))
        {
            ErrorMessage = "The reset link is missing required information.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var response = await _client.PostAsJsonAsync("api/account/reset-password", new
        {
            email = Input.Email,
            token = Input.Token,
            newPassword = Input.NewPassword
        });

        var responseBody = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            StatusMessage = "Your password has been reset. You can sign in now.";
            return Page();
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (root.TryGetProperty("errorCode", out var errorCodeElement))
            {
                var errorCode = errorCodeElement.GetString();
                var message = root.TryGetProperty("message", out var messageElement)
                    ? messageElement.GetString()
                    : null;

                if (errorCode == "AccountNotFound")
                {
                    ErrorMessage = message ?? "No account was found for that email address.";
                    return Page();
                }

                ErrorMessage = message ?? "Unable to reset your password.";
                return Page();
            }

            if (root.TryGetProperty("errors", out var errorsElement))
            {
                foreach (var error in errorsElement.EnumerateObject())
                {
                    if (error.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var msg in error.Value.EnumerateArray())
                        {
                            ModelState.AddModelError(error.Name, msg.GetString() ?? "Unknown error");
                        }
                    }
                }
            }
            else if (root.TryGetProperty("message", out var messageElement))
            {
                ErrorMessage = messageElement.GetString();
            }
            else if (root.TryGetProperty("Message", out var uppercaseMessageElement))
            {
                ErrorMessage = uppercaseMessageElement.GetString();
            }
        }
        catch (JsonException)
        {
            ErrorMessage = string.IsNullOrWhiteSpace(responseBody)
                ? "Unable to reset your password."
                : responseBody;
        }

        if (string.IsNullOrWhiteSpace(ErrorMessage) && ModelState.ErrorCount == 0)
        {
            ErrorMessage = "Unable to reset your password.";
        }

        return Page();
    }
}