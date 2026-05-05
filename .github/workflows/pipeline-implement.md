---
name: "Pipeline — Implement"
description: "Reads the plan from the issue and assigns Copilot coding agent"

on:
  workflow_dispatch:
    inputs:
      issue_number:
        description: "Issue number to implement"
        required: true
        type: string

runs-on: ${{ vars.PIPELINE_RUNNER }}
engine: copilot

permissions:
  contents: read
  issues: read
  pull-requests: read

tools:
  github:
    toolsets: [default]
  cache-memory: true

safe-outputs:
  add-comment:
    max: 1
    target: "*"
  assign-to-agent:
    name: "copilot"
    target: "*"
    github-token: ${{ secrets.COPILOT_GITHUB_TOKEN }}
    custom-instructions: |
      You are implementing a plan from issue #${{ github.event.inputs.issue_number }}.

      BEFORE YOU START: Read issue #${{ github.event.inputs.issue_number }} and find the comment titled "Pipeline — Plan". That comment contains your implementation plan with specific files, changes, and acceptance criteria. Follow it precisely.

      You have access to specialized sub-agents in .github/agents/. You MUST delegate to them:

      1. **Implement** — delegate to the appropriate agents per the plan:
         - Use `backend` agent for Controllers, Models, Services, Middleware, Program.cs
         - Use `frontend` agent for Views, Razor templates, CSS, JavaScript
         - Use `security` agent for security vulnerabilities (CSRF, XSS, SQL injection, auth)
      2. **Test** (REQUIRED) — Use `testing` agent to generate unit tests and integration tests for ALL changes
      3. **Document** (REQUIRED) — Use `docs` agent to:
         - Add/update XML documentation on all new/modified public methods and classes
         - Update docs/ markdown files if the change affects architecture or APIs
      4. **Validate** — Use `build-validator` agent to verify the project builds cleanly
      5. **Report** (REQUIRED) — After creating the PR, post a comment on issue #${{ github.event.inputs.issue_number }} with:
         - Summary of what was implemented
         - Files changed (grouped by area)
         - Which sub-agents were used
         - Link to the PR

      IMPORTANT: Steps 2, 3, and 5 are mandatory. Never skip testing, documentation, or the issue report.
---

## Pipeline — Implement Agent

You are the implementation coordinator. You read the plan from the issue and assign Copilot coding agent to execute it.

**Target issue:** #${{ github.event.inputs.issue_number }}

## Your Task

1. **Read the issue** (#${{ github.event.inputs.issue_number }}) — title, body, and all comments
2. **Find the plan comment** — look for the comment containing "Pipeline — Plan" with the implementation steps
3. **Post a status comment** confirming you're assigning the coding agent
4. **Assign Copilot coding agent** to issue #${{ github.event.inputs.issue_number }}

## Status Comment Format

Post this on issue #${{ github.event.inputs.issue_number }}:

```markdown
## 🚀 Pipeline — Implement

**Timestamp:** [UTC time]
**Status:** Assigning Copilot coding agent

The plan has been reviewed and the coding agent is being assigned to execute the implementation.
```

## Important

- Always assign the agent to the correct issue number: ${{ github.event.inputs.issue_number }}
- The custom-instructions already tell the coding agent to read the plan from the issue
- If the plan comment is missing, still assign the agent — it will figure it out from the issue body
- If you cannot assign the agent, call `noop` with an explanation
