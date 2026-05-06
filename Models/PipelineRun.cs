namespace Demo1.Models;

/// <summary>
/// Represents the known lifecycle stages for an AI pipeline run.
/// </summary>
public enum PipelineStage
{
    /// <summary>
    /// Initial triage stage.
    /// </summary>
    Triage,
    /// <summary>
    /// Planning stage.
    /// </summary>
    Plan,
    /// <summary>
    /// Implementation stage.
    /// </summary>
    Implement,
    /// <summary>
    /// Review stage.
    /// </summary>
    Review,
    /// <summary>
    /// Deployment stage.
    /// </summary>
    Deploy,
}

/// <summary>
/// Represents the final or current outcome of a pipeline run.
/// </summary>
public enum PipelineOutcome
{
    /// <summary>
    /// The run completed successfully.
    /// </summary>
    Success,
    /// <summary>
    /// The run failed.
    /// </summary>
    Failure,
    /// <summary>
    /// The run is still active.
    /// </summary>
    InProgress,
}

/// <summary>
/// Captures a transition into a pipeline stage.
/// </summary>
/// <param name="Stage">The stage entered.</param>
/// <param name="EnteredAt">The UTC timestamp when the stage was entered.</param>
/// <param name="ExitedAt">The UTC timestamp when the stage was exited, if known.</param>
/// <param name="AgentName">The agent or user associated with the transition.</param>
public sealed record StageTransition(
    PipelineStage Stage,
    DateTime EnteredAt,
    DateTime? ExitedAt,
    string AgentName);

/// <summary>
/// Represents a single pipeline run mapped from a repository issue.
/// </summary>
public sealed record PipelineRun
{
    /// <summary>
    /// Gets or sets the run identifier.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the issue number associated with the run.
    /// </summary>
    public int IssueNumber { get; init; }

    /// <summary>
    /// Gets or sets the issue title.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the current stage.
    /// </summary>
    public PipelineStage CurrentStage { get; init; } = PipelineStage.Triage;

    /// <summary>
    /// Gets or sets when the run started.
    /// </summary>
    public DateTime StartedAt { get; init; }

    /// <summary>
    /// Gets or sets when the run completed, if completed.
    /// </summary>
    public DateTime? CompletedAt { get; init; }

    /// <summary>
    /// Gets or sets the run outcome.
    /// </summary>
    public PipelineOutcome Outcome { get; init; } = PipelineOutcome.InProgress;

    /// <summary>
    /// Gets or sets the known stage transitions.
    /// </summary>
    public IReadOnlyList<StageTransition> StageTransitions { get; init; } = Array.Empty<StageTransition>();
}
