using System.Net;
using System.Security.Claims;
using Flightr.Web.Pages.FlightLogs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Flightr.Web.Tests;

public class FlightLogsPagesTests
{
    [Fact]
    public async Task Create_Get_Loads_Aircraft_Types()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, "[\"Cessna 172\",\"Piper PA-28\"]");
        var model = CreateCreateModel(handler, userId: "pilot-1");

        await model.OnGetAsync();

        model.AircraftTypes.Should().Equal("Cessna 172", "Piper PA-28");
        handler.Requests.Should().ContainSingle(request => request.Method == HttpMethod.Get);
    }

    [Fact]
    public async Task Create_Post_Returns_Page_When_ModelState_Invalid()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, "[\"Cessna 172\"]");
        var model = CreateCreateModel(handler, userId: "pilot-1");
        model.ModelState.AddModelError("AircraftType", "Required");

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        handler.Requests.Should().ContainSingle(request => request.Method == HttpMethod.Get);
    }

    [Fact]
    public async Task Create_Post_Returns_Page_When_User_Id_Is_Missing()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, "[\"Cessna 172\"]");
        var model = CreateCreateModel(handler, userId: null);
        model.Input = ValidCreateInput();

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        handler.Requests.Should().ContainSingle(request => request.Method == HttpMethod.Get);
    }

    [Fact]
    public async Task Create_Post_Redirects_When_Save_Succeeds()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, "[\"Cessna 172\"]")
            .Enqueue(HttpStatusCode.Created, "{}");
        var model = CreateCreateModel(handler, userId: "pilot-1");
        model.Input = ValidCreateInput();

        var result = await model.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("/FlightLogs/Index");
        handler.Requests.Should().HaveCount(2);
        handler.Requests[1].Method.Should().Be(HttpMethod.Post);
    }

    [Fact]
    public async Task Create_Post_Shows_Error_When_Api_Fails()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, "[\"Cessna 172\"]")
            .Enqueue(HttpStatusCode.BadRequest, "bad request");
        var model = CreateCreateModel(handler, userId: "pilot-1");
        model.Input = ValidCreateInput();

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        handler.Requests.Should().HaveCount(2);
    }

    [Fact]
    public async Task Edit_Get_Returns_NotFound_When_Log_Does_Not_Exist()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, "[\"Cessna 172\"]")
            .Enqueue(HttpStatusCode.OK, "null");
        var model = CreateEditModel(handler, userId: "pilot-1", id: 99);

        var result = await model.OnGetAsync();

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Edit_Get_Populates_Input_And_Adds_Unknown_Aircraft()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, "[\"Cessna 172\",\"Piper PA-28\"]")
            .Enqueue(HttpStatusCode.OK, """
            {
              "flightDate": "2026-05-07T00:00:00Z",
              "aircraftType": "Beechcraft Bonanza",
              "tailNumber": "N12345",
              "departureAirport": "KSFO",
              "arrivalAirport": "KLAX",
              "route": "KSFO-KLAX",
              "totalHours": 2.5,
              "picHours": 2.5,
              "sicHours": 0,
              "crossCountryHours": 2.5,
              "nightHours": 0,
              "instrumentHours": 0,
              "takeoffsDay": 1,
              "takeoffsNight": 0,
              "landingsDay": 1,
              "landingsNight": 0,
              "remarks": "Nice flight"
            }
            """);
        var model = CreateEditModel(handler, userId: "pilot-1", id: 7);

        var result = await model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        model.Input.AircraftType.Should().Be("Beechcraft Bonanza");
        model.AircraftTypes.Should().Equal("Beechcraft Bonanza", "Cessna 172", "Piper PA-28");
    }

    [Fact]
    public async Task Edit_Post_Returns_Page_When_ModelState_Invalid()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, "[\"Cessna 172\"]");
        var model = CreateEditModel(handler, userId: "pilot-1", id: 7);
        model.ModelState.AddModelError("Route", "Required");

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        handler.Requests.Should().ContainSingle(request => request.Method == HttpMethod.Get);
    }

    [Fact]
    public async Task Edit_Post_Redirects_When_Save_Succeeds()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, "[\"Cessna 172\",\"Piper PA-28\"]")
            .Enqueue(HttpStatusCode.OK, "{}");
        var model = CreateEditModel(handler, userId: "pilot-1", id: 7);
        model.Input = ValidEditInput();

        var result = await model.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("/FlightLogs/Index");
    }

    [Fact]
    public async Task Edit_Post_Shows_Error_When_Api_Fails()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, "[\"Cessna 172\"]")
            .Enqueue(HttpStatusCode.BadRequest, "update failed");
        var model = CreateEditModel(handler, userId: "pilot-1", id: 7);
        model.Input = ValidEditInput();

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task Delete_Get_Returns_NotFound_When_Log_Does_Not_Exist()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, "null");
        var model = CreateDeleteModel(handler, id: 11);

        var result = await model.OnGetAsync();

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_Get_Sets_Summary_For_Existing_Log()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, "{\"flightDate\":\"2026-05-07T00:00:00Z\",\"aircraftType\":\"Cessna 172\"}");
        var model = CreateDeleteModel(handler, id: 11);

        var result = await model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        model.Summary.Should().Be("2026-05-07 (Cessna 172)");
    }

    [Fact]
    public async Task Delete_Post_Redirects_When_Delete_Succeeds()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.NoContent);
        var model = CreateDeleteModel(handler, id: 11);

        var result = await model.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("/FlightLogs/Index");
    }

    [Fact]
    public async Task Delete_Post_Shows_Error_When_Delete_Fails()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.BadRequest, "cannot delete");
        var model = CreateDeleteModel(handler, id: 11);

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
    }

    private static CreateModel CreateCreateModel(ScriptedHttpMessageHandler handler, string? userId)
    {
        var model = new CreateModel(new StubHttpClientFactory(handler))
        {
            PageContext = PageTestSupport.CreatePageContext(userId is null ? null : new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) })))
        };

        return model;
    }

    private static EditModel CreateEditModel(ScriptedHttpMessageHandler handler, string? userId, int id)
    {
        var model = new EditModel(new StubHttpClientFactory(handler))
        {
            PageContext = PageTestSupport.CreatePageContext(userId is null ? null : new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }))),
            Id = id
        };

        return model;
    }

    private static DeleteModel CreateDeleteModel(ScriptedHttpMessageHandler handler, int id)
    {
        var model = new DeleteModel(new StubHttpClientFactory(handler))
        {
            PageContext = PageTestSupport.CreatePageContext(),
            Id = id
        };

        return model;
    }

    private static CreateModel.InputModel ValidCreateInput() => new()
    {
        FlightDate = new DateTime(2026, 5, 7),
        AircraftType = "Cessna 172",
        TailNumber = "N12345",
        DepartureAirport = "KSFO",
        ArrivalAirport = "KLAX",
        Route = "KSFO-KLAX",
        PicHours = 2.5m,
        SicHours = 0m,
        CrossCountryHours = 2.5m,
        NightHours = 0m,
        InstrumentHours = 0m,
        TakeoffsDay = 1,
        TakeoffsNight = 0,
        LandingsDay = 1,
        LandingsNight = 0,
        Remarks = "Nice flight"
    };

    private static EditModel.InputModel ValidEditInput() => new()
    {
        FlightDate = new DateTime(2026, 5, 7),
        AircraftType = "Cessna 172",
        TailNumber = "N12345",
        DepartureAirport = "KSFO",
        ArrivalAirport = "KLAX",
        Route = "KSFO-KLAX",
        PicHours = 2.5m,
        SicHours = 0m,
        CrossCountryHours = 2.5m,
        NightHours = 0m,
        InstrumentHours = 0m,
        TakeoffsDay = 1,
        TakeoffsNight = 0,
        LandingsDay = 1,
        LandingsNight = 0,
        Remarks = "Nice flight"
    };
}
