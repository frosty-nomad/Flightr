using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Flightr.Web.Pages.FlightLogs;

[Authorize]
public class EditModel : PageModel
{
    private readonly HttpClient _client;

    public EditModel(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("Api");
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime FlightDate { get; set; }

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

    public async Task<IActionResult> OnGetAsync()
    {
        var log = await _client.GetFromJsonAsync<FlightLogDetailDto>($"api/flight-logs/{Id}");
        if (log is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            FlightDate = log.FlightDate,
            AircraftType = log.AircraftType,
            TailNumber = log.TailNumber,
            DepartureAirport = log.DepartureAirport,
            ArrivalAirport = log.ArrivalAirport,
            Route = log.Route,
            TotalHours = log.TotalHours,
            PicHours = log.PicHours,
            SicHours = log.SicHours,
            CrossCountryHours = log.CrossCountryHours,
            NightHours = log.NightHours,
            InstrumentHours = log.InstrumentHours,
            TakeoffsDay = log.TakeoffsDay,
            TakeoffsNight = log.TakeoffsNight,
            LandingsDay = log.LandingsDay,
            LandingsNight = log.LandingsNight,
            Remarks = log.Remarks
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var response = await _client.PutAsJsonAsync($"api/flight-logs/{Id}", new
        {
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
            ? "Unable to update the flight log."
            : "Unable to update the flight log: " + errorBody;
        ModelState.AddModelError(string.Empty, message);

        return Page();
    }

    private class FlightLogDetailDto
    {
        public DateTime FlightDate { get; set; }

        public string AircraftType { get; set; } = string.Empty;


        public string? TailNumber { get; set; }

        public string? DepartureAirport { get; set; }

        public string? ArrivalAirport { get; set; }

        public string? Route { get; set; }

        public decimal TotalHours { get; set; }

        public decimal PicHours { get; set; }

        public decimal SicHours { get; set; }

        public decimal CrossCountryHours { get; set; }

        public decimal NightHours { get; set; }

        public decimal InstrumentHours { get; set; }

        public int TakeoffsDay { get; set; }

        public int TakeoffsNight { get; set; }

        public int LandingsDay { get; set; }

        public int LandingsNight { get; set; }

        public string? Remarks { get; set; }
    }
}
