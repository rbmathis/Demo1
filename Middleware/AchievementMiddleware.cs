using System.Threading.Channels;
using Demo1.Models;

namespace Demo1.Middleware;

/// <summary>
/// Middleware that tracks HTTP requests by publishing achievement events to a Channel&lt;AchievementEventMessage&gt;.
/// Events are published non-blocking (fire-and-forget) and processed asynchronously by the background service.
/// </summary>
public class AchievementMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="AchievementMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public AchievementMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Invokes the middleware. Calls the next delegate first, then publishes an achievement event
    /// if the request is trackable (not a static file, has a session).
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="channel">The achievement event channel (injected per-request).</param>
    public async Task InvokeAsync(HttpContext context, Channel<AchievementEventMessage> channel)
    {
        // Call next FIRST — we need the response status code
        await _next(context);

        // Skip static files and non-trackable requests
        var path = context.Request.Path.Value ?? string.Empty;
        if (ShouldSkip(path))
            return;

        // Skip if no session established
        string? sessionId = null;
        try
        {
            sessionId = context.Session?.Id;
        }
        catch (InvalidOperationException)
        {
            // Session not configured — skip tracking
            return;
        }

        if (string.IsNullOrEmpty(sessionId))
            return;

        // Create and publish the event (non-blocking)
        var message = new AchievementEventMessage
        {
            SessionId = sessionId,
            RequestPath = path,
            HttpMethod = context.Request.Method,
            StatusCode = context.Response.StatusCode,
            Timestamp = DateTime.UtcNow
        };

        if (!channel.Writer.TryWrite(message))
        {
            // Channel is full — log warning but don't block
            // This is expected under heavy load with DropOldest policy
        }
    }

    /// <summary>
    /// Determines whether a request path should be skipped for achievement tracking.
    /// </summary>
    /// <param name="path">The request path to evaluate.</param>
    /// <returns>True if the path should be skipped; otherwise, false.</returns>
    public static bool ShouldSkip(string path)
    {
        // Skip static file directories
        if (path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Skip static file extensions
        var extension = Path.GetExtension(path);
        if (!string.IsNullOrEmpty(extension))
        {
            var skipExtensions = new[] { ".css", ".js", ".map", ".png", ".jpg", ".ico" };
            if (skipExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

/// <summary>
/// Extension methods for adding the Achievement tracking middleware to the application pipeline.
/// </summary>
public static class AchievementMiddlewareExtensions
{
    /// <summary>
    /// Adds the Achievement tracking middleware that publishes request events for badge processing.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseAchievementTracking(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AchievementMiddleware>();
    }
}
