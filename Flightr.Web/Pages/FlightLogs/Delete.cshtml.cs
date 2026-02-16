using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Flightr.Web.Pages.FlightLogs;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly HttpClient _client;

    public DeleteModel(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("Api");
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public string? Summary { get; private set; }

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var log = await _client.GetFromJsonAsync<FlightLogSummaryDto>($"api/flight-logs/{Id}");
        if (log is null)
        {
            return NotFound();
        }

        Summary = $"{log.FlightDate:yyyy-MM-dd} ({log.AircraftType})";
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var response = await _client.DeleteAsync($"api/flight-logs/{Id}");
        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage("/FlightLogs/Index");
        }

        var errorBody = await response.Content.ReadAsStringAsync();
        ErrorMessage = string.IsNullOrWhiteSpace(errorBody)
            ? "Unable to delete the flight log."
            : "Unable to delete the flight log: " + errorBody;
        return Page();
    }

    private class FlightLogSummaryDto
    {
        public DateTime FlightDate { get; set; }

        public string AircraftType { get; set; } = string.Empty;
    }
}
