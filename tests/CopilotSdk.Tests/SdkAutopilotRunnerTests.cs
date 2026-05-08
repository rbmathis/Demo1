using Autopilot.Copilot;
using Autopilot.GitHub;
using Autopilot.Options;
using Autopilot.Pipeline;

namespace Autopilot.Tests;

public sealed class SdkAutopilotRunnerTests
{
    [Fact]
    public async Task RunAsync_ClosedIssue_NoOpsBeforeLabelsAndStages()
    {
        var issueClient = new FakeIssueClient { IssueState = "CLOSED" };
        var labels = new FakeLabelService();
        var stageRunner = new FakeStageRunner();
        var modelChecker = new FakeModelChecker(ModelAvailabilityResult.Available);
        var output = new StringWriter();
        var runner = CreateRunner(issueClient, labels, stageRunner, modelChecker, output);

        var exitCode = await runner.RunAsync();

        Assert.Equal(0, exitCode);
        Assert.Contains("already closed", output.ToString());
        Assert.Equal(0, labels.EnsureRequiredCalls);
        Assert.Equal(0, labels.EnsureProvenanceCalls);
        Assert.Empty(labels.StageLabels);
        Assert.Equal(0, stageRunner.Calls);
        Assert.Equal(0, modelChecker.Calls);
    }

    [Fact]
    public async Task RunAsync_UnavailableModel_StopsBeforeProvenanceAndStages()
    {
        var issueClient = new FakeIssueClient { IssueState = "OPEN" };
        var labels = new FakeLabelService();
        var stageRunner = new FakeStageRunner();
        var modelChecker = new FakeModelChecker(ModelAvailabilityResult.Unavailable("Model is not available."));
        var output = new StringWriter();
        var runner = CreateRunner(issueClient, labels, stageRunner, modelChecker, output, approveAll: true);

        var exitCode = await runner.RunAsync();

        Assert.Equal(11, exitCode);
        Assert.Contains("Copilot model is not available", output.ToString());
        Assert.Equal(1, labels.EnsureRequiredCalls);
        Assert.Equal(0, labels.EnsureProvenanceCalls);
        Assert.Empty(labels.StageLabels);
        Assert.Equal(0, stageRunner.Calls);
    }

    [Fact]
    public async Task RunAsync_CheckModelOnly_DoesNotTouchIssueOrLabels()
    {
        var issueClient = new FakeIssueClient { IssueState = "OPEN" };
        var labels = new FakeLabelService();
        var modelChecker = new FakeModelChecker(ModelAvailabilityResult.Available);
        var output = new StringWriter();
        var options = CreateOptions(issueNumber: 0, approveAll: false) with { CheckModelOnly = true };
        var runner = new SdkAutopilotRunner(options, issueClient, labels, new FakePromptBuilder(), new FakeStageRunner(), modelChecker, output);

        var exitCode = await runner.RunAsync();

        Assert.Equal(0, exitCode);
        Assert.Equal(0, issueClient.StateCalls);
        Assert.Equal(0, labels.EnsureRequiredCalls);
        Assert.Equal(1, modelChecker.Calls);
    }

    private static SdkAutopilotRunner CreateRunner(
        FakeIssueClient issueClient,
        FakeLabelService labels,
        FakeStageRunner stageRunner,
        FakeModelChecker modelChecker,
        TextWriter output,
        bool approveAll = false)
    {
        var options = CreateOptions(122, approveAll);
        return new SdkAutopilotRunner(options, issueClient, labels, new FakePromptBuilder(), stageRunner, modelChecker, output);
    }

    private static AutopilotOptions CreateOptions(int issueNumber, bool approveAll)
    {
        return new AutopilotOptions(issueNumber, Directory.GetCurrentDirectory(), "rbmathis/Demo1", "test-model", false, false, false, false, TimeSpan.FromMinutes(10), approveAll, false, false);
    }

    private sealed class FakeIssueClient : IGitHubIssueClient
    {
        public string IssueState { get; init; } = "OPEN";
        public int StateCalls { get; private set; }

        public Task AddIssueLabelAsync(int issueNumber, string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<string>> GetIssueLabelsAsync(int issueNumber, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<string>>([]);

        public Task<string> GetIssueStateAsync(int issueNumber, CancellationToken cancellationToken = default)
        {
            StateCalls++;
            return Task.FromResult(IssueState);
        }

        public Task<IReadOnlySet<string>> GetRepositoryLabelsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlySet<string>>(new HashSet<string>());
        public Task RemoveIssueLabelAsync(int issueNumber, string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CreateOrUpdateLabelAsync(string label, string color, string description, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeLabelService : ISdkLabelService
    {
        public int EnsureRequiredCalls { get; private set; }
        public int EnsureProvenanceCalls { get; private set; }
        public List<string> StageLabels { get; } = [];

        public Task EnsureProvenanceAsync(int issueNumber, CancellationToken cancellationToken = default)
        {
            EnsureProvenanceCalls++;
            return Task.CompletedTask;
        }

        public Task EnsureRequiredLabelsAsync(bool createMissing, CancellationToken cancellationToken = default)
        {
            EnsureRequiredCalls++;
            return Task.CompletedTask;
        }

        public Task SetStageAsync(int issueNumber, string stageLabel, CancellationToken cancellationToken = default)
        {
            StageLabels.Add(stageLabel);
            return Task.CompletedTask;
        }
    }

    private sealed class FakePromptBuilder : IPromptBuilder
    {
        public Task<string> BuildAsync(StageDefinition stage, string mission, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"Prompt for {stage.Name}");
        }
    }

    private sealed class FakeStageRunner : IStageRunner
    {
        public int Calls { get; private set; }

        public TimeSpan? LastTimeout { get; private set; }

        public Task<StageResult> RunAsync(StageDefinition stage, string prompt, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            Calls++;
            LastTimeout = timeout;
            return Task.FromResult(new StageResult("GO", "approved", "rollout-exempt", true, null));
        }
    }

    private sealed class FakeModelChecker(ModelAvailabilityResult result) : IModelAvailabilityChecker
    {
        public int Calls { get; private set; }

        public Task<ModelAvailabilityResult> CheckAsync(string model, string repoRoot, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(result);
        }
    }
}
