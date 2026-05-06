using System.Threading.Tasks;
using Demo1.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Demo1.UnitTests.Middleware;

public class ServerTimingMiddlewareTests
{
    /// <summary>
    /// Creates an HttpContext that properly supports OnStarting callbacks
    /// by using a custom IHttpResponseFeature implementation.
    /// </summary>
    private static HttpContext CreateContextWithResponseFeature()
    {
        var context = new DefaultHttpContext();
        var feature = new TestHttpResponseFeature();
        context.Features.Set<IHttpResponseFeature>(feature);
        return context;
    }

    [Fact]
    public async Task InvokeAsync_InvokesNextDelegate()
    {
        var invoked = false;
        RequestDelegate next = context =>
        {
            invoked = true;
            return Task.CompletedTask;
        };
        var middleware = new ServerTimingMiddleware(next);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        Assert.True(invoked);
    }

    [Fact]
    public async Task InvokeAsync_AddsServerTimingHeader()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new ServerTimingMiddleware(next);
        var context = CreateContextWithResponseFeature();

        await middleware.InvokeAsync(context);

        // Trigger OnStarting callbacks
        var feature = context.Features.Get<IHttpResponseFeature>() as TestHttpResponseFeature;
        await feature!.FireOnStartingAsync();

        var header = context.Response.Headers["Server-Timing"].ToString();
        Assert.StartsWith("ttfb;dur=", header);
    }

    [Fact]
    public async Task InvokeAsync_MeasuresDuration_GreaterThanOrEqualToZero()
    {
        RequestDelegate next = async _ => await Task.Delay(15);
        var middleware = new ServerTimingMiddleware(next);
        var context = CreateContextWithResponseFeature();

        await middleware.InvokeAsync(context);

        // Trigger OnStarting callbacks
        var feature = context.Features.Get<IHttpResponseFeature>() as TestHttpResponseFeature;
        await feature!.FireOnStartingAsync();

        var header = context.Response.Headers["Server-Timing"].ToString();
        var durStr = header.Replace("ttfb;dur=", "");
        var duration = double.Parse(durStr, System.Globalization.CultureInfo.InvariantCulture);
        Assert.True(duration >= 0);
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrows_ExceptionPropagates()
    {
        RequestDelegate next = _ => throw new InvalidOperationException("Test exception");
        var middleware = new ServerTimingMiddleware(next);
        var context = new DefaultHttpContext();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => middleware.InvokeAsync(context));
    }

    /// <summary>
    /// Test implementation of IHttpResponseFeature that captures OnStarting callbacks.
    /// </summary>
    private class TestHttpResponseFeature : IHttpResponseFeature
    {
        private readonly List<(Func<object, Task> Callback, object State)> _onStarting = new();

        public int StatusCode { get; set; } = 200;
        public string? ReasonPhrase { get; set; }
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public Stream Body { get; set; } = new MemoryStream();
        public bool HasStarted { get; private set; }

        public void OnStarting(Func<object, Task> callback, object state)
        {
            _onStarting.Add((callback, state));
        }

        public void OnCompleted(Func<object, Task> callback, object state) { }

        public async Task FireOnStartingAsync()
        {
            HasStarted = true;
            // Fire in reverse order (same as Kestrel)
            for (int i = _onStarting.Count - 1; i >= 0; i--)
            {
                await _onStarting[i].Callback(_onStarting[i].State);
            }
        }
    }
}
