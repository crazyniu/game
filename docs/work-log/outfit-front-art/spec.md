# Front outfit art: mint, cocoa and snow

## Assigned scope

Create front-facing accessories for `outfit.mint.1..3`, `outfit.cocoa.1..3` and `outfit.snow.1..3`. Preserve all existing bird and outfit art. Deliver each outfit's transparent 320×320 overlay, front composite, two blink composites, and matching Unity importer metadata with fresh GUIDs. No runtime code changes.

## Art contract

Use the built-in image-generation tool on 1280×1280 upscaled front references, matching the existing side outfit's item. Extract accessory-only pixels using front-reference differences and carefully cleaned masks; regenerate dirty or incorrect artwork. Preserve the original front pose, light and velvet plush family. Never mirror. Any eyewear must use transparent lenses so the original blink remains visible. Composite the exact same overlay onto all three canonical front frames with PIL geometry only.

## Evidence and verification

`sheet_b.jpg` contains nine rows in species order mint, cocoa, snow and outfit order 1, 2, 3. Columns: existing side outfit, front, blink 01, blink 02, on cream. Check item identity, fit, attachment and visible blinking. Verify dimensions, alpha, identical overlay composition and importer settings/GUID uniqueness. Run both naming guards. This assigned Linux image worker cannot establish Mac Unity/runtime acceptance; report that boundary explicitly.

---

# Front outfit art

## Assigned scope

Linux image worker on `outfit-front-a`: `outfit.sunny.1` through `outfit.sunny.3`, `outfit.sky.1` through `outfit.sky.3`, and `outfit.berry.1` through `outfit.berry.3`.

For each outfit, add four 320 × 320 RGBA sprites to `PerchSort/Assets/Resources/Birds`:

- `<species>_outfit_<n>_front_overlay.png`: accessory only, transparent background.
- `<species>_outfit_<n>_front.png`: canonical front plus overlay.
- `<species>_outfit_<n>_front_blink_01.png`: canonical first blink plus the identical overlay.
- `<species>_outfit_<n>_front_blink_02.png`: canonical second blink plus the identical overlay.

Use the built-in image tool to edit a 4× upscaled canonical front with the accessory from the existing side outfit. Extract the difference and clean its alpha; regenerate difficult accessory cutouts when necessary. Use PIL/NumPy only for geometry, masks, alpha, registration, and compositing. Never mirror a bird or repaint its underlying expression. Preserve item identity, front perspective, seated attachment, warm lighting, and velvet plush treatment. Glasses and goggles retain translucent lenses.

Copy each species' front metadata verbatim except for a new random 32-hex asset GUID. Preserve all existing assets, metadata, code, scenes, settings, and persistence identifiers.

## Evidence and handoff

`sheet_a.jpg` has nine rows in species order Sunny, Sky, Berry, each with outfits 1, 2, 3. Columns are existing side art, new front, blink 01, blink 02. Background is warm cream. No font-rendered labels are added.

Check every outfit's item, placement, transparency, and three exact composites. Run the naming guard and its self-tests. Attempt the prescribed serialized Unity commands and report unavailable tooling honestly. Static asset evidence does not establish Unity runtime or timed-animation acceptance. Commit and push only the assigned branch.

---

