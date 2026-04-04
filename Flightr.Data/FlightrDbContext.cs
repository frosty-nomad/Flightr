using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Flightr.Data;

public class FlightrDbContext : IdentityDbContext<ApplicationUser>
{
    public FlightrDbContext(DbContextOptions<FlightrDbContext> options)
        : base(options)
    {
    }

    public DbSet<FlightLog> FlightLogs => Set<FlightLog>();
    public DbSet<AircraftType> AircraftTypes => Set<AircraftType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AircraftType>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(64);
            entity.HasData(
                new AircraftType { Id = 1, Name = "Beechcraft Bonanza" },
                new AircraftType { Id = 2, Name = "Cessna 150" },
                new AircraftType { Id = 3, Name = "Cessna 152" },
                new AircraftType { Id = 4, Name = "Cessna 172" },
                new AircraftType { Id = 5, Name = "Cessna 182" },
                new AircraftType { Id = 6, Name = "Cirrus SR20" },
                new AircraftType { Id = 7, Name = "Cirrus SR22" },
                new AircraftType { Id = 8, Name = "Diamond DA20" },
                new AircraftType { Id = 9, Name = "Diamond DA40" },
                new AircraftType { Id = 10, Name = "Grumman AA-5" },
                new AircraftType { Id = 11, Name = "Mooney M20" },
                new AircraftType { Id = 12, Name = "Piper Archer" },
                new AircraftType { Id = 13, Name = "Piper Cherokee" },
                new AircraftType { Id = 14, Name = "Piper PA-28" },
                new AircraftType { Id = 15, Name = "Piper Saratoga" },
                new AircraftType { Id = 16, Name = "Piper Seminole" },
                new AircraftType { Id = 17, Name = "Robin DR400" });
        });

        modelBuilder.Entity<FlightLog>(entity =>
        {
            entity.Property(e => e.TotalHours).HasPrecision(5, 2);
            entity.Property(e => e.PicHours).HasPrecision(5, 2);
            entity.Property(e => e.SicHours).HasPrecision(5, 2);
            entity.Property(e => e.CrossCountryHours).HasPrecision(5, 2);
            entity.Property(e => e.NightHours).HasPrecision(5, 2);
            entity.Property(e => e.InstrumentHours).HasPrecision(5, 2);
        });
    }
}
