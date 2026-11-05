# Map light patch report

## Status

**Visual acceptance is not met.** The rectangle remains at current level 10 in the after capture. The gate-light culling and its regression test are implemented, but they do not remove the reported patch.

## Findings

The patch is not produced by a gate light: at current level 10 all three map gate lights have alpha zero, and disabling every gate light produces a byte-identical PNG to the baseline (SHA-256 `0d87e2222acfe156f4e4696cfe677dd8d247cd6c26425a92be4234c4c7544ff2`). Disabling the current-node halo changes pixels lower down, not the reported patch.

Runtime isolation identifies the overlap between `World 1 background` (`Art/map_bg_w1`) and the `Boundary canopy` cropped from that same background. The first 160 source pixels of the world background fade from transparent to opaque, exposing the canopy's different section of path underneath. Disabling either scenery layer removes that compositing, but also removes a large, necessary section of the map. Hiding either layer is therefore not a safe patch. Correcting the seam without changing other map visuals requires an appropriate aligned transition or artwork; neither was substituted here.

[Before and after: levels 4, 10 and 11](evidence/map-comparison.jpg). [Sprite isolation around level 10](evidence/sprite-isolation.jpg): the two right and lower images intentionally disable individual scenery layers and show why those layers cannot simply be hidden. Raw captures and Unity logs are in ignored `build/map-light-before/`, `build/map-light-after/` and `build/map-light-isolation.log`. These are synthetic, nonpersistent profiles, with instant motion, rendered by the actual Unity camera at 1080 × 1920; the JPEGs are reduced evidence sheets.

## Changes

- `MapView` updates each gate light whenever the scroll offset changes, including programmatic movement, user scroll and gate replacement. The light is active only while its gate rectangle intersects the viewport.
- `GateView` toggles the map light without affecting popup gates, door pose or animation state.
- `MapGateLightTests` exercises visible and offscreen gates while scrolling between multiple worlds and back.
- The scenery composition and current-node halo remain unchanged. No artwork, progression, saved data or map layout was changed.

## Verification

| Command | Result |
| --- | --- |
| `python3 ci/check_current_game_names.py --self-test` | 9 passed, 0 failed |
| `python3 ci/check_current_game_names.py` | 179 tracked text files inspected after staging, 0 violations |
| Graphics PlayMode `MapGateLightTests` | 2/2 passed, including temporary capture case; capture code removed before full suite |
| `flock /tmp/unity.lock bash scripts/run_tests.sh` | EditMode 317/317, PlayMode 88/88; 0 failed or skipped |
| `flock /tmp/unity.lock bash scripts/screenshots.sh` | Passed; 34 stable 1080 × 1920 captures |

The level 10 patch remains visible in the final comparison, so the requested no-patch visual criterion fails despite the passing tests. No device, animation or audio acceptance is claimed. Unity generated unrelated asset metadata changes in the workspace; they were not committed and are preserved in ignored `build/unity-generated-state.patch`.

## Boundary experiments after coordinator review

The requested visual correction is still **not accepted**. [The comparison](evidence/boundary-experiments.jpg) shows three runtime experiments, none retained:

- Ending the canopy exactly where the world-background fade begins exposes the camera's white background through the 160-pixel transparent fade. This creates a conspicuous white band.
- Softening the canopy's bottom edge with a `RectMask2D` leaves the patch and exposes a lighter horizontal band.
- Removing the first 160 pixels of the world background and retaining the canopy removes the blended patch but replaces it with a hard horizontal scenery seam.

The underlying issue is that the two images do not depict the same path at the seam. Removing or reducing the lower image's alpha while the upper image is translucent also exposes the clear background. A transition that satisfies both the requested visual continuity and the no-art-change constraint has not been demonstrated. The retained source remains unchanged from the gate-light commit; no fade-boundary regression was added because the visual behavior still fails.

For a reproducible check, the 1080 × 1920 captures were resized to 540 × 960 using Lanczos. In the requested comparison-sheet region, corresponding to capture x=225..448 and y=225..284, luminance was calculated as `0.2126 R + 0.7152 G + 0.0722 B`, averaged over each four-pixel-wide column across the region height, and the maximum absolute difference between adjacent columns was divided by 255. Baseline: **6.352%**; clipped canopy: **6.329%**; softened canopy: **6.689%**; trimmed background: **6.738%**. All exceed the requested 6% threshold using this interpretation; the maximum is at x=337, where the chick also contributes. The measurement therefore does not isolate the patch from foreground detail.

The naming self-test passed 9/9, the naming guard reported 0 violations, and the serialized full suite passed 317/317 EditMode and 88/88 PlayMode with no skips or failures on the retained source. Temporary experiment and capture code was removed.
