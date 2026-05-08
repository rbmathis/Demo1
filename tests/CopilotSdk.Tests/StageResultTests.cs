using Autopilot.Pipeline;

namespace Autopilot.Tests;

public sealed class StageResultTests
{
    [Fact]
    public void Parse_UsesLastFencedJsonBlock()
    {
        var result = StageResult.Parse("""
			```json
			{ "status": "STOP" }
			```
			Later result:
			```json
			{ "status": "GO", "decision": "APPROVED", "rollout_status": "rollout-exempt" }
			```
			""");

        Assert.True(result.IsValid);
        Assert.Equal("GO", result.Status);
        Assert.Equal("approved", result.Decision);
        Assert.Equal("rollout-exempt", result.RolloutStatus);
    }

    [Fact]
    public void Parse_ReturnsInvalidWhenJsonBlockIsMissing()
    {
        var result = StageResult.Parse("No structured result here.");

        Assert.False(result.IsValid);
        Assert.Equal("INVALID", result.Status);
        Assert.Contains("No fenced JSON", result.Error);
    }

    [Fact]
    public void Parse_ReturnsInvalidForUnknownDecision()
    {
        var result = StageResult.Parse("""
			```json
			{ "status": "GO", "decision": "merge_now" }
			```
			""");

        Assert.False(result.IsValid);
        Assert.Contains("Unknown decision", result.Error);
    }
}
