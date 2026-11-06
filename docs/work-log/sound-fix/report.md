# Sound investigation report

## Root cause

The existing Mac profile has `ProgressStore.MutedKey` (`v5.muted`) set to **1**. The silent batch run loaded a copy of that profile through the real `GameRoot.Initialize(true, false)`: the Settings state was muted, `SfxEnabled=False`, `MusicEnabled=False`, and every source was muted. The map music source was already playing `bgm_menu` at volume **0.200**. The saved mute choice explains the reported icon and silence.

No current preview persistence leak was reproduced. `PopupFakeData.Apply` enters `BeginBoardPreview` before its muted Settings state calls `SetMuted`; the replacement profile has `persistProgress=false`. The probe verified the entire saved profile fingerprint after returning to the real game and after stopping Play mode while that preview remained muted. Both were unchanged. `GameCapture.Run` and `RunGlyphSizing` initialize with saving disabled and use nonpersistent fixtures. The preference file does not record its writer; there is no evidence identifying which earlier interaction set mute to 1.

## Change

- `ProgressStore.Load` now explicitly uses `PlayerPrefs.GetInt(MutedKey, 0)`: new profiles without the key default to mute OFF. This makes the existing implicit default explicit. It does not reset an existing mute choice or any progress.
- Added the batch-only `GameAudioProbe` and its Editor assembly Audio reference, plus `scripts/probe_sound.sh` to reproduce the investigation safely.
- No playback-code defect was found on the fetched main branch. Music routing, mute behavior, assets, scene and project audio settings required no repair. **The existing user profile remains muted; turn its Settings sound disc on once to enable sound.**

## Observed audio state

Captured evidence: [probe.log](probe.log). Unity **6000.6.7f1**, base `09b8ecf734b01dd01fabcf48f37a3261ee76afeb`.

| Check | Observed result |
| --- | --- |
| Real scene | `Assets/Scenes/Game.unity`; runtime `Game audio` manager; 8 effect sources and 2 music sources |
| Real map after Settings unmute | `bgm_menu`, playing, looped, volume 0.200, mute false; playhead 2.709 → 3.093 seconds |
| Map effect | `ui_tap`, playing, volume 0.140, mute false |
| Real first board | Entered through `LoadLevel(1)`, `preview=False`; `bgm_game`, playing, looped, volume 0.200, mute false |
| Board effect | `chirp_04`, playing, volume 0.140, mute false |
| Settings mute/unmute | Both music and effects follow the disc; painted icon switches between `icon_sound_off` and `icon_sound_on`; music playhead continues |
| Resource loading | All 16 named sound keys, both music clips and error/undo fallback clips found; game music initially streamed on demand, then played on the board |
| Mixer/gain | Sources route directly to Unity output, no mixer group; project master volume 1.000; 2D sources (`spatialBlend=0`) |
| Listener | Exactly one enabled AudioListener; pause false; original volume 1.000; volume 0.000 during every probe snapshot, restored after Play mode |
| Project audio | Disable Unity Audio false |
| Game view mute | `EditorUtility.audioMasterMute=False` in the worker process; never changed by the probe |
| Saved-profile safety | Original preference file byte-identical before/after; SHA-256 `2e89cca11437b7bad67b76cd90504aaf27036f26cc4b36d95f0884a40680873d` |

The launcher temporarily changes only this worker's company name to `PerchSortSoundProbe`, seeds that separate preference domain from the existing profile, and restores project settings afterward. The ordinary GameRoot reads/writes the isolated copied profile. Notification popups are suppressed to enter the first board directly; neither the map nor board playback evidence uses fake board data. Preview data is used only in the explicitly labelled preview checks.

Batch mode has no focused Game view (`Application.isFocused=False`); after logging the untouched startup state, the probe explicitly sends the foreground focus/pause callbacks for the playback samples. Speakers remain silenced by `AudioListener.volume=0`. This proves the source playback and control path, not audible speaker/device acceptance.

The interactive primary Editor's current Game view toggle was not directly observed. Its toolbar controls `EditorUtility.audioMasterMute`, a separate Editor setting that does not select the game's muted icon. See [Unity's GameView source](https://github.com/Unity-Technologies/UnityCsReference/blob/6000.6/Editor/Mono/GameView/GameView.cs#L648). No code forces that user setting.

## Verification

```bash
python3 ci/check_current_game_names.py --self-test
python3 ci/check_current_game_names.py
bash -n scripts/probe_sound.sh
flock /tmp/unity.lock bash scripts/probe_sound.sh
git diff --check
```

- Naming guard self-tests: **14 passed**; naming scan: **0 violations**.
- Unity compile and silent probe: **PASS, exit 0**. The first diagnostic compilation needed an explicit `PerchSort.Audio` reference in the Editor assembly; corrected before the successful run.
- EditMode and PlayMode suites: **NOT RUN**, as explicitly requested. No tests were disabled or changed.
- Unity emitted an unrelated `UnityEditor.Search.SearchDatabase` indexing exception during startup and a persistent-allocation warning at shutdown. Captured excerpts are appended to `probe.log`; no game/audio exception or probe failure occurred. Full local engine output: `build/sound-probe/unity.log`.
- Recorded 1,923 scene/meta/settings/package hashes before Unity. Restored 925 import-only metadata rewrites and removed 201 generated resource metadata files. All 1,923 hashes match afterward. No art, scene, package, save schema, primary-checkout file, or existing profile was changed.

## User verification

1. After coordinator integration, open `Assets/Scenes/Game.unity` and press Play. Use **Tools > UI States > Return to real game** if a preview is active.
2. Open Settings and tap the sound disc once so it shows sound on. This is needed for the existing saved muted profile; do not reset progress. Ensure the Editor Game view's **Mute Audio** toggle is off and focus the Game view.
3. Listen on the map, enter level 1, and tap birds. Music should change to the board track and effects should play. Mute and unmute in Settings; both categories should follow the disc. Stop and restart Play mode to confirm the chosen setting persists.
4. Select **Tools > UI States > Popups > Settings / Muted**, then **Return to real game**. The real mute choice must return. Stopping Play mode during that preview must also preserve it.

Requested contract exception: this task's explicit no-suites instruction overrides the older shared contract's full-suite requirement. No other contract change is requested. Audible primary-Editor/device verification remains with the user/coordinator.
