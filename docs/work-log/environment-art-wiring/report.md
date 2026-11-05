# Environment art wiring report

## Coordinator rework delivery

Merged the coordinator's integrated branch before implementation:

```bash
git fetch origin
git merge --no-edit origin/final-integration
```

The merge fast-forwarded the assigned branch `mac-environment-wiring/work` to integrated commit `3a466e00`. The prior verification-generated metadata, scene and Python bytecode changes were preserved in the named Git stash `Preserve environment verification generated state`; they were not reapplied over the integrated assets.

**All requested runtime corrections are implemented and the full serialized Unity suite is green. Exact closed-master image identity remains an asset prerequisite, and the integrated naming checks fail outside this assignment.** See the explicit remaining acceptance boundaries below.

Implementation changes remain inside `BranchView.cs`, `SceneView.cs`, `GameLayout.cs`, `MapView.cs`, `GateView.cs` and `EnvironmentArtTests.cs`. `MapModel.cs` and `MapMotion.cs` required no further edits. No changes to `BirdView.cs`, rules, levels, economy, saves, import policy or artwork files are included in the rework commit.

## Binding rework items

### Bamboo removal

Removed the complete runtime grove load/draw path. A search for `map_bamboo_w1` under `PerchSort/Assets/Scripts` returns no matches. `Resources/World/map_bamboo_w1.png` is intentionally unused and retained, as requested. The world map now shows the baked background path without the rectangular grove overlay.

### Door-strip removal

Available gate parts now replace the entire closed-gate image rather than being drawn beneath a whole-door overlay. Popup gate presentation remains unchanged; its assets are not used by the map.

The remaining top strip after the first parts assembly was the lower edge of an arch clipped by normal level-ten focus. Corrected the geometry instead of hiding it with an opacity or visibility special case:

- `MapStride` is 4000, leaving enough inter-world separation that the next arch is fully above the normal world-end viewport. The existing normal node focus ratio remains unchanged.
- Unlock camera positioning fits the union of the complete arch, current node and current chick, including its bounce envelope. Both sides of each world boundary are covered.
- Coordinates within each world remain unchanged. Existing native-scale background sprites and cached boundary canopy slices continue to fill the map.

The current-level-ten capture no longer contains a top door strip. No legacy full-height gate image or closed whole-gate overlay is drawn when the provided parts are available.

### Native gate assembly and timing

Worlds one through three use the documented asset placement on a 560 × 600 canvas:

- Frame behind the other parts.
- Light: 320 × 440 at top-left `(120, 130)`, behind the doors.
- Left leaf: 160 × 288 at `(118, 274)`.
- Right leaf: 160 × 288 at `(278, 274)`.
- Hinge axes are four pixels inside the outer edges: left pivot x=0.025, right pivot x=0.975. These are applied directly to the UI rectangles, so importer recentering cannot move the runtime axes.
- Existing standalone padlock alpha bounds are cropped at runtime and placed at `(242, 364)`, size 73 × 99. No artwork file is edited.

The left and right leaves use their different supplied drawings, never mirrored. Each scales x from 1 to 0.08 over 0.9 seconds with smooth ease-in-out. Light alpha reaches 1 over 0.6 seconds. Padlock alpha reaches 0 in the first 0.2 seconds. The static arch remains visible. The existing unlock callback, cancellation handling and persistence flow are retained. Missing optional parts still use the originally specified new-arch dissolve fallback, but the integrated supplied-art path exercises the actual swing.

The timed runtime sheet and log show both leaves opening together, the lock gone by the nominal 0.2-second sample, full light by 0.6 seconds, and x scale 0.08 with exactly one completion callback by 0.9 seconds.

### Equal trunk widths and junctions

Both sides now use the same trunk geometry, vertical extent and horizontal normalization. The right side is the exact static mirror of the left. Different crown widths no longer alter right-trunk height and source-row sampling. Crown positions remain unchanged; the source crown attachment remains at design x=25 on the left and its mirrored position on the right.

Lower bark is normalized around the production `TrunkVisibleWidth` of 170 design pixels. Junction inner ends use half that width as the trunk centre line; twig scaling remains 0.8. Bird slots, row positions, hit rectangles, capacities and growth/entrance behavior remain unchanged.

The three runtime densities have identical left/right rectangle sizes, y positions and mirror placement. Projecting the supplied alpha silhouettes through those captured rectangles gives equal widths on both sides, approximately 163–177 design pixels across the sampled lower-screen rows. Measurements are in [trunk-widths.log](evidence/trunk-widths.log).

## Verification

All Unity commands waited on `flock /tmp/unity.lock`. The primary checkout was not modified and no other worker's Editor was killed.

