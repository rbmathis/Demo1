# ASP.NET Core MVC Project

This is a new ASP.NET Core MVC web application built with .NET 9.

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

## Documentation

- Docs hub: [`docs/README.md`](docs/README.md)
- Architecture: [`docs/architecture.md`](docs/architecture.md)
- Coding & docs conventions: [`docs/conventions.md`](docs/conventions.md)
- Configuration: [`docs/configuration.md`](docs/configuration.md)
- Testing guidelines: [`docs/testing.md`](docs/testing.md)

### XML Documentation

- The project generates XML docs on build: `bin/<Configuration>/<TargetFramework>/Demo1.xml`
- All **public** APIs should include `///` XML comments (enforced by the Documentation Helper CI agent)

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
