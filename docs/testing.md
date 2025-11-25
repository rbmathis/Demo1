# 🧪 Testing Guidelines

## Current Status

- `Demo1.UnitTests` contains xUnit-based unit tests with mocked dependencies.
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
- Unit tests with coverage: `dotnet tool install --global dotnet-coverage` (first time) followed by `dotnet-coverage collect --output coverage/coverage.cobertura.xml --output-format cobertura --include-files Demo1.dll dotnet test tests/Demo1.UnitTests/Demo1.UnitTests.csproj --no-build` and `python scripts/check_coverage.py coverage/coverage.cobertura.xml 90` to verify the threshold locally.
- Playwright browsers (first run): `dotnet build tests/Demo1.PlaywrightTests` followed by `pwsh bin/Debug/net9.0/playwright.ps1 install` or simply execute the tests once and the suite will install browsers automatically.
- CI (GitHub Actions): Automatically installs Playwright and dotnet-coverage, enforces ≥90 % coverage for unit tests, and runs `dotnet test` when test projects exist.

## Coverage

- Coverage is measured with `dotnet-coverage` against the `Demo1` assembly and filtered to `Controllers`, `Middleware`, and `Telemetry` namespaces (Playwright smoke tests are excluded).
- CI fails if coverage drops below **90 %**.
- Coverage artifacts (`coverage/coverage.cobertura.xml`) are uploaded for each run and can be processed with ReportGenerator/Codecov if desired.

## Guidelines

- Use AAA (Arrange-Act-Assert) pattern.
- Mock dependencies (e.g., `ILogger<T>`, services) where appropriate.
- Keep tests deterministic and isolated.
