using Autopilot.Copilot;
using Autopilot.GitHub;
using Autopilot.Options;
using Autopilot.Pipeline;

namespace Autopilot;

internal sealed class AutopilotApp(TextWriter output, TextWriter error)
{
    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = AutopilotOptions.Parse(args);
            if (options.ShowHelp)
            {
                output.WriteLine(AutopilotOptions.HelpText);
                return 0;
            }

            var cli = new GitHubCli(options.RepoRoot, options.Repository);
            var issueClient = new GitHubIssueClient(cli);
            var labels = new SdkLabelService(issueClient, output);
            var promptBuilder = new PromptBuilder(options.RepoRoot, options.IssueNumber);
            var stageRunner = new CopilotStageRunner(options.RepoRoot, options.Model, output, error);
            var modelChecker = new CopilotModelAvailabilityChecker();
            var runner = new SdkAutopilotRunner(options, issueClient, labels, promptBuilder, stageRunner, modelChecker, output);
            return await runner.RunAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            error.WriteLine($"SDK autopilot failed: {ex.Message}");
            return 1;
        }
    }
}
