# T50 v7 integration milestone and remaining work

## Current handoff — HOLD requested by Codex coordinator
All source WIP is isolated in **`9f84a95`** on `mac-ui/t50`. Do not merge it into main: popup/profile/root/intro/gate integration is mid-flight and has not been compiled or tested. No tests, Unity runs or all-level playthrough were performed after the pause instruction. Full changed/new-file inventory, interface boundaries and unfinished fixture/persistence checks are in `docs/team/status/mac-ui.md`.

**Last review-ready checkpoint: `1abcee5`. The implementation/verification sections below describe that checkpoint, not `9f84a95`.** In particular, the WIP now adds a Meta world-claim API and notification persistence beyond the unchanged Meta/runtime state described in the reviewed milestone. Popup views previously existed; automatic queue/acknowledgement integration is now written but unverified in the WIP. No worker dispatch, merge or checkout change will occur until the current Codex coordinator supplies the exact ownership plan/checkpoint.

## Status
Green integration WIP on `mac-ui/t50`, resumed from lead-reviewed `dcd36a8`. This is not final v7 visual acceptance or Phase 2 completion. Work was performed directly; no sub-workers were started.

## Implemented and exercised
- Merged the lead review documentation and successive `origin/mac-art/t48` / `origin/mac-birds/t49` deliveries, including measured `art/v7/layout_v7.json`, aligned house/gate/gift contracts, all delivered UI parts and current bird exports.
- `V7Layout.cs` now contains the measured screen rectangles, HUD button families, K1 large-house/K2–K7 count adaptation, aligned 512px house door/hinge coordinates and all forty per-world baked-path node centers. Map stride is 2840px with 160px alpha overlap; top scrolling avoids exposing the top fade. Gameplay row density adapts to the actual authored branch counts.
- Lead review fix (`22df2a5`): right rows are exactly half a row lower; dense boards end at y=1640. Four-seat canvases stay within their own side (110px seat pitch), and 530px branch artwork cannot cover an opposite-side bird. Added all 24 rows-2..5/K-2..7 combinations checking bird rectangles, branch tips, house clearance and true half-row staggering. Regenerated and visually inspected K7: the center overlaps are removed without changing authored birds/branches.
- Added `Editor/V5/V7ArtImporter.cs`, invoked by `V5SceneBuilder`. Map/background textures allow 4096px; sprites preserve their aligned canvases, recorded nine-slice borders and door/lid hinge pivots. Graphics capture explicitly verifies the loaded W1 map texture is 1080x3000.
- World integration (`46c977d`): merged all 32 scenery PNGs and six coloured booster illustrations from `origin/mac-art/world`. V7 world backgrounds/branches/twigs/trunks/crowns are now required resources; legacy scenery and W1-substitution fallbacks are removed. Edge trunks start under the measured crown attachment and extend offscreen; mirrored crowns use per-world attachment coordinates. Branch flat-top coordinates follow the delivered W1/W2/W3/W4 contracts (twigs separately). Perched feet are pinned to that plane while home-flight visual centers still shrink exactly into their doorway.
- V7 resource names/load paths are used for buttons, glyphs, gifts, houses, panels and available bird frames/portraits. Missing T49 files retain explicit log-once legacy fallback. Original art remains unchanged.
- Replaced goal chips/tray with colored-roof birdhouses, aligned doorway portraits, ropes/beam and home checks. Matched available purple/green controls, red/white count bubbles, coin-pill text area and three/four-button layouts. Undo remains unlimited and does not receive a fictitious stock counter.
- Implemented the four-world scrolling map using delivered baked-path backgrounds, wooden/current/stone node states, stars, Sunny chick, gates/world signs and fixed chrome. Shop/Aviary open only the specifically authorized placeholders. Locked nodes retain their opaque stone appearance instead of the regular disabled-action fade.
- Converted settings, win, level card, booster unlock, goal info and all four popup presentations to delivered v7 parts. Added portrait rings/masks, win birds, aligned daily gift parts, gate/world-sign presentations and Minty's watering-can badge.
- Booster unlock cards now require the delivered `booster_hint_illus`, `booster_sapling_illus`, `booster_owl_eye_illus`, `booster_bell_illus`, `booster_scarecrow_illus`, `booster_rain_illus`. The full coloured composition uses the measured main-object area; the obsolete white-glyph/extra-egg fallback is removed.
- Added `V5GameRoot.OpenPopup` as a presentation entry point. **Automatic trigger scheduling, acknowledgement payouts and persistent popup bookkeeping have not been implemented yet.** Current preview calls do not grant currency. The live runtime still uses `V5Profile`/`V5Progress`; existing Meta policies must be reused without inventing a second reward rule.
- Existing map -> card -> level -> win -> map, settings, booster unlock, goal info, test stock and level-40 Next behavior remain covered by the passing suite. Core/Meta and authored levels were not edited.

## Verification
Every Unity run acquired the team lock, checked for other Unity processes and released the lock in `finally`. No Unity process was killed. Latest complete pre-checkpoint run:

| Suite | Passed | Failed | Skipped |
| --- | ---: | ---: | ---: |
| EditMode (`build/test-results.xml`) | 453 | 0 | 0 |
| PlayMode (`build/playmode-results.xml`) | 31 | 0 | 8 |

