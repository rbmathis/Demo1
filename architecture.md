# Architecture

Technical reference for the Demo1 ASP.NET Core MVC application. This is the single source of truth for solution structure, dependencies, middleware pipeline, services, and infrastructure. Keep this file updated when meaningful code changes land.

## Solution Overview

| Property | Value |
|----------|-------|
| Framework | ASP.NET Core MVC (.NET 10) |
| SDK | `Microsoft.NET.Sdk.Web` |
| Root namespace | `Demo1` |
| Entry point | `Program.cs` (minimal hosting) |
| Versioning | Semantic — read from `VERSION` file at build time |

## Folder Layout

```text
Controllers/            MVC controllers
  Api/V1/               Versioned API controllers (v1)
  Api/V2/               Versioned API controllers (v2)
Data/                   EF Core DbContext classes
Features/               Feature flag constants
Middleware/              Custom middleware components
Models/                 View models, API models, domain models
  Api/                  API response models
Services/               Service interfaces and implementations
Telemetry/              Custom telemetry initializers
Views/                  Razor views
  Achievement/          Trophy case & anti-pattern demo views
  Home/                 Home, About, Privacy, anti-pattern pages
  Performance/          Performance dashboard views
  SecurityLab/          Security lab interactive views
  Shared/               Layout, partials, error pages
wwwroot/                Static assets (css, js, lib)
docs/                   Developer documentation
tests/
  Demo1.UnitTests/      xUnit unit tests
  Demo1.IntegrationTests/ Integration tests (WebApplicationFactory)
  Demo1.PlaywrightTests/  Browser-based E2E tests
Properties/             launchSettings.json
scripts/                Build, commit, and CI helper scripts
```

## NuGet Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Asp.Versioning.Mvc | 8.1.0 | URL-segment API versioning |
| Asp.Versioning.Mvc.ApiExplorer | 8.1.0 | Swagger integration for versioned APIs |
| Microsoft.ApplicationInsights.AspNetCore | 2.23.0 | Application Insights telemetry |
| Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel | 2.23.0 | Server telemetry channel |
| Microsoft.Azure.AppConfiguration.AspNetCore | 8.4.0 | Azure App Configuration + feature flags |
| Microsoft.Extensions.Caching.StackExchangeRedis | 9.0.0 | Redis distributed cache (optional) |
| Microsoft.FeatureManagement.AspNetCore | 4.3.0 | Feature flag management |
| Swashbuckle.AspNetCore | 7.2.0 | Swagger/OpenAPI generation |
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.0 | SQLite database provider for achievement persistence |
| Microsoft.EntityFrameworkCore.Design | 10.0.0 | EF Core design-time tools (migrations) |
| Microsoft.EntityFrameworkCore.Tools | 10.0.0 | EF Core CLI tools |

## Middleware Pipeline

Order matters — this is the actual registration order in `Program.cs`:

```text
1. ExceptionHandler / HSTS          (production only)
2. Swagger UI                       (dev or config-enabled)
3. HTTPS Redirection
4. ServerTimingMiddleware            (custom — adds Server-Timing header)
5. SecurityHeadersMiddleware         (custom — CSP, X-Frame-Options, etc.)
6. SecurityLabMiddleware             (custom — relaxed headers for security lab pages)
7. StatusCodePages                   (re-execute to /Home/Error{code})
8. Routing
9. RateLimiter                       (IP-based fixed-window)
10. RateLimitHeadersMiddleware       (custom — X-RateLimit-* headers)
11. Session                          (required for achievement tracking)
12. AchievementMiddleware             (custom — publishes events to Channel<T>)
13. AzureAppConfiguration            (feature flag refresh, when configured)
14. Authorization
15. Static Assets / Controllers
16. Health Checks                    (/health/ready)
```

## Custom Middleware

| Middleware | File | Purpose |
|-----------|------|---------|
| `ServerTimingMiddleware` | Middleware/ServerTimingMiddleware.cs | Adds `Server-Timing` header with request duration |
| `SecurityHeadersMiddleware` | Middleware/SecurityHeadersMiddleware.cs | Adds CSP, X-Content-Type-Options, X-Frame-Options, Referrer-Policy |
| `SecurityLabMiddleware` | Middleware/SecurityLabMiddleware.cs | Relaxes security headers on `/SecurityLab/*` routes for demo purposes |
| `RateLimitHeadersMiddleware` | Middleware/RateLimitHeadersMiddleware.cs | Adds `X-RateLimit-Limit` and `X-RateLimit-Remaining` headers |
| `AchievementMiddleware` | Middleware/AchievementMiddleware.cs | Publishes request events to `Channel<AchievementEventMessage>` for async badge processing |

