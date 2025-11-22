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

## Logging

- Configured via `appsettings*.json` under `Logging`.
- Inject `ILogger<T>` into controllers/services.

## Deployment

- Publish output is produced by `dotnet publish` (see GitHub Actions `dotnet.yml`).
- Update `.github/workflows/deploy.yml` with your Azure Web App name and secrets to deploy.
