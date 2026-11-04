# Stuck hint report

## Implementation

- `GameRoot.Refresh` observes the session through one hook. `StuckHint` waits until the committed action has settled and the input lock and modal have cleared, then checks `GameSession.HasLegalMove` before scheduling the hint.
- The non-interactive panel uses the existing panel, leaf and icon sprites. It animates in, floats, pulses Undo, and dismisses on pointer input or a subsequent state change. Instant motion disables the visual animation.
- The fixture leaves sleeping birds with no legal transfer after a real Core move; Undo restores its previous state.

## Verification

- `python3 ci/check_current_game_names.py --self-test`: 8 passed.
- `python3 ci/check_current_game_names.py`: 0 violations.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`: final run: EditMode 312/312; PlayMode 57/57; no skips. The PlayMode total includes four new hint cases; retained tests were not removed.
- An earlier full run had one failure in `T54AMusicTests.FocusAndApplicationPauseFreezePlayheadsUntilBothResume`: expected 1.13065755 ± 0.02, observed 1.15199542. The subsequent full runs passed; the audio test was not changed.
- `flock /tmp/unity.lock bash scripts/screenshots.sh`: completed; stable screenshots in ignored `build/screens/game/`.
- The graphical PlayMode capture ran `StuckHintTests.HintAppearsAndFloatsAfterUnlock` with `STUCK_HINT_CAPTURE_DIR` set under `flock /tmp/unity.lock`: 1/1 passed. [Contact sheet](evidence/contact-sheet.png), [full-screen capture](evidence/full-screen.png), and [scheduled/actual timings](evidence/timing.csv) cover 0–3 seconds after the fixture move unlocks input.
- The hint tests cover a legal starting board, the 0.8-second delay, a dead end, Undo, modal cancellation, a won board, entrance, non-intercepting raycasts and timed visibility. They do not simulate a physical tap; tap-to-dismiss still requires interactive device verification.

Unity regenerated scene, bird importer metadata and Python bytecode during verification; those generated changes were restored to their clean pre-run state and are not part of the branch.
