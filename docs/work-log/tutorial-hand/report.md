# Tutorial hand report

## Implementation

The bird pointer tracks the target bird body at its lower third, remains unmirrored, is clamped inside the design safe area with a 24-unit right inset, and disappears immediately on selection. Deselecting restores it. Its cosine pulse runs from 1 to 0.92 and back over 0.9 seconds. Non-bird targets retain their original positions, including the sapling tool-button centre.

The PlayMode case checks face-rectangle separation, canvas containment for the bird pointer, and selection/deselection behavior. The images in `evidence/` record the earlier pre-integration run, before the sapling positioning correction; `bird-pulse-*.png` are successive timed bird frames.

## Verification
### Integrated correction

- Merged `origin/integration-next` before the correction. The previously failing `GameplayTests.SaplingIntroPersistsOnShowAndBranchTapDismissesWithoutSpending` passed unchanged in the full run.
- `python3 ci/check_current_game_names.py --self-test`: 8 passed; naming guard: 0 violations.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`: scene build succeeded; EditMode 317/317 passed; PlayMode 69/70 passed, no skips. The unrelated failure is `IllegalMoveFeedbackTests.MissingTurnSequencesUseExistingBlinkAndInstantSettlesImmediately`: it expects a missing sprite, but `mint_turn_left_01` exists in the integrated assets. Results are in `build/test-results.xml` and `build/playmode-results.xml` (ignored local artifacts).
- Serialized graphics PlayMode run of `TutorialHandTests`: 1/1 passed. Inspected current bird and sapling screenshots in `build/tutorial-hand-current/` (ignored local artifacts).

### Original run


- `python3 ci/check_current_game_names.py --self-test`: 8 passed.
- `python3 ci/check_current_game_names.py`: 0 violations before Unity.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`: scene build succeeded; EditMode 312/312 passed, no skips; PlayMode 53/54 passed, 1 failed, no skips. Results: `build/test-results.xml`, `build/playmode-results.xml` (ignored local artifacts).
- `flock /tmp/unity.lock bash scripts/screenshots.sh`: succeeded; stable captures in `build/screens/game/` (ignored local artifacts).
- Serialized graphics PlayMode run with `TUTORIAL_HAND_EVIDENCE` and test filter `PerchSort.PlayModeTests.TutorialHandTests`: 1/1 passed, no skips. `evidence/bird.png` and `evidence/sapling.png` were captured after three seconds of displayed fixture gameplay; pulse frames and measured scales are recorded in `evidence/timing.txt`. The test uses nonpersistent fixtures and instant level construction, then enables real-time pointer motion. These are not captures three seconds after starting the normal entrance, which itself lasts longer than three seconds and blocks the pointer.

## Original acceptance boundary

The original full run failed the sapling anchor assertion because the pointer was clamped. The integrated correction above restores the original tool-button position and resolves that failure without changing the existing gameplay test.

The timed evidence does not establish device-specific safe-inset behavior or a normal, non-instant level entrance screenshot at three seconds. Runtime screenshot inspection shows the target bird face and upper body visible, and the sapling pointer on its control.
