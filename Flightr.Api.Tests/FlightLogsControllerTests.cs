using System.Security.Claims;
using Flightr.Api.Controllers;
using Flightr.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace Flightr.Api.Tests;

public class FlightLogsControllerTests
{
    private static FlightrDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<FlightrDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new FlightrDbContext(options);
    }

    private static ControllerContext CreateControllerContextWithUser(string? userId)
    {
        var user = new ClaimsPrincipal();
        if (!string.IsNullOrEmpty(userId))
        {
            user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }));
        }

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetAll_Returns_Unauthorized_When_User_Missing()
    {
        using var ctx = CreateInMemoryContext("GetAll_NoUser");
        var controller = new FlightLogsController(ctx)
        {
            ControllerContext = CreateControllerContextWithUser(null)
        };

        var result = await controller.GetAll(default);

        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetAll_Returns_Only_User_Logs()
    {
        using var ctx = CreateInMemoryContext("GetAll_ReturnsOnlyUserLogs");
        // arrange data
        ctx.FlightLogs.Add(new FlightLog { Id = 1, PilotId = "user-a", FlightDate = DateTime.UtcNow, AircraftType = "Cessna 172", TotalHours = 1, PicHours = 1, SicHours = 0, CrossCountryHours = 0, NightHours = 0, InstrumentHours = 0, TakeoffsDay = 1, TakeoffsNight = 0, LandingsDay = 1, LandingsNight = 0, CreatedAtUtc = DateTime.UtcNow });
        ctx.FlightLogs.Add(new FlightLog { Id = 2, PilotId = "user-b", FlightDate = DateTime.UtcNow, AircraftType = "Cessna 172", TotalHours = 2, PicHours = 2, SicHours = 0, CrossCountryHours = 0, NightHours = 0, InstrumentHours = 0, TakeoffsDay = 2, TakeoffsNight = 0, LandingsDay = 2, LandingsNight = 0, CreatedAtUtc = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        var controller = new FlightLogsController(ctx)
        {
            ControllerContext = CreateControllerContextWithUser("user-a")
        };

        var result = await controller.GetAll(default);

        result.Result.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value![0].PilotId.Should().Be("user-a");
    }

    [Fact]
    public async Task GetById_Unauthorized_When_User_Missing()
    {
        using var ctx = CreateInMemoryContext("GetById_NoUser");
        var controller = new FlightLogsController(ctx)
        {
            ControllerContext = CreateControllerContextWithUser(null)
        };

        var result = await controller.GetById(1, default);

        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetById_Returns_NotFound_When_Log_Not_Exist()
    {
        using var ctx = CreateInMemoryContext("GetById_NotFound");
        var controller = new FlightLogsController(ctx)
        {
            ControllerContext = CreateControllerContextWithUser("user-a")
        };

        var result = await controller.GetById(999, default);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_Returns_NotFound_When_Log_Belongs_To_Other_User()
    {
        using var ctx = CreateInMemoryContext("GetById_OtherUser");
        ctx.FlightLogs.Add(new FlightLog { Id = 5, PilotId = "user-b", FlightDate = DateTime.UtcNow, AircraftType = "Cessna 172", TotalHours = 3, PicHours = 3, SicHours = 0, CrossCountryHours = 0, NightHours = 0, InstrumentHours = 0, TakeoffsDay = 3, TakeoffsNight = 0, LandingsDay = 3, LandingsNight = 0, CreatedAtUtc = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        var controller = new FlightLogsController(ctx)
        {
            ControllerContext = CreateControllerContextWithUser("user-a")
        };

        var result = await controller.GetById(5, default);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_Returns_Log_When_User_Owns_It()
    {
        using var ctx = CreateInMemoryContext("GetById_Owned");
        ctx.FlightLogs.Add(new FlightLog { Id = 6, PilotId = "user-a", FlightDate = DateTime.UtcNow, AircraftType = "Cessna 172", TotalHours = 4, PicHours = 4, SicHours = 0, CrossCountryHours = 0, NightHours = 0, InstrumentHours = 0, TakeoffsDay = 4, TakeoffsNight = 0, LandingsDay = 4, LandingsNight = 0, CreatedAtUtc = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        var controller = new FlightLogsController(ctx)
        {
            ControllerContext = CreateControllerContextWithUser("user-a")
        };

        var result = await controller.GetById(6, default);

        result.Result.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(6);
    }
}
