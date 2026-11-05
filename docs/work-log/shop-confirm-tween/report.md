# Shop confirmation tween

## Change

The confirmation's `GameMotion` now belongs to the dialog, rather than the shop. Disabling or destroying the dialog cancels its motions through `GameMotion` lifecycle hooks. The opening callback also checks that the dialog is active and the panel still exists. Shop purchase feedback continues to use the shop's separate motion owner.

## Before and after

- Before the fix, a normal-motion PlayMode regression opened a Firefly confirmation and invoked its confirm button in the same frame. Coins and stock assertions were reached, then Unity logged `MissingReferenceException` from `ShopView.OpenConfirmation` at `ShopView.cs:206`. The filtered run failed 0/1, with 0 skipped; evidence is `build/shop-quick-before.xml` and `build/shop-quick-before.log` (local ignored output).
- After the fix, the same regression passed in the full PlayMode suite. It asserts the catalog price deduction, a one-unit stock increase, a closed dialog, and no unexpected logs after 0.6 seconds of normal motion. Additional PlayMode cases exercise immediate close followed by dialog destruction, and destruction of the panel while the dialog remains open; both passed with `LogAssert.NoUnexpectedReceived()`.

## Verification

- `python3 ci/check_current_game_names.py --self-test`: 9 passed. `python3 ci/check_current_game_names.py`: 179 tracked text files inspected, 0 violations.
- Before fix: `flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -projectPath "$PWD/PerchSort" -runTests -testPlatform PlayMode -testFilter PerchSort.PlayModeTests.ShopQuickPurchaseTests -testResults "$PWD/build/shop-quick-before.xml" -logFile "$PWD/build/shop-quick-before.log"`: failed 1/1 as described.
- After fix: `flock /tmp/unity.lock bash scripts/run_tests.sh`: scene build succeeded; EditMode 317/317 passed, PlayMode 90/90 passed; 0 failures or skips. Results: `build/test-results.xml`, `build/playmode-results.xml` (local ignored output).
- The PlayMode tests exercise actual Unity frames and invoke the UI button directly, not physical input. No screenshot, device, or manual visual acceptance was performed; this change does not alter the intended animation appearance.
- The worktree was clean before Unity. Unity generated 665 unrelated tracked file modifications, including asset metadata and scene/build artifacts; they were not staged, committed, or blindly restored.
