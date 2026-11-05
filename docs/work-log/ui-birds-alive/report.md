# Win-panel bird rendering repair

## Result and scope

Integrated `origin/t54-integration` (`a7679d97`) into `mac-t54f/t54` before diagnosing the reported missing birds. The repair changes **only `WinPanel.cs`** among runtime files. `PopupMotion.cs`, `UiBirdLife.cs`, modal layering, art/import settings, gameplay, persistence, existing tests and other workers' files remain unchanged by this repair.

The first normal-motion opening now renders the blue bird on the left edge, the pink bird on the right edge, and the yellow hero above the ribbon in every sampled tile from 1 s through 10 s. Their actual supplied cheer/hop/flap drawings change in those rendered frames. Three further normal-motion openings also passed rendered visibility checks, including an entrance interrupted at 0.2 s.

## Root cause

Both bird destination conversions used unary vector negation:

```csharp
Vector2 destination = -bird.rectTransform.anchoredPosition;
```

`anchoredPosition` has positive-right x and positive-up y, whereas the popup flight API consumes positive-right x and positive-down design y. Only y must change sign. Negating the entire vector sent all three birds off the **left** side of the screen. The two affected sites are `WinPanel.Emerge` and the hero flight destination in `WinPanel.Show`.

The fix constructs `(position.x, -position.y)` at both sites. It does not special-case capture resolution or species, elevate canvas sorting, force opacity, disable masks, or suppress the animation player. Existing flight timing, landing, looping drawings and cancellation remain intact.

The cold-opening diagnostic found normal renderer alpha and inherited alpha, valid nonzero image rectangles and no renderer cull, but negative screen x coordinates. Thus this defect was not caused by the settings/popup modal canvas layer or a transparent CanvasGroup. A preliminary run which opened the win panel in instant mode before the observed normal opening masked the sign error and rendered the birds; the failing reproduction and final acceptance deliberately use the **first** normal opening without that warm-up.

## Rendered evidence

All captures use the genuine Game scene and integrated resources in Unity 6000.6.7f1 PlayMode, a nonpersistent fixture, and a **540x960** camera render target. The scenario performs the first authored delivery and invokes `Win.Show(3, 30, 10)`, matching the reported evidence scenario. `GameMotion.Instant` is false throughout each observed win presentation.

Sampling uses `Time.captureDeltaTime = 1/60` and `Time.time` for deterministic animation-time observations: 0–4 s at 0.1 s targets, followed by 4.5–10 s at 0.5 s targets, 53 tiles in total. Labels and CSVs record actual game times; maximum sample lateness is one frame, 0.017 s at the CSV precision. This is real PlayMode rendering, not a claim of physical-device or wall-clock frame-rate performance.

- [`evidence/win-panel-before.jpg`](evidence/win-panel-before.jpg): reproduced missing birds on the integrated first normal opening.
- [`evidence/win-panel-after.jpg`](evidence/win-panel-after.jpg): corrected full 53-tile sequence. All three birds are visible in every tile from 1 s onward.
- [`evidence/win-panel-reopen.jpg`](evidence/win-panel-reopen.jpg): settled reopen, reopen after an entrance interrupted at 0.2 s, and another reopen.
- [`evidence/win-panel-before-samples.csv`](evidence/win-panel-before-samples.csv) and [`evidence/win-panel-after-samples.csv`](evidence/win-panel-after-samples.csv): positions, sprites, alphas, renderer state, and per-bird visible-pixel measurements.

Every JPEG is at most 1600 px wide. Contact sheets are cropped from real rendered frames; no bird or UI element is composited into them.

### Visibility, not just component sprite changes

For each of the 43 samples from 1–10 s, the temporary acceptance harness renders the visible frame, temporarily sets **one bird's own CanvasGroup alpha** to zero, renders again without advancing the game frame, and compares the two pixel buffers. It restores the alpha before proceeding. The acceptance threshold is at least 200 changed pixels per bird per sampled frame, with a summed RGB difference greater than 12. This distinguishes a changing but off-screen component from an actually rendered bird.

