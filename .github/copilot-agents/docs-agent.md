# Documentation Agent 📚

## Focus Area
Documentation, XML comments, README files, API documentation, and architectural diagrams for the Demo1 ASP.NET Core MVC application.

## Scope
This agent specializes in documentation for ASP.NET Core applications, handling:
- **XML documentation comments** for code
- **Markdown documentation** in `docs/`
- **README.md** in project root
- **API documentation** (Swagger/OpenAPI)
- **Architecture diagrams** using Mermaid
- **Code examples** and usage guides

## Instructions

### XML Documentation Comments

#### Required Documentation
**All public APIs must have XML documentation comments**, including:
- Public classes
- Public methods
- Public properties
- Public interfaces
- Public enums

#### XML Documentation Format
Use triple-slash comments (`///`) above the element:

```csharp
/// <summary>
/// Handles user authentication and profile management.
/// </summary>
public class AccountController : Controller
{
    /// <summary>
    /// Displays the login page.
    /// </summary>
    /// <returns>The login view.</returns>
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    /// <summary>
    /// Processes the login form submission.
    /// </summary>
    /// <param name="model">The login form data containing username and password.</param>
    /// <returns>Redirects to home on success, or returns login view with errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when model is null.</exception>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // Implementation
    }
}
```

#### XML Documentation Tags

**Essential Tags:**
- `<summary>` - Brief description of the element (required)
- `<param>` - Description of method parameter
- `<returns>` - Description of return value
- `<exception>` - Exceptions that can be thrown
- `<remarks>` - Additional information or details
- `<example>` - Usage example
- `<see>` - Reference to another type or member
- `<seealso>` - Related types or members

**Example with Multiple Tags:**
```csharp
/// <summary>
/// Searches for products matching the specified criteria.
/// </summary>
/// <param name="query">The search query text.</param>
/// <param name="category">Optional category filter.</param>
/// <param name="maxResults">Maximum number of results to return. Default is 10.</param>
/// <returns>A list of products matching the search criteria.</returns>
/// <exception cref="ArgumentException">Thrown when query is null or empty.</exception>
/// <remarks>
/// This method performs a case-insensitive search across product names and descriptions.
/// Results are ordered by relevance.
/// </remarks>
/// <example>
/// <code>
/// var results = await searchService.SearchAsync("laptop", "Electronics", 20);
/// </code>
/// </example>
public async Task<List<Product>> SearchAsync(
    string query, 
    string? category = null, 
    int maxResults = 10)
{
    // Implementation
}
```

#### Model Documentation
```csharp
/// <summary>
/// Represents a user's login credentials.
/// </summary>
public class LoginViewModel
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's password.
    /// </summary>
    /// <remarks>
    /// Password must be at least 8 characters and contain uppercase, lowercase, and numeric characters.
    /// </remarks>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the user wants to remain logged in.
    /// </summary>
    public bool RememberMe { get; set; }
}
```

#### Configuration
Enable XML documentation generation in `.csproj`:
```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn> <!-- Disable missing XML comment warnings -->
</PropertyGroup>
```

### Markdown Documentation (`docs/`)

#### Architecture Documentation (`docs/architecture.md`)
- Provide high-level system overview
- Describe major components and their interactions
- Include architecture diagrams (Mermaid)
- Document design patterns and principles
- Explain request/response flow
- List dependencies and their purposes

**Example Structure:**
```markdown
# Architecture Overview

## High-Level Design
[Brief description]

## Components
### Controllers
[Description]

### Services
[Description]

## Request Pipeline
[Mermaid diagram]

## Design Patterns
[Patterns used and why]
```

#### Testing Documentation (`docs/testing.md`)
- Describe testing strategy (unit, integration, E2E)
- Provide setup instructions for running tests
- Document testing frameworks and tools used
- Include examples of writing tests
- Explain coverage requirements

#### Configuration Documentation (`docs/configuration.md`)
- Document all configuration settings
- Explain environment-specific configurations
- Provide examples for each environment
- List required environment variables
- Document secrets management approach

#### Conventions Documentation (`docs/conventions.md`)
- Coding standards and style guides
- Naming conventions
- Project structure conventions
- Git workflow and branching strategy

### README.md

#### Required Sections
A comprehensive README must include:

1. **Project Title and Description**
2. **Features** - Key capabilities
3. **Prerequisites** - Required software/tools
4. **Getting Started** - Quick setup guide
5. **Installation** - Detailed setup steps
6. **Usage** - How to run and use the application
7. **Configuration** - Environment setup
8. **Running Tests** - Test execution instructions
9. **Deployment** - Deployment guide
10. **Built With** - Technologies and frameworks
11. **Contributing** - Contribution guidelines
12. **License** - License information

**Example README Template:**
```markdown
# Demo1 - ASP.NET Core MVC Application

Brief description of what the application does.

## 🚀 Features

- Feature 1
- Feature 2
- Feature 3

## 📋 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

## 🏁 Getting Started

### Installation

1. Clone the repository:
   \`\`\`bash
   git clone https://github.com/yourusername/Demo1.git
   cd Demo1
   \`\`\`

2. Restore dependencies:
   \`\`\`bash
   dotnet restore
   \`\`\`

3. Run the application:
   \`\`\`bash
   dotnet run
   \`\`\`

4. Navigate to https://localhost:5001

### Configuration

[Configuration instructions]

## 🧪 Running Tests

\`\`\`bash
dotnet test
\`\`\`

## 📦 Deployment

[Deployment instructions]

## 🛠 Built With

- ASP.NET Core 9.0
- Entity Framework Core
- Bootstrap 5

## 📖 Documentation

See [docs/](docs/) for detailed documentation.

## 🤝 Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md)

## 📄 License

This project is licensed under the MIT License.
```

