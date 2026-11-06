# Ember, Ink and Hoot front outfit frames

## Scope

Image-worker assignment on branch `outfit-front-c`: `outfit.ember.1..3`, `outfit.ink.1..3`, and `outfit.hoot.1..3`. This species-specific directory avoids taking ownership of the other image workers' shared reports.

For each outfit, add four 320×320 RGBA PNGs under the canonical `PerchSort/Assets/Resources/Birds` directory: `<species>_outfit_<n>_front_overlay.png`, `<species>_outfit_<n>_front.png`, and the corresponding `_front_blink_01.png` / `_front_blink_02.png`. Copy the species front PNG import settings with a fresh random GUID for every new PNG.

## Art contract

Use the built-in image generator to fit the existing side outfit accessory to the species' unchanged front pose. Work from a four-times upscaled front input. Extract accessories with difference inspection, geometric masks and alpha cleanup; regenerate dirty extractions. Preserve plush materials, upper-left light, true transparency and accessory identity. Never mirror a bird or draw substitute artwork in code.

Compose all three front expressions from the original species frame and one identical overlay. Eyewear must contain no baked eye pixels and must preserve visible blinking through semi-transparent lenses. Hoot's graduation overlay must cover the nightcap and pompom embedded in its base sprite without modifying that original sprite.

## Evidence and acceptance boundary

`../sheet_c.jpg` has nine rows, in Ember / Ink / Hoot order and outfit 1 / 2 / 3 order within each species. Columns are old side art, new front, blink 01, blink 02, on cream. Inspect accessory identity, contact, centering, alpha edges and eye visibility. Validate dimensions, metadata parity, GUID uniqueness and exact overlay composition. Linux artifact acceptance does not establish Unity import, runtime animation or device acceptance.
