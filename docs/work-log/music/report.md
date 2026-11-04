# T54A — background music

## Scope

- Branch: `mac-t54a/t54`, created at the supplied `t54-contract` tip.
- Runtime changes are restricted to `Assets/Scripts/Game/GameRoot.cs` and `Assets/Scripts/Audio/AudioManager.cs` under `PerchSort`.
- Added `PerchSort/Assets/Tests/PlayMode/MusicTests.cs` and its Unity metadata. Existing tests and assembly definitions are unchanged.
- No artwork, resource, save identifier, game rule, level, scene, or project-setting change is intended.

## P2 implementation

| Surface/state | Music behavior |
| --- | --- |
| Initial map and all map worlds | `bgm_menu`, looping |
| Level, entrance, restart, and win overlay | `bgm_game`, looping |
| Map level card, settings, information/reward popups | Preserve the underlying page's clip and playhead; no music restart |
| Map/level transition | Linear, unscaled-time crossfade over 1 second; outgoing source stops at completion |
| Existing mute-only control | Applies to music and effects together; preserves `ProgressStore.MutedKey` and its existing storage value |
| Focus loss/application pause | Mute output and pause music sources and fade progress; resume only when focused and not paused |
| First pointer/touch or keyboard interaction | Retry a stopped current music source; never restart an already-playing source |
| Root cleanup with a borrowed AudioManager | Restore previous effects/music enabled state and previous page, including explicit no-music state |

Music assets already exist at `PerchSort/Assets/Audio/Resources/bgm_menu.wav` and `bgm_game.wav`. Their valid Resources keys remain `bgm_menu` and `bgm_game`; no asset was missing or generated. GameRoot previously stopped music at initialization and never selected the current page track.

## PlayMode acceptance coverage

`MusicTests` uses the genuine Game scene, GameRoot, AudioManager, and shipped clips. AudioSource inspection avoids changing the existing PlayMode assembly's references. Focus/pause callbacks are driven with Unity SendMessage; music is not mocked. Profile keys are preserved and restored by fixture teardown.

1. Menu/level/map clip routing, looping, actual playback, and approximately 1-second crossfade, including `Time.timeScale = 0`.
2. Level card, settings, popup, win, and restart preserve the page music playhead.
3. Mute applies to all sources across a page change and survives scene reload; unmute restores playback.
4. Focus loss freezes the playhead; focus restoration does not resume while application pause is still active.
5. Page change while suspended resumes its pending fade without counting elapsed background time.
6. User-interaction retry starts a stopped source without resetting an active track.

Each scenario logs timestamped `T54A` lines containing page, phase, clip, playing/loop/mute flags, gain, and playhead. These are timed audio evidence, not an animation/contact-sheet claim.

## Verification

- `python3 ci/check_current_game_names.py`: passed, 569 tracked text files inspected, zero violations before the implementation commit.
- Before Unity: scene, project settings, and package diff was empty. The user's primary-checkout Editor was initially open; it was not killed. User/coordinator confirmed closure, and process inspection then found no Editor.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`: compilation and scene build completed; EditMode **303/304 passed, 1 failed, 0 skipped** (14.807 seconds). The one failure is the intentional timing mismatch described below. The script stops on EditMode failure, before PlayMode.
- Complete PlayMode suite: **27/27 passed, 0 failed, 0 skipped** (60.446 seconds), including all 21 retained cases and six new music cases. Exact command:

  ```bash
  flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -projectPath /Users/apple/orca/workspaces/game/t54-music/PerchSort -runTests -testPlatform PlayMode -testResults /Users/apple/orca/workspaces/game/t54-music/build/playmode-results.xml -logFile /Users/apple/orca/workspaces/game/t54-music/build/playmode-tests.log
  ```

- Diagnostic-only follow-up: avoid querying `AudioSource.time` for an unassigned clip in the evidence formatter. Final targeted rerun: **6/6 passed, 0 failed, 0 skipped** (16.390 seconds). Exact command:

  ```bash
  flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -projectPath /Users/apple/orca/workspaces/game/t54-music/PerchSort -runTests -testPlatform PlayMode -testFilter PerchSort.PlayModeTests.MusicTests -testResults /Users/apple/orca/workspaces/game/t54-music/build/music-results.xml -logFile /Users/apple/orca/workspaces/game/t54-music/build/music-tests.log
  ```

