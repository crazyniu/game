# Dead ends and resuming play — report

## Changes

- `game/board_view.gd`: `_dead_end()` (no legal move, or a cached `UNSOLVABLE` search result) drives
  `_watch_stuck()`; `_poll_hint()` re-checks when the proof arrives after the move settled; the background search
  computes a fallback move only on `BUDGET_EXCEEDED`. Preview state `dead_end` plays one real shuffle.
- `meta/profile.gd`: `last_level` saved as `lastLevel`. `meta/progress.gd`: `resume_level()`.
- `game/main.gd` records the level on every board open; Home Play, the chest's Play and the map focus use
  `resume_level()`.
- `tests/run_core.gd`: the dead-end position from the user's report keeps legal moves and is proven unsolvable;
  resume rules and the save round trip.

## Evidence

- `bash scripts/run_tests.sh`: naming guard OK, smoke 0 failures, core 0 failures.
- `godot --path PerchSort -- --capture=<png> --preview=board_view:dead_end --motion --wait=6` logs
  `DEAD_END moves=1 stuck=true after_ms=1698 hint_failure=1 hint_move=false`; capture
  [evidence/dead_end_pointer.png](evidence/dead_end_pointer.png) shows the pointer over Undo.

## Boundaries

- A dead end is recognised only once the background search has exhausted the reachable positions; very large
  positions that exceed the search budget are not flagged.
- Launching with `--level=N` still opens level N directly.
