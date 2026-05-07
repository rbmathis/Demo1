using System.Threading.Channels;
using Demo1.Middleware;
using Demo1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Demo1.UnitTests.Middleware;

/// <summary>
/// Unit tests for <see cref="AchievementMiddleware"/> verifying event publishing and filtering behavior.
/// </summary>
public class AchievementMiddlewareTests
{
    private static Channel<AchievementEventMessage> CreateChannel(int capacity = 100)
    {
        return Channel.CreateBounded<AchievementEventMessage>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
    }

    private static DefaultHttpContext CreateContextWithSession(string path = "/", string method = "GET")
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Method = method;
        context.Features.Set<ISessionFeature>(new TestSessionFeature());
        return context;
    }

    [Fact]
    public async Task InvokeAsync_PublishesEvent_ForNormalRequest()
    {
        // Arrange
        var channel = CreateChannel();
        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        };
        var middleware = new AchievementMiddleware(next);
        var context = CreateContextWithSession("/Home/Privacy");

        // Act
        await middleware.InvokeAsync(context, channel);

        // Assert
        Assert.True(channel.Reader.TryRead(out var message));
        Assert.Equal("/Home/Privacy", message!.RequestPath);
        Assert.Equal("GET", message.HttpMethod);
        Assert.Equal(200, message.StatusCode);
        Assert.Equal("test-session", message.SessionId);
    }

    [Fact]
    public async Task InvokeAsync_SkipsStaticFiles()
    {
        // Arrange
        var channel = CreateChannel();
        RequestDelegate next = ctx => Task.CompletedTask;
        var middleware = new AchievementMiddleware(next);
        var context = CreateContextWithSession("/lib/bootstrap/css/bootstrap.css");

        // Act
        await middleware.InvokeAsync(context, channel);

        // Assert
        Assert.False(channel.Reader.TryRead(out _));
    }

    [Fact]
    public async Task InvokeAsync_SkipsWhenNoSession()
    {
        // Arrange
        var channel = CreateChannel();
        RequestDelegate next = ctx => Task.CompletedTask;
        var middleware = new AchievementMiddleware(next);
        var context = new DefaultHttpContext(); // No session feature
        context.Request.Path = "/Home/Privacy";

        // Act
        await middleware.InvokeAsync(context, channel);

        // Assert
        Assert.False(channel.Reader.TryRead(out _));
    }

    [Fact]
    public async Task InvokeAsync_CapturesStatusCode_AfterNextInvoked()
    {
        // Arrange
        var channel = CreateChannel();
        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = 404;
            return Task.CompletedTask;
        };
        var middleware = new AchievementMiddleware(next);
        var context = CreateContextWithSession("/nonexistent");

        // Act
        await middleware.InvokeAsync(context, channel);

        // Assert
        Assert.True(channel.Reader.TryRead(out var message));
        Assert.Equal(404, message!.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_DoesNotBlock_WhenChannelFull()
    {
        // Arrange — create channel with capacity 1 and fill it
        var channel = CreateChannel(1);
        await channel.Writer.WriteAsync(new AchievementEventMessage());
        // Channel is now full

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        };
        var middleware = new AchievementMiddleware(next);
        var context = CreateContextWithSession("/test");

        // Act — should not throw or block
        var task = middleware.InvokeAsync(context, channel);
        var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(5)));

        // Assert
        Assert.Equal(task, completed); // The task completed, didn't hang
    }

    [Fact]
    public async Task InvokeAsync_InvokesNext_Always()
    {
        // Arrange
        var channel = CreateChannel();
        var invoked = false;
        RequestDelegate next = ctx =>
        {
            invoked = true;
            return Task.CompletedTask;
        };
        var middleware = new AchievementMiddleware(next);
        var context = CreateContextWithSession("/test");

        // Act
        await middleware.InvokeAsync(context, channel);

        // Assert
        Assert.True(invoked);
    }

    [Theory]
    [InlineData("/lib/bootstrap/css/bootstrap.css", true)]
    [InlineData("/css/site.css", true)]
    [InlineData("/js/app.js", true)]
    [InlineData("/favicon.ico", true)]
    [InlineData("/image.png", true)]
    [InlineData("/photo.jpg", true)]
    [InlineData("/script.map", true)]
    [InlineData("/Home/Privacy", false)]
    [InlineData("/api/v1/weather", false)]
    [InlineData("/Achievement/TrophyCase", false)]
    public void ShouldSkip_ReturnsExpected(string path, bool expected)
    {
        Assert.Equal(expected, AchievementMiddleware.ShouldSkip(path));
    }

    #region Test Infrastructure

    private class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();
        public string Id => "test-session";
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _store.Keys;
        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, out byte[]? value) => _store.TryGetValue(key, out value);
    }

    private class TestSessionFeature : ISessionFeature
    {
        public ISession Session { get; set; } = new TestSession();
    }

    #endregion
}
