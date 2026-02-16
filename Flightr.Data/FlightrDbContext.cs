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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
