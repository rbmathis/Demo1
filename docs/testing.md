# 🧪 Testing Guidelines

## Current Status

- No test projects exist yet (`*.Tests`); the CI pipeline skips tests when none are found.

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
- CI (GitHub Actions): Automatically runs `dotnet test` if any test projects exist.

## Guidelines

- Use AAA (Arrange-Act-Assert) pattern.
- Mock dependencies (e.g., `ILogger<T>`, services) where appropriate.
- Keep tests deterministic and isolated.