| Bird | Screen x before | Screen x after | Minimum visible pixels before | Minimum visible pixels after | Distinct sampled drawings after |
| --- | ---: | ---: | ---: | ---: | ---: |
| Left blue | -105 | 105 | 0 | 13,427 | 11 |
| Right pink | -443 | 443 | 0 | 13,694 | 8 |
| Yellow hero | -260 | 260 | 0 | 6,179 | 6 |

Before the fix, **all 129 bird/sample combinations** contributed zero visible pixels despite reporting changing sprite names. After the fix, **all 129 contributed visible pixels** and the temporary acceptance assertion passed. Reopening adds nine further per-bird pixel assertions, all passing.

Visible samples include all six `sky_cheer` drawings, all six `berry_cheer` drawings, all six `sunny_flap` drawings, and left/right hop drawings. The existing player still uses 10 fps cheer, 12 fps/0.5 s hop with alternating sides, and 14 fps hero flap. Sparse samples cannot establish every intervening frame; they establish that real changing drawings are now visible rather than merely assigned to invisible components.

## Capture command

The temporary `WinBirdCapture` PlayMode harness and its generated metadata were removed before the full retained suites. It did not modify permanent test sources or assembly definitions.

```sh
env BIRD_CAPTURE_PHASE=after flock /tmp/unity.lock python3 -c \
  'import subprocess,sys; sys.exit(subprocess.run(sys.argv[1:],timeout=600).returncode)' \
  /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -screen-width 540 -screen-height 960 \
  -projectPath /Users/apple/orca/workspaces/game/t54-ui-birds/PerchSort \
  -runTests -testPlatform PlayMode -testFilter PerchSort.PlayModeTests.WinBirdCapture \
  -testResults /Users/apple/orca/workspaces/game/t54-ui-birds/build/win-birds-after.xml \
  -logFile /Users/apple/orca/workspaces/game/t54-ui-birds/build/win-birds-after.log
```

The diagnostic cold-before run uses `BIRD_CAPTURE_PHASE=cold-before` and separate result/log paths. Both captures completed; only the after run enables the visibility and reopen assertions. Local full-resolution frames, capture logs and XML results remain ignored under `build/`.

## Verification and cleanup

| Check | Observed result |
| --- | --- |
| `python3 ci/check_current_game_names.py --self-test` before Unity | 8/8 passed |
| `python3 ci/check_current_game_names.py` before Unity | 1514 tracked text files, zero violations |
| Temporary corrected rendered PlayMode acceptance | 1/1 passed, zero failures/skips; all 129 settled bird samples visible and all nine reopen pixel checks passed |
| `flock /tmp/unity.lock bash scripts/run_tests.sh` after temporary-harness removal | Scene build succeeded; **312/312 EditMode**, **53/53 PlayMode**, zero failures/skips |
| Retained discovery safeguards | LevelsData 81 cases and Layout 41 cases preserved; no non-passing cases in either full-suite XML |
| `flock /tmp/unity.lock bash scripts/screenshots.sh` | Succeeded; stable captures under `build/screens/game/`; actual `win.png` inspected with all three birds visible |

Every Unity run used the worktree project and `flock /tmp/unity.lock`. Process state was inspected before starting; no user Editor or other worker's process was killed. Scene/meta/settings/package input checksums were recorded before Unity. Generated scene/import/settings output was not added to the repair commit or blindly restored.

The temporary capture source and metadata were deleted before the full-suite run. Existing tests were not edited, weakened, skipped or repinned; no permanent test outside the three-file runtime authorization was added. The temporary pixel-level acceptance is the direct regression proof, because transform/sprite-only assertions did not detect off-screen rendering.

## Requested changes elsewhere and boundaries

None required for this fix. The suspected `PanelsView`/`SettingsPanel` modal implementation and all other runtime files remain untouched. This delivery proves the specified first-opening, timed visibility, changing real drawings, and reopen/cancellation behavior at the tested render size. It does not claim physical-device testing, wall-clock frame-rate acceptance, or fixes to unrelated integrated surfaces.
