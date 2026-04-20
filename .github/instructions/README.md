# `.github/instructions/` — Path-Specific Copilot Instructions

This folder contains **path-specific custom instruction files** that GitHub Copilot applies automatically when working on files matching a specified glob pattern. Unlike the repository-wide `copilot-instructions.md` in `.github/`, these files target specific parts of the codebase and activate only when relevant files are in context.

## How Copilot Discovers and Activates These Files

Copilot scans `.github/instructions/` for any file ending in `.instructions.md`. Each file declares an `applyTo` glob in its YAML frontmatter. When Copilot is working on a file whose path matches the glob, the instructions in that file are automatically prepended to the request — no manual selection required.

If **both** a path-specific instructions file and the repository-wide `copilot-instructions.md` apply, Copilot uses **both** simultaneously.

**Activation contexts:**

| Context | Behavior |
|---------|----------|
| VS Code Copilot Chat (Agent mode) | Automatically included when a matched file is open or referenced |
| Copilot code completions | Applied inline as you type in a matched file |
| Copilot code review | Applied when reviewing changes in matched files |
| GitHub Copilot cloud agent | Applied when the cloud agent edits matched files |

## File Format

Each file must begin with a YAML frontmatter block containing the `applyTo` key. The value is a glob pattern (or comma-separated list of globs) that determines which files trigger these instructions.

```markdown
---
applyTo: "Controllers/**"
---

# Instructions in plain Markdown...
```

**Glob examples:**

| Pattern | Matches |
|---------|---------|
| `Controllers/**` | All files under `Controllers/` at any depth |
| `**/*.csproj` | All `.csproj` files anywhere in the repo |
| `Views/**,Shared/**` | Files under either `Views/` or `Shared/` |
| `**` | Every file in the repository |

You can also use `excludeAgent` in the frontmatter to restrict an instructions file to only one consumer:

```markdown
---
applyTo: "**"
excludeAgent: "code-review"   # Only used by cloud-agent, not code review
---
```

## Instructions Files in This Repository

### `controllers.instructions.md`

**Applies to:** `Controllers/**`

Activated whenever Copilot edits or reviews any C# controller file. Enforces:

- Inheriting from `Controller` base class
- Explicit `[Route]` and HTTP verb attributes
- Correct `IActionResult` return types (`View()`, `RedirectToAction()`, `NotFound()`, etc.)
- `ModelState.IsValid` checks before processing submissions
- Constructor injection for services (no service locator pattern)
- `[Authorize]` on protected actions, `[ValidateAntiForgeryToken]` on POST actions
- XML documentation comments on all public methods
- Try-catch blocks for external service calls
- Thin controllers — business logic belongs in services, not controllers

---

### `models.instructions.md`

**Applies to:** `Models/**`

Activated whenever Copilot edits or reviews any model or view model class. Enforces:

- Data annotation validators (`[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, etc.)
- Preference for `record` types for immutable DTOs
- Proper use of nullable reference types (`string?` for optional fields)
- Single-purpose, focused model classes
- Naming conventions: `*ViewModel` suffix for view models, `*Request`/`*Response` for DTOs
- PascalCase for all public properties
- XML documentation on all public classes and properties

---

### `views.instructions.md`

**Applies to:** `Views/**`

Activated whenever Copilot edits or reviews any Razor view (`.cshtml`). Enforces:

- Strongly-typed views with `@model` directive at the top
- Minimal logic in views — complex logic belongs in controllers or view components
- Tag helpers preferred over HTML helpers (e.g., `<a asp-controller>` over `Html.ActionLink`)
- Extending `_Layout.cshtml` for consistent page structure
- `@section` blocks for page-specific scripts and styles
- Views organized in folders matching controller names
- Default Razor output encoding for all user-generated content (XSS prevention)
- `@Html.AntiForgeryToken()` in all POST forms
- Semantic HTML and proper heading hierarchy for accessibility

---

### `projects.instructions.md`

**Applies to:** `**/*.csproj`

Activated whenever Copilot edits any `.csproj` project file. Enforces:

- Latest LTS or current .NET version (currently .NET 9)
- `<Nullable>enable</Nullable>` — required for null safety
- `<ImplicitUsings>enable</ImplicitUsings>` — reduces boilerplate
- `<GenerateDocumentationFile>true</GenerateDocumentationFile>` — enables XML docs
- Specific NuGet version numbers (no floating `*` versions)
- Microsoft-supported packages preferred over third-party alternatives
- No deprecated packages or known-vulnerable versions
- Release configuration for production builds

---

### `docs.instructions.md`

**Applies to:** `docs/**`

Activated whenever Copilot edits or reviews any file in the `docs/` folder. Enforces:

- Clear, concise language in present tense and active voice
- Proper Markdown formatting (headings, lists, code blocks with language tags)
- Table of contents for documents longer than a few sections
- Code examples where concepts benefit from illustration
- Defining acronyms on first use
- Prerequisites and requirements stated upfront
- Cross-links to related documentation files

## References

- [GitHub Docs: Creating path-specific custom instructions](https://docs.github.com/en/copilot/how-tos/configure-custom-instructions/add-repository-instructions#creating-path-specific-custom-instructions)
- [GitHub Docs: About customizing GitHub Copilot responses](https://docs.github.com/en/copilot/concepts/prompting/response-customization)
