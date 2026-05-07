---
name: "Pipeline — Deliver"
description: "Merges approved PRs and dispatches deploy for issue closure"

on:
  workflow_dispatch:
    inputs:
      issue_number:
        description: "Issue number whose PR should be merged"
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

safe-outputs:
  add-comment:
    max: 1
    target: "*"
  add-labels:
    allowed: ["pipeline/delivering"]
    max: 1
    target: "*"
    github-token: ${{ secrets.COPILOT_GITHUB_TOKEN }}
  remove-labels:
    allowed: ["pipeline/triage", "pipeline/planning", "pipeline/implementing", "pipeline/review", "pipeline/awaiting-merge", "pipeline/documenting", "pipeline/delivering", "pipeline/deploying", "pipeline/done"]
    max: 9
    target: "*"
    github-token: ${{ secrets.COPILOT_GITHUB_TOKEN }}
  merge-pull-request:
    max: 1
    method: squash
    target: "*"
    github-token: ${{ secrets.COPILOT_GITHUB_TOKEN }}
  delete-branch:
    max: 1
    target: "*"
    github-token: ${{ secrets.COPILOT_GITHUB_TOKEN }}
  dispatch-workflow: [pipeline-deploy]
---

## Pipeline — Deliver Agent

You are the delivery agent. When dispatched with an issue number, you find the approved PR, squash-merge it to main, delete the feature branch, and dispatch deploy.

**Target issue:** #${{ github.event.inputs.issue_number }}

## Your Task

1. **Find the PR** for issue #${{ github.event.inputs.issue_number }} — search for open PRs whose body contains "Closes #${{ github.event.inputs.issue_number }}" or "Fixes #${{ github.event.inputs.issue_number }}"
2. **If no approved PR found** — call `noop` with "No approved PR found for issue #${{ github.event.inputs.issue_number }}. Cannot deliver."
3. **Remove all `pipeline/*` labels** and **add `pipeline/delivering`** on issue #${{ github.event.inputs.issue_number }}
4. **Verify pre-merge conditions:**
   - PR review status is "approved"
   - CI/CD status checks are passing (or no required checks)
   - No merge conflicts
5. **Squash-merge the PR** to main
6. **Delete the feature branch**
7. **Post a delivery comment** on issue #${{ github.event.inputs.issue_number }} (format below)
8. **Dispatch `pipeline-deploy`** with input `issue_number` set to `${{ github.event.inputs.issue_number }}`

## Pre-Merge Verification

Before merging, confirm:
- PR has at least one approving review
- No `REQUEST_CHANGES` reviews outstanding
- CI checks pass (if any required)
- No merge conflicts with main

If any check fails, post a comment explaining why delivery is blocked and call `noop`.

## Delivery Comment Format

Post this on issue #${{ github.event.inputs.issue_number }}:

```markdown
## 🚀 Pipeline — Delivered

**Timestamp:** [UTC time]
**PR:** #[pr_number]
**Branch:** `[branch_name]` → `main`
**Method:** Squash merge

### Pre-Merge Checks

| Check | Status |
|-------|--------|
| Review Approved | ✅ |
| CI Checks | ✅ |
| No Conflicts | ✅ |

Merge commit: `[sha]`
Branch `[branch_name]` deleted.
```

## Important

- Only merge PRs that are approved — never force-merge
- Always squash merge to keep main history clean
- Always delete the feature branch after merge
- Always dispatch `pipeline-deploy` after successful merge
- If merge fails, post an error comment and do NOT dispatch deploy
