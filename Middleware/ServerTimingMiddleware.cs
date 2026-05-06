using System.Diagnostics;

namespace Demo1.Middleware;

/// <summary>
/// Middleware that measures request duration and adds a Server-Timing header
/// with the time-to-first-byte (TTFB) measurement.
/// </summary>
public class ServerTimingMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerTimingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public ServerTimingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Invokes the middleware to measure request processing time and add the Server-Timing header.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        context.Response.OnStarting(() =>
        {
            stopwatch.Stop();
            var duration = stopwatch.Elapsed.TotalMilliseconds;
            context.Response.Headers.Append("Server-Timing", $"ttfb;dur={duration:F1}");
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
