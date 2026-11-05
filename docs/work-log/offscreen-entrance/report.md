# Off-screen level entrance

## Scope and accepted contract

Issue: #101. Branch: `mac-offscreen-entrance/work`.

Authored changes:
- `PerchSort/Assets/Scripts/Game/LevelEntrance.cs`
- `PerchSort/Assets/Scripts/Game/BoardView.cs`
- `PerchSort/Assets/Tests/PlayMode/OffscreenEntranceTests.cs` and its Unity metadata
- The coordinator-authorized timing assertion in `PerchSort/Assets/Tests/PlayMode/FlightFramesTests.cs`
- This report and its timed evidence

`BirdView.cs` required no changes. Its existing seconds-based drawn flight frames, direction selection, cubic entry easing, anticipation and touchdown remain in use. No gameplay rules, authored levels, persistence, art or other presentation implementations were changed by this task.

The coordinator explicitly accepted folding an opposite-side random draw onto the assigned branch's side: own edge probability 2/3, top probability 1/3. This resolves the otherwise contradictory uniform-three-side and side-preference requirements without sending birds across the full width from the opposite edge.

## Behavior

- Removed the interior `(target.x, 420)` starts, including the every-fifth-bird special case.
- Each entrance preparation obtains a zero-based attempt number for that level from the entrance component. A local `System.Random`, seeded with `level id + attempt`, isolates flight starts from unrelated idle randomness. Attempts are runtime-only and do not alter save data.
- Side starts use x = -180 or 1260 and y uniformly sampled from 150 through 1500. Top starts use y = -180 and x uniformly sampled from 100 through 980. Both coordinates receive independent jitter in [-60, 60]. The minimum 120-pixel outside margin cannot jitter into the canvas or goal tray.
- Eggs and effectively sleeping birds are placed on their seats, hidden during preparation, then pop from scale 0.6 to 1 over 0.25 seconds with smooth easing after their branch has grown. They never launch, increment the airborne count or display flight drawings.
- Houses remain first, then growing branches, then overlapping row waves. Existing 0.08-second within-row staggering and 1.0-second flights remain unchanged. The entrance timeline minimum changes from 3.2 to 3.5 seconds so a restart, which has no map wipe, also meets the requested lower bound.
- Input remains gated until the pose timeline and every arrival complete. Existing tap-to-skip and cancellation restore canonical perch positions and full scale. Instant mode settles synchronously.
- Frames face the direction of travel: a bird entering from the left and flying right uses the existing unmarked right-facing drawings; travel toward the left uses `_l_` drawings. The authored images were inspected and the first-flight direction was asserted in runtime tests. No sprite mirroring was introduced.

## Targeted coverage

Six new behavioral PlayMode cases cover only this change:
- Levels 1, 5, 21 and 40: every flying bird's prepared start and first observed flight-frame position are outside `GameLayout.SafeDesignArea` and `GameLayout.GoalTray`; first-frame direction matches start versus seat x; no opposite-edge starts; a restart produces different starts; both loads meet the timing range.
- The same level and attempt across fresh scenes reproduce identical starts despite unrelated idle draws.
- Level 40's hidden seated entrances restore full scale on skip and an instant restart.

The four load cases also verify that every egg/sleeper stays on its seat, retains its resting drawing, appears only after branch growth and completes its 0.25-second scale transition. Pop timing is bounded by the actual observation gaps at both ends, not by the unrelated final frame's delta.

## Observed timing

Final graphical run, normal motion, time scale 1, real Game scene and pointer-click input. Initial recorded loads include level-card dismissal and the map wipe; restarts do not. Capture-only setup warms resource loading before the timing clock. No capture-framerate override or synthetic animation was used.

| Level | Recorded load to playable | Restart to playable | Flying birds | Seated birds |
| --- | --- | --- | --- | --- |
| 1 | 4.0510 s | 3.5004 s | 8 | 0 |
| 5 | 4.0505 s | 3.5002 s | 12 | 0 |
| 21 | 4.0509 s | 3.5004 s | 18 | 3 |
| 40 | 4.0510 s | 3.5003 s | 18 | 10 |

## Timed evidence

