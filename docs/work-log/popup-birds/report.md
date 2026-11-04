# T54F report — popup and birdhouse presentation

## Scope and contract

- Branch: `mac-t54f/t54`, created at the supplied contract tip with `git switch -c mac-t54f/t54`.
- Integrated the requested `origin/t54-contract` updates through `9826515`, including the binding exact action tables and the empty-goal clarification.
- Runtime ownership respected: only `WinPanel.cs`, `PopupMotion.cs`, `BirdhouseView.cs`, and new `UiBirdLife.cs` changed under `Assets/Scripts/Game`. `PopupFlow.cs` did not need a change. `BirdView`, `BirdIdleGroup`, Core, Meta, artwork, resource metadata, default layout constants, and other workers' files were not edited.
- New permanent tests are confined to `Assets/Tests/PlayMode/PopupBirdTests.cs` and `PopupBirdSequenceTests.cs`, with their required Unity metadata. No existing test was edited, weakened, or skipped.

## Changes by problem

### F1 — popup birds and the frame-sequence addendum

`UiBirdLife` probes `GameArt.Optional` for `<species>_<action>_01`, `_02`, and subsequent frames until the first missing frame. Clips are cached at configuration, and playback performs no per-frame collection allocation. There is no mirroring or rotation-as-action.

- Side birds continuously play `cheer` at 10 fps. WinPanel schedules one 0.5 s `hop` at a time, alternates left/right, and chooses the next start 1.4–2.2 s later. Real hop sequences support an 18 px eased arc and a 0.10 s landing squash.
- The hero continuously plays `flap` at 14 fps, with the specified +/-6 px hover over 1.2 s only when the drawn sequence exists.
- Missing sequences use the nearest existing still, with breathing rather than a fabricated hop, flap, hover, tilt, shake, or mirror. This is an explicitly exercised fallback, not a claim that missing drawings have been delivered.
- All three entrances last 0.6 s, follow an outside-screen curved path, cross-fade the existing `fly_up`/`fly_down` poses every 0.12 s, and land with a 0.10 s `(1.06, 0.94)` squash.
- Close, hide, disable and cancellation stop the reusable players before restoring tracked popup poses. The player uses `Update`, not coroutines. `GameMotion.Instant` restores a motionless rest pose.

### F2 — button proportions

The win row uses 124 px purple discs and a 156 px green Next disc, ratio `156/124 = 1.2580645`. Centres are `(350, 1348)`, `(530, 1348)`, `(710, 1348)` in design coordinates: equal 40 px edge gaps and a common optical centreline. Existing gold-rim artwork is retained without editing assets. There is no idle Next-size pulse.

This is the current bottom-row green/purple trio. Other cards' separate header close controls are not treated as bottom-row siblings. Shared `PopupMotion.ButtonsLast` now brings a button group in together with a 0.2 s fade and 0.9-to-1 scale, instead of staggering the buttons. Canonical `GameLayout` authored rectangles/default sizes remain unchanged because that file is outside this task's ownership.

### F3 — empty goals, delivery, resident life, undo

- Every fresh house has an empty dark doorway, with no coloured portrait, face, silhouette, or check badge.
- The existing door animation is retained and uses the exact table's 0.25 s timing.
- The coloured resident appears only after the whole matching flock arrives. It plays the 0.5 s drawn hop when available, then the door closes and the check pops in over 0.25 s with ease-out-back.
- A resident breathes from scale 1.000 to 1.020 over 2.6 s. Scheduled blink starts are due every 2–4 s; peek is 8 fps forward, 0.8 s hold, then backward, due every 4–8 s; turns are 10 fps forward, 0.8 s hold, then backward, due every 6–10 s. An active action is not overlapped. A shared house start reservation defers conflicting idle starts to keep at least 0.5 s between houses.
- Undo clears the logical home/check immediately, plays the resident's exit/door transition, and leaves the house empty. Cancellation generations prevent stale callbacks from repopulating it. Instant undo returns directly to empty.

### F4 — exact popup choreography

- Panel: 0.6-to-1 scale, 0.35 s, ease-out-back.
- Ribbon: 80 px drop, 0.30 s, beginning at 0.10 s.
- Stars: 0.18 s stagger, 0.35 s overshoot to 1.25 then settle to 1.0; six sparkle particles per earned star.
- Coin and chain labels: 0.6 s count-up. Reward persistence/navigation remains owned by the existing root/flow; presentation does not grant coins.
- Rays: 6 degrees per second.
- Confetti: 40 pieces falling for 3 s, each using a 400–700 px/s speed and 30–60 px sway.
- Buttons: together after the final star at 1.16 s, fading/scaling into place over 0.2 s.

## Verification

All Unity launches used this worktree and `flock /tmp/unity.lock`. No other worker's process or user Editor was killed.