| Check | Observed result |
| --- | --- |
| Full serialized EditMode suite | 317 passed, 0 failed, 0 skipped |
| Full serialized PlayMode suite | 87 passed, 0 failed, 0 skipped |
| Permanent environment cases | 5 passed; case count unchanged |
| Graphics-enabled environment run | 6 passed, including temporary evidence capture |
| Stable screenshot command | Passed; 34 required 1080 × 1920 captures |
| Retained expanded level-data cases | 81 |
| Retained layout cases | 41 |
| Integrated naming self-tests | 7 passed, 2 failed, 9 total |
| Integrated naming guard | Failed: 1820 violations across 2527 tracked text files |

The full runner was executed after the final runtime changes and after removing the temporary capture case. No permanent test was removed, skipped or weakened. PlayMode discovery includes the additional features from the merged integration; the environment fixture still contains the same five permanent cases.

The environment checks cover all forty current-level profiles at peak node bounce, disjoint gate/node/chick bounds, unchanged branch-hit snapshots, all four worlds' trunk/junction assets, symmetric trunk geometry, junction root alignment and missing-limb growth fallback. Gate motion exercises both adjacent current-level profiles for every world boundary: levels 10/11, 20/21 and 30/31. It verifies complete gate/current-chick viewport visibility, fixed hinge positions, lock/light timing, positive leaf scaling, one callback and retention of the open pose after lifecycle cancellation. Normal world-end focus separately rejects clipped door strips.

### Commands

```bash
python3 ci/check_current_game_names.py --self-test
python3 ci/check_current_game_names.py
flock /tmp/unity.lock bash scripts/run_tests.sh
flock /tmp/unity.lock bash scripts/screenshots.sh
```

The two naming self-test failures concern historical-path exemption expectations and task-name path expectations in `ci/test_current_game_names.py`. The main guard reports existing identifiers throughout unowned runtime tests, art inventories, historical documents and tooling. The policy, its test expectations and those unowned files were not edited or exempted to obtain a pass. Both checks ran before Unity; their failure was reported, not silently bypassed or described as green. Full guard output is preserved in ignored `build/naming-guard.log`.

The graphics-enabled timed run used:

```bash
flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -projectPath /Users/apple/orca/workspaces/game/environment-wiring/PerchSort \
  -runTests -testPlatform PlayMode \
  -testFilter PerchSort.PlayModeTests.EnvironmentArtTests \
  -testResults /Users/apple/orca/workspaces/game/environment-wiring/build/environment-results.xml \
  -screen-width 1080 -screen-height 1920 \
  -logFile /Users/apple/orca/workspaces/game/environment-wiring/build/environment-capture.log
```

The temporary case used the real game root, retained Fredoka font, synthetic nonpersistent profiles, actual camera rendering and real-time gate motion. It was removed before the full suite. Full XML and Unity logs remain under ignored `build/`; [verification.log](evidence/verification.log) records exact counts and the passing environment cases. Unity-generated metadata/scene changes remain outside the commit and are preserved separately in ignored `build/unity-generated-state.patch`, rather than being blindly restored.

## Current evidence

The canonical evidence files were refreshed in place; previous captures remain only in Git history.

- [Sparse board beside approved board](evidence/board_sparse_comparison.jpg): authored level 1, two rows.
- [Medium board beside approved board](evidence/board_medium_comparison.jpg): authored level 5, three rows.
- [Dense board beside approved board](evidence/board_dense_comparison.jpg): authored level 40, five rows.
- [Map at current levels one, four, ten and eleven beside the approved map](evidence/map_comparison.jpg).
- [Real gate swing at nominal 0.1-second intervals beside the approved map](evidence/gate_opening_sheet.jpg).
- [Runtime timestamps, leaf scales, light/lock alpha, callback count and trunk transforms](evidence/runtime-capture.log).
- [Source-layer closed-master comparison measurements](evidence/closed-gate-source-comparison.log).

Raw runtime frames remain at 1080 × 1920. Timing-log elapsed values are recorded after camera readback, roughly 0.08 seconds after the corresponding pose sampling; the sheet labels are the nominal sample times. Stable screenshots alone are not used to establish animation timing.

## Remaining acceptance boundaries

1. **Exact closed-master identity is not met by the supplied artwork.** The asset owner's [separated-parts report](../environment-art/report.md#follow-up-separated-gate-parts) and [comparison](../environment-art/evidence/gate_parts_comparison_w1.jpg) explicitly describe different grain, frame registration, studs and padlock shape. A native-size source-layer assembly at the documented positions differs from the three closed masters by mean absolute RGBA values of approximately 17.23, 17.45 and 19.70 on a 0–255 scale. These are source-image measurements, not runtime screenshot metrics. Correct native placement and animation cannot make different drawings identical. Matching source layers or artwork authority is required to satisfy that final visual clause. No whole-master overlay is used to conceal the mismatch.
2. **Naming checks are not green on the merged integration.** Repairing the policy/test mismatch and the 1820 unowned findings is outside this bounded wiring assignment. Unity compilation and both full suites are green independently of that failure.
3. No device, audio-listening or complete bird-animation acceptance is claimed by these environment captures. Existing board/junction artwork and inherited house/HUD composition were not redesigned under this rework.
