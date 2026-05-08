using Autopilot.Copilot;
using Autopilot.GitHub;
using Autopilot.Options;

namespace Autopilot.Pipeline;

internal sealed class SdkAutopilotRunner(
    AutopilotOptions options,
    IGitHubIssueClient issueClient,
    ISdkLabelService labels,
    IPromptBuilder promptBuilder,
    IStageRunner stageRunner,
    IModelAvailabilityChecker modelChecker,
    TextWriter output)
{
    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        if (options.CheckLabelsOnly)
        {
            WriteHeader("Label Preflight");
            WriteStep("Checking SDK labels");
            await labels.EnsureRequiredLabelsAsync(options.EnsureLabels, cancellationToken);
            WriteSuccess("SDK labels are ready. No stages were run.");
            return 0;
        }

        if (options.CheckModelOnly)
        {
            return await CheckModelAsync(cancellationToken);
        }

        WriteHeader($"Autopilot issue #{options.IssueNumber}");
        WriteDetail("Repository", options.RepoRoot);
        WriteDetail("Model", options.Model);
        WriteDetail("Stage timeout", FormatDuration(options.StageTimeout));

        var issueState = await issueClient.GetIssueStateAsync(options.IssueNumber, cancellationToken);
        if (issueState.Equals("CLOSED", StringComparison.OrdinalIgnoreCase))
        {
            WriteWarning($"Issue #{options.IssueNumber} is already closed. Autopilot will not run or modify labels.");
            return 0;
        }

        await labels.EnsureRequiredLabelsAsync(options.EnsureLabels, cancellationToken);

        if (!options.ApproveAll)
        {
            WriteWarning("Autopilot requires --approve-all before running Copilot tool requests.");
            WriteDetail("Pilot tip", "Run with --skip-deliver first, and only use --approve-all in a trusted repository and issue context.");
            return 10;
        }

        var modelCheckExitCode = await CheckModelAsync(cancellationToken);
        if (modelCheckExitCode != 0)
        {
            return modelCheckExitCode;
        }

        await labels.EnsureProvenanceAsync(options.IssueNumber, cancellationToken);

        return await RunPipelineAsync(cancellationToken);
    }

    private async Task<int> CheckModelAsync(CancellationToken cancellationToken)
    {
        WriteHeader("Model Preflight");
        WriteStep($"Checking Copilot model availability: {options.Model}");
        var result = await modelChecker.CheckAsync(options.Model, options.RepoRoot, cancellationToken);
        if (result.IsAvailable)
        {
            WriteSuccess($"Copilot model is available: {options.Model}");
            return 0;
        }

        WriteFailure($"Copilot model is not available: {options.Model}");
        WriteDetail("SDK error", result.Error ?? "No details returned.");
        WriteDetail("Next step", "Retry with --model <available-model-id>.");
        return 11;
    }

    private async Task<int> RunPipelineAsync(CancellationToken cancellationToken)
    {
        await labels.SetStageAsync(options.IssueNumber, StageCatalog.Triage.Label, cancellationToken);
        var triage = await RunStageAsync(StageCatalog.Triage, "Classify the issue and publish the mandatory triage handoff comment.", cancellationToken);
        if (triage.Status.Equals("STOP", StringComparison.OrdinalIgnoreCase))
        {
            WriteWarning("Triage returned STOP. Holding for human intervention.");
            return 2;
        }

        if (!triage.Status.Equals("GO", StringComparison.OrdinalIgnoreCase) && !triage.Status.Equals("DUPLICATE", StringComparison.OrdinalIgnoreCase))
        {
            return await HaltAsync($"Triage returned unexpected status '{triage.Status}'.", cancellationToken);
        }

        if (triage.Status.Equals("DUPLICATE", StringComparison.OrdinalIgnoreCase))
        {
            WriteSuccess("Duplicate confirmed. Marking SDK pipeline complete without implementation.");
            await labels.SetStageAsync(options.IssueNumber, "sdk/done", cancellationToken);
            return 0;
        }

        if (!triage.RolloutStatus.Equals("rollout-exempt", StringComparison.OrdinalIgnoreCase))
        {
            if (!triage.RolloutStatus.Equals("rollout-required", StringComparison.OrdinalIgnoreCase) && !triage.RolloutStatus.Equals("rollout-optional", StringComparison.OrdinalIgnoreCase))
            {
                return await HaltAsync("Triage did not provide a valid rollout status. Expected rollout-required, rollout-optional, or rollout-exempt.", cancellationToken);
            }

            await labels.SetStageAsync(options.IssueNumber, StageCatalog.FeatureFlags.Label, cancellationToken);
            var rollout = await RunStageAsync(StageCatalog.FeatureFlags, "Read the issue and triage handoff, classify rollout status, and post a feature-flag rollout consultation comment. If rollout is required or optional, include the complete rollout checklist downstream agents must follow.", cancellationToken);
            if (!rollout.Status.Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                return await HaltAsync($"Feature-flag consultation returned '{rollout.Status}'.", cancellationToken);
            }
        }
        else
        {
            WriteStep("Feature-flag consultation skipped: triage marked rollout-exempt.");
        }

        await labels.SetStageAsync(options.IssueNumber, StageCatalog.Plan.Label, cancellationToken);
        var plan = await RunStageAsync(StageCatalog.Plan, "Create the implementation plan, issue comments, and feature branch exactly as the plan agent defines. Read and preserve the feature-flags consultation when one exists; if rollout is optional, record the final flagging verdict and rationale.", cancellationToken);
        if (!plan.Status.Equals("GO", StringComparison.OrdinalIgnoreCase))
        {
            return await HaltAsync($"Plan returned '{plan.Status}'.", cancellationToken);
        }

        await labels.SetStageAsync(options.IssueNumber, StageCatalog.Implement.Label, cancellationToken);
        var implement = await RunStageAsync(StageCatalog.Implement, "Execute the plan and the feature-flag rollout contract, validate both flag-off and flag-on behavior when applicable, commit, push, create the PR, and post the build-complete issue comment.", cancellationToken);
        if (!implement.Status.Equals("GO", StringComparison.OrdinalIgnoreCase))
        {
            return await HaltAsync($"Implement returned '{implement.Status}'.", cancellationToken);
        }

        var review = await RunReviewLoopAsync(cancellationToken);
        if (!review.Status.Equals("GO", StringComparison.OrdinalIgnoreCase))
        {
            return await HaltAsync($"Review returned '{review.Status}'.", cancellationToken);
        }

        if (!review.Decision.Equals("approved", StringComparison.OrdinalIgnoreCase))
        {
            WriteWarning("Review did not return an approval. Halting before docs/deliver.");
            return 4;
        }

        await labels.SetStageAsync(options.IssueNumber, StageCatalog.Docs.Label, cancellationToken);
        var docs = await RunStageAsync(StageCatalog.Docs, "Update XML/markdown documentation and post the human verification walkthrough. Include flag-off and flag-on verification plus activation and rollback instructions when the rollout consultation requires or recommends a flag. Continue even if there are no docs changes.", cancellationToken);
        if (!docs.Status.Equals("GO", StringComparison.OrdinalIgnoreCase))
        {
            if (!options.AllowMissingDocs)
            {
                return await HaltAsync($"Docs returned '{docs.Status}'. Rerun with --allow-missing-docs to continue anyway.", cancellationToken);
            }

            WriteWarning($"Docs returned '{docs.Status}', but --allow-missing-docs is set. Continuing.");
        }

        if (options.SkipDeliver)
        {
            WriteSuccess("--skip-deliver set. Stopping before merge.");
            return 0;
        }

        await labels.SetStageAsync(options.IssueNumber, StageCatalog.Deliver.Label, cancellationToken);
        var deliver = await RunStageAsync(StageCatalog.Deliver, "Merge the approved PR, delete the feature branch, and post the landing report. Do not close the issue.", cancellationToken);
        if (!deliver.Status.Equals("GO", StringComparison.OrdinalIgnoreCase))
        {
            return await HaltAsync($"Deliver returned '{deliver.Status}'.", cancellationToken);
        }

        await labels.SetStageAsync(options.IssueNumber, "sdk/done", cancellationToken);
        WriteSuccess("Landing confirmed. Airspace clear.");
        return 0;
    }

    private async Task<StageResult> RunReviewLoopAsync(CancellationToken cancellationToken)
    {
        StageResult review = StageResult.Empty;
        for (var cycle = 1; cycle <= 2; cycle++)
        {
            await labels.SetStageAsync(options.IssueNumber, StageCatalog.Review.Label, cancellationToken);
            review = await RunStageAsync(StageCatalog.Review, $"Review the linked PR for issue #{options.IssueNumber}. Validate the feature-flag rollout contract, dual-path tests, activation safety, and cleanup reference when applicable. This is review cycle {cycle} of 2.", cancellationToken);

            if (!review.Status.Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                return new StageResult("STOP", review.Decision, review.RolloutStatus, review.IsValid, review.Error);
            }

            if (!review.Decision.Equals("changes_requested", StringComparison.OrdinalIgnoreCase))
            {
                return review;
            }

            if (cycle == 2)
            {
                WriteWarning("Review requested changes twice. Halting for human intervention.");
                return review;
            }

            WriteStep("Review requested changes. Handing back to implementation for a go-around.");
            await labels.SetStageAsync(options.IssueNumber, StageCatalog.Implement.Label, cancellationToken);
            var rework = await RunStageAsync(StageCatalog.Implement, "Address the latest review findings, including any feature-flag rollout gaps, push fixes to the existing PR branch, and update the issue.", cancellationToken);
            if (!rework.Status.Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                return new StageResult("STOP", rework.Decision, rework.RolloutStatus, rework.IsValid, rework.Error);
            }
        }

        return review;
    }

    private async Task<int> HaltAsync(string reason, CancellationToken cancellationToken)
    {
        WriteFailure($"Pipeline halted: {reason}");
        await labels.SetStageAsync(options.IssueNumber, "sdk/failed", cancellationToken);
        return 20;
    }

    private async Task<StageResult> RunStageAsync(StageDefinition stage, string mission, CancellationToken cancellationToken)
    {
        WriteHeader($"Stage: {stage.DisplayName}");
        WriteDetail("Issue", $"#{options.IssueNumber}");
        WriteDetail("Label", stage.Label);
        WriteDetail("Timeout", FormatDuration(options.StageTimeout));
        var prompt = await promptBuilder.BuildAsync(stage, mission, cancellationToken);
        var result = await stageRunner.RunAsync(stage, prompt, options.StageTimeout, cancellationToken);
        if (!result.IsValid)
        {
            WriteFailure($"Stage {stage.DisplayName} returned invalid JSON result: {result.Error}");
            return result;
        }

        WriteSuccess($"Stage {stage.DisplayName} complete");
        WriteDetail("Status", result.Status);
        WriteDetail("Decision", result.Decision);
        WriteDetail("Rollout", result.RolloutStatus);
        return result;
    }

    private void WriteHeader(string title)
    {
        output.WriteLine();
        output.WriteLine("============================================================");
        output.WriteLine(title);
        output.WriteLine("============================================================");
    }

    private void WriteStep(string message)
    {
        output.WriteLine($"[step] {message}");
    }

    private void WriteSuccess(string message)
    {
        output.WriteLine($"[ ok ] {message}");
    }

    private void WriteWarning(string message)
    {
        output.WriteLine($"[warn] {message}");
    }

    private void WriteFailure(string message)
    {
        output.WriteLine($"[fail] {message}");
    }

    private void WriteDetail(string name, string value)
    {
        output.WriteLine($"  {name,-14}: {value}");
    }

    private static string FormatDuration(TimeSpan duration)
    {
        return duration.TotalMinutes >= 1
            ? $"{duration.TotalMinutes:0.##} min"
            : $"{duration.TotalSeconds:0.##} sec";
    }
}
