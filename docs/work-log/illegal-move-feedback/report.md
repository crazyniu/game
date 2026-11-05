# Illegal-move feedback report

## Integrated fallback fixture correction

Merged `origin/integration-next` first by fast-forward to `5b191c0e`. The reported failure was accepted without rerunning it: `mint` now has drawn turn frames, so absence of shipped art was an invalid fixture assumption.

The fallback test now seeds only its fresh `GameArt` instance's existing private sprite cache with null entries for `mint_turn_left_01`, `mint_turn_right_01`, and `mint_blink_01`. The sequence loader stops at the injected first-frame gaps, regardless of current or future shipped sequences. Existing idle/blink sprites still render the real fallback; no resources, shared caches, production accessor, or production fallback behavior are changed. The blink-sequence gap keeps the existing still-blink assertion independent of newly shipped blink sequences as well.

Verification commands:

```bash
PYTHONDONTWRITEBYTECODE=1 python3 ci/check_current_game_names.py --self-test
PYTHONDONTWRITEBYTECODE=1 python3 ci/check_current_game_names.py
flock /tmp/unity.lock bash scripts/run_tests.sh
```

The naming self-test passed all eight tests; the guard inspected 2143 tracked text files with zero violations. The canonical scene build and compilation succeeded. Full suites: **317/317 EditMode passed; 69/70 PlayMode passed; zero skips**. All three `IllegalMoveFeedbackTests` cases passed, including `MissingTurnSequencesUseExistingBlinkAndInstantSettlesImmediately`.

The sole full-suite failure is outside the corrected fixture: `GameplayTests.SaplingIntroPersistsOnShowAndBranchTapDismissesWithoutSpending`, `GameplayTests.cs:324`, expected tutorial position `(720.00, -1802.00)` but observed `(720.00, -1764.80)`. No tutorial code or assertions were modified, and no green-full-suite claim is made. XML/logs are `build/test-results.xml`, `build/playmode-results.xml`, `build/editmode-tests.log`, and `build/playmode-tests.log`. The pre-run working tree contained only the intended test edit; generated-only resource metadata and scene changes were backed up in `build/integration-generated-changes.patch` and restored, not committed.

The rest of this document records the original implementation and its original-run evidence; those earlier totals and failures are historical, not the integrated correction's results. No new screenshot run was needed for this test-only correction.

## Delivery and ownership

Branch: `mac-illegal-move/work`, based on the assigned gameplay-gaps tip `f8b961ac3c45c5c6d6114245abd61c93e4e764fb`.

Runtime edits are limited to `PerchSort/Assets/Scripts/Game/BirdView.cs` and `BirdIdleGroup.cs`. Added the explicitly requested `PerchSort/Assets/Tests/PlayMode/IllegalMoveFeedbackTests.cs` and its Unity metadata, this report, and evidence. No rules, caller APIs, flights, idle scheduling, scene, settings, package, save, level, or artwork changes are included.

- `BirdView.Shake` uses the existing cached drawn sequences: left 01,02,03,02,01; right 01,02,03,02,01; idle. Ten frames at 16 fps take 0.625 seconds per bird.
- `BirdIdleGroup.InvalidFeedbackDelay` uses the existing seat-ordered group to start the tip first, then each bird 0.04 seconds later. This does not change idle leases or scheduling.
- The invalid-feedback path keeps normal breathing, fixes the body to its seat, and introduces no rotation, translation, mirroring, or additional scaling.
- If either required turn sequence has fewer than three usable frames, the existing blink playback remains the fallback, with the same tip-first delay. The corrected fixture forces missing sequences in its own art cache rather than assuming a shipped species lacks them.
- `GameMotion.Instant` skips delays and returns every affected bird to its rest pose synchronously.
- LitMotion binds its initial value before a configured delay. The regression test caught premature first frames; the final implementation gates frame assignment using elapsed time inside the motion instead.

## Requirements impossible under the supplied contract

1. **Selection cancellation after the last bird, existing completion callback, and input lock:** those facilities do not exist in the assigned tip's invalid-feedback path. `GameSession.TryMove` calls `ClearPresentation`, cancelling logical selection immediately. `GameRoot.TryMove` then calls `Board.ShowSelection(..., -1)` and synchronous `Board.Illegal`. `Board.Illegal` calls parameterless `BirdView.Shake`; none accepts a completion callback, and `GameRoot.IsBusy` is not set. The capture log records selection `-1` and busy `False` throughout. Correcting this requires authority to change callers outside the two owned runtime files. No reflection, synthetic flight, or other out-of-scope workaround was added. Tests assert selection is cancelled afterwards, not that it remains selected until completion; that missing behavior is not accepted here.
2. **Last-bird completion within a 0.7-second lock:** the required sequence and spacing give `0.625 + 0.04 * (runCount - 1)` seconds. Three birds finish at 0.705 seconds; four finish at 0.745 seconds. The level-one three-bird regression preserves the requested sequence and spacing rather than silently accelerating it. The existing fallback blink itself lasts 0.7 seconds when no blink sequence exists, before any stagger.
3. **Full suite green:** the final full run has one failure outside the invalid-feedback path: `T54CIdleTests.BreathingPersistsBetweenActionsWithoutMovingLogicalSeatsAndInstantFreezesIt`, at its line 136 exact unit-scale assertion. Unity formats both vectors as `(1.00, 1.00, 1.00)` despite their inequality. This task does not alter idle scheduling or Instant idle reset, and does not weaken that test. No full-green claim is made.

