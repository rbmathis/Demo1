# ASP.NET Core MVC Project

[![Build & Test](https://github.com/rbmathis/Demo1/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/rbmathis/Demo1/actions/workflows/dotnet.yml)
[![Deploy](https://github.com/rbmathis/Demo1/actions/workflows/deploy.yml/badge.svg?branch=main)](https://github.com/rbmathis/Demo1/actions/workflows/deploy.yml)
[![Copilot Agents](https://github.com/rbmathis/Demo1/actions/workflows/copilot-agents.yml/badge.svg?branch=main)](https://github.com/rbmathis/Demo1/actions/workflows/copilot-agents.yml)
[![Targeted Coverage](https://img.shields.io/badge/Controllers%20Coverage-%E2%89%A5%2090%25-ff69b4?logo=codecov&logoColor=white)](coverage/report/Summary.txt)
[![Playwright Ready](https://img.shields.io/badge/Playwright-smoke%20tested-3fb950?logo=playwright&logoColor=white)](tests/Demo1.PlaywrightTests)

Welcome to the glam corner of .NET 9 where MVC meets main-character energy. This repo is our stage for building production-ready web apps with a confident strut, buttery-smooth tooling, and telemetry that keeps the spotlight exactly where we want it.

## Highlights

- **Turnkey pipelines** – CI keeps the runway green with linting, unit tests, and smoke checks.
- **Feature-flag finesse** – Azure App Configuration flips features faster than a costume change.
- **Observability drip** – Application Insights, custom telemetry, and coverage gates keep the receipts.


## Getting Started

### Prerequisites

- .NET 9 SDK
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

### VS Code Tasks

This project includes VS Code tasks that you can run via:

- **Ctrl+Shift+P** → "Tasks: Run Task"
- Available tasks: `build`, `run`, `watch`

### Project Structure

- **Controllers/**: MVC controllers
- **Views/**: Razor view templates
- **Models/**: Data models
- **wwwroot/**: Static files (CSS, JS, images)
- **Program.cs**: Application entry point
- **appsettings.json**: Configuration settings

### Development

To debug the application in VS Code:

1. Press **F5** to start debugging
2. Or use **Ctrl+Shift+P** → "Debug: Start Debugging"

The debugger will launch the application and open it in your default browser.

## Testing

- `dotnet test` triggers all suites, including the Playwright smoke tests in `Demo1.PlaywrightTests`.
- On the first run Playwright downloads headless browser binaries automatically; alternatively, install them explicitly by executing `pwsh tests/Demo1.PlaywrightTests/bin/Debug/net9.0/playwright.ps1 install` from the repository root after a build (ensure your working directory is the solution root).
- The GitHub Actions workflow installs the Playwright CLI (`Microsoft.Playwright.CLI`) so CI runs the same headless checks.

## Documentation

- Docs hub: [`docs/README.md`](docs/README.md)
- Architecture: [`docs/architecture.md`](docs/architecture.md)
- Coding & docs conventions: [`docs/conventions.md`](docs/conventions.md)
- Configuration: [`docs/configuration.md`](docs/configuration.md)
- Testing guidelines: [`docs/testing.md`](docs/testing.md)

### XML Documentation

- The project generates XML docs on build: `bin/<Configuration>/<TargetFramework>/Demo1.xml`
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

This project is configured with GitHub Actions workflows and Copilot Custom Agents:

### Automated Workflows

- **🔨 Build & Test**: Runs on every push and PR
- **🚀 Deploy**: Handles production deployments
- **🤖 Copilot Agents**: Custom agents for code review and quality checks

### Copilot Custom Agents

- **Code Reviewer**: Reviews PRs for MVC best practices
- **Build Validator**: Ensures successful builds
- **Security Auditor**: Scans for vulnerabilities (weekly + PRs)
- **Documentation Helper**: Maintains docs quality

### Getting Started with CI/CD

1. Push your code to trigger automated builds
2. Create a pull request to activate code review agents
3. Agents provide feedback and suggestions automatically
4. Configure deployment secrets for production releases

See `.github/copilot-instructions.md` for detailed agent configuration.

## 🤖 Using Custom Copilot Agents

This project includes custom GitHub Copilot agents that you can chat with directly in VS Code!

### Available Agents

Use `@agent-name` in Copilot Chat to invoke them:

#### @code-reviewer 💖
Reviews your code for quality and best practices.

```
@code-reviewer look at this controller
@code-reviewer check my security implementation
```

#### @build-validator 🔧
Validates project files and dependencies.

```
@build-validator check my .csproj file
@build-validator analyze dependencies
```

#### @security-auditor 🛡️
Scans for security vulnerabilities.

```
@security-auditor scan this file
@security-auditor check for SQL injection risks
```

#### @doc-helper 📚
Helps improve documentation.

```
@doc-helper generate XML comments
@doc-helper check documentation coverage
```

#### @issue-helper 🎯
Helps triage GitHub issues.

```
@issue-helper classify this issue
@issue-helper suggest labels
```

### Quick Start

1. Open Copilot Chat (Ctrl+Shift+I)
2. Type `@` to see available agents
3. Select an agent and ask your question
4. Get intelligent, context-aware assistance!

### Examples

```
# Review current file
@code-reviewer analyze this controller for MVC best practices

# Check security
@security-auditor scan for authentication issues

# Validate build
@build-validator check package versions

# Improve docs
@doc-helper add XML comments to selected code
```
