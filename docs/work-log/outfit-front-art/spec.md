
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
