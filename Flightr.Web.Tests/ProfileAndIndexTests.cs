using System.Net;
using System.Security.Claims;
using Flightr.Web.Pages;
using Flightr.Web.Pages.Account;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Flightr.Web.Tests;

public class ProfileAndIndexTests
{
    [Fact]
    public void Index_Get_Redirects_When_User_Is_Not_Authenticated()
    {
        var model = new IndexModel
        {
            PageContext = PageTestSupport.CreatePageContext()
        };

        var result = model.OnGet();

        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("/Account/Login");
    }

    [Fact]
    public void Index_Get_Returns_Page_When_User_Is_Authenticated()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Pilot") }, "Cookies"));
        var model = new IndexModel
        {
            PageContext = PageTestSupport.CreatePageContext(user)
        };

        var result = model.OnGet();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task Profile_Get_Populates_Input_When_Api_Returns_Profile()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, """
            {
              "pilotName": "Pilot",
              "licenseNumber": "LN123",
              "licenseExpirationDate": "2028-01-01",
              "licenseGoal": "Private"
            }
            """);
        var model = CreateProfileModel(handler);

        var result = await model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        model.Input.PilotName.Should().Be("Pilot");
        model.Input.LicenseNumber.Should().Be("LN123");
        model.Input.LicenseExpirationDate.Should().Be(new DateOnly(2028, 1, 1));
        model.Input.LicenseGoal.Should().Be("Private");
    }

    [Fact]
    public async Task Profile_Get_Adds_Error_When_Api_Returns_NonSuccess()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.InternalServerError, "server down");
        var model = CreateProfileModel(handler);

        var result = await model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        model.ErrorMessages.Should().ContainSingle(message => message.Contains("Failed to load profile"));
    }

    [Fact]
    public async Task Profile_Get_Adds_Error_When_Json_Is_Invalid()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, "not-json");
        var model = CreateProfileModel(handler);

        var result = await model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        model.ErrorMessages.Should().ContainSingle(message => message.Contains("An error occurred"));
    }

    [Fact]
    public async Task Profile_Post_Returns_Page_When_ModelState_Invalid()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, "{}");
        var model = CreateProfileModel(handler);
        model.ModelState.AddModelError("PilotName", "Required");

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task Profile_Post_Sets_Status_When_Save_Succeeds()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, "{}");
        var model = CreateProfileModel(handler);
        model.Input = new ProfileModel.InputModel
        {
            PilotName = "Pilot",
            LicenseNumber = "LN123",
            LicenseGoal = "Private",
            LicenseExpirationDate = new DateOnly(2028, 1, 1)
        };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.StatusMessage.Should().Be("Profile updated successfully.");
    }

    [Fact]
    public async Task Profile_Post_Collects_Validation_Errors_From_Api()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.BadRequest, """
            {
              "errors": {
                "PilotName": ["Required"],
                "LicenseNumber": ["Too short"]
              }
            }
            """);
        var model = CreateProfileModel(handler);
        model.Input = new ProfileModel.InputModel
        {
            PilotName = "Pilot",
            LicenseNumber = "LN123",
            LicenseGoal = "Private",
            LicenseExpirationDate = new DateOnly(2028, 1, 1)
        };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.ErrorMessages.Should().Contain(new[] { "Required", "Too short" });
    }

    [Fact]
    public async Task Profile_Post_Adds_Generic_Error_When_Response_Is_Not_Json()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.BadRequest, "plain text error");
        var model = CreateProfileModel(handler);
        model.Input = new ProfileModel.InputModel
        {
            PilotName = "Pilot",
            LicenseNumber = "LN123",
            LicenseGoal = "Private",
            LicenseExpirationDate = new DateOnly(2028, 1, 1)
        };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.ErrorMessages.Should().ContainSingle(message => message.Contains("An error occurred while updating the profile."));
    }

    private static ProfileModel CreateProfileModel(ScriptedHttpMessageHandler handler)
    {
        var model = new ProfileModel(new StubHttpClientFactory(handler))
        {
            PageContext = PageTestSupport.CreatePageContext()
        };

        return model;
    }
}
