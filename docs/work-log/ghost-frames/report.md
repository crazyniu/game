# Popup and house bird ghost-frame repair

## Result

Fixed [issue #68](https://github.com/crazyniu/game/issues/68) on `mac-ghost-frames/work`, based on `cc07f847cbe807333e8d61cb1b575c4dc9c771b4`.

Drawn sequences now use one fully opaque drawing at a time, including transitions into and out of an action. The win panel's side birds and hero retain their entrance CanvasGroup fade, travel and landing. The corrected runtime contact sheets contain no blended foreground bird drawing outside that intentional entrance fade.

## Changes and ownership

- `UiBirdLife.cs`: sequence playback renders the drawn-frame overlay at its original full opacity and disables the idle Image rather than alpha-blending both drawings. Frame selection, rates, reverse/hold timing, looping and action scheduling are unchanged. Returning to rest re-enables the idle Image and hides the overlay. Cross-fading remains only in the single-still fallback path.
- `PopupMotion.cs`: popup flight uses existing authored `<species>_flap_NN` drawings as hard cuts at 14 fps, matching the existing UiBirdLife flap rate. The old two-still wing cross-fade remains only for species without a multi-frame flap sequence. Entrance CanvasGroup opacity, trajectory, duration, landing and cancellation are preserved.
- New `PopupBirdCutTests.cs` and Unity metadata: two isolated, nonpersistent real-scene PlayMode regressions sample every rendered frame for six seconds in the normal suite. They assert exactly one visible drawing, Graphic alpha 1 within 0.001 outside entrance fading, and completion of every entrance. The win case also asserts an intermediate entrance opacity for each of its three birds, preventing removal of the fade to satisfy the test. Optional capture mode extends the same scenarios to 10 and 15 seconds.
- `WinPanel.cs` and `BirdhouseView.cs` need no edits: their existing UiBirdLife consumers receive the fix. No art, layout, rules, level data, persistence, assembly definition, existing test or runner changes.

## Before and after evidence

Real Unity 6000.6.7f1 PlayMode rendering of `Game.unity`, using a synthetic nonpersistent level-one fixture and a 540×960 camera target. `GameMotion.Instant` is false throughout each observation. Both houses are explicitly marked occupied in the fixture; this is not a claim of newly tested delivery logic. The win capture is the first normal-motion opening, without an instant-mode win warm-up.

`Time.captureDeltaTime = 1/60` supplies deterministic animation time, not a wall-clock or device-performance claim. Each win bird has 601 observed frames from 0 through 10 seconds; each house bird has 901 from 0 through 15 seconds. Contact-sheet targets are 0.1 seconds for win and 0.25 seconds for houses, including both endpoints. Actual times are recorded in [contact-sheet-times.csv](evidence/contact-sheet-times.csv); maximum positive target deviation is 0.000001 seconds at the logged precision.

| Surface | Before | After | Tiles per sheet |
| --- | --- | --- | ---: |
| Win side birds and hero, 0–10 s / 0.1 s | [win-before.jpg](evidence/win-before.jpg) | [win-after.jpg](evidence/win-after.jpg) | 101 |
| Two occupied houses, 0–15 s / 0.25 s | [houses-before.jpg](evidence/houses-before.jpg) | [houses-after.jpg](evidence/houses-after.jpg) | 61 |

The JPEGs are cropped from genuine rendered frames, at most 1600 pixels wide. No bird is composited into a frame. At time zero the camera target's layout has not yet completed its first scaler update; subsequent tiles use the settled target layout. The crops retain the subjects in both layouts.

Visible-Graphic CSVs retain the every-frame opacity measurements used by the assertions, including entrance alpha: [win before](evidence/win-before-alpha.csv), [win after](evidence/win-after-alpha.csv), [houses before](evidence/houses-before-alpha.csv), [houses after](evidence/houses-after-alpha.csv). Disabled/zero-alpha drawings are omitted from these compact exports; full diagnostic rows remain under ignored `build/bird-cuts-{before,after}/`.

| Bird | Fractional Graphic samples outside entrance fade, before | After |
| --- | ---: | ---: |
| Left blue win bird | 126 | 0 |
| Right pink win bird | 118 | 0 |
| Yellow win hero | 70 | 0 |
| Yellow house resident | 112 | 0 |
| Blue house resident | 112 | 0 |

The original house failure includes `sky_idle` alpha 0.623843 and `sky_blink_01` alpha 0.376157 simultaneously at 2.183 s. The original win sheet also shows a ghosted left bird around 3.3 s and later action boundaries. After correction every visible drawing outside entrance fading has alpha 1, and exactly one drawing is enabled/visible per bird. Frame logs show changing drawings throughout both scenarios, not a frozen opaque replacement.

Inspected both before/after sheets, the actual stable `build/screens/game/win.png`, and the approved `art/approved_ui/ui_win.png` reference. Retained the current forest/panel/bird treatment and layout; this repair does not redesign the existing differences from the reference.

## Verification

| Command / check | Observed result |
| --- | --- |
| `python3 ci/check_current_game_names.py --self-test` before capture and again before final suites | 9/9 passed each time |
| `python3 ci/check_current_game_names.py` immediately before final suites, with new source staged | 179 tracked text files inspected; zero violations |
| Before runtime alpha regression | Both new opacity regressions fail on unmodified runtime; completed house and win observations retained in before evidence |
| Corrected rendered capture, `PopupBirdCutTests` | 2/2 passed, zero failures/skips; all 162 after tiles captured |
| `flock /tmp/unity.lock bash scripts/run_tests.sh`, run once after implementation | Scene build succeeded; **317/317 EditMode**, **89/89 PlayMode**, zero failures/skips/inconclusive cases |
| Retained expanded suites | LevelsData **81/81**, Layout **41/41**; all passed |
| New regressions in full suite | **2/2 passed**; existing 87 PlayMode cases remain passing |
| `flock /tmp/unity.lock bash scripts/screenshots.sh` | Succeeded; 34 stable screenshots under `build/screens/game/`; inspected `win.png` |

Every Unity invocation waited on `flock /tmp/unity.lock`. Other workers were using or waiting for the lock during setup; no process was killed. Initial Git status was clean. Recorded SHA-256 state for 33 scene/settings/package files before Unity. The scene builder regenerated `Assets/Scenes/Game.unity`; it remains uncommitted and was not blindly restored. Packages and ProjectSettings checksums are unchanged.

Capture-harness setup exposed an ambiguous `SafeArea` type (qualified it as `PerchSort.Game.SafeArea`) and, in the first win attempt, a cached transient wing Image destroyed on landing (skip destroyed cached Graphics). These were new harness errors, not production regressions. The house's before observation completed during that attempt; the corrected harness then completed the separate before win observation and failed on the intended opacity assertion. Neither existing tests nor discovery safeguards were altered.

### Capture commands

Executed from the assigned workspace, with separate results/logs for each phase:

```sh
env BIRD_CUT_CAPTURE_DIR=/Users/apple/orca/workspaces/game/ghost-frames/build/bird-cuts-before flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -screen-width 540 -screen-height 960 -projectPath /Users/apple/orca/workspaces/game/ghost-frames/PerchSort -runTests -testPlatform PlayMode -testFilter PerchSort.PlayModeTests.PopupBirdCutTests -testResults /Users/apple/orca/workspaces/game/ghost-frames/build/bird-cuts-before.xml -logFile /Users/apple/orca/workspaces/game/ghost-frames/build/bird-cuts-before.log

env BIRD_CUT_CAPTURE_DIR=/Users/apple/orca/workspaces/game/ghost-frames/build/bird-cuts-before flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -screen-width 540 -screen-height 960 -projectPath /Users/apple/orca/workspaces/game/ghost-frames/PerchSort -runTests -testPlatform PlayMode -testFilter PerchSort.PlayModeTests.PopupBirdCutTests.WinBirdDrawingsStayOpaqueOutsideEntranceFade -testResults /Users/apple/orca/workspaces/game/ghost-frames/build/win-cut-before.xml -logFile /Users/apple/orca/workspaces/game/ghost-frames/build/win-cut-before.log

env BIRD_CUT_CAPTURE_DIR=/Users/apple/orca/workspaces/game/ghost-frames/build/bird-cuts-after flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -screen-width 540 -screen-height 960 -projectPath /Users/apple/orca/workspaces/game/ghost-frames/PerchSort -runTests -testPlatform PlayMode -testFilter PerchSort.PlayModeTests.PopupBirdCutTests -testResults /Users/apple/orca/workspaces/game/ghost-frames/build/bird-cuts-after.xml -logFile /Users/apple/orca/workspaces/game/ghost-frames/build/bird-cuts-after.log
```

## Boundaries

No requested contract change or missing art. Real rendered PlayMode evidence and the full retained suites establish the requested hard-cut behavior on the tested Mac editor target. No physical-device, manual touch-input, listening, or wall-clock performance acceptance is claimed. The still-only fallback is retained by design; the timed captures exercise the authored sunny/sky/berry sequences, not a missing-art species.
