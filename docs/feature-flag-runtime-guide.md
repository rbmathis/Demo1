# Feature Flag Runtime Guide

> Conventions for implementing flagged changes in the Demo1 application.
> See `docs/feature-flag-rollout-contract.md` for the rollout policy contract.

## Table of Contents

- [Default-Off Branching](#default-off-branching)
- [Side-Effect Suppression](#side-effect-suppression)
- [API Branching](#api-branching)
- [Test Seams](#test-seams)
- [Migration Conventions](#migration-conventions)
- [Cleanup Mechanics](#cleanup-mechanics)
- [Backfill Gate](#backfill-gate)

---

## Default-Off Branching

All new feature flags default to **off**. The existing behavior (old path) runs when the flag is off; new behavior runs only when the flag is on.

### Whole-Route Gating with `[FeatureGate]`

Use `[FeatureGate]` to hide an entire controller action behind a flag. When the flag is off, the framework returns 404.

```csharp
using Microsoft.FeatureManagement.Mvc;

[FeatureGate(FeatureFlags.MyNewFeature)]
public IActionResult NewFeaturePage()
{
    return View();
}
```

### Behavior Branching with `IFeatureManager`

Use `IFeatureManager` or `IFeatureManagerSnapshot` for conditional logic within a controller or service:

```csharp
public class MyController : Controller
{
    private readonly IFeatureManager _featureManager;

    public MyController(IFeatureManager featureManager)
    {
        _featureManager = featureManager;
    }

    public async Task<IActionResult> Index()
    {
        if (await _featureManager.IsEnabledAsync(FeatureFlags.MyNewFeature))
        {
            // New behavior
            return View("NewIndex", newViewModel);
        }

        // Old behavior (default when flag is off)
        return View("Index", existingViewModel);
    }
}
```

Use `IFeatureManagerSnapshot` when the same flag is checked multiple times in a single request — it caches the result.

### Razor View Gating with `<feature>`

Use the `<feature>` tag helper to conditionally show/hide UI elements:

```html
<feature name="MyNewFeature">
    <li><a href="/new-page">New Page</a></li>
</feature>
```

The `<feature>` tag requires `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers` (already registered in `_ViewImports.cshtml`).

### Adding a New Flag

1. Add a constant to `Features/FeatureFlags.cs` in the appropriate section (temporary or permanent)
2. Add the flag to `appsettings.json` under `FeatureManagement` with value `false`
3. Use the flag constant in code — never use string literals for flag names

```csharp
// In FeatureFlags.cs
public const string MyNewFeature = "MyNewFeature";
```

```json
// In appsettings.json
"FeatureManagement": {
    "MyNewFeature": false
}
```

---

## Side-Effect Suppression

When a flag is off, **no new side effects should execute**. This is the default expectation.

### Guard Pattern for Side Effects

```csharp
public async Task ProcessOrderAsync(Order order)
{
    // Existing behavior always runs
    await SaveOrderAsync(order);

    // New side effect only fires when flag is on
    if (await _featureManager.IsEnabledAsync(FeatureFlags.OrderNotifications))
    {
        await _emailService.SendOrderConfirmationAsync(order);
        _logger.LogInformation(
            "Order notification sent for {OrderId} (flag: {Flag})",
            order.Id, FeatureFlags.OrderNotifications);
    }
    else
    {
        _logger.LogDebug(
            "Order notification suppressed for {OrderId} (flag: {Flag} off)",
            order.Id, FeatureFlags.OrderNotifications);
    }
}
```

### Side Effects That Must Be Guarded

- Email or SMS sends
- External API calls (webhooks, third-party integrations)
- Background job enqueuing
- Database writes for the new feature path
- File system or blob storage operations for the new feature

### Exceptions to Suppression

If the rollout plan requires shadow execution or deferred replay:

1. The rollout checklist must explicitly justify why
2. Idempotency handling must be implemented
3. Duplicate execution prevention must be documented
4. Log the exception clearly

---

## API Branching

### Flag-Gated API Endpoints

For new API endpoints, use `[FeatureGate]` to return 404 when the flag is off:

```csharp
[FeatureGate(FeatureFlags.NewApiEndpoint)]
[HttpGet("new-resource")]
public IActionResult GetNewResource() { ... }
```

### Behavioral Changes on Existing Endpoints

For changes to existing endpoint behavior, use `IFeatureManager` to branch:

```csharp
[HttpGet("resource")]
public async Task<IActionResult> GetResource()
{
    if (await _featureManager.IsEnabledAsync(FeatureFlags.EnhancedResource))
    {
        return Ok(await _service.GetEnhancedResourceAsync());
    }

    return Ok(await _service.GetResourceAsync());
}
```

### API Contract Rules

- Never remove existing response fields when the flag is off
- New response fields may be added when the flag is on
- Breaking contract changes require a new API version, not just a flag

---

## Test Seams

Flagged changes must define deterministic flag control for unit and integration tests.

### Unit Tests — Configuration Overrides

For unit tests using `IFeatureManager`, inject a mock:

```csharp
var featureManager = Substitute.For<IFeatureManager>();
featureManager.IsEnabledAsync(FeatureFlags.MyFeature).Returns(true);

var controller = new MyController(featureManager);
var result = await controller.Index();

Assert.IsType<ViewResult>(result);
```

Or use in-memory configuration with the real feature management stack:

```csharp
var config = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["FeatureManagement:MyFeature"] = "true"
    })
    .Build();

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(config);
services.AddFeatureManagement();
var provider = services.BuildServiceProvider();
var featureManager = provider.GetRequiredService<IFeatureManager>();
```

### Integration Tests — Factory Flag Overrides

`Demo1WebApplicationFactory` provides `WithFeatureFlags()` for testing both flag states:

```csharp
// Test with flag on
using var flagOnFactory = _factory.WithFeatureFlags(new Dictionary<string, bool>
{
    [FeatureFlags.MyFeature] = true
});
var client = flagOnFactory.CreateClient();
var response = await client.GetAsync("/my-feature-page");
Assert.Equal(HttpStatusCode.OK, response.StatusCode);

// Test with flag off (default — but explicit for clarity)
using var flagOffFactory = _factory.WithFeatureFlags(new Dictionary<string, bool>
{
    [FeatureFlags.MyFeature] = false
});
var offClient = flagOffFactory.CreateClient();
var offResponse = await offClient.GetAsync("/my-feature-page");
Assert.Equal(HttpStatusCode.NotFound, offResponse.StatusCode);
```

### Required Test Coverage

Every flagged change must include:

| Test Scenario | Description |
|---------------|-------------|
| **Flag-off (default)** | Old behavior works, new behavior is not reachable |
| **Flag-on** | New behavior works as expected |
| **Route visibility** | `[FeatureGate]` endpoints return 404 when off, 200 when on |
| **Side-effect suppression** | Side effects do not fire when the flag is off |
| **API compatibility** | Existing API contracts are preserved when the flag is off |

---

## Migration Conventions

Database work uses **explicit EF Core migrations** with expand/backfill/switch/contract sequencing. The web application is not the primary schema migrator.

### Migration Commands

```bash
# Generate a new migration
dotnet ef migrations add MigrationName --context AchievementDbContext --output-dir Data/Migrations --project Demo1.csproj

# Apply migrations (external step — run before app startup)
dotnet ef database update --context AchievementDbContext --project Demo1.csproj

# Script migrations for CI/CD
dotnet ef migrations script --context AchievementDbContext --project Demo1.csproj --idempotent
```

### Execution Model

| Environment | Migration Runner | Notes |
|-------------|-----------------|-------|
| **Local dev** | `db.Database.Migrate()` at startup (Development/Testing env only) | Convenience — same migration path as external |
| **Integration tests** | `Demo1WebApplicationFactory` applies Migrate() in fixture setup | Uses same migration path |
| **CI/CD** | `dotnet ef database update` or migration bundle before deployment | External step, not in-app |
| **Production** | Human-initiated via Azure deployment pipeline | Never auto-applied at startup |

### Expand/Backfill/Switch/Contract

| Phase | What Happens | Flag State |
|-------|-------------|-----------|
| **Expand** | Add new columns/tables (backward-compatible) | Before activation |
| **Backfill** | Populate new structures from existing data | Before activation |
| **Switch** | Activate code paths using new structures | Flag turned on |
| **Contract** | Remove old columns/tables | Cleanup issue, later |

### Key Rules

- The feature flag gates **code-path adoption**, not schema existence
- Backward-compatible migrations land before flag activation
- Destructive schema cleanup belongs to a later cleanup issue
- `Program.cs` applies migrations in Development/Testing only — production uses external migration

### Existing Database Transition

If you have an existing `achievements.db` created by `EnsureCreated()`, it lacks migration history. To transition:

```bash
# Option 1: Drop and recreate (if data is disposable)
Remove-Item achievements.db
dotnet ef database update --context AchievementDbContext --project Demo1.csproj

# Option 2: Stamp baseline as applied (if data must be preserved)
# Insert the baseline migration ID into __EFMigrationsHistory manually
# after verifying the schema matches the InitialCreate migration
```

---

## Cleanup Mechanics

Temporary rollout flags must be removed after the feature is stable and fully activated.

### Cleanup Checklist

1. Remove the flag constant from `Features/FeatureFlags.cs`
2. Remove the flag entry from `appsettings.json`
3. Remove flag checks from controllers, services, and views — keep only the new behavior
4. Remove dual-path tests — keep only tests for the (now permanent) new behavior
5. Remove the flag from Azure App Configuration
6. Close the cleanup issue

### Cleanup Ownership

| Stage | Responsibility |
|-------|---------------|
| **Plan** | Creates cleanup issue reference in rollout checklist |
| **Review** | Blocks approval if temporary flag has no cleanup reference |
| **Docs** | Records cleanup issue in verification output |
| **Deliver / cloud-finish** | Backstop — creates cleanup issue only if upstream missed it |

### Temporary Flag Metadata

Every temporary flag in `FeatureFlags.cs` should have a comment indicating:
- Owner
- Cleanup milestone
- Cleanup issue reference

```csharp
// --- Temporary rollout flags ---

/// <summary>
/// Achievement leaderboard feature. Temporary rollout flag.
/// Owner: @rbmathis | Cleanup: v2.1 | Issue: #456
/// </summary>
public const string AchievementLeaderboard = "AchievementLeaderboard";
```

---

## Backfill Gate

Expand/backfill/switch changes must define one lightweight completion check before switch/activation.

### Examples

| Gate Type | Example |
|-----------|---------|
| **Row-count parity** | `SELECT COUNT(*) FROM new_table` matches expected source count |
| **Null-free invariant** | `SELECT COUNT(*) FROM new_table WHERE new_column IS NULL` returns 0 |
| **Data-quality check** | Custom query verifying referential integrity |

### Where to Record

- The backfill gate is documented in the plan comment's rollout checklist
- Docs records the gate in the verification output
- The gate must pass before flag activation proceeds
