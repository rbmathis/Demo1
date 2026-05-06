# AI-Driven Software Development Lifecycle — Local Mode (AI-SDLC-LOCAL)

A fully-autonomous, AI-powered SDLC pipeline that runs **locally** via Copilot CLI (`copilot` command) and VS Code Copilot Chat. Issues flow from intake to deployment with minimal human intervention. The local agents use `local/*` labels to track progress — completely isolated from the cloud `pipeline/*` labels that trigger GitHub Actions workflows.

---

## Architecture Overview

```text
┌──────────────────────────────────────────────────────────────────────┐
│  LOCAL MACHINE (Copilot CLI / VS Code Chat)                          │
│                                                                      │
│   "triage issue 135"                                                 │
│       │                                                              │
│       ▼                                                              │
│   ┌──────────┐  delegates  ┌──────────┐  delegates  ┌───────────┐  │
│   │  TRIAGE  │────────────▶│   PLAN   │────────────▶│ IMPLEMENT │  │
│   └──────────┘             └──────────┘             └───────────┘  │
│   classifies issue,        analyzes codebase,        delegates to   │
│   posts triage comment,    posts plan comment,       specialists,   │
│   adds local/triage        creates branch            creates PR     │
│                                                         │           │
│                                                         ▼           │
│                            ┌──────────┐             ┌──────────┐   │
│                            │  DEPLOY  │◀────────────│  REVIEW  │   │
│                            └──────────┘  approved   └──────────┘   │
│                            squash merges,            multi-agent    │
│                            closes issue              code review    │
│                                                                      │
└──────────────────────────────────────────────────────────────────────┘
```

**Chaining mechanism:** The `pipeline` controller agent delegates to each stage agent sequentially. No label-based triggers — the controller handles all orchestration in-process.

**State tracking:** Each stage posts a structured comment on the GitHub issue. The issue is the state file — same as the cloud pipeline.

---

## How to Invoke

> **TL;DR — Always use `@pipeline run`.** It's the controller that runs all stages for you.

### Full Pipeline (the normal way)

```bash
# Copilot CLI:
copilot "@pipeline run issue 135"
```

```text
# VS Code Copilot Chat:
@pipeline run issue 135
```

The **`@pipeline` agent** is the single entry point. It auto-chains all 5 stages in order:
triage → plan → implement → review → deploy.

You don't need to invoke `@triage` directly — `@pipeline` handles everything.

### Running a Single Stage (advanced)

If you need to re-run or skip to a specific stage (e.g., after a failure):

```bash
copilot "@triage triage issue 135"      # classify only
copilot "@plan plan issue 135"          # plan only (reads triage comment)
copilot "@implement implement issue 135" # implement only (reads plan comment)
copilot "@review review PR 142"          # review a specific PR
copilot "@deploy deploy PR 142"          # merge and close
```

This is rarely needed — the pipeline controller handles sequencing, retries, and error reporting automatically.

---

## Pipeline Stages

### Stage 1: Triage

| Attribute | Value |
|-----------|-------|
| **Agent** | `triage` |
| **Tools** | read, search, github |
| **Label** | `local/triage` → `local/planning` |
| **Output** | Classification comment, type labels |

The triage agent:
- Reads the issue title and body via GitHub API
- Applies `local/triage` label (removes other `local/*` labels)
- Classifies by type, difficulty, priority, and scope
- Determines which specialist agents are needed
- Posts a structured triage comment
- Applies classification labels (bug, enhancement, feature, etc.)
- Replaces `local/triage` with `local/planning`

### Stage 2: Plan

| Attribute | Value |
|-----------|-------|
| **Agent** | `plan` |
| **Tools** | read, search, github, agent |
| **Sub-agents** | backend, frontend, security, devops |
| **Label** | `local/planning` → `local/implementing` |
| **Output** | Detailed plan comment, feature branch created |

The plan agent:
- Reads the issue and triage comment
- Researches the codebase (finds patterns, conventions, existing implementations)
- Creates a detailed implementation plan with:
  - Branch name (`feat/issue-{N}-{slug}`)
  - File-level tasks with agent assignments
  - Design decisions and rationale
  - Acceptance criteria
- Creates the feature branch from main
- Posts the plan as a structured comment
- Replaces `local/planning` with `local/implementing`

### Stage 3: Implement

| Attribute | Value |
|-----------|-------|
| **Agent** | `implement` |
| **Tools** | read, edit, search, execute, github, agent, todos |
| **Sub-agents** | backend, frontend, security, testing, docs, devops, build-validator |
| **Label** | `local/implementing` (no change until PR created) |
| **Output** | Code committed, PR opened |

The implement agent:
- Reads the plan comment from the issue
- Checks out the feature branch
- Executes tasks in dependency order, delegating to specialists:
  - `backend` — Controllers, Models, Services, Middleware
  - `frontend` — Views, Razor, CSS, JavaScript
  - `security` — security hardening
  - `testing` — unit + integration tests
  - `docs` — documentation
- Validates build: `dotnet build --configuration Release`
- Runs tests: `dotnet test tests/Demo1.UnitTests`
- Commits with conventional commit messages
- Creates a Pull Request referencing the issue
- Posts a status comment on the issue

### Stage 4: Review

| Attribute | Value |
|-----------|-------|
| **Agent** | `review` |
| **Tools** | read, search, github, agent |
| **Sub-agents** | security-auditor, code-reviewer, build-validator |
| **Label** | `local/implementing` → `local/review` |
| **Output** | PR review posted |

