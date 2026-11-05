# Shop card press feedback

## Change

Booster, outfit and birdhouse cards now use the existing `GameButton` interaction, retaining their existing click handlers, locked-card shake and insufficient-funds feedback. Locked booster cards remain interactable and are not scaled down at rest. `GameButton` supplies linear press, ease-out-back release, cancellation and `GameMotion.Instant` behavior.

## Shared timing contract

The coordinator clarified that the required values are exactly the existing `GameLayout.ButtonPressScale`, `ButtonPressSeconds` and `ButtonReleaseSeconds`: `0.92`, `0.08` and `0.18`. `GameLayout.cs` is unchanged. Both tests read the shared constants for their press assertion and timing.

## Verification

- `python3 ci/check_current_game_names.py --self-test`: 9 passed.
- `python3 ci/check_current_game_names.py`: 184 tracked text files inspected, zero violations before Unity; 185 tracked text files inspected, zero violations after staging the new test and evidence.
- After the coordinator's clarification, `flock /tmp/unity.lock bash scripts/run_tests.sh` passed: EditMode 317/317 and PlayMode 98/98, zero failures or skips. Both card tests passed with the shared constants. Results: `build/test-results.xml` and `build/playmode-results.xml` (ignored local output).
- The previous full suite failed both new tests against the superseded `0.96` requirement; the observed press scale was `0.920000017`, and both release assertions passed.
- A separate serialized graphical PlayMode capture of the same two EventSystem interactions used `Time.captureFramerate = 50`. The temporary capture instrumentation was removed. The contact sheets are `evidence/firefly-press-release.jpg` and `evidence/springrain-press-release.jpg`: rest, five 0.02-second press samples, and fifteen 0.02-second release samples. Frames are real camera renders; fixed game-time intervals are not wall-clock timing evidence. Pointer-up was dispatched without a click, so these sheets do not demonstrate the existing shake or purchase.
- No physical device or manual touch test was performed. Cosmetic card presses and shake behavior were not separately exercised.

## Workspace boundary

Unity generated changes to `Game.unity` and hundreds of asset `.meta` files during the run. The pre-run working tree was clean; these generated changes are not included in the commit and were not blindly restored. The test's Unity-generated `.meta` file is included.
