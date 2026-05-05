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
      ## YOUR MISSION

      Implement the plan from issue #${{ github.event.inputs.issue_number }}.

      ## STEP 0: READ THE PLAN (do this first)

      Read issue #${{ github.event.inputs.issue_number }} and find the comment titled "📋 Pipeline — Plan". That comment has your implementation plan with specific files, changes, and acceptance criteria. Follow it precisely.

      ## STEP 1: IMPLEMENT

      Use the specialized sub-agents in .github/agents/:
      - `backend` agent for Controllers, Models, Services, Middleware, Program.cs
      - `frontend` agent for Views, Razor templates, CSS, JavaScript
      - `security` agent for security vulnerabilities (CSRF, XSS, SQL injection, auth)

      ## STEP 2: TEST (mandatory — do not skip)

      Use `testing` agent to generate unit tests and integration tests for ALL changes.

      ## STEP 3: DOCUMENT (mandatory — do not skip)

      Use `docs` agent to:
      - Add/update XML documentation on all new/modified public methods and classes
      - Update docs/ markdown files if the change affects architecture or APIs

      ## STEP 4: VALIDATE

      Use `build-validator` agent to verify the project builds cleanly and tests pass.

      NOTE: Do NOT post an implementation report on the issue. The review pipeline handles that automatically.
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
