# AI-Driven Software Development Lifecycle (AI-SDLC)

A fully-autonomous, AI-powered SDLC pipeline built on GitHub Actions and Copilot agents. Issues flow from intake to deployment with zero human intervention (unless a stage fails or review requests changes).

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         PIPELINE CONTROLLER                              │
│                    (pipeline-controller.yml)                              │
│                                                                          │
│  ┌──────────┐    ┌──────────┐    ┌──────────┐                          │
│  │  INTAKE  │───▶│  TRIAGE  │───▶│  ROUTE   │ ─── chained steps ───    │
│  └──────────┘    └──────────┘    └──────────┘                          │
│       ▲                                  │                               │
│       │                                  │ applies pipeline:planning     │
│  issue opened                            ▼ (cross-workflow trigger)      │
└─────────────────────────────────────────────────────────────────────────┘
                                           │
┌─────────────────────────────────────────────────────────────────────────┐
│                      PLAN & IMPLEMENT                                    │
│                   (pipeline-implement.yml)                                │
│                                                                          │
│  ┌──────────┐    ┌───────────┐                                          │
│  │   PLAN   │───▶│ IMPLEMENT │ ─── chained steps ───                    │
│  └──────────┘    └───────────┘                                          │
│       ▲                  │                                               │
│       │                  │ Copilot coding agent creates PR               │
│  pipeline:planning       │ with pipeline:testing label                   │
└─────────────────────────────────────────────────────────────────────────┘
                                           │
┌─────────────────────────────────────────────────────────────────────────┐
│                       TEST & REVIEW                                      │
│                    (pipeline-review.yml)                                  │
│                                                                          │
│  ┌──────────┐    ┌──────────┐                                           │
│  │   TEST   │───▶│  REVIEW  │ ─── chained steps ───                    │
│  └──────────┘    └──────────┘                                           │
│       ▲                  │                                               │
│       │                  │ applies pipeline:deploying                    │
│  pipeline:testing        ▼ (cross-workflow trigger)                      │
└─────────────────────────────────────────────────────────────────────────┘
                                           │
┌─────────────────────────────────────────────────────────────────────────┐
│                          DEPLOY                                           │
│                    (pipeline-deploy.yml)                                  │
│                                                                          │
│  ┌──────────┐    ┌───────────────┐    ┌──────────┐                     │
│  │  MERGE   │───▶│ HEALTH CHECK  │───▶│ COMPLETE │                     │
│  └──────────┘    └───────────────┘    └──────────┘                     │
│       ▲                  │                                               │
│       │                  │ on failure: pipeline:rollback                  │
│  pipeline:deploying      ▼                                               │
└─────────────────────────────────────────────────────────────────────────┘
                                           │
┌─────────────────────────────────────────────────────────────────────────┐
│                         ROLLBACK                                          │
│                   (pipeline-rollback.yml)                                 │
│                                                                          │
│  Reverts merge commit → Redeploys previous version                      │
│  Triggered by: pipeline:rollback label                                   │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Pipeline Stages

### Stage 1: Intake

| Attribute | Value |
|-----------|-------|
| **Trigger** | Issue opened or reopened |
| **Agent** | `issue-helper` |
| **Workflow** | `pipeline-controller.yml` |
| **Output** | Validates issue structure, applies `pipeline:intake` label |

The intake stage:
- Ensures all required pipeline labels exist in the repository
- Validates the issue has sufficient information
- Posts a narrative comment acknowledging receipt
- Posts machine-readable state as a collapsed JSON block

### Stage 2: Triage

| Attribute | Value |
|-----------|-------|
| **Trigger** | Chained from intake (same job) |
| **Agent** | `issue-helper` |
| **Workflow** | `pipeline-controller.yml` |
| **Output** | Classification labels (type, difficulty, priority) |

The triage stage classifies the issue:
- **Type:** bug, enhancement, refactor, security
- **Difficulty:** easy, moderate, hard
- **Priority:** low, medium, high, critical
- **Scope areas:** Controllers, Models, Views, Tests, DevOps, etc.

### Stage 3: Route

| Attribute | Value |
|-----------|-------|
| **Trigger** | Chained from triage (same job) |
| **Agent** | `orchestrator` |
| **Workflow** | `pipeline-controller.yml` |
| **Output** | `pipeline:planning` label (triggers next workflow) |

