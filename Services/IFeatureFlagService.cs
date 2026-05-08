using Demo1.Models;

namespace Demo1.Services;

/// <summary>
/// Provides read and write access to the application's feature flags.
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Gets whether Azure App Configuration is available as the backing store.
    /// When <c>false</c>, <see cref="SetFlagAsync"/> will always return <c>false</c>.
    /// </summary>
    bool IsAzureAppConfigurationAvailable { get; }

    /// <summary>
    /// Returns the current state of all known feature flags.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A read-only list of <see cref="FeatureFlagViewModel"/> instances.</returns>
    Task<IReadOnlyList<FeatureFlagViewModel>> GetFlagsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables or disables a feature flag in Azure App Configuration.
    /// </summary>
    /// <param name="flagName">The name of the flag to change.</param>
    /// <param name="enabled">The new enabled state.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>
    /// <c>true</c> if the flag was updated successfully; <c>false</c> if Azure App Configuration
    /// is not available or the flag name is not recognised.
    /// </returns>
    Task<bool> SetFlagAsync(string flagName, bool enabled, CancellationToken cancellationToken = default);
}
