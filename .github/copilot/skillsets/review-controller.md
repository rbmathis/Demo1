# Review Controller Skill 🎮

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
   - Comment: "@github-copilot review the controllers in this PR"
   - The GitHub Copilot agent will use this skillset to analyze controllers

2. In GitHub Actions Workflows:
   - Configure workflows to trigger Copilot controller reviews
   - The agent executes using this skillset configuration

3. Manual Invocation on GitHub.com:
   - Navigate to repository settings → Copilot
   - Manually trigger controller reviews
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

FOR LOCAL CONTROLLER REVIEW:
Instead of this skillset, use:
- VS Code extensions for code analysis
- Local CLI tools (dotnet format analyzers, Roslyn analyzers)
- Pre-commit hooks with code quality checks
- The scripts in /scripts directory of this project
- VS Code's built-in IntelliSense and code suggestions

RELATIONSHIP TO OTHER FILES:
- .github/copilot-instructions.md: General Copilot behavior for ALL contexts
- .github/copilot/skillsets/*.md: Specific tasks for GitHub.com Copilot Agents only
- .github/instructions/*.md: Coding standards for IDE usage

═══════════════════════════════════════════════════════════════════════════════
-->

## Purpose
Analyze ASP.NET MVC controller code for best practices and common issues.

## What to Check

### MVC Best Practices
- ✅ Controllers should be thin (delegate to services)
- ✅ Use dependency injection
- ✅ Return appropriate ActionResult types
- ✅ Async/await for I/O operations

### Security
- 🔒 [Authorize] attributes present
- 🔒 ModelState validation before processing
- 🔒 AntiForgeryToken for state-changing operations

### Code Quality
- 📝 XML comments on public actions
- 🎯 Single responsibility per method
- ⚡ Proper error handling with try-catch

## Red Flags
- ❌ Business logic in controllers
- ❌ Direct database access (use repositories/services)
- ❌ Synchronous I/O operations
- ❌ Missing authorization checks

## Example Good Pattern

```csharp
[Authorize]
public class ProductController : Controller
{
    private readonly IProductService _productService;
    
    public ProductController(IProductService productService)
    {
        _productService = productService;
    }
    
    /// <summary>
    /// Gets product details by ID
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            return NotFound();
            
        return View(product);
    }
}
```

## Now, To Actually Make This Work 💪

You still need to create those skillset files! Here's the minimum to get started:

### [review-controller.md](file:///c%3A/Users/rmathis/source/Demo1/.github/copilot/skillsets/review-controller.md)
