# Copilot Agent Configuration Files

This directory contains specialized Copilot agent configuration files that provide focused guidance for different aspects of the Demo1 ASP.NET Core MVC application.

## Available Agents

### 🔧 [Backend Agent](backend-agent.md)
**Focus:** Controllers, Models, Program.cs, Middleware, Dependency Injection

Provides guidance on:
- Implementing MVC controllers
- Creating domain and view models
- Configuring services and middleware
- Dependency injection patterns
- ASP.NET Core MVC best practices for .NET 9

---

### 🎨 [Frontend Agent](frontend-agent.md)
**Focus:** Views, Razor templates, CSS, JavaScript, Static Assets

Provides guidance on:
- Creating Razor views and layouts
- Writing clean CSS and JavaScript
- Managing static assets
- Responsive design
- Accessibility best practices

---

### 🔒 [Security Agent](security-agent.md)
**Focus:** Authentication, Authorization, Security Headers, OWASP

Provides guidance on:
- Configuring authentication and authorization
- Implementing security headers
- Protecting against OWASP Top 10 vulnerabilities
- Managing secrets securely
- Security best practices

---

### 🚀 [DevOps Agent](devops-agent.md)
**Focus:** CI/CD, GitHub Actions, Deployment, Configuration

Provides guidance on:
- Building and maintaining GitHub Actions workflows
- Deployment pipelines and strategies
- Environment-specific configuration
- Docker containerization
- Quality gates and automated checks

---

### 📚 [Documentation Agent](docs-agent.md)
**Focus:** XML Comments, README, API Documentation, Diagrams

Provides guidance on:
- Writing XML documentation comments
- Maintaining markdown documentation
- Creating architecture diagrams with Mermaid
- API documentation with Swagger/OpenAPI
- Technical writing best practices

---

### 🧪 [Testing Agent](testing-agent.md)
**Focus:** Unit Tests, Integration Tests, Test Coverage

Provides guidance on:
- Writing unit tests with xUnit
- Creating integration tests with WebApplicationFactory
- End-to-end testing with Playwright
- Mocking and test doubles
- Test coverage and best practices

---

## How to Use These Agents

These configuration files serve as comprehensive reference guides for Copilot when working on different aspects of the application. When you're working in a specific area:

1. **Copilot will automatically reference** the relevant agent based on the files you're working with
2. **Agent files provide context** about best practices, conventions, and patterns specific to this project
3. **Examples and code snippets** help demonstrate the recommended approaches
4. **Checklists and guidelines** ensure consistent quality across the codebase

## File Structure

Each agent configuration follows a consistent structure:

- **Focus Area**: What the agent specializes in
- **Scope**: Files and patterns the agent handles
- **Instructions**: Detailed guidelines and best practices
- **Code Examples**: Practical demonstrations
- **Related Files**: Relevant file paths
- **Related Documentation**: Links to additional resources

## Contributing

When adding new features or patterns to the application:

1. Update the relevant agent configuration file
2. Add examples demonstrating the new pattern
3. Document any new conventions or best practices
4. Keep instructions clear and actionable

---

## Quick Reference

| Agent | Primary Focus | Key Files |
|-------|--------------|-----------|
| Backend | Server-side logic | `Controllers/`, `Models/`, `Services/`, `Program.cs` |
| Frontend | Client-side UI | `Views/`, `wwwroot/` |
| Security | Application security | Authentication, Authorization, Security headers |
| DevOps | Automation & deployment | `.github/workflows/`, `Dockerfile`, `appsettings*.json` |
| Documentation | Code docs & guides | XML comments, `docs/`, `README.md` |
| Testing | Quality assurance | `tests/`, test projects |

---

*These agent configuration files are part of the Demo1 project's commitment to maintainable, secure, and well-documented code.*
