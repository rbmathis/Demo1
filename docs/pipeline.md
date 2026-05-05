# SDLC Pipeline Operations Guide

This document describes how to use and operate the fully-autonomous AI SDLC pipeline.

## Overview

The pipeline automatically processes GitHub Issues through 8 stages:

```
📥 Intake → 🏷️ Triage → 🔀 Route → 📋 Plan → 🔨 Implement → 🧪 Test → 👀 Review → 🚀 Deploy
```

Each stage is executed by specialized AI agents that post detailed reasoning and decisions as issue comments.

## How It Works

1. **Open an issue** with a clear title, description, and acceptance criteria
2. The pipeline automatically validates, classifies, and routes the issue
3. AI agents plan the implementation, write code, run tests, review, and deploy
4. Every decision is logged as a narrative comment on the issue
5. On success, the issue is closed and the feature is live

## Pipeline Commands

Comment these on any pipeline-tracked issue:

| Command | Action |
|---------|--------|
| `/pipeline status` | Show current pipeline state |
| `/pipeline resume` | Resume from last successful stage |
| `/pipeline restart` | Reset and restart from intake |
| `/pipeline skip {stage}` | Skip a specific stage |

## Writing Pipeline-Ready Issues

For best results, include:

- **Clear title** (6+ characters, descriptive)
- **Detailed description** (50+ characters explaining the requirement)
- **Acceptance criteria** (use words like "should", "must", "expected")
- **Context** (environment, related files, prior art)

Issues missing information will receive a `needs-info` label with specific guidance on what to add.

### Opting Out

Add `[skip pipeline]` or `[no pipeline]` anywhere in the issue body to bypass automation.

## Stage Details

### 1. Intake (`issue-helper` agent)

- Validates issue completeness
- Checks for title quality, description length, acceptance criteria, context
- Posts quality score (e.g., "4/4 checks passed")
- Blocks with `needs-info` if requirements are insufficient

### 2. Triage (`issue-helper` agent)

- Classifies type: bug, enhancement, refactor, security
- Estimates difficulty: easy, moderate, hard
- Assigns priority: low, medium, high, critical
- Identifies scope areas (Controllers, Models, Views, etc.)

### 3. Route (`orchestrator` agent)

- Determines which specialist agents are needed
- Explains WHY each agent was (or wasn't) assigned
- Sets execution order (parallel vs sequential)
- Documents decision rationale

### 4. Plan (`planner` agent)

- Researches codebase for patterns and conventions
- Creates detailed task list per agent
- Creates feature branch: `feat/issue-{N}-{slug}`
- Documents architectural decisions

### 5. Implement (`implementer` agent)

- Executes plan by delegating to specialists
- Posts progress updates for each task
- Uses conventional commits referencing the issue
- Creates PR with `Closes #N` in the body

### 6. Test (`testing` + `build-validator` agents)

- Builds the project in Release configuration
- Runs full test suite with coverage collection
- Reports results in structured format
- Can auto-fix simple test failures (2 retry budget)

### 7. Review (`reviewer` agent)

- Multi-dimensional review: architecture, security, quality, tests, docs
- Delegates to `security-auditor`, `code-reviewer`, `build-validator`
- Posts findings with severity levels
- Approves or requests changes (max 2 review cycles)

### 8. Deploy (`deployer` agent)

- Squash-merges PR to main
- Builds for production
- Deploys to Azure (App Service / Container Apps)
- Runs health checks
- Auto-rolls back on failure

## Failure Handling

### Retry Logic

- Each stage gets **2 automatic retry attempts**
- On failure: AI diagnoses root cause and tries alternative approach
- On retry exhaustion: `pipeline:failed` label + human notification

### Rollback

If deployment health checks fail:
1. Merge commit is automatically reverted on main
2. Previous version is redeployed
3. Health checks verify restoration
4. Issue receives `pipeline:rolled-back` + `needs-investigation` labels

### Manual Intervention

When `pipeline:failed` or `pipeline:blocked` labels appear:
1. Review the diagnostic comments on the issue
2. Fix the underlying problem
3. Use `/pipeline resume` to continue, or `/pipeline restart` to start fresh

## Monitoring

### Issue Labels

Track pipeline progress via labels:
- Purple labels = early stages (intake, triage)
- Blue labels = planning/implementation
- Yellow labels = testing/review
- Green labels = deploying/done
- Red labels = failure/blocked/rollback

### CI Integration

The `dotnet.yml` workflow automatically reports build/test status to the linked pipeline issue for pipeline PRs (branches matching `feat/issue-*`).

## Architecture

### State Tracking

Pipeline state is stored as **JSON in issue comments** (inside `<details>` blocks). Each stage reads the previous stage's state and posts its own.

### Workflow Triggers

All stage transitions are driven by **label changes**. When a workflow completes a stage, it:
1. Removes the current stage label
2. Applies the next stage label
3. This triggers the next workflow

### Agent Collaboration

```
pipeline-controller.yml
    ├── issue-helper (intake + triage)
    ├── orchestrator (route)
    │
pipeline-implement.yml
    ├── planner (plan)
    ├── implementer → backend, frontend, security, devops, docs
    │
pipeline-review.yml
    ├── testing + build-validator (test)
    ├── reviewer → security-auditor, code-reviewer, build-validator
    │
pipeline-deploy.yml
    ├── deployer → devops
    │
pipeline-rollback.yml
    └── deployer (rollback mode)
```
