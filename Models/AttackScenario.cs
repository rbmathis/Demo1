namespace Demo1.Models;

/// <summary>
/// Represents a type of security attack that can be demonstrated in the Security Lab.
/// </summary>
public enum AttackType
{
    /// <summary>Cross-site scripting attack.</summary>
    XSS,
    /// <summary>Clickjacking attack using iframes.</summary>
    Clickjacking,
    /// <summary>MIME type sniffing attack.</summary>
    MimeSniff
}

/// <summary>
/// Represents a security attack scenario with its payload and explanation.
/// </summary>
public class AttackScenario
{
    /// <summary>Gets or sets the display name of the attack.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the type of attack.</summary>
    public AttackType Type { get; set; }

    /// <summary>Gets or sets the attack payload (HTML/JS code).</summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>Gets or sets the expected behavior when the attack succeeds.</summary>
    public string ExpectedBehavior { get; set; } = string.Empty;

    /// <summary>Gets or sets the explanation of how the header mitigates this attack.</summary>
    public string MitigationExplanation { get; set; } = string.Empty;

    /// <summary>Gets or sets the security header that prevents this attack.</summary>
    public string AffectedHeader { get; set; } = string.Empty;
}
