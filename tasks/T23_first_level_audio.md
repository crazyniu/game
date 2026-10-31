# T23 — First-level audio verification

Base: PR7 head 2c94081eb783a08a9abe31139dfd17feddd5d4aa. Author only on251; test only in disposable headless Mac Unity Editor. Preserve t21/t22 and the pending Player-window authorization. No Player/CUA, system recording, microphone, drivers, packages, later levels, storage or scene/UI redesign. Do not commit/push.

Read AGENTS/CLAUDE, README/router/evidence, UI_SPEC global mute, INTERACTION sound, DATA_SAVE 5.1, Entry/Flow/Hud/Board/AudioManager and existing audio tests. Verify real resource decoding, first-level events, global mute/recovery, failed-save session choice and cancellation. Only fix a reproduced audio defect. Existing audio/UI test files are uncommitted in t21 and were not transferred.

Public Unity AudioRenderer.Start/Render/Stop may produce an explicitly labeled engine-only sample in the isolated Editor after checking official6000.6 documentation. It is not a live Player recording. No existing video is modified. Source sample rate/gains stay unchanged. Keep test artifacts outside version control; deliver hashes, exact results and limitations for independent coordinator review.
