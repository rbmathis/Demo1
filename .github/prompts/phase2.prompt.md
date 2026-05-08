Implement Phase 2 of flag-plan.md only.

You are working in the Demo1 repository.

Primary objective:
- Wire the rollout contract into local and cloud orchestration.
- Make triage, planning, implementation, review, docs, and delivery stages consistently honor the rollout checklist contract.

Authoritative plan:
- Read flag-plan.md first and follow the Phase 2 intent exactly.
- Do not start Phase 1 design work, Phase 3 runtime support work, or Phase 4 pilot/documentation validation work except where directly required by this phase.

In scope:
- Update the local agent prompts that participate in rollout-aware orchestration.
- Update the cloud workflow prompt files that participate in rollout-aware orchestration.
- Update only the documentation or handoff surfaces needed to explain the Phase 2 orchestration contract.
- Ensure the orchestration contract reflects:
  - triage emits rollout status
  - plan owns rollout-optional yes/no flagging verdicts
  - the checklist artifact is required
  - docs produces the cloud activation packet
  - `cloud-finish.yml` remains backstop-only for cleanup issue creation

Primary local files in scope:
- `.github/agents/autopilot.agent.md`
- `.github/agents/triage.agent.md`
- `.github/agents/plan.agent.md`
- `.github/agents/implement.agent.md`
- `.github/agents/review.agent.md`
- `.github/agents/docs.agent.md`
- `.github/agents/deliver.agent.md`

Primary cloud files in scope:
- `.github/workflows/cloud-triage.md`
- `.github/workflows/cloud-plan.md`
- `.github/workflows/cloud-implement.md`
- `.github/workflows/cloud-review.md`
- `.github/workflows/cloud-docs.md`
- `.github/workflows/cloud-finish.yml` only if a narrowly scoped backstop clarification is required by this phase

Out of scope:
- Do not create runtime helper code.
- Do not change application controllers, services, views, models, middleware, or `Program.cs` behavior.
- Do not implement EF migration code changes.
- Do not run pilot issues.
- Do not update broad documentation sets beyond what is required to explain handoff artifacts.
- Do not begin Phase 3 runtime/test work or Phase 4 pilot validation.

Required outcomes:
- Local and cloud orchestration surfaces consistently require the same rollout checklist artifact.
- Triage records rollout-required / rollout-optional / rollout-exempt and passes that downstream.
- Plan explicitly owns the final flagging verdict for rollout-optional issues.
- Review and docs stages enforce their Phase 2 contract responsibilities.
- Cloud docs emits activation-packet expectations and cloud finish remains backstop-only.
- If workflow `.md` files are changed, corresponding `.lock.yml` files are regenerated with `gh aw compile`.

Constraints:
- Prefer minimal edits.
- Preserve existing style and conventions.
- Use apply_patch for file edits.
- If you touch any cloud workflow `.md` files under `.github/workflows/`, delete the corresponding `.lock.yml` files and run `gh aw compile` in the same phase.
- Do not implement beyond the declared Phase 2 scope even if later work seems obvious.

Stop when:
- Local and cloud orchestration prompts/workflows consistently enforce the rollout checklist and stage responsibilities.
- Any touched workflow lock files have been regenerated successfully.
- No Phase 3+ runtime or pilot work has started.

Before finishing:
- Summarize exactly what changed.
- List anything intentionally left for Phase 3.
- Call out any blockers or ambiguities that still need a human decision.
- Do not continue into the next phase.