| Command / run | Observed result |
| --- | --- |
| `python3 ci/check_current_game_names.py` | PASS, 575 tracked text files, zero violations on the committed implementation |
| Initial targeted PlayMode regression run before the implementation | 1/3 passed, 2 failed: a house bird did not move over 4 s; old Next/purple ratio was 1.48571432 rather than the requested range |
| Rendered T54F acceptance, including the temporary timed capture scenario | 7/7 passed, zero failures/skips; `build/ui-birds-acceptance.xml` |
| `flock /tmp/unity.lock bash scripts/run_tests.sh` | PASS: scene build, **304/304 EditMode**, **27/27 PlayMode**, zero failures/skips |

The final full-suite PlayMode count is the retained 21 plus six permanent T54F cases. The six cases exercise: side-bird transform changes over 2 s after landing plus cancellation; delivered-house movement over 4 s and Instant freeze; fresh empty goals, delayed appearance after the real authored delivery, check visibility, and undo; button size ratio/centreline/equal gaps; drawn frame order and return-to-rest; missing-sequence fallback without fake motion or mirroring. Fake sequence frames exist only in the isolated test cache and are not production artwork or visual evidence.

Rendered acceptance command:

```sh
flock /tmp/unity.lock python3 -c 'import subprocess,sys; sys.exit(subprocess.run(sys.argv[1:],timeout=600).returncode)' \
  /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -projectPath /Users/apple/orca/workspaces/game/t54-ui-birds/PerchSort \
  -runTests -testPlatform PlayMode -testFilter T54F \
  -testResults /Users/apple/orca/workspaces/game/t54-ui-birds/build/ui-birds-acceptance.xml \
  -logFile /Users/apple/orca/workspaces/game/t54-ui-birds/build/ui-birds-acceptance.log
```

The temporary `T54FCapture` PlayMode harness loaded the genuine Game scene, initialized a nonpersistent profile, used actual `TryMove(0, 1)` / `TryMove(2, 0)` deliveries, and rendered the real camera to 1080x1920 while motion was non-instant. It was removed before the full retained-suite run. No capture harness remains in compiled assets.

Earlier attempts: two launches timed out after 1200 s waiting for another worktree's Unity lock, before this worktree's Unity process started; these are not passing test results. The lock-owning worker was notified of its own compile-error hang. The first subsequent rendered attempt failed because the temporary capture harness referenced TMPro without that test assembly reference; the harness's redundant TMPro-dependent force-update code was removed, and the corrected rendered run and final full suites passed. No production compile error was suppressed.

## Timed evidence

Committed under [`docs/work-log/popup-birds/evidence`](../../../docs/work-log/popup-birds/evidence). Every JPEG is at most 1600 px wide. Labels use measured Unity runtime timestamps, not invented or interpolated frames.

| Evidence | Sampling |
| --- | --- |
| [`win-entrance.jpg`](evidence/win-entrance.jpg) | 41 frames, 0–4 s, 0.1 s targets |
| [`win-idle.jpg`](evidence/win-idle.jpg) | 12 additional frames, 4.5–10 s, 0.5 s targets |
| [`houses-empty.jpg`](evidence/houses-empty.jpg) | 61 frames, 0–15 s, 0.25 s targets; both houses empty |
| [`delivery.jpg`](evidence/delivery.jpg) | 33 frames, 0–8 s, 0.25 s targets; first authored blue-flock delivery |
| [`houses-resident.jpg`](evidence/houses-resident.jpg) | 61 frames, 0–15 s after delivery, 0.25 s targets; only the delivered blue resident is present |
| [`win-buttons.jpg`](evidence/win-buttons.jpg) | Actual rendered button-row crop, not a mockup |
| [`timing.csv`](evidence/timing.csv) | All 208 target and actual frame timestamps |

Win, delivery and resident captures were within 0.001 s of their target timestamps at the CSV's millisecond precision. The initial empty-house sequence had cold-render/startup hitches, maximum 0.317 s lateness; its actual timestamps are retained rather than presented as exact intervals. Full-resolution source PNGs and local XML/logs remain ignored under `build/`.

## Missing prerequisites and limits

- **T54G sequence artwork was absent in this worktree at verification:** `Assets/Resources/Birds/*_[0-9][0-9].png` returned no files. In particular, sunny/sky/berry `cheer_01..06`, `flap_01..06`, `hop_01..06`, `blink_01..03`, `peek_01..04`, and `turn_left/right_01..04` are not available here. Other species' sequences are absent too.
- Consequently, these real Unity sheets prove entrance choreography, empty/delivered goal state, counters, proportions, cancellation-compatible playback and the permitted still-plus-breathing fallback. They **do not** prove the appearance/quality of the not-yet-delivered drawn cheer/flap/hop/peek/turn sequences. The sequence player is verified with isolated fake sequences and will discover real matching assets on configuration without a code rename or placeholder sequence.
- No artwork or substitute sequence was generated. No claim of completed T54G visual acceptance, touch-device acceptance, or manual Editor interaction is made.
- Scenes, settings, packages and resource metadata are not included in this task's commits. Their input checksums were recorded before the final scene-builder run; generated output remains outside the scoped delivery.

## Requested contract change

None for the reachable implementation. Final drawn-action visual acceptance requires the separately owned T54G assets to be integrated by the coordinator and the timed scenario to be repeated against those actual drawings. This report does not substitute breathing transforms for that missing art.
