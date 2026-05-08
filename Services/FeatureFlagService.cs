using Azure.Data.AppConfiguration;
using Azure.Identity;
using Demo1.Features;
using Demo1.Models;
using Microsoft.FeatureManagement;

namespace Demo1.Services;

/// <summary>
/// Reads feature flags via <see cref="IFeatureManager"/> and writes them back to
/// Azure App Configuration when that store is configured.
/// </summary>
public class FeatureFlagService : IFeatureFlagService
{
    private readonly IFeatureManager _featureManager;
    private readonly AzureAppConfigAdminOptions _adminOptions;
    private readonly ILogger<FeatureFlagService> _logger;

    // The set of flags managed by this application, in display order.
    private static readonly string[] KnownFlags =
    [
        FeatureFlags.Feature1,
        FeatureFlags.DarkMode,
        FeatureFlags.ContactForm,
        FeatureFlags.BetaFeatures,
    ];

    /// <inheritdoc/>
    public bool IsAzureAppConfigurationAvailable => _adminOptions.IsAvailable;

    /// <summary>
    /// Initializes a new instance of <see cref="FeatureFlagService"/>.
    /// </summary>
    /// <param name="featureManager">The ASP.NET feature manager used to read current flag states.</param>
    /// <param name="adminOptions">Azure App Configuration connectivity options registered at startup.</param>
    /// <param name="logger">Logger for audit and diagnostic messages.</param>
    public FeatureFlagService(
        IFeatureManager featureManager,
        AzureAppConfigAdminOptions adminOptions,
        ILogger<FeatureFlagService> logger)
    {
        _featureManager = featureManager;
        _adminOptions = adminOptions;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FeatureFlagViewModel>> GetFlagsAsync(CancellationToken cancellationToken = default)
    {
        var flags = new List<FeatureFlagViewModel>(KnownFlags.Length);
        var source = _adminOptions.IsAvailable ? "Azure App Configuration" : "Local config";

        foreach (var name in KnownFlags)
        {
            var isEnabled = await _featureManager.IsEnabledAsync(name);
            flags.Add(new FeatureFlagViewModel
            {
                Name = name,
                IsEnabled = isEnabled,
                Source = source,
                Label = _adminOptions.Label,
            });
        }

        return flags;
    }

    /// <inheritdoc/>
    public async Task<bool> SetFlagAsync(string flagName, bool enabled, CancellationToken cancellationToken = default)
    {
        if (!_adminOptions.IsAvailable)
        {
            _logger.LogWarning(
                "Feature flag toggle rejected — Azure App Configuration is not configured. Flag: {FlagName}",
                flagName);
            return false;
        }

        if (!KnownFlags.Contains(flagName, StringComparer.Ordinal))
        {
            _logger.LogWarning("Feature flag toggle rejected — unknown flag name: {FlagName}", flagName);
            return false;
        }

        var client = CreateConfigurationClient();
        if (client is null)
        {
            _logger.LogError(
                "Feature flag toggle failed — could not create Azure App Configuration client. Flag: {FlagName}",
                flagName);
            return false;
        }

        var setting = new FeatureFlagConfigurationSetting(flagName, isEnabled: enabled);
        if (!string.IsNullOrEmpty(_adminOptions.Label))
        {
            setting.Label = _adminOptions.Label;
        }

        await client.SetConfigurationSettingAsync(setting, onlyIfUnchanged: false, cancellationToken);

        _logger.LogInformation(
            "Feature flag {FlagName} changed to {Enabled} in Azure App Configuration (label: '{Label}')",
            flagName,
            enabled,
            _adminOptions.Label);

        return true;
    }

    /// <summary>
    /// Creates an <see cref="Azure.Data.AppConfiguration.ConfigurationClient"/> from the stored options.
    /// Returns <c>null</c> when neither an endpoint nor a connection string is configured.
    /// </summary>
    private ConfigurationClient? CreateConfigurationClient()
    {
        if (!string.IsNullOrWhiteSpace(_adminOptions.Endpoint))
        {
            return new ConfigurationClient(new Uri(_adminOptions.Endpoint), new DefaultAzureCredential());
        }

        if (!string.IsNullOrWhiteSpace(_adminOptions.ConnectionString))
        {
            return new ConfigurationClient(_adminOptions.ConnectionString);
        }

        return null;
    }
}