## Verification

All Unity commands waited on `flock /tmp/unity.lock`; no user's Editor was killed. The working tree was clean before Unity, and initial scene/meta/settings/package Git blob IDs were recorded. Generated-only import/scene/bytecode changes were backed up separately under `build/generated-changes.patch` and excluded from the delivery.

Commands run from the repository root:

```bash
python3 ci/check_current_game_names.py --self-test
python3 ci/check_current_game_names.py
flock /tmp/unity.lock bash scripts/run_tests.sh
flock /tmp/unity.lock bash scripts/screenshots.sh
```

The naming self-test passed all eight tests; the guard found zero violations. The full runner built the canonical scene and compiled successfully. Original implementation results:

| Suite | Passed | Total | Failed | Skipped |
| --- | ---: | ---: | ---: | ---: |
| EditMode | 312 | 312 | 0 | 0 |
| PlayMode | 55 | 56 | 1 | 0 |
| Targeted illegal feedback | 3 | 3 | 0 | 0 |

All 53 pre-existing PlayMode cases remain discovered, plus the three new cases. All 81 LevelsData and 41 Layout expanded cases remain discovered. The actual assigned tip has more cases than the historical documentation baseline; no discovery safeguard was reduced. Full XML/logs are `build/test-results.xml`, `build/playmode-results.xml`, `build/editmode-tests.log`, and `build/playmode-tests.log`. The initial full attempt also exposed a music focus/pause timing failure and the now-fixed premature stagger frame; its PlayMode XML was preserved as `build/full-initial-playmode.xml`. Music passed on the subsequent full run after the stagger fix; its code and tests were untouched.

The new level-one regression failed before the runtime edit with zero turn frames observed (`build/illegal-before.xml`). The targeted fixture proves ordered drawn frames and frame timing within 1/16 second, fixed seat/body position and rotation, positive breathing-only scale, unchanged board/bird data and move count, cancellation afterwards, three-bird tip-first stagger and completion times within one 120 Hz simulation step, real missing-species blink fallback, and immediate Instant completion.

Timed runtime capture command (Unity executable shown exactly):

```bash
flock /tmp/unity.lock env \
  ILLEGAL_MOVE_EVIDENCE_DIR=/Users/apple/orca/workspaces/game/illegal-move/build/illegal-move-frames \
  /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -screen-width 540 -screen-height 960 \
  -projectPath /Users/apple/orca/workspaces/game/illegal-move/PerchSort \
  -runTests -testPlatform PlayMode \
  -testFilter PerchSort.PlayModeTests.IllegalMoveFeedbackTests \
  -testResults /Users/apple/orca/workspaces/game/illegal-move/build/illegal-capture.xml \
  -logFile /Users/apple/orca/workspaces/game/illegal-move/build/illegal-capture.log
```

The initial failing-before run used `-batchmode -nographics`, the same project path, the filter ending in `.IllegalLevelOneTransferUsesDrawnTurnsWithoutChangingBoard`, and output paths `build/illegal-before.xml` and `build/illegal-before.log`.

## Evidence and boundaries

- [Contact sheet](evidence/contact-sheet.png): actual Unity camera pixels from the illegal level-one transfer from branch 0 to branch 2, sampled every 0.05 seconds from 0 through 0.75 seconds. The selected bird's branch is cropped uniformly for readable pose comparison; no artwork is modified.
- [Samples](evidence/samples.csv): scheduled/observed simulation time, actual sprite name, logical selection, busy state, body position, rotation and scale.
- Source camera frames and targeted XML/log remain under `build/illegal-move-frames/`, `build/illegal-capture.xml`, and `build/illegal-capture.log`. Capture uses `Time.captureFramerate = 120`, restored in teardown: evidence establishes Unity runtime simulation cadence, not wall-clock device performance or physical pointer input.
- Stable screenshot command succeeded for all 34 expected 1080×1920 captures under `build/screens/game/`. These stills are not used as animation proof.

No temporary runtime harness or alternative implementation was added. Tests initialize without persistence and restore motion mode, capture rate and random state. The caller-level cancellation/lock gap and the full-suite idle failure remain explicitly unfulfilled acceptance criteria, not hidden follow-up work.
