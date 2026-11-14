# Skill cues (issue 246)

- Light step (Flurry): while `Board.light_step` is set and the board is at rest, every branch with a free landing seat
  (and no egg on its tip) shows a pulsing warm glow at that seat, between the branches and the birds, and Flurry's key
  under the house pulses. Both go when the step is used (the next move starts, the board is no longer at rest).
- Free move (Berry, Inky): while `Board.encore` is set and the board is at rest, the painted `gift_coin_encore` sits on the
  right end of the status capsule, clear of the move count; it goes with the next move.
- First pointer: once per profile (`keys.shown`), on the first board with bird keys, a hand taps the first key for two
  cycles after the entrance and with no popup open; any tap stops it. No words. Never while another hand is on the board.
- All cues are hidden mid-move so they follow the gift animation instead of racing it; `_finish_action` re-syncs them
  once the board is at rest.
