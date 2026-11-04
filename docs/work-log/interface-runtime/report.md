# T50 — final accepted-source integration report

## Coordinator publication

The reviewed integration `bf31ba2` was merged into main through [PR36](https://github.com/crazyniu/game/pull/36) as `2a5bc259187cc924d1d5a9c666a8385b95217327`. This supersedes the historical worker statements below that publication is pending. All bounded source work is complete; the stage is paused for user testing. See the [current handoff](../docs/team/HANDOFF_2026-11-04_CODEX.md#published-stage--coordinator-closeout) and [branch reconciliation](../docs/team/HANDOFF_2026-11-04_BRANCH_COVERAGE.md). Unity and code tests remain NOT_RUN.

## Delivery state

**All bounded T49/T50 A/B/C/D/E code and final bird/egg/overlay sources are accepted by the current Codex coordinator for integration. This is not compilation, visual or runtime acceptance.** There is no remaining code-feature TODO in this stage. No new feature or T51 cleanup was added during finalization.

- Integration branch/workspace: `mac-notifications/t50`, `/Users/apple/orca/workspaces/game/t50-notifications`.
- Preserved WIP: source `9f84a95`, handoff `0b44966`.
- Isolated D review boundary: `08b32a733ab952f79ca66f3424696f498a2f6720`; fixture correction `e39e0ddc5f330d35ff416a609f0eefbb20e7fcad`; Win-count handoff correction `f24c11f`.
- Approved code-only A/B/C integration: `3a057636325fa017aecbbcb074766eeb79e01b46`.
- Approved asset integration: `b9d019c5aa6d9fe204fa8d6dd0d7d807a5013db1`; published records `15516a7a08461bdb37f86119a76d0e5b6ef167fa`.
- E integrated before main reconciliation at **`83a4d93db4b811598a923f7926fb73389ec81a4b`**. This is the accepted runtime/art snapshot preserved during the main merge.
- Fetched main input: **`3e82c64734f3e7291a332d1c0e90263954ce40cd`** (PR35). This worker merges that main snapshot **into the task branch only**. It does not merge the task into main or update `/Users/apple/Documents/projects/game`.
- Exact resolved-main merge and final pushed submission tip are recorded in the final integration handoff/terminal. The coordinator reviews that tip, performs the final PR/main merge using the existing user authorization, and updates the user-test checkout separately.

## Accepted source boundaries

| Delivery | Exact accepted revision | Scope / report |
| --- | --- | --- |
| A — bird/board/home motion | `a012e70aeaa58280da920ac18c53a205fbfa4c9d` | Bird/branch mechanics, random idle, flock doorway mask/open/close/check. [A report](T50A_report.md). |
| B — popup/win/settings/gate | `4256de63412041e65ddf2554cb14323d79f6d888` | Reviewed daily-flight, side-bird layering and NEXT-wrapper cancellation corrections included. [B report](T50B_report.md). |
| C — map/ambient/leaf wipe | `fb90bb77d4262456c8375a6f0b5fad713a1c819b` | Includes delivered W1 leaf correction, displayed-node origin and cancel-safe map/transition APIs. [C report](T50C_report.md). |
| D — notifications/intro/root | `bf643f4b513e60e7a24bc241e5907562a7087ecd` | Final transaction-generation fix passed coordinator static re-review. [D report](T50D_report.md). |
| E — authored family markers | `2b2592560e8a7128e35e874e046bfa7dfd97f00f` | Primary and independent static review passed; BirdView plus internal V7BirdOverlay, Sunny/Skye pose anchors and normalized scaling. [E report](T50E_report.md). |
| T49 — ten bird species | `fd1446948f58d5d8c4fe9f5b201b75eb15d5edb9` | Coordinator reviewed 170 distinct 320×320 RGBA frames and ten portraits, naming, metadata, style and sheets. [Bird report](T49_report.md). |
| Eggs / overlays | `f79dc3875ca7b6138e9c304059c2d3432a5c2775` | Asset `34eddebeca3b75fa5e39019090c8b486ba99911f`: five egg frames, two overlays, seven metadata files and contract. [Asset report](T49_eggs_report.md). |

Earlier v7 UI delivery `825ca1d` and world/booster delivery `6a29b50` are retained. Original/fallback assets remain; no cleanup cutover was added.

## Implemented contract

- Approved v7 plush/soft forest, honey wood, green ribbons and purple/green controls remain the visual direction. Authored boards, 40-level progression and existing gameplay/economy formulas remain unchanged.
- Level intro uses the existing LitMotion/V5Motion path, approximately 1.6 seconds, skippable; settled poses, tutorial/input boundary and subsequent new-friend/booster delivery are root-owned.
- Notification priority is world complete → world unlock → map daily → new friends in Meta debut order → booster cards. Never stack mandatory popups. Rook retains the no-home presentation.
- New-friend met/+20, daily/+50 and world-complete/+200 commit only on matching acknowledgement through the existing Meta policies. Ordinary win state/first-finale eligibility commits before choreography; receipts/balance/continuations persist together. Replay/preview does not duplicate notification rewards.
- First committed finale wins at 10/20/30/40 release their sequence when leaving Win. Worlds 1–3 open the map gate then world-unlock; deferred Next runs only afterward. World 4 returns to the map.
- Daily eligibility uses the injectable calendar clock and is map-only. Root acknowledgement/save followed by Map.CollectCoins is the single collection spectacle. Win HUD counting starts once from the actual view coin-flight callback with captured totals, never another grant.
- Ignored airborne taps and the one queued landing/effect tap invalidate hints but not the committed Board completion. D's transaction generation is separate from input/hint revision; cancellation/session/lifecycle/one-shot guards remain.
- Leaf wipe owns a full-Canvas layer. Old presentation cancellation precedes Play; covered BuildLevel/map setup does not cancel its own wipe. Completion starts intro/notification pumping; external cancellation discards the old continuation.
- Root always passes the displayed-node origin query's out value, including its center fallback, to the consumed-once level-card origin setter.
- A owns flock door/masked flight/close/check internally. E routes every BirdView body-frame change through synchronous overlay attachment; authored Sunny/Skye families use delivered normalized anchors and material-pose calibrations. No duplicate root or popup choreography was added.
- Existing fixture compatibility is corrected without execution: notification strings preserved separately from integer keys; unrelated gameplay fixtures seed acknowledged receipts via Meta and preserve their original expected balances; test asmdefs explicitly reference Meta.
- Shop/Aviary retain only their previously authorized placeholders. They are not new unfinished stage features. No new libraries, Core rules, authored levels, economy features or T51 work.

## Main reconciliation policy and static checks

PR35 is a squash: common ancestry is `48d4256`, while main already contains reviewed checkpoint `1abcee5`. Its ancestry conflicts are not authority to replace newer accepted code with older presentation logic.

- Keep the approved pre-main snapshot's runtime/art/fixtures and latest owner reports/status. Resolve source conflicts from those accepted versions; remove non-conflicting old-main residue as needed to recover exact approved bytes.
- Keep main's current Codex/parallel/OMP-preferred/Codex-fallback/model-choice/Mac-only execution directives in AGENTS.md, CLAUDE.md and TEAM.md. Those current directives override historical Claude-led prose. No retired coordinator is resumed.
- Rewrite this report and the shared handoff around the final state instead of restoring old WIP/feature-next instructions.
- Required version-control checks: approved runtime/art/fixture snapshot unchanged against pre-main `83a4d93`; no Core/authored-level/scene/ProjectSettings diff against incoming main; no unresolved conflicts; retain main directive files. These are source/tree checks only, not executed gameplay or compilation. Final observed results are recorded in the handoff.

## Evidence boundary

**NOT_RUN for this continuation:** Unity Editor/batch/import/compilation, C# compiler, EditMode, PlayMode, all code tests, runtime smoke, gameplay simulation/all-level playthrough, screenshots, timed animation captures, profiler/device/safe-area checks.

Offline asset readiness and coordinator static source acceptance are the only current acceptance evidence. Historical `1abcee5` suite/capture results and `build/screens/v7/` files apply only to their recorded checkpoint, not this integration. PR35 CI validated configuration/scripts and did not trigger Core tests. No new test/capture count is claimed.

## Runtime-only calibration / acceptance risks

- Unity 6000.6.7f1 import/compilation and actual sprite/meta interpretation are unverified.
- Sunny/Skye overlay head/chest attachment, roll, facing, chick/density scale, flight/cheer transitions and mixed legacy fallback appearance use reviewed static calibrations, not in-context proof. Other species do not gain invented family pose maps.
- Birdhouse doorway clipping/hinges, flock landings, popup silhouette layering, gate fake-3D swing, coin alignment and count timing need actual observation at K1/K3/K7 densities.
- Real input/teardown/rebuild timing, intro skip, queued busy taps, stale hints, mandatory popup acknowledgement, disable/reload persistence, date rollback/midnight and deferred finale navigation are not exercised.
- Full-screen wipe/safe-area coverage, transformed node origins, scroll inertia, ambient visual density/performance, muted audio and per-bird scheduling have not been run.

These are explicit runtime-only limits, not remaining code feature TODOs or claimed passes. The user will test after the coordinator's final PR/main integration.

## User entry and HOLD

User entry is **`PerchSort/Assets/Scenes/V5.unity`** in the `PerchSort` Unity 6000.6.7f1 project. Do not use the historical Game scene as the v7 entry. This worker has not opened Unity, modified either scene, merged into main or updated the original user-test checkout.

After isolated branch publication: **HOLD** for coordinator final tree/PR review. The coordinator records actual merged-main/push/checkout state after its own final integration. Subsequent development is Mac-only, starts from the final handoff and git status, never resumes old Claude, and decomposes every feature into parallel non-overlapping tasks (OMP preferred, Codex fallback; coordinator chooses model/effort). T51 and new features remain outside this stage.

## Observed final tree checks

Source-only staged checks passed: accepted pre-main PerchSort/art/approved_ui and owner records preserved exactly; no Core/authored-level/scene/ProjectSettings diff versus incoming main; AGENTS/CLAUDE/TEAM exactly match main; no unmerged entries and clean staged whitespace/conflict-marker check. Referenced reports and V5.unity exist. These checks do not execute product code or provide compilation/runtime evidence.

Resolved main→task merge boundary: **`571d4f39349fdd759bc73a88a72a54167f73ed81`** (parents: accepted runtime/art/E `83a4d93db4b811598a923f7926fb73389ec81a4b`, incoming main `3e82c64734f3e7291a332d1c0e90263954ce40cd`). Final-record commit is documentation-only; terminal submission pins its pushed tip and clean status. Coordinator final PR/main merge and original-checkout update remain separate, not performed by this worker.
