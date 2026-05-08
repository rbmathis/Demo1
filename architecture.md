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
TagHelpers/             Custom Razor tag helpers
ViewComponents/         Razor ViewComponents for reusable UI
Views/                  Razor views
  Achievement/          Trophy case & anti-pattern demo views
  ComponentShowcase/    Component showcase index and preview pages
  Demo/                 Anti-pattern demo pages (RawSqlSearch, ViewLogicCalculator, CallbackHellWeather, InlineCssHell)
  Home/                 Home, About, Privacy, Contact, error pages
  Performance/          Performance dashboard views
  Profile/              Profile management demo page (GodObjectProfile)
  SecurityLab/          Security lab interactive views
  Shared/               Layout, partials, error pages
    Components/         ViewComponent default views
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
| `SecurityHeadersMiddleware` | Middleware/SecurityHeadersMiddleware.cs | Adds CSP, X-Content-Type-Options, X-Frame-Options (path-aware: `SAMEORIGIN` for `/ComponentShowcase/Preview`, `DENY` elsewhere), Referrer-Policy |
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
| `IComponentRegistryService` | `ComponentRegistryService` | Singleton | Component showcase catalog registry |
| `IAchievementService` | `AchievementService` | Scoped | Achievement data retrieval and progress calculation |
| `AchievementProcessorService` | (self — `BackgroundService`) | Hosted (Singleton) | Consumes `Channel<T>` events, persists and evaluates achievement rules |
| `AchievementDbContext` | (self — `DbContext`) | Scoped | EF Core context for achievement SQLite database |
| `Channel<AchievementEventMessage>` | Bounded channel (1 000 cap, DropOldest) | Singleton | In-memory producer-consumer queue for achievement events |

## Controllers

### MVC Controllers

| Controller | Routes | Purpose |
|-----------|--------|---------|
| `HomeController` | `/`, `/Home/*` | Index, About, Contact, Privacy, error pages |
| `DemoController` | `/Home/RawSqlSearch`, `/Home/ViewLogicCalculator`, `/Home/CallbackHellWeather`, `/Home/InlineCssHell` | Anti-pattern demo pages (routes preserved via `[Route("Home/[action]")]` attribute) |
| `ProfileController` | `/Home/GodObjectProfile`, `/Home/GodObjectProfileUpdate` | Profile management demo (routes preserved via `[Route("Home/[action]")]` attribute) |
| `PerformanceController` | `/Performance/*` | Performance dashboard and budget monitoring |
| `SecurityLabController` | `/SecurityLab/*` | Interactive XSS/injection attack demos |
| `HealthController` | `/health/*` | Health check endpoints |
| `AchievementController` | `/Achievement/*` | Trophy case, badges API (`/Achievement/api/badges`), anti-pattern demo |
| `ComponentShowcaseController` | `/ComponentShowcase/*` | Browsable UI component catalog with isolated previews |

### API Controllers

| Controller | Route | Purpose |
|-----------|-------|---------|
| `WeatherForecastController` (V1) | `/api/v1/weatherforecast` | Weather forecast API v1 |
| `WeatherForecastController` (V2) | `/api/v2/weatherforecast` | Weather forecast API v2 (enhanced) |

## ViewComponents

| ViewComponent | View | Purpose |
|--------------|------|---------|
| `ButtonShowcaseViewComponent` | `Views/Shared/Components/ButtonShowcase/Default.cshtml` | Bootstrap button variants (contextual, outline, sizes) |
| `CardShowcaseViewComponent` | `Views/Shared/Components/CardShowcase/Default.cshtml` | Card layouts (basic, header/footer, list group) |
| `AlertShowcaseViewComponent` | `Views/Shared/Components/AlertShowcase/Default.cshtml` | Alert variants (contextual, dismissible) |
| `FormShowcaseViewComponent` | `Views/Shared/Components/FormShowcase/Default.cshtml` | Form elements (inputs, selects, checkboxes, radios, textarea) |
| `BadgeShowcaseViewComponent` | `Views/Shared/Components/BadgeShowcase/Default.cshtml` | Badge variants (contextual, pill, in heading/button) |

## Custom Tag Helpers

| Tag Helper | Target Element | Purpose |
|-----------|---------------|---------|
| `CopyMarkupTagHelper` | `<copy-markup>` | Renders child HTML as encoded `<pre><code>` block with copy-to-clipboard button |

## Anti-Pattern Showcases

Intentionally bad code for teaching purposes. Each page demonstrates a common mistake:

