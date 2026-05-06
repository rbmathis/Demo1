namespace Demo1.Models;

/// <summary>
/// View model for the Security Lab playground page.
/// </summary>
public class SecurityLabViewModel
{
    /// <summary>Gets or sets the current state of each security header (true = enabled/secure).</summary>
    public Dictionary<string, bool> HeaderStates { get; set; } = new();

    /// <summary>Gets or sets the list of available attack scenarios.</summary>
    public List<AttackScenario> AttackScenarios { get; set; } = new();

    /// <summary>Gets or sets the current protection score as a percentage (0-100).</summary>
    public int ProtectionScore { get; set; }
}