## Registered Services

| Interface | Implementation | Lifetime | Purpose |
|-----------|---------------|----------|---------|
| `ISecurityLabService` | `SecurityLabService` | Scoped | Security lab attack scenario management |
| `ISearchService` | `InMemorySearchService` | Singleton | In-memory search (anti-pattern demo) |
| `IWeatherService` | `MockWeatherService` | Singleton | Mock weather data for demos |
| `IUserProfileService` | `InMemoryUserProfileService` | Singleton | User profile management (anti-pattern demo) |
| `IStyleGeneratorService` | `StyleGeneratorService` | Singleton | CSS style generation for demos |
| `IUptimeService` | `UptimeService` | Singleton | Application uptime tracking |
| `IPerformanceMetricsService` | `PerformanceMetricsService` | Singleton | Performance budget monitoring |
| `IAchievementService` | `AchievementService` | Scoped | Achievement data retrieval and progress calculation |
| `AchievementProcessorService` | (self — `BackgroundService`) | Hosted (Singleton) | Consumes `Channel<T>` events, persists and evaluates achievement rules |
| `AchievementDbContext` | (self — `DbContext`) | Scoped | EF Core context for achievement SQLite database |
| `Channel<AchievementEventMessage>` | Bounded channel (1 000 cap, DropOldest) | Singleton | In-memory producer-consumer queue for achievement events |

## Controllers

### MVC Controllers

| Controller | Routes | Purpose |
|-----------|--------|---------|
| `HomeController` | `/`, `/Home/*` | Index, About, Contact, Privacy, anti-pattern demo pages |
| `PerformanceController` | `/Performance/*` | Performance dashboard and budget monitoring |
| `SecurityLabController` | `/SecurityLab/*` | Interactive XSS/injection attack demos |
| `HealthController` | `/health/*` | Health check endpoints |
| `AchievementController` | `/Achievement/*` | Trophy case, badges API (`/Achievement/api/badges`), anti-pattern demo |

### API Controllers

| Controller | Route | Purpose |
|-----------|-------|---------|
| `WeatherForecastController` (V1) | `/api/v1/weatherforecast` | Weather forecast API v1 |
| `WeatherForecastController` (V2) | `/api/v2/weatherforecast` | Weather forecast API v2 (enhanced) |

## Anti-Pattern Showcases

Intentionally bad code for teaching purposes. Each page demonstrates a common mistake:

| Page | Anti-Pattern | Location |
|------|-------------|----------|
| GodObjectProfile | God object — one class does everything | Views/Home/GodObjectProfile.cshtml |
| InlineCssHell | Inline styles instead of CSS classes | Views/Home/InlineCssHell.cshtml |
| CallbackHellWeather | Deeply nested async callbacks | Views/Home/CallbackHellWeather.cshtml |
| ViewLogicCalculator | Business logic in Razor views | Views/Home/ViewLogicCalculator.cshtml |
| RawSqlSearch | Raw SQL queries (injection risk) | Views/Home/RawSqlSearch.cshtml |
| SynchronousAntiPattern | Synchronous inline achievement checking (blocks request thread) | Views/Achievement/SynchronousAntiPattern.cshtml |

## Feature Flags

Managed via Azure App Configuration (when configured) or local `appsettings.json`:

| Flag | Purpose |
|------|---------|
| `Feature1` | Example toggle |
| `DarkMode` | Dark mode UI toggle |
| `ContactForm` | Contact form visibility |
| `BetaFeatures` | Master toggle for beta features |

## External Integrations

| Service | Required? | Configuration |
|---------|-----------|---------------|
| Azure App Configuration | Optional | `AZUREAPPCONFIGURATION__ENDPOINT` or connection string |
| Application Insights | Optional | `APPLICATIONINSIGHTS__CONNECTIONSTRING` |
| Redis | Optional | `Redis:ConnectionString` or `REDIS_CONNECTIONSTRING` (falls back to in-memory cache) |

## Build & Test

