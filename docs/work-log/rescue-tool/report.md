# Rescue tool pointer: report

Delivered by PR #262 (issue #243). This report was written afterwards from the PR record, because the worker could
not write it at the time.

## What changed

- A dead end after the free rescue is used or declined searches the visible tools on a worker thread, cheapest first,
  at 20000 states / 600 ms each. The Firefly is skipped.
- A booster counts as a rescue when the board with its effect applied is proven SOLVED (`Solver.rescue_booster`).
- Undo counts as a rescue when an earlier position of the attempt is in `_winnable`, or the previous position is
  proven SOLVED. The free rescue records the position it returns to.
- The glow and pointer go to that tool. Tapping its slot uses it, or opens the one-more offer when out of stock.
- When no tool rescues, the pointer stays on Undo and the log reads `RESCUE_TOOL ... tool=none`.
- A tap, move or HUD refresh cancels a running search.
- Fixed a `modulate on Nil` error in the stuck pointer's tween.

Files: `PerchSort/core/solver.gd`, `PerchSort/game/board_view.gd`, `PerchSort/tests/run_stuck_rescue.gd`.

## Verification at delivery

- `godot --headless --path PerchSort --audio-driver Dummy -s res://tests/run_stuck_rescue.gd`: 36 checks, 0 failures.
  Main-thread slice at most 0.47 ms; longest headless frame during a search 11.3 ms.
- `bash scripts/run_tests.sh`: naming 0 violations, smoke 0 failures, core 0 failures.
- `run_undo_allowance.gd` 19 checks, 0 failures; `run_purchase_dialogs.gd` 100 checks, 0 failures.
- Timed evidence was a 9 s frame sequence at 0.25 s steps (pointer lands on the Sapling at 2.75 s). It was not kept;
  re-record with `run_stuck_rescue.gd -- --frames=<dir> --wait=9 --step=0.25` under xvfb.

## Boundaries

- Undo is always the cheapest candidate, so on catalog levels it is normally the tool named. The booster case is
  shown only on a test board.
- The 600 ms-per-tool worst case was not exercised.
- Not played on a device. The user has not accepted the pointer.
