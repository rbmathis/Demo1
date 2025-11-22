# ⚙️ Configuration & Environments

## Files

- `appsettings.json`: Base configuration
- `appsettings.Development.json`: Dev overrides
- Add more environment files as needed (e.g., `appsettings.Production.json`)

## Environment Variables

ASP.NET Core uses the `ASPNETCORE_ENVIRONMENT` variable (`Development`, `Staging`, `Production`).

Common settings:

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

- `SamplingTelemetryInitializer`: Configures fixed sampling percentage

To add custom telemetry initializers, implement `ITelemetryInitializer` and register in `Program.cs`.

## HTTPS & Security

- `UseHttpsRedirection()` is enabled by default in non-development environments.
- Configure authentication/authorization via `builder.Services.AddAuthentication()`/`AddAuthorization()`
- Add security headers middleware as needed.

## Logging

- Configured via `appsettings*.json` under `Logging`.
- Inject `ILogger<T>` into controllers/services.

## Deployment

- Publish output is produced by `dotnet publish` (see GitHub Actions `dotnet.yml`).
- Update `.github/workflows/deploy.yml` with your Azure Web App name and secrets to deploy.
