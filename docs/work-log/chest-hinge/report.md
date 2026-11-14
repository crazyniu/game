# Chest motion implementation and verification

## Cause and changes

The former four complete chest poses blended opacity but did not produce a continuously opening lid. Their differing box silhouettes made the whole box morph. The reward transition also changed box size and moved its footline 172 pixels in one step while two full pages briefly faded over each other.

The chest now draws the original fixed box and star lock with a separately generated, matching outer lid and the original inner lid. The two opaque faces are projected continuously around the opening angle on each rendered frame. This is stylized 2.5D, not a physical 3D mesh. Anticipation, eased swing and settling retain existing sounds and mute rules. The final box/support move smoothly to the reward layout before the original transaction callback runs. The completed page is hidden when the reward page replaces it without another whole-page fade.

Reward icons use source-verified alpha bounds instead of a first-use GPU image readback. An independent 300 ms confirmation gate prevents an opening tap burst dismissing the new reward, even without a fade. No prices, reward counts, progress, profile fields or save implementation changed.

## Validation

- `scripts/run_tests.sh`: passed; naming/painted-word/launcher guards, scene smoke, core and recovery, bird animation, 94 goal checks.
- `tests/run_chest.gd`: normal and 15/30/60 FPS limits passed, zero failures. Covers 0/1/multiple claims, disabled input, cancel and scene destruction before commit, mute, repeated Open, continuous input through the reward boundary, confirmation recovery, exact reward amounts, re-entry, locales, real navigation and source-alpha bounds. Transform assertions check the pivot-based centered foot anchor.
- Actual GL Compatibility desktop captures: both languages, closed/intermediate/open/reward states, 1080x1920 and 720x1280 windows. The initial prototype pivot displacement was found visually and corrected before delivery. Frame sequences use real timestamps; recording readback itself costs time, so the video is not a benchmark.
- Independent review found a confirmation pass-through regression when skipping the fade; the explicit gate and boundary input test were added before submission.

## Timing evidence and limits

Godot 4.7.2, GL Compatibility, Mesa llvmpipe software rendering, 1080x1920, no screenshots during timing. Three alternating process runs per implementation, each with cold and warm opening, fixed random seed, task-only XDG data and **real `Profile.save` enabled**. Earlier trial logs with capture-driver setup errors were discarded and rerun. Each final run reported the isolated save file present and no script errors.

| Run | Prior cold p95/max ms | New cold p95/max ms | Prior warm p95/max ms | New warm p95/max ms |
| --- | --- | --- | --- | --- |
| 1 | 48.15 / 142.35 | 34.72 / 46.96 | 41.67 / 48.11 | 34.04 / 87.62 |
| 2 | 36.56 / 43.69 | 46.41 / 121.40 | 39.51 / 115.55 | 50.00 / 142.50 |
| 3 | 44.44 / 53.19 | 32.20 / 83.14 | 49.42 / 103.30 | 41.88 / 119.66 |

Cold synchronous page construction was 565/442/431 ms before and 520/339/317 ms after; warm entry was 1.5–3.2 ms before and 1.5–1.6 ms after. Median frame times remain approximately 28–32 ms. These mixed results do **not** establish that all hitches are fixed or that a user device achieves 60 FPS. The definite visual change is continuous geometry rather than four-pose opacity mixing. Source loading still has a substantial cold-entry cost in this software-rendered environment.

Separately, isolated headless `Profile.save` samples were 0.37–0.50 ms; they do not prove disk performance on the user's device. Audio event/mute behavior was tested, but audio was not listened to. No real user save was read or modified. User hardware and subjective motion acceptance remain unverified.
