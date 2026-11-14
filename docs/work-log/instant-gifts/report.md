# Instant homecoming gifts: report

## Verification (Godot 4.7.2, this machine)

- `bash scripts/run_tests.sh`: exit 0. Naming guard 0 violations, smoke 0 failures, core 0 failures (all 210 stored
  routes replay with matching keys), save recovery UI 0, bird animation 0, run_goal 94 checks 0 failures.
- `tests/run_visible_skills.gd`: 41 checks, 0 failures. New checks: snow homing adds one undo and the badge targets
  the undo slot; undo then homing again, and restart then homing again, pay nothing; berry and ink homing moves are
  refunded at once and the badge targets the move counter; the next move counts again.
- `tests/run_family_dialogs.gd` (xvfb, OpenGL): 1330 checks, 0 failures (bird cards in both languages).
- Re-key run: levels re-solved [169, 179], par raised for none.

## Timed evidence

Frame sequences at 0.1 s: `main.tscn -- --preview=board_view:undo_gift --frames=<dir> --wait=6 --step=0.1` and the
same for `move_refund`. Checked: the badge drops below the house at twice a goal key, with a light ring and sparkles,
hovers about 0.3 s, flies on an arc (sideways toward the counter at the top), lands with flash, sparkles, squash and a
"+1"; the undo count goes 3 -> 4; the refund counter shows 1 and returns to 2 on landing. The reward takes about
1.5 s and holds input only while it plays. Evidence images are local, not committed.

## Boundaries

No device play, no audio check by ear (sounds reuse `collect` and `star_3`), and the owner has not accepted the
animation or the new captions yet.
