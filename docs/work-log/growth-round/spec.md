# Growth round: shared contract for workers

The user accepted the coordinator's review (2026-11-08): better feel and layout, mechanic introductions, a fair
economy with per-level booster and coin gifts, a star chest, smoother level order, and a Chinese version. Read
`CLAUDE.md`, `docs/design/ui-system.md`, `docs/CURRENT_GAME.md` first. Hard constraints apply: words and numbers are
baked label sprites (never a runtime font), same-role buttons one size, disabled = dimmed, birds never static, no
version names, timed frame evidence for motion, never force-push or merge, no rule changes outside your task.

## Current kit (use it, do not restyle it)
- Painted matte kit in `art/kit` drawn by `UI.shape`, `UI.button`, `UI.pill(parent, name, rect, kind, label_id,
  on_press, icon, cap)`, `UI.badge`, `UI.kit_piece`, `UI.title_ribbon`; painted pictures via `UI.icon` (`UI.PICTURES`).
- Words: catalog `tools/kit/locales/en.json` (`labels`), baked by
  `uv run --no-project --isolated --with pillow tools/kit/build_localized_labels.py en --labels-only <ids...>`
  (Pillow with RAQM is installed here; no DYLD variable), then `godot --headless --path PerchSort --import`.
  `UI.label(parent, id, rect, cap, color)`. Documents: `tools/kit/build_documents.py`.
- Only birds on the board make bird sounds; menus play only music.
- Popups: `Modal.open(host, title_id, heights, close, action_h, above, instant, quick, title_number)` in `game/popup.gd`.

## Ownership (non-overlapping)
| Task | Files |
| --- | --- |
| feel-layout | `game/board_view.gd`, `game/bird_view.gd`, `game/map_view.gd`, `game/sound.gd` (haptics helper), a `vibration` switch row in `popups.settings` only |
| mechanics-intro | `game/tutorial.gd` (new mechanic lessons), one hook in `game/main.gd` (before a level whose mechanic is new) |
| chinese | new `tools/kit/locales/zh.json`, `zh_docs.json`, fonts under `tools/kit/fonts/`, `tools/kit/build_*` locale support, `game/text_art.gd` / locale selection, a language row in `popups.settings` |
| coordinator | `meta/`, `core/levels` order, `levels/`, `game/home_view.gd`, `game/popups.gd` (gift and chest popups), `game/main.gd` |

Appending to `tools/kit/locales/en.json`: add new ids at the END of `labels` only (others append too; the coordinator
merges). Keep `meta/profile.gd` field names; you may not add save fields unless your task says so.

## Verify and deliver
`bash scripts/run_tests.sh` (naming 0, smoke 0, core 0), captures of every state you touch (`bash scripts/capture.sh`,
add preview states to `scripts/screenshots.sh`), frame sequences for motion (`--frames=<dir> --wait=S --step=0.1`
under xvfb). Look at all of them. Commit on your branch, push, write `docs/work-log/growth-round/<task>-report.md`
(exact commands, results, evidence paths, honest open issues) and finish with `worker_done`.
