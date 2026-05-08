using Autopilot.GitHub;

namespace Autopilot.Tests;

public sealed class SdkLabelServiceTests
{
    [Fact]
    public async Task SetStageAsync_RemovesOnlySdkStageLabels()
    {
        var issueClient = new FakeIssueClient
        {
            IssueLabels = ["sdk", "sdk/planning", "bug", "local/triage"]
        };
        var service = new SdkLabelService(issueClient, TextWriter.Null);

        await service.SetStageAsync(122, "sdk/review");

        Assert.Equal(["sdk/planning"], issueClient.RemovedLabels);
        Assert.Equal(["sdk/review"], issueClient.AddedLabels);
    }

    [Fact]
    public async Task EnsureRequiredLabelsAsync_CreatesOnlyMissingLabels()
    {
        var issueClient = new FakeIssueClient
        {
            RepositoryLabels = new HashSet<string>(["sdk", "sdk/triage"], StringComparer.OrdinalIgnoreCase)
        };
        var service = new SdkLabelService(issueClient, TextWriter.Null);

        await service.EnsureRequiredLabelsAsync(createMissing: true);

        Assert.DoesNotContain("sdk", issueClient.CreatedLabels);
        Assert.Contains("sdk/done", issueClient.CreatedLabels);
        Assert.Contains("sdk/failed", issueClient.CreatedLabels);
    }

    private sealed class FakeIssueClient : IGitHubIssueClient
    {
        public IReadOnlyList<string> IssueLabels { get; init; } = [];
        public IReadOnlySet<string> RepositoryLabels { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public List<string> AddedLabels { get; } = [];
        public List<string> RemovedLabels { get; } = [];
        public List<string> CreatedLabels { get; } = [];

        public Task AddIssueLabelAsync(int issueNumber, string label, CancellationToken cancellationToken = default)
        {
            AddedLabels.Add(label);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<string>> GetIssueLabelsAsync(int issueNumber, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(IssueLabels);
        }

        public Task<string> GetIssueStateAsync(int issueNumber, CancellationToken cancellationToken = default)
        {
            return Task.FromResult("OPEN");
        }

        public Task<IReadOnlySet<string>> GetRepositoryLabelsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(RepositoryLabels);
        }

        public Task RemoveIssueLabelAsync(int issueNumber, string label, CancellationToken cancellationToken = default)
        {
            RemovedLabels.Add(label);
            return Task.CompletedTask;
        }

        public Task CreateOrUpdateLabelAsync(string label, string color, string description, CancellationToken cancellationToken = default)
        {
            CreatedLabels.Add(label);
            return Task.CompletedTask;
        }
    }
}
