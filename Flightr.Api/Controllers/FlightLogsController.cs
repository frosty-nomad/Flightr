using System.Text;
using Flightr.Api.Contracts;
using Flightr.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flightr.Api.Controllers;

[ApiController]
[Route("api/flight-logs")]
[Authorize]
public class FlightLogsController : ControllerBase
{
    private readonly FlightrDbContext _dbContext;

    public FlightLogsController(FlightrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FlightLog>>> GetAll(CancellationToken cancellationToken)
    {
        var logs = await _dbContext.FlightLogs
            .AsNoTracking()
            .OrderByDescending(log => log.FlightDate)
            .ToListAsync(cancellationToken);

        return Ok(logs);
    }

    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] string? pilotId, CancellationToken cancellationToken)
    {
        var query = _dbContext.FlightLogs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(pilotId))
        {
            query = query.Where(log => log.PilotId == pilotId);
        }

        var logs = await query
            .OrderBy(log => log.FlightDate)
            .ToListAsync(cancellationToken);

        var csv = BuildCsv(logs);
        var fileName = string.IsNullOrWhiteSpace(pilotId)
            ? "flight-logs.csv"
            : $"flight-logs-{pilotId}.csv";

        return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FlightLog>> GetById(int id, CancellationToken cancellationToken)
    {
        var log = await _dbContext.FlightLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(entry => entry.Id == id, cancellationToken);

        return log is null ? NotFound() : Ok(log);
    }

    [HttpPost]
    public async Task<ActionResult<FlightLog>> Create(CreateFlightLogRequest request, CancellationToken cancellationToken)
    {
        var log = new FlightLog
        {
            PilotId = request.PilotId,
            FlightDate = request.FlightDate,
            AircraftType = request.AircraftType,
            TailNumber = request.TailNumber,
            DepartureAirport = request.DepartureAirport,
            ArrivalAirport = request.ArrivalAirport,
            Route = request.Route,
            TotalHours = request.TotalHours,
            PicHours = request.PicHours,
            SicHours = request.SicHours,
            CrossCountryHours = request.CrossCountryHours,
            NightHours = request.NightHours,
            InstrumentHours = request.InstrumentHours,
            TakeoffsDay = request.TakeoffsDay,
            TakeoffsNight = request.TakeoffsNight,
            LandingsDay = request.LandingsDay,
            LandingsNight = request.LandingsNight,
            Remarks = request.Remarks,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.FlightLogs.Add(log);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = log.Id }, log);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<FlightLog>> Update(int id, UpdateFlightLogRequest request, CancellationToken cancellationToken)
    {
        var log = await _dbContext.FlightLogs
            .FirstOrDefaultAsync(entry => entry.Id == id, cancellationToken);

        if (log is null)
        {
            return NotFound();
        }

        log.FlightDate = request.FlightDate;
        log.AircraftType = request.AircraftType;
        log.TailNumber = request.TailNumber;
        log.DepartureAirport = request.DepartureAirport;
        log.ArrivalAirport = request.ArrivalAirport;
        log.Route = request.Route;
        log.TotalHours = request.TotalHours;
        log.PicHours = request.PicHours;
        log.SicHours = request.SicHours;
        log.CrossCountryHours = request.CrossCountryHours;
        log.NightHours = request.NightHours;
        log.InstrumentHours = request.InstrumentHours;
        log.TakeoffsDay = request.TakeoffsDay;
        log.TakeoffsNight = request.TakeoffsNight;
        log.LandingsDay = request.LandingsDay;
        log.LandingsNight = request.LandingsNight;
        log.Remarks = request.Remarks;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(log);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var log = await _dbContext.FlightLogs
            .FirstOrDefaultAsync(entry => entry.Id == id, cancellationToken);

        if (log is null)
        {
            return NotFound();
        }

        _dbContext.FlightLogs.Remove(log);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static string BuildCsv(IEnumerable<FlightLog> logs)
    {
        var builder = new StringBuilder();
        builder.AppendLine("FlightDate,AircraftType,TailNumber,DepartureAirport,ArrivalAirport,Route,TotalHours,PicHours,SicHours,CrossCountryHours,NightHours,InstrumentHours,TakeoffsDay,TakeoffsNight,LandingsDay,LandingsNight,Remarks");

        foreach (var log in logs)
        {
            builder.Append(log.FlightDate.ToString("yyyy-MM-dd"));
            builder.Append(',');
            builder.Append(EscapeCsv(log.AircraftType));
            builder.Append(',');
            builder.Append(EscapeCsv(log.TailNumber));
            builder.Append(',');
            builder.Append(EscapeCsv(log.DepartureAirport));
            builder.Append(',');
            builder.Append(EscapeCsv(log.ArrivalAirport));
            builder.Append(',');
            builder.Append(EscapeCsv(log.Route));
            builder.Append(',');
            builder.Append(log.TotalHours);
            builder.Append(',');
            builder.Append(log.PicHours);
            builder.Append(',');
            builder.Append(log.SicHours);
            builder.Append(',');
            builder.Append(log.CrossCountryHours);
            builder.Append(',');
            builder.Append(log.NightHours);
            builder.Append(',');
            builder.Append(log.InstrumentHours);
            builder.Append(',');
            builder.Append(log.TakeoffsDay);
            builder.Append(',');
            builder.Append(log.TakeoffsNight);
            builder.Append(',');
            builder.Append(log.LandingsDay);
            builder.Append(',');
            builder.Append(log.LandingsNight);
            builder.Append(',');
            builder.Append(EscapeCsv(log.Remarks));
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        if (!needsQuotes)
        {
            return value;
        }

        return '"' + value.Replace("\"", "\"\"") + '"';
    }
}
