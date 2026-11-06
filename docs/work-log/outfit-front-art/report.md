
---

# Front outfit art — Sunny, Sky, Berry

## Delivered

Nine accessory overlays and 27 front/blink composites: 36 new 320 × 320 RGBA PNGs and 36 metadata files in the canonical Birds resource directory. Each metadata file is byte-identical to its species' existing front metadata after removing the asset GUID line; all 36 replacement GUIDs are unique.

Existing art and metadata remain unchanged. No runtime code, scene, package, project setting, save data, or rules were changed.

Review [sheet_a.jpg](sheet_a.jpg). Rows and checks:

| Row | Outfit | Item and placement review |
| --- | --- | --- |
| 1 | outfit.sunny.1 | Honey daisy sunhat, brown band, white flower and green leaves; front brim seated around the existing curl, eyes clear. |
| 2 | outfit.sunny.2 | Small blue bow tie, centered immediately below the beak. |
| 3 | outfit.sunny.3 | Small gold crown, central red jewel and two blue jewels; seated around curl base. |
| 4 | outfit.sky.1 | Two cream scarf wraps and short fringed end at viewer-right; fitted below beak. Surface follows current velvet requirement rather than reference knit texture. |
| 5 | outfit.sky.2 | Brown aviator goggles with brass rims and buckles; resting on forehead as in reference, both eyes unobstructed. Strap has no stitch lines. |
| 6 | outfit.sky.3 | Tiny charcoal top hat with dark band, seated on blue crest. |
| 7 | outfit.berry.1 | Round gold glasses with central bridge and short temples; both original eyes and closed eyelids visible through the lenses. |
| 8 | outfit.berry.2 | Pink two-loop bow, seated at crest base above eyebrows. |
| 9 | outfit.berry.3 | Silver filigree tiara with central pale gem; centered across forehead with clear eyebrows and eyes. |

Columns are old side art, new front, blink 01, blink 02. Every row was visually inspected at native sprite size. Accessory-only layers were also inspected against a dark background; stray hat/curl and tiara fragments were removed. The same overlay is used without motion or transformation for all three frames of an outfit.

## Production method

The built-in image-generation tool received 1280 × 1280 (4×) upscaled front edit targets and corresponding outfit references in nine individual edits. The tool returned 1254 × 1254 images despite the requested canvas size; these were registered to the 1280 × 1280 work canvas before extraction and final downsampling.

Premultiplied RGB differences against the upscaled fronts identified changed regions. Generated bird changes made unconstrained differences unsuitable. Hand-guided geometric and color masks retained only accessories, with antialiased alpha. Accessory-only regeneration was additionally requested for the sunhat, goggles and glasses. The hat regeneration retained curl remnants and was rejected; the front edit was cleaned with a traced mask instead. The goggles and glasses regenerated layers were retained after halo cleanup, registration and lens-alpha adjustment. No original bird pixels were replaced in the delivered composites except where covered by an accessory layer.

PIL/NumPy processing was limited to resizing, difference measurement, selection masks, alpha cleanup, placement and alpha compositing. No fonts, text painting, sprite mirroring, or procedural accessory drawing were used.

Working inputs, raw edits, difference masks, rejected extraction, large overlays, composition script, dark review, and validation measurements are under ignored `build/outfit-front-art/`. They are not additional resource roots or committed historical copies.

## Validation

- `python3 build/outfit-front-art/compose.py`: generated the nine layers, 27 composites, copied metadata, and contact sheet.
- Inline PIL/NumPy asset verification: **36/36** PNGs have RGBA mode, 320 × 320 canvas, transparent borders, and matching metadata; **27/27** composites exactly equal canonical species frame plus the saved overlay. Pixels outside overlay alpha are byte-identical to canonical frames. **36/36** GUIDs are unique and occur once in Birds metadata.
- Lens alpha samples on the final 8-bit overlays: Berry glasses **10, 10 / 255**; Sky goggles **44, 35 / 255**. Berry's open, partial-blink and closed eyes were visually checked. No eye images are baked into the overlay.
- `python3 ci/check_current_game_names.py --self-test`: **14 passed**, no failures.
- `python3 ci/check_current_game_names.py`: **0 violations** before Unity attempts; repeated after staging to include delivered files.
- `git diff --check`: passed before staging. The staged check reported **624 trailing-whitespace findings**, all inherited verbatim from the required source `.png.meta` copies. These are preserved to honor the instruction to change only GUIDs. No other whitespace findings occurred; the documentation-only check passed.

## Unity boundary

Before the attempted runs, the scene/settings/packages diff was empty, and no Unity process or pre-existing Unity lock file was found. Both commands were serialized with the required lock:

```bash
flock /tmp/unity.lock bash scripts/run_tests.sh
flock /tmp/unity.lock bash scripts/screenshots.sh
```

Both exited **1** before Unity launch because `/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity` does not exist on this Linux worker. Attempt logs are `build/outfit-front-art/unity-tests-attempt.log` and `unity-screenshots-attempt.log`. **EditMode: 0 executed; PlayMode: 0 executed; runtime screenshots: 0 produced.** No suites were weakened or skipped within Unity; Unity never started.

Mac import/compilation, full EditMode and PlayMode suites, runtime integration, and timed animation acceptance remain for the integration owner. This contact sheet proves the delivered asset compositions and visual fit, not runtime motion or device acceptance.
