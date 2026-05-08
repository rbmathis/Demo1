# AI-Driven Software Development Lifecycle - SDK Mode (AI-SDLC-SDK)

A programmatic, AI-powered SDLC pipeline built with the GitHub Copilot SDK. SDK mode runs as a .NET console app under `copilot-sdk/Autopilot.csproj`, but it reuses the same local agent prompts and issue-as-state model as the local pipeline.

SDK mode is the experimental third lane: more deterministic than a chat-orchestrated local run, more inspectable than a fully cloud-dispatched workflow, and useful for testing how much of the AI-SDLC controller can live in regular application code.

---

## Architecture Overview

```text
┌──────────────────────────────────────────────────────────────────────────────┐
│  LOCAL MACHINE (.NET console app + Copilot SDK)                              │
│                                                                              │
│   dotnet run --project copilot-sdk/Autopilot.csproj -- issue 135             │
│       │                                                                      │
│       ▼                                                                      │
│   ┌───────────┐                                                              │
│   │ AUTOPILOT │  owns labels, model check, timeouts, and stage sequencing     │
│   └───────────┘                                                              │
│       │                                                                      │
│       ▼                                                                      │
│   ┌──────────┐  SDK session  ┌───────────────┐  SDK session  ┌────────────┐ │
│   │  TRIAGE  │──────────────▶│ FEATURE-FLAGS │──────────────▶│    PLAN    │ │
│   └──────────┘               └───────────────┘               └────────────┘ │
│   classifies issue,          only when rollout               creates plan,    │
│   emits rollout              is required/optional             branch, tasks    │
│   decision                                                                  │
│       │                                                                      │
│       ▼                                                                      │
│   ┌───────────┐       ┌──────────┐       ┌──────────┐       ┌───────────┐    │
│   │ IMPLEMENT │──────▶│  REVIEW  │──────▶│   DOCS   │──────▶│  DELIVER  │    │
│   └───────────┘       └──────────┘       └──────────┘       └───────────┘    │
│   writes code,         reviews PR,         verifies docs,     merges if       │
│   creates PR           may rework          activation notes   allowed         │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

**Chaining mechanism:** `SdkAutopilotRunner` calls each stage in sequence. Each stage gets a fresh Copilot SDK session with a wrapped prompt.

**State tracking:** The GitHub issue remains the state file. Stage agents read and write issue comments, while the SDK runner owns the transient `sdk/*` labels.

**Prompt source:** SDK mode loads `.github/agents/*.agent.md` at runtime. Changes to local agent behavior usually flow into SDK mode automatically.

---

## How to Invoke

From the repository root:

```powershell
dotnet run --project .\copilot-sdk\Autopilot.csproj -- issue 135 --repo rbmathis/Demo1 --approve-all --skip-deliver
```

From `copilot-sdk/`:

```powershell
dotnet run -- issue 135 --repo rbmathis/Demo1 --approve-all --skip-deliver
```

Use `--skip-deliver` for pilot runs. Remove it only when you want the SDK runner to continue through merge.

Useful preflights:

```powershell
dotnet run --project .\copilot-sdk\Autopilot.csproj -- --check-labels --repo rbmathis/Demo1
dotnet run --project .\copilot-sdk\Autopilot.csproj -- --check-labels --ensure-labels --repo rbmathis/Demo1
dotnet run --project .\copilot-sdk\Autopilot.csproj -- --check-model --repo rbmathis/Demo1
```

Timeout tuning:

```powershell
dotnet run --project .\copilot-sdk\Autopilot.csproj -- issue 135 --repo rbmathis/Demo1 --approve-all --stage-timeout-minutes 20
```

Each stage waits up to 10 minutes by default.

---

## Pipeline Stages

### Stage 0: Preflight

| Attribute | Value |
|-----------|-------|
| **Code** | `AutopilotApp`, `SdkAutopilotRunner` |
| **Label** | none yet |
| **Output** | validated options, label readiness, model readiness |

The SDK runner:

- Parses CLI options
- Resolves the repository root
- Checks required SDK labels
- Verifies the target issue is open
- Checks the selected Copilot model is available
- Requires `--approve-all` before allowing Copilot SDK tool permissions

If the issue is already closed, SDK mode exits without mutating labels.

### Stage 1: Triage

| Attribute | Value |
|-----------|-------|
| **Prompt** | `.github/agents/triage.agent.md` |
| **Label** | `sdk/triage` |
| **Output** | classification, rollout status, issue comment |

Triage classifies the issue and emits a structured result. SDK mode requires a valid final fenced JSON block so the controller can decide whether to continue, stop, or treat the issue as a duplicate.

### Stage 2: Feature Flags

| Attribute | Value |
|-----------|-------|
| **Prompt** | `.github/agents/feature-flags.agent.md` |
| **Label** | `sdk/feature-flags` |
| **Output** | rollout verdict and gating guidance |

This stage runs only when triage reports `rollout-required` or `rollout-optional`. Rollout-exempt issues skip it.

### Stage 3: Plan

| Attribute | Value |
|-----------|-------|
| **Prompt** | `.github/agents/plan.agent.md` |
| **Label** | `sdk/planning` |
| **Output** | implementation plan, branch, issue comment |

Plan reads the issue, triage handoff, and feature-flag consultation when present. It creates the implementation playbook and the feature branch.

### Stage 4: Implement

| Attribute | Value |
|-----------|-------|
| **Prompt** | `.github/agents/implement.agent.md` |
| **Label** | `sdk/implementing` |
| **Output** | code, tests, commit, pushed PR |

Implement executes the plan using the same specialist-agent guidance as local mode. The shared implement prompt tells the agent to use snarky conventional commits and spicy, review-friendly PR titles/bodies.

### Stage 5: Review

| Attribute | Value |
|-----------|-------|
| **Prompt** | `.github/agents/review.agent.md` |
| **Label** | `sdk/review` |
| **Output** | PR review decision |

Review checks the linked PR. If changes are requested, SDK mode loops back to implement for up to two review cycles.

### Stage 6: Docs

| Attribute | Value |
|-----------|-------|
| **Prompt** | `.github/agents/docs.agent.md` |
| **Label** | `sdk/docs` |
| **Output** | XML docs, markdown docs, verification notes |

Docs are blocking by default in SDK mode because rollout-sensitive work may need activation and rollback instructions. Use `--allow-missing-docs` only when deliberately accepting that risk.

### Stage 7: Deliver

| Attribute | Value |
|-----------|-------|
| **Prompt** | `.github/agents/deliver.agent.md` |
| **Label** | `sdk/delivering` |
| **Output** | merged PR, branch cleanup, landing report |

Deliver merges the approved PR and posts the landing report. `--skip-deliver` stops before this stage.

---

## Labels

SDK mode uses its own namespace so it can run beside local and cloud pipelines without label collisions.

Persistent provenance label:

| Label | Meaning |
|-------|---------|
| `sdk` | Issue has been handled by the Autopilot SDK runner |

Stage labels:

| Label | Stage |
|-------|-------|
| `sdk/triage` | Being classified |
| `sdk/feature-flags` | Rollout consultation |
| `sdk/planning` | Plan in progress |
| `sdk/implementing` | Code being written |
| `sdk/review` | PR under review |
| `sdk/docs` | Documentation update |
| `sdk/delivering` | Merge in progress |
| `sdk/done` | Pipeline complete |
| `sdk/failed` | Pipeline halted |

The plain `sdk` label is permanent provenance. The runner removes only `sdk/*` labels during stage transitions.

---

## Project Layout

| Path | Purpose |
|------|---------|
| `copilot-sdk/Program.cs` | Console entry point |
| `copilot-sdk/AutopilotApp.cs` | Composition root |
| `copilot-sdk/Options/` | CLI parsing and repo-root discovery |
| `copilot-sdk/GitHub/` | GitHub CLI wrappers and SDK label management |
| `copilot-sdk/Copilot/` | Copilot SDK session execution and model preflight |
| `copilot-sdk/Pipeline/` | Stage catalog, prompt wrapping, result parsing, orchestration |
| `tests/CopilotSdk.Tests/` | Unit tests for parsing, labels, orchestration, and preflights |

---

## Safety Gates

SDK mode is intentionally explicit about mutation:

- It checks labels before running stages.
- It checks model availability before applying provenance or stage labels.
- It exits without mutation for closed issues.
- It requires `--approve-all` before Copilot tool permissions are approved.
- It uses per-stage timeouts so long-running tasks can finish without hanging forever.
- It parses final JSON results and fails closed when the result is missing or malformed.

---

## How SDK Differs From Local and Cloud

| Capability | Local | Cloud | SDK |
|------------|-------|-------|-----|
| Entry point | VS Code/Copilot Chat agent | GitHub label/workflow dispatch | .NET console app |
| Orchestration | `autopilot.agent.md` | `cloud-*.md` workflows | `SdkAutopilotRunner` |
| Stage prompts | `.github/agents/*.agent.md` | Workflow prompts plus imported agents | `.github/agents/*.agent.md` with SDK wrapper |
| Labels | `local/*` | `cloud/*` plus `autopilot` | `sdk/*` plus `sdk` |
| Runtime | Current editor/chat session | GitHub Actions + Copilot coding agent | Copilot SDK sessions |
| Best for | Hands-on local delivery | Remote autonomous delivery | Programmatic experiments and repeatable controller behavior |

---

## Current Status

SDK mode is an experiment, not the default production delivery path. It is useful for proving that the local autopilot workflow can be driven by code, testing prompt contracts, validating model availability, and identifying where agent stages need stronger structure.

For detailed code-level notes, see [copilot-sdk/README.md](copilot-sdk/README.md).
