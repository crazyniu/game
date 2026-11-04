# Integrated motion evidence

## Method and limits

Captured the integration tip in Unity 6000.6.7f1 PlayMode, with real scene and resources, `GameMotion.Instant = false` during each observation and `Random.InitState(12345)`. The temporary capture test used a 540×960 camera render target and `Time.captureDeltaTime = 1/60` for deterministic game-time sampling; labels are actual `Time.time` elapsed, not wall-clock timings. One frame's quantization can put a sample roughly 0.017 s after its target. JPEG sheets are 80 quality and at most 1600 px wide. Captures are not a physical-device or real-time performance assessment.

The level entrance was started via the map level-card Play button. The nonpersistent fixture requires notifications enabled to invoke the actual entrance; the capture turns notifications off once the entrance starts to prevent unrelated first-visit popups. The idle observation begins after that entrance, at five seconds after Play. The house observation reloads level 1 as an instant fixture, then uses normal motion for the entire 15-second observation and invokes the real move at 3.0 s. Thus its first three seconds show empty houses on a freshly loaded, already-settled level, not the entrance animation; the separate entrance sheet shows the actual level start. The win panel is invoked with `Win.Show(3, 30, 10)` after the first delivery, rather than by completing the level. Settings are opened via `OpenSettings` on level and map. No persisted profile was used.

A first temporary capture attempt timed out after 180 seconds because a first-visit popup kept the board modal; it was corrected as described above. The successful temporary capture test passed 1/1. Its test file and generated meta were removed before the full suite.

## Observations

- `evidence/entrance_5s.jpg` and `entrance_5s.csv`: 51 frames, 0.1 s targets. The first sampled playable state is 3.8000 s; 3.7167 s is still locked. At 0.500 s the complete board flashes on screen before the trees and birds reset for the entrance at 0.617 s. This is a visible pop. The houses, branches and flying birds then enter over the remaining sequence.
- `evidence/branch_idle_20s.jpg` and `branch_idle_samples.csv`: 81 tiles at 0.25 s targets, with four bird rows per tile. Sampled sprites include `sunny_turn_left_01/02/04`, `sunny_turn_right_01/03/04`, `sunny_blink_01/02/03`, `sunny_preen_01..05`, `sunny_hop_03/06`, `sky_hop_01/04/06`, `sky_preen_02/04/05`, and `sky_turn_left_02/03/04`. Sampled body y scales span 1.0000–1.0200 for birds 0–2 and 0.9891–1.0200 for bird 3. A tutorial hand overlaps the fourth bird and branch tip throughout. These samples do not establish action cadence between frames.
- `evidence/branch_idle_zoom_actions.jpg`: 12 tiles each at 0.1 s targets for the same sunny bird: look (3.717–4.800 s), hop (8.867–9.967 s), blink (14.067–15.150 s), preen (18.717–19.817 s). The hop visibly lifts off the seat; look, blink and preen change drawn poses. A neighboring blue bird partially occludes the right edge of the crop. No additional shake or floating at rest is established by these samples.
- `evidence/houses_delivery_15s.jpg` and `houses_delivery_samples.csv`: both doorways are empty through the move at 3.0 s. The sky delivery appears in the right doorway from 5.0 s, followed by the check at 6.25 s. Sampled resident/flight sprites include `sky_fly_down`, `sky_hop_03/06`, `sky_idle`, `sky_blink_01..03`, `sky_peek_01/03/04`, and `sky_turn_right_02/03/04`. The 0.25 s samples cannot prove every intervening frame or a smooth transition.
- `evidence/win_panel_10s.jpg` and `win_panel_samples.csv`: 0–4 s at 0.1 s targets, then 4.5–10 s at 0.5 s targets. **Defect: the blue side bird, pink side bird and yellow hero are not visible on the rendered settled panel**, even though the active image components report changing sprites. The CSV records `sky_cheer_01..06`, `berry_cheer_01..06`, `sunny_flap_01..06`, plus sampled `sky_hop` and `berry_hop` frames. Component sprite changes are not visual proof of animation when the sprites are absent from the rendered panel. The sheet also shows the panel and stars popping in, counters and buttons settling, and rays continuing behind it.
- `evidence/settings_level_and_map.jpg` and `settings_samples.csv`: 0–0.5 s at 0.05 s targets plus 1.0 s settled state on each surface. The three controls appear together while the panel opens; no settings birds are present.

## Negative x scales

The CSV `negative_x` column lists all active image-bearing transforms with a negative **local** x scale at each sample, not only birds. `Edge trunk`, `Leaf crown` and two `Branch` transforms are negative in the level captures; the entrance transition additionally samples `Leaf curtain 1`, `Leaf curtain 3` and `Leaf curtain 5`. Therefore the addendum's literal prohibition on negative x scale everywhere is not met. No bird-named image transform was reported negative in these samples, and the sampled branch body `scale_x` values are positive. This is a sampled local-scale check, not a continuous or effective world-transform audit.

## Verification

- `python3 ci/check_current_game_names.py --self-test`: 8 passed.
- `python3 ci/check_current_game_names.py`: 1514 tracked text files, 0 violations, before Unity.
- Temporary capture: serialized Unity PlayMode batch run filtered to the temporary capture test, with graphics enabled: 1 passed, 0 failed, 0 skipped.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`, after temporary-file removal: scene build succeeded; EditMode 312/312 passed, 0 failed, 0 skipped; PlayMode 51/53 passed, 2 failed, 0 skipped. The runner exited 1.
  - `FocusAndApplicationPauseFreezePlayheadsUntilBothResume`: expected 1.13065755 ± 0.02 s; actual 1.15199542 s.
  - `BreathingPersistsBetweenActionsWithoutMovingLogicalSeatsAndInstantFreezesIt`: scale equality failed; NUnit formatted both vectors as `(1.00, 1.00, 1.00)`, concealing the underlying difference.
- The naming self-test and Unity runs left 124 tracked generated files modified in this worktree, including bird importer metadata, `Game.unity`, and Python bytecode. They were not committed or blindly restored.

## Requested contract change

None. Product code was not changed. The visual defects and evidence limitations above require coordinator review, not a QA-only fix.
