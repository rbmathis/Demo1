using Demo1.Features;
using Demo1.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;

namespace Demo1.UnitTests.Middleware;

/// <summary>
/// Unit tests for <see cref="ThemePreferenceMiddleware"/> verifying flag-gating,
/// cookie validation, and HttpContext.Items population.
/// </summary>
public class ThemePreferenceMiddlewareTests
{
    private static ThemePreferenceMiddleware CreateMiddleware(
        RequestDelegate next,
        bool darkModeEnabled,
        out Mock<IFeatureManager> featureManagerMock)
    {
        featureManagerMock = new Mock<IFeatureManager>();
        featureManagerMock
            .Setup(fm => fm.IsEnabledAsync(FeatureFlags.DarkMode))
            .ReturnsAsync(darkModeEnabled);

        var logger = Mock.Of<ILogger<ThemePreferenceMiddleware>>();
        return new ThemePreferenceMiddleware(next, featureManagerMock.Object, logger);
    }

    private static DefaultHttpContext CreateContextWithCookie(string? cookieValue = null)
    {
        var context = new DefaultHttpContext();
        if (cookieValue is not null)
        {
            context.Request.Headers["Cookie"] = $"theme-preference={cookieValue}";
        }
        return context;
    }

    [Fact]
    public async Task InvokeAsync_FlagOff_SkipsMiddleware_CallsNext()
    {
        var nextCalled = false;
        RequestDelegate next = ctx => { nextCalled = true; return Task.CompletedTask; };
        var middleware = CreateMiddleware(next, darkModeEnabled: false, out _);
        var context = CreateContextWithCookie("dark");

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.False(context.Items.ContainsKey("ThemePreference"));
    }

    [Fact]
    public async Task InvokeAsync_FlagOn_NoCookie_CallsNext_NoItemSet()
    {
        var nextCalled = false;
        RequestDelegate next = ctx => { nextCalled = true; return Task.CompletedTask; };
        var middleware = CreateMiddleware(next, darkModeEnabled: true, out _);
        var context = CreateContextWithCookie(); // no cookie

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.False(context.Items.ContainsKey("ThemePreference"));
    }

    [Fact]
    public async Task InvokeAsync_FlagOn_ValidDarkCookie_SetsItemToDark()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = CreateMiddleware(next, darkModeEnabled: true, out _);
        var context = CreateContextWithCookie("dark");

        await middleware.InvokeAsync(context);

        Assert.Equal("dark", context.Items["ThemePreference"]);
    }

    [Fact]
    public async Task InvokeAsync_FlagOn_ValidLightCookie_SetsItemToLight()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = CreateMiddleware(next, darkModeEnabled: true, out _);
        var context = CreateContextWithCookie("light");

        await middleware.InvokeAsync(context);

        Assert.Equal("light", context.Items["ThemePreference"]);
    }

    [Fact]
    public async Task InvokeAsync_FlagOn_ValidAutoCookie_SetsItemToAuto()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = CreateMiddleware(next, darkModeEnabled: true, out _);
        var context = CreateContextWithCookie("auto");

        await middleware.InvokeAsync(context);

        Assert.Equal("auto", context.Items["ThemePreference"]);
    }

    [Fact]
    public async Task InvokeAsync_FlagOn_InvalidCookieValue_DoesNotSetItem()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = CreateMiddleware(next, darkModeEnabled: true, out _);
        var context = CreateContextWithCookie("rainbow");

        await middleware.InvokeAsync(context);

        Assert.False(context.Items.ContainsKey("ThemePreference"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InvokeAsync_AlwaysCallsNext(bool darkModeEnabled)
    {
        var nextCalled = false;
        RequestDelegate next = ctx => { nextCalled = true; return Task.CompletedTask; };
        var middleware = CreateMiddleware(next, darkModeEnabled, out _);
        var context = CreateContextWithCookie("dark");

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }
}
