using System.Net;
using System.Security.Claims;
using System.Text;
using Flightr.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Flightr.Web.Tests;

internal sealed class ScriptedHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();

    public List<(HttpMethod Method, Uri? Uri)> Requests { get; } = new();

    public ScriptedHttpMessageHandler Enqueue(HttpStatusCode statusCode, string? content = null)
    {
        _responses.Enqueue(_ => CreateResponse(statusCode, content));
        return this;
    }

    public ScriptedHttpMessageHandler Enqueue(Func<HttpRequestMessage, HttpResponseMessage> factory)
    {
        _responses.Enqueue(factory);
        return this;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add((request.Method, request.RequestUri));

        if (_responses.Count == 0)
        {
            throw new InvalidOperationException("No scripted response was queued for this request.");
        }

        return Task.FromResult(_responses.Dequeue()(request));
    }

    private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string? content)
    {
        var response = new HttpResponseMessage(statusCode);
        if (content is not null)
        {
            response.Content = new StringContent(content, Encoding.UTF8, "application/json");
        }

        return response;
    }
}

internal sealed class StubHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _client;

    public StubHttpClientFactory(HttpMessageHandler handler)
    {
        _client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.test/")
        };
    }

    public HttpClient CreateClient(string name) => _client;
}

internal sealed class TestAuthenticationService : IAuthenticationService
{
    public ClaimsPrincipal? SignedInPrincipal { get; private set; }

    public AuthenticationProperties? SignInProperties { get; private set; }

    public string? SignInScheme { get; private set; }

    public string? SignOutScheme { get; private set; }

    public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
        => Task.FromResult(AuthenticateResult.NoResult());

    public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
        => Task.CompletedTask;

    public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
        => Task.CompletedTask;

    public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
    {
        SignedInPrincipal = principal;
        SignInScheme = scheme;
        SignInProperties = properties;
        return Task.CompletedTask;
    }

    public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
    {
        SignOutScheme = scheme;
        return Task.CompletedTask;
    }
}

internal static class PageTestSupport
{
    public static PageContext CreatePageContext(ClaimsPrincipal? user = null, IAuthenticationService? authService = null)
    {
        var services = new ServiceCollection();
        if (authService is not null)
        {
            services.AddSingleton(authService);
        }

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        var httpContext = new DefaultHttpContext
        {
            User = user ?? new ClaimsPrincipal(new ClaimsIdentity()),
            RequestServices = services.BuildServiceProvider()
        };

        return new PageContext
        {
            HttpContext = httpContext
        };
    }

    public static TempDataDictionary CreateTempData(HttpContext httpContext)
        => new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

    public static string CreateBase64UrlToken(string jsonPayload)
    {
        var bytes = Encoding.UTF8.GetBytes(jsonPayload);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json)
        => new(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
}
