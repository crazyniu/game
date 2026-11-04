# T54A Background music (problem P2)
Read tasks/T54_contract.md. Owned: `PerchSort/Assets/Scripts/Game/GameRoot.cs`, `PerchSort/Assets/Scripts/Audio/*`, new `Tests/**/T54A*.cs`.
Implement the Music feel target. Find where pages change (map, level, popups, settings mute) and drive `AudioManager.SwitchMusic`/mute from there with
minimal edits in GameRoot. Keep save compatibility (existing mute key). Report which clip plays on which page, and the PlayMode test proving it.
