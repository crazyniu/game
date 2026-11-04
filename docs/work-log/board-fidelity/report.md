# T53A board fidelity report

## Delivery status

Branch: `mac-ui-board/t53`, based on the supplied `t53-ui-fidelity` tip (`5ba155e820b37a157b1f8a494af9c607b0f05d9e`).

D1/D2 have visible geometry improvements, but **exact approved-image fidelity is not achieved** because forked-trunk artwork remains blocked. The coordinator has resolved the illustration-versus-level count difference in favor of all playable Places. The authorized per-density crown correction is implemented and re-captured: crowns clear the first-row bird silhouettes on the three-/five-row boards. Compilation through the latest capture succeeded. The earlier full-suite results below retain one coordinator-confirmed pre-existing timed-flight failure; they are not claimed as a fresh full-suite run for the crown correction.

## Owned changes

Only these runtime source files were edited:

- `PerchSort/Assets/Scripts/Game/BranchView.cs`
- `PerchSort/Assets/Scripts/Game/SceneView.cs`
- `PerchSort/Assets/Scripts/Game/GameLayout.cs`

`BoardView.cs` and `Ambient.cs` were intentionally unchanged. Bird rendering, slot coordinates, hit targets, authored levels, Core/Meta, peer-owned UI, artwork, and existing tests were not edited. No new artwork or permanent tests were added. The required report and four coordinator-requested review screenshots under `docs/work-log/board-fidelity/evidence/` are included in the branch.

### D1: trees and crowns

- Replaced the unshaped full-width trunk quads with segmented geometry using the existing bark sprites. The visible trunks now taper and bend toward the crown attachment instead of remaining uniform 90-pixel edge columns.
- Enlarged the non-snow crowns from a 400-pixel canvas to 500 pixels on the left and 540 on the right; both are attached using the existing measured crown-root coordinates.
- Sized trunks from the crown attachment to below the screen rather than stretching a 1920-pixel column down from an arbitrary top.
- Moved snowy crowns to cropped outer-edge placements. A first iteration enlarged them like the green-world crowns; actual comparison showed that this obscured the top-row silhouette and did not resemble the approved snowy board. The final capture uses small side-edge snow foliage instead.
- Follow-up authorized density adaptation lives entirely inside `SceneView`: row count matches `BoardView`'s derivation; boards with at least three logical rows shrink broad crowns to 72% and shift the crown canvas above the full first-row bird height plus breathing/sway clearance. Already-cropped snowy crowns shift upward without further shrinking. Trunk tops are recomputed from the same transformed crown-root offset so the trees stay attached. Sparse level-1 scenery is unchanged.
- Existing crown sway and ambient motion are unchanged.

### D2: branches

- Added a segmented limb image using the supplied branch/twig texture, with extra thickness concentrated at the trunk and a lightly rising sitting surface.
- Preserved the entire source texture, including its rounded tip, when a growing branch has fewer seats. The previous filled-image crop visibly sliced off the endpoint. Growth still drives `fillAmount`, but geometry now resizes the complete textured limb rather than discarding its tip.
- Retained source vines/leaf sprigs at branch roots; preserved mirroring, entrance, landing flex, growth timing and cancellation.
- Left all `RowY`, `BirdSlot`, `BranchHit`, capacities and logical Place coordinates unchanged. No actionable branches were hidden or merged to imitate an illustration.

World art IDs used, for each `w1` through `w4`:

- `World/trunk_left_wN`
- `World/tree_crown_wN`
- `World/branch_left_wN`
- `World/twig_left_wN`

Existing `leaf_fall_wN` effects remain unchanged. Vine/sprig artwork is embedded in the limb sprites; there is no separate vine asset in the current World resource set.

## Runtime visual evidence

Executed `flock /tmp/unity.lock bash scripts/screenshots.sh` four times: baseline, first geometry iteration, snowy-crown correction, and the authorized density-aware crown correction. All four completed successfully. No user Editor was running at the initial or follow-up process checks; no Editor was killed. `flock` was initially unavailable and was installed with Homebrew before any Unity launch.

The approved/runtime side-by-sides were generated with `/tmp/t53a/compare.py` (outside the repository), opened and visually inspected. Final full-resolution comparisons are in this Mac workspace:

| Runtime capture | Actual authored density | Approved comparison | Final side-by-side |
| --- | --- | --- | --- |
| `level1_start` / level 1 | 2 logical row indices, 4 actionable limbs | `ui_level_k3.png` | `build/t53a/level1_start_comparison.png` |
| `level_k3` / level 5 | 3 logical row indices, 5 actionable limbs | `ui_level_k3.png` | `build/t53a/level_k3_comparison.png` |
| `level_k7` / level 40 | 5 logical row indices, 9 actionable limbs | `ui_level_k7.png` | `build/t53a/level_k7_comparison.png` |
| `world_w3` / level 30 | 5 logical row indices, 9 actionable limbs | `ui_level_k7.png`, structure comparison only; different forest world | `build/t53a/world_w3_comparison.png` |

Uncomposited final frames are preserved as `build/t53a/{level1_start,level_k3,level_k7,world_w3}_final.png`. Initial frames are preserved as matching `_before.png` files. `build/t53a/mech_growing_final.png` was also inspected to verify the shortened limb retains a rounded end; `mech_crow_twig_final.png` is retained. Canonical capture output remains under `build/screens/game/`. These full-resolution build artifacts are gitignored.

