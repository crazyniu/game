# Input lock fixes

## Result

Issues #95 and #96 are fixed in the owned runtime paths. Final rendered verification passed **12/12 PlayMode cases**, including all ten new input-lock cases and both retained cases that exposed a clock-cancellation regression during the normal suite run. No Core, economy, save, art, layout, assembly-definition, or retained test source was edited.

Branch: `mac-input-lock-fixes/work`. Fetched and merged `origin/main` at `0805f87f` before implementing the repair. The merge brought only the upstream gameplay-proposal review document; no peer branch was merged.

## Permanent lock: ownership and cause

The failing test was written and run before the repair, in the real Game scene with `GameMotion.Instant=false`, `Time.timeScale=1`, and `Time.captureDeltaTime=0`:

1. Level 1: tap 0, then 1; wait for idle.
2. Invoke the real Undo button callback.
3. Yield one frame, then repeat taps 0 and 1.
4. Playback remained busy after 30 seconds. The two-cycle case failed the same way. Undo during an existing homeward flight already passed.

`BirdhouseView.SetHome(false)` scheduled a departure door/hop/close chain without advancing its generation. After replay began opening the door, the departure's delayed close could cancel the replay's opening callback. The board then waited forever for an arrival that never started.

`BirdhouseView` now starts each asynchronous transition through `BeginTransition`: advance the revision, cancel old motion channels/resident work, then schedule the new owner. Nested door, hop, and badge steps keep that revision instead of starting another owner. Stale completions cannot advance the transaction. Repeated state synchronization does not restart an already-owned departure.

Covered by:
- `UndoThenNextFrameReplayReleasesInput`
- `TwoQuickUndoReplayCyclesReleaseInput`
- `UndoDuringHomewardFlightDiscardsStaleCallbacks`
- `ReplacingDepartureAndArrivalOnlyCompletesCurrentTransition`

## Long locks: measured queue budget

Measured each event before changing playback. Typical baseline costs were about 2.660 seconds per serialized flock home, 0.400 per gift/growth, 0.300 per wake, and 0.150 per noninitial resolution pause. Raw individual measurements are in [before-events.csv](evidence/before-events.csv).

The new queue:
- Estimates its critical-path duration, accounting for parallel groups.
- Uses `max(0.4, min(1, 5.0 / estimate))` as the duration scale. Every queue duration and stagger runs on the same scaled LitMotion clock; Unity global time scale is unchanged.
- Starts all birds of a homecoming flock together, and follows chicks together.
- Overlaps adjacent independent same-wave homes, wakes, hatches/reveals/wobbles, and branch growth. Repeated bird, branch, or species owners do not overlap. Gifts and wave/dependency boundaries retain ordering.
- Completes presentation for every committed event; no event is dropped or replaced with an instant board update.
- Applies an idempotent three-times acceleration to the remaining queue after a subsequent tap, including airborne taps. The move-start frame cannot accelerate itself. Playback taps are consumed, not stored as a later gameplay action.

Final normal-motion wall-clock measurements:

| Level | Sequence / measured move | Before busy seconds | After busy seconds | Presented / committed |
| --- | --- | ---: | ---: | ---: |
| 11 | 0 to 4 | 0.5882 | 0.5606 | 2 / 2 |
| 11 | then 1 to 0 | 10.5424 | 4.8681 | 12 / 12 |
| 15 | 0 to 1 | 11.1025 | 4.9631 | 13 / 13 |
| 21 | 0 to 1 | 0.5827 | 0.5580 | 2 / 2 |
| 21 | then 2 to 0 | 14.3034 | 4.8794 | 16 / 16 |
| 24 | swivel branch 0, then 0 to 2 | 18.0840 | 4.8908 | 20 / 20 |

Level 24's preceding swivel also passed: 0.5045 seconds, 2/2 presented events. The six transfer measurements above are retained in [busy-times.csv](evidence/busy-times.csv); [after-moves.csv](evidence/after-moves.csv) also includes the swivel, repeated Undo/replay, and board-rebuild cases.

The four slow moves have parallel critical-path estimates of 7.18, 7.79, 7.98, and 8.50 seconds; their scales are approximately 0.6964, 0.6418, 0.6266, and 0.5882. See [playback-budget.csv](evidence/playback-budget.csv). The estimate table reflects current view primitive durations; changes to those primitives must keep the estimates current. The 0.4 readability floor is intentional, not a promise that arbitrary future queues always fit five seconds.

The tap-acceleration case measured 4.8510 seconds without a skip versus 1.7617 seconds with repeated pointer clicks. It checks identical Core results, all event identities completed exactly once, no queued selection, unchanged global time scale, and restored default scheduler. See [acceleration.csv](evidence/acceleration.csv).

## Clock lifetime correction found by retained tests

The normal suite exposed two stalls after a board rebuild. LitMotion's `ManualMotionDispatcher.Reset()` resets sparse handle versions to 1. A subsequently destroyed old view can therefore hold a handle that aliases a new queue's handle after reset.

The queue now keeps its clock and handle generations monotonic across cancellation/rebuild. Existing view owners cancel their own handles; the dispatcher resets only when `BoardView` is destroyed. `RebuildingBeforeDeferredDestructionKeepsNewPlaybackAlive` exercises immediate new playback before old objects are destroyed. Both originally failing retained cases passed after this correction, without changing their assertions or deadlines.

## Verification

All Unity invocations used this worktree's project path and `flock /tmp/unity.lock`; the primary-checkout Editor was not touched.

