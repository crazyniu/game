# Level card tools: report

## Changes
- `PerchSort/game/popups.gd`: `level_card` takes optional `tools`, `stock_of`, `on_tool`; new private `_tool_row`;
  constants `LEVEL_TOOL*`; previews `level_card_tools_two`, `level_card_tools_four`.
- `PerchSort/game/main.gd`: `show_level_card` passes the fitting unlocked tools, the profile inventory and
  `_offer_card_tool` (new, beside it: the one-more offer, grant to profile inventory, refresh on grant and close).
- `PerchSort/tests/run_level_card_tools.gd`: new test (26 checks).

## Commands and results
- `godot --headless --path PerchSort --audio-driver Dummy -s res://tests/run_level_card_tools.gd`: 26 checks, 0 failures.
- `bash scripts/run_tests.sh`: exit 0 (naming guard, painted words 0 problems, smoke 0 failures, core 0 failures).
- `run_family_dialogs.gd`: 1330 checks, 0 failures. `run_visible_skills.gd`: 22 checks, 0 failures.
- Stills (en and zh) of no row, two tools, four tools with one empty:
  `docs/work-log/level-card-tools/evidence/level_card_{first_visit,tools_two,tools_four}_{en,zh}.png`
  via `bash scripts/capture.sh <png> --preview=popups:<state> --lang=<l>`.
- Frame sequence (0.25 s step): `evidence/frames/f000..f005.png` via `main.tscn -- --preview=popups:level_card_tools_four --frames=<dir> --wait=1.5 --step=0.25`.

## Boundaries
- Evidence images are not tracked. Not played on a device; the user accepts the UI.
- The buy/ad path is tested with the feather purchase only; the ad path reuses the board's `AdService.show_rewarded`.
- `run_family_dialogs.gd` was not edited (its level-card checks pass unchanged with the default empty row).
