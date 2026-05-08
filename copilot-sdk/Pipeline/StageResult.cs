using System.Text.Json;
using System.Text.RegularExpressions;

namespace Autopilot.Pipeline;

internal sealed record StageResult(string Status, string Decision, string RolloutStatus, bool IsValid, string? Error)
{
    public static StageResult Empty { get; } = new("GO", "unknown", "unknown", true, null);

    public static StageResult Parse(string content)
    {
        var jsonMatches = Regex.Matches(content, "```json\\s*(?<json>\\{.*?\\})\\s*```", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (jsonMatches.Count == 0)
        {
            return Invalid("No fenced JSON result block found.");
        }

        var json = jsonMatches[^1].Groups["json"].Value;
        try
        {
            using var document = JsonDocument.Parse(json);
            var status = ReadString(document.RootElement, "status");
            if (string.IsNullOrWhiteSpace(status))
            {
                return Invalid("JSON result is missing required property 'status'.");
            }

            var normalizedStatus = status.ToUpperInvariant();
            if (normalizedStatus is not ("GO" or "STOP" or "DUPLICATE"))
            {
                return Invalid($"Unknown status '{status}'.");
            }

            var decision = ReadString(document.RootElement, "decision")?.ToLowerInvariant() ?? "unknown";
            if (decision is not ("unknown" or "approved" or "changes_requested" or "comment"))
            {
                return Invalid($"Unknown decision '{decision}'.");
            }

            var rolloutStatus = ReadString(document.RootElement, "rollout_status")?.ToLowerInvariant() ?? "unknown";
            return new StageResult(normalizedStatus, decision, rolloutStatus, true, null);
        }
        catch (JsonException ex)
        {
            return Invalid($"Malformed JSON result: {ex.Message}");
        }
    }

    private static string? ReadString(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var property) ? property.GetString() : null;
    }

    private static StageResult Invalid(string error)
    {
        return new StageResult("INVALID", "unknown", "unknown", false, error);
    }
}