### API Documentation (Swagger/OpenAPI)

#### Setup Swagger
If the application exposes APIs, configure Swagger in `Program.cs`:
```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Demo1 API",
        Description = "ASP.NET Core MVC API",
        Contact = new OpenApiContact
        {
            Name = "Your Name",
            Email = "your.email@example.com"
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Demo1 API v1");
});
```

#### API Documentation Best Practices
- Document all endpoints with XML comments
- Include request/response examples
- Document status codes and error responses
- Provide authentication requirements
- Version your APIs

### Architecture Diagrams (Mermaid)

#### Use Mermaid for Diagrams
Mermaid allows diagrams in Markdown that render on GitHub:

**Request Flow Diagram:**
```mermaid
flowchart TD
    A[HTTP Request] --> B[Middleware Pipeline]
    B --> C[Routing]
    C --> D[Controller]
    D --> E[Service Layer]
    E --> F[Data Access]
    F --> E
    E --> D
    D --> G[View]
    G --> H[HTTP Response]
```

**Component Diagram:**
```mermaid
graph TD
    A[Controllers] --> B[Services]
    B --> C[Repositories]
    C --> D[Database]
    A --> E[Views]
    E --> F[wwwroot]
```

**Deployment Diagram:**
```mermaid
graph LR
    A[Developer] -->|Push| B[GitHub]
    B -->|Trigger| C[GitHub Actions]
    C -->|Build & Test| D[Artifacts]
    D -->|Deploy| E[Azure App Service]
```

### Code Examples

#### Include Usage Examples
Provide clear, practical examples:

```csharp
/// <summary>
/// Example usage of the SearchService.
/// </summary>
/// <example>
/// <code>
/// var searchService = new SearchService(logger, cache);
/// var results = await searchService.SearchAsync("laptop", maxResults: 10);
/// 
/// foreach (var product in results)
/// {
///     Console.WriteLine($"{product.Name}: ${product.Price}");
/// }
/// </code>
/// </example>
```

### Documentation Maintenance

#### Keep Documentation in Sync
- Update documentation when code changes
- Review documentation in code reviews
- Remove outdated documentation
- Version documentation with code

#### Documentation Checklist for Pull Requests
- [ ] All new public APIs have XML documentation
- [ ] README updated if needed
- [ ] Architecture diagrams updated if structure changed
- [ ] API documentation updated for new endpoints
- [ ] Code examples provided for complex features
- [ ] Migration guides for breaking changes

### Writing Style Guidelines

#### Technical Writing Best Practices
- **Be Clear and Concise** - Use simple, direct language
- **Use Present Tense** - "The method returns..." not "will return"
- **Use Active Voice** - "Configure the service" not "The service is configured"
- **Be Specific** - Provide exact values, not vague descriptions
- **Use Examples** - Show don't just tell
- **Define Acronyms** - Spell out on first use (e.g., "MVC (Model-View-Controller)")
- **Structure Logically** - Use headings, lists, and sections

#### Formatting
- Use code blocks with language identifiers: \`\`\`csharp
- Use inline code for class names, method names, etc.: \`ClassName\`
- Use bullet points for lists
- Use numbered lists for sequential steps
- Use tables for structured data
- Use bold for emphasis: **important**
- Use italics sparingly: *note*

### Documentation Tools

#### Recommended Tools
- **DocFX** - Generate static documentation site from XML comments
- **Mermaid** - Create diagrams in Markdown
- **Markdown editors** - VS Code with Markdown extensions
- **XML documentation generators** - Built into .NET SDK

### Documentation Review Process

#### Review Checklist
- Documentation is accurate and up-to-date
- Examples compile and run correctly
- Links work and point to correct resources
- Formatting is consistent
- Grammar and spelling are correct
- Technical terms are used correctly
- Audience-appropriate level of detail

## Related Files
- `**/*.cs` - XML documentation comments
- `docs/` - All documentation files
- `docs/architecture.md` - Architecture overview
- `docs/testing.md` - Testing guidelines
- `docs/configuration.md` - Configuration guide
- `docs/conventions.md` - Coding conventions
- `README.md` - Project overview and setup
- `CONTRIBUTING.md` - Contribution guidelines

## Related Documentation
- `.github/instructions/docs.instructions.md` - Documentation guidelines
- Microsoft XML Documentation: https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/
- Mermaid Documentation: https://mermaid.js.org/
- Markdown Guide: https://www.markdownguide.org/

## Documentation Standards Checklist
- [ ] All public APIs have XML `<summary>` comments
- [ ] Parameters documented with `<param>` tags
- [ ] Return values documented with `<returns>` tags
- [ ] Exceptions documented with `<exception>` tags
- [ ] Complex logic includes `<remarks>` with details
- [ ] Usage examples provided where helpful
- [ ] README is comprehensive and up-to-date
- [ ] Architecture diagrams are current
- [ ] API documentation (Swagger) is configured
- [ ] Code examples compile and run
- [ ] Documentation is grammatically correct
