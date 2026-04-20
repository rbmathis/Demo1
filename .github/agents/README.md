# `.github/agents/` — Custom Copilot Agents

This folder contains **custom agent definitions** for GitHub Copilot. Each file defines a specialized AI persona that can be selected in VS Code's Copilot Chat, in the GitHub Copilot cloud agent on GitHub.com, or invoked as a subagent by the orchestrator.

## How Copilot Discovers and Activates These Agents

GitHub Copilot automatically scans the `.github/agents/` folder for any file ending in `.agent.md`. No registration or configuration is required — just drop a properly formatted file here and it appears in the agents dropdown in Copilot Chat.

**Activation paths:**

| Context | How to activate |
|---------|----------------|
| VS Code Copilot Chat | Select agent from the agents dropdown at the bottom of the Chat panel |
| GitHub.com cloud agent | Choose agent from the dropdown at [github.com/copilot/agents](https://github.com/copilot/agents) |
| Subagent invocation | The `orchestrator` agent calls other agents via the `agent` tool |
| Copilot CLI | Use `/agent <name>` or reference the agent in a prompt |

## Agent File Format

Each `.agent.md` file uses YAML frontmatter to declare its identity and capabilities, followed by Markdown instructions that define the agent's behavior:

```markdown
---
name: optional-display-name         # Defaults to filename without extension
description: "Required. Short description shown in the agents dropdown."
tools: ['read', 'edit', 'search']   # Tool access list (empty list disables ALL tools)
model: claude-sonnet-4-5 (copilot)  # Optional: pin to a specific model
target: vscode                       # Optional: 'vscode' or 'github-copilot' (default: both)
---

# Agent Name

Instructions, persona, and behavioral guidelines in Markdown...
```

### Tool Aliases Reference

| Alias | What it grants |
|-------|---------------|
| `read` | Read file contents |
| `edit` | Create and modify files |
| `search` | Search files and text (grep/glob) |
| `execute` | Run shell commands |
| `agent` | Invoke other custom agents as subagents |
| `web` | Fetch URLs and run web searches |
| `todo` | Create and manage task lists |

> **Important:** `tools: []` disables **all** tools. Omit the `tools` property entirely to grant access to all available tools.

## Agents in This Repository

### `orchestrator.agent.md`

**Tools:** `read`, `search`, `agent`

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

**Tools:** `edit`, `search`, `todos`, `fetch`

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

Issue triage assistant. Classifies issues by difficulty (easy / moderate / hard), suggests labels, identifies missing reproduction steps, and recommends assignees. Does not modify code. Activated manually in chat when triaging a new GitHub issue.

## References

- [GitHub Docs: Custom Agents Configuration](https://docs.github.com/en/copilot/reference/custom-agents-configuration)
- [GitHub Docs: Creating Custom Agents](https://docs.github.com/en/copilot/how-tos/use-copilot-agents/cloud-agent/create-custom-agents)
- [VS Code Docs: Custom Agents in VS Code](https://code.visualstudio.com/docs/copilot/customization/custom-agents)
