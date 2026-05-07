# AI-Driven Software Development Lifecycle (AI-SDLC)

A fully-autonomous, AI-powered SDLC pipeline built on [GitHub Agentic Workflows](https://github.github.com/gh-aw/) and Copilot agents. Issues flow from intake to deployment with zero human intervention. Each stage posts structured comments to the issue — the issue itself is the state file.

---

## Architecture Overview

```text
┌─────────────────────────────────────────────────────────────────────────────┐
│                                                                             │
│   cloud/autopilot label                                                     │
│       │                                                                     │
│       ▼                                                                     │
│   ┌───────────┐                                                             │
│   │ AUTOPILOT │  validates issue, dispatches Triage                         │
│   └───────────┘                                                             │
│       │                                                                     │
│       ▼                                                                     │
│   ┌──────────┐  dispatch   ┌──────────┐  dispatch   ┌───────────┐         │
│   │  TRIAGE  │────────────▶│   PLAN   │────────────▶│ IMPLEMENT │         │
│   └──────────┘             └──────────┘             └───────────┘         │
│   classifies issue,        analyzes codebase,        assigns               │
│   adds type labels,        posts detailed plan       Copilot               │
│   dispatches Plan          to issue, dispatches      coding agent           │
│                            Implement                                        │
│                                                         │                  │
│                                           Copilot creates PR               │
│                                                         │                  │
│                                                         ▼                  │
│                                                  ┌─────────────┐          │
│                                                  │ NOTIFY CODE │          │
│                                                  │  COMPLETE   │          │
│                                                  └─────────────┘          │
│                                                  posts comment,            │
│                                                  dispatches Review         │
│                                                         │                  │
│                                                    ┌────▼─────┐           │
│                          ┌─────────────────────────│  REVIEW  │           │
│                          │ (request changes,       └──────────┘           │
│                          │  max 2 rework cycles)    │ approve             │
│                          ▼                          ▼                      │
│                   ┌───────────┐              ┌──────────┐                 │
│                   │ IMPLEMENT │              │   DOCS   │                 │
│                   │ (rework)  │              └──────────┘                 │
│                   └───────────┘              adds XML docs,               │
│                                              updates docs/,               │
│                                              dispatches Finish            │
│                                                    │                      │
│                                                    ▼                      │
│                                              ┌──────────┐                 │
│                                              │  FINISH  │                 │
│                                              └──────────┘                 │
│                                              squash merges PR,            │
│                                              deletes branch,              │
│                                              closes issue                 │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Chaining mechanism:** `dispatch-workflow` safe output (agentic stages). Each stage explicitly dispatches the next, eliminating race conditions. The one exception is the Implement→Review gap: after Copilot coding agent finishes, a human applies the `cloud/review` label to resume the pipeline.

**Review loop:** If review requests changes, it re-dispatches implement (max 2 rework cycles). After 2 failures, the pipeline halts for human intervention.

**State tracking:** Each stage posts a structured comment (✈️ Autopilot, 🏷️ Triage, 📋 Plan, 🚀 Implement, ✅ Code Complete, 🔄 Review, 📚 Docs, ✅ Complete) to the issue. Downstream stages read upstream comments to understand context.

---

## Pipeline Stages

### Entry Point: Autopilot

| Attribute | Value |
| ----------- | ------- |
| **Trigger** | `cloud/autopilot` label on issue, or `workflow_dispatch` |
| **Workflow** | `cloud-autopilot.md` |
| **Engine** | Copilot |
| **Output** | Validates issue, posts engagement comment, dispatches Triage |

The autopilot:

- Validates the issue has enough information to proceed
- Removes any stale `cloud/*` labels
- Posts an "Autopilot Engaged" comment with the full pipeline stage table
- Dispatches `cloud-triage` (or applies `cloud/triage-requested` label)

> **Note:** The autopilot is optional — you can still trigger the pipeline by applying `cloud/triage-requested` directly.

### Stage 1: Triage

| Attribute | Value |
| ----------- | ------- |
| **Trigger** | `label_command: cloud/triage-requested` on issues |
| **Workflow** | `cloud-triage.md` |
| **Engine** | Copilot |
| **Output** | Classification comment, type labels, dispatches Plan |

The triage agent:

- Reads the issue title and body
- Classifies by type, difficulty, priority, and scope
- Determines which specialist agents are needed
- Posts a structured triage comment with classification table
- Applies classification labels (bug, enhancement, feature, security, etc.)
- Dispatches `cloud-plan` with the issue number

### Stage 2: Plan

| Attribute | Value |
| ----------- | ------- |
| **Trigger** | `workflow_dispatch` (dispatched by Triage with `issue_number` input) |
| **Workflow** | `cloud-plan.md` |
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
- Dispatches `cloud-implement` with the issue number

### Stage 3: Implement

| Attribute | Value |
| ----------- | ------- |
| **Trigger** | `workflow_dispatch` (dispatched by Plan with `issue_number` input) |
| **Workflow** | `cloud-implement.md` |
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
| **Trigger** | `cloud/review` label on issue, or `workflow_dispatch` with `issue_number` input |
| **Workflow** | `cloud-review.md` |
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
- Posts inline review comments on specific lines
- Submits a consolidated review verdict
- Posts the **Implementation Report** on the issue (changes table, review verdict, branch)
- **If APPROVE/COMMENT:** Dispatches `cloud-docs` with the issue number
- **If REQUEST_CHANGES:** Dispatches `cloud-implement` for rework (max 2 cycles, then halts)

### Review Loop (Rework)

If the review agent submits `REQUEST_CHANGES`, it re-dispatches `cloud-implement`. The coding agent reads the review comments and fixes the issues, creating a new commit on the same PR. When Copilot finishes, the user applies the `cloud/review` label again to restart review. This loop repeats up to 2 times before halting.

### Stage 5: Docs

| Attribute | Value |
| ----------- | ------- |
| **Trigger** | `workflow_dispatch` (dispatched by Review with `issue_number` input) |
| **Workflow** | `cloud-docs.md` |
| **Engine** | Copilot |
| **Imports** | docs agent |
| **Output** | XML docs + markdown updates committed, dispatches Finish |

The docs agent:

- Finds the PR associated with the issue
- Reads the diff to understand what was implemented
- Adds XML documentation to new/modified public methods and classes
- Updates docs/ markdown if architecture or APIs changed
- Commits documentation to the feature branch
- Posts a documentation summary comment on the issue
- Dispatches `cloud-finish` with the issue number

### ⏸️ Human Touchpoint: Resume After Copilot Finishes

After the Implement stage assigns Copilot coding agent, the agent works asynchronously for 5–15 minutes. When finished, it creates a PR and marks it ready for review. GitHub sends an email notification.

**To resume the pipeline:** Apply the `cloud/review` label to the issue. This triggers the Review stage, and the rest of the pipeline (Review → Docs → Finish) runs automatically.

> **Why manual?** GitHub's anti-recursion rule prevents workflows triggered by GitHub App tokens (which Copilot uses) from triggering other `on: pull_request` workflows. The label is the simplest reliable bridge.

### Stage 6: Finish

| Attribute | Value |
| ----------- | ------- |
| **Trigger** | `workflow_dispatch` (dispatched by Docs with `issue_number` input) |
| **Workflow** | `cloud-finish.yml` (plain YAML, not agentic) |
| **Engine** | GitHub Actions (no Copilot) |
| **Output** | PR squash-merged, branch deleted, issue closed |

The finish workflow:

- Finds the approved PR for the issue
- Squash-merges the PR to main
- Deletes the feature branch
- Removes all `cloud/*` labels and adds `cloud/done`
- Posts a completion comment on the issue
- Closes the issue as completed

> **Note:** Finish is plain YAML (not agentic) — it requires no Copilot engine or first-time approval gate.

---

## Issue as State File

The issue itself serves as the pipeline's state file. Each stage appends a structured comment:

```text
Issue #100: "Add a /health endpoint"
│
├── ✈️ Pipeline — Autopilot Engaged  (pipeline activated, dispatches Triage)
├── 🏷️ Pipeline — Triage        (classification, agents needed)
├── 📋 Pipeline — Plan           (implementation steps, files, acceptance criteria)
├── 🚀 Pipeline — Implement      (agent assignment confirmation)
├── ✅ Pipeline — Code Complete   (Copilot finished — user applies cloud/review label)
├── 🔄 Pipeline — Review In Progress  (multi-agent review started)
├── 🏗️ Pipeline — Implementation Report  (changes table, review verdict)
├── 📚 Pipeline — Documentation   (XML docs and markdown updates)
└── ✅ Pipeline — Complete        (PR merged, branch deleted, issue closed)
```

Downstream stages read upstream comments to understand context — no separate state files, no label-based state machine, no JSON blocks to parse.

---

## Labels

Labels serve two purposes: **issue classification** and **stage tracking**.

### Classification Labels (applied by Triage)

| Label | Purpose |
| ------- | --------- |
| `bug` | Issue type |
| `enhancement` | Issue type |
| `feature` | Issue type |
| `security` | Issue type |
| `documentation` | Issue type |
| `refactor` | Issue type |

### Stage Labels (`cloud/*` — applied by each stage)

| Label | Applied By | Meaning |
| ------- | ----------- | --------- |
| `cloud/autopilot` | Human | Initial trigger — kicks off the pipeline |
| `cloud/triage-requested` | Autopilot | Triggers Triage via label_command |
| `cloud/triage` | Triage | Classifying |
| `cloud/planning` | Plan | Creating implementation plan |
| `cloud/implementing` | Implement | Copilot coding agent assigned |
| `cloud/review` | Review | Multi-agent review in progress |
| `cloud/awaiting-merge` | Review | Review approved, awaiting next stage |
| `cloud/documenting` | Docs | Adding documentation |
| `cloud/done` | Finish | Pipeline complete |

Stage labels are mutually exclusive — each stage removes prior `cloud/*` labels before applying its own. Stage transitions still use `dispatch-workflow`; labels provide at-a-glance status.

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
| `cloud-autopilot.md` | Single entry point, validates and dispatches | `label_command: cloud/autopilot` or `workflow_dispatch` |
| `cloud-triage.md` | Classify and dispatch | `label_command: cloud/triage-requested` |
| `cloud-plan.md` | Create implementation plan | `workflow_dispatch` (from Triage) |
| `cloud-implement.md` | Assign Copilot coding agent | `workflow_dispatch` (from Plan) |
| `cloud-review.md` | Multi-agent code review + issue report | `label_command: cloud/review` or `workflow_dispatch` |
| `cloud-docs.md` | Add XML docs and update markdown | `workflow_dispatch` (from Review on approve) |
| `cloud-finish.yml` | Squash-merge PR, close issue | `workflow_dispatch` (from Docs) |

Agentic workflows (`.md` source) are compiled to `.lock.yml` via [gh-aw](https://github.github.com/gh-aw/). `cloud-finish.yml` is plain YAML.

---

## Failure Handling

Failures are logged as comments on the issue. There are no automatic retry or rollback mechanisms — the issue comment history provides full telemetry of what happened and where it stopped.

If a stage fails:

- The workflow's conclusion comment is posted on the issue
- A human can investigate and re-run the workflow manually from the Actions tab
- The issue remains open with full context of what was attempted

If code review requests changes:

- Review dispatches `cloud-implement` for rework (max 2 cycles)
- Copilot coding agent reads review comments and pushes fixes
- `review_requested` event cannot re-trigger workflows due to GitHub's app token anti-recursion rule, so the user must apply the `cloud/review` label again
- After 2 failed rework cycles, the pipeline halts for human intervention

---

## How It Works End-to-End

```text
1. Developer applies `cloud/autopilot` label to an issue (or `cloud/triage-requested` directly)
2. Autopilot validates and dispatches Triage                  (~30 sec) [automatic]
3. Triage classifies the issue and dispatches Plan            (~2 min)  [automatic]
4. Plan analyzes the codebase and posts implementation plan   (~3 min)  [automatic]
5. Implement assigns Copilot coding agent                     (~2 min)  [automatic]
6. Copilot agent writes code, tests, creates PR              (~5-15 min) [automatic]
7. ⏸️  User receives email that PR is ready for review                     [notification]
8. User applies `cloud/review` label to the issue                         [manual — ~5 sec]
9. ⏸️  HUMAN approves the review workflow run (first-time gate)            [manual]
10. Review runs multi-agent code review on the PR              (~3 min)  [automatic]
11. Review posts implementation report on linked issue                    [automatic]
12. Review dispatches Docs (on approve) or Implement (on request changes)
13. Docs adds XML documentation, dispatches Finish             (~2 min)  [automatic]
14. Finish squash-merges PR, deletes branch, closes issue      (~30 sec) [automatic]
```

**Human touchpoints (2-3):**

1. Apply `cloud/review` label after Copilot finishes coding (you'll get an email)
2. Approve the review workflow run (one-time per new workflow)
3. *(Optional)* If review requests changes after 2 rework cycles, human intervenes

Everything else — including PR merge and issue closure — is fully autonomous.

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
2. Apply the `cloud/autopilot` label to the issue (or `cloud/triage-requested` to skip autopilot)
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
