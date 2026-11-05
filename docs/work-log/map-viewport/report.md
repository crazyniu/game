# T54I — Map viewport, focus and clipping report

## Delivery

Branch: `mac-t54i/t54`, created from the supplied contract tip before other work. No merge, release, artwork edit, persistence change or gameplay change.

Owned implementation changes:
- `PerchSort/Assets/Scripts/Game/MapView.cs`
- `PerchSort/Assets/Tests/PlayMode/MapViewportTests.cs` and its Unity metadata
- This report and `docs/work-log/map-viewport/evidence/`

`MapModel.cs`, `MapMotion.cs`, `GateView.cs` and `LeafTransition.cs` did not require changes. Existing tests were not edited or weakened.

## Problems and changes

### I1 — Bottom bar collisions

The old scroll mask covered the entire design canvas. Nodes and stars could render and receive taps behind the bottom controls. The new viewport ends at design y=1712, 40 pixels above the controls at y=1752. A soft 48-pixel vertical mask edge fades path content before the reserved zone. Both visible clipping and mask raycast rejection apply to the path.

Scenery is a separate, non-interactive scrolling layer beneath the reserved HUD/bar areas. It follows the same content offset. Existing endpoint background sprites continue beyond the authored map ends using the existing 160-pixel overlap convention; no artwork or import settings are changed.

### I2 — Final chick clipping and focus

The viewport starts at y=166: below the larger HUD bottom edge (134), plus a 32-pixel margin. Its full rect is `(0, 166, 1080, 1546)`.

The path has 400 pixels of top padding. Its clamped offset range is 0–10374. Focus uses the actual inset viewport height and places the target node at 40% from the viewport top, subject to endpoint clamping. Level 40 now reaches that focus position with its whole chick and bounce clearance visible. Level 1 remains fully visible at the lower clamp. Gate focus uses the same padded coordinate system.

### I3 — Final node spacing

Node diameters, authored node coordinates, number sizes, star sizes and star pitch are unchanged. The inset mask resolves the reported collision without shrinking nodes or changing their authored spacing. PlayMode coverage checks actual label/star bounds between adjacent levels 34–40.

### I4 — Duplicate gears

Both controls remain: `Map settings` at the top right and `Settings` in the bottom bar. Neither was removed, repositioned or resized.

## Regression and verification

Unity executable: `/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity`.
All Unity commands used `/tmp/unity.lock` and this worktree's `PerchSort` project path.

| Check | Result |
| --- | --- |
| `python3 ci/check_current_game_names.py` | PASS: 569 tracked text files before tests; 571 after staging the deliverable; zero violations in both runs |
| Targeted PlayMode regression before the fix | 0/4 passed; all four failed because the scroll viewport intersected the HUD zone |
| First `flock /tmp/unity.lock bash scripts/run_tests.sh` | EditMode 304/304; PlayMode 25/25; zero failures/skips |
| Final `flock /tmp/unity.lock bash scripts/run_tests.sh`, after scenery correction and stronger label/star checks | EditMode 304/304; PlayMode 25/25; zero failures/skips |
| Graphics-enabled Unity capture at current levels 1, 4, 10, 11, 34, 40 and both clamped ends | PASS; reviewed the rendered images |

The final suite includes all 21 pre-existing PlayMode cases plus four new cases. The scene builder completed in both full runs.

The targeted regression command was:

```sh
flock -w 900 /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$PWD/PerchSort" \
  -runTests -testPlatform PlayMode -testFilter PerchSort.PlayModeTests.MapViewportTests \
  -testResults "$PWD/build/viewport-before.xml" -logFile "$PWD/build/viewport-baseline-tests.log"
```

The new PlayMode scenarios use nonpersistent profiles at current levels 1, 10, 34 and 40. Each checks the focused chick/node, both scroll endpoints, all forty node-centred offsets, clipped node/number/star bounds against the HUD and bottom-bar zones, off-viewport raycast rejection, and final-stretch decoration separation. Offscreen content is measured after viewport clipping: raw offscreen RectTransforms can traverse reserved zones, but cannot render there or receive taps there.

