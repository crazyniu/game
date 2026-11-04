# T54A — background music

## Scope

- Branch: `mac-t54a/t54`, created at the supplied `t54-contract` tip.
- Runtime changes are restricted to `Assets/Scripts/Game/GameRoot.cs` and `Assets/Scripts/Audio/AudioManager.cs` under `PerchSort`.
- Added `PerchSort/Assets/Tests/PlayMode/T54AMusicTests.cs` and its Unity metadata. Existing tests and assembly definitions are unchanged.
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

`T54AMusicTests` uses the genuine Game scene, GameRoot, AudioManager, and shipped clips. AudioSource inspection avoids changing the existing PlayMode assembly's references. Focus/pause callbacks are driven with Unity SendMessage; music is not mocked. Profile keys are preserved and restored by fixture teardown.

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
  flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -projectPath /Users/apple/orca/workspaces/game/t54-music/PerchSort -runTests -testPlatform PlayMode -testFilter PerchSort.PlayModeTests.T54AMusicTests -testResults /Users/apple/orca/workspaces/game/t54-music/build/music-results.xml -logFile /Users/apple/orca/workspaces/game/t54-music/build/music-tests.log
  ```

- Committed timed evidence: [`tasks/evidence/T54A/music-playmode.log`](evidence/T54A/music-playmode.log). It includes the full-suite counts, retained failure, and final six-case timestamped transport logs. For example, level crossfade begins at 9.199 seconds, both sources are playing and unmuted at 9.600 seconds (menu gain 0.117, game gain 0.083), and the game track is at full gain with menu stopped at 10.350 seconds. These are source-output states, not an acoustic listening claim.
- Local raw outputs: `build/scene-build.log`, `build/test-results.xml`, `build/editmode-tests.log`, `build/playmode-results.xml`, `build/playmode-tests.log`, `build/music-results.xml`, and `build/music-tests.log`.
- Unity generated changes in `Assets/Scenes/Game.unity` and seven Bird overlay `.png.meta` files (`chick_fluff`, five egg frames, `mama_bow`). All were clean before the run. Preserved their final binary-capable diff in ignored `build/unity-generated.patch`, then restored only those generated paths. No generated scene/art metadata changes are included in this task branch.

## Requested contract change

The retained `PerchSort.Tests.Audio.AudioManagerTests.MusicSwitchHonorsPendingPlayAndExplicitStop` sets `fadeStart` to `Time.unscaledTime - .5f` and expects the crossfade to be complete. The approved target changes that duration from 0.5 seconds to 1 second. The shared task contract forbids modifying existing tests for intentional timing changes, so this assertion remains unchanged. Coordinator authorization is needed to update that assertion after review; no test has been weakened or skipped.

## Remaining limits

- Mac PlayMode validates source transport and timing, not acoustic speaker/headphone quality or device volume calibration.
- A browser's actual autoplay policy and first-touch audio-context unlocking require WebGL/device acceptance. The PlayMode retry scenario exercises the real stopped source, not a browser security context.
- The full integrated game still requires coordinator acceptance after the other task branches are integrated.
