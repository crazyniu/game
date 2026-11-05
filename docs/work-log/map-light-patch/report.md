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
