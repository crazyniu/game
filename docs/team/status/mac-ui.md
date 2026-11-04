# mac-ui — T50 current status

## Branch / checkpoint
- Branch: `mac-ui/t50`, resumed from lead-reviewed `dcd36a8`.
- Integration commit: `e7d84a5` (v7 UI, measured layout, importer and generated metadata).
- Latest code checkpoint: `46c977d` (complete v7 scenery/coloured illustrations, aligned feet/branch planes and new metadata). Earlier green milestones pushed at `3e6aef4` and `39dc66b`.
- Full handoff and exact lead popup policy: `tasks/T50_report.md`.
- Working directly. No sub-workers or services were started during this resumption.

## Done and verified
- Read the newest inbox/review after merging lead documentation. Merged successive T48/T49 deliveries, including measured layout, house alignment, gate/gift parts and current bird exports.
- Implemented `V7ArtImporter.cs` (4096px maps/backgrounds, aligned canvases, slices and hinge pivots), hooked into `V5SceneBuilder`. Runtime capture verifies the W1 map texture remains 1080x3000.
- Replaced provisional v6 geometry in `V7Layout.cs` with measured v7 screen rectangles, house-count adaptation, house doorway/hinge coordinates and all forty baked-path node centers.
- Integrated available v7 assets into `V5Art`, `V5UI`, `V5Button`, `HudView`, `BirdhouseView`, `BirdView`, `BranchView`, `BoardView`, `V5SceneView`, `V5ViewModel`, `SettingsPanel`, `WinPanel`, `V5Panels`, `V6MapModel` and `V6MapView`.
- Added actual popup presentations and root presentation API, authorized shop/Aviary placeholders, aligned gift composition and world-gate/sign views. No preview reward grants were added.
- Added required v7 captures plus map worlds/gates/test-mode and popup/placeholder previews. Corrected beam/chrome overlap, faded locked nodes, goal-info text clearance and gate centering after inspecting actual runtime images.
- Updated layout and runtime pose checks; removed old asset-filename-only assertions instead of re-pinning them. Core/Meta, level data, existing save/economy behavior and intentional skips remain unchanged.
- Lead review fixed: right-side rows interleave at exactly half-row pitch; opposite-side full bird canvases and branch artwork are separated. All 24 rows-2..5/K-2..7 layout combinations pass; regenerated K7 was visually inspected. No authored board edits.
- All six coloured booster illustrations are now bound to their delivered names. The unlock card displays the combined illustration in its measured main-object area; the obsolete glyph/egg fallback is removed.
- Merged `origin/mac-art/world`: all 16 backgrounds and all four branch/twig/trunk/crown sets. Removed legacy scenery fallback. `V7Layout`, `V5SceneView`, `BranchView` and `BirdView` align crown attachments, edge trunks, branch perch planes and feet; home-flight centering still passes its existing runtime regression.
- Latest graphics capture regenerated all required PNGs plus `v7_world_w1.png` through `v7_world_w4.png`. K3/K7, coloured Owl Eye, bamboo and maple were opened and visually inspected. Logs: `build/t50-world-tests.log`, `build/t50-world-capture.log`.

## Latest locked Mac verification
| Suite | Passed | Failed | Skipped |
| --- | ---: | ---: | ---: |
| EditMode (`build/test-results.xml`) | 453 | 0 | 0 |
| PlayMode (`build/playmode-results.xml`) | 31 | 0 | 8 |

`V5Capture.Run` completed with graphics. Required PNGs are in `build/screens/v7/`: `v7_level_k3`, `v7_level_k7`, `v7_map`, `v7_level_card`, `v7_win`, `v7_settings`, `v7_unlock_card`, `v7_goal_info`, `v7_popup_new_friend`. Additional captures cover all map worlds/gates, synthetic test-mode clears, daily gift, world complete/unlock and the authorized placeholders. Actual images were opened and inspected. No manual desktop playthrough, all-level gameplay run or new timed-animation evidence is claimed.

## Half done / visual blockers
- World scenery is fully delivered and integrated from `origin/mac-art/world`; no scenery fallback remains. Full visual acceptance still awaits the remaining bird/egg/overlay deliveries and lead review of actual captures.
- T49 now has complete Sunny/Skye/Berry/Minty sets (`c9fc784`), including corrected Berry plumes. Cocoa/Snow/Ember/Ink/Hoot/Rook exports, eggs and mama/chick overlays remain incomplete. Some K7 birds/mechanics still use legacy fallback.
- New popup views are implemented, but automatic scheduling, acknowledgement payouts, one-time bookkeeping and queue persistence are NOT implemented yet. Current runtime uses `V5Profile`/`V5Progress`; the existing Meta Aviary/Economy policy must be integrated without duplicate rewards.

## Exact next policy (recorded in full in T50 report)
New-friend cards follow intro/debut order and mark met/pay 20 only on acknowledgement, including Rook with 'Never goes home.'; daily gift is map-only on first calendar-day map open with injectable clock and 50 on collection; first finale win schedules one-time +200 world complete when leaving win; worlds 1-3 then run map-gate unlock and world-unlock before deferred Next continues; booster unlock follows intro. Priority: world complete -> world unlock -> daily gift -> new friend -> booster unlock. Never stack popups.

## Next
1. **Now, without waiting for art (latest lead instruction):** implement the exact Meta-backed popup queue and acknowledgement policy, persistence, one-time receipts, calendar clock, no stacking and EditMode/PlayMode coverage. This is not blocked by the visual gate.
2. Merge future T49/world/illustration deliveries while finishing visuals; then complete the intro and all Phase 2 bird/mechanic/ambient/UI/map/door animations and per-bird idle scheduler.
3. Add required cancellation/input/completion coverage and timed flight/home/win/map/intro/idle/new-friend/world/goal-info sequences. Stable PNGs are not animation proof.

## Submission hygiene
Every edited code file and generated V7 `.meta` is staged with the code checkpoint. The clean pre-run `Game.unity` and `V5.unity` bytes were restored after SceneBuilder; no unrelated package/settings edits were staged. `build/` evidence stays ignored. Unity runs ended and the owned lock was released.

IN PROGRESS: world/illustration captures and the board-overlap fix are green and ready for lead review. Exact popup policy is the immediate next implementation; it does not wait for remaining art.
