namespace Autopilot.GitHub;

internal interface IGitHubIssueClient
{
    Task AddIssueLabelAsync(int issueNumber, string label, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetIssueLabelsAsync(int issueNumber, CancellationToken cancellationToken = default);
    Task<string> GetIssueStateAsync(int issueNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlySet<string>> GetRepositoryLabelsAsync(CancellationToken cancellationToken = default);
    Task RemoveIssueLabelAsync(int issueNumber, string label, CancellationToken cancellationToken = default);
    Task CreateOrUpdateLabelAsync(string label, string color, string description, CancellationToken cancellationToken = default);
}

internal sealed class GitHubIssueClient(IGitHubCli cli) : IGitHubIssueClient
{
    public async Task AddIssueLabelAsync(int issueNumber, string label, CancellationToken cancellationToken = default)
    {
        await cli.RunAsync(["issue", "edit", issueNumber.ToString(), "--add-label", label], cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetIssueLabelsAsync(int issueNumber, CancellationToken cancellationToken = default)
    {
        var result = await cli.RunAsync(["issue", "view", issueNumber.ToString(), "--json", "labels", "--jq", ".labels[].name"], cancellationToken: cancellationToken);
        return LineSplitter.Split(result);
    }

    public async Task<string> GetIssueStateAsync(int issueNumber, CancellationToken cancellationToken = default)
    {
        return (await cli.RunAsync(["issue", "view", issueNumber.ToString(), "--json", "state", "--jq", ".state"], cancellationToken: cancellationToken)).Trim();
    }

    public async Task<IReadOnlySet<string>> GetRepositoryLabelsAsync(CancellationToken cancellationToken = default)
    {
        var result = await cli.RunAsync(["label", "list", "--limit", "200", "--json", "name", "--jq", ".[].name"], cancellationToken: cancellationToken);
        return LineSplitter.Split(result).ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public async Task RemoveIssueLabelAsync(int issueNumber, string label, CancellationToken cancellationToken = default)
    {
        await cli.RunAsync(["issue", "edit", issueNumber.ToString(), "--remove-label", label], allowFailure: true, cancellationToken);
    }

    public async Task CreateOrUpdateLabelAsync(string label, string color, string description, CancellationToken cancellationToken = default)
    {
        await cli.RunAsync(["label", "create", label, "--color", color, "--description", description, "--force"], cancellationToken: cancellationToken);
    }
}
