# 🧪 Testing Guidelines

## Current Status

The solution has **192 tests** across three test projects covering unit, integration, and end-to-end scenarios.

| Project | Framework | Test Count | Purpose |
|---------|-----------|------------|---------|
| `Demo1.UnitTests` | xUnit + Moq | ~140 | Unit tests for controllers, services, models, middleware, and telemetry |
| `Demo1.IntegrationTests` | xUnit + `WebApplicationFactory` | ~40 | HTTP-level integration tests against an in-memory test server |
| `Demo1.PlaywrightTests` | Playwright | ~12 | Browser-based end-to-end smoke tests |

## Test Projects

### Demo1.UnitTests

Unit tests with mocked dependencies for fast, isolated verification.

| Category | Files | What's Covered |
|----------|-------|----------------|
| **Controllers** | `HomeControllerTests`, `HomeControllerAntiPatternTests`, `HomeControllerCsrfTests`, `HealthControllerTests`, `PerformanceControllerTests`, `SecurityLabControllerTests` | Action results, view data, model binding, error handling, CSRF protection |
| **Services** | `InMemorySearchServiceTests`, `MockWeatherServiceTests`, `InMemoryUserProfileServiceTests`, `StyleGeneratorServiceTests`, `AchievementServiceTests`, `AchievementProcessorServiceTests`, `SecurityLabServiceTests` | Search filtering, weather data generation, profile CRUD, CSS generation, achievement processing |
| **Models** | `ModelValidationTests` | Default values, property behavior, and structure validation for `SearchQuery`, `WeatherData`, `ViewLogicData`, `InlineCssModel`, `ErrorViewModel`, `UserProfile` |
| **Middleware** | `ServerTimingMiddlewareTests`, `SecurityHeadersMiddlewareTests`, `RateLimitHeadersMiddlewareTests`, `AchievementMiddlewareTests` | Header injection, request pipeline behavior |
| **Telemetry** | `CustomTelemetryInitializerTests` | Telemetry property enrichment |
| **Infrastructure** | `PerformanceMetricsServiceTests` | Performance budget monitoring |

### Demo1.IntegrationTests

Integration tests using `WebApplicationFactory<Program>` to spin up an in-memory test server and make real HTTP requests through the full middleware pipeline.

| Category | Files | What's Covered |
|----------|-------|----------------|
| **Controllers** | `HomeControllerTests`, `HomeAntiPatternRouteTests`, `HealthControllerTests`, `AchievementControllerTests`, `WeatherForecastVersioningTests`, `ErrorHandlingTests` | Route accessibility, HTTP status codes, content types, API versioning, error pages |
| **Middleware** | `SecurityHeadersTests` | Security header presence on real responses |

The `Demo1WebApplicationFactory` fixture (in `Fixtures/`) configures the test server with SQLite in-memory database and test-specific settings.

### Demo1.PlaywrightTests

Browser-based end-to-end tests that launch a headless Chromium instance and interact with the running application.

## Running Tests

```bash
# All tests (unit + integration + Playwright)
dotnet test Demo1.sln -c Release

# Unit tests only
dotnet test tests/Demo1.UnitTests/Demo1.UnitTests.csproj -c Release

# Integration tests only
dotnet test tests/Demo1.IntegrationTests/Demo1.IntegrationTests.csproj -c Release

# Playwright E2E tests only
dotnet test tests/Demo1.PlaywrightTests/Demo1.PlaywrightTests.csproj -c Release
```

**Playwright first-run setup:** Playwright downloads headless browser binaries automatically on first run. Alternatively, install them explicitly:

```bash
dotnet build tests/Demo1.PlaywrightTests
pwsh tests/Demo1.PlaywrightTests/bin/Debug/net9.0/playwright.ps1 install
```

**CI:** The GitHub Actions workflow (`dotnet.yml`) runs all test projects on every push and PR. Playwright CLI is installed automatically.

## Guidelines

- **AAA pattern** — Arrange, Act, Assert in every test method.
- **Mock dependencies** — Use `Moq` for `ILogger<T>`, service interfaces, and other dependencies.
- **Descriptive names** — Test methods follow `MethodUnderTest_Scenario_ExpectedBehavior` naming.
- **XML documentation** — Test classes and complex test methods include `///` summary comments describing the test intent (optional but encouraged).
- **Deterministic & isolated** — No shared mutable state between tests. Each test creates its own service/controller instance.
- **Concurrency tests** — Where applicable (e.g., `InMemorySearchServiceTests`), tests verify thread safety with parallel operations.

## Adding New Tests

1. Add test files to the appropriate project and folder:
   - `tests/Demo1.UnitTests/Controllers/` for controller unit tests
   - `tests/Demo1.UnitTests/Services/` for service unit tests
   - `tests/Demo1.UnitTests/Models/` for model validation tests
   - `tests/Demo1.UnitTests/Middleware/` for middleware tests
   - `tests/Demo1.IntegrationTests/Controllers/` for integration tests
2. Follow existing patterns — look at similar test files for setup and mocking conventions.
3. Run `dotnet test` locally before pushing to verify everything passes.

## Coverage

CI may produce coverage artifacts for badges and checks. To generate coverage locally with Coverlet:

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```
