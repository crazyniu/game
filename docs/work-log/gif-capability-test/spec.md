# Built-in GIF capability evaluation

Evaluate only Codex's built-in image generation on the existing plush bird references. This is offline evaluation, not production artwork or a Unity implementation.

Owned paths: `docs/work-log/gif-capability-test/` and `art/gif_tests/`. Preserve product code, Resources, original art, scenes and settings. Branch: `mac-gif-capability-test/work`.

Requested cases:
- Flight: Sunny enters from outside the top-left, lands bottom-centre on a flat surface with touchdown squash; 24 frames at 20 fps.
- Hat cheer: Sunny wearing the existing straw hat cheers in a 6-8 frame loop.
- Burst: sparkle and confetti, 12 frames.

Probe flight first. If the built-in generator cannot return animated GIFs, stop further generation and document what it actually returned; do not synthesize a GIF with code, convert a still, or use another service. Mark unattempted cases explicitly.

For returned artifacts, inspect actual file signatures, dimensions, frame count, timing, size, visible colour counts and alpha levels. Extract every actual temporal frame losslessly. Compare native-size and nearest-neighbour enlarged crops against existing PNG frames, composite over dark/light/green-leaf backgrounds, and show a labelled 50-percent reference overlay. A static output cannot establish temporal consistency or smoothness.

Keep the original returned bytes under `art/gif_tests/`, JPEG evidence sheets and a report under this work log. Retain actual GIF copies here only if generated and smaller than 2 MB. No production or Unity acceptance claims.
