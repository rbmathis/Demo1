Implement Phase 3 of flag-plan.md only.

You are working in the Demo1 repository.

Primary objective:
- Add the minimum runtime conventions, test expectations, migration-step guidance, and cleanup rules needed to implement flagged work safely.

Authoritative plan:
- Read flag-plan.md first and follow the Phase 3 intent exactly.
- Do not start earlier design/orchestration work unless required to keep Phase 3 internally consistent.
- Do not start Phase 4 pilot validation or broad documentation polish.

In scope:
- Minimal runtime support needed for default-off branching and rollout-aware implementation guidance.
- Minimal documentation or code abstractions needed to support:
  - default-off behavior branching
  - side-effect suppression by default
  - deterministic test seams for flags
  - migration-step expectations and failure behavior
  - cleanup issue mechanics
  - backfill completion gate expectations
- Focused test or test-guidance changes required by the Phase 3 contract.

Potential files in scope:
- `Program.cs`
- `Features/FeatureFlags.cs`
- `tests/Demo1.IntegrationTests/Fixtures/Demo1WebApplicationFactory.cs`
- any minimal runtime/documentation surfaces directly required to establish rollout implementation conventions

Out of scope:
- Do not implement a real product feature behind a new flag unless absolutely necessary to establish the convention.
- Do not perform broad application refactors.
- Do not run the end-to-end pilot.
- Do not broaden into Phase 4 docs/validation work except for narrowly necessary guidance updates.
- Do not opportunistically change unrelated runtime behavior.

Required outcomes:
- The repository has enough concrete runtime/test guidance or helper support for future flagged implementations.
- Side effects are clearly modeled as suppressed by default when flags are off unless explicitly overridden.
- Migration-step expectations are concrete enough that future work will not rely on in-app schema creation.
- Test seams are defined clearly enough for unit and integration coverage of off/on paths.
- Cleanup mechanics and backfill-gate expectations are reflected in the relevant runtime/testing guidance.

Constraints:
- Prefer minimal edits.
- Preserve existing style and conventions.
- Use apply_patch for file edits.
- Favor convention-focused changes unless a helper abstraction is truly necessary.
- Do not implement beyond the declared Phase 3 scope even if later work seems obvious.

Stop when:
- Downstream contributors can implement flagged work without inventing new runtime/test conventions.
- The Phase 3 contract is materially supported by the repository.
- No Phase 4 pilot execution or broad documentation sweep has started.

Before finishing:
- Summarize exactly what changed.
- List anything intentionally left for Phase 4.
- Call out any blockers or ambiguities that still need a human decision.
- Do not continue into the next phase.