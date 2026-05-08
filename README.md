# Autopilot, 3 Ways

[![Build & Test](https://github.com/rbmathis/Demo1/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/rbmathis/Demo1/actions/workflows/dotnet.yml)
[![Deploy](https://github.com/rbmathis/Demo1/actions/workflows/deploy.yml/badge.svg?branch=main)](https://github.com/rbmathis/Demo1/actions/workflows/deploy.yml)
[![Playwright Ready](https://img.shields.io/badge/Playwright-smoke%20tested-3fb950?logo=playwright&logoColor=white)](tests/Demo1.PlaywrightTests)

Demo1 is an ASP.NET Core MVC app that doubles as a live lab for AI-driven software delivery. The interesting part is not just the website; it is the delivery system around it. This repo shows the same AI-SDLC pipeline running three different ways: local agents, cloud workflows, and a programmatic Copilot SDK runner.

The theme is simple: create an issue, let Autopilot classify it, plan it, implement it, review it, document it, and deliver it with enough personality that the PR does not feel like it escaped from a spreadsheet.

## The Three Autopilots

| Mode | Entry Point | Best For | State Labels | Guide |
|------|-------------|----------|--------------|-------|
| **Local** | VS Code Copilot Chat or Copilot CLI | Hands-on agent orchestration from the editor | `local`, `local/*` | [AI-SDLC-LOCAL.md](AI-SDLC-LOCAL.md) |
| **Cloud** | GitHub Agentic Workflows | GitHub-native automation from issue labels and workflow dispatch | `autopilot`, `cloud/*` | [AI-SDLC-CLOUD.md](AI-SDLC-CLOUD.md) |
| **SDK** | .NET console app in `copilot-sdk/` | Programmatic, testable orchestration with Copilot SDK sessions | `sdk`, `sdk/*` | [AI-SDLC-SDK.md](AI-SDLC-SDK.md) |

All three modes share the same delivery philosophy:

- The GitHub issue is the state file.
- Triage classifies work and decides rollout sensitivity.
- Planning creates the branch, task list, acceptance criteria, and rollout contract.
- Implementation delegates to specialist agents and creates a PR.
- Review checks quality, security, tests, docs, and rollout safety.
- Documentation gives humans a verification path.
- Delivery merges only after the pipeline has enough signal to trust the change.

## What Each Mode Proves

### Local: Agents In The Editor

Local mode is the most conversational path. The `@autopilot` custom agent runs inside VS Code Copilot Chat or Copilot CLI and delegates to local stage agents under [.github/agents/](.github/agents). It owns `local/*` stage labels, keeps the issue thread current, loops through review when needed, and uses specialist agents for backend, frontend, security, testing, docs, build validation, feature flags, and code review.

Start here when you want to watch the agents work, intervene quickly, or run a full AI-SDLC loop without pushing orchestration into GitHub Actions.

```text
@autopilot run issue 135
```

### Cloud: Agentic Workflows In GitHub

Cloud mode runs through [GitHub Agentic Workflows](https://github.github.com/gh-aw/). Applying `cloud/autopilot` to an issue starts the workflow chain. The cloud pipeline validates the issue, dispatches triage, creates a plan, assigns the Copilot coding agent, waits for a human to apply `cloud/review`, then continues through review, docs, and finish.

Use this path when you want GitHub to own the orchestration and leave an auditable trail in Actions and issue comments.

```text
Apply label: cloud/autopilot
```

Cloud workflow source lives in [.github/workflows/cloud-*.md](.github/workflows). These files compile to `.lock.yml` files with `gh aw compile`. When editing them, delete the existing lockfiles first, then compile.

### SDK: Autopilot As Code

SDK mode is a .NET console app that wraps the same local agent prompts with Copilot SDK sessions. It gives us a maintainable C# orchestration surface with unit tests, model preflight checks, required label checks, closed-issue guards, explicit permission gating, configurable per-stage timeouts, and structured JSON stage results.

Use this path when you want automation that can be tested and refactored like application code.

```powershell
dotnet run --project .\copilot-sdk\Autopilot.csproj -- issue 135 --repo rbmathis/Demo1 --approve-all --skip-deliver
```

Useful SDK checks:

```powershell
dotnet run --project .\copilot-sdk\Autopilot.csproj -- --check-labels --repo rbmathis/Demo1
dotnet run --project .\copilot-sdk\Autopilot.csproj -- --check-model --repo rbmathis/Demo1
dotnet test .\tests\CopilotSdk.Tests\Autopilot.Tests.csproj
```

## Shared Pipeline Shape

```text
Issue
  |
  v
Autopilot
  |
  v
Triage -> Feature-flag consultation when needed -> Plan
  |
  v
Implement -> Pull Request
  |
  v
Review -> Docs -> Deliver
```

The exact transport changes by mode, but the contract stays recognizable:

| Stage | Local | Cloud | SDK |
|-------|-------|-------|-----|
| Autopilot | Controller agent | `cloud-autopilot.md` | `SdkAutopilotRunner` |
| Triage | `triage.agent.md` | `cloud-triage.md` | SDK session over `triage.agent.md` |
| Plan | `plan.agent.md` | `cloud-plan.md` | SDK session over `plan.agent.md` |
| Implement | `implement.agent.md` | Copilot coding agent assignment | SDK session over `implement.agent.md` |
| Review | `review.agent.md` | `cloud-review.md` | SDK session over `review.agent.md` |
| Docs | `docs.agent.md` | `cloud-docs.md` | SDK session over `docs.agent.md` |
| Deliver | `deliver.agent.md` | `cloud-finish.yml` | SDK session over `deliver.agent.md` |

## Rollout Safety

The AI-SDLC pipeline ships risky work dark by default. Triage classifies every issue as `rollout-required`, `rollout-optional`, or `rollout-exempt`. Planning records the flagging verdict. Implementation preserves the default-off path. Review blocks unsafe rollout gaps. Docs emits the human verification and activation path.

Start with these docs:

- [docs/feature-flag-rollout-contract.md](docs/feature-flag-rollout-contract.md) for the rollout policy and checklist contract
- [docs/feature-flag-runtime-guide.md](docs/feature-flag-runtime-guide.md) for implementation patterns, test seams, and migration conventions

Cloud activation is human-controlled through Azure App Configuration. Local activation is human-controlled through local configuration. Autopilot does not flip flags on your behalf.

## PRs With A Pulse

The pipelines use the `snarky-commit` style for commits and PRs. That means PR titles should describe the real change with a little swagger, PR bodies should still be useful to reviewers, and quality checks should be visible without sounding like a tax form.

The personality lives in:

- [.github/skills/snarky-commit/SKILL.md](.github/skills/snarky-commit/SKILL.md)
- [.github/agents/implement.agent.md](.github/agents/implement.agent.md)
- [.github/workflows/cloud-implement.md](.github/workflows/cloud-implement.md)

Cloud PRs still preserve the required `Closes #N` line and `automated` label. Local PRs use `Refs #N` because the local controller owns issue closure. SDK PRs inherit the local implement-agent behavior.

## The Demo App

The app is intentionally feature-rich enough to exercise real delivery paths:

- MVC controllers, Razor views, services, middleware, EF Core, and telemetry
- Achievement system using `Channel<T>` and `BackgroundService`
- Component showcase with isolated previews and copyable markup
- Security headers playground
- Rate limiting with response headers
- Feature flags through `Microsoft.FeatureManagement`
- Unit, integration, and Playwright smoke tests

Key app docs:

- [architecture.md](architecture.md)
- [docs/README.md](docs/README.md)
- [docs/configuration.md](docs/configuration.md)
- [docs/testing.md](docs/testing.md)
- [docs/ci-cd.md](docs/ci-cd.md)
- [docs/component-showcase.md](docs/component-showcase.md)
- [docs/security-lab.md](docs/security-lab.md)

## Running The App

Prerequisites:

- .NET 10 SDK or later
- Visual Studio Code with C# Dev Kit for the editor experience
- GitHub CLI for local and SDK pipeline operations

Build and run:

```bash
dotnet build
dotnet run
dotnet watch run
```

Docker:

```bash
docker build -t demo1 .
docker run -p 5555:8080 demo1
docker compose up
docker compose --profile dev up
```

`docker compose up` starts the app and Redis stack. `docker compose --profile dev up` starts the hot-reload profile on `http://localhost:5556`.

## Testing

```bash
dotnet test
dotnet test tests/Demo1.UnitTests
dotnet test tests/Demo1.IntegrationTests
dotnet test tests/CopilotSdk.Tests/Autopilot.Tests.csproj
```

The solution includes unit tests, integration tests, Playwright smoke tests, and focused Autopilot SDK tests. See [docs/testing.md](docs/testing.md) for conventions.

## Repository Map

| Path | Purpose |
|------|---------|
| [.github/agents/](.github/agents) | Local custom agents shared by local and SDK modes |
| [.github/skills/](.github/skills) | Reusable skills like snarky commit and gh-aw compile |
| [.github/workflows/](.github/workflows) | GitHub Actions and gh-aw workflow sources/locks |
| [copilot-sdk/](copilot-sdk) | Programmatic Autopilot SDK runner |
| [tests/CopilotSdk.Tests/](tests/CopilotSdk.Tests) | SDK runner tests |
| [Controllers/](Controllers) | MVC controllers |
| [Services/](Services) | Service interfaces and implementations |
| [Middleware/](Middleware) | Custom middleware |
| [Models/](Models) | View and domain models |
| [Views/](Views) | Razor views |
| [docs/](docs) | Project documentation |

## Workflow Maintenance

When editing `.github/workflows/cloud-*.md`:

```powershell
.\scripts\check-gh-aw-version.ps1
Remove-Item .github/workflows/cloud-*.lock.yml -ErrorAction SilentlyContinue
gh aw compile
```

Commit the regenerated `.lock.yml` files with the `.md` source. Do not edit lockfiles by hand.

## Start Here

- Want editor-driven autonomy? Read [AI-SDLC-LOCAL.md](AI-SDLC-LOCAL.md).
- Want GitHub-native orchestration? Read [AI-SDLC-CLOUD.md](AI-SDLC-CLOUD.md).
- Want Autopilot as testable C# code? Read [AI-SDLC-SDK.md](AI-SDLC-SDK.md).

Same pipeline. Three control surfaces. Pick your cockpit.
