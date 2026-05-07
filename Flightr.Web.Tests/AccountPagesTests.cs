using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Flightr.Web.Pages.Account;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using Xunit;

namespace Flightr.Web.Tests;

public class AccountPagesTests
{
    [Fact]
    public async Task Login_Post_Returns_Page_When_ModelState_Invalid()
    {
        var model = CreateLoginModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.OK, "{}"));
        model.ModelState.AddModelError("Email", "Required");

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task Login_Post_Signs_In_With_User_Id_When_Token_Is_Valid()
    {
        var payload = PageTestSupport.CreateBase64UrlToken("{\"sub\":\"user-123\"}");
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, $"{{\"accessToken\":\"header.{payload}.sig\",\"pilotName\":\"Pilot\",\"email\":\"pilot@example.com\"}}");
        var auth = new TestAuthenticationService();
        var model = CreateLoginModel(handler, auth);
        model.Input = new LoginModel.InputModel { Email = "pilot@example.com", Password = "Password123!" };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("/FlightLogs/Index");
        auth.SignInScheme.Should().Be(CookieAuthenticationDefaults.AuthenticationScheme);
        var signedInPrincipal = auth.SignedInPrincipal;
        signedInPrincipal.Should().NotBeNull();
        var principal = signedInPrincipal!;
        principal.FindFirstValue(ClaimTypes.NameIdentifier).Should().Be("user-123");
        principal.FindFirstValue("access_token").Should().Contain("header.");
    }

    [Fact]
    public async Task Login_Post_Signs_In_Without_NameIdentifier_When_Token_Cannot_Be_Decoded()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.OK, "{\"accessToken\":\"not-a-jwt\",\"pilotName\":\"Pilot\",\"email\":\"pilot@example.com\"}");
        var auth = new TestAuthenticationService();
        var model = CreateLoginModel(handler, auth);
        model.Input = new LoginModel.InputModel { Email = "pilot@example.com", Password = "Password123!" };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var signedInPrincipal = auth.SignedInPrincipal;
        signedInPrincipal.Should().NotBeNull();
        var principal = signedInPrincipal!;
        principal.FindFirstValue("access_token").Should().Be("not-a-jwt");
        principal.FindFirstValue(ClaimTypes.NameIdentifier).Should().BeNull();
    }

    [Fact]
    public async Task Login_Post_Returns_AccountNotFound_Message_When_Api_Says_AccountNotFound()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.Unauthorized, "{\"errorCode\":\"AccountNotFound\",\"message\":\"No account found\"}");
        var model = CreateLoginModel(handler);
        model.Input = new LoginModel.InputModel { Email = "pilot@example.com", Password = "Password123!" };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.AccountNotFoundMessage.Should().Contain("No account found for pilot@example.com");
        model.LoginErrorMessage.Should().Contain("Account not found");
    }

    [Fact]
    public async Task Login_Post_Returns_Generic_Error_For_Unexpected_Response()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.InternalServerError, "{\"message\":\"Boom\"}");
        var model = CreateLoginModel(handler);
        model.Input = new LoginModel.InputModel { Email = "pilot@example.com", Password = "Password123!" };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.LoginErrorMessage.Should().Contain("We couldn't sign you in right now");
    }

    [Fact]
    public async Task Login_Post_Returns_Page_When_Api_Is_Unauthorized_Without_Error_Code()
    {
        var handler = new ScriptedHttpMessageHandler()
            .Enqueue(HttpStatusCode.Unauthorized, "");
        var model = CreateLoginModel(handler);
        model.Input = new LoginModel.InputModel { Email = "pilot@example.com", Password = "WrongPassword!" };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.LoginErrorMessage.Should().Be("Incorrect email or password. Please try again.");
    }

    [Fact]
    public async Task Register_Post_Returns_Page_When_ModelState_Invalid()
    {
        var model = CreateRegisterModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.OK, "{}"));
        model.ModelState.AddModelError("Email", "Required");

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task Register_Post_Redirects_To_Login_When_Api_Succeeds()
    {
        var model = CreateRegisterModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.OK));
        model.Input = new RegisterModel.InputModel
        {
            Email = "pilot@example.com",
            PilotName = "Pilot",
            LicenseNumber = "LN123",
            LicenseGoal = "Private",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("/Account/Login");
        model.TempData["RegistrationStatusMessage"].Should().Be("Account created. You can sign in now.");
    }

    [Fact]
    public async Task Register_Post_Collects_Field_Errors_From_ValidationProblem_Response()
    {
        var json = "{\"errors\":{\"Email\":[\"Email already exists\"],\"Password\":[\"Too weak\"]}}";
        var model = CreateRegisterModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.BadRequest, json));
        model.Input = new RegisterModel.InputModel
        {
            Email = "pilot@example.com",
            PilotName = "Pilot",
            LicenseNumber = "LN123",
            LicenseGoal = "Private",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.RegistrationErrors.Should().Contain(new[] { "Email already exists", "Too weak" });
    }

    [Fact]
    public async Task Register_Post_Uses_Detail_Message_When_Response_Has_Detail()
    {
        var json = "{\"detail\":\"Registration failed because of server rules.\"}";
        var model = CreateRegisterModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.BadRequest, json));
        model.Input = new RegisterModel.InputModel
        {
            Email = "pilot@example.com",
            PilotName = "Pilot",
            LicenseNumber = "LN123",
            LicenseGoal = "Private",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.RegistrationErrorMessage.Should().Contain("server rules");
    }

    [Fact]
    public async Task Register_Post_Falls_Back_To_Raw_Error_Text_For_NonJson_Response()
    {
        var model = CreateRegisterModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.BadRequest, "plain error text"));
        model.Input = new RegisterModel.InputModel
        {
            Email = "pilot@example.com",
            PilotName = "Pilot",
            LicenseNumber = "LN123",
            LicenseGoal = "Private",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.RegistrationErrorMessage.Should().Be("plain error text");
    }

    [Fact]
    public async Task Logout_Post_Requests_SignOut_And_Redirects()
    {
        var auth = new TestAuthenticationService();
        var model = new LogoutModel
        {
            PageContext = PageTestSupport.CreatePageContext(authService: auth)
        };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("/Index");
        auth.SignOutScheme.Should().Be(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    private static LoginModel CreateLoginModel(ScriptedHttpMessageHandler handler, TestAuthenticationService? authService = null)
    {
        var model = new LoginModel(new StubHttpClientFactory(handler))
        {
            PageContext = PageTestSupport.CreatePageContext(authService: authService),
        };

        return model;
    }

    private static RegisterModel CreateRegisterModel(ScriptedHttpMessageHandler handler)
    {
        var model = new RegisterModel(new StubHttpClientFactory(handler))
        {
            PageContext = PageTestSupport.CreatePageContext()
        };

        model.TempData = PageTestSupport.CreateTempData(model.PageContext.HttpContext);
        return model;
    }
}
