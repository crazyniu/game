# Environment art wiring report

## Delivery

Implemented the assigned board and map wiring on `mac-environment-wiring/work`. Runtime changes are confined to `BranchView.cs`, `SceneView.cs`, `GameLayout.cs`, `MapView.cs`, `MapModel.cs` and `GateView.cs`. `MapMotion.cs` did not need changes. Added `EnvironmentArtTests.cs` and its Unity metadata. No changes to `BirdView.cs`, rules, level data, economy, persistence keys, resource import policy or popup gate callers are included in the commit.

**Environment checks pass; the full suite is not green.** Two failing tests outside the owned files prevent the requested full-suite green result. Their exact failures are recorded below and in [verification evidence](evidence/verification.log).

## Implementation

### Board

- Curved world trunks replace the procedurally reshaped legacy trunks. The right scenery is mirrored. Existing crown placement and dense-board crown-clearance logic remain in place.
- Every branch has a world-specific junction at its trunk-side root, vertically centred on the logical seat line. Twig junctions use 0.8 size. Junctions participate in the existing flex/entrance transform and survive cancellation and growth cleanup.
- Optional `branch_limb_w<N>` and `twig_limb_w<N>` resources are probed once per world. Missing art retains the existing branch/twig sprites.
- Row positions, bird slots, hit rectangles, capacities, filled-limb growth and entrance APIs are unchanged. The obsolete procedural trunk mesh was removed rather than retained as a parallel presentation.

### Map

- Gates use the closed arch for the world being completed, rather than the existing popup door presentation. Runtime gate keys still identify the destination world, preserving the existing unlock callback/state flow.
- Separate frame/door/light resources enable a 0.9-second smooth ease-in-out door swing from x scale 1 to 0.08 around opposite hinge edges, with a 0.6-second light fade. If any part is absent, the closed arch dissolves over 0.5 seconds. Already-open gates retain their open pose through lifecycle cancellation.
- Gate geometry includes clearance for the complete current chick, not only node faces. The original 500-pixel inter-world separation cannot contain a 600-pixel arch plus neighbouring nodes and the chick. `MapStride` is therefore 3600 instead of 2840, giving 1260 pixels between boundary nodes. Node coordinates within each world are unchanged. Existing 3000-pixel background paths retain their native scale; cached canopy sprite regions fill the additional inter-world interval with overlapping edges. No new art was authored.
- World-one bamboo is behind its gate with matching top alignment. The six-cell stepping-stone sheet is sliced into cached sprites; each consecutive-node segment receives a deterministic level-seeded group of three to five flat stones, outside node rectangles.
- Foreground corner sprites are non-interactive, horizontally confined to edge regions that do not intersect nodes, drawn above map content and clipped below the HUD areas. See the specification conflict under limitations.
- Added runtime sprite slices are disposed when the map is destroyed. Decorations do not add update loops, colliders or particles.

## Verification

Every Unity command waited on `flock /tmp/unity.lock`; no competing Editor was killed. The initial worktree had no recorded scene/settings/package changes. The primary checkout was not modified.

| Check | Observed result |
| --- | --- |
| Naming guard self-tests | 8 passed |
| Naming guard | 0 violations |
| Full EditMode suite | 317 passed, 0 failed, 0 skipped |
| Full PlayMode suite | 73 passed, 2 failed, 0 skipped; 75 discovered |
| New permanent environment tests | 5 passed |
| Graphics-enabled environment acceptance | 6 passed, including the temporary capture case |
| Stable screenshot command | Passed; 34 captures at 1080 × 1920 |

The full runner was executed before and after the final runtime refinements and temporary capture-harness removal. Both runs discovered the same counts and the same two failures. Existing test files, discovery safeguards and exclusions were not changed. The full run includes the retained 81 expanded level-data cases and 41 layout cases. The permanent environment tests add five cases; keeping the literal total unchanged while also adding the required tests is not possible without removing existing coverage. The assigned feature tip already discovers more cases than the older counts quoted in the project entry documentation.

Permanent environment checks exercise:

- All forty current-level profiles at the peak of the existing 12-pixel node bounce: every gate rectangle is disjoint from every node rectangle and the current chick; stone and corner rectangles do not cover nodes and do not intercept input.
- Recorded pre-wiring branch hit snapshots for authored levels 1, 5, 20, 30 and 40, including fractional dense-row heights.
- Curved trunk and junction resources in all four worlds, right-side mirroring, entrance participation and cancellation retention.
- Missing optional limb art, twig-to-normal growth, completion callback and final full fill.
- Real-time map opening, exactly one completion callback and preservation of the open pose.

