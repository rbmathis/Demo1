namespace Demo1.Features;

/// <summary>
/// Constants for feature flag names used with Microsoft.FeatureManagement.
/// </summary>
/// <remarks>
/// <para>
/// All flags use PascalCase names and must be representable in both
/// <c>appsettings.json</c> (under <c>FeatureManagement</c>) and Azure App Configuration.
/// New flags default to <c>false</c> (off) in <c>appsettings.json</c>.
/// </para>
/// <para>
/// <strong>Temporary rollout flags</strong> are introduced for a specific issue and
/// removed after rollout completes. They must declare an owner, cleanup milestone,
/// and cleanup issue reference in the rollout checklist.
/// </para>
/// <para>
/// <strong>Permanent product flags</strong> control long-lived feature toggles and
/// are not subject to automatic cleanup. Mark them as permanent in the rollout checklist.
/// </para>
/// <para>
/// See <c>docs/feature-flag-rollout-contract.md</c> and
/// <c>docs/feature-flag-runtime-guide.md</c> for conventions.
/// </para>
/// </remarks>
public static class FeatureFlags
{
    // --- Permanent product flags ---

    /// <summary>
    /// Example feature flag - Feature1. Permanent product flag.
    /// </summary>
    public const string Feature1 = "Feature1";

    /// <summary>
    /// Dark mode feature toggle. Permanent product flag.
    /// </summary>
    public const string DarkMode = "DarkMode";

    /// <summary>
    /// Contact form feature toggle. Permanent product flag.
    /// </summary>
    public const string ContactForm = "ContactForm";

    /// <summary>
    /// Beta features master toggle. Permanent product flag.
    /// </summary>
    public const string BetaFeatures = "BetaFeatures";

    // --- Temporary rollout flags ---
    // Add temporary flags below. Each must have a cleanup issue reference
    // in the rollout checklist. Remove the flag after rollout completes.

    /// <summary>
    /// Dashboard home page experience rollout toggle. Temporary rollout flag.
    /// </summary>
    public const string DashboardHomePage = "DashboardHomePage";
}