```bash
# Build
dotnet build Demo1.sln -c Release

# Unit tests
dotnet test tests/Demo1.UnitTests/Demo1.UnitTests.csproj -c Release

# Integration tests
dotnet test tests/Demo1.IntegrationTests/Demo1.IntegrationTests.csproj -c Release

# All tests
dotnet test Demo1.sln -c Release

# Run locally
dotnet run
```

## Containerization

Multi-stage Dockerfile using `mcr.microsoft.com/dotnet/sdk:9.0` for build and runtime. Supports build args for `VERSION` and `REVISION`. Docker Compose available via `docker-compose.yml`.

## CI/CD Pipeline

### GitHub Actions (Traditional)

| Workflow | Trigger | Purpose |
|---------|---------|---------|
| `dotnet.yml` | Push, PR | Build, test, coverage validation |
| `deploy.yml` | Push to main | Production deployment |

### Cloud Agentic Pipeline (gh-aw)

Automated AI-driven pipeline triggered by labeling an issue with `cloud/autopilot`:

```text
Autopilot → Triage → Plan → Implement → ⏸️ (apply cloud/review label) → Review → Docs → Finish
```

- Source files: `.github/workflows/cloud-*.md`
- Compiled output: `.github/workflows/cloud-*.lock.yml`
- Compile command: Delete lock files first, then `gh aw compile`

See [AI-SDLC-CLOUD.md](AI-SDLC-CLOUD.md) for full pipeline documentation.

## Achievement System

### Architecture Overview

The achievement system tracks user actions across the site and awards badges when criteria are met. It uses an **async producer-consumer pattern** to avoid blocking the request pipeline.

```text
┌─────────────┐    ┌────────────────────┐    ┌──────────────────────────┐    ┌──────────┐
│   Request    │───▶│ AchievementMiddle  │───▶│ Channel<AchievementEvent │───▶│ Background│
│   Pipeline   │    │     ware           │    │       Message>           │    │ Service   │
│              │    │ (non-blocking      │    │ (bounded, 1000 cap,      │    │ (reads &  │
│              │    │  TryWrite)         │    │  DropOldest)             │    │  persists)│
└─────────────┘    └────────────────────┘    └──────────────────────────┘    └─────┬─────┘
                                                                                   │
                                                                           ┌───────▼───────┐
                                                                           │ SQLite DB      │
                                                                           │ (achievements  │
                                                                           │  .db)          │
                                                                           └───────────────┘
```

### Data Flow

1. **AchievementMiddleware** wraps `_next()` to capture response status codes
2. After `_next()` completes, middleware creates an `AchievementEventMessage` DTO
3. Middleware calls `channel.Writer.TryWrite()` (non-blocking, fire-and-forget)
4. **AchievementProcessorService** (BackgroundService) reads from the channel
5. Processor persists the event as an `AchievementEvent` entity in SQLite
6. Processor evaluates all unearned achievement rules for the session
7. If criteria are met, a `UserAchievement` record is created

### Models

| Entity | Purpose |
|--------|---------|
| `Achievement` | Badge definition (seeded): name, icon, trigger type, target |
| `UserAchievement` | Records when a session earns a badge |
| `AchievementEvent` | Persisted event history for progress queries |
| `AchievementEventMessage` | In-memory DTO flowing through the channel (not an entity) |
| `TrophyCaseViewModel` | View model for the trophy case page |
| `AchievementProgress` | Progress toward a single achievement |

### Achievement Rules

| Badge | Trigger Type | Target | Condition |
|-------|-------------|--------|-----------|
| Explorer | PageVisitCount | 5 | Visit 5 distinct pages |
| Speed Demon | RateLimited | 1 | Receive a 429 status code |
| White Hat | SecurityLabXss | 1 | Access /SecurityLab/Attack |
| Benchmarker | SpecificPage | 1 | Visit /Performance/Dashboard |
| API Curious | ApiCall | 1 | Call any /api/ endpoint |
| Completionist | AllPages | 8 | Visit all 8 core pages |

### Session-Based Tracking

- Uses the existing ASP.NET Core session (`.Demo1.Session` cookie, 30-min timeout)
- Session ID is the user identity key — no authentication required
- Achievements persist in SQLite but are scoped to a session lifetime

### Anti-Pattern Comparison

The `SynchronousAntiPattern` action demonstrates the wrong way:
- Synchronous DB queries inline with the request
- Blocking the thread pool with every page load
- Latency scales linearly with event count

The proper implementation (Channel + BackgroundService) has zero impact on request latency.
