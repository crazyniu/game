# Current T52 entry and required Mac acceptance

The current game is **`PerchSort/Assets/Scenes/Game.unity`**, retaining the accepted T49/T50 implementation. Start with [CURRENT_GAME](../CURRENT_GAME.md), [CODE_MAP](../spec/CODE_MAP.md), [AGENTS](../../AGENTS.md) and the [T52C report](../../tasks/T52C_report.md). Current runtime code/resources are unversioned; retired code/scenes/tools/tests are archived outside Unity. Historical design/art/task filenames and compatibility save keys are not runnable forks.

**Latest user update:** current Codex coordinator must perform acceptance **and testing on the Mac after integrating all T52 deliveries**: compilation, EditMode, PlayMode and real GUI/timed acceptance. This supersedes the earlier no-tests continuation instruction. Workers do not independently launch Unity. Source-derived discovery expectations are 304 EditMode (237 current game, 53 Meta, 14 shared Audio) and 17 PlayMode, no Ignore/Explicit; LevelsData 81 includes 40 optimal solver/replays, Layout 41 includes 24 combinatorial. Actual discovery/results must be investigated, not assumed or weakened.

T52C has not opened Unity, compiled, tested, captured, merged peer branches or touched the primary checkout. Scene/root GUID/font preservation is static evidence only. The coordinator reported primary-checkout-generated bird/overlay metas and scene InputSystem ID changes; preserve those before updating the user checkout, reconcile resource-owner metadata, inspect the actual imported scene and verify startup. The unified importer explicitly preserves all seven authored egg/family anchor/mesh/texture settings and keeps six compressed fallback imports separate.

Development is Mac-only in isolated workspaces. Reports go to current Codex `orca_session_id:codex_0551b7e6_4ea7_4c29_936b_a50a25d9c1ec`, never retired Claude. Workers commit/push only their own task branches; coordinator reviews/integrates and owns the serialized Mac verification. Do not begin unrelated future features. Automatic PR CI remains enabled on current fixtures; no archive is tested.

The following T49/T50 handoff is a **historical checkpoint record**. Its old scene paths, numbered type names, host/ownership instructions, pause/no-tests language and old primary-checkout state apply only to that recorded stage; they are not today's entry or execution authority. Its source SHAs, findings and NOT_RUN evidence are preserved below without manufacturing new acceptance.

---

# T49/T50 final source integration and Mac-only handoff

## Published stage — coordinator closeout

