namespace Demo1.Services;

/// <summary>
/// Service interface for managing the Security Lab header configuration and attack scenarios.
/// </summary>
public interface ISecurityLabService
{
    /// <summary>Gets the current header states from session.</summary>
    /// <returns>Dictionary of header names to enabled states.</returns>
    Dictionary<string, bool> GetHeaderStates();

    /// <summary>Sets the enabled state for a specific header.</summary>
    /// <param name="header">The header name.</param>
    /// <param name="enabled">Whether the header should be enabled.</param>
    void SetHeaderState(string header, bool enabled);

    /// <summary>Gets the list of available attack scenarios.</summary>
    /// <returns>List of attack scenarios.</returns>
    List<Demo1.Models.AttackScenario> GetAttackScenarios();

    /// <summary>Gets the current protection score based on enabled headers.</summary>
    /// <returns>Protection percentage from 0 to 100.</returns>
    int GetProtectionScore();

    /// <summary>Resets all headers to their default enabled state.</summary>
    void ResetToDefaults();
}
