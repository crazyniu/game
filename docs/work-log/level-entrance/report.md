# T54B entrance pacing report

## Delivery and authority

Branch: `mac-t54b/t54`, created from the supplied contract tip `cb438e87e67e6d66c97bdf00b77ef356ca161290`. No merge and no changes to main or the primary checkout.

Implementation commits:
- `6603e2af7b6c8f13260986c34e89261a73dca24e` — **BirdView: entry flight duration/easing parameters**. The coordinator explicitly authorized only the entry-flight parameter hunk in this otherwise read-only file. Normal-flight defaults remain unchanged. This commit is separate for selective integration with the idle worker.
- `1560e69` — **Pace level entrances with staggered row waves and landing gates**. Changes only `LevelEntrance.cs`, `BoardView.cs`, and the new `T54BEntranceTests.cs` / metadata.

`BranchView.cs`, `SceneView.cs`, and `HudView.cs` needed no changes: their existing trunk-root scaling, scene alpha, and HUD entrance entry points are reused. No artwork was generated or edited. No existing test was edited, weakened, ignored, or removed.

## P1 change

The previous entrance timeline was 1.6 seconds, with bird delays repeating every seven birds and 0.45-second flights. The new sequence is:

| Stage | Behavior |
| --- | --- |
| Houses and forest | 0.8-second drop/fade, reduced soft back-overshoot |
| Branches | 0.7-second trunk-to-tip growth, rows offset by 0.15 seconds |
| Birds | Each row starts after its branch finishes; birds within that row launch 0.08 seconds apart; rows overlap |
| Entry flight | 1.0-second flight with cubic ease-out; existing 0.06-second anticipation and 0.12-second touchdown squash retained |
| Controls and input | Controls reveal over the last 0.45 seconds; input remains blocked until the timeline and every arrival callback have completed |

The timeline has a 3.2-second minimum. Its upper bound follows the final row-wave arrival, not the total number of birds. A shared arrival callback avoids allocating a new completion delegate for every bird. Completion never truncates a late flight to satisfy a timer. Tap-to-skip cancels the outstanding flights, restores canonical perch positions and scales, and invokes completion once. `GameMotion.Instant` settles synchronously.

## Approved contract adjustment

The original constraints cannot all hold literally for dense boards: level 22 contains 28 birds, so fully serial houses, all branches, global 0.08-second bird staggering, a minimum 1.0-second flight, and the existing map wipe exceed 4.5 seconds. In addition, the original `BirdView.Enter` exposed neither duration nor easing.

The coordinator's explicit ruling permitted parallel per-row waves and changes only to the `Enter` / `FlyCore` duration/easing path in `BirdView.cs`. That narrow hunk is isolated in `6603e2a`; no idle methods or fields were touched. The normal `Fly` and `FlyHome` paths retain 0.45-second, linear motion defaults.

## Timed Unity evidence

Real Unity 6000.6.7f1 PlayMode, graphics enabled, normal motion, actual `Game` scene and UI pointer-click handler. These are not edit-mode instant captures. The level card and render target are prepared before the timing clock starts. Tests use a synthetic acknowledged profile to exclude unrelated notifications, and restore all saved profile keys and global test settings afterward.

| Scenario | Tap to playable, observed each frame | Captures |
| --- | --- | --- |
| Level 1, two rows | 3.7504 seconds | 51 frames, nominal 0.1-second intervals through 5 seconds |
| Level 37, five rows / 29 birds | 4.3911 seconds | 51 frames, nominal 0.1-second intervals through 5 seconds |
| Level 1, idle after the first five-second observation | Input stayed unlocked, no birds airborne | 41 frames over 20 seconds, nominal 0.5-second intervals |

Committed evidence under `tasks/evidence/T54B/`:
- `level1-entrance.jpg`, `level37-entrance.jpg`: complete entrance contact sheets, each 1600 pixels wide.
- `level1-idle.jpg`: twenty-second idle contact sheet, 1600 pixels wide. This records the existing idle implementation, not integration or acceptance of the separate idle task.
- Matching `*-timing.csv`: scheduled sample times, actual game times, wall times after rendering/encoding, introducing/airborne/playable state.
- `capture-results.xml`: all five focused graphical acceptance cases passed.
- `test-results.xml`, `playmode-results.xml`: final full-suite results.

