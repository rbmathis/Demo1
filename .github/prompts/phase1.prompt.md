Implement Phase 1 of flag-plan.md only.

You are working in the Demo1 repository.

Primary objective:
- Define the rollout contract for feature-flag-aware delivery.
- Create the new feature-flags specialist agent.
- Lock in the migration and flagging conventions needed before any implementation-phase work begins.

Authoritative plan:
- Read flag-plan.md first and follow the Phase 1 intent exactly.
- Do not start Phase 2, 3, or 4 work.

In scope:
- Create or update planning/design artifacts needed to establish the rollout contract.
- Create `.github/agents/feature-flags.agent.md`.
- Update only the local/cloud planning guidance surfaces that must exist for the rollout contract to be real and unambiguous at design time.
- Document the Phase 1 decisions around:
  - rollout-required / rollout-optional / rollout-exempt
  - checklist-style rollout artifact
  - plan-agent ownership of rollout-optional flagging verdicts
  - temporary flag metadata
  - side-effect default behavior when flag is off
  - migration conventions
  - wrapper-driven external migration model
  - cloud activation ownership and activation packet expectations

Out of scope:
- Do not implement runtime helper code.
- Do not modify controllers, services, views, middleware, or production feature behavior.
- Do not implement EF migration code changes yet.
- Do not run pilot issues.
- Do not execute later-phase workflow wiring beyond what is strictly necessary to establish the Phase 1 contract.
- Do not begin Phase 2 orchestration changes except where a minimal design-time reference is required by the new specialist agent.

Required outcomes:
- A new `feature-flags` specialist agent exists with a clear charter aligned to flag-plan.md.
- The repository contains a clear Phase 1 rollout contract that later phases can consume without inventing policy.
- The migration model is documented as externalized and wrapper-driven for local/test, with a separate cloud migration step.
- The contract clearly states:
  - autopilot ships dark by default
  - cloud activation is human-controlled
  - rollout-optional is not an exemption
  - side effects are suppressed by default when the flag is off
  - temporary flags require lightweight metadata
- Any design-time prompt or planning-surface changes made in this phase must stay narrowly scoped to establishing the contract.

Constraints:
- Prefer minimal edits.
- Preserve existing style and conventions.
- Use apply_patch for file edits.
- If you touch any cloud workflow `.md` files under `.github/workflows/`, delete the corresponding `.lock.yml` files and run `gh aw compile` in the same phase.
- Do not implement beyond the declared Phase 1 scope even if later work seems obvious.

Stop when:
- The specialist agent is created.
- The rollout contract is documented and unambiguous.
- Phase 1 design decisions are reflected in the appropriate planning surfaces.
- No Phase 2+ implementation work has started.

Before finishing:
- Summarize exactly what changed.
- List anything intentionally left for Phase 2.
- Call out any blockers or ambiguities that still need a human decision.
- Do not continue into the next phase.