using Autopilot.Options;

namespace Autopilot.Tests;

public sealed class AutopilotOptionsTests
{
    [Fact]
    public void Parse_UsesDefaultModel()
    {
        var options = AutopilotOptions.Parse(["--check-model", "--repo-root", Directory.GetCurrentDirectory()]);

        Assert.Equal("claude-sonnet-4.6", options.Model);
        Assert.True(options.CheckModelOnly);
        Assert.Equal(TimeSpan.FromMinutes(10), options.StageTimeout);
    }

    [Fact]
    public void Parse_AllowsModelCheckWithoutIssueNumber()
    {
        var options = AutopilotOptions.Parse(["--check-model", "--model", "custom-model", "--repo-root", Directory.GetCurrentDirectory()]);

        Assert.Equal(0, options.IssueNumber);
        Assert.Equal("custom-model", options.Model);
    }

    [Fact]
    public void Parse_RequiresIssueNumberForPipelineRuns()
    {
        var exception = Assert.Throws<ArgumentException>(() => AutopilotOptions.Parse(["--repo-root", Directory.GetCurrentDirectory()]));

        Assert.Contains("positive issue number", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_AllowsCustomStageTimeout()
    {
        var options = AutopilotOptions.Parse(["--check-model", "--stage-timeout-minutes", "2.5", "--repo-root", Directory.GetCurrentDirectory()]);

        Assert.Equal(TimeSpan.FromMinutes(2.5), options.StageTimeout);
    }
}