The four latest approved/runtime review images are committed for cross-host review. Each is **1184x1080 JPEG, quality 80**; encoded JPEG quantization tables and the height limit were verified after writing:

- [`docs/work-log/board-fidelity/evidence/level1_start_comparison.jpg`](evidence/level1_start_comparison.jpg)
- [`docs/work-log/board-fidelity/evidence/level_k3_comparison.jpg`](evidence/level_k3_comparison.jpg)
- [`docs/work-log/board-fidelity/evidence/level_k7_comparison.jpg`](evidence/level_k7_comparison.jpg)
- [`docs/work-log/board-fidelity/evidence/world_w3_comparison.jpg`](evidence/world_w3_comparison.jpg)

All four JPEGs were opened and inspected. The latest green three-row and autumn/snow five-row captures show crown/first-row separation with trunk attachment preserved; the sparse level-1 composition remains unchanged. The external comparison utility is still `/tmp/t53a/compare.py`.

## Verification

The full-suite counts below belong to the initial geometry delivery. After the density-aware crown change, the locked screenshot command compiled and ran the real Unity capture path successfully; all four new review comparisons were inspected. Full EditMode/PlayMode suites were not rerun for this follow-up, and the coordinator-reported pre-existing flight failure was not rerun to reconfirm it.

1. `python3 ci/check_current_game_names.py`
   - PASS: 567 tracked text files inspected, zero violations.
2. `flock /tmp/unity.lock bash scripts/run_tests.sh` — executed once.
   - Scene build / compilation: succeeded; the runner proceeded into both suites.
   - EditMode: **304/304 passed, 0 failed, 0 skipped**.
   - PlayMode: **16/17 passed, 1 failed, 0 skipped**.
   - Overall command exit: **1** (Unity's PlayMode invocation exited 2).
   - Failure: `PerchSort.PlayModeTests.GameplayTests.TimedFlockArrivalsReachTheirGoalDoor`, `GameplayTests.cs:405`.
   - Assertion: visible flock distance must be `< 40`; observed closest distance **45.114975**.
   - Exact XML: `build/test-results.xml`, `build/playmode-results.xml`; logs: `build/scene-build.log`, `build/editmode-tests.log`, `build/playmode-tests.log`.

The timed-flight failure is **pre-existing per the coordinator's explicit task update**: the same assertion failed on base `63d4782` in T52 attempt-04 (45.17 versus <40), and its fix is in `c074716`, which is not in this worker's base. That baseline/fix attribution is coordinator-provided evidence, not a baseline run performed by this worker. The assertion samples `BirdView.Position` against `BirdhouseView.DoorPoint`; those implementations and the board's logical anchors are unchanged here. The test was not weakened, skipped or rerun to obtain a passing sample. No BirdView change, threshold adjustment or out-of-scope cherry-pick was made.

Stable capture exercises the actual Unity rendering/capture path, not animation timing. Full interactive GUI/device acceptance was NOT_RUN by this worker and remains a coordinator boundary. Existing PlayMode tests were run without modifications; no mock-only proof was substituted.

## Remaining differences / requested contract change

1. **D1 artwork limitation:** `trunk_left_w1.png` through `trunk_left_w4.png` are straight bark columns, not the approved forked tree silhouettes. `branch_left_wN.png` supplies a shallow flared root with a vine/sprig, not the approved continuous heavy curved trunk-to-limb junction. The geometry correction improves taper/connection but cannot recreate the painted forks, broad curved junctions and snow masses exactly. Missing artwork regions: green master x=0..150 and x=930..1080 from roughly y=690 downward; snowy master side trunks and main limb joints from roughly y=600..1680. Stop on replacement artwork per contract: coordinator must supply approved derived forked/junction assets, or explicitly authorize a resource-owner change. No originals were modified or invented.
2. **D2 authored-board difference, coordinator decision:** approved green master has 4 main limbs; approved snowy master has 5 main limbs plus a crow perch. Current level 5 has 5 actionable limbs; level 40 has 9. Level 40 retains its approximately 98-pixel opposite-side spacing rather than the illustration's approximately 210-pixel main-limb spacing. The coordinator explicitly decided that playable Places win: all authored branches and anchors stay visible and unchanged; no hiding or merging is permitted.
3. **Crown/row interaction, resolved:** per-density placement was authorized and implemented in `SceneView`. The latest three-/five-row comparisons show crowns clear of first-row bird silhouettes, still attached to their raised trunk tops. This is no longer an open limitation.
4. Houses/HUD/boosters, tutorial hand, bird species/poses, background variant, coin totals and bottom foreground differ in the full screenshots. They are outside this worker's ownership; no whole-screen match is claimed.
5. The pre-existing timed flock-arrival failure is still present in this branch's test results. The coordinator owns integration of the existing `c074716` fix; these worker results are not an all-green acceptance claim.

## Coordination and integration

Before the coordinator's latest routing correction, the repository-designated Codex session received two accepted Orca messages on the connected host: `msg_4e0963cf3c9e` (fidelity constraints), `msg_3dcf967a8228` (test failure). The coordinator then explicitly confirmed the pre-existing failure and instructed this worker to deliver the final SHA through its last terminal message, not the Codex route. Final delivery follows that instruction; no further Codex notification is sent.

No merge, main update, release or peer-branch integration was performed. Initial staging was limited to the three owned source files and this report; follow-up staging includes only `SceneView.cs`, this report and the four expressly requested review JPEGs. Unity-generated scene/project/import state is not included or blindly restored. Final delivery remains through the last terminal message, not the Codex route.
