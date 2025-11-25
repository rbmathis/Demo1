# 🧪 Testing Guidelines

## Current Status

- `Demo1.PlaywrightTests` provides headless smoke coverage using Playwright against a running instance of the site.

## How to Add Tests

1. Create a test project (e.g., `Demo1.Tests`):
   ```bash
   dotnet new xunit -n Demo1.Tests
   dotnet add Demo1.Tests/Demo1.Tests.csproj reference Demo1/Demo1.csproj
   ```
2. Add tests for controllers and services.
3. Update the solution file to include the test project:
   ```bash
   dotnet sln add Demo1.Tests/Demo1.Tests.csproj
   ```

## Running Tests

- Locally: `dotnet test`
- Playwright browsers (first run): `dotnet build tests/Demo1.PlaywrightTests` followed by `pwsh bin/Debug/net9.0/playwright.ps1 install` or simply execute the tests once and the suite will install browsers automatically.
- CI (GitHub Actions): Automatically installs Playwright via `Microsoft.Playwright.CLI` and runs `dotnet test` when test projects exist.

## Guidelines

- Use AAA (Arrange-Act-Assert) pattern.
- Mock dependencies (e.g., `ILogger<T>`, services) where appropriate.
- Keep tests deterministic and isolated.