- Committed timed evidence: [`docs/work-log/music/evidence/music-playmode.log`](evidence/music-playmode.log). It includes the full-suite counts, retained failure, and final six-case timestamped transport logs. For example, level crossfade begins at 9.199 seconds, both sources are playing and unmuted at 9.600 seconds (menu gain 0.117, game gain 0.083), and the game track is at full gain with menu stopped at 10.350 seconds. These are source-output states, not an acoustic listening claim.
- Local raw outputs: `build/scene-build.log`, `build/test-results.xml`, `build/editmode-tests.log`, `build/playmode-results.xml`, `build/playmode-tests.log`, `build/music-results.xml`, and `build/music-tests.log`.
- Unity generated changes in `Assets/Scenes/Game.unity` and seven Bird overlay `.png.meta` files (`chick_fluff`, five egg frames, `mama_bow`). All were clean before the run. Preserved their final binary-capable diff in ignored `build/unity-generated.patch`, then restored only those generated paths. No generated scene/art metadata changes are included in this task branch.

## Requested contract change

The retained `PerchSort.Tests.Audio.AudioManagerTests.MusicSwitchHonorsPendingPlayAndExplicitStop` sets `fadeStart` to `Time.unscaledTime - .5f` and expects the crossfade to be complete. The approved target changes that duration from 0.5 seconds to 1 second. The shared task contract forbids modifying existing tests for intentional timing changes, so this assertion remains unchanged. Coordinator authorization is needed to update that assertion after review; no test has been weakened or skipped.

## Remaining limits

- Mac PlayMode validates source transport and timing, not acoustic speaker/headphone quality or device volume calibration.
- A browser's actual autoplay policy and first-touch audio-context unlocking require WebGL/device acceptance. The PlayMode retry scenario exercises the real stopped source, not a browser security context.
- The full integrated game still requires coordinator acceptance after the other task branches are integrated.

## Follow-up: deterministic focus/pause regression

- Merged `origin/t54-integration` first; the worker branch fast-forwarded from `c1fd9e6b` to integrated base `a7679d97`.
- The coordinator observed a flaky paused-playhead comparison: expected `1.13065755 +/- 0.02`, actual `1.15199542`, followed by a passing run. This report accepts that observation; the old assertion was not rerun to reproduce a known failure.
- Only `T54AMusicTests.FocusAndApplicationPauseFreezePlayheadsUntilBothResume` changes. Production audio behavior is unchanged.
- Replaced float-second comparisons with integer `AudioSource.timeSamples`. The pause command is asynchronous to the audio mixer: wait one actual DSP-buffer duration and allow at most `ceil(bufferSamples * clip.frequency / AudioSettings.outputSampleRate)` samples between the immediate pause request and the settled reference. This bound comes from the active audio configuration, not an enlarged arbitrary tolerance.
- After that single pause boundary, require the sample position to be exactly equal to the settled reference on every rendered frame throughout both pause phases. The second phase still verifies that focus restoration cannot override application pause. Hold durations use `Time.unscaledTimeAsDouble`, with no global capture-time changes.
- Sample the reference and resumed playhead on the two sides of `UnPause` in the same frame. Continuity tolerance is `floor(clip.frequency / 60)` samples, at most one 60 Hz frame. After a real 0.25-second hold, require the real source to be playing and its playhead to have advanced.
- Before Unity, the worktree was clean apart from this task's test edit, no Unity Editor was running, and the Unity lock was unused. Naming policy self-tests passed **8/8**; the integrated naming guard inspected **1514** tracked text files with zero violations.
- Validation: **10/10 passed**, zero failed or skipped iterations. Ran ten fresh Unity PlayMode launches under `flock /tmp/unity.lock bash build/run-music-repeat.sh`, each using `-testFilter PerchSort.PlayModeTests.T54AMusicTests.FocusAndApplicationPauseFreezePlayheadsUntilBothResume`. Each iteration's XML contains exactly that one discovered and passed case; the runner stops on any failed launch or invalid XML.
- Committed evidence: [`music-pause-repeat.log`](evidence/music-pause-repeat.log), with the exact per-run Unity invocation, counts, and sample/time logs from all ten runs. Raw XML/logs remain locally in `build/music-pause-repeat/`; the temporary runner was removed after use.
- Active clip frequency was **44100 Hz**. The derived one-DSP-buffer bound was **941 clip samples**. Six runs observed a 941-sample transition at the initial pause boundary and four observed zero. Every subsequent per-frame assertion required exact sample equality, and all ten immediate resume comparisons observed **zero samples** of displacement. Resume tolerance was **735 samples**, exactly 1/60 second.
- Unity reserialized 121 Bird sprite metadata files; the naming self-tests also regenerated two tracked Python cache files. All were clean before this follow-up. Their diffs were preserved locally under `build/music-pause-repeat/` and only those generated paths were restored; none is included in the follow-up commit.
- Boundary: this test-only follow-up ran the requested filtered test ten times, not the full integrated EditMode/PlayMode suites. Earlier full-suite counts above remain historical and are not presented as validation of the integrated branch.
