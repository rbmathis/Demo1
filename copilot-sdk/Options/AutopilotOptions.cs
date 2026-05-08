namespace Autopilot.Options;

internal sealed record AutopilotOptions(
    int IssueNumber,
    string RepoRoot,
    string? Repository,
    string Model,
    bool SkipDeliver,
    bool EnsureLabels,
    bool CheckLabelsOnly,
    bool CheckModelOnly,
    TimeSpan StageTimeout,
    bool ApproveAll,
    bool AllowMissingDocs,
    bool ShowHelp)
{
    public const string DefaultModel = "claude-sonnet-4.6";
    public static readonly TimeSpan DefaultStageTimeout = TimeSpan.FromMinutes(10);

    public static string HelpText => string.Join(Environment.NewLine,
            "Demo1 Autopilot",
            string.Empty,
            "Usage:",
            "  dotnet run -- run issue <number> [--repo owner/name] [--model model-id] [--skip-deliver]",
            "  dotnet run -- issue <number> [--repo owner/name]",
            "  dotnet run -- <number>",
            "  dotnet run -- --check-labels [--repo owner/name]",
            "  dotnet run -- --check-model [--model model-id]",
            string.Empty,
            "Options:",
            "  --repo-root <path>   Repository root. Defaults to the nearest parent containing .github/agents.",
            "  --repo <owner/name>  GitHub repository for gh issue operations. Defaults to gh repo view.",
            "  --model <model-id>   Copilot model. Defaults to claude-sonnet-4.6.",
            "  --stage-timeout-minutes <minutes>",
            "                       Wait time for each Copilot stage. Defaults to 10.",
            "  --ensure-labels     Create missing sdk labels before running.",
            "  --check-labels      Check sdk labels and exit without running stages.",
            "  --check-model       Check Copilot model availability and exit without running stages.",
            "  --approve-all       Allow Copilot SDK to approve all tool permission requests.",
            "  --allow-missing-docs Continue to deliver if the docs stage fails.",
            "  --skip-deliver      Run through docs but stop before merge/deliver.");

    public static AutopilotOptions Parse(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help", StringComparer.OrdinalIgnoreCase) || args.Contains("-h", StringComparer.OrdinalIgnoreCase))
        {
            return new AutopilotOptions(0, string.Empty, null, DefaultModel, false, false, false, false, DefaultStageTimeout, false, false, true);
        }

        int? issueNumber = null;
        string? repoRoot = null;
        string? repository = null;
        var model = DefaultModel;
        var skipDeliver = false;
        var ensureLabels = false;
        var checkLabelsOnly = false;
        var checkModelOnly = false;
        var stageTimeout = DefaultStageTimeout;
        var approveAll = false;
        var allowMissingDocs = false;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            switch (arg)
            {
                case "run":
                case "issue":
                case "autopilot":
                    continue;
                case "--repo-root":
                    repoRoot = RequireValue(args, ref index, arg);
                    break;
                case "--repo":
                    repository = RequireValue(args, ref index, arg);
                    break;
                case "--model":
                    model = RequireValue(args, ref index, arg);
                    break;
                case "--stage-timeout-minutes":
                    stageTimeout = ParsePositiveMinutes(RequireValue(args, ref index, arg), arg);
                    break;
                case "--skip-deliver":
                    skipDeliver = true;
                    break;
                case "--ensure-labels":
                    ensureLabels = true;
                    break;
                case "--check-labels":
                    checkLabelsOnly = true;
                    break;
                case "--check-model":
                    checkModelOnly = true;
                    break;
                case "--approve-all":
                    approveAll = true;
                    break;
                case "--allow-missing-docs":
                    allowMissingDocs = true;
                    break;
                default:
                    if (int.TryParse(arg, out var parsed))
                    {
                        issueNumber = parsed;
                    }
                    break;
            }
        }

        if ((issueNumber is null or <= 0) && !checkLabelsOnly && !checkModelOnly)
        {
            throw new ArgumentException("Provide a positive issue number. Try: dotnet run -- run issue 135");
        }

        return new AutopilotOptions(issueNumber ?? 0, RepoRootFinder.Find(repoRoot), repository, model, skipDeliver, ensureLabels, checkLabelsOnly, checkModelOnly, stageTimeout, approveAll, allowMissingDocs, false);
    }

    private static TimeSpan ParsePositiveMinutes(string value, string optionName)
    {
        if (!double.TryParse(value, out var minutes) || minutes <= 0)
        {
            throw new ArgumentException($"{optionName} requires a positive number of minutes.");
        }

        return TimeSpan.FromMinutes(minutes);
    }

    private static string RequireValue(string[] args, ref int index, string optionName)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"{optionName} requires a value.");
        }

        index++;
        return args[index];
    }
}
