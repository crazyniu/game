# T54D — Uniform buttons and coin header

## Delivery status

Implemented in the assigned canonical files. Unity compilation, full retained suites, graphics-enabled timed capture and stable-state screenshots have now run successfully in this worker's isolated Mac worktree. No merge or main modification.

- Branch: `mac-t54d/t54`, based on the supplied contract tip `cb438e87e67e6d66c97bdf00b77ef356ca161290`.
- Button implementation: `f1b31dd`.
- Coin-header follow-up and regression coverage: `d25cf3c`.
- The earlier push `f64c3c9` reported NOT_RUN because the user's Editor was open and validation had been transferred by explicit user choice. The user subsequently closed it and explicitly directed this worker to run serialized Unity acceptance. The results below supersede that earlier validation boundary.

## Problem P4 — uniform buttons

`PerchSort/Assets/Scripts/Game/GameButton.cs` no longer reads `GameLayout.DisabledScale`. Disabled buttons retain unit scale, existing alpha dimming and non-interactivity. Disabling during a press cancels the pending scale transition and immediately restores unit scale. Only enabled pressed feedback uses the existing smaller scale; press/release timing, sprites and easing remain unchanged.

`PerchSort/Assets/Scripts/Game/HudView.cs` uses one common rectangle size per toolbar while preserving authored button centers:

- Three-tool layout: 108 by 108 design units.
- Four-tool layout: 128 by 132 for every tool; the fourth tool previously had a 132 by 132 exception.
- Fresh tutorial: its existing minimal 64-unit control remains.
- Inventory badges use the same local horizontal offset and authored vertical reference within each layout. Existing 46-unit / 50-unit badge sizes remain. Undo has no inventory badge because it consumes history, not stock.

Settings, map bottom-bar controls and popup actions use the same `GameUI.Button` / `GameButton` path. Their owner files were inspected but not changed. The HUD gear's existing temporary input-lock dimming exception remains. `GameLayout.cs` stays read-only, including its unused disabled-scale constant.

## Additional user request — coin header

Reference inspected: `art/v7/ui7_level_k3_v2.png`, plus the actual production `Resources/Art/coin_pill.png` sprite. No artwork or asset imports were authored or changed for this fix.

The old HUD used absolute screen coordinates for its number while the map used a different sprite-relative calculation. The old HUD label began at sprite x=123.272, inside the coin disc's x=128 boundary. The map label center was x=212 instead of the remaining cream area's x=218.

`HudView` now uses one proportional 320-by-128 sprite layout for both HUD variants and the map:

- Same 232-by-92.8 design-unit pill, with the existing integrated coin disc overlapping its left end; no separate duplicated coin.
- Label centered on the cream region, not the complete sprite. Its rectangle is sprite x=142..294, y=32..92, clear of the disc and rounded right end.
- Eight design units of horizontal text inset, single-line layout, existing 24..44 font auto-sizing. This also leaves room for the font's glyph overhang at `99999`.
- Shared left placement at x=40 and an eight-unit downward inset from each page's authored header row. The full pill rectangle stays clear of the first suspension, attached leaves and house art; the dense layout stays above the beam.
- `SetLevel` and entrance positioning use the same derived label bounds, so changing the house count cannot restore the old absolute text placement.

No house, rope, level, rule, economy, persistence identifier or other presentation-owner file was changed.

## Retained regression coverage

### `Tests/PlayMode/ButtonSizeTests.cs` — four passing cases

1. Three active tools on authored level 5: equal rect sizes and local/rendered scales with undo disabled, after legal move 0 to 3 enables undo, and after timed disable/re-enable transitions; equal badge size/offset.
2. Four active tools on authored level 40: the same checks, using legal move 3 to 6.
3. Disable a pressed undo: reject the click without consuming history, restore size immediately, remain stable after the old animation window, then settle after a later press/release.
4. Load all 40 authored levels and assert the actual coin pill does not overlap house artwork, suspension ropes, attached leaves or the beam.

The fixture uses the genuine Game scene and existing nonpersistent initialization / fixture-loading APIs. It restores motion, time-scale and dev-option state and does not seed PlayerPrefs.

### `Tests/EditMode/CoinHeaderTests.cs` — eight passing cases

HUD and map, each with `0`, `240`, `2000`, `99999`: verify the label rectangle is inside the cream area with disc/end clearance, centered there, the coin is not distorted, the actual font remains on one line, and rendered glyph bounds fit with margin.

The test uses a real Canvas and production Fredoka font. Reflection accesses the actual TMP component because the shared test assembly does not reference TMP and its assembly definition is outside this worker's ownership. No production reflection or assembly changes were introduced.

All retained baseline tests are unchanged; no assertions were weakened, ignored or skipped.

## Exact verification

All Unity invocations were protected by `flock /tmp/unity.lock`. Preflight after the user's closure found no running Unity Editor. The user's Editor was never killed, and the primary checkout was not modified.