The complete reviewed integration `bf31ba2f3b9701446be8782bd2d7f9138af701fa` was squash-merged through [PR36](https://github.com/crazyniu/game/pull/36) into main as **`2a5bc259187cc924d1d5a9c666a8385b95217327`**. All A/B/C/D/E code and final bird/egg/overlay assets are included. The sections below retain the worker's pre-merge evidence; their statements that main publication is pending are historical, superseded by this record.

All repository branch work was reconciled in [the branch coverage record](T50_BRANCH_COVERAGE.md). Existing merged branches need no duplicate PR; cancelled Godot/v6/concept branches remain preserved. The missing T44 popup-trigger policy is restored in the owning T50 task without changing runtime behavior.

Observed checkout synchronization on 2026-11-04: `/Users/apple/Documents/projects/game` was fast-forwarded on `main` to the PR36 code checkpoint above, matched `origin/main`, and had an empty working-tree status. Linux `/home/developer/Projects/game` also reached that checkpoint while preserving the two pre-existing untracked review PNGs. This closeout changes documentation only; use `git status` and `git log -1` for the final documentation tip. The Mac project to open is `/Users/apple/Documents/projects/game/PerchSort`, scene `Assets/Scenes/V5.unity`.

PR36 CI passed configuration/script checks only: `Core tests: False; workflows: False`. Unity/import/compilation, game execution, test suites and timed visual acceptance remain **NOT_RUN**. This stage is complete at source level and **PAUSED for user testing**. Do not start T51 or other development automatically. Further development belongs on the Mac with parallel, non-overlapping tasks, OMP preferred and Codex fallback.

## Authority and stopping point

- Current coordinator is the native Codex conversation `orca_session_id:codex_0551b7e6_4ea7_4c29_936b_a50a25d9c1ec`, not retired Claude. No old Claude recipient/session is resumed.
- The coordinator explicitly accepted A/B/C/D/E and final bird/egg/overlay sources for integration. **Source acceptance is not runtime, compilation or visual acceptance.** No code-feature TODO remains in the bounded stage.
- This worker integrates only on `mac-notifications/t50` at `/Users/apple/orca/workspaces/game/t50-notifications`. It does not merge into main, deploy/release or mutate `/Users/apple/Documents/projects/game`.
- The coordinator will review the final resolved tip and perform the final PR/main merge under existing user authorization, then separately update the user-test checkout. Do not report either action as already done.
- No code tests, Unity/import/compile, runtime or all-level playthrough. The user tests later. Historical results do not validate this continuation.
- After publication: **HOLD**. Next development is directly on the Mac; 251 is not the ongoing development host. Every future feature is decomposed into parallel tasks with non-overlapping ownership and coordinator integration. OMP preferred, Codex fallback; coordinator chooses gpt-6.1-terra/gpt-6.1-sol/gpt-6-astra and effort by complexity.
- Preserve main's current AGENTS/CLAUDE/TEAM execution directives. They override historical Claude-led prose and old tests-before-push instructions. No new features, artwork generation or T51 cleanup.

## Protected main / user-test state

- Fetched incoming main is **`3e82c64734f3e7291a332d1c0e90263954ce40cd`**, PR35's completed v7 UI checkpoint.
- Coordinator previously reported `/Users/apple/Documents/projects/game` clean on that main for user testing. This worker did not reconfigure, edit, switch or update it. The original mac-ui worker remains HOLD.
- PR35 was a squash: merge-base is `48d4256`, while its snapshot contains checkpoint `1abcee5`. Expected ancestry conflicts are resolved without restoring older runtime or owner reports.
- Merge direction here is **origin/main → task branch**, never task branch → main. No main branch ref or original checkout mutation is part of this delivery.

## Exact accepted source and asset boundaries

| Module / delivery | Exact accepted revision | Task-branch integration boundary |
| --- | --- | --- |
| A — bird/board/home motion | `a012e70aeaa58280da920ac18c53a205fbfa4c9d` | `d8d00f73c98de48088238fa9f1c76406ccad1af6` |
| B — win/popups/settings/gates | `4256de63412041e65ddf2554cb14323d79f6d888` | `3a057636325fa017aecbbcb074766eeb79e01b46` |
| C — map/ambient/leaf transition | `fb90bb77d4262456c8375a6f0b5fad713a1c819b` | `8dd09021b9524cd08af97bea7bd034adda2a18ef` |
| D — final committed-transaction fix | `bf643f4b513e60e7a24bc241e5907562a7087ecd` | Separate committed/pushed correction, coordinator static re-review passed |
| E — authored family overlay integration | `2b2592560e8a7128e35e874e046bfa7dfd97f00f` | `83a4d93db4b811598a923f7926fb73389ec81a4b` |
| Ten-species final birds | `fd1446948f58d5d8c4fe9f5b201b75eb15d5edb9` | `5c22b064b7cab8d6c702d4a6b9321963cd474b2d` |
| Five egg frames / two overlays | `f79dc3875ca7b6138e9c304059c2d3432a5c2775` | `b9d019c5aa6d9fe204fa8d6dd0d7d807a5013db1` |

D history remains isolated: source `52cad38e2742263b4469ead8ef5a90d80f564b73`, fixture `e39e0ddc5f330d35ff416a609f0eefbb20e7fcad`, review boundary `08b32a733ab952f79ca66f3424696f498a2f6720`, Win-count handoff `f24c11f`. Egg asset commit is `34eddebeca3b75fa5e39019090c8b486ba99911f`. Earlier UI `825ca1d` and world/booster `6a29b50` remain retained.

Pre-main accepted runtime/art snapshot is **`83a4d93db4b811598a923f7926fb73389ec81a4b`**. The exact resolved-main merge and final published tip are reported with the final submission; pin that final tip for PR review rather than a mutable peer branch.

## Four actual code workspaces

| Owner | Branch | Workspace | Final owned scope |
| --- | --- | --- | --- |
| A / E | `mac-motion/t50` | `/Users/apple/orca/workspaces/game/t50-bird-motion` | BirdView, BoardView, BranchView, BirdhouseView; E's internal V7BirdOverlay. No root/asset ownership. |
| B | `mac-popups/t50` | `/Users/apple/orca/workspaces/game/t50-popup-motion` | WinPanel, V5Panels, SettingsPanel, V7GateView and popup helper. |
| C | `mac-map-motion/t50` | `/Users/apple/orca/workspaces/game/t50-map-motion` | V6MapView, V5SceneView, map/ambient/leaf helpers. |
| D / integration | `mac-notifications/t50` | `/Users/apple/orca/workspaces/game/t50-notifications` | Root/notifications/profile/intro/HUD, authorized fixture compatibility and reviewed integration/final documentation. |

Bird art is from `mac-birds/t49`; seven egg/overlay assets from `mac-art/world`. Peer workspaces/branches were not edited by the integrator. No sub-workers or invented Dispatch IDs were started. Cross-server automatic adoption remains unavailable; current terminal/status channels are the coordination record.

## Completed shared contracts

- Existing public signatures remain. Root calls Map.TryGetLevelCardOrigin(int,out Vector2), always passes its out point (including fallback) to V5Panels.SetLevelCardOrigin before LevelCard.
- Full-Canvas V7LeafTransition.Configure(V5Art)/Play(Action covered,Action completed)/Cancel owns its lifecycle. Covered BuildLevel/map setup does not cancel the same wipe or invalidate its completion. External cancellation discards stale continuations.
- WinPanel.Show(int,int,int,Action coinFlightStarted) synchronizes exactly one root HUD count of captured already-committed totals. Views grant/save nothing. Daily collection is root acknowledgement/save → Map.CollectCoins only; B's pre-ack flight is removed.
- Notification receipts/order and deferred Next are complete, including map-only clock daily, first-finales/+200 once, Rook and level-40 map return. World/friend/daily grants occur only on matching acknowledgement.
- D's transactionGeneration is independent of InputTap/hint revision. Ignored/queued busy taps cannot discard the committed Board completion; real cancellation/session/lifecycle/one-shot guards and one queued landing/effect tap remain.
- A's Board internally owns flock doorway/masked flight/close/check. E is integrated: Sunny/Skye family markers use normalized source-frame anchors/scale and synchronous pose routing, including cancel/rest. No E code or other feature is still pending.
- Existing fixture string preservation and deliberate Meta receipts are corrected without execution. Core rules, authored levels, economy formulas and original assets remain unchanged. Existing Shop/Aviary placeholders are the previously authorized scope, not new feature work.

## Evidence and runtime-only risks

**NOT_RUN:** Unity/import/C# compilation, EditMode/PlayMode/all test suites, runtime smoke, gameplay simulation/all-level playthrough, screenshots, timed sequences, profiling, device/safe-area checks.

Offline approved assets: 170 distinct 320×320 RGBA bird frames plus ten portraits; five egg frames and two RGBA overlays with seven metadata files, stable 300 baseline/support band, contracts and sheets. This is not in-context animation proof. Historical checkpoint `1abcee5` results and `build/screens/v7/` artifacts apply only to that checkpoint; no new passing counts or captures are claimed. PR35 CI covered configuration/scripts, not Core tests.

Known runtime-only calibration risks: actual Unity import/meta behavior; Sunny/Skye head/chest marker roll/facing/chick/density placement across poses and mixed fallbacks; house/popup doorway/silhouette clipping and gate hinges; intro/Win/coin timing; cancellation/rebuild/input/hint interactions; saved receipts on interruption and midnight/date rollback; fullscreen leaf safe areas; map origin/inertia; ambient/idle visual density/performance and muted sound. Static values are reviewable candidates, not measured runtime acceptance. Original/fallback assets remain; T51 is not part of the integration.

## User entry / Mac continuation

Open the `PerchSort` project with Unity **6000.6.7f1** and existing scene **`PerchSort/Assets/Scenes/V5.unity`** after the coordinator's final integration/user-test checkout update. The worker has not opened Unity or rewritten scenes/settings. Do not use historical `Game.unity` as the v7 entry.

A Mac continuation starts by reading this handoff, [final T50 report](../../tasks/T50_report.md) and git status; preserve user changes and never resume retired Claude. Coordinator performs the final PR/main merge and records its actual SHA, remote publication and test-checkout state afterward. Until then: isolated integration branch only, **HOLD**. Future features remain parallelized on the Mac with OMP preferred and Codex fallback; no automatic T51/new-feature work or test execution.

## Observed final reconciliation checks — source/tree only

- Staged runtime/art/fixture tree under PerchSort and art/v7 is byte-for-byte identical to accepted pre-main `83a4d93db4b811598a923f7926fb73389ec81a4b`. Latest A/B/C/E/art/UI owner records and T49/A/B/C/E reports are preserved exactly.
- Staged diff against incoming main `3e82c64734f3e7291a332d1c0e90263954ce40cd` is empty for Core, authored level JSON/Levels resources/tools, every Unity scene and ProjectSettings.
- AGENTS.md, CLAUDE.md and TEAM.md exactly match incoming main, including current Codex/parallel/OMP/Mac-only directives.
- No unmerged index entries remain. Staged diff whitespace/conflict-marker checks passed. All final report/owner-report links and the V5.unity entry file were found.
- Automatic merge residue (old booster queue code, missing house imports and incidental spacing) was removed to recover the exact reviewed source rather than introducing new code. No compiler, Unity or test was run for these checks.

## Pinned resolved-main integration

Resolved main→task merge is **`571d4f39349fdd759bc73a88a72a54167f73ed81`**, with accepted E/runtime/art parent `83a4d93db4b811598a923f7926fb73389ec81a4b` and incoming main parent `3e82c64734f3e7291a332d1c0e90263954ce40cd`. The following final-record commit changes documentation only; its exact pushed tip and clean status are reported in the terminal submission. Neither commit is a task→main merge or user-test checkout update. HOLD for the coordinator's final tree/PR review.
