# ⚙️ Configuration & Environments

## Files

- `appsettings.json`: Base configuration
- `appsettings.Development.json`: Dev overrides
- Add more environment files as needed (e.g., `appsettings.Production.json`)

## Environment Variables

ASP.NET Core uses the `ASPNETCORE_ENVIRONMENT` variable (`Development`, `Staging`, `Production`).

Common settings:

- `ConnectionStrings__AchievementDb` for achievement system SQLite database (default: `Data Source=achievements.db`)
- `ConnectionStrings__Default` for database connections
- `Logging__LogLevel__Default` for logging verbosity
- `ApplicationInsights__ConnectionString` for Application Insights connection

## Secret Management

- Use **User Secrets** for local development: `dotnet user-secrets init`
- Use **Azure Key Vault** or similar in production
- Never commit secrets to source control

## Application Insights Configuration

Application Insights telemetry is integrated to capture requests, dependencies, exceptions, and traces.

### Connection String

Set the connection string via configuration:

- In `appsettings.json`: Update `ApplicationInsights:ConnectionString` (leave empty for local dev)
- Via User Secrets: `dotnet user-secrets set "ApplicationInsights:ConnectionString" "InstrumentationKey=..."`
- Via Environment Variable: `ApplicationInsights__ConnectionString`
- In Azure: Set application setting `ApplicationInsights__ConnectionString` with your App Insights connection string

### Sampling Configuration

