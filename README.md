# ASP.NET Core MVC Project

[![Build & Test](https://github.com/rbmathis/Demo1/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/rbmathis/Demo1/actions/workflows/dotnet.yml)
[![Deploy](https://github.com/rbmathis/Demo1/actions/workflows/deploy.yml/badge.svg?branch=main)](https://github.com/rbmathis/Demo1/actions/workflows/deploy.yml)
[![Copilot Agents](https://github.com/rbmathis/Demo1/actions/workflows/copilot-agents.yml/badge.svg?branch=main)](https://github.com/rbmathis/Demo1/actions/workflows/copilot-agents.yml)
[![Targeted Coverage](https://img.shields.io/badge/Controllers%20Coverage-%E2%89%A5%2090%25-ff69b4?logo=codecov&logoColor=white)](coverage/report/Summary.txt)
[![Playwright Ready](https://img.shields.io/badge/Playwright-smoke%20tested-3fb950?logo=playwright&logoColor=white)](tests/Demo1.PlaywrightTests)

Welcome to the glam corner of .NET 9 where MVC meets main-character energy. This repo is our stage for building production-ready web apps with a confident strut, buttery-smooth tooling, and telemetry that keeps the spotlight exactly where we want it.

## Demo Focus

This repository is also a hands-on demo of autopilot-style agentic delivery with GitHub Copilot. It shows how the same repo can run an AI-driven SDLC in two modes:

- **Local autopilot delivery** via VS Code Copilot Chat and the local controller/agent set documented in [AI-SDLC-LOCAL.md](AI-SDLC-LOCAL.md)
- **Cloud autopilot delivery** via GitHub Agentic Workflows and the cloud pipeline documented in [AI-SDLC-CLOUD.md](AI-SDLC-CLOUD.md)

If you're here to explore the agentic workflow itself, start with these guides:

- [AI-SDLC-LOCAL.md](AI-SDLC-LOCAL.md) for local, in-editor autopilot execution
- [AI-SDLC-CLOUD.md](AI-SDLC-CLOUD.md) for cloud-dispatched autopilot orchestration

## Highlights

- **Turnkey pipelines** – CI keeps the runway green with linting, unit tests, and smoke checks.
- **Feature-flag finesse** – Azure App Configuration flips features faster than a costume change.
- **Observability drip** – Application Insights, custom telemetry, and coverage gates keep the receipts.
- **Rate limiting** – IP-based request throttling with configurable limits and informative response headers. See [`docs/configuration.md`](docs/configuration.md#rate-limiting) for details.
- **Achievement system** – Earn badges by exploring the site. A `Channel<T>` + `BackgroundService` pattern tracks actions asynchronously with zero impact on page loads. Visit `/Achievement/TrophyCase` to see your progress.
- **Component showcase** – Browsable catalog of UI components with isolated iframe previews, copy-to-clipboard markup snippets, and category filtering. Visit `/ComponentShowcase` to explore. See [`docs/component-showcase.md`](docs/component-showcase.md) for details.
- **Dark-launch delivery** – The AI-SDLC pipeline ships new behavior behind default-off feature flags. Activation is human-controlled via local config or Azure App Configuration. See [`docs/feature-flag-rollout-contract.md`](docs/feature-flag-rollout-contract.md).

## Getting Started

### Prerequisites

- .NET 10 SDK or later
- Visual Studio Code with C# Dev Kit extension

### Running the Application

1. **Build the project:**

   ```bash
   dotnet build
   ```

2. **Run the project:**

   ```bash
   dotnet run
   ```

3. **Run with hot reload (development):**

   ```bash
   dotnet watch run
   ```

The application will be available at `https://localhost:7XXX` and `http://localhost:5XXX` (ports will be displayed in the terminal).

### Docker

```bash
docker build -t demo1 .
docker run -p 5555:8080 demo1
docker compose up
docker compose --profile dev up
```

- `docker compose up` starts the app + Redis stack.
- `docker compose --profile dev up` starts the hot-reload development profile on `http://localhost:5556` (while the standard service remains on `http://localhost:5555`).
- CI Docker image build/push is handled in [`.github/workflows/deploy.yml`](.github/workflows/deploy.yml); image push is gated by the `GHCR_TOKEN` secret.

### VS Code Tasks

This project includes VS Code tasks that you can run via:

- **Ctrl+Shift+P** → "Tasks: Run Task"
- Available tasks: `build`, `run`, `watch`

### Versioning

This project uses automatic semantic versioning based on PR merge activity:

- **Current Version**: See [`VERSION`](VERSION) file in repo root
- **Version Format**: `MAJOR.MINOR.PATCH` (e.g., `0.27.9`)
- **Automatic Updates**: When a PR is merged to `main`, the version is automatically bumped based on PR body content:
  - **Major bump** (`1.0.0`): PR body contains `BREAKING CHANGE` or `#major`
  - **Minor bump** (`0.28.0`): PR body contains `#minor`, `feat:`, or `feature`
  - **Patch bump** (`0.27.10`): Default for all other PRs
- **Artifacts**: Each version bump creates:
  - Updated `VERSION` file
  - Git tag (e.g., `v0.27.10`)
  - GitHub Release with auto-generated notes
  - Docker images with version labels
  - .NET assemblies with embedded version

See [`.github/workflows/version.yml`](.github/workflows/version.yml) for implementation details.

### Project Structure

- **Controllers/**: MVC controllers
- **Data/**: EF Core database contexts
- **Middleware/**: Custom middleware components
- **Models/**: Data and view models
- **Services/**: Service interfaces and implementations
- **Views/**: Razor view templates
- **wwwroot/**: Static files (CSS, JS, images)
- **Program.cs**: Application entry point
- **appsettings.json**: Configuration settings

### Development

To debug the application in VS Code:

1. Press **F5** to start debugging
2. Or use **Ctrl+Shift+P** → "Debug: Start Debugging"

The debugger will launch the application and open it in your default browser.

## Testing

The solution includes **192 tests** across three projects:

| Project | Type | What's Covered |
|---------|------|----------------|
| `Demo1.UnitTests` | xUnit + Moq | Controllers, services, models, middleware, telemetry |
| `Demo1.IntegrationTests` | xUnit + WebApplicationFactory | HTTP routes, security headers, API versioning, error handling |
| `Demo1.PlaywrightTests` | Playwright | Browser-based end-to-end smoke tests |

```bash
# Run all tests
dotnet test

# Run a specific project
dotnet test tests/Demo1.UnitTests
dotnet test tests/Demo1.IntegrationTests
```

- On the first run Playwright downloads headless browser binaries automatically; alternatively, install them explicitly by executing `pwsh tests/Demo1.PlaywrightTests/bin/Debug/net9.0/playwright.ps1 install` from the repository root after a build.
- The GitHub Actions workflow installs the Playwright CLI (`Microsoft.Playwright.CLI`) so CI runs the same headless checks.
- See [`docs/testing.md`](docs/testing.md) for detailed testing guidelines and conventions.

## Documentation

- Docs hub: [`docs/README.md`](docs/README.md)
- Architecture: [`architecture.md`](architecture.md)
- Coding & docs conventions: [`docs/conventions.md`](docs/conventions.md)
- Configuration: [`docs/configuration.md`](docs/configuration.md)
- Testing guidelines: [`docs/testing.md`](docs/testing.md)
- CI/CD pipeline: [`docs/ci-cd.md`](docs/ci-cd.md)
- Build performance: [`docs/build-performance.md`](docs/build-performance.md) — SDK pinning, shared compilation, and analyzer suppression
- Security Headers Playground: [`docs/security-lab.md`](docs/security-lab.md) — interactive lab for toggling HTTP security headers and observing attack behaviors
- Component Showcase: [`docs/component-showcase.md`](docs/component-showcase.md) — browsable UI component catalog with isolated previews
- Feature flag rollout: [`docs/feature-flag-rollout-contract.md`](docs/feature-flag-rollout-contract.md) — rollout policy, checklist contract, activation model
- Feature flag runtime: [`docs/feature-flag-runtime-guide.md`](docs/feature-flag-runtime-guide.md) — implementation patterns, test seams, migration conventions

### XML Documentation

- XML docs are generated in **Release builds only** (see [`docs/build-performance.md`](docs/build-performance.md) for rationale): `bin/Release/<TargetFramework>/Demo1.xml`
- All **public** APIs should include `///` XML comments (enforced by the Documentation Helper CI agent)

## Additional Notes

### Client-side libraries

- Client-side libraries (Bootstrap, jQuery, validation) are managed with LibMan. Run `libman restore` to populate `wwwroot/lib/` when working locally.
- The repository currently contains restored files in `wwwroot/lib/` so Docker builds and CI do not need to fetch from CDNs at build time. If you prefer not to commit vendor files, update the Dockerfile and CI to run `libman restore` during the build.

### Optional: Swagger / OpenAPI

- The docs include an example for enabling Swagger, but it is not enabled by default in `Program.cs`.
- To enable Swagger locally during development, you can add the minimal services and middleware in `Program.cs` (or set a feature flag):

```csharp
// Example: enable when configuration flag is set or in Development
if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwagger", false))
{
   app.UseSwagger();
   app.UseSwaggerUI();
}
```

### Code Coverage

- CI may produce coverage artifacts used by badges and checks. To generate coverage locally with Coverlet (example):

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

Check your CI workflow for the exact coverage collector and reporting steps if you rely on published coverage reports.

### Docker and client libraries

- The Docker build expects `wwwroot/lib/` to contain client libs (this repository currently tracks them). If you change that approach, ensure CI installs LibMan or restores client libraries during the Docker build to avoid publish failures.

## GitHub Actions & Copilot Integration

This project uses an **AI-SDLC Pipeline** powered by [GitHub Agentic Workflows](https://github.github.com/gh-aw/) that automates the full software development lifecycle from issue to deploy.

For the full delivery models, see [AI-SDLC-LOCAL.md](AI-SDLC-LOCAL.md) for local autopilot runs and [AI-SDLC-CLOUD.md](AI-SDLC-CLOUD.md) for cloud autopilot orchestration.

### AI-SDLC Pipeline

When you open an issue, the pipeline runs autonomously through these stages:

```text
Issue opened
    │
    ▼
┌─────────┐
│ Triage  │ → AI classifies issue, applies labels, adds pipeline:planning
└────┬────┘
     ▼
┌────────────────┐
│ Plan & Assign  │ → Creates implementation plan, assigns Copilot coding agent
└────────┬───────┘
         ▼
┌──────────────────────┐
│ Copilot Implements   │ → Delegates to specialist sub-agents:
│                      │   backend, frontend, security, testing, docs
└────────┬─────────────┘   → Posts report on issue, creates PR
         ▼
┌──────────────┐
│ Test & Review│ → Delegates to code-reviewer, security-auditor,
└──────┬───────┘   testing, docs agents → approves or requests changes
       ▼
╔══════════════════╗
║  YOU MERGE PR    ║  ← manual gate
╚════════╤═════════╝
         ▼
┌────────────────┐
│ Post-Merge     │ → Detects merge, transitions to deploy
└────────┬───────┘
         ▼
┌────────┐
│ Deploy │ → Verifies merge, posts summary, closes issue
└────────┘
```

### Pipeline Workflows

| Workflow | Trigger | Purpose |
| ---------- | --------- | --------- |
| `pipeline-triage.md` | Issue comment `/triage` by `rbmathis` | AI classification and routing |
| `pipeline-implement.md` | `pipeline:planning` label | Plan + assign Copilot agent |
| `pipeline-review.md` | PR opened/updated | Multi-agent code review |
| `pipeline-post-merge.md` | PR merged | Transition to deploy stage |
| `pipeline-deploy.md` | `pipeline:deploying` label | Verify merge, close issue |
| `pipeline-retry.md` | `pipeline:retry` label | Re-assign Copilot after failure |
| `pipeline-rollback.md` | `pipeline:failed` label | Handle failed deployments |

### Specialist Agents

The pipeline delegates to these custom agents (defined in `.github/agents/`):

| Agent | Role |
| ------- | ------ |
| `backend` | Controllers, Models, Services, Middleware |
| `frontend` | Views, Razor templates, CSS, JavaScript |
| `security` | OWASP vulnerabilities, CSRF, XSS, auth |
| `testing` | Unit tests, integration tests |
| `docs` | XML documentation, docs/ updates |
| `code-reviewer` | MVC patterns, code quality, SOLID |
| `security-auditor` | Security scanning and audit |
| `build-validator` | Project files, dependencies |
| `feature-flags` | Feature flag rollout strategy |

### Other Automated Workflows

- **🔨 Build & Test**: Runs on every push and PR
- **🚀 Deploy**: Handles production deployments
- **🔍 CodeQL**: Security analysis on every push

## 🤖 Using Custom Copilot Agents

This project includes custom GitHub Copilot agents that you can chat with directly in VS Code!

### Available Agents

Use `@agent-name` in Copilot Chat to invoke them:

#### @code-reviewer 💖

Reviews your code for quality and best practices.

```bash
@code-reviewer look at this controller
@code-reviewer check my security implementation
```

#### @build-validator 🔧

Validates project files and dependencies.

```bash
@build-validator check my .csproj file
@build-validator analyze dependencies
```

#### @security-auditor 🛡️

Scans for security vulnerabilities.

```bash
@security-auditor scan this file
@security-auditor check for SQL injection risks
```

#### @doc-helper 📚

Helps improve documentation.

```bash
@doc-helper generate XML comments
@doc-helper check documentation coverage
```

#### @issue-helper 🎯

Helps triage GitHub issues.

```bash
@issue-helper classify this issue
@issue-helper suggest labels
```

### Quick Start

1. Open Copilot Chat (Ctrl+Shift+I)
2. Type `@` to see available agents
3. Select an agent and ask your question
4. Get intelligent, context-aware assistance!

### Examples

```bash
# Review current file
@code-reviewer analyze this controller for MVC best practices

# Check security
@security-auditor scan for authentication issues

# Validate build
@build-validator check package versions

# Improve docs
@doc-helper add XML comments to selected code
```
