using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Flightr.Web.Pages.Account;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly HttpClient _client;

    public ProfileModel(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("Api");
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? StatusMessage { get; set; }
    public List<string> ErrorMessages { get; set; } = new();

    public class InputModel
    {
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
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var response = await _client.GetAsync("api/account/profile");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ErrorMessages.Add($"Failed to load profile: {response.StatusCode}");
                return Page();
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseBody);
            var profile = document.RootElement;
            
            Input.PilotName = profile.GetProperty("pilotName").GetString() ?? string.Empty;
            Input.LicenseNumber = profile.GetProperty("licenseNumber").GetString() ?? string.Empty;
            
            var expirationDateStr = profile.GetProperty("licenseExpirationDate").GetString();
            if (!string.IsNullOrEmpty(expirationDateStr) && DateOnly.TryParse(expirationDateStr, out var expirationDate))
            {
                Input.LicenseExpirationDate = expirationDate;
            }
            
            Input.LicenseGoal = profile.GetProperty("licenseGoal").GetString() ?? string.Empty;

            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessages.Add($"An error occurred: {ex.Message}");
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var response = await _client.PutAsJsonAsync("api/account/profile", new
        {
            pilotName = Input.PilotName,
            licenseNumber = Input.LicenseNumber,
            licenseExpirationDate = Input.LicenseExpirationDate,
            licenseGoal = Input.LicenseGoal
        });

        if (response.IsSuccessStatusCode)
        {
            StatusMessage = "Profile updated successfully.";
            return Page();
        }

        var errorBody = await response.Content.ReadAsStringAsync();
        try
        {
            using var document = JsonDocument.Parse(errorBody);
            var root = document.RootElement;

            if (root.TryGetProperty("errors", out var errorsElement))
            {
                foreach (var error in errorsElement.EnumerateObject())
                {
                    if (error.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var msg in error.Value.EnumerateArray())
                        {
                            ErrorMessages.Add(msg.GetString() ?? "Unknown error");
                        }
                    }
                }
            }
        }
        catch
        {
            ErrorMessages.Add("An error occurred while updating the profile.");
        }

        return Page();
    }
}
