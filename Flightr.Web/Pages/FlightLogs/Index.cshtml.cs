using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Flightr.Web.Pages.FlightLogs;

[Authorize]
public class IndexModel : PageModel
{
    private readonly HttpClient _client;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("Api");
    }

    public List<FlightLogDto> Logs { get; private set; } = new();

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync()
    {
        try
        {
            var results = await _client.GetFromJsonAsync<List<FlightLogDto>>("api/flight-logs");
            Logs = results ?? new List<FlightLogDto>();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Unable to load flight logs: " + ex.Message;
        }
    }

    public class FlightLogDto
    {
        public int Id { get; set; }

        public DateTime FlightDate { get; set; }

        public string AircraftType { get; set; } = string.Empty;

        public string? Route { get; set; }

        public decimal TotalHours { get; set; }

        public decimal PicHours { get; set; }

        public decimal SicHours { get; set; }

        public decimal NightHours { get; set; }

        public decimal CrossCountryHours { get; set; }

        public decimal InstrumentHours { get; set; }

        public int LandingsDay { get; set; }

        public int LandingsNight { get; set; }
    }
}
