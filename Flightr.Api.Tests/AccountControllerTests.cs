using System.Security.Claims;
using System.Text;
using Flightr.Api.Contracts;
using Flightr.Api.Controllers;
using Flightr.Api.Services;
using Flightr.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Flightr.Api.Tests;

public class AccountControllerTests
{
    private static AccountController CreateController(Mock<UserManager<ApplicationUser>> userManagerMock, IConfiguration configuration, IEmailSender? emailSender = null)
    {
        return new AccountController(userManagerMock.Object, configuration, emailSender);
    }

    private static IConfiguration CreateConfiguration(string? jwtKey = "test-signing-key-12345")
    {
        var values = new Dictionary<string, string?>();
        if (jwtKey is not null)
        {
            values["Jwt:Key"] = jwtKey;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    [Fact]
    public async Task Register_Returns_ValidationProblem_When_ModelState_Invalid()
    {
        var userManager = TestHelpers.CreateUserManagerMock();
        var controller = CreateController(userManager, CreateConfiguration());
        controller.ModelState.AddModelError("Email", "Required");

        var result = await controller.Register(new RegisterAccountRequest("", "", "", "", null, ""));

        result.Should().BeOfType<ObjectResult>();
    }

    [Fact]
    public async Task Register_Returns_Ok_When_User_Created()
    {
        var userManager = TestHelpers.CreateUserManagerMock();
        userManager.Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(userManager, CreateConfiguration());

        var result = await controller.Register(new RegisterAccountRequest("pilot@example.com", "Password123!", "Pilot", "12345", null, "Private"));

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Login_Returns_Unauthorized_When_User_Not_Found()
    {
        var userManager = TestHelpers.CreateUserManagerMock();
        userManager.Setup(manager => manager.FindByEmailAsync("pilot@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        var controller = CreateController(userManager, CreateConfiguration());

        var result = await controller.Login(new LoginRequest("pilot@example.com", "Password123!"));

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_Returns_Unauthorized_When_Password_Is_Invalid()
    {
        var userManager = TestHelpers.CreateUserManagerMock();
        var user = new ApplicationUser { Id = "user-1", Email = "pilot@example.com", PilotName = "Pilot" };
        userManager.Setup(manager => manager.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        userManager.Setup(manager => manager.CheckPasswordAsync(user, "BadPassword!"))
            .ReturnsAsync(false);

        var controller = CreateController(userManager, CreateConfiguration());

        var result = await controller.Login(new LoginRequest(user.Email!, "BadPassword!"));

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_Returns_ServerError_When_Jwt_Key_Missing()
    {
        var userManager = TestHelpers.CreateUserManagerMock();
        var user = new ApplicationUser { Id = "user-1", Email = "pilot@example.com", PilotName = "Pilot" };
        userManager.Setup(manager => manager.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        userManager.Setup(manager => manager.CheckPasswordAsync(user, "Password123!")).ReturnsAsync(true);

        var controller = CreateController(userManager, CreateConfiguration(null));

        var result = await controller.Login(new LoginRequest(user.Email!, "Password123!"));

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Login_Returns_Token_When_Credentials_Are_Valid()
    {
        var userManager = TestHelpers.CreateUserManagerMock();
        var user = new ApplicationUser { Id = "user-1", Email = "pilot@example.com", PilotName = "Pilot" };
        userManager.Setup(manager => manager.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        userManager.Setup(manager => manager.CheckPasswordAsync(user, "Password123!")).ReturnsAsync(true);

        var controller = CreateController(userManager, CreateConfiguration("test-signing-key-1234567890-test-signing-key"));

        var result = await controller.Login(new LoginRequest(user.Email!, "Password123!"));

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task ForgotPassword_Returns_BadRequest_When_User_Not_Found()
    {
        var userManager = TestHelpers.CreateUserManagerMock();
        userManager.Setup(manager => manager.FindByEmailAsync("pilot@example.com")).ReturnsAsync((ApplicationUser?)null);

        var controller = CreateController(userManager, CreateConfiguration());

        var result = await controller.ForgotPassword(new ForgotPasswordRequest("pilot@example.com"));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ForgotPassword_Returns_Ok_When_User_Exists()
    {
        var userManager = TestHelpers.CreateUserManagerMock();
        var user = new ApplicationUser { Id = "user-1", Email = "pilot@example.com", PilotName = "Pilot" };
        userManager.Setup(manager => manager.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        userManager.Setup(manager => manager.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");

        var emailSender = new Mock<IEmailSender>();
        emailSender.Setup(sender => sender.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var controller = CreateController(userManager, CreateConfiguration(), emailSender.Object);
        controller.ControllerContext = TestHelpers.CreateControllerContextWithUser(null);

        var result = await controller.ForgotPassword(new ForgotPasswordRequest(user.Email!));

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ResetPassword_Returns_BadRequest_When_User_Not_Found()
    {
        var userManager = TestHelpers.CreateUserManagerMock();
        userManager.Setup(manager => manager.FindByEmailAsync("pilot@example.com")).ReturnsAsync((ApplicationUser?)null);

        var controller = CreateController(userManager, CreateConfiguration());

        var result = await controller.ResetPassword(new ResetPasswordRequest("pilot@example.com", "token", "NewPassword123!"));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ResetPassword_Returns_Ok_When_Reset_Succeeds()
    {
        var userManager = TestHelpers.CreateUserManagerMock();
        var user = new ApplicationUser { Id = "user-1", Email = "pilot@example.com", PilotName = "Pilot" };
        userManager.Setup(manager => manager.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        userManager.Setup(manager => manager.ResetPasswordAsync(user, "token", "NewPassword123!"))
            .ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(userManager, CreateConfiguration());

        var result = await controller.ResetPassword(new ResetPasswordRequest(user.Email!, "token", "NewPassword123!"));

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetProfile_Returns_Unauthorized_When_User_Id_Missing()
    {
        var userManager = TestHelpers.CreateUserManagerMock();
        var controller = CreateController(userManager, CreateConfiguration());
        controller.ControllerContext = TestHelpers.CreateControllerContextWithUser(null);

        var result = await controller.GetProfile();

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetProfile_Returns_Ok_When_User_Exists()
    {
        var userManager = TestHelpers.CreateUserManagerMock();
        var user = new ApplicationUser
        {
            Id = "user-1",
            PilotName = "Pilot",
            LicenseNumber = "LN123",
            LicenseGoal = "Private"
        };

        userManager.Setup(manager => manager.FindByIdAsync("user-1")).ReturnsAsync(user);

        var controller = CreateController(userManager, CreateConfiguration());
        controller.ControllerContext = TestHelpers.CreateControllerContextWithUser("user-1");

        var result = await controller.GetProfile();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateProfile_Returns_ValidationProblem_When_ModelState_Invalid()
    {
        var userManager = TestHelpers.CreateUserManagerMock();
        var controller = CreateController(userManager, CreateConfiguration());
        controller.ModelState.AddModelError("PilotName", "Required");

        var result = await controller.UpdateProfile(new UpdateProfileRequest("", "", null, ""));

        result.Should().BeOfType<ObjectResult>();
    }

    [Fact]
    public async Task UpdateProfile_Returns_Ok_When_Update_Succeeds()
    {
        var userManager = TestHelpers.CreateUserManagerMock();
        var user = new ApplicationUser
        {
            Id = "user-1",
            PilotName = "Pilot",
            LicenseNumber = "LN123",
            LicenseGoal = "Private"
        };

        userManager.Setup(manager => manager.FindByIdAsync("user-1")).ReturnsAsync(user);
        userManager.Setup(manager => manager.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(userManager, CreateConfiguration());
        controller.ControllerContext = TestHelpers.CreateControllerContextWithUser("user-1");

        var result = await controller.UpdateProfile(new UpdateProfileRequest("New Pilot", "LN999", null, "Commercial"));

        result.Should().BeOfType<OkResult>();
    }
}
