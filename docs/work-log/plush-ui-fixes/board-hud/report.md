# Board HUD alignment report

Branch: `mac-board-hud/work`. Base: `f1b37110ec0eca21e4fb7df2542edaba2eb07c7f` (`origin/main` at assignment).

The HUD and toolbar are aligned to the four measured board references. The complete three-tool reference state has a maximum measured rectangle deviation of 19 px. Across all visible HUD states, the maximum after the documented data-dependent positioning is 19 px. This is HUD geometry evidence, not a claim that the entire board or all existing artwork matches the mockups.

## Changes

- Enlarged the cream counters and painted numerals; placed the coin, footprint, divider, three stars and infinity threshold at their measured positions. The move-number box leaves space on both sides even for the captured 999 value. Day/night remains conditional on the existing level data.
- Positioned one house and rope at the measured bounds. Cropped the existing house loop and rope geometrically rather than compressing the whole rope. The doorway target and entry mask follow the displayed house.
- Filled the goal rings with masked, animated front portraits; enlarged the existing species gift-coin badges at their lower-right corners. Existing delivery checks and bird motion remain wired.
- Matched settings and tool-disc/icon sizes with existing gold rims. Visible tools center together; the cream bar contracts with their count and disappears with zero tools. Disabled tools retain the same dimensions. Hint pointers, halo and booster flight origins follow the current tool position.
- Added Editor batch-capture-only logging in HudView; no player-visible debug UI and no capture-tool or state-provider edits.

Only HudView.cs, BirdhouseView.cs, the four board entries in layout_metrics.json and corresponding GameLayout.cs cases changed. HudSnapshot.cs needed no change. No rules, saves, levels, IScreenStates names, scene, package, project settings or art files are included.

## Measurements and interpretation

[layout-map.md](layout-map.md) lists **every element** of all four JSONs and its runtime object or ownership boundary. Positions convert absolute top-left pixels to the existing parent's top-left anchor space. Existing transparent image padding is compensated; actual RectMask2D and portrait masks bound the crops.

[compare_rects.py](compare_rects.py) reads live rendered mesh/RectTransform rectangles logged during each capture, then compares x, y, width and height against the supplied layout JSONs. Image alpha bounds use the existing source PNGs at alpha > 16. Number measurements use the actual PaintedNumber layout boxes; the glyphs themselves fit inside those boxes. Baked house parts, coin emblems and gift details have no independent RectTransform and are explicitly identified in the map rather than reported as separately verified.

The four references measure the same partially occluded coin backing differently: x ranges from 91 to 129. A shared backing `[110, 59, 233, 104]` stays within 19 px of those individual backing measurements; the many-branches JSON instead measures the union with the coin. The move-number box is `[122, 195, 50, 50]` to retain a gap before the divider with longer values.

The references show three species and three tools. Existing fixtures also show two, four or seven species, hide Undo, omit the phase counter, or recommend a different tool. The table retains the **raw** comparison to the reference and a separately labelled **adapted** comparison after only these count/target translations. Toolbar width changes by 240 px per missing slot; goal pitch remains 139.5 px and tool pitch remains 240 px. Values and availability are never changed to impersonate the reference. Hidden/unavailable HUDs are **not assessed**, not zero-error passes.

Full comparator stdout: [rect-output.txt](rect-output.txt). Structured results: [rect-results.json](rect-results.json). Exact capture log payloads: [rendered-rects.log](rendered-rects.log).

Reproduce the numeric report without Unity:

```sh
uv run --with pillow python docs/work-log/plush-ui-fixes/board-hud/compare_rects.py docs/work-log/plush-ui-fixes/board-hud/rendered-rects.log
```

## Captures and checks

All final captures are 1080 x 1920. Each committed JPEG is 1440 x 1280, with the mockup on the left and capture on the right at the same 1280-pixel height. Every state's HUD/tool regions were visually reviewed in the side-by-sides, with full frames reviewed for the representative reference, first visit, real first board and dense board states.

