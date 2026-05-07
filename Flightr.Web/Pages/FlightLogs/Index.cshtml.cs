using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var response = await _client.DeleteAsync($"api/flight-logs/{id}");
        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage();
        }

        try
        {
            var results = await _client.GetFromJsonAsync<List<FlightLogDto>>("api/flight-logs");
            Logs = results ?? new List<FlightLogDto>();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Unable to load flight logs: " + ex.Message;
        }

        var errorBody = await response.Content.ReadAsStringAsync();
        ErrorMessage = string.IsNullOrWhiteSpace(errorBody)
            ? "Unable to delete the flight log."
            : "Unable to delete the flight log: " + errorBody;

        return Page();
    }

    public async Task<IActionResult> OnGetDownloadAsync()
    {
        try
        {
            var response = await _client.GetAsync("api/flight-logs/download");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var csvContent = await response.Content.ReadAsStreamAsync();
            var fileName = $"flight-log-{DateTime.Now:yyyy-MM-dd-HH-mm}.csv";
            return File(csvContent, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Unable to download flight logs: " + ex.Message;
            return Page();
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
