using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Flightr.Web.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly HttpClient _client;

    public RegisterModel(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("Api");
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? RegistrationErrorMessage { get; set; }
    public List<string> RegistrationErrors { get; set; } = new();

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(128)]
        public string PilotName { get; set; } = string.Empty;

        [Required]
        [MaxLength(32)]
        public string LicenseNumber { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateOnly? LicenseExpirationDate { get; set; }

        [Required]
        [MaxLength(32)]
        public string LicenseGoal { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
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

        var response = await _client.PostAsJsonAsync("api/account/register", new
        {
            email = Input.Email,
            password = Input.Password,
            pilotName = Input.PilotName,
            licenseNumber = Input.LicenseNumber,
            licenseExpirationDate = Input.LicenseExpirationDate,
            licenseGoal = Input.LicenseGoal
        });

        if (response.IsSuccessStatusCode)
        {
            TempData["RegistrationStatusMessage"] = "Account created. You can sign in now.";
            return RedirectToPage("/Account/Login");
        }

        // Parse error response to extract validation errors
        var errorBody = await response.Content.ReadAsStringAsync();
        try
        {
            using var document = JsonDocument.Parse(errorBody);
            var root = document.RootElement;

            // Handle validation problem format
            if (root.TryGetProperty("errors", out var errorsElement))
            {
                foreach (var error in errorsElement.EnumerateObject())
                {
                    if (error.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var msg in error.Value.EnumerateArray())
                        {
                            RegistrationErrors.Add(msg.GetString() ?? "Unknown error");
                        }
                    }
                }
            }

            // Handle single error message
            if (root.TryGetProperty("detail", out var detailElement))
            {
                RegistrationErrorMessage = detailElement.GetString();
            }
        }
        catch (JsonException)
        {
            // If JSON parsing fails, use raw response
            RegistrationErrorMessage = errorBody;
        }

        if (RegistrationErrors.Count == 0 && string.IsNullOrWhiteSpace(RegistrationErrorMessage))
        {
            RegistrationErrorMessage = "Registration failed. Please review the details and try again.";
        }

        ModelState.AddModelError(string.Empty, RegistrationErrorMessage ?? "Registration failed.");
        return Page();
    }
}
