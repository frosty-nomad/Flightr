using System;
using System.ComponentModel.DataAnnotations;

namespace Flightr.Api.Contracts;

public record RegisterAccountRequest(
    [Required] string Email,
    [Required] string Password,
    [Required] string PilotName,
    [Required] string LicenseNumber,
    DateOnly? LicenseExpirationDate,
    [Required] string LicenseGoal);

public record LoginRequest(
    [Required] string Email,
    [Required] string Password);

public record LoginResponse(
    string AccessToken,
    string PilotName,
    string Email);

public record LoginErrorResponse(
    string ErrorCode,
    string Message);
