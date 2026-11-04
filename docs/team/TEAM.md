# Perch Sort team charter

## Current authority

Current coordinator: Codex (`orca_session_id:codex_0551b7e6_4ea7_4c29_936b_a50a25d9c1ec`). Reports, questions and acceptance return there, never retired Claude. Agent entry filenames are compatibility entry points, not separate leads. See [AGENTS](../../AGENTS.md) and [CURRENT_GAME](../CURRENT_GAME.md).

Development is Mac-only, in isolated Orca task workspaces. Do not edit the user's primary checkout from a worker. Linux 251 is no longer the development machine; an existing coordinator message route does not authorize development there.

| Role | Responsibility | Boundary |
| --- | --- | --- |
| Current Codex coordinator | Shared contract, task decomposition, integration/review, required Mac testing and acceptance, user handoff | No invented runtime results; preserve user checkout before updates |
| Code/resource/editor workers | Assigned non-overlapping files; English spec/report; static evidence; commit and push task branch | No peer merges, main push, unauthorized scope, sub-workers or independent Unity |
| Integrated Mac acceptance | Coordinator-serialized compile, EditMode, PlayMode, GUI/timed review | One Unity process; no killing user's Editor or restoring user edits blindly |

OMP preferred for coding, Codex fallback; coordinator chooses model/effort by complexity. Every feature effort is parallelized into independent ownership slices with one contract and coordinator review. Do not start idle or additional workers without assignment.

## Current stage

T52 consolidates the accepted T49/T50 implementation into Game.unity, unversioned current source/resources, one editor/tool entry and the retained current tests. It does not redesign gameplay, add levels, replace originals or reset saves. Historical design filenames/reports are evidence, not multiple runnable versions.

The updated user instruction requires coordinator Mac compilation, EditMode, PlayMode and GUI acceptance after integration. Earlier no-tests language is superseded. Workers report NOT_RUN locally; stage testing is not optional. Automatic CI tests remain enabled on current pure-Core fixtures. Expected retained discovery is 304 EditMode (237 game, 53 Meta, 14 Audio) and 17 PlayMode, no Ignore/Explicit; discrepancies must be investigated, not concealed by lowered guards.

## Working rules

- Chinese with the user; English agent instructions/specs/reports/comments/commits.
- Follow assigned file ownership and one shared contract. Questions/reports go to current Codex through the supplied Orca route or worker terminal if routing fails. Do not invent Task/Dispatch IDs.
- Each task has a spec, report and isolated branch. Commit/push that branch only. Coordinator has user authority to integrate reviewed completed work into main for testing; deployment/release requires separate authority.
- Protect all original `art/` files and historical task/research evidence. Preserve metadata GUIDs, resource/level payloads and storage identifiers. No version-like save key renaming.
- Keep Core/Meta pure and separate from Unity presentation. Inspect exact current behavior before changing it; T52 grants no rule/economy redesign.
- Record pre-run scene/meta/settings/package state. Preserve user changes; never blindly checkout generated files. Never kill another Unity process. `build/` output is not committed.
- No credential disclosure, paid API fallback or unauthorized network service. Current placeholder shop/Aviary scope and release package/account gaps do not authorize extra features.

## Reporting

Deliver `DONE` or `BLOCKED`, task branch and SHA, exact changed paths, static evidence, mapping changes/risks and honest runtime limits. The coordinator reviews integrated sources and records actual Mac execution results. After the bounded task delivery, wait; do not start future work automatically.
