using System.Net.Http.Headers;
using System.Security.Claims;
using Flightr.Web.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Flightr.Web.Tests;

public class ApiAuthHandlerTests
{
    [Fact]
    public async Task SendAsync_Adds_Bearer_Token_When_Claim_Exists()
    {
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim("access_token", "token-123")
                }))
            }
        };

        var capturedAuthHeader = string.Empty;
        var terminalHandler = new DelegatingHandlerStub(request =>
        {
            capturedAuthHeader = request.Headers.Authorization?.ToString() ?? string.Empty;
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        });

        var handler = new ApiAuthHandler(accessor)
        {
            InnerHandler = terminalHandler
        };

        using var client = new HttpClient(handler);
        await client.GetAsync("https://api.test/resource");

        capturedAuthHeader.Should().Be("Bearer token-123");
    }

    [Fact]
    public async Task SendAsync_Leaves_Request_Without_Token_When_Claim_Missing()
    {
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        var terminalHandler = new DelegatingHandlerStub(request =>
            new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        var handler = new ApiAuthHandler(accessor)
        {
            InnerHandler = terminalHandler
        };

        using var client = new HttpClient(handler);
        await client.GetAsync("https://api.test/resource");

        terminalHandler.LastRequest!.Headers.Authorization.Should().BeNull();
    }

    private sealed class DelegatingHandlerStub : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _callback;

        public DelegatingHandlerStub(Func<HttpRequestMessage, HttpResponseMessage> callback)
        {
            _callback = callback;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_callback(request));
        }
    }
}
