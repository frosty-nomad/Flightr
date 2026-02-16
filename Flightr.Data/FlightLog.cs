using System;
using System.ComponentModel.DataAnnotations;

namespace Flightr.Data;

public class FlightLog
{
    public int Id { get; set; }

    [Required]
    [MaxLength(64)]
    public string PilotId { get; set; } = string.Empty;

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


    [MaxLength(512)]
    public string? Remarks { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
