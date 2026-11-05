# Flight frame wiring report

## Delivery

Branch: `mac-flight-wiring/work`.

Implemented the assigned drawn-flight selection in `PerchSort/Assets/Scripts/Game/BirdView.cs`. Added `PerchSort/Assets/Tests/PlayMode/FlightFramesTests.cs` and its Unity metadata. No artwork, rules, saves, levels, UI layout, or unowned implementation files were changed. `BoardView.cs` is intentionally unchanged: transfers, chick-follow flights, crow departures, house flights, and entrance flights already converge on `BirdView.FlyCore`.

Full-suite green acceptance is **not achieved**: two retained PlayMode tests fail outside the assigned ownership. All seven new flight tests pass, and both rendered acceptance scenarios pass. Details and specification conflicts are recorded below rather than hidden by modifying unrelated tests.

## Implementation

- Cache the six directional phase sequences per species and `GameArt` lifetime, using the same weak-key cache ownership pattern as the existing idle sequences.
- Probe the first takeoff, flap, and landing frame in each direction. Missing phase/frame data selects the existing `fly_up` / `fly_down` behavior for the species; no new transform is applied for fallback.
- Select the direction once from the initial target and start positions. Equal x selects right-facing art. No sprite mirroring or negative scale is introduced.
- Select takeoff frames during `min(0.18, 0.2 * duration)` and landing frames during the final `min(0.22, 0.25 * duration)` seconds. Advance the middle wing loop at 14 fps, relative to the start of the middle phase.
- Use elapsed game seconds from flight launch, not eased path progress. Entrance still receives its existing duration and `Ease.OutCubic`; all path, arc, tilt, trail, doorway masking, and homeward shrink calculations remain unchanged.
- Flights below 0.4 seconds omit the wing loop. After the three initial takeoff drawings, hold the third takeoff drawing until the specified final landing window. This retains both specified phase windows without inventing a new phase or timing split.
- Retain the last landing drawing through touchdown squash, then return to idle. `GameMotion.Instant` does not assign flight art. Existing cancellation resets the idle frame and cancels pending callbacks.

## Specification conflicts and ownership limits

1. **Touchdown timing conflicts with duration preservation.** The supplied code uses a 0.12 s touchdown squash, not the spec's stated existing 0.10 s. Implemented the explicit 0.10 s requirement. Consequently the ordinary-flight arrival callback occurs 0.02 s earlier than before; preserving every callback time and changing that duration cannot both be satisfied. Flight durations, entrance easing, the 0.06 s preparation, and stagger delays are unchanged. The unowned entrance scheduler retains its existing 0.12 s settle allowance.
2. **Homeward and fleeing flights have no existing touchdown squash or idle return.** Their existing completion path immediately invokes arrival and disables/removes the view. Preserved that path and its callbacks rather than inserting a new delay, reactivating a removed bird, or redesigning house behavior. They do use the drawn takeoff/flap/landing frames until completion. A universal postflight squash/idle requirement cannot coexist with the instruction to preserve these callbacks and lifecycle.
3. **The supplied tree cannot produce a green full suite within the owned files.** The full run discovers 317 EditMode and 70 retained PlayMode tests, not the older documentation baseline of 304 and 21. No existing tests were deleted, skipped, weakened, or renamed. The added fixture increases PlayMode discovery to 77.
   - `GameplayTests.SaplingIntroPersistsOnShowAndBranchTapDismissesWithoutSpending`, `GameplayTests.cs:324`: expects `(720.00, -1802.00)` but receives `(720.00, -1764.80)`. The unowned `TutorialHand.PlaceHand` clamps the pointer using its pivot and screen height; with its current 160 px height and 0.97 pivot, the lower bound is -1764.8.
   - `IllegalMoveFeedbackTests.MissingTurnSequencesUseExistingBlinkAndInstantSettlesImmediately`, `IllegalMoveFeedbackTests.cs:158`: expects `mint_turn_left_01` to be absent, but the supplied tip contains and loads that sprite. The assertion fails before configuring a `BirdView`.

## Verification

Every Unity invocation was serialized with `flock /tmp/unity.lock`; commands waited for shared-lock ownership. No other worker's Unity process or user Editor was killed.

| Check | Observed result |
| --- | --- |
| `python3 ci/check_current_game_names.py --self-test` | 8 tests passed |
| `python3 ci/check_current_game_names.py` | 0 violations |
| Before-change targeted flight regression | Failed as intended: `sunny_fly_up` instead of `sunny_flight_takeoff_01` at launch |
| `flock /tmp/unity.lock bash scripts/run_tests.sh` | Scene build succeeded; EditMode 317/317 passed; PlayMode 75/77 passed, 2 failed, 0 skipped/inconclusive |
| Retained expanded EditMode cases | LevelsData 81; Layout 41 |
| Added flight fixture in the full run | 7/7 passed |
| Rendered flight acceptance run | 2/2 passed, 0 skipped |
| `flock /tmp/unity.lock bash scripts/screenshots.sh` | 34 stable-state PNGs generated at 1080 × 1920 |

