using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Flightr.Web.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly HttpClient _client;

    public ForgotPasswordModel(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("Api");
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? StatusMessage { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ResetLink { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
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

        var response = await _client.PostAsJsonAsync("api/account/forgot-password", new
        {
            email = Input.Email
        });

        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            if (TryReadKnownError(responseBody))
            {
                return Page();
            }

            TryReadValidationErrors(responseBody);
            if (ModelState.ErrorCount == 0)
            {
                ErrorMessage = "Unable to generate a password reset link.";
            }

            return Page();
        }


        // On success, display a confirmation message indicating a reset email has been sent.
        ErrorMessage = null;
        ResetLink = null;
        StatusMessage = $"A password reset email has been sent to {Input.Email}.";

        return Page();
    }

    private bool TryReadKnownError(string responseBody)
    {
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
                    ErrorMessage = message ?? "No account exists for that email address.";
                    return true;
                }

                ErrorMessage = message ?? "Unable to generate a password reset link.";
                return true;
            }
        }
        catch (JsonException)
        {
        }

        return false;
    }

    private void TryReadValidationErrors(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

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
        }
        catch (JsonException)
        {
        }
    }
}