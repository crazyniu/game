# Island homepage report

## Delivered
- Replaced only the canonical `HomeView` presentation with the user-approved island composition.
- Retained six features: Daily Challenge, Lucky Draw, Map, Shop, Aviary and Chest. Play, Settings and the live coin counter remain separate.
- Preserved the existing navigation signals, next-level selection, reward availability, English/Chinese preference, profile data and menu music. No shared screen, Core, Meta, level, save, economy or audio code changed.
- Added independent, technically validated homepage art and six eight-frame bird sequences. Original shared forest assets remain untouched. The approved reference is retained byte-for-byte at `art/approved_ui/island_home.png`.
- Fixed the green parrot's tail/branch occlusion and seated both logo birds. Birds breathe continuously and perform drawn idle actions without mirroring.

## Verification
Godot `4.7.2.stable.official.ed1daf0bf`, Compatibility/OpenGL, Mesa llvmpipe, 1080 x 1920.

1. `bash scripts/run_tests.sh`: naming self-tests passed; 73 tracked source files, zero naming violations; smoke zero failures; all 100 core level replays, zero failures.
2. `godot --headless --path PerchSort -s res://tests/run_home.gd`: zero failures. This test loads the actual main scene with a fake profile, disables persistence before navigation and dispatches real viewport mouse input.
3. GUI equivalent under `xvfb-run -a -s '-screen 0 1280x2000x24'`, with `--resolution 1080x1920 --rendering-driver opengl3 --audio-driver Dummy` and `-- --home-evidence=<output>`: zero failures. All six entries plus Play and Settings opened their existing screens or popups. Repeated navigation, dynamic 99999 coins, localization, equal sibling dimensions and unchanged pressed sizes passed.
4. All six birds have eight distinct drawn frames. All eight poses and continuous breathing were observed for every bird. The existing menu audio stream played, stopped on mute and resumed on unmute. A test-only exit cleanup now stops music, eliminating the earlier audio-resource shutdown warning without changing production audio.
5. English and Chinese/high-coin captures generated through `scripts/capture.sh <output> --preview=home_view:home --lang=en` and `--preview=home_view:home_max --lang=zh`.
6. Timed production captures: `-- --capture=<unused-output> --frames=<output-dir> --wait=6 --step=0.25 --motion --preview=home_view:home_new`, 24 saved frames. The timer interval is 0.25 seconds; PNG encoding and software rendering add wall-clock overhead. The separate instrumented test logs actual capture timestamps.

The cloud command sandbox could not open a local X11 socket. The same repository commands were run in the authorized cloud desktop terminal with workspace-local Debian Xvfb. No repository capture script, machine security setting or other worktree was changed.

## Evidence
- [English homepage](evidence/home_english.jpg)
- [Chinese homepage with 99999 coins](evidence/home_chinese_coins.jpg)
- [All eight navigation outcomes](evidence/navigation_checks.jpg)
- [Real idle-frame sequence](evidence/idle_sequence.jpg)
- [Timestamped homepage checks and audio state checks](evidence/home_checks.log)
- [Repository suite](evidence/test_suite.log)
- `build/island-home/`: full-resolution PNGs, 24-frame sequence and logs, intentionally ignored by Git.

## Acceptance boundary
Technical asset validation and runtime checks passed. Final visual acceptance of the running homepage remains with the user. The AI-generated independent assets follow the approved composition but are not pixel-identical source layers; the manifest records their differences and provenance. No phone/device test, subjective audio listening test, merge, release or deployment was performed. Other pages are unchanged. Keep the pull request a draft until the user finishes reviewing the running homepage.
