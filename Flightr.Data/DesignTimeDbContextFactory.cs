using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Flightr.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FlightrDbContext>
{
    public FlightrDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FlightrDbContext>();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var database = environment == "Development" ? "flightr_dev" : "flightr";
        var connectionString = $"server=localhost;port=3306;database={database};user=flightr;password=flightr_dev";
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 34));

        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new FlightrDbContext(optionsBuilder.Options);
    }
}
