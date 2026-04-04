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

    public List<string> AircraftTypes { get; private set; } = new();

    public class InputModel
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime FlightDate { get; set; }

        [Required]
        [MaxLength(32)]
        public string AircraftType { get; set; } = string.Empty;

        [Required]
        [MaxLength(16)]
        public string TailNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(8)]
        public string DepartureAirport { get; set; } = string.Empty;

        [Required]
        [MaxLength(8)]
        public string ArrivalAirport { get; set; } = string.Empty;

        [Required]
        [MaxLength(128)]
        public string Route { get; set; } = string.Empty;

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
        await LoadAircraftTypesAsync();

        var log = await _client.GetFromJsonAsync<FlightLogDetailDto>($"api/flight-logs/{Id}");
        if (log is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            FlightDate = log.FlightDate,
            AircraftType = log.AircraftType,
            TailNumber = log.TailNumber ?? string.Empty,
            DepartureAirport = log.DepartureAirport ?? string.Empty,
            ArrivalAirport = log.ArrivalAirport ?? string.Empty,
            Route = log.Route ?? string.Empty,
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

        if (!AircraftTypes.Contains(Input.AircraftType, StringComparer.OrdinalIgnoreCase))
        {
            AircraftTypes.Add(Input.AircraftType);
            AircraftTypes = AircraftTypes
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(type => type)
                .ToList();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadAircraftTypesAsync();

        if (!string.IsNullOrWhiteSpace(Input.AircraftType)
            && !AircraftTypes.Contains(Input.AircraftType, StringComparer.OrdinalIgnoreCase))
        {
            AircraftTypes.Add(Input.AircraftType);
            AircraftTypes = AircraftTypes
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(type => type)
                .ToList();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        Input.TotalHours = Input.PicHours
            + Input.SicHours
            + Input.CrossCountryHours
            + Input.NightHours
            + Input.InstrumentHours;

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

    private async Task LoadAircraftTypesAsync()
    {
        try
        {
            var types = await _client.GetFromJsonAsync<List<string>>("api/flight-logs/aircraft-types");
            AircraftTypes = types ?? new List<string>();
        }
        catch
        {
            AircraftTypes = new List<string>();
        }
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
