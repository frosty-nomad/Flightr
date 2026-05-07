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

public record ForgotPasswordRequest(
    [Required] string Email);

public record ForgotPasswordResponse(
    string Message,
    string? ResetToken);

public record ResetPasswordRequest(
    [Required] string Email,
    [Required] string Token,
    [Required] string NewPassword);

public record GetProfileResponse(
    string PilotName,
    string LicenseNumber,
    DateOnly? LicenseExpirationDate,
    string LicenseGoal);

public record UpdateProfileRequest(
    [Required] string PilotName,
    [Required] string LicenseNumber,
    DateOnly? LicenseExpirationDate,
    [Required] string LicenseGoal);
