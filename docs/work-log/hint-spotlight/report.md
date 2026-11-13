# Hint spotlight — report

## Changes

- `PerchSort/game/hint_spotlight.gdshader`: dim mask with two rounded holes and a pulsing rim.
- `PerchSort/game/board_view.gd`: `show_hint` builds the mask, dotted trail and travelling hand;
  `_hint_tap` (press and ring), `_hint_hole` (branch rect), `_hide_hint` frees all three.

## Evidence

- Frame sequence: `godot --path PerchSort -- --frames=<dir> --preview=board_view:hint_transfer --wait=4 --step=0.2`;
  contact sheet [evidence/hint_sequence.png](evidence/hint_sequence.png) shows tap with ring at the source, glide
  along the dots, tap at the target, fade, restart.
- Real press: `--preview=board_view:hint_timing_13` (Firefly pressed on level 13), frame
  [evidence/hint_level13.png](evidence/hint_level13.png).
- `bash scripts/run_tests.sh`: naming guard 0 violations, smoke 0 failures, core 0 failures.

## Boundaries

- The swivel variant was checked by code path only; no current preview state shows a swivel hint.
- Visual acceptance is the user's.
