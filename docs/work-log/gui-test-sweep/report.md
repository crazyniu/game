# Standalone test sweep repair

## Baseline and boundaries

Baseline main: `6d696bfd5b0d34330074d7813a5b74a7f14891c6` (after #270, #275 and #276). Changes are confined to tests and this work log. No gameplay, UI, save or asset changes.

Actual executable: Godot `4.6.3.stable.official.7d41c59c4`, GL Compatibility, software Mesa/llvmpipe, Xvfb 1280x2000, game 1080x1920. The repository calls for 4.7.2; these are **not** 4.7.2 or physical-device results. Test profiles are isolated. No visual acceptance or real ad/payment integration is claimed.

## Repairs and retained coverage

- `check_family_pointer`: formerly waited forever for human clicks. Now emits genuine pointer events on disabled Play, Close, Home, Retry, Next and world Play; checks exact callback counts and bounds every close wait by two seconds. The outer sweep remains capped at 300 seconds.
- `check_trail_clearance` is renamed/repaired as `check_map_bird_clearance`, preserving its UID. The old test used a perch over the tile and required decorative birds to fit inside the tile's click area. Production now places the bird beside the tile. The replacement preserves all-pose alpha metadata, idle envelope, screen clearance and other-marker separation across **210 levels x 9 species**, using the current side-perch geometry. `run_map_nodes` separately verifies actual rendered feet/centering and motion; `check_map_hit_regions` retains actual input coverage for all 210 levels. Its summary no longer misleadingly says 100.
- Retired `check_tree_pointer`: interactive-only old-map script, replaced by the automated current map pointer/geometry tests. Retired `capture_tree_motion` (hard-coded crop of removed tree) and `measure_map_birds` (manual retired-tree generator). Current motion capture remains in `run_map_nodes`; no executable callers referenced these utilities.
- `run_tool_inventory_gui`: baseline already passed the signal-only zero-stock check. Added the actual Main one-more handler, Buy/WatchAd presence, production price, bounded cancellation, unchanged profile and no shop overlay. It does not claim the old issue's signal failure still exists.
- `run_purchase_surface`: default execution now checks current purchase card/title-room/shared nine-patch, shading and input contracts instead of requiring an absent historical file. Removed stale AtlasTexture/ForestBackground assumptions from a retired material. Explicit `--surface-baseline=<file>` still compares historical non-panel geometry; record mode still requires a path. Default logs clearly state historical comparison **NOT RUN**. No newly recorded baseline is presented as historical equivalence.
- `run_stuck_rescue`: the old assertion conflated background-search main-thread work with whole-frame rendering/scheduling and required each full frame below 16 ms (even nominal 60 Hz is approximately 16.67 ms). Preserve the original below-16-ms measured main-thread slice check; add bounded search completion; retain all frame-gap logs. A positive `--max-frame-ms=<budget>` explicitly enables whole-frame acceptance for the intended device. Default clearly reports device-frame-rate acceptance **NOT RUN**, not “no lag”. This separation was approved by the coordinator. Original baseline gaps of 35–42 ms and sub-0.2-ms main-thread slices remain in evidence. Strict 0.001-ms negative-control run correctly fails all three frame gates; invalid nonpositive budgets exit 1.
- `run_shop` and `run_shop_cards_gui` now default screenshots to ignored build directories instead of polluting the project root. Existing explicit output parameters remain supported.
- Added `tests/sweep_gui.py`: discovers all standalone run/check scripts, uses scenes for Node tests, records engine version, duration/exit/error in CSV and JSON, creates per-test isolated user-data directories, enforces the 300-second timeout and supports focused repeat runs. It requires an existing graphical display. `scripts/run_tests.sh` is unchanged.

## Remaining acceptance

`run_purchase_close` baseline passed once, but three focused repetitions reproduced the fixed-0.20-second close wait race on Chinese outfit states. The test now waits for the actual modal lifetime, with a two-second failure watchdog. It retains exact callback and unchanged-profile assertions, and separately logs elapsed close time against the production 120-ms tween; a two-second watchdog is not a claim that a two-second animation is acceptable. The active #277 integration changes shell geometry in another hunk; this patch does not overwrite those checks. Validate the combination after integration. This evidence does not establish real-device frame-rate acceptance or the documented 4.7.2 engine run.

## Commands and local evidence

- Baseline: standalone GUI invocation for all 43 run/check scripts, timeout 300 seconds each. `build/test-sweep/before/results.json`, one log per test.
- After: `python3 PerchSort/tests/sweep_gui.py --output build/test-sweep/after` on an existing TCP Xvfb display.
- Repeated existing aviary/map regressions: `... --repeat 3 --output build/test-sweep/repeated run_aviary_actions run_map_nodes`.
- Focused inventory/close runs: `... --repeat 3 --output build/test-sweep/focused run_tool_inventory_gui run_purchase_close run_purchase_surface`; intermediate surface failures are retained, final repaired surface has separate `surface-fixed` evidence.
- Aggregate: isolated `XDG_DATA_HOME` followed by `bash scripts/run_tests.sh`; `build/test-sweep/core.log`, exit 0. All 210 stored solutions and 94 goal checks pass; deliberately invalid save/goal fixtures emit expected diagnostics.
- Two attempts collided with already-reserved Xvfb display numbers and did not launch tests; kept in `*-display-unavailable`, excluded from semantic before/after counts. The final run uses a separate display and verifies server startup. A separate negative-control attempt lacked writable default user data and crashed before the test; the isolated retry correctly exercised argument validation. Neither infrastructure failure is counted as a game result.

Evidence is ignored and is not committed. No issue was closed or branch merged by this work.

## Sweep results

Results table is generated from the completed logs below; PASS is automated contract execution, not device or visual approval.

Baseline: **38/43 pass**, five known test/precondition failures. After: **42/42 pass** (obsolete manual check removed, clearance renamed), with the final close change additionally verified three times.

| Test | Before | After | Failure explanation / change |
| --- | --- | --- | --- |
| `check_family_pointer.gd` | FAIL 300.02s | PASS 3.78s | Manual input wait timed out; automated pointers and bounded close. |
| `check_map_hit_regions.gd` | PASS 12.85s | PASS 16.47s |  |
| `check_trail_clearance.gd` | FAIL 2.07s | PASS 2.28s | 420 retired on-tile containment assertions; replaced by current side-perch clearance. |
| `check_tree_pointer.gd` | FAIL 300.02s | retired | Manual old-map wait timed out; retired, current map input retained. |
| `run_aviary_actions.gd` | PASS 7.15s | PASS 6.85s |  |
| `run_aviary_text.gd` | PASS 6.11s | PASS 6.73s |  |
| `run_bird_animation.gd` | PASS 5.06s | PASS 5.17s |  |
| `run_board_hud.gd` | PASS 4.15s | PASS 4.69s |  |
| `run_board_shop_refresh.gd` | PASS 10.06s | PASS 10.80s |  |
| `run_chest.gd` | PASS 30.01s | PASS 30.76s |  |
| `run_core.gd` | PASS 0.87s | PASS 1.24s |  |
| `run_daily_reward_flow.gd` | PASS 3.99s | PASS 4.44s |  |
| `run_dialog_skin.gd` | PASS 0.72s | PASS 1.23s |  |
| `run_draw_purchase.gd` | PASS 6.73s | PASS 7.04s |  |
| `run_draw_result.gd` | PASS 13.48s | PASS 13.91s |  |
| `run_family_dialogs.gd` | PASS 10.50s | PASS 10.59s |  |
| `run_feather_prices.gd` | PASS 1.66s | PASS 1.02s |  |
| `run_gameplay_dialogs.gd` | PASS 8.10s | PASS 8.10s |  |
| `run_goal.gd` | PASS 4.61s | PASS 4.15s |  |
| `run_heading_composites.gd` | PASS 1.89s | PASS 1.89s |  |
| `run_home.gd` | PASS 25.52s | PASS 25.94s |  |
| `run_level_card_tools.gd` | PASS 3.54s | PASS 3.95s |  |
| `run_level_heading.gd` | PASS 3.93s | PASS 4.43s |  |
| `run_live_feather_balances.gd` | PASS 10.32s | PASS 10.87s |  |
| `run_map_aviary.gd` | PASS 23.00s | PASS 23.16s |  |
| `run_map_card_clip.gd` | PASS 2.38s | PASS 2.48s |  |
| `run_map_nodes.gd` | PASS 5.96s | PASS 6.31s |  |
| `run_menu_navigation.gd` | PASS 5.80s | PASS 5.96s |  |
| `run_purchase_buttons.gd` | PASS 9.57s | PASS 9.18s |  |
| `run_purchase_close.gd` | PASS 17.70s | PASS 17.53s |  |
| `run_purchase_dialogs.gd` | PASS 6.62s | PASS 6.88s |  |
| `run_purchase_surface.gd` | FAIL 0.72s | PASS 7.95s | No historical baseline argument; default contracts restored and stale material assertions repaired. |
| `run_reward_quantities.gd` | PASS 4.26s | PASS 4.67s |  |
| `run_shop.gd` | PASS 21.32s | PASS 22.64s |  |
| `run_shop_amounts.gd` | PASS 9.61s | PASS 12.78s |  |
| `run_shop_cards.gd` | PASS 3.44s | PASS 3.39s |  |
| `run_shop_cards_gui.gd` | PASS 19.00s | PASS 20.69s |  |
| `run_stuck_rescue.gd` | FAIL 6.68s | PASS 6.28s | Whole-frame <16ms failed on software renderer; split measured main-thread budget from explicit device frame budget. |
| `run_tool_guides.gd` | PASS 18.41s | PASS 19.91s |  |
| `run_tool_inventory_gui.gd` | PASS 4.19s | PASS 6.67s |  |
| `run_undo_allowance.gd` | PASS 3.19s | PASS 3.40s |  |
| `run_unlimited.gd` | PASS 0.67s | PASS 0.77s |  |
| `run_visible_skills.gd` | PASS 4.80s | PASS 5.12s |  |

### Required three consecutive runs

| Test | Durations (s) | Outcome |
| --- | --- | --- |
| `run_aviary_actions.gd` | 9.2, 7.72, 6.84 | PASS,PASS,PASS |
| `run_map_nodes.gd` | 6.99, 6.68, 6.5 | PASS,PASS,PASS |
| `run_tool_inventory_gui.gd` | 6.37, 7.04, 6.42 | PASS,PASS,PASS |
| `run_purchase_close.gd` | 16.1, 16.17, 17.54 | PASS,PASS,PASS |
| `run_purchase_surface.gd` | 8.04, 6.94, 7.46 | PASS,PASS,PASS |

Final close telemetry: 180 double-pointer cases, minimum 20.19ms, median 120.81ms, maximum 256.06ms from tap to freed modal. Production close tween is 120ms; measured end-to-end includes scheduling and queued deletion. No case approached the 2000ms watchdog. These are software-rendered cloud observations, not mobile frame-rate certification.
