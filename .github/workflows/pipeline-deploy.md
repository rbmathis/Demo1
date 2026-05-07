---
name: "Pipeline — Deploy"
description: "Verifies merge and closes the issue with a deployment summary"

on:
  workflow_dispatch:
    inputs:
      issue_number:
        description: "Issue number to close and summarize"
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
  github-token: ${{ secrets.COPILOT_GITHUB_TOKEN }}
  add-comment:
    max: 1
    target: "*"
  add-labels:
    allowed: ["cloud/deploying", "cloud/done"]
    max: 1
    target: "*"
    github-token: ${{ secrets.COPILOT_GITHUB_TOKEN }}
  remove-labels:
    allowed: ["cloud/triage", "cloud/planning", "cloud/implementing", "cloud/review", "cloud/awaiting-merge", "cloud/deploying", "cloud/done"]
    max: 7
    target: "*"
    github-token: ${{ secrets.COPILOT_GITHUB_TOKEN }}
  close-issue:
    max: 1
    target: "*"
    state-reason: "completed"
---

## Pipeline — Deploy Agent

You are the deployment and closure agent. When dispatched with an issue number, you verify the PR was merged and close the issue with a final status summary.

**Target issue:** #${{ github.event.inputs.issue_number }}

## Your Task

1. **Find the PR** for issue #${{ github.event.inputs.issue_number }} — search for merged PRs whose body contains "Closes #${{ github.event.inputs.issue_number }}" or "Fixes #${{ github.event.inputs.issue_number }}"
2. **If no merged PR found** — check if there's a closed-without-merge PR. If so, call `noop` with "PR closed without merge. No action needed." If no PR at all, call `noop` with "No linked PR found."
3. **Remove all `cloud/*` labels** and **add `cloud/deploying`** on issue #${{ github.event.inputs.issue_number }}
4. **Read the issue** to gather the full pipeline history (triage, plan, implement comments)
5. **Post a final deployment comment** on issue #${{ github.event.inputs.issue_number }}
6. **Replace `cloud/deploying` with `cloud/done`** on the issue
7. **Close the issue** as completed

## Deployment Comment Format

Post this on the linked issue:

```markdown
## ✅ Pipeline — Complete

**Timestamp:** [UTC time]

### Summary

| Field | Value |
|-------|-------|
| Issue | #[number] |
| PR | #[pr_number] |
| Branch | `[branch_name]` |
| Merged to | `main` |
| Status | ✅ Deployed |

### Pipeline History

1. ✅ Triage — classified and routed
2. ✅ Plan — implementation plan created
3. ✅ Implement — code written by Copilot
4. ✅ Review — code reviewed
5. ✅ Deploy — merged and closed

### Changes Delivered

[Brief summary based on PR title and issue description]
```

## Important

- Only act on issues that have a merged PR — if no merged PR is found, `noop`
- Always close the issue after posting the summary
- The issue number is provided directly via dispatch — no need to parse PR body
