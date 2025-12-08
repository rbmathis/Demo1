# Validate Project File Skill 📦

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
   - Comment: "@github-copilot validate the project files in this PR"
   - The GitHub Copilot agent will use this skillset to check .csproj files

2. In GitHub Actions Workflows:
   - Configure workflows to trigger Copilot project file validation
   - The agent executes using this skillset configuration

3. Manual Invocation on GitHub.com:
   - Navigate to repository settings → Copilot
   - Manually trigger project validation
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

FOR LOCAL PROJECT FILE VALIDATION:
Instead of this skillset, use:
- VS Code's MSBuild extension for .csproj editing
- Local CLI tools (dotnet list package, dotnet outdated)
- NuGet Package Manager in Visual Studio/VS Code
- dotnet-format and Roslyn analyzers
- The scripts in /scripts directory of this project

RELATIONSHIP TO OTHER FILES:
- .github/copilot-instructions.md: General Copilot behavior for ALL contexts
- .github/copilot/skillsets/*.md: Specific tasks for GitHub.com Copilot Agents only
- .github/instructions/*.md: Coding standards for IDE usage

═══════════════════════════════════════════════════════════════════════════════
-->

## Check These Elements

### Project Configuration
- ✅ SDK: Microsoft.NET.Sdk.Web for web apps
- ✅ TargetFramework: net8.0 (latest stable)
- ✅ Nullable: enabled
- ✅ ImplicitUsings: enabled

### Package References
- 📦 No duplicate packages
- 📦 Consistent versions across solution
- 📦 No deprecated packages
- 📦 Check for security vulnerabilities

### Build Settings
- ⚙️ GenerateDocumentationFile for libraries
- ⚙️ TreatWarningsAsErrors for production code
- ⚙️ Proper OutputType (Exe/Library)

## Common Issues
- ⚠️ Missing package references
- ⚠️ Outdated packages
- ⚠️ Mixed package management styles
