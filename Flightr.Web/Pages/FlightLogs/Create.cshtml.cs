using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Flightr.Web.Pages.FlightLogs;

[Authorize]
public class CreateModel : PageModel
{
    private readonly HttpClient _client;

    public CreateModel(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("Api");
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime FlightDate { get; set; } = DateTime.Today;

        [Required]
        [MaxLength(32)]
        public string AircraftType { get; set; } = string.Empty;

        [MaxLength(16)]
        public string? TailNumber { get; set; }

        [MaxLength(8)]
        public string? DepartureAirport { get; set; }

        [MaxLength(8)]
        public string? ArrivalAirport { get; set; }

        [MaxLength(128)]
        public string? Route { get; set; }

        [Range(0, 999)]
        public decimal TotalHours { get; set; }

        [Range(0, 999)]
        public decimal PicHours { get; set; }

        [Range(0, 999)]
        public decimal SicHours { get; set; }

        [Range(0, 999)]
        public decimal CrossCountryHours { get; set; }

        [Range(0, 999)]
        public decimal NightHours { get; set; }

        [Range(0, 999)]
        public decimal InstrumentHours { get; set; }

        [Range(0, 999)]
        public int TakeoffsDay { get; set; }

        [Range(0, 999)]
        public int TakeoffsNight { get; set; }

        [Range(0, 999)]
        public int LandingsDay { get; set; }

        [Range(0, 999)]
        public int LandingsNight { get; set; }


        [MaxLength(512)]
        public string? Remarks { get; set; }
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

        var pilotId = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(pilotId))
        {
            ModelState.AddModelError(string.Empty, "Unable to resolve the pilot identifier. Please log in again.");
            return Page();
        }

        var response = await _client.PostAsJsonAsync("api/flight-logs", new
        {
            PilotId = pilotId,
            Input.FlightDate,
            Input.AircraftType,
            Input.TailNumber,
            Input.DepartureAirport,
            Input.ArrivalAirport,
            Input.Route,
            Input.TotalHours,
            Input.PicHours,
            Input.SicHours,
            Input.CrossCountryHours,
            Input.NightHours,
            Input.InstrumentHours,
            Input.TakeoffsDay,
            Input.TakeoffsNight,
            Input.LandingsDay,
            Input.LandingsNight,
            Input.Remarks
        });

        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage("/FlightLogs/Index");
        }

        var errorBody = await response.Content.ReadAsStringAsync();
        var message = string.IsNullOrWhiteSpace(errorBody)
            ? "Unable to save the flight log."
            : "Unable to save the flight log: " + errorBody;
        ModelState.AddModelError(string.Empty, message);

        return Page();
    }
}