A stricter peak-bounce check first reproduced `Path stone covers Level 1`. Stone placement now reserves that complete motion envelope, including the scatter collision check. The same expanded case passes in the graphics-enabled run. The failing-before result is preserved in [bounce regression evidence](evidence/bounce-regression.log).

### Commands

```bash
python3 ci/check_current_game_names.py --self-test
python3 ci/check_current_game_names.py
flock /tmp/unity.lock bash scripts/run_tests.sh
flock /tmp/unity.lock bash scripts/screenshots.sh
```

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

The temporary capture case used the real game root, the retained Fredoka font, nonpersistent profiles, camera rendering and real-time gate motion. It was removed before final full-suite verification. An initial capture-harness compile attempt failed because this test assembly does not reference TextMeshPro directly and `SafeArea` was ambiguous; the harness was corrected without changing assembly ownership, then the graphics run passed.

Full XML/log outputs remain in ignored `build/`. Unity import/build and naming self-tests generated unowned resource metadata, scene and Python bytecode changes. These were not staged or blindly restored. Their tracked diff is preserved separately in ignored `build/unity-generated-state.patch` for coordinator inspection; generated changes are not part of the pushed implementation.

### Full-suite failures outside ownership

- `GameplayTests.SaplingIntroPersistsOnShowAndBranchTapDismissesWithoutSpending`, `GameplayTests.cs:324`: expected pointer position `(720, -1802)`, observed `(720, -1764.80)`.
- `IllegalMoveFeedbackTests.MissingTurnSequencesUseExistingBlinkAndInstantSettlesImmediately`, `IllegalMoveFeedbackTests.cs:158`: expected a null mint turn sprite, but `mint_turn_left_01` exists.

Neither test was edited, skipped or weakened. Making the full command green requires changes outside this assignment's owned files.

## Visual and timed evidence

All raw runtime frames are 1080 × 1920. Comparisons include the unchanged approved reference pixels beside actual rendered output.

- [Sparse board beside approved board](evidence/board_sparse_comparison.jpg): authored level 1, two rows.
- [Medium board beside approved board](evidence/board_medium_comparison.jpg): authored level 5, three rows.
- [Dense board beside approved board](evidence/board_dense_comparison.jpg): authored level 40, five rows.
- [Current map levels one, four, ten and eleven beside the approved map](evidence/map_comparison.jpg).
- [Gate opening sheet, nominal 0.1-second intervals, beside the approved map](evidence/gate_opening_sheet.jpg).
- [Timed measurements](evidence/runtime-capture.log), plus raw `gate_frame_a.png` through `gate_frame_k.png`.

The measured available-art path is the specified dissolve fallback. Captured alpha values are 1.0000, 0.7968, 0.5972, 0.3975, 0.1972 and 0.0000 across the first six nominal samples; completion changes from zero to exactly one callback. Logged elapsed times are recorded after camera readback, approximately 0.08 seconds later than their corresponding pose sampling. This is timed runtime evidence, not an instant-capture animation claim.

## Limitations and impossible combinations

- Separate world gate frame, door and light sprites are absent. The dissolve path is implemented and exercised; a visual door-swing/light acceptance claim cannot be made without those assets. Optional limb sprites are also absent, so captures deliberately show legacy-limb fallback.
- The spec simultaneously asks for corners pinned to the viewport and for every corner to scroll with the map. Both behaviours cannot hold at once. The final scrolling clause is followed: corners start at the focused viewport's bottom edge and then scroll with content. They are not permanently viewport-fixed.
- The old boundary spacing cannot satisfy the required arch/node/chick clearance at the requested arch size. The explicit inter-world spacing adjustment above is necessary to satisfy the non-intersection requirement; a full-size arch at the former midpoint would overlap neighbouring presentation.
- Current junction art has a visible elevated tip on shortened, empty growth limbs when placed exactly with its canvas centre on the seat line. This is visible in the supplied board comparisons. Art was not edited, seat lines were not moved, and optional replacement limbs were not fabricated to conceal it. Exact reference-image equivalence is not claimed; inherited house/HUD layout and authored bird compositions were not redesigned under an environment-only assignment.
- A green full suite with the literal unchanged total cannot be delivered inside the owned-file boundary for the reasons above. No device, audio, or complete bird-animation acceptance is claimed by these environment captures.
