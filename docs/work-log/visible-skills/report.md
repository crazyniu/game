# Visible skills — report

## Changes

- `game/popups.gd`: `goal_info(host, species, world)` with the trigger row (`_trigger_row`); level-card rings get
  a hit area `Bird_<species>` that opens the card.
- `game/board_view.gd`: `bird_info_requested`, `bird_at()`, hold detection in `_gui_input` (`HOLD_SECONDS` 0.45,
  `HOLD_SLOP` 24 px); `_gift_targets()` and the badge flight in `_gift()` (0.4 s flight, 0.05 s stagger, at most
  8 targets); preview states `gift_reach` and `hold_bird`.
- `game/main.gd`: the board's request opens the card (catalog and lab levels).
- `tests/run_visible_skills.gd` (new).

## Evidence

- `bash scripts/run_tests.sh`: naming guard, smoke, core, dive: 0 failures.
- `run_visible_skills.gd` 15 checks, `run_family_dialogs.gd` 1330, `run_gameplay_dialogs.gd` 72,
  `run_stuck_rescue.gd` 22: 0 failures.
- Stills: [bird_card_zh](evidence/bird_card_zh.png), [bird_card_en](evidence/bird_card_en.png),
  [level_card_zh](evidence/level_card_zh.png), [hold_bird_zh](evidence/hold_bird_zh.png) (the card opened by a
  hold; game dialogs cover the board with their own backdrop).
- Timed sequence, 0.2 s per frame: a sunny flock goes home, its badge flies to the two sleepers, they wake:
  [gift_reach_flow](evidence/gift_reach_flow.png) (`--preview=board_view:gift_reach --frames=<dir> --wait=6 --step=0.2`).

## Boundaries

- Nothing on the board tells the player that birds can be held; the level-card rings are the visible way in.
- Light step (Flurry) and moonbeam (Hoot) change no single object, so their badge still only rises and fades.
- Awaiting the badge flight adds about half a second to a homecoming that has an effect.
- The Dive talent of the lab levels is not on the card (the card describes the homecoming gift).
- Not tried on a device; hold timing and the flight speed are first values.