All existing intentional skips remain; no new skips or raised budgets. `V5LayoutTests.cs` replaces withdrawn tray/tip geometry assertions with house-door containment, three/four-button separation, seat/touch-region bounds, modal target separation and measured world-node bounds. The obsolete sprite-name-only `ViewPolicyTests.PhaseSleepAndHiddenEggKeepDifferentPresentations` test was removed with its unused `RestFrame` string helper. Runtime sleep/rest-pose checks now verify the displayed pose rather than old filenames. One old `star_small_full` filename assertion was removed rather than re-pinned to a new asset filename; test-mode accessibility/progress/stock/economy assertions remain. Net EditMode change: +4 layout cases, -1 incidental naming case, 426 -> 429.
The subsequent lead-review matrix adds 24 cross-board clearance cases: 429 -> 453. `build/t50-review-tests.log` and `build/t50-review-capture.log` record the latest green run and graphics capture after merging T49 `c9fc784`.
The latest world integration run is also 453 EditMode passed and 31/0/8 PlayMode. Logs: `build/t50-world-tests.log`, `build/t50-world-capture.log`. Regenerated K3/K7, coloured Owl Eye, bamboo and maple images were opened and inspected. Additional actual authored-board captures are `v7_world_w1.png` through `v7_world_w4.png`; these exercise one authored variant per world, not all sixteen variants or all forty levels.

Graphics-enabled `V5Capture.Run` completed. Actual runtime captures were opened and inspected, including corrections for beam/chrome overlap, locked-node fading, goal-info text clearance and world-gate placement. Captures are under `build/screens/v7/`:
- Required: `v7_level_k3.png`, `v7_level_k7.png`, `v7_map.png`, `v7_level_card.png`, `v7_win.png`, `v7_settings.png`, `v7_unlock_card.png`, `v7_goal_info.png`, `v7_popup_new_friend.png`.
- Additional: `v7_popup_daily_gift.png`, `v7_popup_world_complete.png`, `v7_popup_world_unlock.png`, `v7_map_gate_w2.png` through `v7_map_gate_w4.png`, `v7_map_w2.png` through `v7_map_w4.png`, `v7_map_test_mode.png`, `v7_shop_placeholder.png`, `v7_aviary_placeholder.png`.

Capture profiles are synthetic/nonpersistent and do not write PlayerPrefs. Popup reward numbers in previews are presentation parameters, not executed grants. Map previews for all worlds are not an all-forty-level gameplay run. No new Phase 2 timed frame sequences or manual desktop playthrough are claimed.

## Visual prerequisites / known differences
- T49 now exports complete Sunny/Skye/Berry/Minty sets, including Berry's corrected reference head/plumes and all seventeen Minty poses (`c9fc784`). Cocoa/Snow/Ember/Ink/Hoot/Rook sets remain incomplete. V7 eggs and mama/chick overlays are not delivered.
- All world scenery and coloured booster artwork is integrated. Actual K3/K7 captures now use v7 backgrounds, thin branches rooted in edge trunks and crowns above; Owl Eye uses the delivered coloured illustration. These are no longer visual blockers.
- Approved concepts use illustrative boards/counts. Captures preserve actual authored level-5/40 boards rather than changing level data to imitate the screenshots.
- Rendered popup views are real, but their automatic gameplay scheduling/payout integration is still pending the exact policy below.

## Popup trigger policy — exact lead decision
1. **New friend:** when a level starts and it contains a species the profile has never met (Meta Aviary OnLevelStarted), show the new-friend popup AFTER the level intro finishes and BEFORE input unlocks; one popup per new species in debut order; mark met and pay the 20 coins when the popup is acknowledged. Rook (crow) also gets it, with the line **'Never goes home.'** instead of a gift.
2. **Daily gift:** on the first map open of each calendar day (injectable clock, Meta Economy daily rule), show it over the map; collecting pays 50 coins with the coins-to-pill animation. Never during a level.
3. **World complete:** after the FIRST win of level 10, 20, 30 or 40, when the player leaves the win panel (Next or Home), show the world-complete popup (gate opens, birds fly through, +200 once).
4. **World unlock:** right after world complete (worlds 1-3), the map opens scrolled to the gate, plays the gate unlock animation, then the world-unlock popup; Next from the win panel then continues into the next world's first level only after this sequence.
5. **Booster unlock card:** unchanged (shown when its unlock level first starts, after the intro).
6. **Order when several are due at once:** world complete -> world unlock -> daily gift -> new friend -> booster unlock. Never stack two popups; queue them.

This policy is recorded, not implemented in this checkpoint. Reuse the existing Meta Aviary/Economy rules while deferring the new-friend mark/payment to acknowledgement exactly as directed; do not grant preview rewards or duplicate profile rewards.

## Next
1. **Immediate next task (lead explicitly removed the art dependency):** implement the exact popup queue, acknowledgement payouts and persistence now, using Meta Aviary/Economy without duplicate rewards. Cover order, one active popup, debut/Rook handling, calendar clock, one-time finale rewards and deferred Next in EditMode/PlayMode.
2. Merge subsequent T49 exports as they land and finish the remaining bird/egg/overlay parity; regenerate metadata and rerun locked suites/captures after each integration milestone.
3. Implement all remaining Phase 2 intro/bird/mechanic/ambient/UI/map/door animations and the random per-bird scheduler, including specified missing-part procedural/fade fallbacks. The intro completion must gate new-friend/booster delivery before board input is enabled.
4. Add the required completion/cancellation/input tests and timed sequences: flight, flock home, full win, map scroll, intro (approximately twelve frames), four-second idle, new friend, world complete/unlock and goal info.
5. Run the full Mac suite before every push. Stage every edited source and generated `.meta` file. Never represent stable PNGs as animation proof.

## Workspace hygiene
Generated V7 `.meta` files and the new importer metadata are staged with code so GUIDs remain stable. `Game.unity` and `V5.unity` were restored to their exact clean pre-run snapshots; no package/settings changes are needed. `build/` evidence is ignored, not committed. The Unity lock was released after the latest run; no sub-workers/services were started.