| Command | Captured | Runtime errors |
| --- | ---: | ---: |
| `bash scripts/state_captures.sh -states "Access"` | 10 | 0 |
| `bash scripts/state_captures.sh -states "Data"` | 18 | 0 |
| `bash scripts/state_captures.sh -states "Hints"` | 8 | 0 |
| `bash scripts/state_captures.sh -states "First board"` | 1 | 0 |
| `bash scripts/state_captures.sh -states "Plush"` | 9 | 0 |

The Data substring also selects two Environment states; their evidence is included. `First board` is used because the actual provider is named `Real game`, with a space; `Real_game` is only the output-folder spelling. Plush covers the four explicit visual references and their existing related states. Each final index reports four settle frames and `listenerVolume=0; listenerPaused=true`; all runs use the script's built-in shared Unity lock and synthetic nonpersistent profiles.

An initial capture failed compilation because the new logger used the wrong GetMesh signature. It was corrected to Unity's parameterless CanvasRenderer.GetMesh, and all final runs completed. The script's built-in naming self-check ran 14 cases and its naming scan passed. The requested final `python3 ci/check_current_game_names.py` also passed. No EditMode/PlayMode suites, separate lint/build workflow, audio listening, timed motion capture or device test ran.

Unity regenerated metadata during import. All 925 modified tracked metadata files were restored against the recorded pre-Unity hashes; 242 newly generated metadata files were removed. Scene, package and project settings stayed unchanged. No build output or metadata noise is committed.

## Per-state results

Every visible state retains the shared art substitutions listed below and the unowned board scenery differences. “Adapted” does not waive those visual differences; it only separates the expected count/availability translation from placement error. All distances are pixels, max over x/y/width/height.

