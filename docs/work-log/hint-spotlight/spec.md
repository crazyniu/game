# Hint spotlight

The hint hand alone was hard to find on a busy board. A hint now:

- dims the whole board and HUD except two soft rounded holes around the move's source and target branches,
  each ringed by a slowly pulsing warm glow (`game/hint_spotlight.gdshader`); the mask fades in over 0.3 s and
  never takes input, so the player taps through it;
- shows the move with a larger hand: it taps the source birds, glides along a dotted arc to the landing seat,
  taps there and fades, then repeats (about 2.6 s per loop); each tap spreads a ring from the fingertip;
- for a swivel lights its branch and taps the turn-around button;
- disappears with the hint (any move, undo or booster); re-selecting the source keeps it without restarting.

Hint search, Firefly accounting and rules are unchanged.
