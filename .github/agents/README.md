# `.github/agents/` — Custom Copilot Agents

This folder contains **custom agent definitions** for GitHub Copilot. Each file defines a specialized AI persona that can be selected in VS Code's Copilot Chat, in the GitHub Copilot cloud agent on GitHub.com, or invoked as a subagent by the orchestrator.

## Autonomous SDLC Pipeline

This repository implements a **fully-autonomous AI-driven SDLC pipeline**. When an issue is opened, it flows automatically through 8 stages:

```
📥 Intake → 🏷️ Triage → 🔀 Route → 📋 Plan → 🔨 Implement → 🧪 Test → 👀 Review → 🚀 Deploy
```

Each stage posts detailed narrative logs and machine-readable state to the issue, providing full transparency of AI thinking and decisions. See [`pipeline-controller.yml`](../.github/workflows/pipeline-controller.yml) for the automation workflow.

**Manual commands:** Comment `/pipeline status`, `/pipeline resume`, `/pipeline restart`, or `/pipeline skip {stage}` on any issue.

## How Copilot Discovers and Activates These Agents

GitHub Copilot automatically scans the `.github/agents/` folder for any file ending in `.agent.md`. No registration or configuration is required — just drop a properly formatted file here and it appears in the agents dropdown in Copilot Chat.

**Activation paths:**

| Context | How to activate |
|---------|----------------|
| VS Code Copilot Chat | Select agent from the agents dropdown at the bottom of the Chat panel |
| GitHub.com cloud agent | Choose agent from the Copilot Chat interface on github.com (requires `target: github-copilot`) |
| Subagent invocation | The `orchestrator` agent calls other agents via the `agent` tool |

## Agent File Format

Each `.agent.md` file uses YAML frontmatter to declare its identity and capabilities, followed by Markdown instructions that define the agent's behavior:

```markdown
---
name: optional-display-name         # Defaults to filename without extension
description: "Required. Short description shown in the agents dropdown."
tools: ['read', 'edit', 'search']   # Tool access list (empty list disables ALL tools)
agents: ['*']                        # Optional: subagents this agent can invoke (* = all)
model: claude-sonnet-4-5 (copilot)  # Optional: pin to a specific model or array of models
target: vscode                       # Optional: 'vscode' or 'github-copilot' (default: both)
user-invocable: true                 # Optional: show in agents dropdown (default: true)
disable-model-invocation: false      # Optional: prevent subagent invocation (default: false)
argument-hint: "Describe your task"  # Optional: hint text shown in the chat input field
---

# Agent Name

Instructions, persona, and behavioral guidelines in Markdown...
```

### Tool Sets and Tools Reference

| Tool Set | What it grants | Individual tools |
|----------|---------------|------------------|
| `read` | Read files in your workspace | `read/readFile`, `read/problems`, `read/terminalLastCommand`, `read/terminalSelection`, `read/getNotebookSummary`, `read/readNotebookCellOutput` |
| `edit` | Create and modify files | `edit/createFile`, `edit/editFiles`, `edit/createDirectory`, `edit/editNotebook` |
| `search` | Search files and text | `search/codebase`, `search/textSearch`, `search/fileSearch`, `search/listDirectory`, `search/usages`, `search/changes` |
| `execute` | Run shell commands and tasks | `execute/runInTerminal`, `execute/getTerminalOutput`, `execute/createAndRunTask`, `execute/runNotebookCell`, `execute/testFailure` |
| `agent` | Invoke other custom agents as subagents | `agent/runSubagent` |
| `web` | Fetch URLs and access web content | `web/fetch` |
| `todos` | Create and manage task lists | *(standalone tool)* |
| `browser` | (Experimental) Interact with integrated browser | Navigate, screenshot, click, type, hover, drag |

> **Important:** `tools: []` disables **all** tools. Omit the `tools` property entirely to grant access to all available tools.
>
> You can reference entire tool sets (e.g., `search`) or individual tools (e.g., `search/usages`). Tool sets include MCP tools and extension tools in addition to built-in tools.

## Agents in This Repository

### `orchestrator.agent.md`

**Tools:** `read`, `search`, `agent`
**Agents:** all specialized agents
**Argument hint:** "Describe your task and I'll route it to the right specialist"

The primary entry point for complex or ambiguous requests. Analyzes user intent, determines the correct specialist(s) to handle the task, and routes work accordingly. Uses the `agent` tool to invoke subagents. Choose this agent when you are unsure which specialist to use or when a task spans multiple domains (e.g., "add a feature with tests and documentation").