| Page | Anti-Pattern | Location |
|------|-------------|----------|
| GodObjectProfile | God object — one class does everything | Views/Profile/GodObjectProfile.cshtml |
| InlineCssHell | Inline styles instead of CSS classes | Views/Demo/InlineCssHell.cshtml |
| CallbackHellWeather | Deeply nested async callbacks | Views/Demo/CallbackHellWeather.cshtml |
| ViewLogicCalculator | Business logic in Razor views | Views/Demo/ViewLogicCalculator.cshtml |
| RawSqlSearch | Raw SQL queries (injection risk) | Views/Demo/RawSqlSearch.cshtml |
| SynchronousAntiPattern | Synchronous inline achievement checking (blocks request thread) | Views/Achievement/SynchronousAntiPattern.cshtml |

## Feature Flags

Managed via Azure App Configuration (when configured) or local `appsettings.json`:

| Flag | Purpose | Type |
|------|---------|------|
| `Feature1` | Example toggle | Permanent |
| `DarkMode` | Dark mode UI toggle | Permanent |
| `ContactForm` | Contact form visibility | Permanent |
| `BetaFeatures` | Master toggle for beta features | Permanent |

See `Features/FeatureFlags.cs` for the constant definitions and XML doc conventions.

### Feature Flag Rollout Model

New user-visible, API-affecting, or data-path changes ship **dark by default** behind a feature flag. Activation is always human-controlled — the pipeline never enables flags automatically.

**Rollout classes** (assigned by triage):

| Class | Meaning |
|-------|---------|
| `rollout-required` | User-visible behavior, API changes, side effects, database-affecting work |
| `rollout-optional` | Low-risk/user-invisible, but plan agent must record a deliberate flagging verdict |
| `rollout-exempt` | Docs-only, test-only, internal refactors, build/CI cleanup, emergency security fixes |

**Temporary vs permanent flags:** Temporary rollout flags include owner, cleanup milestone, and cleanup issue reference in `FeatureFlags.cs`. Permanent product flags are explicitly called out in the plan.

**Canonical docs:**
- Rollout contract: [`docs/feature-flag-rollout-contract.md`](docs/feature-flag-rollout-contract.md)
- Runtime guide: [`docs/feature-flag-runtime-guide.md`](docs/feature-flag-runtime-guide.md)

### Database Migration Model

Schema changes use **explicit EF Core migrations** with expand/backfill/switch/contract sequencing. The web app is not the primary schema migrator.

| Environment | Migration Runner |
|-------------|-----------------|
| Local dev | `db.Database.Migrate()` at startup (Development/Testing env only) |
| Integration tests | `Demo1WebApplicationFactory` applies `Migrate()` in fixture setup |
| CI/CD | `dotnet ef database update` or migration bundle before deployment |
| Production | Human-initiated via deployment pipeline |

```bash
# Generate a new migration
dotnet ef migrations add MigrationName --context AchievementDbContext --output-dir Data/Migrations

# Apply migrations externally
dotnet ef database update --context AchievementDbContext
```

### Flag-Off / Flag-On Example

Using `DarkMode` as the example flag:

| State | Behavior |
|-------|----------|
| **Flag off (default)** | Standard light theme renders; `<feature name="DarkMode">` blocks in `_Layout.cshtml` are hidden; no dark-mode CSS loads |
| **Flag on** | Dark-mode CSS loads; UI elements inside `<feature name="DarkMode">` blocks become visible; `[FeatureGate(FeatureFlags.DarkMode)]` endpoints return 200 instead of 404 |

The `[FeatureGate]` attribute, `IFeatureManager` checks, and Razor `<feature>` tag helpers are the three gating mechanisms. See the runtime guide for patterns.

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

### Test Coverage Summary

| Project | Tests | Categories |
|---------|-------|------------|
| `Demo1.UnitTests` | ~155 | Controllers (7 files), Services (8 files), TagHelpers (1 file), Models (1 file), Middleware (4 files), Telemetry (1 file) |
| `Demo1.IntegrationTests` | ~44 | Controller routes (7 files), Middleware headers (1 file) |
| `Demo1.PlaywrightTests` | ~12 | Browser-based E2E smoke tests |

**Total: ~211 tests** across unit, integration, and E2E layers. See [`docs/testing.md`](docs/testing.md) for full details.

## Containerization

Multi-stage Dockerfile with three stages:

| Stage | Base Image | Purpose |
|-------|-----------|---------|
| `build` | `mcr.microsoft.com/dotnet/sdk:10.0` | Restore, publish |
| `dev` | `mcr.microsoft.com/dotnet/sdk:10.0` | Hot-reload via `dotnet watch` (opt-in) |
| `runtime` | `mcr.microsoft.com/dotnet/aspnet:10.0` | Production image |

Supports build args `VERSION` and `REVISION`. Docker Compose (`docker-compose.yml`) includes:

- `demo1` — standard service on port `5555`
- `demo1-dev` — hot-reload profile (`--profile dev`) on port `5556` with source bind-mount
- `redis` — Redis 7 for distributed caching

CI image builds always run; push to GHCR is gated by the `GHCR_TOKEN` secret (see `deploy.yml`).

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