Committed under `evidence/`:
- `level1-entrance.jpg` and `level40-entrance.jpg`: visually inspected 1600 x 2766 contact sheets, each containing 51 real 540 x 960 Unity frames scheduled every 0.1 seconds from Play through 5 seconds. Labels show actual game times.
- `level1-timing.csv` and `level40-timing.csv`: scheduled times, actual game times, wall times after capture, introducing/airborne/playable state. Maximum sample lateness: 0.9 ms and 2.4 ms respectively. Maximum capture wall/game drift: 26.3 ms and 31.0 ms.
- `level1-starts.csv` and `level40-starts.csv`: each flying bird's prepared start, first-flight-frame position and frame name.
- `capture-results.xml`: final focused run, **7/7 passed**, zero failed/skipped, including all six new cases and the authorized retained timing regression.
- `editmode-results.xml` and `playmode-results.xml`: exact results of the single full normal-suite invocation, including the original timing-test failure described below.

Raw frames and Unity logs remain in ignored `build/offscreen-entrance/` and `build/`. The sheets show empty houses and growing branches before arrivals; eggs and sleeping birds appear on their seats rather than flying from the houses. This is Mac automated runtime evidence, not mobile-device or human feel acceptance.

## Verification and authorized retained-test correction

Naming guard self-tests: **9/9 passed**. Naming guard: zero violations before Unity, including the newly tracked test.

The normal suite was invoked exactly once:
- Scene build and compilation passed.
- **318/318 EditMode passed**, zero skips; retained LevelsData and Layout discovery safeguards passed.
- **108/109 PlayMode passed**, zero skips. All six new cases passed. The only failure was `FlightFramesTests.LevelEntranceUsesSecondsBasedFramesAndSettlesEveryBird`, which observed for **3.4 seconds** and then required completion, conflicting with the new **3.5-second** minimum.

The coordinator then authorized changing only that test's timing checks: observation window **3.4 -> 4.6 seconds**, plus a completion timestamp assertion rejecting completion before **3.4 seconds**. Every frame-direction, airborne, perch-position and resting-pose check remains intact. The final focused graphical run passed that corrected test and all six new cases, **7/7**. The full suite was not repeated; the committed full-suite XML intentionally retains its original failure rather than claiming an unperformed all-green full run.

Earlier focused attempts are not hidden: the first compile found the new fixture accessing an internal duration constant; the fixture now reads the production constant through reflection without expanding the runtime API. The first successful graphical run passed 6/6 but level 40 had a cold-load sampling gap. A warmed recapture exposed an overly narrow final-delta-only pop timing tolerance; the fixture now uses both observed boundary gaps and exact full-scale completion. Final recapture passed 7/7 with the sampling accuracy reported above. A non-failing existing audio-source warning appeared during fresh-scene teardown; no audio behavior was changed or claimed.

Commands, from this workspace:

```bash
git switch -c mac-offscreen-entrance/work
git fetch origin && git merge origin/main
env PYTHONDONTWRITEBYTECODE=1 python3 ci/check_current_game_names.py --self-test
env PYTHONDONTWRITEBYTECODE=1 python3 ci/check_current_game_names.py
flock /tmp/unity.lock bash scripts/run_tests.sh
flock /tmp/unity.lock env OFFSCREEN_ENTRANCE_CAPTURE_DIR="$PWD/build/offscreen-entrance" /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath "$PWD/PerchSort" -runTests -testPlatform PlayMode -testFilter 'PerchSort.PlayModeTests.OffscreenEntranceTests;PerchSort.PlayModeTests.FlightFramesTests.LevelEntranceUsesSecondsBasedFramesAndSettlesEveryBird' -testResults "$PWD/build/offscreen-entrance-results.xml" -logFile "$PWD/build/offscreen-entrance.log"
```

All Unity invocations used `/tmp/unity.lock`. The coordinator explicitly authorized isolated-worktree batch runs while the primary checkout's Editor remained open. No user Editor was stopped and the primary checkout was not modified.

## Integration and generated state

`origin/main` was merged before implementation and again before the normal suite. The final requested merge fast-forwarded to `e1e376b6`, bringing in the coordinator's front-facing asset delivery. The recorded Unity runs precede that last asset merge; no additional full-suite result is claimed for that tip.

Before Unity, hashes for 1713 scene/meta/settings/package paths and the authored diff were preserved in `build/`. Unity subsequently changed **907 existing metadata files and Game.unity**. Their binary-capable patch is preserved in `build/unity-generated.diff`, with path/hash inventories in `build/unity-state-before.json` and `build/unity-state-changes.json`. These generated changes remain unstaged and are excluded from the task commit. Project settings and package hashes remained unchanged. The new test's metadata is included with its source.
