# Stuck restart report

Changes: `game/stuck_offer.gd` (`open_restart`, shared `_card`, preview state `restart`), `game/board_view.gd`
(`restart_offered` signal, world gate in `_watch_stuck`, `_stuck_picture`), `game/main.gd` (both board openers connect
the card), `tests/run_stuck_rescue.gd` (world 1 vs world 2 gate, pointer, restart action), `scripts/screenshots.sh`.

`_watch_stuck` runs after a move and again after a background hint proves a dead end; a newer run drops an older tool
search. The card is therefore owed by a flag (`_restart_card_due`) and opened by whichever run finishes its search.

Verification:
- `xvfb-run -a godot --path PerchSort --audio-driver Dummy --rendering-driver opengl3 -s res://tests/run_stuck_rescue.gd`: 47 checks, 0 failures.
- same for `res://tests/run_single_card_actions.gd`: 458 checks, 0 failures.
- `bash scripts/run_tests.sh`: exit 0.
- `bash scripts/screenshots.sh stuck_offer`: captures `stuck_offer__normal.png` and `stuck_offer__restart.png`
  (local evidence only). No mockup exists for this card; it is compared with the approved free-rewind card.

Not checked: user approval of the card; a timed frame sequence of a real second-forest dead end.
