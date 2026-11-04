# T53D — Map and world gate report

## Delivery status

Branch: `mac-ui-map/t53`. Implementation commit: `30831f4`.

**Reachable presentation changes delivered; D10/D11 are not an exact approved-image match.** Actual Unity renders were compared and iterated. The remaining art/layout discrepancies are listed below rather than counted as fixed. No merge or main-branch change was made.

Only `PerchSort/Assets/Scripts/Game/MapView.cs` changed in production. `MapModel.cs`, `MapMotion.cs`, `GateView.cs`, `LeafTransition.cs`, progress/unlock rules, storage, originals, resources, shared geometry, scene source, and existing tests were intentionally unchanged. The coordinator subsequently authorized the committed review images under `docs/work-log/map-fidelity/evidence/`.

## Changes by defect

### D10 — Nodes, current marker, boundary gates

- Node canvases: completed/open 220 px, current 240 px, locked 200 px; aspect-preserved stone/wood sprites retained. Number font increased to 100 px with a brown outline.
- Completed nodes retain the real earned/unearned star policy, with larger 48 px stars arranged as a trio. Level-four fixture shows three completed nodes with their three earned gold stars.
- Current node has a golden halo and two sparkles. Chick canvas is 310 px to account for the delivered frame's transparent margins. It is drawn after the face so its feet visibly overlap the upper rim instead of being hidden behind the wood.
- Boundary gate canvas increased from 364 to 532 px, giving approximately 444 px of visible arch width; the side sign moved outward to avoid the enlarged arch. Existing locked/open door states and opening choreography retained.
- Existing current-level selection, scroll clamping and animated focus retained. A reference-height focus attempt was rejected after its runtime screenshot put level 1 under the bottom controls. Restoring the existing focus ratio keeps the three completed nodes above those controls in the level-four fixture.

### D11 — Bottom navigation

- Retained the measured 152 px purple/gold discs and the three authored bottom-bar centers at y=1828.
- Enlarged the bottom basket, aviary and gear icon canvases from 62% to 76% of the disc size.
- Retained both settings entries, real button actions, modal disabling, pressed sprites and navigation behavior. No forced enabling or modal bypass.

## Runtime evidence

Approved image is always on the **left**; actual Unity render is on the **right**. These are stable-state captures, not timed-animation or device acceptance.

Committed review JPEGs are **1080 px tall, JPEG quality 80**:

| Fixture | Committed comparison |
| --- | --- |
| Original new-game runtime | [Before](evidence/map_before_comparison.jpg) |
| New game, level 1 current | [Current 1](evidence/map_current_1_comparison.jpg) |
| Level 4 current, levels 1–3 earned three stars | [Current 4](evidence/map_current_4_comparison.jpg) |
| World end, level 10 current, locked gate | [Current 10](evidence/map_current_10_comparison.jpg) |
| Level 11 current, naturally open previous-world gate | [Current 11](evidence/map_current_11_comparison.jpg) |
| Extra top-clamp boundary, level 40 current | [Current 40](evidence/map_current_40_comparison.jpg) |
| Existing gate-opening completion | [Open gate](evidence/map_gate_open_comparison.jpg) |

Full-resolution comparisons: `build/screens/t53d/*_comparison.jpg`.
Runtime sources: `build/screens/game/map_current_{1,4,10,11,40}.png`, `map_gate_open.png`, plus the standard capture set. Comparison helper is outside the repository at `/tmp/t53d/compare.py`.

## Verification

All Unity launches used `/opt/homebrew/opt/flock/bin/flock /tmp/unity.lock`. No Unity process was present at the initial process check; no Editor was killed. Bare `flock` was not on the session PATH; Homebrew reported it already installed, and its absolute binary path was used.

