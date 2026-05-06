# Agentic Workflows Migration Guide

## Status: PR #85 Ready — Awaiting Secret Configuration

The migration from custom YAML pipelines to GitHub Agentic Workflows (gh-aw) is complete and pushed as [PR #85](https://github.com/rbmathis/Demo1/pull/85).

## One Secret Required

The `gh aw secrets bootstrap` analysis confirmed only **one secret** needs manual configuration:

### `COPILOT_GITHUB_TOKEN`

This is a fine-grained PAT that powers the Copilot AI engine in the workflows.

**Create it at:** <https://github.com/settings/personal-access-tokens/new>

**Configuration:**

- Token name: `Agentic Workflows Copilot`
- Expiration: 90 days (recommended for testing)
- Resource owner: Your personal account
- Repository access: **"Public repositories"** (required for Copilot Requests permission to appear)
- Permissions → **Copilot Requests: Read-only**

**Set it:**

```bash
gh aw secrets set COPILOT_GITHUB_TOKEN --value "github_pat_YOUR_TOKEN_HERE"
```

> **Note:** Must start with `github_pat_`. Classic PATs (`ghp_...`) are not supported.

### Auto-Managed Secrets (no action needed)

- `GH_AW_GITHUB_TOKEN` — Auto-provisioned by gh-aw infrastructure
- `GH_AW_GITHUB_MCP_SERVER_TOKEN` — Auto-provisioned by gh-aw infrastructure  
- `GH_AW_AGENT_TOKEN` — Used for `assign-to-agent`; same as COPILOT_GITHUB_TOKEN
- `GITHUB_TOKEN` — Standard GitHub Actions token

## Activation Steps

1. **Set the secret** (see above)
2. **Merge PR #85** into main
3. **Delete old YAML pipelines** (they trigger on same events and will conflict):

   ```bash
   git rm .github/workflows/pipeline-controller.yml
   git rm .github/workflows/pipeline-implement.yml
   git rm .github/workflows/pipeline-review.yml
   git rm .github/workflows/pipeline-deploy.yml
   git rm .github/workflows/pipeline-rollback.yml
   git rm .github/workflows/pipeline-retry.yml
   git commit -m "chore: remove old YAML pipelines (replaced by gh-aw)"
   git push
   ```

4. **Test the pipeline** — Open a new issue and watch:
   - `pipeline-triage.lock.yml` fires → classifies issue → applies `pipeline:planning` label
   - `pipeline-implement.lock.yml` fires → creates plan → assigns Copilot coding agent
   - Copilot opens a PR → `pipeline-review.lock.yml` fires → AI reviews the PR

## Architecture Overview

```text
Issue Opened
    │
    ▼
pipeline-triage.md ──► Classify & Label (pipeline:planning)
    │
    ▼
pipeline-implement.md ──► Create Plan ──► assign-to-agent: copilot
    │
    ▼
Copilot Coding Agent ──► Creates branch, implements, opens PR
    │
    ▼
pipeline-review.md ──► AI Code Review with inline comments
    │
    ▼
PR Merged ──► pipeline-deploy.md ──► Verify & Close Issue
    │
    ▼ (on failure)
pipeline-rollback.md ──► Analyze failure, coordinate rollback
pipeline-retry.md ──► Enforce budget, re-assign Copilot
```

## Editing Workflows

The `.md` files are the source of truth. After editing:

```bash
gh aw compile
git add .github/workflows/*.md .github/workflows/*.lock.yml
git commit -m "chore: update agentic workflows"
git push
```

## Reference

- [gh-aw Documentation](https://github.github.com/gh-aw/introduction/overview/)
- [Safe Outputs Reference](https://github.github.com/gh-aw/reference/safe-outputs/)
- [assign-to-agent](https://github.github.com/gh-aw/reference/safe-outputs/#assign-to-agent)
- [Auth & Tokens](https://github.github.com/gh-aw/reference/auth/#copilot_github_token)
