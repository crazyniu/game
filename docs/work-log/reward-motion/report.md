# Readable reward animation report

Baseline: integrated `1466792`, Godot 4.7.2. This worker changed only reward-animation sections of `game/popups.gd`, plus dedicated tests/docs. No commit, push or asset replacement.

## Changes
- **Draw:** every visited prize now steps forward by 6% while other prizes soften to 72% opacity. The actual selected card lifts 22 px and settles through a bounded 14% pop; the final hold makes the outcome readable. Decorative laps decrease from 24–31 to 16–23, with a shorter slowdown and 0.55-second winner hold. The callback-selected target, payout-before-animation, busy guard and disabled Close/Spin during the roll are unchanged. Dismissing results restores every card's scale/opacity and the draw controls.
- **Daily claim:** today's card lifts 24 px with a 12% pop before its existing stamp lands. Existing flying reward sprites are 96 px rather than 72 px with a wider initial fan, making the source-to-balance path visible. The flight/stamp duration, live-balance reader and wait-for-last-arrival contract are preserved.
- **Win:** stars rise 72 px, rotate back 22 degrees and settle sequentially at 0.3-second spacing. Feather count-up follows and receives one 16% emphasis; booster drops follow afterward. Navigation remains immediately usable; it never waits for the decorations.
- **Reward result groups:** existing prizes rise 44 px as they scale in, at 0.16-second intervals rather than 0.22, retaining exact amount count-ups and existing effects. No new glow, rays or materials. Approved artwork and final geometry are unchanged.

## Confirmed close race
The daily claim coroutine previously checked only instance validity after its delay. A page is still valid during its 0.12-second exit, so closing 1.02 seconds into a 1.1-second no-feather claim wait could open results after dismissal. Production-constructor regression reproduced one failure before the fix. The existing guard now also rejects a queued/departing modal. Reward settlement remains before animation and is not undone.

## Verification
- `run_reward_motion.tscn`: all eight possible draw target indexes, repeated Spin presses, reset after result dismissal, forced navigation removal during roll, daily repeat press/current balance, close-tail race, sequential win stars, exact final amount and single navigation dispatch. Initial headless run: 108 checks, zero failures. Graphical X11/OpenGL run: 108 checks, zero failures; observed target result times 2047–2707 ms. Final graphical run adds early-win-exit availability/cancellation: 110 checks, zero failures.
- Real graphical daily flow: reward sprites observed, last arrival at 1261 ms, results at 1917 ms; 4 checks, zero failures. Results do not cover a still-flying reward.
- Existing world/reward regression: 113 checks, zero failures, including exact booster quantities, all seven prizes and single callbacks.
- Full `scripts/run_tests.sh`: naming self-tests/guard, painted words, smoke, 210 stored solutions and bird-animation regression passed. `git diff --check` passed.
- Before and after graphical captures: draw spin, daily claim and three-star win, 50 samples per sequence. Reviewed selection, winning pop/result, claimed-card lift/flight/stamp, star sequence and final reward values. Capturing frames slows execution; measured timing above comes from the non-capture graphical test, not the frame sheets.

Local evidence is outside the repository in `reward-motion-evidence/`; do not commit it. The capture launcher still emits the pre-existing ObjectDB/resource-at-exit warning (also present before these edits). It is not claimed fixed. No real-device frame-rate or external billing certification is claimed. Coordinator must integrate with the warm dialog shell and run the final unified checks before the single submission.