The contact-sheet labels use actual game time rather than relabeling late frames as exact scheduled timestamps. Maximum entrance sample lateness was 0.7 ms for level 1 and 13.6 ms for level 37. Raw 540 x 960 PNG frames remain in ignored `build/entrance-capture/`; the portable JPEG sheets and timing logs are committed. All three final sheets were visually inspected.

## Verification

Every Unity invocation was serialized with `flock /tmp/unity.lock`. The user's Editor was observed closed before starting. No Editor was killed.

Commands executed from this worktree:

```bash
python3 ci/check_current_game_names.py
flock /tmp/unity.lock bash scripts/run_tests.sh
flock /tmp/unity.lock env T54B_CAPTURE_DIR=/Users/apple/orca/workspaces/game/t54-entrance/build/entrance-capture /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath /Users/apple/orca/workspaces/game/t54-entrance/PerchSort -runTests -testPlatform PlayMode -testFilter PerchSort.PlayModeTests.T54BEntranceTests -testResults /Users/apple/orca/workspaces/game/t54-entrance/build/entrance-capture-results.xml -logFile /Users/apple/orca/workspaces/game/t54-entrance/build/entrance-capture.log
```

Final full runner: scene build/compilation succeeded; **304/304 EditMode**, **26/26 PlayMode**, zero failures and zero skipped cases. The retained 21 PlayMode tests passed alongside five new behavioral cases:
1. Level 1 remains blocked through entrance/landing, then accepts its authored first move.
2. The worst scheduled five-row wave finishes within the tap-to-play target and leaves all birds perched.
3. A timed entry flight covers most horizontal distance before its landing approach, lands within 1.0–1.4 seconds, and retains touchdown squash.
4. A pointer tap during flight skips immediately; no delayed callback replays motion or changes the board afterward.
5. Instant motion completes the five-row entrance synchronously.

The separate final graphical run passed **5/5**. Full-suite logs are `build/scene-build.log`, `build/editmode-tests.log`, and `build/playmode-tests.log`; graphical log is `build/entrance-capture.log`.

Earlier unsuccessful attempts were fixed, not omitted: the first compilation found an ambiguous `SafeArea` in the new capture helper, corrected by qualifying `PerchSort.Game.SafeArea`. The next full run passed 304/304 EditMode and 24/26 PlayMode; only two new five-row cases failed because their synthetic profile lacked the preceding world-gate stars. The fixture was corrected without changing production access rules. No retained test failed due to an intentionally changed timing assertion.

## Generated Unity state and remaining limits

Scene/meta/settings/package state was recorded before Unity. The runner rebuilt `Assets/Scenes/Game.unity`; Unity also reserialized seven existing bird overlay metadata files: `chick_fluff`, `egg_crack`, `egg_idle`, `egg_shell`, `egg_wobble_l`, `egg_wobble_r`, and `mama_bow`. These unowned generated changes remain uncommitted and were not restored or staged. Their delta is preserved in ignored `build/unity-generated.diff`. The scene metadata, package manifest/lock, and `ProjectSettings.asset` hashes remained unchanged. Integrate the authored commits, not those uncommitted generated files.

Observed presentation boundary: the existing map wipe briefly exposes the rebuilt settled board before `LevelEntrance.Play` resets its pose, visible around 0.5 seconds in the sheets. Existing vertical entry staging also leaves some waiting birds visible near the houses. Neither positions nor the `GameRoot` / wipe orchestration were redesigned under this duration/stagger/easing task. Eliminating those presentation artifacts requires a separate orchestration/staging authorization.

This is automated Mac Unity UI/runtime evidence, not mobile-device testing or human feel sign-off. Only levels 1 and 37 received timed entrance capture; the full retained suites additionally cover the existing game contracts. Peer music, idle, buttons, and asset changes were not merged or validated here. The coordinator must revalidate the integrated branch, especially the separate `BirdView` hunks.
