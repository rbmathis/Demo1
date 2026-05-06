using System.ComponentModel.DataAnnotations;

namespace Demo1.Models;

/// <summary>
/// View model for the AI Pipeline Observatory dashboard.
/// </summary>
public class PipelineObservatoryViewModel
{
    /// <summary>
    /// Gets or sets the current pipeline runs.
    /// </summary>
    public List<PipelineRun> Runs { get; init; } = new();

    /// <summary>
    /// Gets or sets counts of runs by current stage.
    /// </summary>
    public Dictionary<PipelineStage, int> StageCounts { get; init; } = new();

    /// <summary>
    /// Gets or sets the recent activity stream by agent.
    /// </summary>
    public List<PipelineActivityItem> AgentActivity { get; init; } = new();

    /// <summary>
    /// Gets or sets a user-friendly error message, if available.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Represents a single activity entry for the observatory timeline.
/// </summary>
/// <param name="Timestamp">The UTC event timestamp.</param>
/// <param name="AgentName">The actor associated with the event.</param>
/// <param name="Message">The activity message.</param>
/// <param name="IssueNumber">The related issue number.</param>
/// <param name="Stage">The associated pipeline stage.</param>
public sealed record PipelineActivityItem(
    DateTime Timestamp,
    string AgentName,
    string Message,
    int IssueNumber,
    PipelineStage Stage);

/// <summary>
/// Request model for pipeline stage prediction.
/// </summary>
public class PredictionRequest
{
    /// <summary>
    /// Gets or sets free-form issue text used for prediction.
    /// </summary>
    [Required]
    [StringLength(8000, MinimumLength = 5)]
    public string IssueText { get; set; } = string.Empty;
}

/// <summary>
/// Result model returned for a stage prediction.
/// </summary>
/// <param name="PredictedStage">The predicted stage.</param>
/// <param name="Confidence">The confidence score from 0 to 1.</param>
/// <param name="Rationale">Human-readable explanation of the prediction.</param>
public sealed record PredictionResult(
    PipelineStage PredictedStage,
    double Confidence,
    string Rationale);
