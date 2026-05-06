using Demo1.Middleware;
using Demo1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Demo1.UnitTests.Middleware;

public class RateLimitHeadersMiddlewareTests
{
    private static IOptions<RateLimitingOptions> CreateOptions(int permitLimit = 100)
    {
        return Options.Create(new RateLimitingOptions { PermitLimit = permitLimit });
    }

    [Fact]
    public async Task InvokeAsync_AddsRateLimitHeaders_ToResponse()
    {
        // Arrange
        var invoked = false;
        RequestDelegate next = context =>
        {
            invoked = true;
            return Task.CompletedTask;
        };
        var middleware = new RateLimitHeadersMiddleware(next, CreateOptions(100));
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(invoked);
        var headers = context.Response.Headers;
        Assert.Equal("100", headers["X-RateLimit-Limit"].ToString());
        Assert.Equal("100", headers["X-RateLimit-Remaining"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_UsesConfiguredPermitLimit()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new RateLimitHeadersMiddleware(next, CreateOptions(250));
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var headers = context.Response.Headers;
        Assert.Equal("250", headers["X-RateLimit-Limit"].ToString());
        Assert.Equal("250", headers["X-RateLimit-Remaining"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_DoesNotPreventOverwrite_ByDownstreamMiddleware()
    {
        // Arrange — downstream middleware overwrites the headers
        RequestDelegate next = context =>
        {
            context.Response.Headers["X-RateLimit-Limit"] = "50";
            context.Response.Headers["X-RateLimit-Remaining"] = "10";
            return Task.CompletedTask;
        };
        var middleware = new RateLimitHeadersMiddleware(next, CreateOptions(100));
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert — downstream middleware can override the values
        var headers = context.Response.Headers;
        Assert.Equal("50", headers["X-RateLimit-Limit"].ToString());
        Assert.Equal("10", headers["X-RateLimit-Remaining"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        // Arrange
        var callCount = 0;
        RequestDelegate next = _ =>
        {
            callCount++;
            return Task.CompletedTask;
        };
        var middleware = new RateLimitHeadersMiddleware(next, CreateOptions());
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(1, callCount);
    }
}
