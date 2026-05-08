using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo1.Controllers;

/// <summary>
/// Admin-only dashboard for inspecting and toggling application feature flags at runtime.
/// Requires the <c>AdminOnly</c> authorization policy (cookie-authenticated Admin role).
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class FeatureFlagController : Controller
{
    private readonly IFeatureFlagService _featureFlagService;
    private readonly ILogger<FeatureFlagController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="FeatureFlagController"/>.
    /// </summary>
    /// <param name="featureFlagService">Service for reading and writing feature flags.</param>
    /// <param name="logger">Logger for diagnostic and audit messages.</param>
    public FeatureFlagController(IFeatureFlagService featureFlagService, ILogger<FeatureFlagController> logger)
    {
        _featureFlagService = featureFlagService;
        _logger = logger;
    }

    /// <summary>
    /// Displays the feature flag admin dashboard.
    /// </summary>
    /// <returns>The dashboard view showing all known flags and their current state.</returns>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var flags = await _featureFlagService.GetFlagsAsync(HttpContext.RequestAborted);

        var model = new FeatureFlagDashboardViewModel
        {
            Flags = flags,
            IsAzureAppConfigurationAvailable = _featureFlagService.IsAzureAppConfigurationAvailable,
            RefreshIntervalSeconds = 30,
            Label = flags.Count > 0 ? flags[0].Label : string.Empty,
        };

        return View(model);
    }

    /// <summary>
    /// Toggles a feature flag in Azure App Configuration and redirects back to the dashboard.
    /// </summary>
    /// <param name="flagName">The name of the flag to toggle.</param>
    /// <param name="enabled">The new enabled state (<c>true</c> = on, <c>false</c> = off).</param>
    /// <returns>A redirect to the dashboard with a status message in TempData.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(string flagName, bool enabled)
    {
        var adminUser = SanitizeForLog(User.Identity?.Name);
        var safeFlagName = SanitizeForLog(flagName);

        _logger.LogInformation(
            "Admin '{AdminUser}' requested flag toggle: {FlagName} → {Enabled}",
            adminUser, safeFlagName, enabled);

        var success = await _featureFlagService.SetFlagAsync(flagName, enabled, HttpContext.RequestAborted);

        if (success)
        {
            _logger.LogInformation(
                "Feature flag changed — flag: {FlagName}, new state: {Enabled}, admin: {AdminUser}",
                safeFlagName, enabled, adminUser);

            TempData["Success"] = $"Flag '{flagName}' has been {(enabled ? "enabled" : "disabled")}. " +
                                  "Changes propagate within the configured refresh interval.";
        }
        else
        {
            _logger.LogWarning(
                "Feature flag toggle failed — flag: {FlagName}, requested state: {Enabled}, admin: {AdminUser}",
                safeFlagName, enabled, adminUser);

            TempData["Error"] = _featureFlagService.IsAzureAppConfigurationAvailable
                ? $"Failed to update flag '{flagName}'. Check application logs for details."
                : "Azure App Configuration is not configured — flags can only be changed in appsettings.json.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Strips newline characters from a value before it is written to a log sink,
    /// preventing log-forging via user-supplied input.
    /// </summary>
    private static string SanitizeForLog(string? value) =>
        value?.ReplaceLineEndings(" ").Trim() ?? "unknown";
}
