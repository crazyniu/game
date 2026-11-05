# Branch limb repair

## Result

Runtime changes are confined to `Assets/Scripts/Game/BranchView.cs` and `GameLayout.cs`. Branches now use the supplied limb texture at a uniform native aspect ratio, without the former thickness amplification or procedural bend. Bird slots, hit rectangles, authored levels, Core rules, persistence, resource PNGs and importer settings were not edited.

### Diagnosis verified

- The previous mesh multiplied thickness by `1.55 + 1.2 * root * root`, reaching 2.75 at the root. Before screenshots reproduce the oversized wavy silhouette.
- Each junction contains its own thin horizontal stub. The previous left junction occupied x=85–325 and exposed this stub above the limb and beyond short roots.
- Actual checked-out PNG canvases are **620×90 normal**, **400×80 twig**, and **240×220 junction**, rather than a normal limb canvas with a 150-pixel root. The excessive multiplier remains the confirmed problem.
- The tall upward forks above the first row belong to `trunk_curved_w<N>` itself, not to junction children. Those existing trunk silhouettes remain unchanged. This repair does not claim a whole-screen match to every feature of the approved illustrations.

### Rendering decisions

- Normal display: 530×76.93549, exactly the same 530/620 scale in both axes. Twig display: 400×80, native scale.
- Growth reveals the root-side texture instead of squeezing the complete image into a shorter width. A rounded geometric clip closes a shortened tip; UVs follow the clipped geometry, so bark is neither stretched nor thickened. At full length the native silhouette is retained.
- Entrance also reveals fill from the root, rather than compressing the flex transform horizontally. The junction fades in without changing its proportions. Existing timings and growth `Ease.OutBack` remain unchanged.
- Growth starts at the previous fill immediately, avoiding a full-end-length flash before the first tween update. Cancellation restores the current seat-derived length and normal junction opacity.
- Normal junction canvases now begin at the screen-edge root, centered at x=120 on left branches and mirrored on right branches. Downward center offsets are 40, 32, 32 and 14 design pixels for the four worlds. The visible bark joins the trunk while the horizontal stub is covered by the limb.
- **Twig junctions are disabled, in every world.** The normal junction's stub does not fit the narrower twig silhouette. The single junction object is retained inactive for reuse when a twig upgrades to a normal branch; no junction is rendered on a twig. No art was regenerated or modified.

## Evidence

All paths below are relative to this report. Source captures were actual Unity PlayMode camera renders at **1080×1920**, graphics enabled, nonpersistent profiles, `Random.InitState(12345)`. Each comparison preserves two full-resolution 1080×1920 panels, with a 48-pixel label strip above them; the composed JPEG is 2160×1968. JPEG quality is 88, without chroma subsampling.

| Board | Before / after |
| --- | --- |
| Level 1 | [Comparison](evidence/level1-before-after.jpg) |
| Level 5, three rows | [Comparison](evidence/level5-before-after.jpg) |
| Level 40, five rows and short growing branches | [Comparison](evidence/level40-before-after.jpg) |
| Level 31, growing branches | [Comparison](evidence/level31-before-after.jpg) |
| Level 21, twig | [Comparison](evidence/level21-before-after.jpg) |
| Level 4, initial board | [Comparison](evidence/level4-before-after.jpg) |
| Level 4, after actual Sapling booster use | [Comparison](evidence/level4-sapling-before-after.jpg) |

Approved illustrations are placed beside the repaired runtime in [level 5](evidence/level5-approved-comparison.jpg) and [level 40](evidence/level40-approved-comparison.jpg). The repaired limb silhouette is a single thinner tapered perch, rather than an amplified wavy slab plus exposed stub. HUD, bird art, crowns, existing trunk forks and other unrelated differences are not redesigned.

[Growth contact sheet](evidence/branch-growth.jpg) and [timestamps](evidence/branch-growth.csv): authored level 31, empty left growing branch index 6, direct production `BranchView.Grow` call from two to three seats with normal LitMotion playback. Native-resolution crop x=0–580, y=1460–1625 from each full-size render; samples at 0.0–0.6 seconds every 0.1 seconds. `Time.captureDeltaTime=1/60` provides deterministic **game-time**, not wall-clock performance evidence. Fill progresses from 0.5188679 to 0.726415, with the retained OutBack overshoot to 0.744616 at 0.2 seconds. Completion is observed by the 0.5-second sample. This is direct branch-presentation evidence, not a homecoming cascade or physical-input recording.

[Before geometry](evidence/before-geometry.csv) and [after geometry](evidence/after-geometry.csv) record sprite, rectangle and fill for the requested initial boards. They describe image rectangles, not the old amplified mesh bounds.

## Regression coverage