The review agent:
- Finds the PR (from issue number or PR number)
- Updates label to `local/review`
- Reads the PR diff and all changed files
- Delegates review dimensions:
  - `security-auditor` — OWASP, CSRF, XSS, injection
  - `code-reviewer` — MVC patterns, SOLID, naming, quality
  - `build-validator` — build health
- Reviews directly: architecture, test coverage, documentation
- Posts consolidated review verdict on the PR
- Makes approve/request-changes decision

**If changes requested:** The pipeline controller can loop back to implement (max 2 cycles).

### Stage 5: Deploy

| Attribute | Value |
|-----------|-------|
| **Agent** | `deploy` |
| **Tools** | read, search, execute, github, web |
| **Label** | `local/review` → `local/done` |
| **Output** | PR merged, issue closed |

The deploy agent:
- Verifies PR is approved and CI checks pass
- Squash merges to main
- Deletes the feature branch
- Updates label to `local/done`
- Posts deployment summary on the issue
- Closes the issue as completed

---

## Labels (Local vs Cloud)

Local agents use `local/*` labels to avoid triggering GitHub Actions workflows:

| Label | Stage | Applied By |
|-------|-------|------------|
| `local/triage` | Being classified | Triage |
| `local/planning` | Plan in progress | Triage → Plan |
| `local/implementing` | Code being written | Plan → Implement |
| `local/review` | PR under review | Review |
| `local/done` | Pipeline complete | Deploy |

The cloud pipeline uses `pipeline/triage-requested` as its trigger — completely separate namespace.

---

## Agent Roster

### Pipeline Agents (orchestration)

| Agent | File | Role |
|-------|------|------|
| `pipeline` | `pipeline.agent.md` | Controller — auto-chains all stages |
| `triage` | `triage.agent.md` | Classifies issues |
| `plan` | `plan.agent.md` | Creates implementation plans |
| `implement` | `implement.agent.md` | Writes code via specialists |
| `review` | `review.agent.md` | Multi-dimensional code review |
| `deploy` | `deploy.agent.md` | Merges and closes |

### Specialist Agents (delegated work)

| Agent | File | Expertise |
|-------|------|-----------|
| `backend` | `backend.agent.md` | Controllers, Models, Services, Middleware |
| `frontend` | `frontend.agent.md` | Views, Razor, CSS, JavaScript |
| `security` | `security.agent.md` | Auth, headers, OWASP |
| `testing` | `testing.agent.md` | Unit + integration tests |
| `docs` | `docs.agent.md` | Documentation |
| `devops` | `devops.agent.md` | CI/CD, Actions, Docker |
| `build-validator` | `build-validator.agent.md` | Build health |
| `code-reviewer` | `code-reviewer.agent.md` | Code quality review |
| `security-auditor` | `security-auditor.agent.md` | Security scanning |

---

## Issue as State File

Same as the cloud pipeline — the issue tracks all pipeline state via structured comments:

```text
Issue #135: "Add user preferences endpoint"
│
├── 🏷️ Pipeline — Triage                (classification table)
├── 📋 Pipeline — Plan                  (file-level tasks, branch name)
├── 🔨 Pipeline — Implement             (PR link, commits summary)
├── 🔍 Pipeline — Review                (verdict, findings)
└── 🚀 Pipeline — Deploy                (merge confirmation, summary)
```

---

## Differences from Cloud Pipeline

| Aspect | Cloud (AI-SDLC-CLOUD) | Local (AI-SDLC-LOCAL) |
|--------|----------------------|----------------------|
| **Runtime** | GitHub Actions runners | Local machine (Copilot CLI) |
| **Trigger** | `pipeline/triage-requested` label | `copilot "triage issue N"` |
| **Chaining** | `dispatch-workflow` safe outputs | In-process agent delegation |
| **Labels** | None (dispatch-based) | `local/*` for progress tracking |
| **Human gates** | Approve workflow run, merge PR, close issue | Merge PR (optional: review before merge) |
| **Tools** | gh-aw sandbox (limited MCP) | Full GitHub MCP server, local filesystem |
| **Speed** | ~15-25 min end-to-end | ~5-15 min (no queue wait) |
| **Cost** | GitHub Actions minutes | Copilot subscription only |

---

## Failure Handling

- If any stage fails, the pipeline controller stops and reports the error
- The issue retains all comments posted so far — full audit trail
- Re-run by invoking the pipeline again: `copilot "triage issue 135"`
- Or resume from a specific stage: `copilot "@implement implement issue 135"`

If code review requests changes:
- The pipeline controller loops back to implement (max 2 review cycles)
- After 2 failed reviews, it stops and reports for human intervention

---

## Prerequisites

- [Copilot CLI](https://docs.github.com/en/copilot/github-copilot-in-the-cli) installed and authenticated
- .NET 9 SDK
- Git configured with push access to the repository
- GitHub CLI (`gh`) authenticated

---

## End-to-End Flow

```text
1. Developer opens an issue describing work needed
2. Developer runs: copilot "@pipeline run issue 135"
3. Triage classifies and posts comment                     (~30s)
4. Plan researches codebase and posts plan                 (~1-2 min)
5. Implement writes code, tests, docs, creates PR         (~3-8 min)
6. Review runs multi-agent code review                     (~1-2 min)
7. Deploy merges PR and closes issue                       (~30s)
```

**Human touchpoints (1):** Optionally review the PR before the deploy stage merges it. The pipeline can run fully autonomous if desired.