Control the percentage of telemetry sent to Application Insights:

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=your-key;IngestionEndpoint=...",
    "SamplingPercentage": 100.0
  }
}
```

- `SamplingPercentage`: Value between 0 and 100 (default: 100.0)
  - 100.0 = capture all telemetry
  - 50.0 = capture 50% of telemetry
  - 10.0 = capture 10% of telemetry

### Telemetry Captured

Application Insights automatically captures:

- **Requests**: HTTP requests to controllers
- **Dependencies**: Outbound HTTP calls, database queries, etc.
- **Exceptions**: Unhandled exceptions
- **Traces**: Log messages from ILogger

### Telemetry Initializers

Custom telemetry initializers are configured in `Program.cs`:

- `CustomTelemetryInitializer`: Adds custom properties (like ApplicationName) to all telemetry items

To add custom telemetry initializers, implement `ITelemetryInitializer` and register in `Program.cs`.

### Sampling

Sampling is configured using the `SamplingPercentage` setting. The SDK uses fixed-rate sampling to control the percentage of telemetry sent to Application Insights. This helps manage costs and data volume while maintaining representative data.

## Achievement System

The achievement system uses an SQLite database for persistence. The database file is created automatically on first run.

### Connection String

```json
{
  "ConnectionStrings": {
    "AchievementDb": "Data Source=achievements.db"
  }
}
```

- **Default**: `Data Source=achievements.db` (file created in the application root)
- **Override via environment variable**: `ConnectionStrings__AchievementDb`
- The database is auto-created with seed data on startup (`db.Database.EnsureCreated()`)
- No migrations are needed — the schema is managed by EF Core's `EnsureCreated()`

### Channel Configuration

The bounded channel for async event processing is configured in `Program.cs` with these defaults:

| Setting | Value | Purpose |
|---------|-------|---------|
| Capacity | 1 000 | Maximum queued events before dropping |
| FullMode | DropOldest | Drops oldest event when channel is full |
| SingleReader | true | Optimized for single background consumer |

These values are hardcoded in `Program.cs`. To change them, modify the `BoundedChannelOptions` constructor call.

## HTTPS & Security

### HTTPS Redirection

- `UseHttpsRedirection()` is enabled in all environments to ensure secure communication.
- Configure HTTPS port in `Properties/launchSettings.json`.

### HSTS (HTTP Strict Transport Security)

- `UseHsts()` is enabled for non-development environments.
- Default max-age is 30 days. Configure via `builder.Services.AddHsts()` if needed.
- HSTS tells browsers to only access the site via HTTPS for the specified duration.

### Security Headers

The application includes a custom `SecurityHeadersMiddleware` that adds the following headers:

- **X-Content-Type-Options**: `nosniff` - Prevents MIME type sniffing
- **X-Frame-Options**: `DENY` - Prevents clickjacking by disabling iframe embedding
- **X-XSS-Protection**: `1; mode=block` - Enables browser XSS filtering
- **Referrer-Policy**: `strict-origin-when-cross-origin` - Controls referrer information
- **Content-Security-Policy**: Restricts resource loading to trusted sources
  - `default-src 'self'` - Only allow resources from same origin by default
  - `script-src 'self' 'unsafe-inline' 'unsafe-eval'` - Allow inline scripts (required for Bootstrap/jQuery)
  - `style-src 'self' 'unsafe-inline'` - Allow inline styles
  - `img-src 'self' data:` - Allow images from same origin and data URIs
  - `font-src 'self'` - Allow fonts from same origin
  - `connect-src 'self'` - Allow AJAX requests to same origin

**Note**: The CSP policy can be customized in `Middleware/SecurityHeadersMiddleware.cs` based on your application's needs.

### Authentication & Authorization

- Configure authentication/authorization via `builder.Services.AddAuthentication()`/`AddAuthorization()`
- See issue #4 for Azure AD integration plans.

## Logging (Serilog)

Demo1 uses [Serilog](https://serilog.net/) as its structured logging provider, replacing the default ASP.NET Core logger. Serilog provides rich, structured log events with multiple output sinks, contextual enrichment, and fine-grained level control — all configured via `appsettings.json`.

### NuGet Packages

| Package | Purpose |
|---------|---------|
| `Serilog.AspNetCore` | Core integration with ASP.NET Core host |
| `Serilog.Sinks.Console` | Writes structured logs to stdout |
| `Serilog.Sinks.File` | Writes logs to rolling daily files |
| `Serilog.Enrichers.Environment` | Adds `MachineName` to log events |
| `Serilog.Enrichers.Thread` | Adds `ThreadId` to log events |

### Configuration

Serilog reads its settings from the `Serilog` section in `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning",
        "Demo1": "Debug"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/demo1-.log",
          "rollingInterval": "Day",
          "fileSizeLimitBytes": 10485760,
          "retainedFileCountLimit": 7,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{MachineName}][{ThreadId}] {SourceContext} — {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

### Sinks

#### Console Sink

The console sink writes to stdout using a compact, human-readable template designed for local development:

```
[10:13:59 INF] Request finished HTTP/1.1 GET /Home/Index
```

#### File Sink

The file sink writes to daily rolling log files with machine and thread context for production diagnostics.

| Setting | Value | Description |
|---------|-------|-------------|
| `path` | `logs/demo1-.log` | Output path; date stamp inserted automatically (e.g., `demo1-20260508.log`) |
| `rollingInterval` | `Day` | Rolls to a new file each calendar day |
| `fileSizeLimitBytes` | `10485760` (10 MB) | Maximum size per log file before rolling |
| `retainedFileCountLimit` | `7` | Keeps the most recent 7 log files; older files are deleted automatically |

### Enrichers

Enrichers attach contextual properties to every log event:

| Enricher | Property Added | Description |
|----------|---------------|-------------|
| `FromLogContext` | *(varies)* | Includes properties pushed onto `LogContext` (e.g., request-scoped values) |
| `WithMachineName` | `MachineName` | The hostname of the server emitting the log |
| `WithThreadId` | `ThreadId` | The managed thread ID that produced the event |

### Per-Namespace Level Overrides

Serilog applies namespace-level minimum log levels to reduce noise from framework internals while keeping application logs verbose:

| Namespace | Minimum Level | Rationale |
|-----------|--------------|-----------|
| *(Default)* | `Information` | Baseline for all sources |
| `Microsoft.AspNetCore` | `Warning` | Suppresses routine ASP.NET Core request plumbing |
| `Microsoft.EntityFrameworkCore` | `Warning` | Suppresses EF Core SQL and change-tracker chatter |
| `System` | `Warning` | Suppresses low-level runtime messages |
| `Demo1` | `Debug` | Enables verbose logging for application code |

### Environment Overrides

`appsettings.Development.json` can override Serilog levels for local development. For example, to increase framework verbosity during debugging:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft.AspNetCore": "Information"
      }
    }
  }
}
```

### Environment Variables

Serilog configuration values can be overridden at runtime via environment variables using the standard ASP.NET Core `__` (double-underscore) separator:

```bash
# Set the default minimum level
Serilog__MinimumLevel__Default=Debug

