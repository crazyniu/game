# UI rebuild: report

Branch `ui-rebuild` (from `ui-redesign`). The user found the side trees fake and the UI messy, handed the whole UI
to the coordinator and approved a new direction on 2026-11-07: cartoon, simple and clean, nothing fancy.

## Root cause and approach

Every earlier round composed separately generated sprites (trunks, collars, branches, felt panels, ribbons) by
code; their light, softness and scale never matched, so the board looked pasted together. The rebuild uses one
source per kind of art:

- Scenes: one painting per world (`tools/scene/board_wN_raw.png`) plus the same painting with the branches removed
  (`board_wN_empty.png`), both by the local Codex. `tools/scene/cut_branches.py` detects each branch in the
  painting, cuts it into its own layer and measures its perch line and tip, written to
  `world/scene/board_wN_slots.json`. Worlds have 8, 10, 12 and 12 painted slots; a level uses a centred block of
  rows, leaving room for the sapling's extra row.
- UI shapes are drawn by code (`UI.shape`, `UI.button`, `UI.counter`, `UI.badge`): flat rounded fills, a darker
  bottom edge and one soft shadow, one size per role.
- Icons: one white family from two painted sheets (`tools/kit/icons_white*.png`, cut by `tools/kit/cut_kit.py`).
- Lettering: one painted rounded typeface (`tools/kit/letters_white.png`). Words are painted label sprites built
  by `tools/kit/build_words.py` (white with a warm outline and soft shadow, the user's choice "B"); numbers are
  the same glyphs composed at runtime with matching outlines.

Rejected along the way (kept only in Git history of the abandoned branches, now deleted): Codex-painted trunks with
separate collars, a Blender metaball renderer, Hyper3D trunk models rendered in a Godot 3D stage.

## Screens

- Board (`board_view.gd`): world painting and branch layers; seats spread over each painted branch; branches with
  fewer seats are the same painting narrowed from the edge; moves counter with a three-star meter, sun counter,
  settings, hanging house (new default house art), goal circles, tool row with stock badges, new sleep bubble.
- Popups (`popup.gd`, `popups.gd`): one panel, title word (Level N is word plus number), content groups, action
  row; all nine popups rebuilt.
- Map, shop, aviary: rebuilt by three Claude workers (Opus 5.5, high effort) in separate worktrees against the
  shared contract `spec.md`; reports `map.md`, `shop.md`, `aviary.md`; reviewed and merged by the coordinator.
- Shared fixes: outfits stay on birds through every idle action (`bird_view.gd`); species names come from
  `Aviary.display_name` only (Skye, Minty, Flurry, Inky, Rook); red close buttons.

## Removed

338 unused sprites (old labels, number glyphs, felt panels and buttons, trunks, branches, twigs, time-of-day
backgrounds, old house parts and tool icons), the measured mockup layouts (`layout/`, `game/layout.gd`,
`art/layout_metrics.json`) and their generator tools.

## Evidence

- `bash scripts/run_tests.sh`: naming guard 0 violations, smoke 0 failures, core 0 failures (all 40 levels solved
  and replayed).
- `bash scripts/screenshots.sh`: 42 states captured, failed=0, every state reviewed at full size.
- Timed: level entrance 3646 ms (autoplay, level 3); tree motion frames (wobble, sapling branch) in
  `build/motion`; map, shop and aviary frames in the workers' reports.

## Open

- The user has not reviewed the rebuilt screens on the Mac yet; after approval, captures become the new
  `art/approved_runtime` baseline.
- Repository Orca setup runs `pnpm install`, which does not apply to this project (fails harmlessly).
