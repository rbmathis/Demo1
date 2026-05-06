using System.Text.RegularExpressions;
using Demo1.Models;
using Octokit;

namespace Demo1.Services;

/// <summary>
/// Provides read access to GitHub issue activity mapped to pipeline runs.
/// </summary>
public interface IGitHubPipelineService
{
    /// <summary>
    /// Gets recent pipeline runs for the configured repository.
    /// </summary>
    /// <param name="count">Maximum number of runs to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of recent pipeline runs.</returns>
    Task<IReadOnlyList<PipelineRun>> GetRecentRunsAsync(int count = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a mapped pipeline run for a specific issue number.
    /// </summary>
    /// <param name="issueNumber">GitHub issue number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The mapped run when found; otherwise <see langword="null"/>.</returns>
    Task<PipelineRun?> GetRunByIssueNumberAsync(int issueNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a dashboard view model with recent runs and aggregate activity.
    /// </summary>
    /// <param name="count">Maximum number of runs to include.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The populated dashboard model.</returns>
    Task<PipelineObservatoryViewModel> GetDashboardAsync(int count = 20, CancellationToken cancellationToken = default);
}

/// <summary>
/// GitHub-backed implementation of <see cref="IGitHubPipelineService"/>.
/// </summary>
public class GitHubPipelineService : IGitHubPipelineService
{
    private static readonly Regex StageRegex = new(
        @"Pipeline\s*[—-]\s*(Triage|Plan|Implement|Review|Deploy)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly IGitHubClient _gitHubClient;
    private readonly ILogger<GitHubPipelineService> _logger;
    private readonly string _repositoryOwner;
    private readonly string _repositoryName;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubPipelineService"/> class.
    /// </summary>
    /// <param name="gitHubClient">Configured GitHub API client.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="repositoryOwner">Repository owner.</param>
    /// <param name="repositoryName">Repository name.</param>
    public GitHubPipelineService(
        IGitHubClient gitHubClient,
        ILogger<GitHubPipelineService> logger,
        string repositoryOwner,
        string repositoryName)
    {
        _gitHubClient = gitHubClient;
        _logger = logger;
        _repositoryOwner = repositoryOwner;
        _repositoryName = repositoryName;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PipelineRun>> GetRecentRunsAsync(int count = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new RepositoryIssueRequest
            {
                State = ItemStateFilter.All,
            };

            var options = new ApiOptions
            {
                PageSize = Math.Clamp(count, 1, 100),
                PageCount = 1,
                StartPage = 1,
            };

            var issues = await _gitHubClient.Issue.GetAllForRepository(_repositoryOwner, _repositoryName, request, options);
            var runs = new List<PipelineRun>(issues.Count);

            foreach (var issue in issues.OrderByDescending(i => i.CreatedAt))
            {
                cancellationToken.ThrowIfCancellationRequested();
                runs.Add(await BuildRunAsync(issue, cancellationToken));
            }

            return runs;
        }
        catch (ApiException ex)
        {
            _logger.LogWarning(ex, "GitHub API error when fetching recent pipeline runs.");
            return Array.Empty<PipelineRun>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when fetching recent pipeline runs.");
            return Array.Empty<PipelineRun>();
        }
    }

    /// <inheritdoc />
    public async Task<PipelineRun?> GetRunByIssueNumberAsync(int issueNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            var issue = await _gitHubClient.Issue.Get(_repositoryOwner, _repositoryName, issueNumber);
            return await BuildRunAsync(issue, cancellationToken);
        }
        catch (NotFoundException)
        {
            return null;
        }
        catch (ApiException ex)
        {
            _logger.LogWarning(ex, "GitHub API error when fetching issue run {IssueNumber}.", issueNumber);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when fetching issue run {IssueNumber}.", issueNumber);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<PipelineObservatoryViewModel> GetDashboardAsync(int count = 20, CancellationToken cancellationToken = default)
    {
        var runs = (await GetRecentRunsAsync(count, cancellationToken)).ToList();
        var stageCounts = Enum.GetValues<PipelineStage>().ToDictionary(stage => stage, _ => 0);

        foreach (var run in runs)
        {
            stageCounts[run.CurrentStage]++;
        }

        var activity = runs
            .SelectMany(run => run.StageTransitions.Select(transition =>
                new PipelineActivityItem(
                    transition.EnteredAt,
                    transition.AgentName,
                    $"Pipeline entered {transition.Stage}",
                    run.IssueNumber,
                    transition.Stage)))
            .OrderByDescending(item => item.Timestamp)
            .Take(100)
            .ToList();

        return new PipelineObservatoryViewModel
        {
            Runs = runs,
            StageCounts = stageCounts,
            AgentActivity = activity,
        };
    }

    private async Task<PipelineRun> BuildRunAsync(Issue issue, CancellationToken cancellationToken)
    {
        IReadOnlyList<IssueComment> comments = Array.Empty<IssueComment>();
        try
        {
            comments = await _gitHubClient.Issue.Comment.GetAllForIssue(_repositoryOwner, _repositoryName, issue.Number);
        }
        catch (ApiException ex)
        {
            _logger.LogWarning(ex, "Failed to fetch comments for issue {IssueNumber}. Returning partial data.", issue.Number);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var stageEvents = comments
            .Select(comment => new { Comment = comment, Stage = ParseStage(comment.Body) })
            .Where(item => item.Stage.HasValue)
            .Select(item => new StageTransition(item.Stage!.Value, item.Comment.CreatedAt.UtcDateTime, null, item.Comment.User?.Login ?? "unknown"))
            .OrderBy(transition => transition.EnteredAt)
            .ToList();

        if (stageEvents.Count == 0)
        {
            stageEvents.Add(new StageTransition(
                PipelineStage.Triage,
                issue.CreatedAt.UtcDateTime,
                null,
                issue.User?.Login ?? "unknown"));
        }

        var transitions = new List<StageTransition>(stageEvents.Count);
        for (var i = 0; i < stageEvents.Count; i++)
        {
            var nextStart = i < stageEvents.Count - 1 ? stageEvents[i + 1].EnteredAt : (DateTime?)null;
            transitions.Add(stageEvents[i] with { ExitedAt = nextStart });
        }

        var outcome = DetermineOutcome(issue, comments, transitions);
        var completedAt = outcome == PipelineOutcome.InProgress
            ? null
            : issue.ClosedAt?.UtcDateTime ?? comments.OrderByDescending(c => c.CreatedAt).FirstOrDefault()?.CreatedAt.UtcDateTime;

        return new PipelineRun
        {
            Id = issue.Id.ToString(),
            IssueNumber = issue.Number,
            Title = issue.Title,
            CurrentStage = transitions.Last().Stage,
            StartedAt = issue.CreatedAt.UtcDateTime,
            CompletedAt = completedAt,
            Outcome = outcome,
            StageTransitions = transitions,
        };
    }

    private static PipelineOutcome DetermineOutcome(Issue issue, IEnumerable<IssueComment> comments, IReadOnlyList<StageTransition> transitions)
    {
        var combinedText = string.Join('\n', comments.Select(c => c.Body ?? string.Empty));
        var failed = combinedText.Contains("❌", StringComparison.Ordinal) ||
                     combinedText.Contains("failed", StringComparison.OrdinalIgnoreCase);

        if (failed)
        {
            return PipelineOutcome.Failure;
        }

        var hasDeploy = transitions.Any(t => t.Stage == PipelineStage.Deploy);
        var deploySuccess = combinedText.Contains("✅", StringComparison.Ordinal) ||
                            combinedText.Contains("success", StringComparison.OrdinalIgnoreCase);

        if (hasDeploy && deploySuccess)
        {
            return PipelineOutcome.Success;
        }

        var issueState = issue.State.ToString();
        return issueState.Equals("closed", StringComparison.OrdinalIgnoreCase) && hasDeploy
            ? PipelineOutcome.Success
            : PipelineOutcome.InProgress;
    }

    private static PipelineStage? ParseStage(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var match = StageRegex.Match(text);
        if (!match.Success)
        {
            return null;
        }

        return match.Groups[1].Value.ToLowerInvariant() switch
        {
            "triage" => PipelineStage.Triage,
            "plan" => PipelineStage.Plan,
            "implement" => PipelineStage.Implement,
            "review" => PipelineStage.Review,
            "deploy" => PipelineStage.Deploy,
            _ => null,
        };
    }
}
