using System.ComponentModel.DataAnnotations;

namespace Flightr.Data;

public class AircraftType
{
    public int Id { get; set; }

    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;
}