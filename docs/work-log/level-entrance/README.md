# Level entrance reveal correction

## Integration and fix

Merged `origin/t54-integration` into `mac-t54b/t54` before editing. The merge fast-forwarded to `a7679d97fc3e8ea92134134a73f29f4de5f0e357`. Fix commit: `5f5e17e` (`Prepare level entrance poses before the leaf wipe uncovers`). Main and the primary checkout were not modified.

The coordinator's supplied `origin/mac-t54j/t54` capture showed a complete settled board at approximately 0.5 seconds, followed by its reset into the entrance pose. The failure was accepted from that evidence, not rerun to reconfirm it.

Cause: `BuildLevel` rebuilt settled views in the wipe's covered callback; the initial entrance pose was applied only in the later wipe-completion callback.

Changes:
- `GameRoot.BuildLevel` prepares an eligible entrance immediately after rebuilding Board/Hud/Scene, while the leaf curtain still covers the level.
- `LevelEntrance.Prepare` moves every bird to its existing entry start, stops idle motion, collapses the branches, holds the houses at their starting height and sets the forest's initial fade. The initial pose is applied **after** cancellation, because branch cancellation restores full scale.
- `Play` consumes this prepared state and starts the existing timeline at the original wipe-completion point. It does not restore a settled pose between preparation and playback.
- Cancellation restores prepared states as well as playing states. Direct/resumed entrances prepare themselves when there was no covered rebuild. Fixture loading without an entrance is unchanged.

No durations, staggers, flight paths or assets changed: houses 0.8 seconds; branches 0.7 seconds with 0.15-second row offsets; birds 0.08 seconds apart within each row with 1.0-second entry flights; 3.2-second minimum entrance timeline; 0.4-second leaf wipe. The existing anticipation, squash and control-reveal durations are unchanged. `BirdView`, `BoardView`, `BranchView`, `SceneView` and `LeafTransition` required no edits.

## Required timed evidence

- [Entrance reveal contact sheet](evidence/entrance_reveal.jpg): **1600 × 1905**, fifteen real Unity PlayMode frames, nominal **0.05-second intervals from 0.30 through 1.00 seconds** after the Play pointer event.
- [Frame timing and state log](evidence/entrance_reveal.csv): target/actual game time, wall time after rendering/encoding, level-surface state, entrance state, grown-branch count, perched-bird count, house height and move acceptance.
- [Graphical regression result](evidence/capture_results.xml): **1/1 passed**, zero skipped cases.

The level surface was first observed rebuilt at **0.3505 seconds**, behind the wipe. Every recorded sample contained **zero grown branches and zero perched birds**. At **0.5005 seconds**, where the supplied evidence exposed the settled board, birds were already at their entry starts, branches remained collapsed, and houses still held their initial height. Playback began at **0.5503 seconds**, without a pose reset.

The permanent regression checks **every intervening Unity frame**, not just the fifteen sampled frames. It requires observation of both the covered rebuilt level and the playing entrance, rejects gameplay throughout the reveal, and checks the final settled board. A second regression disables the root during covered preparation, checks restored perches, then resumes and skips the entrance successfully.

Captures used the real Game scene, normal motion, a real graphics device and a 540 × 960 render target. The test profile acknowledges unrelated notifications and restores the saved profile keys afterward. Canvas scaling settles before the Play event. Contact-sheet labels retain actual frame times; maximum sampling lateness was **18.6 ms**, not hidden by relabeling frames. In the CSV, `level_visible` means the level surface has replaced the map; that surface can still be occluded by the leaf curtain. Raw frames remain in ignored `build/entrance-reveal/`.

The final contact sheet was visually inspected: the level is already in its starting pose when the leaves uncover it; there is no complete-board flash in the requested window. Waiting birds retain the existing start positions, as permitted by this correction.

## Full verification

All Unity commands ran serially under `/tmp/unity.lock`. No user Editor was running at the initial process check, and no Editor was killed.

| Check | Result |
| --- | --- |
| Naming guard self-tests | 8/8 passed |
| Naming guard before Unity | 1514 tracked text files inspected, 0 violations |
| Scene build and compilation | Passed |
| Full EditMode suite | 312/312 passed, 0 skipped |
| Full PlayMode suite | 55/55 passed, 0 skipped |
| Graphical reveal regression | 1/1 passed, 0 skipped |
| Stable screenshot smoke | 34 required 1080 × 1920 captures validated |
| Level 1 tap to playable, full suite | 3.7503 seconds |
| Five-row level 37 tap to playable, full suite | 4.3911 seconds |

Full results: [EditMode XML](evidence/editmode_results.xml), [PlayMode XML](evidence/playmode_results.xml). No retained tests were weakened or skipped. The two new behavioral cases extend the existing entrance fixture.

Commands executed from this worktree:

```bash
python3 ci/check_current_game_names.py --self-test
python3 ci/check_current_game_names.py
flock /tmp/unity.lock bash scripts/run_tests.sh
flock /tmp/unity.lock env ENTRANCE_REVEAL_CAPTURE_DIR=/Users/apple/orca/workspaces/game/t54-entrance/build/entrance-reveal /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath /Users/apple/orca/workspaces/game/t54-entrance/PerchSort -runTests -testPlatform PlayMode -testFilter PerchSort.PlayModeTests.T54BEntranceTests.CoveredRebuildKeepsEntrancePoseThroughReveal -testResults /Users/apple/orca/workspaces/game/t54-entrance/build/entrance-reveal-results.xml -logFile /Users/apple/orca/workspaces/game/t54-entrance/build/entrance-reveal.log
flock /tmp/unity.lock bash scripts/screenshots.sh
```

The first implementation attempt passed 312/312 EditMode and 54/55 PlayMode; the new reveal regression caught preparation applying branch scale before `Board.Cancel` reset it. Moving initial-pose application after cancellation fixed the ordering. The final full suite and graphical run above passed after that correction. Logs remain under ignored `build/`.

## Preserved workspace state and limits

Before integration, the existing local generated scene and seven overlay metadata changes were preserved in stash **`7807e0fb3ff3d9947544fd91bd5221d39d1fa407`**, message `Preserve entrance workspace Unity-generated state before integration`. The stash was deliberately not reapplied over the integrated resource state.

Scene/meta/settings/package hashes were recorded before Unity. This run generated a scene delta, **121 bird metadata deltas**, and two tracked naming-guard bytecode deltas. They remain unstaged and uncommitted; the exact binary-capable patch is preserved in ignored `build/entrance-reveal-generated.diff`. Only the three authored source/test files and this work-log evidence belong to the correction commits.

Verification covers the integrated Mac Unity runtime and automated UI input/rendering, not mobile-device testing or human feel sign-off. The required high-frequency reveal sheet is for level 1; the unchanged-duration five-row scenario is covered by the full suite. No new art or timing redesign is part of this fix.
