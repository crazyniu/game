# Board bird spacing report

Board bird scale is now 1.10 instead of 0.88, increasing width and height by 25 percent. The first-seat distance is 98 at density 1; the 24-pixel outward offset remains on both sides.

Branch perches now fill y=660..1640, with intervals derived from the authored row/side slots. This removes the snow board's large gap below the goals and uses more of the area above the tools. Branch textures translate by the same vertical offset as their measured perch surface. Birds, flight/hop destinations and branch hit areas use the shared perch positions.

Existing branches and birds use the same 0.45-second reflow when an added branch changes row spacing. HUD positions, source artwork, rules, levels and save data are unchanged.

The user subsequently requested testing, then rejected the concurrent HUD restyle. The cream move/star and phase counters were restored, and the level label below Settings was removed. Bird scale and perch layout were retained.

## Authorized checks

- `GODOT=/Applications/Godot.app/Contents/MacOS/Godot bash scripts/run_tests.sh`: passed all 5 naming self-tests, 44-source naming inspection, script/40-level smoke checks and all 40 solver/session replays. Core checks reported 0 failures in 21002 ms. Log: `build/bird-spacing/project-tests.log`.
- After the HUD rollback, `python3 ci/check_current_game_names.py` and `/Applications/Godot.app/Contents/MacOS/Godot --headless --path PerchSort -- --smoke` passed again. Log: `build/bird-spacing/hud-rollback-checks.log`.
- Native Godot 4.7.2 GL Compatibility captures of levels 1, 12, 25, 35 and 40 were inspected at 1080x1920. The restored counters are present, the level label is absent, and the larger birds and relocated branches remain visible. Captures: `build/bird-spacing/level_<level>.png`.
- In the native level-40 window, clicked Sapling and observed the added branch and redistributed rows; undo restored the original rows and stock. Selected the two awake Ember birds, moved them onto an empty branch, and observed the move counter change from 0 to 1 and phase counter from 3 to 2. Undo restored the birds and both counters.
- The interaction run recorded 480 frames using `--frames=build/bird-spacing/interaction-frames --wait=120 --step=0.25 --motion` at a 540x960 window size. Frames 100 and 150 show the original and added-branch layouts. The native capture and interaction logs contain no script errors or warnings.

Native runs used `--audio-driver Dummy`, the `--capture` sandbox and the isolated fixture in `build/bird-spacing/profile.json`; they did not save test progress. No release build was produced. Entrance timing, audible playback and hint-search latency were not assessed. Final visual approval remains with the user.