---

### `backend.agent.md`

**Tools:** `read`, `edit`, `search`, `execute`

Expert in ASP.NET Core MVC backend development. Handles work in `Controllers/`, `Models/`, `Services/`, `Middleware/`, and `Program.cs`. Applies MVC patterns, dependency injection, proper `IActionResult` usage, async/await, `[Authorize]` attributes, and `ModelState` validation. Choose this agent when changing C# server-side code.

---

### `frontend.agent.md`

**Tools:** `read`, `edit`, `search`

Expert in Razor Views, layouts, CSS, and JavaScript. Handles files in `Views/`, `wwwroot/css/`, `wwwroot/js/`, and static assets. Applies strongly-typed view patterns (`@model`), tag helpers, Bootstrap integration, and client-side validation. Choose this agent for UI changes, layout updates, or view logic.

---

### `security.agent.md`

**Tools:** `read`, `edit`, `search`, `execute`

Expert in authentication, authorization, OWASP Top 10 mitigations, security headers, HTTPS enforcement, secrets management, and CSRF protection. Works across the whole codebase. Choose this agent when adding authentication, hardening security, or implementing compliance requirements.

---

### `security-auditor.agent.md`

**Tools:** `read`, `search`, `execute`

Read-focused security scanning agent. Audits the codebase for vulnerabilities using a structured OWASP checklist — covering authentication, input validation, SQL injection, secrets exposure, and HTTPS configuration. Does **not** modify code; produces a security report with prioritized remediation steps. Choose this agent for security reviews and audits.

---

### `testing.agent.md`

**Tools:** `read`, `edit`, `search`, `execute`

Expert in xUnit, Moq, WebApplicationFactory, FluentAssertions, and Playwright. Writes and improves unit tests, integration tests, and end-to-end tests. Works within the `tests/` directory. Choose this agent to add test coverage, fix failing tests, or set up test infrastructure.

---

### `devops.agent.md`

**Tools:** `read`, `edit`, `search`, `execute`

Expert in GitHub Actions workflows, Docker, deployment pipelines, environment configuration, and build automation. Works in `.github/workflows/`, `Dockerfile`, `docker-compose.yml`, and `Properties/launchSettings.json`. Choose this agent for CI/CD changes, pipeline additions, or deployment configuration.

---

### `docs.agent.md`

**Tools:** `edit`, `search`, `todos`, `agent`, `web`
**Agents:** `code-reviewer`, `security-auditor`

Expert in Markdown documentation, XML doc comments, README files, Swagger/OpenAPI, and architecture diagrams (Mermaid). Works in `docs/`, project root, and source files. Choose this agent to write or update documentation, generate XML comments, or improve README clarity.

---

### `code-reviewer.agent.md`

**Tools:** `read`, `search` *(read-only — no file modifications)*

Read-only code quality reviewer. Checks MVC architectural patterns, dependency injection usage, async patterns, security practices, and code maintainability. Produces detailed review feedback without modifying any files. Choose this agent to get a code review before submitting a PR.

---

### `build-validator.agent.md`

**Tools:** `read`, `search`, `execute` *(no file edits)*

Analyzes `.csproj` files for correct SDK, target framework, nullable settings, and NuGet package health. Runs builds and reports dependency vulnerabilities. Does not modify project files. Choose this agent to validate build configuration or diagnose dependency issues.

---

### `issue-helper.agent.md`

**Tools:** `read`, `search` *(read-only)*

SDLC pipeline intake and triage agent. Validates issue quality (intake stage) and classifies issues by type, difficulty, priority, and scope (triage stage). Posts narrative reasoning and machine-readable state to issues. Drives the first two stages of the automated pipeline.

---

## Pipeline Agents

These agents form the automated SDLC pipeline. They are typically invoked by the `pipeline-controller.yml` workflow or by the orchestrator, not directly by users.

### `pipeline.agent.md`

**Tools:** `read`, `search`, `execute`, `agent`, `web`
**Agents:** all pipeline-stage agents

State machine controller for the SDLC pipeline. Manages stage transitions, retry logic, failure escalation, and manual override commands (`/pipeline status`, `/pipeline resume`, `/pipeline restart`, `/pipeline skip`).

---

### `planner.agent.md`

**Tools:** `read`, `search`, `todos`, `agent`
**Agents:** `backend`, `frontend`, `security`, `devops`

