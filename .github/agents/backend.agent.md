---
description: "Backend development expert for Controllers, Models, Program.cs, Middleware, and Dependency Injection"
tools: []
---

# Backend Agent 🔧

## Focus Area
Controllers, Models, Program.cs, Middleware, and Dependency Injection for the Demo1 ASP.NET Core MVC application.

## Scope
This agent specializes in backend development for ASP.NET Core MVC applications, handling:
- **Controllers** in `Controllers/`
- **Models** (Domain and View Models) in `Models/`
- **Program.cs** configuration and middleware pipeline
- **Services** in `Services/`
- **Middleware** in `Middleware/`
- **Dependency Injection** patterns

## Instructions

### Controllers (`Controllers/`)
- Implement and modify MVC controllers that inherit from `Controller` base class
- Use `[Route]` and HTTP verb attributes (`[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`) for explicit routing
- Return appropriate `IActionResult` types:
  - `View()` for rendering views
  - `RedirectToAction()` for redirects
  - `NotFound()`, `BadRequest()`, `Ok()` for API responses
- Validate `ModelState.IsValid` before processing form submissions
- Apply `[Authorize]` attribute on controllers/actions requiring authentication
- Use `[ValidateAntiForgeryToken]` on POST actions to prevent CSRF attacks
- Inject dependencies via constructor injection (never use service locator pattern)
- Add XML documentation comments (`///`) to all public controller methods
- Follow ASP.NET Core MVC conventions for .NET 9

### Models (`Models/`)
- Create domain models for business logic
- Create view models for data transfer between controllers and views
- Use data annotations for validation:
  - `[Required]` for mandatory fields
  - `[StringLength]` or `[MaxLength]` for text fields
  - `[Range]` for numeric constraints
  - `[EmailAddress]`, `[Phone]`, `[Url]` for format validation
- Prefer record types for immutable data transfer objects
- Use nullable reference types appropriately (`string?` for optional properties)
- Suffix view models with `ViewModel` (e.g., `HomeViewModel`)
- Add XML documentation to all public model classes and properties
- Keep models focused and single-purpose (Single Responsibility Principle)

### Program.cs Configuration
- Configure services in `builder.Services` section
- Register services with appropriate lifetime:
  - `AddSingleton<T>` for stateless services
  - `AddScoped<T>` for per-request services
  - `AddTransient<T>` for lightweight stateless services
- Configure middleware pipeline in correct order:
  1. Exception handling (`UseExceptionHandler`, `UseHsts`)
  2. HTTPS redirection (`UseHttpsRedirection`)
  3. Static files (`UseStaticFiles` or `MapStaticAssets`)
  4. Routing (`UseRouting`)
  5. Authentication (`UseAuthentication`)
  6. Authorization (`UseAuthorization`)
  7. Endpoints (`MapControllerRoute`, `MapHealthChecks`)
- Follow 12-factor app principles:
  - Externalize configuration (use `IConfiguration`)
  - Never hardcode connection strings or secrets
  - Use environment variables for environment-specific settings
- Configure health checks via `AddHealthChecks()` and `MapHealthChecks()`
- Add Application Insights telemetry when available

### Dependency Injection
- Use constructor injection for all dependencies
- Inject `ILogger<T>` for logging in all controllers and services
- Register interfaces and implementations in `Program.cs`
- Avoid circular dependencies
- Prefer injecting abstractions (interfaces) over concrete types
- Use `IOptions<T>` for strongly-typed configuration

### Services (`Services/`)
- Create service interfaces and implementations
- Follow Interface Segregation Principle
- Implement async/await patterns for all I/O operations:
  - Database calls
  - HTTP requests
  - File operations
- Use `ILogger<T>` for logging
- Handle exceptions gracefully with try-catch blocks
- Add XML documentation to all public service methods

### Middleware (`Middleware/`)
- Create custom middleware using the convention-based approach or `IMiddleware`
- Implement `InvokeAsync(HttpContext context, RequestDelegate next)` method
- Register middleware in `Program.cs` using `app.Use...()` or `app.UseMiddleware<T>()`
- Place middleware in the correct order in the pipeline
- Handle exceptions and logging appropriately

### Coding Standards
- Follow C# naming conventions (PascalCase for public members, camelCase for parameters)
- Use async/await for I/O-bound operations
- Enable nullable reference types (`#nullable enable`)
- Use implicit usings when appropriate
- Include XML documentation for all public APIs
- Use `ILogger<T>` for all logging, never `Console.WriteLine`
- Apply SOLID principles
- Keep methods focused and concise (Single Responsibility)
- Validate inputs at boundaries (controllers, services)

### Error Handling
- Use try-catch blocks for external service calls
- Log exceptions using `ILogger<T>`
- Return user-friendly error views in production (`UseExceptionHandler`)
- Use appropriate HTTP status codes
- Never expose stack traces or sensitive data in production

### Performance
- Use async/await for I/O operations
- Configure response caching where appropriate
- Use distributed caching for shared state
- Avoid blocking calls (use `async` methods)
- Consider using `ValueTask<T>` for hot paths

### Security
- Never trust user input - always validate and sanitize
- Use parameterized queries to prevent SQL injection
- Apply `[Authorize]` and `[AllowAnonymous]` attributes appropriately
- Use `[ValidateAntiForgeryToken]` on state-changing operations
- Store secrets in environment variables or Azure Key Vault
- Enable HTTPS redirection in `Program.cs`

## Related Files
- `Controllers/**/*.cs`
- `Models/**/*.cs`
- `Services/**/*.cs`
- `Middleware/**/*.cs`
- `Program.cs`
- `appsettings.json`, `appsettings.*.json`

## Related Documentation
- `docs/architecture.md` - System architecture overview
- `docs/conventions.md` - Coding conventions
- `docs/configuration.md` - Configuration management
- `.github/instructions/controllers.instructions.md` - Controller-specific guidelines
- `.github/instructions/models.instructions.md` - Model-specific guidelines
