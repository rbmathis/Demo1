# Demo1 Autopilot

This project is a programmatic Copilot SDK version of the local AI-SDLC autopilot described in `../AI-SDLC-LOCAL.md`.

The executable project is `Autopilot`. It duplicates the local autopilot controller behavior in code, with SDK-specific safety gates:

- runs `triage -> feature-flags -> plan -> implement -> review -> docs -> deliver`
- owns `sdk/*` stage label transitions deterministically through `gh`
- applies the plain `sdk` provenance label when work starts and never removes it
- consults the `feature-flags` specialist after triage only when rollout status is `rollout-required` or `rollout-optional`
- reads the existing stage prompts from `../.github/agents/*.agent.md`
- uses the GitHub issue thread as the pipeline state file
- supports the review rework loop with a maximum of two review cycles
- checks Copilot model availability before applying issue labels or running stages
- requires explicit `--approve-all` before granting Copilot SDK tool permissions
- exits before label changes or stage work when the target issue is already closed
- supports `--skip-deliver` for pilot runs that stop before merge

## Prerequisites

- .NET 10 SDK
- GitHub CLI authenticated with access to `rbmathis/Demo1`
- GitHub Copilot access for the Copilot SDK runtime
- Push access to create branches and PRs when running the implement stage

## Run

From this directory:

```powershell
dotnet run -- run issue 135 --repo rbmathis/Demo1 --approve-all --skip-deliver
```

The default SDK model is `claude-sonnet-4.6`. Use `--model` when you need a different Copilot model available to your account.

Useful options:

```powershell
dotnet run -- --check-labels --repo rbmathis/Demo1
dotnet run -- --check-labels --ensure-labels --repo rbmathis/Demo1
dotnet run -- --check-model --repo rbmathis/Demo1
dotnet run -- --check-model --model claude-sonnet-4.6 --repo rbmathis/Demo1
dotnet run -- run issue 135 --repo rbmathis/Demo1 --approve-all --skip-deliver
dotnet run -- run issue 135 --repo rbmathis/Demo1 --approve-all --allow-missing-docs
dotnet run -- run issue 135 --repo rbmathis/Demo1 --approve-all --model claude-sonnet-4.6
dotnet run -- run issue 135 --repo rbmathis/Demo1 --approve-all --stage-timeout-minutes 20
dotnet run -- run issue 135 --repo-root C:\Users\rdpuser\Source\Demo1
```

Each Copilot stage waits up to 10 minutes by default. Use `--stage-timeout-minutes` for longer triage, implementation, review, or documentation runs.

## Required Labels

The runner fails before touching an issue unless every SDK label exists. Create or verify them explicitly:

```powershell
dotnet run -- --check-labels --repo rbmathis/Demo1
dotnet run -- --check-labels --ensure-labels --repo rbmathis/Demo1
```

Required labels:

- `sdk`
- `sdk/triage`
- `sdk/feature-flags`
- `sdk/planning`
- `sdk/implementing`
- `sdk/review`
- `sdk/docs`
- `sdk/delivering`
- `sdk/done`
- `sdk/failed`

## How It Works

The console app uses the Copilot SDK to create a fresh Copilot session for each pipeline stage. Before each session starts, the app updates issue labels itself:

When a run targets a closed issue, the runner prints a no-op message and exits before it creates, removes, or adds issue labels.

Before the first mutable issue operation, the runner checks that the selected Copilot model is available. Use `--check-model` to run that preflight independently.

The console output groups preflights and stages into bordered sections, prints `[step]`, `[ ok ]`, `[warn]`, and `[fail]` status lines, and shows the timeout applied to each stage.

The plain `sdk` label is permanent provenance. The runner applies it once when work starts and never removes it. Stage transitions remove only labels with the `sdk/` prefix, then add the current stage label. Stage agents are instructed not to manage `sdk` or `sdk/*` labels.

| Stage | Label |
| ----- | ----- |
| Triage | `sdk/triage` |
| Feature flags | `sdk/feature-flags` when triage says `rollout-required` or `rollout-optional` |
| Plan | `sdk/planning` |
| Implement | `sdk/implementing` |
| Review | `sdk/review` |
| Docs | `sdk/docs` |
| Deliver | `sdk/delivering` |
| Complete | `sdk/done` |
| Failed | `sdk/failed` |

Each stage prompt is loaded from the existing local agent file, then wrapped with SDK controller instructions that require a final fenced JSON result block. The runner parses the last JSON block only and fails closed when the block is missing, malformed, or contains an unknown `status` or `decision`.

Docs are blocking by default because rollout-sensitive work may need activation and rollback instructions. Use `--allow-missing-docs` only when deliberately accepting that risk.

## Project Layout

- `Program.cs` wires and starts the app.
- `AutopilotApp.cs` is the composition root.
- `Options/` handles CLI options and repository-root discovery.
- `GitHub/` wraps `gh` commands and owns SDK label behavior.
- `Copilot/` wraps Copilot SDK stage execution and model availability checks.
- `Pipeline/` owns stage definitions, prompt building, result parsing, and orchestration.
- `../tests/CopilotSdk.Tests/` covers option parsing, label parsing, stage result parsing, label transitions, closed-issue no-op behavior, and model preflight behavior.

## Notes

- This is an SDK experiment, not a replacement for the VS Code local agents yet.
- The SDK package is in public preview, so APIs may move.
- The SDK runs with `PermissionHandler.ApproveAll` only when `--approve-all` is supplied; use this only in trusted local development environments.
- The generated project is excluded from the main MVC app's compile glob in `../Demo1.csproj`.