Decomposes triaged issues into detailed, file-level implementation plans. Researches the codebase for patterns and conventions, documents design decisions, creates feature branches, and produces task lists for the implementer.

---

### `implementer.agent.md`

**Tools:** `read`, `edit`, `search`, `execute`, `agent`, `todos`
**Agents:** `backend`, `frontend`, `security`, `devops`, `docs`

Execution engine that takes plans and produces working code. Delegates to specialist agents, posts progress updates, commits with conventional messages, and creates PRs linked to issues.

---

### `reviewer.agent.md`

**Tools:** `read`, `search`, `agent`
**Agents:** `security-auditor`, `code-reviewer`, `build-validator`

Autonomous PR reviewer covering architecture, security, code quality, test coverage, and documentation. Delegates specialized checks to expert agents. Makes pass/fail decisions with detailed findings. Max 2 review cycles before human escalation.

---

### `deployer.agent.md`

**Tools:** `read`, `search`, `execute`, `web`

Manages deployment lifecycle: PR merge, CI/CD monitoring, post-deployment health checks, and auto-rollback on failure. Verifies HTTP health, error rates, and response times before marking deployment complete.

## Pipeline Workflows

The SDLC pipeline is driven by GitHub Actions workflows that trigger on label changes:

| Workflow | Trigger | Stages |
|----------|---------|--------|
| [`pipeline-controller.yml`](../workflows/pipeline-controller.yml) | Issue opened / labeled | Intake → Triage → Route |
| [`pipeline-implement.yml`](../workflows/pipeline-implement.yml) | `pipeline:planning` / `pipeline:implementing` | Plan → Implement |
| [`pipeline-review.yml`](../workflows/pipeline-review.yml) | `pipeline:testing` on PR / `pipeline:reviewing` on issue | Test → Review |
| [`pipeline-deploy.yml`](../workflows/pipeline-deploy.yml) | `pipeline:deploying` | Deploy (merge + health check) |
| [`pipeline-rollback.yml`](../workflows/pipeline-rollback.yml) | `pipeline:rollback` | Rollback (revert + redeploy) |
| [`pipeline-retry.yml`](../workflows/pipeline-retry.yml) | `pipeline:retrying` / `pipeline:failed` | Retry orchestration + failure notification |

**Supporting workflows (pipeline-aware):**
| Workflow | Pipeline Enhancement |
|----------|---------------------|
| [`dotnet.yml`](../workflows/dotnet.yml) | Reports CI status to linked pipeline issues |
| [`copilot-agents.yml`](../workflows/copilot-agents.yml) | Validates PR conventions for pipeline branches |

### Pipeline Labels

| Label | Stage | Color |
|-------|-------|-------|
| `pipeline:intake` | Intake | Purple |
| `pipeline:triage` | Triage | Purple |
| `pipeline:planning` | Plan | Blue |
| `pipeline:implementing` | Implement | Blue |
| `pipeline:testing` | Test | Yellow |
| `pipeline:reviewing` | Review | Yellow |
| `pipeline:deploying` | Deploy | Green |
| `pipeline:done` | Complete | Green |
| `pipeline:retrying` | Retry in progress | Light yellow |
| `pipeline:rollback` | Rolling back | Red |
| `pipeline:rolled-back` | Rollback complete | Red |
| `pipeline:blocked` | Needs human | Red |
| `pipeline:failed` | Failed (exhausted retries) | Red |

### Retry Strategy

Each stage gets **2 automatic retry attempts**. On each retry, the pipeline:
1. Posts a narrative explaining the retry strategy
2. Increments the attempt counter in state
3. Re-triggers the failed stage with fresh context

After 2 failures: applies `pipeline:failed`, posts a comprehensive diagnosis, and requests human intervention.

## References

- [VS Code Docs: Custom Agents in VS Code](https://code.visualstudio.com/docs/copilot/customization/custom-agents)
- [VS Code Docs: Custom Agent File Structure](https://code.visualstudio.com/docs/copilot/customization/custom-agents#_custom-agent-file-structure)
- [VS Code Docs: Use Tools with Agents](https://code.visualstudio.com/docs/copilot/agents/agent-tools)
- [VS Code Docs: Chat Tools Reference](https://code.visualstudio.com/docs/copilot/reference/copilot-vscode-features#_chat-tools)
- [GitHub Docs: Create Custom Agents for Organizations](https://docs.github.com/en/copilot/how-tos/use-copilot-agents/coding-agent/create-custom-agents)
