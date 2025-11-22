# Copilot Instructions for .NET MVC Project

This project uses GitHub Copilot Custom Agents for automated code review, security scanning, and quality assurance.

## Copilot Communication Style

- **Tone**: Slightly flirty, engaging, and humorous
- **Formality**: Informal but professional
- **Clarity**: Clear and concise explanations
- **Encouragement**: Positive reinforcement and constructive feedback

## Custom Agents Configuration

### Code Reviewer Agent

- **Purpose**: Reviews pull requests for .NET MVC best practices
- **Triggers**: PR opened/updated
- **Checks**: MVC patterns, code quality, security practices

### Build Validator Agent

- **Purpose**: Ensures builds are successful and dependencies are correct
- **Triggers**: Push to main/develop, PRs
- **Checks**: Project files, dependencies, build status

### Security Auditor Agent

- **Purpose**: Scans for security vulnerabilities
- **Triggers**: Weekly schedule, PRs
- **Checks**: Dependencies, security headers, authentication

### Documentation Helper Agent

- **Purpose**: Maintains documentation quality
- **Triggers**: PRs, pushes to main
- **Checks**: XML comments, README completeness

## Development Guidelines

### Coding Standards

- Use XML documentation for public APIs
- Follow MVC architectural patterns
- Implement proper error handling
- Use dependency injection appropriately

### Security Requirements

- Enable HTTPS redirection
- Implement authentication/authorization
- Validate all inputs
- Keep dependencies updated

### Testing Standards

- Write unit tests for controllers
- Test model validation
- Include integration tests for key workflows

### Documentation Requirements

- Update README for significant changes
- Document API endpoints
- Include setup and deployment instructions

## GitHub Actions Integration

The project includes three main workflows:

1. **Build and Test** (`dotnet.yml`): Builds, tests, and validates code quality
2. **Deploy** (`deploy.yml`): Handles production deployments
3. **Copilot Agents** (`copilot-agents.yml`): Executes agent-triggered tasks

## Getting Started with Agents

1. Push code to trigger the Build Validator Agent
2. Create a PR to activate the Code Reviewer Agent
3. Weekly Security Auditor runs automatically
4. Documentation Helper checks run on main branch updates

The agents provide automated feedback through GitHub Actions and can hand off tasks between each other for comprehensive code review.
