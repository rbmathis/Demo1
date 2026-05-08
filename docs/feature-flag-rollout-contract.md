# Feature-Flag Rollout Contract

> **Status:** Phase 1 — policy defined. Pipeline enforcement begins in Phase 2.

This document is the canonical reference for the Demo1 feature-flag delivery model. All pipeline agents — local and cloud — validate rollout artifacts against this contract.

## Table of Contents

- [Rollout Classes](#rollout-classes)
- [Canonical Rollout Checklist](#canonical-rollout-checklist)
- [Plan-Agent Ownership](#plan-agent-ownership)
- [Temporary Flag Metadata](#temporary-flag-metadata)
- [Side-Effect Defaults](#side-effect-defaults)
- [Migration Conventions](#migration-conventions)
- [Cloud Activation Ownership](#cloud-activation-ownership)
- [Activation Packet](#activation-packet)
- [Delivery Behavior](#delivery-behavior)
- [Flag Naming and Storage](#flag-naming-and-storage)
- [Testing Seams](#testing-seams)
- [Backfill Gate](#backfill-gate)
- [Cleanup Mechanics](#cleanup-mechanics)
- [Observability](#observability)
- [Phase Boundary](#phase-boundary)

---

## Rollout Classes

Triage classifies every issue into one of three rollout statuses:

### rollout-required

Applies to:
- User-visible UI or page behavior changes
- API contract changes (new endpoints, changed responses, removed fields)
- Side-effecting work (background jobs, email, external APIs, webhooks)
- Risky server logic (auth flows, payment logic, data processing)
- Database-affecting changes (new tables, column changes, indexes, data migrations)

The change **must** ship behind a feature flag with a complete rollout checklist.

### rollout-optional

Applies to:
- Low-risk, user-invisible changes that may benefit from a dark launch
- Internal service refactors with observable behavior preserved but regression risk
- Configuration changes that affect runtime behavior indirectly

**`rollout-optional` is not an exemption.** The plan agent must still invoke the `feature-flags` specialist for an explicit flagging verdict. If the specialist decides no flag is needed, the verdict and justification are recorded in the plan comment.

### rollout-exempt

Applies to:
- Documentation-only changes
- Test-only changes
- Internal refactors with no observable behavior change
- Build/CI/config cleanup with no runtime effect
- Emergency security fixes where delayed activation would be inappropriate

Exempt issues skip rollout analysis entirely.

---

## Canonical Rollout Checklist

The plan comment is the source of truth. For every `rollout-required` or flagged `rollout-optional` issue, the plan must include a checklist-style rollout section containing:

| Field | Description |
|-------|-------------|
| **Rollout status** | `rollout-required` or `rollout-optional` |
| **Flagging verdict** | Ship behind temporary flag / permanent flag / ungated (with justification) |
| **Flag name** | `FeatureFlags.{PascalCaseName}` or N/A |
| **Default state** | Always `Off` |
| **Gating mechanism** | `[FeatureGate]`, `IFeatureManager`, Razor `<feature>`, API branching, side-effect guard |
| **Old-path behavior (flag off)** | What the system does with the flag off |
| **New-path behavior (flag on)** | What the system does with the flag on |
| **Impacted surfaces** | UI, Controller, API, Background job, Schema |
| **Side-effect behavior (flag off)** | Suppressed (default) / Shadow mode (justify) / Deferred replay (justify) |
| **Migration notes** | Expand/backfill/switch/contract details or N/A |
| **Migration step** | External wrapper / N/A |
| **Activation prerequisites** | What must be true before enabling |
| **Activation steps** | Azure App Configuration key, label, value |
| **Rollback steps** | How to revert |
| **Observability** | Logging/telemetry requirements |
| **Dual-path tests** | Flag-off test scenarios, flag-on test scenarios |
| **Flag lifecycle** | Temporary or Permanent |
| **Flag owner** | Person or team responsible |
| **Cleanup milestone** | Target milestone or N/A |
| **Cleanup issue** | Reference or "to be created" |

---

## Plan-Agent Ownership

The **plan agent** (local `plan` and cloud `cloud-plan`) owns the flagging verdict for `rollout-optional` issues:

1. Plan invokes the `feature-flags` specialist with issue context and triage classification
2. The specialist emits a rollout verdict and checklist
3. Plan embeds the checklist in the plan comment
4. If the specialist says "ungated is acceptable," the plan comment records that verdict and the justification
5. Review validates the checklist was completed — it does not re-decide the flagging verdict

---

## Temporary Flag Metadata

Every temporary rollout flag must declare:

| Field | Required |
|-------|----------|
| **Owner** | Yes — person responsible for cleanup |
| **Cleanup milestone** | Yes — intended removal target |
| **Cleanup issue reference** | Yes — created during planning or docs, backstopped by deliver/finish |

Keep metadata lightweight. No additional tracking is required beyond these three fields.

---

## Side-Effect Defaults

When a flag is off, the default expectation is **suppression** — no side effects leave the system:

- No emails sent
- No external API calls made
- No background jobs enqueued
- No webhooks dispatched
- No database writes for the new feature path

### Exceptions

If a plan requires shadow execution, deferred replay, or another exception to suppression:

1. The rollout checklist must explicitly justify why suppression is inappropriate
2. Idempotency handling must be documented
3. Duplicate execution prevention must be specified
4. Review explicitly validates the justification

---

## Migration Conventions

### Sequencing

Database work follows **expand/backfill/switch/contract** sequencing:

| Phase | Description | Flag State |
|-------|-------------|-----------|
| **Expand** | Add new columns/tables — backward-compatible, no data loss | Before flag activation |
| **Backfill** | Populate new structures from existing data | Before flag activation |
| **Switch** | Activate code paths that use new structures | Flag activation |
| **Contract** | Remove old columns/tables | Cleanup issue, later |

### Execution Model

Migration execution is **externalized and wrapper-driven**:

- The web application is **not** the primary schema migrator
- Local development uses a wrapper script or task before app startup
- Integration tests invoke the migration path from fixture/setup code
- Cloud delivery uses a separately invoked migration step before activation

### Key Rules

- The feature flag gates **code-path adoption**, not the existence of a backward-compatible schema change
- Backward-compatible migrations land before flag activation
- Destructive schema cleanup belongs to a later cleanup issue
- The current `Program.cs` `EnsureCreated()` path is replaced with explicit EF Core migrations (Phase 3 implementation)

---

## Cloud Activation Ownership

- Cloud flag activation is owned by **`rbmathis`** as the human release operator
- Activation uses **Azure App Configuration**
- **Neither `cloud-docs` nor `cloud-finish.yml` enables flags automatically**
- `cloud-finish.yml` is not an activation step
- Migration execution is a separate prerequisite when schema changes exist

---

## Activation Packet

The docs stage publishes an activation packet for the human operator. The packet must include:

| Field | Description |
|-------|-------------|
| **App Configuration key** | The feature flag key in Azure App Configuration |
| **Label / environment** | Target label or environment scope |
| **Intended value** | The value to set (typically `true`) |
| **Prerequisites** | Required migrations, deployments, or validations before enabling |
| **Validation steps** | How to verify the flag is working after activation |
| **Rollback steps** | How to disable the flag and what to verify after rollback |

---

## Delivery Behavior

- Autopilot ships new work **dark by default**
- Enabling a flag is a **deliberate, human-controlled action** after validation
- Local activation uses `appsettings.json` or user secrets
- Cloud activation uses Azure App Configuration
- No pipeline stage auto-enables flags

---

## Flag Naming and Storage

### Naming

- All flags are constants in `Features/FeatureFlags.cs` using `PascalCase`
- Names must be descriptive: `ContactForm`, not `FF42`
- Temporary rollout flags indicate their purpose: `AchievementLeaderboard`, `DarkMode`
- Permanent product flags are documented as permanent in the rollout checklist

### Storage

- Local: `appsettings.json` under the `FeatureManagement` section, default `false`
- Cloud: Azure App Configuration with environment-scoped labels
- Both storage surfaces must be representable for every flag

---

## Testing Seams

Flagged changes must define deterministic flag control for unit and integration tests:

- **Unit tests**: configuration overrides or injected `IFeatureManager` seams
- **Integration tests**: test host configuration overrides (e.g., in `Demo1WebApplicationFactory`)
- **Required coverage**: default-off path, flag-on path, route visibility, API compatibility, side-effect suppression when off

Cloud/runtime Azure App Configuration fallback expectations are documented separately from test setup.

---

## Backfill Gate

Expand/backfill/switch changes must define one lightweight completion check before switch or activation:

- Row-count parity check
- Null-free invariant
- Other explicit data-quality gate

The gate is recorded in the docs verification output and referenced in the plan artifact.

---

## Cleanup Mechanics

| Stage | Responsibility |
|-------|---------------|
| **Plan** | Creates the cleanup issue reference in the rollout checklist |
| **Review** | Blocks approval if a temporary flag has no cleanup reference |
| **Docs** | Records the cleanup issue in the verification output |
| **Deliver / cloud-finish** | Backstop only — creates the cleanup issue at merge time if upstream stages failed to establish it |

---

## Observability

Rollout-sensitive implementations must emit enough logging and/or telemetry to identify:

- Which execution path ran (flag-on vs flag-off)
- Whether the flag was evaluated and what state it was in
- Any side-effect suppression decisions

Review checks for this explicitly.

---

## Phase Boundary

| Phase | Scope |
|-------|-------|
| **Phase 1** (current) | Contract definition, specialist agent creation, design-time references |
| **Phase 2** | Pipeline orchestration — triage emits rollout status, plan/implement/review/docs enforce the contract |
| **Phase 3** | Runtime conventions — helper code, testing patterns, migration implementation |
| **Phase 4** | Documentation updates, workflow compilation, pilot validation |

This document defines **policy**. Pipeline enforcement of this policy begins in Phase 2.