| State / side-by-side | Mockup | Raw max | Adapted max | State-specific differences |
| --- | --- | ---: | ---: | --- |
| [Access Context absent](board-access_context_absent.jpg) | board_base | 240.00 | 17.00 | 2 goals; 2 tools; phase hidden. |
| [Access Context does not add toolbar slot](board-access_context_does_not_add_toolbar_slot.jpg) | board_base | 240.00 | 17.00 | 2 goals; 2 tools; phase hidden. |
| [Access Context present](board-access_context_present.jpg) | board_base | 240.00 | 17.00 | 2 goals; 2 tools; phase hidden. |
| [Access First visit before tools unlock](board-access_first_visit_before_tools_unlock.jpg) | board_base | 70.25 | 17.00 | 2 goals; 0 tools; phase hidden. Bar hidden. |
| [Access Later visit](board-access_later_visit.jpg) | board_base | 70.25 | 17.00 | 2 goals; 3 tools; phase hidden. |
| [Access Sapling already used](board-access_sapling_already_used.jpg) | board_base | 240.00 | 17.00 | 2 goals; 2 tools; phase hidden. Disabled discs keep size. |
| [Access Three slots with Undo hidden](board-access_three_slots_with_undo_hidden.jpg) | board_base | 240.00 | 17.00 | 2 goals; 2 tools; phase hidden. |
| [Access Undo hidden after final undo](board-access_undo_hidden_after_final_undo.jpg) | board_base | 240.00 | 17.00 | 2 goals; 2 tools; phase hidden. |
| [Access Undo hidden at start](board-access_undo_hidden_at_start.jpg) | board_base | 240.00 | 17.00 | 2 goals; 2 tools; phase hidden. |
| [Access Undo shown after move](board-access_undo_shown_after_move.jpg) | board_base | 70.25 | 17.00 | 2 goals; 3 tools; phase hidden. |
| [Data 2 rows](board-data_2_rows.jpg) | board_base | 240.00 | 17.00 | 2 goals; 2 tools; phase hidden. |
| [Data 2 species](board-data_2_species.jpg) | board_base | 240.00 | 17.00 | 2 goals; 2 tools; phase hidden. |
| [Data 3 rows](board-data_3_rows.jpg) | board_base | 282.50 | 17.00 | 7 goals; 2 tools; phase hidden. |
| [Data 4 rows](board-data_4_rows.jpg) | board_many_branches | 281.50 | 6.00 | 7 goals; 2 tools; phase hidden. |
| [Data 5 rows](board-data_5_rows.jpg) | board_many_branches | 281.50 | 6.00 | 7 goals; 2 tools; phase hidden. |
| [Data 7 species](board-data_7_species.jpg) | board_many_branches | 281.50 | 6.00 | 7 goals; 2 tools; phase hidden. |
| [Data Added branch](board-data_added_branch.jpg) | board_base | 240.00 | 17.00 | 3 goals; 2 tools; phase hidden. |
| [Data All authored seats occupied](board-data_all_authored_seats_occupied.jpg) | board_many_branches | 281.50 | 6.00 | 7 goals; 2 tools; phase hidden. |
| [Data All home with crow remaining](board-data_all_home_with_crow_remaining.jpg) | board_base | 240.00 | 17.00 | 2 goals; 2 tools; phase hidden. Delivery checks replace gift badges. |
| [Data Eleventh branch - six rows](board-data_eleventh_branch_-_six_rows.jpg) | board_many_branches | 281.50 | 6.00 | 7 goals; 2 tools; phase hidden. |
| [Data Empty branch](board-data_empty_branch.jpg) | board_base | 240.00 | 17.00 | 2 goals; 2 tools; phase hidden. |
| [Data Empty doorway - zero deliveries](board-data_empty_doorway_-_zero_deliveries.jpg) | board_base | 240.00 | 17.00 | 2 goals; 2 tools; phase hidden. |
| [Data Empty start - no snapshot](board-data_empty_start_-_no_snapshot.jpg) | board_base | N/A | N/A | HUD hidden by existing loading/unavailable state. |
| [Data Odd branch count](board-data_odd_branch_count.jpg) | board_base | 240.00 | 17.00 | 4 goals; 2 tools; phase hidden. |
| [Data Typical](board-data_typical.jpg) | board_base | 240.00 | 17.00 | 2 goals; 2 tools; phase hidden. |
| [Data Zero species - unavailable](board-data_zero_species_-_unavailable.jpg) | board_base | N/A | N/A | HUD hidden by existing loading/unavailable state. |
| [Environment Missing required data](board-environment_missing_required_data.jpg) | board_base | N/A | N/A | HUD hidden by existing loading/unavailable state. |
| [Environment Offline local data available](board-environment_offline_local_data_available.jpg) | board_base | 240.00 | 17.00 | 2 goals; 2 tools; phase hidden. |
| [Hints Failed request](board-hints_failed_request.jpg) | board_stuck_hint | 361.30 | 19.00 | 2 goals; 2 tools; phase hidden. Hint follows available target. |
| [Hints No solution](board-hints_no_solution.jpg) | board_stuck_hint | 361.30 | 19.00 | 2 goals; 2 tools; phase hidden. Hint follows available target. |
| [Hints Pending](board-hints_pending.jpg) | board_stuck_hint | 361.30 | 19.00 | 2 goals; 2 tools; phase hidden. Hint follows available target. |
| [Hints Stuck bubble dismissed](board-hints_stuck_bubble_dismissed.jpg) | board_stuck_hint | 240.00 | 19.00 | 2 goals; 2 tools; phase hidden. |
| [Hints Stuck no legal move](board-hints_stuck_no_legal_move.jpg) | board_stuck_hint | 240.00 | 19.00 | 2 goals; 2 tools; phase hidden. Hint follows available target. |
| [Hints Successful transfer hint](board-hints_successful_transfer_hint.jpg) | board_base | 240.00 | 17.00 | 2 goals; 2 tools; phase hidden. |
| [Hints Swivel hint](board-hints_swivel_hint.jpg) | board_base | 240.00 | 17.00 | 2 goals; 2 tools; phase hidden. |
| [Hints Timeout unknown](board-hints_timeout_unknown.jpg) | board_stuck_hint | 361.30 | 19.00 | 2 goals; 2 tools; phase hidden. Hint follows available target. |
| [Plush Base board](board-plush_base_board.jpg) | board_base | 240.00 | 17.00 | 3 goals; 2 tools; phase shown. |
| [Plush Disabled tools keep size](board-plush_disabled_tools_keep_size.jpg) | board_base | 240.00 | 17.00 | 3 goals; 2 tools; phase shown. Disabled discs keep size. |
| [Plush Many branches](board-plush_many_branches.jpg) | board_many_branches | 239.00 | 9.81 | 3 goals; 2 tools; phase shown. |
| [Plush Maximum counters](board-plush_maximum_counters.jpg) | board_base | 240.00 | 17.00 | 3 goals; 2 tools; phase shown. 99999 coins and 999 moves fit. |
| [Plush Nocturnal sleep explanation](board-plush_nocturnal_sleep_explanation.jpg) | board_sleeping_bubble | 240.00 | 19.00 | 3 goals; 2 tools; phase shown. Sleep bubble belongs to BoardView. |
| [Plush Sleeping tap reaction](board-plush_sleeping_tap_reaction.jpg) | board_sleeping_bubble | 240.00 | 19.00 | 3 goals; 2 tools; phase shown. Sleep bubble belongs to BoardView. |
| [Plush Stuck recommends branch](board-plush_stuck_recommends_branch.jpg) | board_stuck_hint | 361.30 | 19.00 | 3 goals; 2 tools; phase shown. Hint follows available target. |
| [Plush Stuck recommends hint](board-plush_stuck_recommends_hint.jpg) | board_stuck_hint | 240.00 | 19.00 | 3 goals; 2 tools; phase shown. Hint follows available target. |
| [Plush Stuck recommends undo](board-plush_stuck_recommends_undo.jpg) | board_stuck_hint | 19.00 | 19.00 | 3 goals; 3 tools; phase shown. Hint follows available target. |
| [First board](real_game-first_board.jpg) | board_base | 70.25 | 17.00 | 2 goals; 0 tools; phase hidden. Bar hidden. |

