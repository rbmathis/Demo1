Implement Phase 4 of flag-plan.md only.

You are working in the Demo1 repository.

Primary objective:
- Document, compile, and validate the rollout-aware pipeline.
- Execute the pilot validation path described in flag-plan.md.

Authoritative plan:
- Read flag-plan.md first and follow the Phase 4 intent exactly.
- Do not reopen earlier-phase design questions unless they block validation.

In scope:
- Update repository documentation to reflect the rollout-aware delivery model.
- Update architecture/pipeline docs to reflect the final staged behavior.
- Regenerate workflow lock files if any workflow `.md` files are changed in this phase.
- Execute the selected pilot validation path described in the plan.

Primary files in scope:
- `AI-SDLC-LOCAL.md`
- `AI-SDLC-CLOUD.md`
- `README.md`
- `architecture.md`
- any affected `.github/workflows/cloud-*.md`
- corresponding `.github/workflows/cloud-*.lock.yml`

Pilot scope expectations:
- Verify local/cloud behavior described in flag-plan.md.
- Exercise migration handoff where schema changes are involved.
- Exercise docs output and cleanup backstop behavior.
- Do not broaden beyond the planned pilot validation path.

Out of scope:
- Do not redesign the rollout contract.
- Do not introduce unrelated workflow cleanup.
- Do not perform opportunistic prompt rewrites outside the rollout feature scope.
- Do not implement unrelated application features.

Required outcomes:
- Documentation accurately reflects the rollout-aware pipeline.
- Any touched workflow lock files are regenerated successfully.
- The pilot validation path has been executed and summarized.
- Validation results clearly state what worked, what failed, and any remaining follow-up needed.

Constraints:
- Prefer minimal edits.
- Preserve existing style and conventions.
- Use apply_patch for file edits.
- If you touch any cloud workflow `.md` files under `.github/workflows/`, delete the corresponding `.lock.yml` files and run `gh aw compile` in the same phase.
- Keep validation tightly scoped to the planned pilot behavior.

Stop when:
- Docs are updated.
- Any touched workflow compilation succeeds.
- The pilot has exercised the required rollout-aware behavior described in the plan.
- Results and remaining follow-up are summarized.

Before finishing:
- Summarize exactly what changed.
- Summarize the pilot outcome.
- List any remaining gaps or follow-up work.
- Do not start a new implementation phase.