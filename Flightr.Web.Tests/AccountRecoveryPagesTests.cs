using System.Net;
using Flightr.Web.Pages.Account;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Flightr.Web.Tests;

public class AccountRecoveryPagesTests
{
    [Fact]
    public void ForgotPassword_Get_Returns_Page()
    {
        var model = CreateForgotPasswordModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.OK));

        model.OnGet();

        model.ErrorMessage.Should().BeNull();
        model.StatusMessage.Should().BeNull();
    }

    [Fact]
    public async Task ForgotPassword_Post_Returns_Page_When_ModelState_Invalid()
    {
        var model = CreateForgotPasswordModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.OK));
        model.ModelState.AddModelError("Email", "Required");

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task ForgotPassword_Post_Sets_Status_Message_On_Success()
    {
        var model = CreateForgotPasswordModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.OK));
        model.Input = new ForgotPasswordModel.InputModel { Email = "pilot@example.com" };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.StatusMessage.Should().Be("A password reset email has been sent to pilot@example.com.");
        model.ErrorMessage.Should().BeNull();
        model.ResetLink.Should().BeNull();
    }

    [Fact]
    public async Task ForgotPassword_Post_Uses_Api_Message_For_AccountNotFound()
    {
        var model = CreateForgotPasswordModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.BadRequest, "{\"errorCode\":\"AccountNotFound\",\"message\":\"No account exists\"}"));
        model.Input = new ForgotPasswordModel.InputModel { Email = "pilot@example.com" };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.ErrorMessage.Should().Be("No account exists");
        model.StatusMessage.Should().BeNull();
    }

    [Fact]
    public async Task ForgotPassword_Post_Uses_General_Message_For_Unknown_ErrorCode()
    {
        var model = CreateForgotPasswordModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.BadRequest, "{\"errorCode\":\"Other\",\"message\":\"Try later\"}"));
        model.Input = new ForgotPasswordModel.InputModel { Email = "pilot@example.com" };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.ErrorMessage.Should().Be("Try later");
    }

    [Fact]
    public async Task ForgotPassword_Post_Adds_Field_Errors_And_No_Generic_Message()
    {
        var model = CreateForgotPasswordModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.BadRequest, "{\"errors\":{\"Email\":[\"Bad email\"]}}"));
        model.Input = new ForgotPasswordModel.InputModel { Email = "pilot@example.com" };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.ModelState.ErrorCount.Should().BeGreaterThan(0);
        model.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task ForgotPassword_Post_Falls_Back_To_Generic_Message_For_NonJson_Response()
    {
        var model = CreateForgotPasswordModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.BadRequest, "plain text"));
        model.Input = new ForgotPasswordModel.InputModel { Email = "pilot@example.com" };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.ErrorMessage.Should().Be("Unable to generate a password reset link.");
    }

    [Fact]
    public void ResetPassword_Get_Shows_Error_When_Email_Or_Token_Missing()
    {
        var model = CreateResetPasswordModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.OK));

        var result = model.OnGet();

        result.Should().BeOfType<PageResult>();
        model.ErrorMessage.Should().Be("The reset link is missing required information.");
    }

    [Fact]
    public void ResetPassword_Get_Leaves_Error_Clear_When_Link_Is_Complete()
    {
        var model = CreateResetPasswordModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.OK));
        model.Input.Email = "pilot@example.com";
        model.Input.Token = "token-123";

        var result = model.OnGet();

        result.Should().BeOfType<PageResult>();
        model.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task ResetPassword_Post_Returns_Page_When_ModelState_Invalid()
    {
        var model = CreateResetPasswordModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.OK));
        model.ModelState.AddModelError("Token", "Required");

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task ResetPassword_Post_Sets_Status_On_Success()
    {
        var model = CreateResetPasswordModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.OK));
        model.Input = new ResetPasswordModel.InputModel
        {
            Email = "pilot@example.com",
            Token = "token-123",
            NewPassword = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.StatusMessage.Should().Be("Your password has been reset. You can sign in now.");
        model.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task ResetPassword_Post_Uses_Api_Message_For_AccountNotFound()
    {
        var model = CreateResetPasswordModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.BadRequest, "{\"errorCode\":\"AccountNotFound\",\"message\":\"No account was found\"}"));
        model.Input = new ResetPasswordModel.InputModel
        {
            Email = "pilot@example.com",
            Token = "token-123",
            NewPassword = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.ErrorMessage.Should().Be("No account was found");
    }

    [Fact]
    public async Task ResetPassword_Post_Uses_Message_Property_When_Present()
    {
        var model = CreateResetPasswordModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.BadRequest, "{\"message\":\"Reset failed\"}"));
        model.Input = new ResetPasswordModel.InputModel
        {
            Email = "pilot@example.com",
            Token = "token-123",
            NewPassword = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.ErrorMessage.Should().Be("Reset failed");
    }

    [Fact]
    public async Task ResetPassword_Post_Uses_Uppercase_Message_Property_When_Present()
    {
        var model = CreateResetPasswordModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.BadRequest, "{\"Message\":\"Reset failed uppercase\"}"));
        model.Input = new ResetPasswordModel.InputModel
        {
            Email = "pilot@example.com",
            Token = "token-123",
            NewPassword = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.ErrorMessage.Should().Be("Reset failed uppercase");
    }

    [Fact]
    public async Task ResetPassword_Post_Adds_Field_Errors_When_Returned()
    {
        var model = CreateResetPasswordModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.BadRequest, "{\"errors\":{\"NewPassword\":[\"Too weak\"]}}"));
        model.Input = new ResetPasswordModel.InputModel
        {
            Email = "pilot@example.com",
            Token = "token-123",
            NewPassword = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.ModelState.ErrorCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ResetPassword_Post_Falls_Back_To_Raw_Body_When_Response_Is_Not_Json()
    {
        var model = CreateResetPasswordModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.BadRequest, "plain text failure"));
        model.Input = new ResetPasswordModel.InputModel
        {
            Email = "pilot@example.com",
            Token = "token-123",
            NewPassword = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.ErrorMessage.Should().Be("plain text failure");
    }

    [Fact]
    public async Task ResetPassword_Post_Falls_Back_To_Generic_Message_When_Body_Is_Empty()
    {
        var model = CreateResetPasswordModel(new ScriptedHttpMessageHandler().Enqueue(HttpStatusCode.BadRequest, string.Empty));
        model.Input = new ResetPasswordModel.InputModel
        {
            Email = "pilot@example.com",
            Token = "token-123",
            NewPassword = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.ErrorMessage.Should().Be("Unable to reset your password.");
    }

    private static ForgotPasswordModel CreateForgotPasswordModel(ScriptedHttpMessageHandler handler)
    {
        return new ForgotPasswordModel(new StubHttpClientFactory(handler));
    }

    private static ResetPasswordModel CreateResetPasswordModel(ScriptedHttpMessageHandler handler)
    {
        return new ResetPasswordModel(new StubHttpClientFactory(handler));
    }
}
