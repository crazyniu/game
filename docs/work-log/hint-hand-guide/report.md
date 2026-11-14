# Hint hand guide: report

- `game/board_view.gd`: hand hint with dotted arc and tap ripple restored; `_guide_to_slot` taps a hand; firefly
  helpers removed; hole height 195 px, clamped inside the screen; idle nudge and keys pointer wait while a hint is
  shown, and `show_hint` clears both.
- `art/hint_firefly_*` removed.

## Verification

- `bash scripts/run_tests.sh`: exit 0.
- GUI: stuck rescue 47/0, tool guides 56/0, visible skills 42/0.
- Frames (local, `evidence/`): `hint_transfer`, `wake_teach_move`, `stuck_guide` at 0.5 s steps over 8 s. One hand
  only; the landing box on the empty right branch stays on screen; boxes hug the birds.

## Boundaries

Not seen on a device. Not yet accepted by the owner.
