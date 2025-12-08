# Security Check Skill

<!-- 
╔══════════════════════════════════════════════════════════════════════════════╗
║                          🚨 IMPORTANT USAGE NOTICE 🚨                        ║
╚══════════════════════════════════════════════════════════════════════════════╝

This is a GitHub Copilot SKILLSET for use with GitHub.com's Copilot Agents.

WHERE THIS WORKS:
✅ GitHub.com Actions (automated workflows)
✅ GitHub.com manual invocation via Copilot interface
✅ GitHub Pull Request reviews and comments
✅ GitHub Issues and Discussions with @github-copilot mentions

WHERE THIS DOES NOT WORK:
❌ VS Code Chat window (@workspace, @terminal, etc.)
❌ VS Code inline suggestions
❌ Local Copilot Chat in any IDE
❌ Command line tools or terminal

WHAT IS A SKILLSET?
Skillsets are specialized instructions that GitHub Copilot Agents use when performing
specific tasks on GitHub.com. They're part of GitHub's hosted Copilot service, not
the local IDE extension.

HOW TO USE THIS SKILLSET:

1. On GitHub.com Pull Requests:
   - Comment: "@github-copilot check security in this PR"
   - The GitHub Copilot agent will use this skillset to scan the PR

2. In GitHub Actions Workflows:
   - Configure workflows to trigger Copilot security checks
   - The agent executes using this skillset configuration

3. Manual Invocation on GitHub.com:
   - Navigate to repository settings → Copilot
   - Manually trigger security checks
   - Agent uses this skillset for analysis

WHY CAN'T I USE THIS IN VS CODE?
VS Code Copilot is a separate service that runs locally in your editor. It uses
different context and doesn't have access to GitHub's skillset system. Skillsets
are server-side configurations that only GitHub.com's hosted Copilot Agents can
execute.

THINK OF IT LIKE:
- GitHub Actions workflows: Run on GitHub servers, not locally
- This skillset: Runs in GitHub Copilot Agents, not in your IDE
- VS Code Copilot: Different service, different capabilities

FOR LOCAL SECURITY SCANNING:
Instead of this skillset, use:
- VS Code extensions for security linting
- Local CLI tools (dotnet format analyzers, security code scan)
- Pre-commit hooks with security checks
- The scripts in /scripts directory of this project

RELATIONSHIP TO OTHER FILES:
- .github/copilot-instructions.md: General Copilot behavior for ALL contexts
- .github/copilot/skillsets/*.md: Specific tasks for GitHub.com Copilot Agents only
- .github/instructions/*.md: Coding standards for IDE usage

═══════════════════════════════════════════════════════════════════════════════
-->

Scan for:
- Missing [Authorize] attributes
- SQL injection risks (string concatenation in queries)
- Missing input validation
- Exposed secrets in code
- Missing HTTPS enforcement
- CSRF protection ([ValidateAntiForgeryToken])
- XSS vulnerabilities

## Purpose
Identify common security vulnerabilities in .NET MVC applications.

## Security Checklist

### Authentication & Authorization
- [ ] [Authorize] attribute used on controllers/actions
- [ ] Anonymous access explicitly marked with [AllowAnonymous]
- [ ] Role-based or policy-based authorization implemented
- [ ] Authentication middleware configured in Program.cs

### Input Validation
- [ ] Model validation with data annotations
- [ ] ModelState.IsValid checked before processing
- [ ] User input sanitized before display (prevents XSS)
- [ ] File upload restrictions (size, type, content validation)

### Data Protection
- [ ] Passwords hashed (never stored plain text)
- [ ] Sensitive data encrypted at rest
- [ ] Connection strings in secure configuration
- [ ] No secrets in source code

### SQL Injection Prevention
- [ ] Parameterized queries or ORM used
- [ ] No string concatenation in SQL
- [ ] Entity Framework or Dapper used properly

### CSRF Protection
- [ ] [ValidateAntiForgeryToken] on POST actions
- [ ] @Html.AntiForgeryToken() in forms
- [ ] SameSite cookie policy configured

### HTTPS & Communication
- [ ] UseHsts() and UseHttpsRedirection() enabled
- [ ] Secure cookie settings
- [ ] CORS configured appropriately

## Red Flags to Report
- Plain text passwords
- SQL string concatenation
- eval() or similar dynamic code execution
- Exposed connection strings
- Missing input validation
- Disabled certificate validation