The route stage:
- Assigns specialist agents based on classification
- Determines execution order (parallel vs sequential)
- Explains why each agent was (or wasn't) assigned
- Applies `pipeline:planning` label → triggers Plan & Implement workflow

### Stage 4: Plan

| Attribute | Value |
|-----------|-------|
| **Trigger** | `pipeline:planning` label applied (cross-workflow) |
| **Agent** | `planner` |
| **Workflow** | `pipeline-implement.yml` |
| **Output** | Feature branch created, task breakdown posted |

The plan stage:
- Parses the route state from issue comments
- Generates a feature branch: `feat/issue-{N}-{slug}`
- Decomposes the issue into tasks with dependencies
- Posts a detailed implementation plan with task table

### Stage 5: Implement

| Attribute | Value |
|-----------|-------|
| **Trigger** | Chained from plan (same job) |
| **Agent** | `implementer` → delegates to specialists |
| **Workflow** | `pipeline-implement.yml` |
| **Output** | Code committed to feature branch, PR created with `pipeline:testing` |

The implement stage:
- Assigns the issue to the Copilot coding agent
- Provides structured instructions for implementation
- The agent follows the plan, commits with conventional messages
- Creates a PR referencing `Closes #N` with the `pipeline:testing` label

### Stage 6: Test

| Attribute | Value |
|-----------|-------|
| **Trigger** | `pipeline:testing` label on PR (cross-workflow) |
| **Agent** | `testing` + `build-validator` |
| **Workflow** | `pipeline-review.yml` |
| **Output** | Build + test results posted; proceeds or returns to implement |

The test stage:
- Builds the project (`dotnet build --configuration Release`)
- Runs all tests (`dotnet test`)
- Posts detailed results with pass/fail status
- On success: chains to review stage
- On failure: posts retry instructions for the implementer

### Stage 7: Review

| Attribute | Value |
|-----------|-------|
| **Trigger** | Chained from test (same job, when tests pass) |
| **Agent** | `reviewer` |
| **Workflow** | `pipeline-review.yml` |
| **Output** | `pipeline:deploying` label or changes requested |

The review stage performs automated multi-dimensional review:
- **Architecture:** MVC patterns, controller conventions
- **Security:** SQL injection, secrets, anti-forgery tokens, validation
- **Quality:** Async patterns, code smells
- **Testing:** Test coverage included
- **Documentation:** Docs updated

Verdict:
- **Approved** (no critical/high findings) → applies `pipeline:deploying`, approves PR
- **Changes Requested** → requests changes on PR, returns to implement

### Stage 8: Deploy

| Attribute | Value |
|-----------|-------|
| **Trigger** | `pipeline:deploying` label applied (cross-workflow) |
| **Agent** | `deployer` |
| **Workflow** | `pipeline-deploy.yml` |
| **Output** | PR merged, deployed, issue closed |

The deploy stage:
- Merges the PR to main
- Deploys to Azure (production environment)
- Runs health checks against the deployed application
- On success: closes the issue, posts completion narrative
- On failure: applies `pipeline:rollback` label

### Rollback (conditional)

| Attribute | Value |
|-----------|-------|
| **Trigger** | `pipeline:rollback` label applied (cross-workflow) |
| **Agent** | `devops` |
| **Workflow** | `pipeline-rollback.yml` |
| **Output** | Merge commit reverted, previous version redeployed |

### Retry (conditional)

| Attribute | Value |
|-----------|-------|
| **Trigger** | `pipeline:retrying` or `pipeline:failed` label |
| **Agent** | `pipeline` |
| **Workflow** | `pipeline-retry.yml` |
| **Output** | Re-dispatches to the failed stage (max 2 attempts) |

---

## Label State Machine

Labels track pipeline progress. Each stage transition swaps labels:

```
pipeline:intake → pipeline:triage → pipeline:planning → pipeline:implementing
    → pipeline:testing → pipeline:reviewing → pipeline:deploying → pipeline:complete
```

| Label | Applied By | Triggers |
|-------|-----------|----------|
| `pipeline:intake` | Controller (intake step) | Tracking only |
| `pipeline:triage` | Controller (triage step) | Tracking only |
| `pipeline:planning` | Controller (route step) | `pipeline-implement.yml` |
| `pipeline:implementing` | Implement workflow (plan step) | Tracking only |
| `pipeline:testing` | Copilot agent (on PR) | `pipeline-review.yml` |
| `pipeline:reviewing` | Review workflow (test step) | Tracking only |
| `pipeline:deploying` | Review workflow (review step) | `pipeline-deploy.yml` |
| `pipeline:complete` | Deploy workflow | Terminal state |
| `pipeline:failed` | Any workflow on failure | `pipeline-retry.yml` |
| `pipeline:retrying` | Retry workflow | `pipeline-retry.yml` |
| `pipeline:rollback` | Deploy workflow on health check failure | `pipeline-rollback.yml` |

---

## Critical Design Decision: GITHUB_TOKEN Limitation

**Problem:** Labels applied within a workflow using the default `GITHUB_TOKEN` do NOT re-trigger the same workflow or other jobs within it.

**Solution:** Stages that would have been separate jobs within the same workflow are combined into **chained steps** using `core.setOutput()`:

| Workflow | Chained Steps |
|----------|--------------|
| `pipeline-controller.yml` | intake → triage → route |
| `pipeline-implement.yml` | plan → implement |
| `pipeline-review.yml` | test → review |

Cross-workflow triggers (label applied in workflow A triggers workflow B) **do work** with GITHUB_TOKEN.

---

## Pipeline State Comments

Every stage posts two comments to the issue:

### 1. Narrative Comment (human-readable)

```markdown
## 🏷️ Pipeline — Triage Stage

**Agent:** `issue-helper`
**Timestamp:** 2026-05-05 12:00:00 UTC

### Classification
| Attribute | Value | Reasoning |
|-----------|-------|-----------|
| **Type** | bug | matched bug/error keywords |
...
```

### 2. Machine-Readable State (collapsed JSON)

```markdown
<details>
<summary>📊 Pipeline State</summary>

​```json
{
  "pipeline": "sdlc",
  "stage": "triage",
  "status": "completed",
  "classification": { "type": "bug", "difficulty": "moderate", "priority": "medium" },
  "branch": null,
  "attempt": 1,
  "next": "route",
  "timestamp": "2026-05-05T12:00:00.000Z"
}
​```

</details>
```

Downstream stages parse this JSON to read decisions made by upstream stages.

---

## Agent Roster

| Agent | Role | Used In Stages |
|-------|------|---------------|
| `issue-helper` | Intake & triage | Intake, Triage |
| `orchestrator` | Agent routing | Route |
| `planner` | Task decomposition | Plan |
| `implementer` | Delegates to specialists | Implement |
| `backend` | Controllers, models, services | Implement |
| `frontend` | Views, Razor, CSS, JS | Implement |
| `security` | Auth, headers, OWASP | Implement |
| `devops` | CI/CD, Docker, workflows | Implement |
| `testing` | Unit + integration tests | Test, Implement |
| `docs` | Documentation | Implement |
| `build-validator` | Build health | Test |
| `code-reviewer` | Quality review | Review |
| `security-auditor` | Security scanning | Review |
| `reviewer` | Multi-faceted PR review | Review |
| `deployer` | Merge + deploy + health | Deploy |
| `pipeline` | State machine control | Retry, Restart |

---

## Workflow Files

| File | Purpose | Trigger |
|------|---------|---------|
| `pipeline-controller.yml` | Intake → Triage → Route + manual commands | Issue opened/reopened, comments |
| `pipeline-implement.yml` | Plan → Implement | `pipeline:planning` label |
| `pipeline-review.yml` | Test → Review | `pipeline:testing` label on PR |
| `pipeline-deploy.yml` | Merge → Deploy → Health check | `pipeline:deploying` label |
| `pipeline-rollback.yml` | Revert → Redeploy | `pipeline:rollback` label |
| `pipeline-retry.yml` | Retry failed stages | `pipeline:retrying` / `pipeline:failed` |

---

## Manual Commands

Comment on any pipeline issue to control it:

| Command | Effect |
|---------|--------|
| `/pipeline status` | Shows current stage, status, branch, attempt count |
| `/pipeline restart` | Strips all labels; close and reopen to re-trigger |
| `/pipeline resume` | Resumes from last completed stage |
| `/pipeline skip {stage}` | Skips specified stage |

---

## Failure Handling

### Automatic Retry
- Maximum 2 attempts per stage
- Retry increments attempt counter and re-dispatches
- After max attempts: marks `pipeline:failed`

### Test Failure Loop
- If tests fail, pipeline returns to implement stage
- Implementer pushes fixes to PR branch
- `synchronize` event re-triggers test stage (no manual intervention needed)

### Review Rejection Loop
- If review finds critical/high issues, requests changes on PR
- Implementer fixes → push triggers test → review again
- No infinite loop risk: attempt counter tracks cycles

### Deploy Failure
- Health check fails → `pipeline:rollback` label applied
- Rollback reverts merge commit and redeploys previous version

---

## How It Works End-to-End

```
1. Developer opens an issue describing work needed
2. Pipeline auto-validates, classifies, and routes (< 30s)
3. Planner creates branch + task breakdown (< 30s)
4. Copilot coding agent implements the plan autonomously
5. Agent creates PR with pipeline:testing label
6. Tests run → if pass → automated review runs
7. Review checks security, architecture, quality
8. If approved → PR merged → deployed to production
9. Health check confirms → issue closed → done! 🎉
```

**Total human intervention required for happy path: ZERO**

---

## Local Development

### Prerequisites
- .NET 9 SDK
- GitHub repository with Actions enabled
- Labels will be auto-created on first pipeline run

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
2. Watch the pipeline stages execute via issue comments
3. Use `/pipeline status` to check progress at any time

---

## Design Principles

1. **Zero-touch happy path** — No human approval gates for autonomous flow
2. **Observable state** — Every decision is explained in issue comments
3. **Parseable state** — Machine-readable JSON enables downstream stages to read upstream decisions
4. **Graceful degradation** — If Copilot agent can't be assigned, posts manual instructions
5. **Bounded retries** — Max 2 attempts prevents infinite loops
6. **Cross-workflow triggers** — Avoids GITHUB_TOKEN limitation by splitting stages across workflow files
7. **Chained steps** — Stages within the same workflow use step outputs instead of labels
8. **Conventional commits** — All agent commits follow `feat(scope): description\n\nRefs: #N`
