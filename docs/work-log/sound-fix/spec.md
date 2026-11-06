# Sound investigation

Investigate the silent Unity Editor game from `origin/main` in the isolated Mac worker checkout. Preserve existing profiles and mute choices. Make the missing-key mute default explicitly OFF; preview and capture states must not write the real mute preference.

Inspect runtime AudioManager/sources, Resources clips, map/board music, gains/mixer routing, listener state, project audio enablement, and the Editor Game view mute setting. Use a silent batch probe (`AudioListener.volume = 0` only during the probe) through the canonical Game scene, real map and `LoadLevel(1)`. Exercise the Settings disc and preview return/exit without running EditMode or PlayMode suites.

The probe uses a temporary company identity in this worker's project settings and a copy of the existing PlayerPrefs. Restore the project settings and verify that the original preference file is byte-identical afterward. Commit the captured log and investigation report here; push only `mac-sound-fix/work`.
