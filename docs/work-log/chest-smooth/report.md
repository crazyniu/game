# Chest pose continuity and resource preparation

## Diagnosis

Main e838612 holds four separate poses for 180 ms each (about 5.6 pose changes per second). This creates a stepped visual independently of renderer FPS. The first opening callback also loads the atlas, constructs frame resources and first-decodes audio. Reward art can decode in the completion callback.

## Changes

Prepare atlas regions and audio when entering the page; prepare the finite possible reward-art set without choosing rewards. Replace held hard cuts with per-render-frame eased transitions between existing authored poses. No image assets, economy, persistence, consumption or reward selection changed. Cancellation/destruction still prevents the final callback; duplicate input remains single-flight. Effects still use Sound.play and honor mute.

## Validation

- Standard run_chest: zero failures; added checks for prepared poses/sounds and an actual intermediate transition value.
- The same regression capped at 15 FPS: zero failures, including cancellation, destruction, repeat input, exact rewards, both locales and navigation.
- Complete scripts/run_tests.sh exited 0.
- Independent read-only functional review found no transaction/mute blocker.
- Real desktop GL Compatibility/llvmpipe Chinese and English sequences recorded and intermediate frames inspected. Recording itself adds GPU readback/encoding overhead, so it is not used for performance claims. Delivered video preserves measured screenshot timestamps, rather than speeding frames up to a nominal FPS.

## Performance evidence and limitations

Separate no-screenshot runs, 1080x1920, the same reward seed, three fresh processes per variant. The original comparison uses the unchanged #294 chest implementation; final includes prewarming reward art. Times are milliseconds.

| Variant/run | Page construction | p50 frame | p95 frame | Max frame |
|---|---:|---:|---:|---:|
| original 1 | 406.477 | 30.00 | 50.068 | 100.00 |
| original 2 | 260.536 | 30.556 | 50.00 | 86.765 |
| original 3 | 464.00 | 31.667 | 41.170 | 78.178 |
| final 1 | 306.653 | 31.667 | 39.893 | 91.297 |
| final 2 | 429.819 | 30.556 | 43.296 | 86.802 |
| final 3 | 352.639 | 30.188 | 45.212 | 139.069 |

The pose stepping is addressed; these measurements do **not** prove every real-time hitch is eliminated. p95 improves in two runs, but the worst final outlier is worse. The renderer remains approximately 30 FPS on this software-GPU environment. No claim of 60 FPS or improvement on the user's physical device is made. Transparent crossfades are not a new skeletal animation; slight transient pose blending remains possible. Audio dispatch/mute were tested, physical speaker output was not heard. First-page preparation is synchronous and was measured explicitly to avoid concealing shifted load cost.
