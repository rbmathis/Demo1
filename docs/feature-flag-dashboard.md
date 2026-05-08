# Feature Flag Admin Dashboard

Operational reference for the admin dashboard at `/FeatureFlag`. This page lets authorised operators inspect all known application feature flags and toggle them in real time when Azure App Configuration is the backing store.

## Prerequisites

| Requirement | Detail |
|-------------|--------|
| Azure App Configuration | Optional — when not configured, the dashboard is read-only |
| Admin credentials | Set via `ADMINDASHBOARD__USERNAME` and `ADMINDASHBOARD__PASSWORD` environment variables |
| Cookie auth | Enabled automatically; no additional infrastructure needed |

## Authentication & Authorisation

The dashboard uses **cookie-based authentication** with an `AdminOnly` authorisation policy. Only users with the `Admin` role can view the dashboard or submit toggle requests.

### Configuring Admin Credentials

Set the following environment variables (or `AdminDashboard` section in `appsettings.json`):

```bash
ADMINDASHBOARD__USERNAME=admin
ADMINDASHBOARD__PASSWORD=<strong-secret>
```

> ⚠ **Never** set a non-empty password in `appsettings.json` and commit it. Use environment variables or user secrets in development.

If `ADMINDASHBOARD__PASSWORD` is empty, all login attempts are rejected with an error message.

### Login Flow

1. Navigate to `/AdminAuth/Login`.
2. Submit username and password.
3. On success, an HTTP-only, `SameSite=Strict` session cookie (`.Demo1.Admin`) is issued and the user is redirected to `/FeatureFlag`.
4. The session expires after 8 hours of inactivity or on explicit sign-out.

## Dashboard Behaviour

### When Azure App Configuration Is Configured

- All known flags are listed with their current enabled/disabled state.
- A **Enable** or **Disable** button appears next to each flag.
- Submitting the form sends a POST to `/FeatureFlag/Toggle` with CSRF protection.
- The backing Azure App Configuration store is updated via `FeatureFlagConfigurationSetting`.
- An informational banner tells the user how long changes may take to propagate (default: **30 seconds** refresh interval).

### When Azure App Configuration Is Not Configured (Read-only Mode)

- Flags are read from `appsettings.json` / local configuration.
- Toggle buttons are hidden and a warning banner is shown.
- No write operations are attempted and no `appsettings.json` file is modified on disk.

## Refresh Timing

Flag state is refreshed from Azure App Configuration on a **30-second** interval (configured in `Program.cs` via `SetRefreshInterval`). After toggling a flag, the change will be visible to all application instances within this interval. The dashboard displays this interval in a banner so operators know what to expect.

## CSRF Protection

All state-changing endpoints use `[ValidateAntiForgeryToken]`. Toggle forms in the view include `@Html.AntiForgeryToken()`. Requests without a valid token receive a `400 Bad Request` response.

## Audit Logging

Flag changes are logged via Serilog at the `Information` level. Each log entry includes:

- The flag name
- The new state (`true` / `false`)
- The Azure App Configuration label
- The authenticated admin username

Example log entry:

```
[2025-01-15 14:32:10.123 +00:00 INF] [Server1][42] Demo1.Controllers.FeatureFlagController — Feature flag changed — flag: DarkMode, new state: True, admin: admin
```

Failed toggle attempts are logged at `Warning` level.

## Security Considerations

| Control | Implementation |
|---------|---------------|
| Authentication | Cookie auth — credentials validated with constant-time comparison (`CryptographicOperations.FixedTimeEquals`) |
| Authorisation | `[Authorize(Policy = "AdminOnly")]` on the controller |
| CSRF | `[ValidateAntiForgeryToken]` on all POST actions |
| Cookie security | `HttpOnly=true`, `SameSite=Strict`, `SecurePolicy=SameAsRequest` |
| Flag name validation | Only flags defined in `FeatureFlags.cs` are accepted; unknown names are rejected |
| No disk writes | `appsettings.json` is never modified at runtime |

## Known Flags

| Flag | Default | Purpose |
|------|---------|---------|
| `Feature1` | `false` | Example toggle — gates the Feature1 demo page |
| `DarkMode` | `false` | Dark mode UI toggle |
| `ContactForm` | `false` | Contact form visibility |
| `BetaFeatures` | `false` | Master toggle for beta features |

To add a new flag: declare a constant in `Features/FeatureFlags.cs`, add a default in `appsettings.json`, and add the name to the `KnownFlags` array in `FeatureFlagService.cs`.

## Service Architecture

```
AdminAuthController  ──► Cookie auth signin/signout
      │
      ▼
FeatureFlagController  (requires AdminOnly policy)
      │
      ▼
IFeatureFlagService / FeatureFlagService
      │
      ├── Read  ──► IFeatureManager  (Microsoft.FeatureManagement)
      │                │
      │                └── Azure App Configuration (when configured)
      │                    or appsettings.json (fallback)
      │
      └── Write ──► Azure.Data.AppConfiguration.ConfigurationClient
                    (FeatureFlagConfigurationSetting)
```

`AzureAppConfigAdminOptions` is a singleton registered in `Program.cs` that carries the connectivity information (`IsAvailable`, `Endpoint`, `ConnectionString`, `Label`) derived from the startup configuration provider setup.

## Running Locally Without Azure App Configuration

The dashboard operates in read-only mode when no Azure App Configuration is configured:

```bash
# appsettings.Development.json (or environment):
# AzureAppConfiguration:Endpoint is empty → read-only mode

dotnet run
# Navigate to /AdminAuth/Login, sign in with configured credentials
# Dashboard shows flags from appsettings.json, no toggle buttons
```

## Configuring Azure App Configuration for Local Development

```bash
export AZUREAPPCONFIGURATION__ENDPOINT="https://<your-store>.azconfig.io"
export ADMINDASHBOARD__USERNAME="admin"
export ADMINDASHBOARD__PASSWORD="<local-secret>"

dotnet run
# Navigate to /AdminAuth/Login
# Dashboard shows live flags with toggle capability
```

Ensure your local identity (DefaultAzureCredential — typically Azure CLI) has the **App Configuration Data Owner** role on the store.
