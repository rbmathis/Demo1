namespace Autopilot.Pipeline;

internal interface IPromptBuilder
{
    Task<string> BuildAsync(StageDefinition stage, string mission, CancellationToken cancellationToken = default);
}

internal sealed class PromptBuilder(string repoRoot, int issueNumber) : IPromptBuilder
{
    public async Task<string> BuildAsync(StageDefinition stage, string mission, CancellationToken cancellationToken = default)
    {
        var promptPath = Path.Combine(repoRoot, ".github", "agents", stage.PromptFile);
        var stagePrompt = await File.ReadAllTextAsync(promptPath, cancellationToken);

        return $$"""
			You are running as the Demo1 SDK autopilot controller.

			Target issue: #{{issueNumber}}
			Repository root: {{repoRoot}}
			Stage: {{stage.Name}}
			Mission: {{mission}}

			The controller has already applied the permanent `sdk` provenance label and the correct SDK stage label for this stage.
			Do not manage the `sdk` label or any `sdk/*` labels yourself. Do not close the issue.

			Execute the stage instructions below yourself. Use the issue thread as the state file.
			Do not delegate to background agents or wait for specialist agent tasks. When the imported prompt asks for specialist input, perform that specialist analysis directly in this SDK session and include the result in the stage summary.

			At the very end of your response, include a fenced JSON block with the best available stage result:

			```json
			{
			  "status": "GO",
			  "decision": "approved",
			  "issue_number": {{issueNumber}}
			}
			```

			Use these status values when applicable: GO, STOP, DUPLICATE.
			Include `rollout_status` when the stage determines or uses rollout status.
			Use these review decision values when applicable: approved, changes_requested, comment.

			<stage-agent-prompt>
			{{stagePrompt}}
			</stage-agent-prompt>
			""";
    }
}
