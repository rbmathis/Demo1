namespace Demo1.Models;

/// <summary>
/// Carries Azure App Configuration connectivity information for the feature-flag admin dashboard.
/// Registered as a singleton in DI by <c>Program.cs</c> once the configuration provider is wired up.
/// </summary>
public class AzureAppConfigAdminOptions
{
    /// <summary>
    /// Gets or sets whether the Azure App Configuration provider was successfully registered at startup.
    /// When <c>false</c>, the admin dashboard operates in read-only mode.
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>Gets or sets the Azure App Configuration endpoint URL, or <c>null</c> when not configured.</summary>
    public string? Endpoint { get; set; }

    /// <summary>Gets or sets the Azure App Configuration connection string, or <c>null</c> when not configured.</summary>
    public string? ConnectionString { get; set; }

    /// <summary>Gets or sets the label applied when reading/writing feature flags.</summary>
    public string Label { get; set; } = string.Empty;
}
