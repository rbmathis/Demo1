using Demo1.Models;

namespace Demo1.Services;

/// <summary>
/// Provides deterministic prediction logic for pipeline stage classification.
/// </summary>
public interface IPredictionService
{
    /// <summary>
    /// Predicts a likely pipeline stage from issue text.
    /// </summary>
    /// <param name="issueText">Raw issue text.</param>
    /// <returns>A prediction result with stage and confidence.</returns>
    PredictionResult PredictStage(string issueText);
}

/// <summary>
/// Rule-based implementation of <see cref="IPredictionService"/> for demo purposes.
/// </summary>
public class PredictionService : IPredictionService
{
    private readonly ILogger<PredictionService> _logger;

    private static readonly Dictionary<PipelineStage, string[]> Keywords = new()
    {
        [PipelineStage.Triage] = new[] { "bug", "error", "exception", "fail", "incident" },
        [PipelineStage.Plan] = new[] { "plan", "design", "proposal", "approach", "scope" },
        [PipelineStage.Implement] = new[] { "implement", "code", "build", "develop", "fix" },
        [PipelineStage.Review] = new[] { "review", "qa", "test", "verify", "pull request", "pr" },
        [PipelineStage.Deploy] = new[] { "deploy", "release", "production", "rollout", "ship" },
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="PredictionService"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostics.</param>
    public PredictionService(ILogger<PredictionService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public PredictionResult PredictStage(string issueText)
    {
        if (string.IsNullOrWhiteSpace(issueText))
        {
            return new PredictionResult(PipelineStage.Triage, 0, "No issue text provided.");
        }

        var normalized = issueText.ToLowerInvariant();
        var scores = Enum.GetValues<PipelineStage>().ToDictionary(stage => stage, _ => 0);

        foreach (var stage in Enum.GetValues<PipelineStage>())
        {
            foreach (var keyword in Keywords[stage])
            {
                if (normalized.Contains(keyword, StringComparison.Ordinal))
                {
                    scores[stage]++;
                }
            }
        }

        var best = scores
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => pair.Key)
            .First();

        var maxPossible = Keywords[best.Key].Length;
        var confidence = maxPossible == 0 ? 0 : Math.Round(best.Value / (double)maxPossible, 2);
        var result = new PredictionResult(
            best.Key,
            confidence,
            best.Value == 0 ? "No keyword matches found; defaulted by deterministic ordering." : $"Matched {best.Value} keyword(s) for {best.Key}.");

        _logger.LogInformation(
            "Predicted pipeline stage {Stage} with confidence {Confidence}",
            result.PredictedStage,
            result.Confidence);

        return result;
    }
}
