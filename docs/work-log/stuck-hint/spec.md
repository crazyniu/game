# Gentle stuck hint (OMP worker)

Design (docs/GAME_DESIGN.md section 1): there is no lose state; if no legal move exists, show a gentle bubble pointing at Undo and the boosters. Core already has `GameSession.HasLegalMove(BoardState)`; nothing in `Scripts/Game` uses it for this.

## Owned files
New: `Scripts/Game/StuckHint.cs`, `Tests/PlayMode/StuckHintTests.cs`. Edit: `GameRoot.cs` only a single small hook (one call after a move/undo/booster has fully resolved and input is unlocked). Everything else read-only.

## Behaviour (exact)
Trigger: after a move, swivel, undo or booster has fully resolved (cascades finished, input unlocked, no modal, level not won, entrance not playing) and `HasLegalMove` is false. Trigger whenever there is no legal move, regardless of boosters (the bubble also points at them).
Delay 0.8 s after input unlocks, then show the bubble; cancel the pending show if any input or state change happens first.
Bubble: cream rounded panel 420x170 with a leaf sprig top-left (use existing panel/leaf sprites), a small tail pointing down at the Undo button. Content icons only, no text: the undo icon in a purple disc 72 px on the left, and the three booster icons (hint, branch, level booster if unlocked) 56 px on the right, 24 px gaps. Panel and tail are centred above the bottom bar, never overlapping the tools, kept inside the safe area.
Animation: panel scale 0.9 to 1.0 in 0.2 s (ease-out-back) when it appears; the Undo button then pulses (scale 1.0 to 1.10 to 1.0, 0.8 s period, ease-in-out) while the bubble is visible; the bubble floats 6 px up and down with a 2 s period. Any tap anywhere, any undo or any state change hides the bubble in 0.12 s (scale 1.0 to 0.9 with fade). It reappears only after the next state change that again has no legal move. Honour `GameMotion.Instant` (bubble shows without motion). Never block input; the bubble itself is non-interactive.
Undo is always possible (free, unlimited) so the player can always leave a dead end; the bubble never appears at the very first board state of a level (authored levels are solvable).

## Evidence and tests
Build a dead-end board using the Core fixtures (see existing test boards under `Tests/`), prove with PlayMode: bubble not shown while moves exist; shown after 0.8 s when none exist; hidden by undo; hidden by tap; not shown on a won level; not shown during the entrance or a modal; never intercepts clicks. Contact sheet 0.1 s for 0-3 s from the moment input unlocks, in `docs/work-log/stuck-hint/evidence/`, plus a full-screen screenshot beside the layout. Run the naming guard and `flock /tmp/unity.lock bash scripts/run_tests.sh`; all existing tests keep passing with unchanged counts. Report in `docs/work-log/stuck-hint/report.md`, push the branch, final SHA in your last message.
