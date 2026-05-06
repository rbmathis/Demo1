# Top 10 Issues & Improvements

Prioritized list of improvements for this repository, ordered by severity and impact.


---

## 2. Content Security Policy Allows `unsafe-eval`

**Severity:** High
**Area:** `Middleware/SecurityHeadersMiddleware.cs`

The CSP header includes `'unsafe-eval'` in `script-src`, which effectively neuters XSS protection. Any injected script can use `eval()` to execute arbitrary code.

**Fix:**
- Remove `'unsafe-eval'` from `script-src`
- Refactor inline scripts to use event listeners instead of `onclick` handlers
- Add nonce-based CSP for any necessary inline scripts
- Add `Permissions-Policy` header to restrict camera, microphone, geolocation
- Add `Cross-Origin-Opener-Policy` and `Cross-Origin-Embedder-Policy` headers

---

## 3. Authorization Without Authentication

**Severity:** High
**Area:** `Program.cs`

`app.UseAuthorization()` is called but no authentication middleware is registered. Authorization checks are meaningless without a way to identify the user.

**Fix:**
- Either add `app.UseAuthentication()` with a configured auth scheme
- Or remove `UseAuthorization()` if auth isn't needed yet
- Configure at minimum a cookie or JWT authentication scheme for future use

---

## 4. Business Logic in Razor Views

**Severity:** High
**Area:** `Views/Home/ViewLogicCalculator.cshtml`

The view contains 80+ lines of C# business logic including math operations, string parsing, JSON/XML/CSV deserialization. This violates MVC separation of concerns and is untestable.

**Fix:**
- Extract all logic into a service (`ICalculatorService`)
- Controller calls the service and passes results via a strongly-typed view model
- View only renders pre-computed data

---

## 5. No Rate Limiting

**Severity:** High
**Area:** `Program.cs`

All endpoints are open to abuse with no request throttling. An attacker could flood the search endpoint, weather endpoint, or profile mutations.

**Fix:**
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        httpContext => RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});
// ...
app.UseRateLimiting();
```

---

## 6. Test Coverage Gaps

**Severity:** Medium
**Area:** `tests/`

Critical gaps:
- No tests for `RawSqlSearch`, `CallbackHellWeather`, `InlineCssHell`, `ViewLogicCalculator`, or `GodObjectProfile` actions (the most complex actions)
- No service-level unit tests (`InMemorySearchService`, `MockWeatherService`, `InMemoryUserProfileService`, `StyleGeneratorService`)
- Integration tests cover only 2 of 10+ routes
- No model validation tests
- No negative/edge-case tests

**Fix:**
- Add unit tests for all service implementations
- Add controller tests for every action, including invalid input scenarios
- Add integration tests for critical paths (search, profile update)
- Add model validation tests for all view models
- Target ≥80% code coverage with enforcement in CI

---

## 7. Overloaded HomeController

**Severity:** Medium
**Area:** `Controllers/HomeController.cs`

Single controller handles 10+ unrelated actions: profiles, search, weather, CSS demos, calculator, about, contact, privacy. This violates Single Responsibility Principle and makes the file difficult to maintain.

**Fix:**
- `DemoController` — ViewLogicCalculator, InlineCssHell, CallbackHellWeather, RawSqlSearch
- `ProfileController` — GodObjectProfile
- `WeatherController` — Weather-related actions
- `HomeController` — Index, Privacy, About, Contact, Error

---

## 8. Security Scan Failures Suppressed in CI

**Severity:** Medium
**Area:** `.github/workflows/` (CI pipeline)

The security vulnerability scan uses `|| true` to swallow failures, meaning known vulnerabilities in dependencies will never break the build.

**Fix:**
- Remove `|| true` from vulnerability scan steps
- Add Roslyn security analyzers (`Microsoft.CodeAnalysis.NetAnalyzers`)
- Add `dotnet format --verify-no-changes` for style enforcement
- Consider adding SonarQube or CodeQL for SAST
- Add container image scanning (Trivy, Grype)

---

## 9. Missing Static Assets & Optimization

**Severity:** Medium
**Area:** `wwwroot/`

Issues:
- No `favicon.ico` — browsers return 404 on every page load
- No minification/bundling pipeline — multiple uncompressed files served
- Bootstrap JS may be incomplete — layout uses collapse/dropdown features
- jQuery loaded but Bootstrap 5 doesn't require it
- No `robots.txt` or `sitemap.xml`
- No cache-busting strategy beyond `asp-append-version`

**Fix:**
- Add favicon (even a simple one)
- Configure `WebOptimizer` or `BundlerMinifier` for production builds
- Verify `bootstrap.bundle.min.js` is present and loaded
- Evaluate removing jQuery if only Bootstrap needs it
- Add `robots.txt` and `sitemap.xml`

---

## 10. Model Quality Issues

**Severity:** Medium
**Area:** `Models/`

Problems:
- `GodObjectProfile.cs` — God object with duplicate properties, inconsistent casing, sensitive fields (password, SSN, creditCard)
- `WeatherData.cs` — Redundant properties (`city`/`CITY`), stores computed values
- `ViewLogicData.cs` — Uses `List<object>` with no type safety
- No models use data annotations (`[Required]`, `[StringLength]`, `[Range]`)
- Nullable reference type warnings throughout

**Fix:**
- Refactor `GodObjectProfile` into proper DTOs with validation
- Add `[Required]`, `[StringLength]`, `[EmailAddress]` etc. to all input models
- Enable `<Nullable>enable</Nullable>` and fix all warnings
- Remove sensitive field storage from models entirely
- Replace `List<object>` with strongly-typed collections

---

## Summary Table

| # | Issue | Severity | Effort |
|---|-------|----------|--------|
| 1 | CSRF via GET mutations | Critical | Low |
| 2 | CSP allows `unsafe-eval` | High | Low |
| 3 | Auth without Authentication | High | Medium |
| 4 | Business logic in views | High | Medium |
| 5 | No rate limiting | High | Low |
| 6 | Test coverage gaps | Medium | High |
| 7 | Overloaded HomeController | Medium | Medium |
| 8 | Security scan suppressed | Medium | Low |
| 9 | Missing static assets | Medium | Low |
| 10 | Model quality issues | Medium | Medium |

---

## How to Use This List

Each issue above can be filed as a GitHub Issue to trigger the AI-SDLC pipeline. The pipeline will automatically triage, plan, implement, test, review, and deploy the fix.

Example issue title: `Fix CSRF vulnerability in GodObjectProfile — convert GET mutations to POST with anti-forgery tokens`