| Check | Command / run | Result |
| --- | --- | --- |
| Naming policy | `python3 ci/check_current_game_names.py` | PASS, zero violations |
| Failing-before coin reproduction | Unity EditMode filter `PerchSort.EditModeTests.CoinHeaderTests` | 0/8 passed, 8 failed on the old layout |
| Full compilation and suites | `flock /tmp/unity.lock bash scripts/run_tests.sh` | Scene build succeeded; EditMode **312/312**; PlayMode **25/25**; zero failures/skips |
| Graphics-enabled timed smoke | Unity PlayMode filter `PerchSort.PlayModeTests.T54DEvidence`, 1080 by 1920 | **1/1 passed**, real UI frames captured; temporary runner removed before the full suites |
| Canonical stable captures | `flock /tmp/unity.lock bash scripts/screenshots.sh` | **34 PNGs**, each validated as 1080 by 1920 |

The focused reproduction used:

```text
flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -projectPath /Users/apple/orca/workspaces/game/t54-buttons/PerchSort -runTests -testPlatform EditMode -testFilter PerchSort.EditModeTests.CoinHeaderTests -testResults /Users/apple/orca/workspaces/game/t54-buttons/build/coin-before.xml -logFile /Users/apple/orca/workspaces/game/t54-buttons/build/coin-before.log
```

The graphics-enabled smoke used the same executable/project/lock with `-batchmode -screen-width 1080 -screen-height 1920 -runTests -testPlatform PlayMode -testFilter PerchSort.PlayModeTests.T54DEvidence -testResults /Users/apple/orca/workspaces/game/t54-buttons/build/evidence-results.xml -logFile /Users/apple/orca/workspaces/game/t54-buttons/build/evidence.log`, without `-nographics`.

Development corrections are recorded honestly: the initial test compilation found the missing TMP test-assembly reference; the test was kept within owned files using reflection. Its rendering fixture then needed a Canvas. The rendered five-digit case exposed glyph overhang with the initial four-unit inset; the production inset was increased to eight. The final full suites pass every case; no baseline test was changed to accommodate these corrections.

Local raw results remain in ignored `build/coin-before.xml`, `build/test-results.xml`, `build/playmode-results.xml`, `build/evidence-results.xml` and their logs. Portable counts and exact worker-case names are committed in `docs/work-log/button-sizing/evidence/verification.json`.

## Committed visual and timed evidence

All JPEG contact sheets are at most 1600 pixels wide, derived only from actual Unity frames. They were visually inspected after capture.

| Evidence under `docs/work-log/button-sizing/evidence/` | Contents |
| --- | --- |
| `bottom_bar_states.jpg` | Three- and four-tool bars, actual undo disabled and enabled by a legal move |
| `button_timed_contact.jpg` | 51 bottom-bar frames from 0 to 5 seconds at requested 0.1-second intervals, with press/release and disable/re-enable changes |
| `level_idle_contact.jpg` | 21 full level-1 frames from 0 to 20 seconds at one-second intervals, showing toolbar stability |
| `coin_headers.jpg` | HUD level 5, dense HUD level 40 and map; balances 0, 240, 2000, 99999 on each |
| `map_shared_controls.jpg` | Actual map settings and popup controls using the shared button path |
| `timing.csv` | Requested/observed timestamps, undo scale, alpha and interactability for both sequences |
| `verification.json` | Parsed before/final test outcomes and names |

At the CSV's millisecond reporting precision, the press sequence intervals are 0.099..0.101 seconds and the maximum reported lateness is 0.001 seconds. Every disabled press-sequence sample has scale 1 and alpha 0.45; enabled rest samples return to scale 1. The existing release easing briefly overshoots during feedback. The idle sequence reports 20.000 seconds elapsed with one-second sample spacing.

Raw graphics-enabled frames are in ignored `build/screens/buttons/`; canonical stable PNGs are in `build/screens/game/`. The capture fixture uses synthetic nonpersistent profiles, instant level setup, then real non-instant button motion and elapsed frame time. It does not claim entrance-animation or bird-idle acceptance for other workers' changes.

## Generated files and remaining limits

The pre-Unity working state had no unrelated tracked modifications. Import expanded seven bird overlay `.meta` files; the scene builder regenerated scene object IDs/order. These generated changes were inspected, preserved under ignored `build/unity-generated/`, and restored to their known pre-run tracked state, not included in this branch. Packages, settings, artwork and scene definitions are not part of the delivery.

No additional ownership or contract change is requested. This verifies the worker branch on Mac at the 1080-by-1920 runtime UI surface, including actual event-handler transitions and rendered frames. Physical-device touch, other aspect ratios/safe areas, audio, save persistence, and acceptance of the combined other-worker integration were not exercised by this task and remain outside this report's claims.
