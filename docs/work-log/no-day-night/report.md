# Day and night removed — report

User decision (2026-11-14): the day/night switch has no real meaning for the player; remove it.

## Why

- 21 of the 100 levels had it; the phase flipped every 2 to 4 moves, too fast to plan around.
- Nocturnal birds (Inky, Hoot) could not move by day, so the player made filler moves to wait for night.
- The same two birds were ordinary in 36 other levels, with homecoming gifts that did nothing there.

## Changes

- `levels/levels.json`: `dayLength` is 0 in every level (was set in levels 10, 48, 53, 60, 63, 68, 70, 71, 73, 76,
  77, 79, 81, 82, 83, 87, 90, 91, 97, 98, 100). Their stored solutions were re-solved (`levels/hints.json`) and
  their pars follow the new routes, which are shorter because the waiting moves are gone.
- `core/rules.gd`: Inky's homecoming gift is now Encore (one free move), Hoot's is the Wake-up song. Both act in
  every level. `game/popups.gd`, `game/board_view.gd`: their badges. Their painted captions reuse Berry's and
  Sunny's caption art (same words); `tools/island/word_catalog.json` records the new texts.
- Level 10 was the day/night lesson: its four Hoots sat complete on one branch, asleep by day. The tip Hoot is now
  an ordinary sleeper, like the other flocks of that level. Level 98 got a losing move in its first three steps
  from the change and was repaired with `tools/levels/repair_early_traps.gd`.

## Pars of the 21 levels

| Level | 10 | 48 | 53 | 60 | 63 | 68 | 70 | 71 | 73 | 76 | 77 | 79 | 81 | 82 | 83 | 87 | 90 | 91 | 97 | 98 | 100 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| before | 3 | 12 | 12 | 13 | 21 | 23 | 22 | 27 | 17 | 25 | 29 | 31 | 25 | 32 | 25 | 26 | 33 | 30 | 29 | 31 | 33 |
| after | 2 | 8 | 7 | 11 | 22 | 20 | 19 | 25 | 13 | 22 | 25 | 23 | 24 | 26 | 20 | 23 | 29 | 25 | 28 | 25 | 28 |

## Evidence

- `bash scripts/run_tests.sh`: naming guard 0 violations, smoke 0 failures, core 0 failures (all 100 levels
  replay their stored solutions).
- `run_visible_skills` 22, `run_board_hud` 198, `run_gameplay_dialogs` 72, `run_family_dialogs` 1330,
  `run_stuck_rescue` 22 checks: 0 failures.
- `tools/levels/measure_fragility.gd` on the 21 levels: none has a losing move within its first three steps
  ([fragility.csv](../gameplay-depth/evidence/fragility.csv)).

## Boundaries

- Levels 48 and 53 (worlds 5 and 6) are now 8 and 7 moves, short for their place; the level order was not changed.
- The day/night engine code, the night paintings, the lesson and its words are still in the tree but can no
  longer be reached; removing them is the next change.
- Saved best move counts on these levels were made under the old rule and stay.
- Not played by hand.
