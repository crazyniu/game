# Continuous map background wiring

## Scope and implementation

Issue #82. Changes are limited to `MapView.cs`, the map constants in `GameLayout.cs`, `MapModel.cs`, new `Tests/PlayMode/MapSeamTests.cs` and its Unity metadata, and this report/evidence directory. No node authoring, levels, rules, saves, artwork, importer or existing tests are changed.

- The four backgrounds now load `Art/map_bg_continuous_w1` through `Art/map_bg_continuous_w4`.
- `MapOverlap` is 160 px; `MapStride` is `MapSegmentHeight - MapOverlap`, or 2840 px. Four 3000 px strips occupy 11520 px. Existing 400 px content padding remains, yielding 11920 px scroll content.
- Every `MapNodes` coordinate is unchanged. `WorldTop` continues to derive from stride, so nodes stay at exactly the same position within their own strip.
- Removed all three boundary canopy images, their dynamically cropped sprites and their disposal path. Full strips overlap directly; no replayed canopy remains.
- Retained the top and bottom continuations, now using the continuous snow and meadow resources. The art delivery explicitly preserves the outer transparent bands and continuation edges; removing these images would expose the backdrop above the snow strip and through its alpha-faded top. Continuation placement uses the shared overlap constant.
- Stones derive from consecutive authored node positions. Foreground corners derive from the focused scroll offset. Scroll height, maximum offset, focus and clamps derive from the new map height; none retains the old stride. The 0.4 focus ratio and viewport/bar margins remain unchanged because their screen-space geometry is independent of world stride.

## Gate placement constraint

After direct overlap, consecutive boundary node centres are 500 px apart instead of 1660 px. The original 600 px tall gate cannot fit between their 240 px current-node bounds, even before reserving a chick silhouette. Moving nodes would violate the fixed strip-local coordinates.

The map gates therefore retain their aspect ratio but use 320 × 342.857 px bounds. Their centre is x=180, y=`WorldTop(lowerWorld) + MapOverlap / 2`: beside the path in the shared boundary band. Their right edge is x=340, clear of boundary node faces and the current chick. Signs continue to follow gate placement. Gate artwork, part layout, open/closed behavior and progression policy are unchanged.

## Behavioral coverage

`MapSeamTests` exercises real `MapView` instances in PlayMode:

- All 40 rendered node holders fit their own full strip; adjacent rendered strip bounds share exactly `GameLayout.MapOverlap` with no gap.
- All three rendered gate rects straddle their overlap-adjacent band and remain clear of every node and the current chick for current levels 10, 11, 20, 21, 30 and 31.
- The same boundary cases exercise real non-instant gate opening: the gate, current node and chick fit during the opening; completion occurs once and scrolling is restored.
- Every level can be focused with its complete node and chick, including bounce allowance, inside the actual viewport. Levels 1 and 40 are also checked at the physical bottom and top ScrollRect clamps.

Existing map tests are intentionally unchanged. `MapViewportTests` retain their HUD/bar clipping, raycast, focus-ratio, star/label and extreme-scroll coverage and passed. Two assertions in the unowned `EnvironmentArtTests.cs` conflict with the new gate geometry; exact failures are recorded below rather than weakened or silently changed.

## Importer integration boundary

`GameArtImporter.Configure` selects 4096 only for `map_bg_w` and `bg_level_` names, not `map_bg_continuous_w`. The art owner's report already identified this limitation. The importer is outside this worker's owned files and is intentionally unchanged. The coordinator must extend the existing tall-map predicate to include `map_bg_continuous_w` and reimport the four derived resources before full-resolution integrated acceptance. No runtime loading workaround or source-art modification was introduced.

## Commands and results