| Command / scenario | Result |
| --- | --- |
| `python3 ci/check_current_game_names.py` before suites | PASS: 567 tracked text files, 0 violations |
| `flock /tmp/unity.lock bash scripts/run_tests.sh` using the absolute flock path | Scene build PASS; EditMode **304/304 passed**, 0 failed/skipped; PlayMode **16/17 passed**, 1 failed, 0 skipped; script exit 1 |
| `flock /tmp/unity.lock bash scripts/screenshots.sh` using the absolute flock path, baseline and final | PASS; all 34 required 1080×1920 captures produced |
| Temporary owned EditMode capture fixture, graphics enabled, `-testFilter PerchSort.EditModeTests.T53DMapCapture` | PASS **1/1**, 0 failed/skipped; current levels 1, 4, 10, 11, 40; current node visible to card-origin lookup; all 40 node selection states checked against each synthetic profile; gate-open callback completed |

PlayMode failure: `PerchSort.PlayModeTests.GameplayTests.TimedFlockArrivalsReachTheirGoalDoor`, `GameplayTests.cs:405`: expected visible-flock distance `< 40`, observed `45.1668015`. This exercises board-to-house flight, outside D's file ownership. It was reported to the coordinator; neither the flight code nor assertion was changed, and the failing check was not rerun. XML/logs: `build/test-results.xml`, `build/playmode-results.xml`, `build/{scene-build,editmode-tests,playmode-tests}.log`.

Capture harness bring-up had two corrected harness-only failures: its initial direct TMPro reference was unavailable to the EditMode assembly, and restoring an empty scene setup failed after captures. The final harness reused the existing capture method and its loaded-scene guard, passed, then was removed together with its generated metadata before the retained full suites. No permanent test or assembly change remains.

Unity left uncommitted reserialization changes in `Assets/Scenes/Game.unity` and seven bird overlay `.png.meta` files (`chick_fluff`, five egg frames, `mama_bow`). These are outside D's ownership, were not staged or blindly restored, and remain in this isolated workspace for coordinator inspection. They are not part of the branch delivery.

## Remaining differences / Requested contract change

1. **Background and placement, D10:** `Resources/Art/map_bg_w1.png` is a top-down, repeated broad winding path with ten measured placements. The approved `ui_map.png` has a perspective six-node composition, bamboo/sky across approximately y=0–520, stepping stones, larger foreground flowers/bushes and different tree/rock placement. These elements are not present in the delivered world-one map. The level-four reference gate is consequently not visible at that viewport in the actual ten-level world. Matching this requires an approved replacement background and corresponding shared placement/viewport contract, not moving the real world-unlock boundary to level 6. No artwork was generated or modified.
2. **Gate artwork, D10:** `gate_arch`, `gate_door_left/right`, `gate_locked/open` depict a full-height door with a different vine/lock treatment. The approved gate region x≈484–928, y≈146–510 has a flowered arch, open archway above shorter planks and a larger central padlock. Delivered gate parts do not supply that design. Request matching aligned gate parts from the art owner; retained animation was not replaced with a whole static image.
3. **Icons, D11:** `icon_basket.png` is a woven handled basket rather than the approved shopping-basket silhouette. `icon_aviary.png` is a framed bird portrait rather than the approved house outline with bird. Their approved regions are x≈214–366 and x≈460–612, y≈1752–1904. Increasing scale cannot correct those silhouettes. Request approved icon assets; no substitute art was generated.
4. **Other visible differences:** delivered chick pose, star/node rendering and shared number/coin typeface differ from the reference. The extra level-40 fixture exposes the existing upper-clamp crop of the chick; its node remains visible. Edge nodes can pass behind fixed bottom controls while scrolling. The map still uses the existing shared focus/clamp and background-stack contract; no hidden alternate layout was introduced.
5. **Acceptance boundary:** exact visual acceptance remains blocked by the above assets/layout; required PlayMode acceptance also has the reported flight failure. GUI/timed map animation and device acceptance were not independently performed. Coordinator integration/acceptance is still required.
