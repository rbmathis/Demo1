using Demo1.Models;
using Microsoft.Extensions.Options;

namespace Demo1.Middleware;

/// <summary>
/// Middleware that adds X-RateLimit-Limit and X-RateLimit-Remaining headers to HTTP responses.
/// Provides clients with visibility into their current rate limit status.
/// </summary>
public class RateLimitHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly int _permitLimit;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitHeadersMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="options">The rate limiting configuration options.</param>
    public RateLimitHeadersMiddleware(RequestDelegate next, IOptions<RateLimitingOptions> options)
    {
        _next = next;
        _permitLimit = options.Value.PermitLimit;
    }

    /// <summary>
    /// Invokes the middleware to add rate limit headers to the response.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        // Add rate limit headers before passing to next middleware.
        // Headers set here may be overwritten by the built-in rate limiter's
        // OnRejected callback when a 429 response is returned.
        context.Response.Headers.Append("X-RateLimit-Limit", _permitLimit.ToString());
        context.Response.Headers.Append("X-RateLimit-Remaining", _permitLimit.ToString());

        await _next(context);
    }
}

/// <summary>
/// Extension methods for adding rate limit headers middleware to the application pipeline.
/// </summary>
public static class RateLimitHeadersMiddlewareExtensions
{
    /// <summary>
    /// Adds the rate limit headers middleware to the application pipeline.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseRateLimitHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitHeadersMiddleware>();
    }
}
