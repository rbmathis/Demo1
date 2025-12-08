# GitHub Copilot Agents Directory Structure

This document explains the proper directory structure for GitHub Copilot Custom Agents in this repository.

## Correct Structure ✅

```text
.github/
├── agents/                          # Custom agent definition files
│   ├── *.agent.md                   # Individual agent files with YAML frontmatter
│   ├── backend.agent.md             # Backend development expert
│   ├── frontend.agent.md            # Frontend & UI expert
│   ├── devops.agent.md              # CI/CD & deployment expert
│   ├── docs.agent.md                # Documentation expert
│   ├── security.agent.md            # Security & OWASP expert
│   ├── testing.agent.md             # Testing & QA expert
│   ├── orchestrator.agent.md        # Request routing coordinator
│   ├── code-reviewer.agent.md       # Code review expert
│   ├── build-validator.agent.md     # Build validation expert
│   ├── security-auditor.agent.md    # Security auditing expert
│   ├── doc-helper.agent.md          # Documentation helper
│   └── issue-helper.agent.md        # Issue triage helper
│
└── copilot/                          # General Copilot configuration
    ├── agents.yml                    # Agent metadata and inline definitions
    ├── examples/                     # Example agent interactions
    ├── skillsets/                    # Reusable agent skillsets
    └── workflows/                    # Agent workflow definitions
```

## Agent File Format

Each `.agent.md` file follows this structure:

```markdown
---
description: "Short description of the agent's role and expertise"
tools: []
---

# Agent Name

## Detailed instructions and guidelines...
```

## How It Works

1. **`.github/agents/*.agent.md`** - GitHub Copilot reads these files to discover available custom agents
2. **`.github/copilot/agents.yml`** - Optional centralized configuration for agent metadata
3. When both exist, `.agent.md` files take precedence

## Migration Notes

**Date:** 2025-12-08

**Change:** Moved agents from `.github/copilot-agents/` to `.github/agents/`

**Reason:** The `.github/copilot-agents/` directory is not a standard GitHub Copilot location. Custom agents must be in `.github/agents/` to appear in the Copilot agent selection dialog in IDEs.

**Files Converted:**
- `backend-agent.md` → `backend.agent.md`
- `frontend-agent.md` → `frontend.agent.md`
- `devops-agent.md` → `devops.agent.md`
- `docs-agent.md` → `docs.agent.md`
- `security-agent.md` → `security.agent.md`
- `testing-agent.md` → `testing.agent.md`
- `orchestrator-agent.md` → `orchestrator.agent.md`

All files were converted to include proper YAML frontmatter with `description` and `tools` fields.

## References

- [GitHub Docs: Custom Agents Configuration](https://docs.github.com/en/copilot/reference/custom-agents-configuration)
- [GitHub Docs: Creating Custom Agents](https://docs.github.com/en/copilot/how-tos/use-copilot-agents/coding-agent/create-custom-agents)
- [GitHub Blog: How to Write Great Agents](https://github.blog/ai-and-ml/github-copilot/how-to-write-a-great-agents-md-lessons-from-over-2500-repositories/)
