# 📌 Issue Backlog (Proposed)

> Use these as ready-made GitHub issues. Copy/paste into GitHub or run `gh issue create` with the provided titles and bodies.

## Table

| ID  | Title                                         | Type          | Priority | Scope         |
| --- | --------------------------------------------- | ------------- | -------- | ------------- |
| 1   | Enforce HTTPS and HSTS                        | Security      | High     | Middleware    |
| 2   | Add global exception handling & logging       | Enhancement   | High     | Middleware    |
| 3   | Implement authentication (Azure AD)           | Feature       | High     | Identity      |
| 4   | Add health check endpoint (`/health`)         | Ops           | Medium   | Observability |
| 5   | Create Demo1.Tests with controller unit tests | Testing       | Medium   | QA            |
| 6   | Integrate Playwright for basic UI smoke       | Testing       | Medium   | QA            |
| 7   | Add Dockerfile and containerized dev workflow | DevOps        | Medium   | Infra         |
| 8   | Add Application Insights telemetry            | Observability | Medium   | Telemetry     |
| 9   | Accessibility pass on views (ARIA/landmarks)  | Accessibility | Medium   | UI            |
| 10  | Add code analyzers and dotnet format baseline | Quality       | Medium   | Tooling       |
| 11  | Optimize CI with restore/build caching        | DevOps        | Low      | CI            |
| 12  | Create custom error pages (404/500)           | UX            | Low      | UI            |

---

## 1. Enforce HTTPS and HSTS

**Type:** Security
**Priority:** High
**Description:** Ensure the app always redirects to HTTPS and configures HSTS in production. Add security headers (CSP, X-Content-Type-Options, X-Frame-Options).

**Acceptance Criteria**

- `UseHttpsRedirection()` enabled and covered by integration tests.
- `UseHsts()` enabled for non-dev.
- Security headers middleware added with configurable CSP.
- Documentation updated in `docs/configuration.md`.

**Labels:** `security`, `enhancement`

---

## 2. Add global exception handling & logging

**Type:** Enhancement
**Priority:** High
**Description:** Add a global exception handler middleware/filter to log exceptions and return friendly error pages/JSON based on request type.

**Acceptance Criteria**

- Middleware or `IExceptionHandlerPathFeature` setup with structured logging.
- Toggle detailed errors by environment.
- Unit tests covering exception-to-response mapping.
- Docs updated in `docs/architecture.md`.

**Labels:** `enhancement`, `observability`

---

## 3. Implement authentication (Azure AD)

**Type:** Feature
**Priority:** High
**Description:** Integrate Microsoft Identity/Azure AD for authentication with cookie-based sessions.

**Acceptance Criteria**

- `AddAuthentication()` configured with Azure AD.
- Protect Privacy page and add a simple profile page.
- Local config via `appsettings.Development.json`; secrets not committed.
- Integration test verifying auth challenge.

**Labels:** `feature`, `security`

---

## 4. Add health check endpoint (`/health`)

**Type:** Ops
**Priority:** Medium
**Description:** Add ASP.NET Core HealthChecks (basic + self) with readiness/liveness endpoints.

**Acceptance Criteria**

- `/health` (liveness) and `/health/ready` endpoints configured.
- Health checks registered for app start and (placeholder) dependencies.
- CI step hitting `/health` in `dotnet.yml`.

**Labels:** `ops`, `observability`

---

## 5. Create Demo1.Tests with controller unit tests

**Type:** Testing
**Priority:** Medium
**Description:** Scaffold `Demo1.Tests` (xUnit), add HomeController tests (Index/Privacy/Error) and validate view model.

**Acceptance Criteria**

- `Demo1.Tests` project referenced in solution.
- Tests assert correct view names/models; error action returns `ErrorViewModel` with request id.
- `dotnet test` passes locally and in CI.

**Labels:** `testing`

---

## 6. Integrate Playwright for basic UI smoke

**Type:** Testing
**Priority:** Medium
**Description:** Add Playwright tests to verify homepage loads, nav links work, and privacy page renders.

**Acceptance Criteria**

- Playwright project added (`playwright-dotnet`).
- CI job runs headless Playwright against `dotnet run` (ephemeral server).
- Artifacts captured on failure (screenshots, traces).

**Labels:** `testing`, `ui`

---

## 7. Add Dockerfile and containerized dev workflow

**Type:** DevOps
**Priority:** Medium
**Description:** Provide Dockerfile for local dev and CI builds; include docker-compose for hot reload (optional).

**Acceptance Criteria**

- Multi-stage Dockerfile (build/publish/runtime) targeting `mcr.microsoft.com/dotnet/aspnet:9.0`.
- `README.md` updated with `docker build`/`docker run` instructions.
- CI job builds image (optional push stage gated by secret).

**Labels:** `devops`, `infrastructure`

---

## 8. Add Application Insights telemetry

**Type:** Observability
**Priority:** Medium
**Description:** Integrate App Insights SDK; capture requests, dependencies, exceptions, traces.

**Acceptance Criteria**

- Connection string from config/secret.
- Telemetry initializers configured; sample rate configurable.
- Docs updated in `docs/configuration.md`.

**Labels:** `observability`

---

## 9. Accessibility pass on views (ARIA/landmarks)

**Type:** Accessibility
**Priority:** Medium
**Description:** Audit Razor views for WCAG 2.1 AA; add ARIA labels, landmarks, and appropriate semantics.

**Acceptance Criteria**

- Accessibility checklist completed.
- Axe/Pa11y scan added to CI (basic checks).
- Document findings and remediation in `docs/conventions.md`.

**Labels:** `accessibility`, `ui`

---

## 10. Add code analyzers and dotnet format baseline

**Type:** Quality
**Priority:** Medium
**Description:** Enable Roslyn analyzers, style rules, and `dotnet format` baseline in CI.

**Acceptance Criteria**

- `.editorconfig` and `AnalysisLevel` set (e.g., `latest`).
- CI step runs `dotnet format --verify-no-changes`.
- Documentation added to `docs/conventions.md`.

**Labels:** `quality`, `tooling`

---

## 11. Optimize CI with restore/build caching

**Type:** DevOps
**Priority:** Low
**Description:** Add `actions/cache` for NuGet packages and `~/.dotnet/tools` to speed builds.

**Acceptance Criteria**

- Cache keys include OS, `global.json` (if any), `*.csproj` hash.
- CI times reduced vs. baseline.

**Labels:** `devops`, `ci`

---

## 12. Create custom error pages (404/500)

**Type:** UX
**Priority:** Low
**Description:** Add custom Razor pages/views for 404 and 500 with branded layout.

**Acceptance Criteria**

- `UseStatusCodePagesWithReExecute` configured.
- Views added under `Views/Shared` (e.g., `Error404.cshtml`, `Error500.cshtml`).
- Manual check documented.

**Labels:** `ux`, `ui`
