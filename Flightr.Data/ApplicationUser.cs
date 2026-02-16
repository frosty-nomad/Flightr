using Microsoft.AspNetCore.Identity;

namespace Flightr.Data;

public class ApplicationUser : IdentityUser
{
    public string PilotName { get; set; } = string.Empty;

    public string LicenseNumber { get; set; } = string.Empty;

    public DateOnly? LicenseExpirationDate { get; set; }

    public string LicenseGoal { get; set; } = string.Empty;
}