Local logs/results:
- `build/viewport-before.xml` and `build/viewport-baseline-tests.log`: failing-before regression.
- `build/scene-build.log`: final scene build.
- `build/test-results.xml`, `build/editmode-tests.log`: final EditMode run.
- `build/playmode-results.xml`, `build/playmode-tests.log`: final PlayMode run.
- `build/map-evidence.log`: final successful evidence capture.

### Initial validation interruption

The initial test invocation encountered this temporary capture-helper compile error:

```text
Assets/Tests/EditMode/T54IMapCapture.cs(48,38): error CS0104: 'SafeArea' is an ambiguous reference between 'PerchSort.Game.SafeArea' and 'UnityEngine.UI.SafeArea'
```

Unity then remained open rather than exiting the test invocation. After the lock-blocker notification, I terminated only my own Unity PID 20814, releasing the lock. The helper was corrected to `PerchSort.Game.SafeArea`; subsequent compilation, captures and tests completed successfully. No user Editor or peer process was terminated. The temporary helper and its metadata were removed from the deliverable.

## Visual evidence

All committed evidence JPEGs are 1200 pixels wide, below the 1600-pixel limit:
- `docs/work-log/map-viewport/evidence/map-contact.jpg`: approved reference, all six requested current levels, both scroll ends.
- `docs/work-log/map-viewport/evidence/level-1-compare.jpg`
- `docs/work-log/map-viewport/evidence/level-4-compare.jpg`
- `docs/work-log/map-viewport/evidence/level-10-compare.jpg`
- `docs/work-log/map-viewport/evidence/level-11-compare.jpg`
- `docs/work-log/map-viewport/evidence/level-34-compare.jpg`
- `docs/work-log/map-viewport/evidence/level-40-compare.jpg`
- `docs/work-log/map-viewport/evidence/endpoints.jpg`: clamped top and bottom.
- `docs/work-log/map-viewport/evidence/before-after.jpg`: original final-chick clipping/bar collision versus the corrected final-level viewport.

The approved reference is preserved at `art/v7/ui7_map.png`; the assignment's literal `ui_map.png` filename is not present in this checkout. Each comparison labels that existing approved original separately from the Unity capture. Its artwork, layout differences and existing runtime icon differences were not changed by this task.

Capture used a temporary `PerchSort.EditModeTests.T54IMapCapture.Run` entry point under the owned new-test filename pattern:

```sh
flock -w 900 /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit -projectPath "$PWD/PerchSort" \
  -executeMethod PerchSort.EditModeTests.T54IMapCapture.Run \
  -screen-width 1080 -screen-height 1920 -logFile "$PWD/build/map-evidence.log"
```

The helper opened the sole Game scene, called `GameRoot.Initialize(false, false)`, showed synthetic profiles with stars only below the requested current level, and rendered the real camera to a 1080×1920 RenderTexture. Endpoint captures set `verticalNormalizedPosition` to 1 and 0 and explicitly dispatched the ScrollRect value-change event normally emitted by LateUpdate, since this capture runs in edit mode. Raw PNGs remain under `build/screens/map-viewport/`; baseline PNGs remain under `build/screens/map-before/`. The helper is intentionally not retained.

## Limits and generated workspace state

- Static Unity camera captures prove layout/clipping, not animated playback, physical-device safe areas or manual mouse/touch acceptance. No bird-action or timed-animation change is claimed.
- Unity import/build changed seven pre-existing bird overlay `.png.meta` files and `Assets/Scenes/Game.unity` in the working directory. The interrupted test run also left an untracked `InitTestScene` asset and metadata. These are outside task ownership, were not blindly restored, and are excluded from the commits. The report does not authorize their integration.
- Build outputs are not committed. Original art and current resource files are unchanged by the owned patch.

## Requested contract change

The old full-screen `GameLayout.MapViewport` field is now unused. `GameLayout.cs` is outside T54I ownership, so it was not edited. The integration owner should remove that obsolete field after integrating this patch; the runtime already uses the private inset derived from the existing canonical HUD/button rectangles. No out-of-scope change is required for the delivered behavior or passing tests.
