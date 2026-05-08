using Demo1.Features;
using Microsoft.FeatureManagement;

namespace Demo1.Middleware;

/// <summary>
/// Middleware that reads the user's theme preference from a cookie and stores it in
/// <see cref="HttpContext.Items"/> for use during response rendering. Suppressed entirely
/// when the <c>DarkMode</c> feature flag is disabled.
/// </summary>
public class ThemePreferenceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<ThemePreferenceMiddleware> _logger;

    private static readonly HashSet<string> AllowedThemes = new(StringComparer.OrdinalIgnoreCase)
    {
        "light", "dark", "auto"
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemePreferenceMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="featureManager">The feature manager used to evaluate the DarkMode flag.</param>
    /// <param name="logger">The logger instance.</param>
    public ThemePreferenceMiddleware(
        RequestDelegate next,
        IFeatureManager featureManager,
        ILogger<ThemePreferenceMiddleware> logger)
    {
        _next = next;
        _featureManager = featureManager;
        _logger = logger;
    }

    /// <summary>
    /// Reads the <c>theme-preference</c> cookie and stores the validated value in
    /// <c>HttpContext.Items["ThemePreference"]</c> when the <c>DarkMode</c> flag is enabled.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!await _featureManager.IsEnabledAsync(FeatureFlags.DarkMode))
        {
            _logger.LogDebug("DarkMode flag off — theme middleware suppressed");
            await _next(context);
            return;
        }

        if (context.Request.Cookies.TryGetValue("theme-preference", out var theme) &&
            AllowedThemes.Contains(theme))
        {
            context.Items["ThemePreference"] = theme.ToLowerInvariant();
            _logger.LogInformation(
                "Theme preference {Theme} applied from cookie for request {Path}",
                theme,
                context.Request.Path);
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods for adding <see cref="ThemePreferenceMiddleware"/> to the pipeline.
/// </summary>
public static class ThemePreferenceMiddlewareExtensions
{
    /// <summary>
    /// Adds the <see cref="ThemePreferenceMiddleware"/> to the application pipeline.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseThemePreference(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ThemePreferenceMiddleware>();
    }
}
