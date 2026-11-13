# Stuck rescue — report

## Changes

- `core/session.gd`: `rewind(steps)`, `history_size()`, `history_state(back)`, `free_rescue_used` (reset by restart).
- `game/board_view.gd`: `rescue_requested`; `_watch_stuck` offers the rescue once per attempt, then the pointer;
  `rescue()` finds the step count on a worker thread (positions on a found winning route are known, others are
  searched newest first with 60000 states / 1500 ms, the opening position is the fallback); `_winnable` cache;
  undo and rescue share `_after_rewind()`.
- `game/stuck_offer.gd` (new): the card, previews `normal` and `rescue`.
- `game/undo_offer.gd`: `open_item()` for one more of any tool; preview `tool`.
- `game/main.gd`: `_offer_tool` replaces the jump to the shop; the rescue offer is wired to the board.
- Painted title `lettering_stuck` in both locale packs.
- `tests/run_stuck_rescue.gd` (new).

## Evidence

- `bash scripts/run_tests.sh`: naming guard 0 violations, smoke 0 failures, core 0 failures.
- `godot --headless --path PerchSort --audio-driver Dummy -s res://tests/run_stuck_rescue.gd`: 22 checks, 0 failures;
  log `RESCUE level=41 steps=2 of=2`.
- `run_undo_allowance.gd` 19 checks and `run_purchase_dialogs.gd` 100 checks, 0 failures.
- Stills: [stuck_zh](evidence/stuck_zh.png), [stuck_en](evidence/stuck_en.png), [tool_offer_zh](evidence/tool_offer_zh.png).
- Timed sequence, 0.5 s per frame, level 41 after a losing opening move: [rescue_flow](evidence/rescue_flow.png)
  (`--preview=stuck_offer:rescue --frames=<dir> --wait=6 --step=0.5`).

## Boundaries

- A dead end is recognised only after the background search has exhausted the position; boards over its budget
  are not flagged, so no card appears there.
- When no earlier position can be proven winnable within the budget, the rescue returns to the opening position.
- The card does not say "free" in words; its action shows the undo arrow only.
- Not played on a device; the user has not accepted the two cards.
- The ad is still the stand-in of `AdService`.
