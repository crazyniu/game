# UI state capture tooling report

Implemented `PerchSort.Editor.UiStateCapture.Run` and `scripts/state_captures.sh` on `mac-ui-state-capture/work`, based on `origin/main` at `09b8ecf734b01dd01fabcf48f37a3261ee76afeb`. The assigned branch already existed at that exact commit, so branch creation reported an existing name and work continued on the clean assigned branch.

The tool enters Play mode in the canonical Game scene, initializes a fresh nonpersistent profile before `GameRoot.Start`, discovers the same providers as Tools > UI States, and captures after four actual frames with instant motion. It renders the composed camera frame, temporarily attaching root overlay canvases to a camera and restoring their settings afterward. It returns from preview and captures the normal map and first authored board. No runtime UI, art, persistence, scene, or project settings changes are included.

Audio is silenced before scene Awake and remains silenced throughout the run: listener volume zero, listener paused, all sources muted with listener bypass disabled. Every successful capture recorded ten muted sources and both listener guards. This is a silent capture run, not music or animation playback acceptance.

Run from any directory with `bash /path/to/worktree/scripts/state_captures.sh`, or add `-states "substring"`. Filtering matches screen or state names case-insensitively. The script holds `flock /tmp/unity.lock` itself; do not wrap it in a second lock. It requires Unity, Python, uv, and flock; uv supplies Pillow for the contact sheets. The script removes its previous generated PNGs, sheets, and index before each run.

Validation performed on the Mac:

- `bash -n scripts/state_captures.sh` and `git diff --cached --check`: passed.
- `python3 ci/check_current_game_names.py --self-test`: 14 passed. `python3 ci/check_current_game_names.py`: 262 tracked text files, zero violations. The wrapper ran these before each Unity launch.
- `bash scripts/state_captures.sh`: completed two silent full capture passes. The first exposed four provider exceptions and an unrelated Unity Search indexing exception. The final pass confirmed that background Editor diagnostics are reported separately from state failures.
- Final run: 432 selected states, 428 PNGs, four state exceptions, six contact sheets, process exit code **1** as required for incomplete captures. Every PNG is 1080x1920; the minimum settling interval is four frames.
- Visually inspected `contact_Map.jpg`, `contact_Shop.jpg`, and `contact_Real_game.jpg`. The map controls, shop cards and confirmation modals, and real first board are present. Re-inspected the Map and Shop sheets from the final pass.
- EditMode and PlayMode test suites: **NOT RUN**, as assigned. The capture itself runs in Play mode.

| Screen | Discovered | Captured |
| --- | ---: | ---: |
| Aviary | 22 | 22 |
| Board | 324 | 324 |
| Map | 11 | 11 |
| Popups | 53 | 49 |
| Shop | 20 | 20 |
| Real game | 2 | 2 |

The four failed Popups states are `Level card / Two stars`, `Level card / Full roster`, `Level card / Maximum moves`, and `Level card / Locked`. All throw `Missing production sprite: hoot_sleep_bare` from `GameArt.Get` through `PanelsView.LevelCard`. The existing asset is under `Resources/Art`, while `GameArt.IsBirdFrame` routes its `hoot_` prefix to `Resources/Birds`. This existing resource routing issue needs the runtime owner's correction; the capture tool records it and continues. No replacement art or runtime workaround was added.

Generated evidence remains local and uncommitted:

- `build/screens/states/index.txt`: all successful PNG paths, provider counts, full exception details, the separate Unity Search diagnostic, summary, and sheet paths.
- `build/screens/states/unity.log`: graphics-enabled batch startup, pre-Awake silence, Play mode initialization, capture progress, and failure exit.
- `build/screens/states/contact_<Screen>.jpg`: six sheets with 360x640 previews and state names below each frame.
- `build/ui-state-capture/verification.json`: PNG dimensions, provider counts, settling interval, and audio guard summary.
- `build/ui-state-capture/before.json`, `import.patch`, and `preservation.json`: original hashes, retained import diff, and restoration evidence. Restored 925 rewritten metadata files and removed 201 newly generated metadata files; all 1,923 protected baseline hashes match.

Current providers use camera canvases and nested modal canvases; the overlay conversion path is implemented but was not exercised by these provider states. These captures establish rendered-state coverage only. The missing four PNGs remain explicit failures for coordinator follow-up.