- `python3 ci/check_current_game_names.py --self-test`: 9 passed.
- `python3 ci/check_current_game_names.py`: 186 tracked text files inspected before Unity, zero violations.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`: scene build succeeded; **317/317 EditMode passed**; **97/99 PlayMode passed, two failed**, zero skipped or inconclusive. Retained LevelsData and Layout counts are 81 and 41. All three new seam cases passed in the full run.
- After adding boundary gate-opening/focus checks to the seam coverage, ran the serialized Unity PlayMode runner with `-testFilter PerchSort.PlayModeTests.MapSeamTests`: **3/3 passed**, zero failures/skips. This exercised all six boundary-current cases through actual non-instant gate opening and completion. Production code did not change after the full run.
- Capture command: `flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath "$PWD/PerchSort" -executeMethod PerchSort.Editor.MapSeamCapture.Run -screen-width 1080 -screen-height 1920 -logFile "$PWD/build/map-seam-capture.log"`. The temporary harness followed `GameCapture`'s camera/render-texture path and initialized the real scene/root with synthetic nonpersistent profiles. It was removed after capture.
- Initial build/capture attempts stopped at a temporary harness compile error: ambiguous `SafeArea`. Qualified `PerchSort.Game.SafeArea`, then reran successfully; no suite ran in the failed build attempt. The first whole-scroll mosaic exposed missing immediate ScrollRect event dispatch in edit-mode capture; the harness was corrected and all captures regenerated. Only corrected evidence is committed.

The two full-suite failures are:

1. `EnvironmentArtTests.GatesAndSceneryNeverCoverNodesOrTheCurrentChick`, line 61: requires the next gate's bottom to be above the viewport at normal world-end focus (`>=773`, observed `263.171051`). That expectation depended on the removed canopy spacing. The new gate is fully visible beside the shared band, not a clipped door strip. The new seam coverage verifies nonintersection with every node and the current chick.
2. `EnvironmentArtTests.MapGateOpeningCompletesOnceAndKeepsItsOpenPose`, line 210: pins each door part to native 160 × 288 px (`91.43 × 164.57` observed after proportional map-gate sizing). The unchanged `GateView` scales all parts to the map gate's bounds. The added seam check exercises the complete transition without pinning incidental part dimensions.

No existing assertion was changed. Reconciling these unowned assertions requires coordinator authority; this is **not a green full-suite delivery**. PlayMode shutdown also reported three persistent allocation leaks. License/telemetry connection warnings did not prevent completed runs. Full result attributes, failing messages, targeted result attributes and XML hashes are in [unity_verification.json](evidence/unity_verification.json).

## Visual evidence and limits

- Nine 1080 × 1920 JPEGs: `evidence/map_level_{1,9,10,11,20,21,30,31,40}.jpg`.
- Approved `art/approved_ui/ui_map.png` beside the requested captures in [early](evidence/map_comparison_early.jpg), [middle](evidence/map_comparison_middle.jpg) and [upper](evidence/map_comparison_upper.jpg) comparison sheets.
- [whole_map.jpg](evidence/whole_map.jpg): 270 × 2980 downscale of an actual 1080 × 11920 runtime-UI scroll mosaic, covering offset 0 through 10374. Viewport top=166, height=1546, bottom=1712. Nine overlapping viewport captures are assembled using their observed scroll offsets; interior soft-mask edges are excluded. This is not an offline artwork stack.
- [capture_manifest.json](evidence/capture_manifest.json) records raw screenshot hashes, dimensions and observed scroll geometry. Lossless originals and full-size mosaic remain under ignored `build/screens/map-seams/`.

Inspected all three comparison sheets, the full-scroll render and the level-10 boundary at source scale. The path and biome transitions are continuous without the former canopy replay or horizontal gap; boundary gates and current nodes/chicks are separate and visible. The retained outer continuations cover the scroll endpoints without exposing a hard image edge. The approved plush forest treatment and control placement remain, but this is not a claim of pixel-identical reference composition: current-level/progress state and biome differ, and gates now sit beside the path to preserve fixed node coordinates.

The capture log independently confirms **737 × 2048 imported textures for all four continuous strips**. These screenshots establish wiring/layout, not full-resolution source-art acceptance. The importer fix and the two existing assertion conflicts remain integration boundaries. No device, listening, persistence or timed bird-animation acceptance is claimed. Stable captures use instant motion; the PlayMode gate transition check is behavioral proof, not a committed animation frame sequence.

## Serialized-state preservation

Recorded hashes and copies of 1591 tracked scene/meta/settings/package files before Unity. After the final run, inspected and preserved generated differences under ignored `build/unity-after/`, then restored 786 generated files from that exact pre-run snapshot: 768 whitespace-only rewrites and 18 substantive changes (four map size-limit rewrites, seven expanded bird-overlay metadata files, six gate-door pivot rewrites and the rebuilt scene). All 1591 snapshot hashes match again. The new test's generated metadata is retained; the temporary capture source/metadata and evidence assembly script were removed. The primary checkout was not touched.

Full Unity suites ran once after the temporary harness compile fix; the final targeted run covers the added gate-transition assertions. Restoring out-of-scope generated metadata does not remove the importer dependency: the unchanged importer will reproduce the observed downsampling on import.

Final naming guard after staging the new test/evidence: 187 tracked text files inspected, zero violations.
