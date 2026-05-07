using System.Security.Claims;
using Flightr.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Flightr.Api.Tests;

internal static class TestHelpers
{
    internal static FlightrDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<FlightrDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new FlightrDbContext(options);
    }

    internal static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var options = Options.Create(new IdentityOptions());
        var passwordHasher = new PasswordHasher<ApplicationUser>();
        var userValidators = Array.Empty<IUserValidator<ApplicationUser>>();
        var passwordValidators = Array.Empty<IPasswordValidator<ApplicationUser>>();
        var keyNormalizer = new UpperInvariantLookupNormalizer();
        var errors = new IdentityErrorDescriber();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            options,
            passwordHasher,
            userValidators,
            passwordValidators,
            keyNormalizer,
            errors,
            services.Object,
            logger.Object);
    }

    internal static ControllerContext CreateControllerContextWithUser(string? userId)
    {
        var identity = string.IsNullOrEmpty(userId)
            ? new ClaimsIdentity()
            : new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) });

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity),
                RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider()
            }
        };
    }
}
