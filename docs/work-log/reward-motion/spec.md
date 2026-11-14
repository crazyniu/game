# Readable reward animation

Enhance draw, daily claim and level-win animation on integrated baseline 1466792, without changing payout, odds, inventory, UI artwork or callback semantics. Use position, bounded squash/scale and sequence timing rather than additional light effects.

- Draw: selected card visibly steps forward, slows down, holds the actual winner, then shows results. Keep the existing callback/duplicate guard. Shorten decorative roll to roughly 2–3 seconds including winner hold.
- Daily claim: a clearer card lift/stamp and larger existing reward sprites travelling to the balance. Results must still wait for the last arrival.
- Win: stars rise/settle in readable sequence, then the feather number counts and pulses, then booster items arrive. Navigation remains available throughout.
- General prize results: staggered rising entries with existing count-ups; no new glow or art.

Verify baseline and changed graphical sequences, exact payout counts, target accuracy, early exit, repeated presses and timing. Local-only changes; coordinator integrates and submits once.
