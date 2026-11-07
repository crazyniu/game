# Polish round: shared contract for workers

The user played the merged UI on the Mac (PR #168 on top of the UI rebuild) and listed problems. Each worker takes
one task file in this folder. Read `CLAUDE.md`, `docs/design/ui-system.md` and `docs/CURRENT_GAME.md` first. Hard
constraints from CLAUDE.md apply: words and numbers are painted sprites (never a runtime font), same-role buttons
keep one size, disabled = dimmed, birds never static, no version names in files, animation acceptance needs timed
frame evidence, never force-push, never merge.

## Facts about the current code

- Text: whole localized phrases are label textures. Catalog `tools/kit/locales/en.json` (ids such as `shop.buy`),
  baked by `tools/kit/build_localized_labels.py` into `PerchSort/art/locale/en/`. Use `UI.label(parent, id, rect,
  cap, color)` and `UI.label_width(id, cap)`; numbers use `UI.number`. To add or change text: edit `en.json`, then run
  on this machine (Pillow with RAQM and uv are installed, no DYLD variable needed)
  `uv run --no-project --isolated --with pillow tools/kit/build_localized_labels.py en --labels-only <id> [<id> ...]`
  and re-import (`godot --headless --path PerchSort --import`). Read `tools/kit/fonts/README.md` for details.
- Icons are white sprites (`UI.icon`) tinted by the kit, or full-colour illustrations in `PerchSort/art`.
- Scenes: `world/scene/board_wN_bg.png` plus cut branch layers (`tools/scene/cut_branches.py`).
- Rules live in `core/` and `meta/`; do not change rules, levels, solver results or save field names unless your task
  says so. `bash scripts/run_tests.sh` must stay green (naming 0, smoke 0, core 0); the smoke test now fails on any
  script that does not parse.
- Captures: `bash scripts/capture.sh <abs.png> --preview=<script>:<state>` and `bash scripts/screenshots.sh [filter]`;
  frame sequences: `xvfb-run -a -s "-screen 0 1280x2000x24" godot --path PerchSort --audio-driver Dummy
  --rendering-driver opengl3 --resolution 1080x1920 -- --frames=<abs dir> --wait=S --step=0.1 --preview=<script>:<state>`.
  Add preview states for anything new. Look at every capture and frame at full size; stills do not prove animation.
- Art you cannot paint yourself: ask the coordinator with `orca orchestration ask` and carry on with the rest.

## Ownership (non-overlapping)

| Task | Files |
| --- | --- |
| board | `game/board_view.gd` (HUD, day/night visuals, hint), `core/solver.gd` only for hint speed, new `world/scene/*night*` use |
| tutorial | new `game/tutorial.gd`, `game/map_view.gd` (a help button), small hooks in `game/main.gd`, `meta/profile.gd` only if a flag field is unavoidable (keep existing field names) |
| shop-aviary | `game/shop_view.gd`, `game/aviary_view.gd`, small hooks in `game/main.gd`, `tools/kit/locales/en.json` (append) |
| birds | `game/bird_view.gd` and bird art helpers under `tools/` |

Only touch other files for a one-line hook and say so in your report. Append to `en.json` instead of reordering it.

## Deliver

Commit on your branch with an English message, push it, write `docs/work-log/polish-round/<task>-report.md`
(commands, results, capture and frame paths, open issues, honestly) and finish with `worker_done`. Never merge.
