# Tutorial hand report

## Implementation

The bird pointer tracks the target bird body at its lower third, remains unmirrored, is clamped inside the design safe area with a 24-unit right inset, and disappears immediately on selection. Deselecting restores it. Its cosine pulse runs from 1 to 0.92 and back over 0.9 seconds. The sapling pointer is also clamped inside the safe design area.

The new PlayMode case checks face-rectangle separation, canvas containment for the bird and sapling pointers, and selection/deselection behavior. Captures in `evidence/` show the bird and sapling tutorial layouts; `bird-pulse-*.png` are successive timed bird frames.

## Verification

- `python3 ci/check_current_game_names.py --self-test`: 8 passed.
- `python3 ci/check_current_game_names.py`: 0 violations before Unity.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`: scene build succeeded; EditMode 312/312 passed, no skips; PlayMode 53/54 passed, 1 failed, no skips. Results: `build/test-results.xml`, `build/playmode-results.xml` (ignored local artifacts).
- `flock /tmp/unity.lock bash scripts/screenshots.sh`: succeeded; stable captures in `build/screens/game/` (ignored local artifacts).
- Serialized graphics PlayMode run with `TUTORIAL_HAND_EVIDENCE` and test filter `PerchSort.PlayModeTests.TutorialHandTests`: 1/1 passed, no skips. `evidence/bird.png` and `evidence/sapling.png` were captured after three seconds of displayed fixture gameplay; pulse frames and measured scales are recorded in `evidence/timing.txt`. The test uses nonpersistent fixtures and instant level construction, then enables real-time pointer motion. These are not captures three seconds after starting the normal entrance, which itself lasts longer than three seconds and blocks the pointer.

## Unmet acceptance / integration action

The full suite is not green. The existing `GameplayTests.SaplingIntroPersistsOnShowAndBranchTapDismissesWithoutSpending` asserts the old exact pointer anchor `(720, -1802)`, which places the bottom of the 160-unit pointer outside the 1920-unit design area. The safe-area requirement instead clamps it to `(720, -1764.8)`. The old assertion is in `GameplayTests.cs`, outside the two owned implementation/test files; it must be removed or replaced by a behavioral safe-area assertion by its owner. No tests were skipped or weakened. The requested unchanged test count is incompatible with adding the explicitly requested new test; the new case adds one to the existing discovery count.

The timed evidence does not establish device-specific safe-inset behavior or a normal, non-instant level entrance screenshot at three seconds. Runtime screenshot inspection shows the target bird face and upper body visible, and the sapling pointer on its control.
