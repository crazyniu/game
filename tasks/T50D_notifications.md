# T50D — Notifications, intro and root integration

## Authority and ownership
- Coordinator: current Codex conversation on 251; the former Claude session is retired.
- Branch/workspace: `mac-notifications/t50`, isolated `t50-notifications`, based on `0b44966` (code WIP `9f84a95`). Never modify the original user-test checkout, main, scenes or project settings. No sub-workers or invented Dispatch IDs.
- Owned source: `GameRoot.cs`, [V5Progress.cs](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T50D_notifications.md#L6), [V7PopupFlow.cs](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T50D_notifications.md#L6), [V7LevelEntrance.cs](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T50D_notifications.md#L6), `HudView.cs`; existing Meta/Economy bridge only if required. Existing public signatures remain available. No other module's views are edited.
- Preserve Core, authored levels, economy amounts/eligibility, approved v7 plush forest/purple-green/honey-wood/green-ribbon art, existing LitMotion/V5Motion patterns. No libraries, art generation, features or T51 cleanup.
- Also owns the coordinator-requested update of `docs/team/HANDOFF_2026-11-04_CODEX.md`, preserving the main handoff directives and actual four-workspace/art ownership.
- Coordinator follow-up explicitly authorizes existing fixture/source-compatibility edits in GameplayTests, V5DevOptionsTests and their assembly references without execution. Preserve the string snapshot and seed acknowledged receipts for unrelated legacy cases. After isolated D commits, only exact approved A `a012e70`, C `fb90bb7` and B `4256de63412041e65ddf2554cb14323d79f6d888` may be merged into this branch.

## Requirements
1. Run the skippable approximately 1.6-second intro on every normal level start/restart; restore final poses on skip/cancel and show the tutorial only after intro/notifications.
2. After intro, deliver unmet species in Meta canonical debut order, including Rook's no-home presentation, then existing booster unlock cards. Met/+20 commits only on matching acknowledgement.
3. Daily gift is map-only, using injectable Meta calendar clock; +50 once, no rollback/reload/double-click grants, followed by coins-to-pill presentation.
4. First committed finale win (10/20/30/40) earns pending world-complete; leaving Win via Next/Home shows complete/+200 once, then map gate/unlock for worlds 1–3. Defer Next until complete → unlock → map daily have finished. World 4 returns to map.
5. Never stack popups. Preserve claim receipts, pending finale/unlock and deferred navigation coherently across saves. Suspend level-only requests when map is shown; never replay a stale level popup over the map.
6. Root integrates the full-canvas T50C leaf wipe and T50B popup APIs without editing peer-owned files; merges require separate exact-revision approval. `covered` rebuilds through a non-canceling root method; only external cancellation invalidates the wipe. Always query/pass the node origin (including C's center fallback) before LevelCard. Invoke one HUD win count from the approved Win coin-flight-start callback using captured totals; no grant/save mutation in that callback. Map collection after acknowledgement is the single daily coin spectacle.
7. Every asynchronous root/intro/HUD presentation cancels on teardown/rebuild, rejects obsolete callbacks and settles its pose without duplicate continuation.

## Acceptance / evidence limits
- Static source/control-flow, assembly-reference and public-callsite review only.
- No new test cases are authored or executed. Existing fixture compatibility is corrected by explicit coordinator authorization. Unity, compilation, runtime smoke, captures and all-level playthrough: **NOT_RUN**, explicitly prohibited by the latest user instruction. Historical green results are not this branch's evidence.
- Deliver English report/status, isolated branch commit/push, exact SHA/files/APIs/implemented and missing requirements/untested boundaries. The coordinator integrates and pauses for user testing, then Mac-only development.

## Sources
- [T50 requirements](T50_ui_code.md), [handoff and exact trigger policy](T50_report.md#popup-trigger-policy--exact-lead-decision), [WIP risks](../docs/team/status/mac-ui.md).
- [Team memory](../docs/team/MEMORY.md), [v5 design](../docs/GAME_DESIGN.md), approved `art/approved_ui/ui_*.png` references.
