using System;

namespace Flightr.Api.Contracts;

public record CreateFlightLogRequest(
    string PilotId,
    DateTime FlightDate,
    string AircraftType,
    string? TailNumber,
    string? DepartureAirport,
    string? ArrivalAirport,
    string? Route,
    decimal TotalHours,
    decimal PicHours,
    decimal SicHours,
    decimal CrossCountryHours,
    decimal NightHours,
    decimal InstrumentHours,
    int TakeoffsDay,
    int TakeoffsNight,
    int LandingsDay,
    int LandingsNight,
    string? Remarks);

public record UpdateFlightLogRequest(
    DateTime FlightDate,
    string AircraftType,
    string? TailNumber,
    string? DepartureAirport,
    string? ArrivalAirport,
    string? Route,
    decimal TotalHours,
    decimal PicHours,
    decimal SicHours,
    decimal CrossCountryHours,
    decimal NightHours,
    decimal InstrumentHours,
    int TakeoffsDay,
    int TakeoffsNight,
    int LandingsDay,
    int LandingsNight,
    string? Remarks);