## Existing art substitutions and remaining work

The existing Art sprites were inventoried before implementation. Reused UI pieces include stat_pill, coin, icon_footprint, the divider portion of move_pill_frame, star_full, dial_sun/dial_moon, goal_house_single, goal_house_door_open, goal_rope_single, badge_ring_gold, gift_coin_*, badge_check, btn_wood, icon_gear, icon_undo, icon_bulb, icon_branch_plus, badge_red, glow_soft and icon_next_arrow. Existing Art/glyphs numbers and the already-wired Resources/Birds front animation sprites supply text and moving portraits. No artwork or fonts were generated, modified or added.

- The exact wide mockup house with its small round opening is missing. The existing house is cropped and fitted to the measured outer rectangle; its opening and roof construction still differ. The exact twisted rope texture also differs despite corrected scale and clipping.
- The small pink wrapped-gift badge is missing. Existing species ability gift-coin badges are kept at the measured badge size, with their existing meaning.
- The mockup's rimmed orange disc, exact cream capsule/bar texture, brown divider, sun, outlined bulb and branch-plus drawing differ from the available kit. Existing orange discs plus gold rings, cream stat_pill, cropped painted divider and existing icons are the closest available pieces at the measured bounds. The soft halo is composed from the existing glow_soft; the exact white/gold ring glow and separate pointer shadow are missing.
- Forest/background, branch positions, board bird sizes, the sleeping bubble and zzz are outside this worker's ownership. Their large visible differences are left intact. Matching those portions requires their owners; no full-screen acceptance is claimed here.
- TutorialHand.cs and BoardFakeData tutorial fixtures still use static HudView.ToolBounds centers for a three-slot bar. A coordinator change should use the live Hud.BoosterPoint for a compacted toolbar. BoardSessionAdapter.cs still uses the old GameLayout.PhaseDial.center for moonbeam gift-flight targeting; it should use the measured sun counter. These unowned integration points were not edited.
- No separate stock-badge rectangles exist in these four JSONs (the many-branches reference explicitly has no badges). Existing 56 px red stock badges and painted counts remain data-driven. Their presence is an intentional state difference, not a claim that the reference has those badges.

User/integrated visual acceptance remains pending, particularly the missing-art substitutions and unowned board surfaces. Motion and sound are preserved in code but are not established by these silent instant captures.
