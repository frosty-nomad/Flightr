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
