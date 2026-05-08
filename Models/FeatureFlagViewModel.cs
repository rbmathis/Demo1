namespace Demo1.Models;

/// <summary>
/// Represents a single feature flag and its current state.
/// </summary>
public class FeatureFlagViewModel
{
    /// <summary>Gets or sets the feature flag name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets whether the flag is currently enabled.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Gets or sets the backing store that provides this flag (e.g., "Azure App Configuration" or "Local config").</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>Gets or sets the Azure App Configuration label applied to this flag, if any.</summary>
    public string Label { get; set; } = string.Empty;
}

/// <summary>
/// View model for the feature flag admin dashboard.
/// </summary>
public class FeatureFlagDashboardViewModel
{
    /// <summary>Gets or sets the collection of known feature flags and their current states.</summary>
    public IReadOnlyList<FeatureFlagViewModel> Flags { get; set; } = [];

    /// <summary>
    /// Gets or sets whether Azure App Configuration is configured and available as the backing store.
    /// When <c>false</c>, the dashboard is read-only and toggle actions are disabled.
    /// </summary>
    public bool IsAzureAppConfigurationAvailable { get; set; }

    /// <summary>Gets or sets the feature-flag refresh interval in seconds (from Azure App Configuration).</summary>
    public int RefreshIntervalSeconds { get; set; } = 30;

    /// <summary>Gets or sets the Azure App Configuration label being queried, or an empty string for the default label.</summary>
    public string Label { get; set; } = string.Empty;
}