The first post-change build exposed an ambiguous `SafeArea` name in the capture helper; qualified it as `PerchSort.Game.SafeArea` before the reported compiled/full-suite run. The initial capture attempt stopped at that same compile error and produced no accepted evidence.

The seven new tests cover:

- Takeoff → repeated flap loop → landing order, including every frame of the six-frame loop on a long-duration eased entry flight.
- Elapsed-time cadence within one update frame, not `Ease.OutCubic` positional progress.
- Positive x, negative x, and equal-x direction selection.
- The 0.3 s versus 0.4 s short-flight boundary.
- Fallback after removing each directional phase's first sprite from an isolated per-test art cache; real Resources assets remain untouched.
- Nonnegative x scale, cancellation during all three phases, idle reset, and suppressed stale callbacks.
- Synchronous instant flight without flight-frame assignment.
- Real board transfers, flock homecoming into the matching house, and the real level entrance routine.

Local logs/results remain in ignored `build/`: `flight-before.xml`, `flight-before.log`, `scene-build.log`, `test-results.xml`, `editmode-tests.log`, `playmode-results.xml`, `playmode-tests.log`, `flight-capture.xml`, `flight-capture.log`, and `screenshots.log`.

## Timed evidence

The contact sheets contain actual Unity camera renders, not reconstructed sprite artwork. Each tile has its scheduled/observed game time and the tracked bird's actual frame name beneath it. The CSV files record that frame name, bird id, position, and body/root x scales.

| Scenario | Contact sheet / CSV | Samples |
| --- | --- | --- |
| Short left-to-right branch transfer | [short-transfer.png](evidence/short-transfer.png) / [short-transfer.csv](evidence/short-transfer.csv) | 13, 0.00–0.60 s |
| Long right-to-left branch transfer | [long-transfer.png](evidence/long-transfer.png) / [long-transfer.csv](evidence/long-transfer.csv) | 14, 0.00–0.65 s |
| Homeward flight into a house | [homeward.png](evidence/homeward.png) / [homeward.csv](evidence/homeward.csv) | 14, 0.00–0.65 s |
| Level entrance | [entrance.png](evidence/entrance.png) / [entrance.csv](evidence/entrance.csv) | 69, 0.00–3.40 s |

All samples are spaced by 0.05 game seconds, using a restored-after-test 120 Hz fixed capture clock. Maximum CSV scheduling difference is 0.000001 s after rounding. These captures establish frame playback and runtime appearance, not wall-clock performance or device acceptance. The short/long transfer labels describe path distance; both retain the existing 0.45 s transfer duration. The homeward CSV uses `removed` after the tracked bird's existing arrival/removal callback. A 0.05 s sheet need not show every shorter-lived drawing; the 120 Hz regression checks cover those intervening frames.

Rendered acceptance command:

```bash
flock /tmp/unity.lock env \
  FLIGHT_CAPTURE_DIR=/Users/apple/orca/workspaces/game/flight-wiring/build/flight-capture \
  /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -projectPath /Users/apple/orca/workspaces/game/flight-wiring/PerchSort \
  -runTests -testPlatform PlayMode \
  -testFilter 'PerchSort.PlayModeTests.FlightFramesTests.BoardTransfersAndHomewardFlightsUseDirectionalFrames;PerchSort.PlayModeTests.FlightFramesTests.LevelEntranceUsesSecondsBasedFramesAndSettlesEveryBird' \
  -screen-width 540 -screen-height 960 \
  -testResults /Users/apple/orca/workspaces/game/flight-wiring/build/flight-capture.xml \
  -logFile /Users/apple/orca/workspaces/game/flight-wiring/build/flight-capture.log
```

Raw 540 × 960 camera frames remain under ignored `build/flight-capture/`. Contact sheets only resize those renders and add labels. Visually inspected the transfer/homeward sheets, the active entrance sequence, and the canonical level-start screenshot.

## Generated-state preservation

Recorded the initial clean workspace and scene/meta/settings/package state before Unity. The scene builder regenerated only `Assets/Scenes/Game.unity` among the 33 recorded scene/settings/package files: object ids/order and InputSystem action references changed. Reviewed those changes, preserved the generated text as ignored `build/scene-build-output.txt`, and restored the verified pre-Unity scene bytes. No generated scene/settings/package changes are included in the branch. The primary checkout was never modified.
