# Bird keys under the house — report

User request (2026-11-14): put a row of birds under the birdhouse again and show a bird's skill when it is tapped,
because few players discover a long press.

## Changes

- `game/board_view.gd`: `_build_skill_row()` / `_refresh_skill_row()`: one key per kind of bird in the level
  (face, homecoming badge at the corner, a check once the flock is home), centred `KEY_DROP` 14 px under the
  house; 88 px keys, 74 px for nine or more kinds. A tap emits `bird_info_requested` (the bird card of
  [visible skills](../visible-skills/report.md)). `PERCH_TOP` 520 to 600 so the top birds stay clear of the row.
- The hold on a bird still opens the same card.
- `tests/run_visible_skills.gd`: row checks (count, centring, clearance to the house and to the highest birds,
  badge, tap, check after a homecoming).

## Evidence

- `bash scripts/run_tests.sh`: 0 failures; `run_visible_skills.gd` 22 checks, `run_board_hud.gd` 198,
  `run_stuck_rescue.gd` 22, `run_gameplay_dialogs.gd` 72: 0 failures.
- Stills: [row_world_10](evidence/row_world_10.png) (nine kinds), [row_level_12](evidence/row_level_12.png).

## Boundaries

- Every board moved down by up to 80 px at the top; the fullest boards keep the 150 px perch step. This changes
  the approved board captures and needs the user's eye.
- The check appears with a pulse; the keys have no other motion.
- Not tried on a device.
