using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
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
            TempData["StatusMessage"] = "Account created. You can sign in now.";
            return RedirectToPage("/Account/Login");
        }

        var errorBody = await response.Content.ReadAsStringAsync();
        var message = string.IsNullOrWhiteSpace(errorBody)
            ? "Registration failed. Please review the details and try again."
            : "Registration failed: " + errorBody;

        ModelState.AddModelError(string.Empty, message);
        return Page();
    }
}
