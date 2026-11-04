# Current-stage coordination and Mac handoff

## Latest user instructions (2026-11-04)

- The current Codex conversation on 251 replaces Claude as coordinator for this stage. It assigns and accepts work; bounded feature implementation belongs to the existing parallel Mac workers. Never contact/resume the former Claude session.
- Every future feature must be decomposed into parallel tasks. OMP is preferred, Codex fallback; the coordinator chooses `gpt-6.1-terra`, `gpt-6.1-sol`, `gpt-6-astra` and effort based on complexity.
- Finish the existing T49/T50 v7 stage. The coordinator reviews submissions before issuing integration/merge authority. D retains isolated implementation/fixture commits; only approved T50A `a012e70` and T50C `fb90bb7` may now be merged into its task branch. B remains pending. Never merge main, deploy or release from this worker.
- No code tests, test suites, Unity run or all-level playthrough. The user tests the integrated result later. Static review is not compilation/runtime evidence; prior passing numbers apply only to their recorded revisions.
- After reviewed integration/merge, pause with a Mac-only continuation handoff. Do not continue feature development on 251. T51 cleanup and extra features remain deferred.
- Preserve approved v7 art, original/fallback assets, Core, authored levels and economy. No artwork generation in the code modules.

## Coordinator channel

- Native Codex chat: `orca_session_id:codex_0551b7e6_4ea7_4c29_936b_a50a25d9c1ec` on 251; it has no terminal handle.
- Cross-server automatic Dispatch/adoption is unavailable (`server_required` before any Task/Dispatch was created). No invented Dispatch IDs, forwarding claims or sub-workers.
- Workers report in their own Orca terminal and owned status/report files. The current coordinator reads these channels.

## User-test main checkpoint — do not touch

- Coordinator confirmed PR35 merged at **`3e82c64734f3e7291a332d1c0e90263954ce40cd`**.
- Coordinator confirmed original `/Users/apple/Documents/projects/game` is clean on that main for user testing. Never edit or switch that checkout from a worker. Prior mac-ui OMP released all edits and is **HOLD**.
- PR35 required CI validated changed configuration/scripts only; no Core tests were triggered. This is not T49/T50 continuation compilation/runtime evidence.

## Four isolated T50 code workspaces

All branches preserve source WIP `9f84a95`; T50D starts at handoff checkpoint `0b44966`. Only explicitly reviewed A/C revisions are currently approved for the D integration branch.

| Module | Branch | Workspace | Ownership / progress |
| --- | --- | --- | --- |
| T50A | `mac-motion/t50` | `/Users/apple/orca/workspaces/game/t50-bird-motion` | Coordinator statically approved `a012e70` (implementation `39f36f5`). Board internally wires flock-door/masked-flight/close/check; no extra root hook or duplicated goal-popup choreography. |
| T50B | `mac-popups/t50` | `/Users/apple/orca/workspaces/game/t50-popup-motion` | `7ddcb721` under review; coordinator-requested corrections pending, including pre-ack daily currency flight removal. Do not merge yet. |
| T50C | `mac-map-motion/t50` | `/Users/apple/orca/workspaces/game/t50-map-motion` | Coordinator approved `fb90bb7` (includes `3542ced` and delivered W1 leaf resource correction). Leaf/origin interfaces unchanged. |
| T50D | `mac-notifications/t50` | `/Users/apple/orca/workspaces/game/t50-notifications` | D implementation `52cad38e2742263b4469ead8ef5a90d80f564b73`; fixture compatibility `e39e0dd`, no execution. Preparing D report/review boundary, then approved A/C integration only. [Status](status/mac-notifications.md), [report](../../tasks/T50D_report.md). V5Progress/Meta bridge reused unchanged. |

Public view signatures remain stable. New narrow APIs must be listed exactly in the owning report. Root integration must reject obsolete callbacks, cancel on teardown/rebuild, restore poses and avoid duplicate completion/grants. No two workers edit another module's files.

Approved cross-module root contract:
- `Map.TryGetLevelCardOrigin(int, out Vector2)` → always `panels.SetLevelCardOrigin(point)` → `panels.LevelCard(int)`, including the returned `(540,960)` fallback.
- Full-canvas leaf owner is independent of Map/Board/HUD/modal groups. Cancel the old presentation before starting its wipe; its covered `BuildLevel`/map-surface setup does not cancel or invalidate that same wipe. Completion starts intro/notification pumping; external cancellation discards the old continuation.
- `WinPanel.Show(int, int, int, Action coinFlightStarted)` preserves the old overload. Root commits ordinary rewards/first-finale pending state before choreography, and invokes only the existing HUD count from this guarded callback with captured totals.
- New-friend/world/daily receipts remain acknowledgement-only. `Map.CollectCoins` after root's daily acknowledgement is the single collection spectacle; T50B removes the pre-ack daily flight. Views never grant/save rewards.
- Existing gate `Configure(V5Art,int,bool)`, `Open(Action)`, `IsOpening` remain unchanged. Booster callbacks use T50B's generation-guarded `Once`; no extra booster overload is needed.

## Art ownership

- Ten bird species remain actively owned by `mac-birds/t49`, including remaining species/portraits/alignment/recognition evidence.
- Eggs (five frames) and mama/chick overlays (two) have now been acknowledged and actively transferred to `mac-art/world`. This supersedes the old pending-transfer wording.
- Earlier UI delivery: `mac-art/t48` at `825ca1d`; world/booster delivery: `mac-art/world` at `6a29b50`. Later delivery SHAs must come from those workers, not inferred here.

## Historical evidence / recovery

- Prior reviewed checkpoint `1abcee5` recorded EditMode 453 passed and PlayMode 31 passed / 0 failed / 8 skipped. These historical counts do not apply to `9f84a95` or the parallel continuation.
- Historical screenshots: `build/screens/v7/`; no new screenshots, timed sequences or runtime evidence are claimed by this document.
- 251 recovery notes previously recorded untracked `art/v7/review_3_runtime_vs_approved.png` and `review_4_runtime_with_world_art.png`; preserve pre-existing work.

## Final integration handoff required before pause

The coordinator/integration owner records reviewed module/art SHAs, merged main SHA and remote push state, the Mac checkout/branch/dirty state, completed/missing requirements, exact new APIs, remaining compile/runtime risks, captures actually produced, and tests explicitly **NOT_RUN**. User entry: open the integrated `PerchSort` project and existing `Assets/Scenes/V5.unity` scene; runtime readiness remains unverified until user testing. A future Mac Codex continuation must read this handoff and inspect git status without resuming the former Claude session.
