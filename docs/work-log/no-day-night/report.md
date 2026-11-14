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
- The day/night engine code, the night paintings, the lesson and its words were left in the tree by this
  change; they are removed under "Unreachable code removed" below.
- Saved best move counts on these levels were made under the old rule and stay.
- Not played by hand.

## Unreachable code removed (issue 225)

- Core: `Board.phase`, `phase_counter`, `day_length`, `moonbeam_left`, `is_nocturnal`, `nocturnal_day_sleep`,
  `Rules._toggle_phase`, `_emit_nocturnal_wakes`, the NIGHT_CALL and MOONBEAM branches and the phase part of
  `_rescue_deadlock`. `Board.Phase` and `Rules.Gift` keep their enum numbers; `Board.search_key` keeps its layout
  (the flag bits stay at 12 and 13, bits 0-11 are now always zero), so stored hints still match. `Levels` no longer
  reads `dayLength` (the key is still written as 0 in `levels.json`, which needs separate authority to change).
- Board view: night painting cross-fade, night life, phase pill and banner, the moon cue in the sleep bubble and the
  preview states `day_wN`, `night_wN`, `phase_change`, `day_break`. Ambient lost its `night` tint.
- Tutorial: the daynight lesson. Popups: `MECHANIC_ART` entry. Test mechanic lists no longer name it.
- Art: ten world night paintings (`board_wN_night_bg`, `board_w1_open`/`board_w10_open` included), `mechanic_day_night`,
  `pic_sun`, `pic_moon`, the two control sun/moon SVGs, and the lesson and phase words in both locales with their manifest entries.
- Checks: `scripts/run_tests.sh` (naming guard, smoke, core) 0 failures; `run_family_dialogs`, `run_level_heading`,
  `run_board_hud`, `run_gameplay_dialogs`, `run_stuck_rescue`, `run_visible_skills`, `run_heading_composites`,
  `run_map_aviary`, `run_menu_navigation` 0 failures; board and tutorial captures show no missing art.
  `run_home` fails on `NAV_SIZE` both here and on main, so it is unrelated to this change.
