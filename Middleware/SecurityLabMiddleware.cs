using System.Text.Json;

namespace Demo1.Middleware;

/// <summary>
/// Middleware that conditionally removes security headers for Security Lab routes
/// based on per-session configuration. Only activates for paths starting with "/SecurityLab".
/// </summary>
public class SecurityLabMiddleware
{
    private readonly RequestDelegate _next;
    private const string SessionKey = "SecurityLab_Headers";

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityLabMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public SecurityLabMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Invokes the middleware. Only modifies headers for /SecurityLab/* routes.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/SecurityLab", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        context.Response.OnStarting(() =>
        {
            var session = context.Session;
            var json = session.GetString(SessionKey);
            
            if (!string.IsNullOrEmpty(json))
            {
                var headerStates = JsonSerializer.Deserialize<Dictionary<string, bool>>(json);
                if (headerStates != null)
                {
                    foreach (var kvp in headerStates)
                    {
                        if (!kvp.Value)
                        {
                            context.Response.Headers.Remove(kvp.Key);
                        }
                    }
                }
            }

            return Task.CompletedTask;
        });

        await _next(context);
    }
}

/// <summary>
/// Extension methods for adding the Security Lab middleware to the application pipeline.
/// </summary>
public static class SecurityLabMiddlewareExtensions
{
    /// <summary>
    /// Adds the Security Lab middleware that conditionally strips security headers for lab routes.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseSecurityLabHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityLabMiddleware>();
    }
}
