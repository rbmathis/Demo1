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

## Secret Management

- Use **User Secrets** for local development: `dotnet user-secrets init`
- Use **Azure Key Vault** or similar in production
- Never commit secrets to source control

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