| Run | Result | Evidence |
| --- | --- | --- |
| Naming guard self-tests | 14/14 passed | Command below |
| Naming guard | 217 tracked text files, zero violations | Command below |
| Initial failing regression run | 1/7 passed, 6 failed, zero skipped; expected reproduction | [before-results.xml](evidence/before-results.xml) |
| Baseline per-event measurement run | Four cascade cases observed to completion, all failed the six-second assertion as expected | [before-events.csv](evidence/before-events.csv) |
| First rendered repair smoke | 9/9 passed, zero skipped | Superseded by final acceptance evidence |
| Required normal suite, run once | EditMode 318/318 passed; PlayMode 129/131 passed, 2 failed, zero skipped | [editmode-results.xml](evidence/editmode-results.xml), [normal-playmode-results.xml](evidence/normal-playmode-results.xml) |
| Final targeted rendered verification after clock correction | 12/12 passed, zero failed/skipped | [after-results.xml](evidence/after-results.xml) |

The normal run's two failures were:
- `FlightFramesTests.BoardTransfersAndHomewardFlightsUseDirectionalFrames`: callback did not complete.
- `GameplayTests.EveryMechanicSettlesAfterRealTimedPlayback`: playback did not settle within its existing 20-second deadline.

Both are included in the final 12/12 run, alongside all ten `InputLockTests`. The latter compares independent Core sessions and visible board birds/seats/flags/home state, and checks exact committed/presented event identity multisets. **No retained tests or pinned durations were updated.** Old/new retained-test assertion list: none.

EditMode discovery retained all 81 `LevelsDataTests` cases and 41 `LayoutTests` cases. No discovery safeguard was lowered.

Two intermediate invocations stopped during compilation of the new fixture: its asmdef does not reference LitMotion, and `SafeArea` was ambiguous. Fixed within the owned test file using scheduler reflection and a qualified `PerchSort.Game.SafeArea`; no asmdef changes or warning suppression.

Commands, from this worktree root:

```bash
python3 ci/check_current_game_names.py --self-test
python3 ci/check_current_game_names.py
flock /tmp/unity.lock bash scripts/run_tests.sh
```

Targeted invocations used:

```bash
UNITY=/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity
PROJECT=/Users/apple/orca/workspaces/game/input-lock-fixes/PerchSort
ROOT=/Users/apple/orca/workspaces/game/input-lock-fixes

# Initial failing tests, then a separate per-event measurement run:
flock /tmp/unity.lock "$UNITY" -batchmode -nographics -projectPath "$PROJECT" \
  -runTests -testPlatform PlayMode -testFilter PerchSort.PlayModeTests.InputLockTests \
  -testResults "$ROOT/build/input-lock-before.xml" -logFile "$ROOT/build/input-lock-before.log"
INPUT_LOCK_EVIDENCE_DIR="$ROOT/build/input-lock-before" \
flock /tmp/unity.lock "$UNITY" -batchmode -nographics -projectPath "$PROJECT" \
  -runTests -testPlatform PlayMode -testFilter 'PerchSort.PlayModeTests.InputLockTests.*Cascade*' \
  -testResults "$ROOT/build/input-lock-measurements.xml" -logFile "$ROOT/build/input-lock-measurements.log"

# Final verification with actual rendering; no -nographics:
INPUT_LOCK_EVIDENCE_DIR="$ROOT/build/input-lock-final" \
INPUT_LOCK_CAPTURE_DIR="$ROOT/build/input-lock-final-frames" \
flock /tmp/unity.lock "$UNITY" -batchmode -projectPath "$PROJECT" \
  -runTests -testPlatform PlayMode \
  -testFilter 'PerchSort.PlayModeTests.InputLockTests;PerchSort.PlayModeTests.FlightFramesTests.BoardTransfersAndHomewardFlightsUseDirectionalFrames;PerchSort.PlayModeTests.GameplayTests.EveryMechanicSettlesAfterRealTimedPlayback' \
  -testResults "$ROOT/build/input-lock-final.xml" -logFile "$ROOT/build/input-lock-final.log"
```

## Timed visual evidence

Captured actual Game camera frames at 0.25-second sampling targets using real elapsed time. Contact sheets retain half-second samples and the observed unlocked final frame; labels use actual timestamps, not synthetic capture time:
- [Level 11](evidence/level-11-playback.jpg)
- [Level 15](evidence/level-15-playback.jpg)
- [Level 21](evidence/level-21-playback.jpg)
- [Level 24](evidence/level-24-playback.jpg)

[frames.csv](evidence/frames.csv) retains timestamps, busy and airborne flags. [after-events.csv](evidence/after-events.csv) records each completed event with its measured elapsed duration. Parallel durations must not be summed as wall-clock busy time. Baseline event move indices continue across fixtures; final records include test names and reset indices per fixture.

## Repository state and limits

The worktree was clean before Unity apart from the deliberately added tests/instrumentation. Unity import/build generated 907 resource `.meta` changes and a Game scene reserialization. These generated files are excluded from commits and preserved separately in ignored `build/unity-generated.patch`; no unrelated user data was restored or overwritten. Package/project settings had no tracked changes in the recorded status. Only the three owned runtime files, the new test/meta, and this subject's specification/report/evidence are task changes.

The required normal suite was run once. It was not repeated after the clock-lifetime correction; final affected-path verification is the 12/12 rendered run, not a claim of a second complete green suite. No device, physical touch, audio/listening, or all-level traversal acceptance is claimed. No stable-state screenshot sweep or UI redesign was performed. No requested ownership expansion remains.