# Override a specific namespace
Serilog__MinimumLevel__Override__Microsoft.AspNetCore=Information
```

This is useful in containerized or CI/CD environments where you cannot modify `appsettings.json`.

### Request Logging

`UseSerilogRequestLogging()` is called in the middleware pipeline to emit a single structured log event per HTTP request, replacing the multiple events produced by default ASP.NET Core logging:

```
[10:13:59 INF] HTTP GET /Home/Index responded 200 in 42.3 ms
```

This provides a concise, performance-friendly summary of every request/response cycle.

### Application Insights Coexistence

Serilog and Application Insights operate side-by-side. All messages written through `ILogger<T>` flow to both Serilog sinks (console, file) and the Application Insights telemetry channel. No additional configuration is needed — the ASP.NET Core logging abstraction routes to all registered providers.

### Usage

Inject `ILogger<T>` into any controller or service using standard dependency injection:

```csharp
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Home page requested");
        return View();
    }
}
```

Use structured logging placeholders (not string interpolation) to preserve queryable properties:

```csharp
// ✅ Correct — structured property
_logger.LogInformation("User {UserId} viewed {Page}", userId, pageName);

// ❌ Avoid — loses structure
_logger.LogInformation($"User {userId} viewed {pageName}");
```

## Rate Limiting

The application includes built-in rate limiting using `Microsoft.AspNetCore.RateLimiting` to prevent API abuse and DDoS attacks.

### Configuration

Add or modify the `RateLimiting` section in `appsettings.json`:

```json
{
  "RateLimiting": {
    "PermitLimit": 100,
    "WindowInSeconds": 60,
    "QueueLimit": 0
  }
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| `PermitLimit` | 100 | Maximum number of requests allowed within the time window |
| `WindowInSeconds` | 60 | Duration of the rate limiting window in seconds |
| `QueueLimit` | 0 | Number of requests to queue when the limit is reached (0 = reject immediately) |

### How It Works

- **IP-based partitioning**: Each client IP address gets its own rate limit window. Requests are tracked per IP using a fixed-window algorithm.
- **Fixed window**: The window resets after `WindowInSeconds` elapses. All requests within a window count toward the `PermitLimit`.
- **Queue processing**: If `QueueLimit` > 0, excess requests are queued (oldest first) instead of being immediately rejected.

### Response Headers

Every response includes rate limit headers:

| Header | Description |
|--------|-------------|
| `X-RateLimit-Limit` | The maximum number of requests permitted in the current window |
| `X-RateLimit-Remaining` | The number of requests remaining in the current window |

When a request is rejected (rate limited):

| Header | Description |
|--------|-------------|
| `Retry-After` | Number of seconds the client should wait before retrying |

### Rate Limited Response

When the rate limit is exceeded, the server responds with:

- **Status**: `429 Too Many Requests`
- **Body**: `Too many requests. Please try again later.`
- **Headers**: `Retry-After`, `X-RateLimit-Limit`, `X-RateLimit-Remaining: 0`

### Customizing Limits

To adjust rate limits per environment, override the settings in environment-specific configuration:

```json
// appsettings.Production.json
{
  "RateLimiting": {
    "PermitLimit": 200,
    "WindowInSeconds": 60,
    "QueueLimit": 10
  }
}
```

Or use environment variables:

```
RateLimiting__PermitLimit=200
RateLimiting__WindowInSeconds=60
RateLimiting__QueueLimit=10
```

## Deployment

- Publish output is produced by `dotnet publish` (see GitHub Actions `dotnet.yml`).
- Update `.github/workflows/deploy.yml` with your Azure Web App name and secrets to deploy.
