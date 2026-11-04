# mac-ui — T50 current status

## Branch / checkpoint
- Branch: `mac-ui/t50`, resumed from lead-reviewed `dcd36a8`.
- Integration commit: `e7d84a5` (v7 UI, measured layout, importer and generated metadata).
- Latest code checkpoint: `d2b9f08` (`wip: center v7 world gates between measured level nodes`).
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

## Latest locked Mac verification
| Suite | Passed | Failed | Skipped |
| --- | ---: | ---: | ---: |
| EditMode (`build/test-results.xml`) | 429 | 0 | 0 |
| PlayMode (`build/playmode-results.xml`) | 31 | 0 | 8 |

`V5Capture.Run` completed with graphics. Required PNGs are in `build/screens/v7/`: `v7_level_k3`, `v7_level_k7`, `v7_map`, `v7_level_card`, `v7_win`, `v7_settings`, `v7_unlock_card`, `v7_goal_info`, `v7_popup_new_friend`. Additional captures cover all map worlds/gates, synthetic test-mode clears, daily gift, world complete/unlock and the authorized placeholders. Actual images were opened and inspected. No manual desktop playthrough, all-level gameplay run or new timed-animation evidence is claimed.

## Half done / visual blockers
- `Resources/V7/World/` is absent: v7 level backgrounds, branches, trunks and crowns are missing. Runtime levels still use explicit legacy scenery fallback and therefore do not match the approved full v7 screens yet.
- T49 currently has complete Sunny/Skye/Berry sets and Mint idle/face. Mint's remaining poses and Cocoa/Snow/Ember/Ink/Hoot/Rook exports, eggs and mama/chick overlays remain incomplete. Some K7 birds/mechanics still use legacy fallback.
- Owl Eye's delivered white glyph differs from the colored eye illustration in the approved unlock concept; art/composition review remains necessary.
- New popup views are implemented, but automatic scheduling, acknowledgement payouts, one-time bookkeeping and queue persistence are NOT implemented yet. Current runtime uses `V5Profile`/`V5Progress`; the existing Meta Aviary/Economy policy must be integrated without duplicate rewards.

## Exact next policy (recorded in full in T50 report)
New-friend cards follow intro/debut order and mark met/pay 20 only on acknowledgement, including Rook with 'Never goes home.'; daily gift is map-only on first calendar-day map open with injectable clock and 50 on collection; first finale win schedules one-time +200 world complete when leaving win; worlds 1-3 then run map-gate unlock and world-unlock before deferred Next continues; booster unlock follows intro. Priority: world complete -> world unlock -> daily gift -> new friend -> booster unlock. Never stack popups.

## Next
1. Complete full v7 visual parity with remaining T49 assets and review Owl Eye composition; rerun locked suites/captures after merges.
2. Only after that visual gate, implement the exact popup queue/acknowledgement policy with the intro, then all Phase 2 bird/mechanic/ambient/UI/map/door animations and per-bird idle scheduler.
3. Add required cancellation/input/completion coverage and timed flight/home/win/map/intro/idle/new-friend/world/goal-info sequences. Stable PNGs are not animation proof.

## Submission hygiene
Every edited code file and generated V7 `.meta` is staged with the code checkpoint. The clean pre-run `Game.unity` and `V5.unity` bytes were restored after SceneBuilder; no unrelated package/settings edits were staged. `build/` evidence stays ignored. Unity runs ended and the owned lock was released.

BLOCKED: final phase-1 visual acceptance awaits remaining v7 artwork/review; Phase 2 and popup policy implementation remain pending, not completed.
