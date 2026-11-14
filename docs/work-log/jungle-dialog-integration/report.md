# Integration verification

## Baseline and scope

Based on main 054d04a, integrating accepted work without overwriting main's current UI regression tests. Shared panel/leaf binaries and imports are byte-identical between purchase and level/result work and are retained once. No core rules or level data changes. Save recovery adds only the separately authorized runtime gate/status accessor, preserving original files and schema.

## Final integrated checks

- Standard scripts/run_tests.sh passes naming/painted-word guards, launcher recovery, script smoke, all 210 stored solutions, save recovery, real Exit preserving both damaged files, and bird animation regressions.
- Graphical checks: shared dialogs 381, world/rewards 113, guides 330, tool guides 56, HUD 216, purchases 118, purchase buttons 128, purchase Close 1676, auxiliary dialogs 10, menu navigation 107 and modal motion 228 all pass: 3363 assertions.
- Purchase Close: first integrated run had one failed right-edge double-click assertion for the English outfit dialog (1/1676). A separate full bilingual rerun passed 1676/1676 with unchanged production code and unchanged test timing. Diagnostic output was added for recurrence. The final animation-integrated bilingual rerun also passed 1676/1676 without changing the existing timing threshold; the initial isolated failure is retained here rather than hidden.
- 28 real 1080x1920 English/Chinese screenshots cover the shell families, typography, outside actions and title placement. First capture review identified outdated daily/win tool illustrations; these now reference the shop's exact six sprites, with matching-texture, repeated-quantity and timed entrance checks. All 28 screenshots were regenerated after animation integration and visually rechecked, including both languages and final shop-matched reward art.
- Godot clamps zero Control scale to 0.00001; reward entrance tests check this collapsed state then the existing near-unit breathing state, not exact mathematical zero/one.

## Boundaries and diagnostics

The animation patch is integrated: card entry is restrained, early dismissal recedes from the current scale, and full-screen modal pages fade without shrinking. The 228 timing assertions cover 46 canonical popup previews plus purchase, tutorial, mechanism and save-recovery entries. Dormant chapters, deliberately excluded from the redesign, still emits its pre-existing missing white action.replay painted-label error when the exhaustive animation preview test visits it; it has no production entry. No mobile hardware, real-money payment, production account or deployment test. The headless save-recovery run logs expected damaged-save errors plus a shader-compiler diagnostic and exit resource warning; GUI auxiliary tests report one resource retained at exit. These are recorded, not hidden by passing check counts. Initial editor import could not save global editor settings; subsequent execution uses isolated writable XDG paths.

Latest main was fetched again and remains 054d04a. No merge or deployment performed.

Final timed visual review: six families (win, daily prize result, tutorial, purchase, save recovery and daily reward page), 30 frames each / 180 real-rendered frames. Reviewed entrance-to-settled contact sheets; card controls remain visible, reward/character animation continues, tutorial content progresses, full-screen daily page stays scale-stable. Capture logs contain no script/runtime errors. Evidence is local under `build/integration/` and is not committed.

## Reproduction commands

- `GODOT=/path/to/Godot_v4.7.2-stable_linux.x86_64 bash scripts/run_tests.sh`
- With authenticated Xvfb and isolated writable XDG directories, run each of `run_jungle_dialogs`, `run_world_reward_dialogs`, `run_guide_dialogs`, `run_tool_guides`, `run_board_hud`, `run_purchase_dialogs`, `run_purchase_buttons`, `run_purchase_close`, `run_aux_dialogs`, `run_menu_navigation` using `godot --path PerchSort --audio-driver Dummy --rendering-driver opengl3 -s res://tests/<name>.gd`.
- `godot --path PerchSort --audio-driver Dummy --rendering-driver opengl3 res://tests/run_modal_motion.tscn`.
- Capture locale pairs through canonical `--preview=<script>:<state> --lang=en|zh --capture=<path>` at 1080x1920. Motion samples use `--frames=<path> --step=0.1 --wait=3`, 30 frames per family. Frame-save overhead means sample indices are not a frame-rate benchmark; timed assertions provide lifecycle timing evidence.
