using System.Text.Json;
using Demo1.Models;

namespace Demo1.Services;

/// <summary>
/// Session-backed implementation of <see cref="ISecurityLabService"/> that manages
/// per-user security header configuration for the Security Lab playground.
/// </summary>
public class SecurityLabService : ISecurityLabService
{
    private const string SessionKey = "SecurityLab_Headers";
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly string[] ManagedHeaders = new[]
    {
        "Content-Security-Policy",
        "X-Frame-Options",
        "X-Content-Type-Options",
        "X-XSS-Protection",
        "Referrer-Policy"
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityLabService"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor for session access.</param>
    public SecurityLabService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Dictionary<string, bool> GetHeaderStates()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            return GetDefaultStates();

        var json = session.GetString(SessionKey);
        if (string.IsNullOrEmpty(json))
            return GetDefaultStates();

        return JsonSerializer.Deserialize<Dictionary<string, bool>>(json) ?? GetDefaultStates();
    }

    /// <inheritdoc />
    public void SetHeaderState(string header, bool enabled)
    {
        var states = GetHeaderStates();
        if (states.ContainsKey(header))
        {
            states[header] = enabled;
            SaveStates(states);
        }
    }

    /// <inheritdoc />
    public List<AttackScenario> GetAttackScenarios()
    {
        return new List<AttackScenario>
        {
            new AttackScenario
            {
                Name = "Cross-Site Scripting (XSS)",
                Type = AttackType.XSS,
                Payload = "<script>document.body.style.background='red';document.getElementById('xss-result').innerText='XSS EXECUTED!';</script>",
                ExpectedBehavior = "Script executes and changes the page background to red",
                MitigationExplanation = "Content-Security-Policy restricts which scripts can execute, blocking inline scripts injected by attackers.",
                AffectedHeader = "Content-Security-Policy"
            },
            new AttackScenario
            {
                Name = "Clickjacking",
                Type = AttackType.Clickjacking,
                Payload = "<iframe src='/SecurityLab/VictimPage' style='opacity:0.3;position:absolute;top:0;left:0;width:100%;height:100%;'></iframe>",
                ExpectedBehavior = "A transparent iframe overlays the page, tricking users into clicking hidden elements",
                MitigationExplanation = "X-Frame-Options DENY prevents the page from being embedded in iframes, blocking clickjacking attacks.",
                AffectedHeader = "X-Frame-Options"
            },
            new AttackScenario
            {
                Name = "MIME Type Sniffing",
                Type = AttackType.MimeSniff,
                Payload = "<a href='/SecurityLab/VictimPage' download='malicious.html'>Download Safe File</a>",
                ExpectedBehavior = "Browser may interpret a file as a different MIME type, potentially executing malicious content",
                MitigationExplanation = "X-Content-Type-Options: nosniff prevents browsers from MIME-sniffing a response away from the declared content-type.",
                AffectedHeader = "X-Content-Type-Options"
            }
        };
    }

    /// <inheritdoc />
    public int GetProtectionScore()
    {
        var states = GetHeaderStates();
        if (states.Count == 0) return 100;
        var enabledCount = states.Values.Count(v => v);
        return (int)((double)enabledCount / states.Count * 100);
    }

    /// <inheritdoc />
    public void ResetToDefaults()
    {
        SaveStates(GetDefaultStates());
    }

    private static Dictionary<string, bool> GetDefaultStates()
    {
        return ManagedHeaders.ToDictionary(h => h, _ => true);
    }

    private void SaveStates(Dictionary<string, bool> states)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        session?.SetString(SessionKey, JsonSerializer.Serialize(states));
    }
}
