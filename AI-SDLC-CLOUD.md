# AI-Driven Software Development Lifecycle (AI-SDLC)

A fully-autonomous, AI-powered SDLC pipeline built on [GitHub Agentic Workflows](https://github.github.com/gh-aw/) and Copilot agents. Issues flow from intake to deployment with zero human intervention. Each stage posts structured comments to the issue — the issue itself is the state file.

---

## Architecture Overview

```text
┌──────────────────────────────────────────────────────────────────────┐
│                                                                      │
│   Issue Opened                                                       │
│       │                                                              │
│       ▼                                                              │
│   ┌──────────┐  dispatch   ┌──────────┐  dispatch   ┌───────────┐  │
│   │  TRIAGE  │────────────▶│   PLAN   │────────────▶│ IMPLEMENT │  │
│   └──────────┘             └──────────┘             └───────────┘  │
│   classifies issue,        analyzes codebase,        assigns        │
│   adds type labels,        posts detailed plan       Copilot        │
│   dispatches Plan          to issue, dispatches      coding agent   │
│                            Implement                                 │
│                                                         │           │
│                                           Copilot creates PR        │
│                                                         │           │
│                                                         ▼           │
│                                                  ┌─────────────┐   │
│                                                  │ NOTIFY CODE │   │
│                                                  │  COMPLETE   │   │
│                                                  └─────────────┘   │
│                                                  posts comment,     │
│                                                  dispatches Review  │
│                                                         │           │
│                            ┌──────────┐  dispatch       │           │
│                            │  DEPLOY  │◀────────────┌───▼──────┐   │
│                            └──────────┘             │  REVIEW  │   │
│                            verifies merge,          └──────────┘   │
│                            posts summary,           multi-agent     │
│                            closes issue             code review     │
│                                                                      │
└──────────────────────────────────────────────────────────────────────┘
```

**Chaining mechanism:** `dispatch-workflow` safe output (agentic stages) and `workflow_dispatch` API call (notify-code-complete bridge). Each stage explicitly dispatches the next, eliminating race conditions.

**State tracking:** Each stage posts a structured comment (🏷️ Triage, 📋 Plan, 🚀 Implement, ✅ Code Complete, 🔄 Review In Progress, 🏗️ Report, ✅ Complete) to the issue. Downstream stages read upstream comments to understand context.

---

## Pipeline Stages

### Stage 1: Triage

| Attribute | Value |
| ----------- | ------- |
| **Trigger** | `label_command: pipeline/triage-requested` on issues |
| **Workflow** | `pipeline-triage.md` |
| **Engine** | Copilot |
| **Output** | Classification comment, type labels, dispatches Plan |

The triage agent:

- Reads the issue title and body
- Classifies by type, difficulty, priority, and scope
- Determines which specialist agents are needed
- Posts a structured triage comment with classification table
- Applies classification labels (bug, enhancement, feature, security, etc.)
- Dispatches `pipeline-plan` with the issue number

### Stage 2: Plan

| Attribute | Value |
| ----------- | ------- |
| **Trigger** | `workflow_dispatch` (dispatched by Triage with `issue_number` input) |
| **Workflow** | `pipeline-plan.md` |
| **Engine** | Copilot (Claude Opus 4.6) |
| **Output** | Detailed implementation plan comment, dispatches Implement |

The plan agent:

- Reads the issue and triage comment
- Analyzes the codebase to understand current state
- Creates a detailed implementation plan including:
  - Branch name (`feat/issue-{N}-{slug}`)
  - Specific files to create/modify with descriptions
  - Agent delegation order
  - Acceptance criteria and testing requirements
- Posts the plan as a structured comment on the issue
- Dispatches `pipeline-implement` with the issue number

### Stage 3: Implement

| Attribute | Value |
| ----------- | ------- |
| **Trigger** | `workflow_dispatch` (dispatched by Plan with `issue_number` input) |
| **Workflow** | `pipeline-implement.md` |
| **Engine** | Copilot |
| **Output** | Copilot coding agent assigned, PR created |

The implement agent:

- Reads the issue and finds the plan comment
- Posts a status comment confirming agent assignment
- Assigns Copilot coding agent to the issue with custom instructions:
  1. Read the plan from the issue
  2. Delegate to specialist sub-agents (backend, frontend, security)
  3. Write tests (mandatory)
  4. Add documentation (mandatory)
  5. Validate build

> **Note:** The coding agent does not post back to the issue. The Copilot coding agent's firewall blocks `api.github.com/graphql`, so the Review stage handles issue reporting instead.

### Stage 4: Review

| Attribute | Value |
| ----------- | ------- |
| **Trigger** | `workflow_dispatch` (dispatched by notify-code-complete with `issue_number` input) |
| **Workflow** | `pipeline-review.md` |
| **Engine** | Copilot |
| **Imports** | code-reviewer, security-auditor, testing, docs agents |
| **Output** | PR review + implementation report on linked issue, dispatches Deploy |

The review agent:

- Finds the PR associated with the given issue number
- **Posts a "Review In Progress" status comment** on the issue (confirms code is complete)
- Reads the PR and changed files
- Delegates to specialist review agents:
  - `security-auditor` — OWASP Top 10, CSRF, XSS, SQL injection
  - `code-reviewer` — MVC patterns, SOLID, code quality
  - `testing` — test coverage and quality
  - `docs` — XML documentation and docs/ updates
- Posts inline review comments on specific lines
- Submits a consolidated review verdict
- Posts the **Implementation Report** on the issue (changes table, review verdict, branch)
- Dispatches `pipeline-deploy` with the issue number

### Notification: Code Complete

| Attribute | Value |
| ----------- | ------- |
| **Trigger** | `pull_request: [ready_for_review, review_requested]` |
| **Workflow** | `notify-code-complete.yml` (plain YAML, not agentic) |
| **Engine** | GitHub Actions (no Copilot) |
| **Output** | "✅ Code Complete" comment on linked issue + dispatches pipeline-review |

This lightweight workflow bridges the async gap between Copilot finishing work and the review stage starting. It:

- Fires instantly on the same PR events that indicate coding is done
- Extracts the linked issue from the PR body (`Closes #N`)
- Posts a "Code Complete" comment to the issue within seconds
- **Dispatches `pipeline-review`** with the issue number (the implement→review bridge)
- Runs as standard GitHub Actions (not an agentic workflow), so no first-time approval is required

### Stage 5: Deploy

| Attribute | Value |
| ----------- | ------- |
| **Trigger** | `workflow_dispatch` (dispatched by Review with `issue_number` input) |
| **Workflow** | `pipeline-deploy.md` |
| **Engine** | Copilot |
| **Output** | Issue closed with deployment summary |

The deploy agent:

- Finds the merged PR associated with the given issue number
- If no merged PR found, noops
- Reads the full pipeline history from issue comments
- Posts a final deployment summary with complete pipeline history
- Closes the issue as completed

---

## Issue as State File

The issue itself serves as the pipeline's state file. Each stage appends a structured comment:

```text
Issue #100: "Add a /health endpoint"
│
├── 🏷️ Pipeline — Triage        (classification, agents needed)
├── 📋 Pipeline — Plan           (implementation steps, files, acceptance criteria)
├── 🚀 Pipeline — Implement      (agent assignment confirmation)
├── ✅ Pipeline — Code Complete   (Copilot finished — posted by notify-code-complete.yml)
├── 🔄 Pipeline — Review In Progress  (multi-agent review started — posted by Review)
├── 🏗️ Pipeline — Implementation Report  (changes table, review verdict — posted by Review)
└── ✅ Pipeline — Complete        (deployment summary, pipeline history)
```

Downstream stages read upstream comments to understand context — no separate state files, no label-based state machine, no JSON blocks to parse.

---

## Labels

Labels are used for **classification only**, never as pipeline triggers:

| Label | Purpose | Applied By |
| ------- | --------- | ----------- |
| `bug` | Issue type | Triage |
| `enhancement` | Issue type | Triage |
| `feature` | Issue type | Triage |
| `security` | Issue type | Triage |
| `documentation` | Issue type | Triage |
| `refactor` | Issue type | Triage |

No `pipeline:*` labels exist. Stage transitions use `dispatch-workflow`.

---

## Agent Roster

| Agent | Role | Used In |
| ------- | ------ | --------- |
| `backend` | Controllers, Models, Services, Middleware | Implement |
| `frontend` | Views, Razor, CSS, JavaScript | Implement |
| `security` | Auth, headers, OWASP | Implement |
| `testing` | Unit + integration tests | Implement, Review |
| `docs` | Documentation | Implement, Review |
| `build-validator` | Build health | Implement |
| `code-reviewer` | Code quality review | Review |
| `security-auditor` | Security scanning | Review |

---

## Workflow Files

| File | Purpose | Trigger |
| ------ | --------- | --------- |
| `pipeline-triage.md` | Classify and dispatch | `label_command: pipeline/triage-requested` |
| `pipeline-plan.md` | Create implementation plan | `workflow_dispatch` (from Triage) |
| `pipeline-implement.md` | Assign Copilot coding agent | `workflow_dispatch` (from Plan) |
| `notify-code-complete.yml` | Post "Code Complete" + dispatch review | `pull_request: [ready_for_review, review_requested]` |
| `pipeline-review.md` | Multi-agent code review + issue report | `workflow_dispatch` (from notify-code-complete) |
| `pipeline-deploy.md` | Verify merge and close issue | `workflow_dispatch` (from Review) |

All workflows are [GitHub Agentic Workflows](https://github.github.com/gh-aw/) (`.md` source compiled to `.lock.yml`).

---

## Failure Handling

Failures are logged as comments on the issue. There are no automatic retry or rollback mechanisms — the issue comment history provides full telemetry of what happened and where it stopped.

If a stage fails:

- The workflow's conclusion comment is posted on the issue
- A human can investigate and re-run the workflow manually from the Actions tab
- The issue remains open with full context of what was attempted

If code review requests changes:

- Copilot coding agent pushes fixes to the PR branch
- `review_requested` event re-triggers `notify-code-complete.yml`, which dispatches Review again

---

## How It Works End-to-End

```text
1. Developer applies `pipeline/triage-requested` label to an issue
2. Triage classifies the issue and dispatches Plan            (~2 min)  [automatic]
3. Plan analyzes the codebase and posts implementation plan   (~3 min)  [automatic]
4. Implement assigns Copilot coding agent                     (~2 min)  [automatic]
5. Copilot agent writes code, tests, docs, creates PR        (~5-15 min) [automatic]
6. notify-code-complete posts comment + dispatches Review     (~5 sec)  [automatic]
7. ⏸️  HUMAN approves the review workflow run (first-time gate)           [manual]
8. Review runs multi-agent code review on the PR              (~3 min)  [automatic]
9. Review posts implementation report on linked issue                    [automatic]
10. Review dispatches Deploy                                             [automatic]
11. ⏸️  HUMAN merges PR (Deploy verifies merge before acting)            [manual]
12. Deploy verifies merge, posts final summary, closes issue  (~1 min)  [automatic]
```

**Human touchpoints (2):**

1. Approve the review workflow run (one-time per new workflow)
2. Merge the PR after reviewing the code and the agent's review

Everything else — including issue closure — is fully autonomous.

---

## Local Development

### Prerequisites

- .NET 9 SDK
- GitHub repository with Actions enabled

### Testing Locally

```bash
# Build
dotnet build --configuration Release

# Test
dotnet test --configuration Release --verbosity normal

# Run
dotnet run
```

### Triggering the Pipeline

1. Create a new issue with a descriptive title and body
2. Apply the `pipeline/triage-requested` label to the issue
3. Watch the pipeline stages execute via issue comments
4. Each stage posts a structured comment as it completes

---

## Design Principles

1. **Zero-touch happy path** — Minimal human intervention from issue to deployed code
2. **Issue is the state file** — All pipeline state lives as comments on the issue
3. **Dispatch over labels** — `dispatch-workflow` chains stages explicitly, no label race conditions
4. **Standalone planning** — Dedicated Plan stage creates actionable specs before coding begins
5. **Observable telemetry** — Every stage posts structured comments with timestamps and details
6. **Classification labels only** — Labels describe what an issue IS, not where it IS in the pipeline
7. **Fail gracefully** — No retry/rollback machinery; failures are logged and humans can intervene
