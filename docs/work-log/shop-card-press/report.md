# Shop card press feedback

## Change

Booster, outfit and birdhouse cards now use the existing `GameButton` interaction, retaining their existing click handlers, locked-card shake and insufficient-funds feedback. Locked booster cards remain interactable and are not scaled down at rest. `GameButton` supplies linear press, ease-out-back release, cancellation and `GameMotion.Instant` behavior.

## Requested contract change — blocking acceptance

`GameLayout.ButtonPressScale`, `ButtonPressSeconds` and `ButtonReleaseSeconds` are currently `0.92`, `0.08` and `0.18`. The assigned target is `0.96`, `0.06` and `0.12`, respectively, while also requiring those same shared constants. `GameLayout.cs` is outside this worker's owned files. The coordinator must arrange the shared-constant change and evaluate its effect on every `GameButton`, then rerun integrated acceptance. This branch intentionally does not introduce independent numbers or change the shared file. The requested behavior is not complete on this branch.

## Verification

- `python3 ci/check_current_game_names.py --self-test`: 9 passed.
- `python3 ci/check_current_game_names.py`: 184 tracked text files inspected, zero violations before Unity; 185 tracked text files inspected, zero violations after staging the new test and evidence.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`: scene build completed; EditMode 317/317 passed; PlayMode 96/98 passed, two failed, zero skipped. Both new pointer tests measured press scale `0.920000017`, failing the requested `0.96 ± 0.01`. Both reached and passed the release scale assertion at 0.3 seconds. Results: `build/test-results.xml`, `build/playmode-results.xml` (local ignored output).
- A separate serialized graphical PlayMode capture of the same two EventSystem interactions used `Time.captureFramerate = 50`; it likewise recorded the two expected press failures. The temporary capture instrumentation was removed. The contact sheets are `evidence/firefly-press-release.jpg` and `evidence/springrain-press-release.jpg`: rest, five 0.02-second press samples, and fifteen 0.02-second release samples. Frames are real camera renders; fixed game-time intervals are not wall-clock timing evidence. Pointer-up was dispatched without a click, so these sheets do not demonstrate the existing shake or purchase.
- No physical device or manual touch test was performed. Cosmetic card presses and shake behavior were not separately exercised.

## Workspace boundary

Unity generated changes to `Game.unity` and hundreds of asset `.meta` files during the run. The pre-run working tree was clean; these generated changes are not included in the commit and were not blindly restored. The test's Unity-generated `.meta` file is included.