`Assets/Tests/PlayMode/BranchLimbTests.cs` adds two behavior tests:

- All 40 authored levels, 288 branch instances: exactly one active native limb image, at most one active junction, no rendered twig junction, native width/height ratio, native UV-to-geometry scaling in both axes, root-origin bounds and rounded shortened tips.
- Entrance reveal and normal-time growth: no transform compression, fixed sprite rectangle and root placement, growing fill, cancellation at the new logical length, and no completion callback after cancellation.

The 12-pixel scenery bound is checked against the **revealed** limb width. Junction overlap is exempt as requested. Transparent hit surfaces, seat-highlight glow, swivel controls, and transient growth/falling-leaf particles are intentionally not treated as stray limb scenery.

Existing `EnvironmentArtTests` retain world-art, mirror, seat, fallback and growth checks; obsolete absolute junction-offset, horizontal-collapse and always-visible-junction assertions were deleted, not repinned. `LevelEntranceTests` now observes the limb's actual fill instead of the obsolete flex-x compression when asserting that branches remain hidden before their entrance phase. No tests were skipped or discovery safeguards lowered.

## Verification procedure and intermediate failures

The initial worktree was clean. Indexed scene/resource/settings/package checksums and the process/lock check were recorded before Unity. The user's primary Editor was open initially; the user closed it before the first launch. No user's Editor or other process was terminated. Every Unity invocation used `flock /tmp/unity.lock`.

Capture invocation parameters:

```text
flock /tmp/unity.lock env BRANCH_CAPTURE_STAGE=before /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath /Users/apple/orca/workspaces/game/branch-limb-fix/PerchSort -runTests -testPlatform PlayMode -testFilter PerchSort.PlayModeTests.BranchEvidenceCapture -testResults /Users/apple/orca/workspaces/game/branch-limb-fix/build/branch-before-results.xml -screen-width 1080 -screen-height 1920 -logFile /Users/apple/orca/workspaces/game/branch-limb-fix/build/branch-before.log
```

The final after capture used `BRANCH_CAPTURE_STAGE=after`, the same capture filter and dimensions, and `build/branch-final-capture-results.xml` / `build/branch-final-capture.log`. The temporary capture fixture and its metadata were removed before the full suite.

Observed intermediate results, not hidden retries:

- First baseline launch failed compilation because the new test initially called an unavailable `CanvasRenderer.GetMesh(mesh)` overload. Corrected before the passing baseline capture: **1/1**.
- First after capture succeeded, but its combined focused run had **1/3 passed, 2 errors** from ambiguous reflection over `OnPopulateMesh`. The test now selects the `VertexHelper` overload explicitly.
- One headless focused launch crashed before test results, exit 139, in Unity/Mono named-pipe/reflection startup (`build/branch-focused.log`). No product assertion ran in that attempt.
- Graphics-enabled focused retry: **6/7 passed**; the remaining failure classified the live seat glow as static limb scenery. The bound check now explicitly excludes that transient highlight, alongside existing controls and particles.
- Final headless focused filter `PerchSort.PlayModeTests.BranchLimbTests;PerchSort.PlayModeTests.EnvironmentArtTests;PerchSort.PlayModeTests.LevelEntranceTests`: **14/14 passed**, zero failures/skips, `build/branch-contract-results.xml`.
- Final graphics-enabled capture: **1/1 passed**, zero failures/skips, `build/branch-final-capture-results.xml`.

No physical device, physical pointer, audio listening or wall-clock frame-performance acceptance was performed. Stable screenshots use instant setup; only the growth contact sheet establishes timed branch motion.

## Final serialized acceptance

Executed once after removing the temporary capture fixture:

```bash
python3 ci/check_current_game_names.py --self-test
python3 ci/check_current_game_names.py
flock /tmp/unity.lock bash scripts/run_tests.sh
```

- Naming self-tests: **9/9 passed**. Naming guard before Unity: **188 tracked text files, zero violations**.
- Sole-scene build and compilation: **passed**.
- Full EditMode: **317/317 passed**, zero failures or skips, `build/test-results.xml`.
- Full PlayMode: **103/103 passed**, zero failures or skips, `build/playmode-results.xml`.
- Retained discovery safeguards were unchanged; the new limb fixture contributes two PlayMode cases.

Unity import/build left hundreds of tracked resource `.meta` changes and a generated `Assets/Scenes/Game.unity` change in this initially clean worktree. They are **not part of the repair commit** and were preserved, not blindly restored. The startup crash diagnostic was moved to ignored `build/branch-focused-crash.json`. Only the assigned runtime changes, affected tests, new test metadata, specification, report and evidence are committed.

After staging the new fixture, `python3 ci/check_current_game_names.py` inspected **189 tracked text files**, again with **zero violations**.
