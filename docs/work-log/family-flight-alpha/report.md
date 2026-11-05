# Family flight perched-bird alpha

## Result

Fixed [issue #72](https://github.com/crazyniu/game/issues/72). The idle base now remains opaque while only the pose overlay fades. The new frame-by-frame family-flight regression fails on the original alpha behavior and passes after the fix. The final full serialized run passed **317 EditMode and 88 PlayMode tests**, with zero failures or skips.

## Cause and change

`BirdView.BlendIdlePose` previously assigned complementary alpha to two overlapping images: pose `blend`, idle `1 - blend`. Standard source-over compositing gives overlapping opaque texels coverage `1 - blend * (1 - blend)`, reaching only 0.75 at the midpoint. This was a perched bird's own idle crossfade happening while its neighbours flew, not a flight-cleanup or parent CanvasGroup fade.

- `BirdView.cs`: keep the idle image at `Color.white`; draw the pose on the existing overlay with alpha `blend`. The existing smooth 0.12-second fade-in/fade-out, hold, breathing, drawn sequences and overlay-attachment interpolation remain unchanged. The displayed-frame accessors continue reporting the pose rather than the newly retained idle underlay.
- `BirdOverlay.cs`: intentionally unchanged. Its `Apply`/`Blend` methods only calibrate attachment transforms; they do not fade a bird.
- `FamilyFlightAlphaTests.cs`: load the actual Game scene and authored level 15 without persistence; move mama 0→1 with real motion. Sample every bird Graphic each game-time frame through both family landings. Check every active nonmoving base/decorative Graphic, CanvasRenderer alpha and inherited CanvasGroup alpha against 1.0 within 0.001. The intentionally fading pose overlay is exempt only from its own Graphic-color alpha check, not from inherited opacity checks. Both moving birds must reach their actual destination seats with opaque bodies.
- `BirdIdleTests.cs`: coordinator-authorized updates to the two affected assertion areas only. Check an opaque base throughout the crossfade, monotonic overlay ramps, a fully opaque hold, 0.12-second transition duration, and opaque cancellation. All existing motion, cadence, hold, landing and cancellation checks remain. No test was removed; PlayMode discovery increased from 87 to 88.

## Timed runtime evidence

Unity 6000.6.7f1, graphics-enabled real PlayMode, `Random.InitState(12345)`, normal motion, `Time.captureDeltaTime = 1/60`, 540×960 camera target, nonpersistent fixture. The regression explicitly starts the third pink bird's production still-pose action at 0.7 seconds during the family move; this makes the reported crossfade reproducible independently of wall-clock/unscaled idle scheduling. It does not inject alpha or replace the production animation.

Each run samples **76 consecutive frames from 0 to 1.25 seconds** until both family birds land. Each contact sheet contains the first **73 consecutive frames, 0 to 1.2 seconds**, labelled with actual game time. The subsequent homecoming/gift cascade is not the observation boundary: it eventually moves/wakes other birds, including the pink flock. The test identifies the mama/chick moving IDs from the actual Core events, not by species or screen position.

| Measurement | Before | After |
| --- | ---: | ---: |
| Nonmoving base Graphic samples | 1064 | 1064 |
| Minimum nonmoving base alpha | 0.000000 | 1.000000 |
| Pink bird 10 base alpha at 0.7500 s | 0.376157 | 1.000000 |
| Pink bird 10 pose-overlay alpha at 0.7500 s | 0.623843 | 0.376157 |
| Pink bird 10 CanvasRenderer / inherited alpha at 0.7500 s | 1 / 1 | 1 / 1 |

The before sheet shows forest leaking through the pink body during the crossfade. The after sheet retains an opaque body throughout the same directed action. All **1216 active nonmoving non-overlay Graphic samples** in the after run had Graphic, CanvasRenderer and inherited alpha exactly 1.0.

Committed evidence:

- [Before full board](evidence/before.jpg) and [pink-branch detail](evidence/before-detail.jpg).
- [After full board](evidence/after.jpg) and [pink-branch detail](evidence/after-detail.jpg).
- [Before alpha samples](evidence/before-alpha.csv) and [after alpha samples](evidence/after-alpha.csv): one row per bird/frame, recording base/overlay alpha and minima across all active Graphics, excluding the intentional pose overlay only from the ordinary Graphic-color minimum.

Raw PNGs and per-Graphic samples remain in ignored `build/family-flight-alpha/{before,after}/`. JPEGs are quality 80; the detail crop uses source pixels `(275,540)–(540,680)`. These are game-time observations, not a claim of 60 FPS wall-clock performance.

## Verification

All Unity invocations held `flock /tmp/unity.lock`; competing workers were allowed to finish. No user's Editor or other Unity process was terminated.

Final required commands, run once after implementation:

```bash
python3 ci/check_current_game_names.py --self-test
python3 ci/check_current_game_names.py
flock /tmp/unity.lock bash scripts/run_tests.sh
```

- Naming self-test: **9 passed**. Naming guard: **179 tracked text files, 0 violations**. The new test was staged before this check so the guard inspected it.
- Sole-scene build: succeeded via the test runner's `PerchSort.Editor.GameSceneBuilder.Build`.
- EditMode: **317/317 passed**, 0 failures, 0 skips; retained LevelsData **81** cases and Layout **41** cases.
- PlayMode: **88/88 passed**, 0 failures, 0 skips.
- Final results: `build/test-results.xml`, `build/playmode-results.xml`; logs: `build/scene-build.log`, `build/editmode-tests.log`, `build/playmode-tests.log`.

Graphics-enabled before/after invocation (substitute `before` or `after` for the capture directory and result/log basename):

```bash
FAMILY_ALPHA_CAPTURE_DIR="$PWD/build/family-flight-alpha/before" \
flock /tmp/unity.lock \
/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity \
-batchmode -projectPath "$PWD/PerchSort" -runTests -testPlatform PlayMode \
-testFilter PerchSort.PlayModeTests.FamilyFlightAlphaTests \
-testResults "$PWD/build/family-flight-alpha/before-results.xml" \
-logFile "$PWD/build/family-flight-alpha/before.log"
```

The completed baseline run failed **0/1 passed, 1 failed** specifically on perched `Pose` alpha, including bird 10 at 0.7500 seconds. The after capture used filter `'PerchSort.PlayModeTests.FamilyFlightAlphaTests;PerchSort.PlayModeTests.BirdIdleTests'`: the family regression passed; that combined run was **5/6 passed** because a newly added fade-boundary check used the final frame's unusually short delta and treated alpha 0.999 as the start of the opaque hold. Corrected the check to use exact opaque alpha and the largest observed sampling interval. The subsequent filtered opaque-base crossfade test passed **1/1**, followed by the fully green required suites above.

During capture-fixture development, one compile attempt failed on an NUnit collection-constraint overload; corrected it to `Has.No.Member`. Two preliminary runs failed fixture-boundary assertions: one treated the entire homecoming cascade as the family-flight duration, and one classified eventual homecoming birds as already moving. Those were corrected before the completed alpha-failure baseline. No product exception was suppressed and no test was skipped.

## Boundaries and repository state

- No art, rules, level data, save keys, entrance timing, audio, or unrelated runtime files changed. The only additional source ownership was the coordinator's explicit authorization for the two `BirdIdleTests.cs` assertion areas.
- No physical-device, physical-pointer or audio-listening acceptance. Camera-rendered timed frames prove this alpha path, not unrelated layout or animation acceptance. Instant stable-state screenshots were not used as motion proof.
- The worktree was clean before Unity. Import/build generated **664 existing asset-metadata modifications and one Game.unity modification** outside this delivery. They remain uncommitted and preserved in this isolated worktree; they were not blindly restored or included in the branch commit. The new test's own `.meta` is included.
