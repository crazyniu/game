# Front outfit art delivery: mint, cocoa and snow

## Delivered

Nine outfits, each with four 320×320 RGBA PNGs under `PerchSort/Assets/Resources/Birds`: accessory overlay, front, blink 01 and blink 02. All 36 accompanying `.meta` files copy the species front importer byte-for-byte except for fresh unique GUIDs. Existing source artwork, scene, settings, packages and runtime code are unchanged.

[Review sheet](sheet_b.jpg): columns are old side art, new front, blink 01 and blink 02. Rows are mint 1–3, cocoa 1–3, snow 1–3. Background is warm cream; every cell uses the actual 320×320 delivered sprite.

| Outfit | Item and visual review |
| --- | --- |
| outfit.mint.1 | Pink five-petal flower with golden center, attached to the viewer-right head leaf. |
| outfit.mint.2 | Red mushroom cap with cream spots and underside, fitted beneath leaves and over brows. |
| outfit.mint.3 | Brown layered acorn cap, fitted beneath leaves and over brows. |
| outfit.cocoa.1 | Red cuffed pom-pom cap centered between ears; short plush pile replaces the reference's knit texture as required by the current style contract. |
| outfit.cocoa.2 | Cream/brown striped bow tie and neck strap centered beneath beak. |
| outfit.cocoa.3 | Gold monocle around viewer-right eye with hanging chain; transparent lens and small translucent glint retain both blink states. |
| outfit.snow.1 | Powder-blue plush earmuffs with arched band, both pads touching the sides of the head. |
| outfit.snow.2 | Ice-blue snowflake clip attached below viewer-left crest. |
| outfit.snow.3 | Small pale-blue pointed tiara centered above forehead markings. |

All nine were inspected against side references for item identity, frontal fit and attachment. The identical overlay is used for each outfit's three front frames. No mirroring, new bird poses, font rendering or painted replacement eyes were used.

## Production

The built-in image-generation tool edited 1280×1280 (4×) upscaled canonical front references, with the existing side outfit supplied separately as item reference. The tool returned 1254×1254 images despite the requested canvas; these were normalized to the 1280×1280 working canvas before extraction and final downsampling.

Seven accessories were extracted using differences from the upscaled front, local accessory colour/region masks, connected-component cleanup and antialiased alpha. Cocoa's bow tie and monocle received additional hand-guided accessory-only regeneration to remove body-colour contamination and all bird pixels inside the lens. Generated lens alpha was reduced, with the clear interior limited to 25/255 after downsampling. PIL/NumPy/SciPy were used only for image geometry, masks, comparisons and composition.

Dark-background inspection caught detached mint eye-surround pixels and cocoa crown pixels in the initial masks; these were removed. Final overlays were checked on dark and cream backgrounds, with enlarged edge and lens inspection. Source birds themselves were never regenerated into the final composites: each delivered frame is exactly the original canonical frame plus its overlay.

Working images, difference maps, composition/validation helpers and diagnostic sheets remain in ignored `build/outfit-front-b/`. Built-in originals remain in the tool's generated-image directory. Only requested assets and review documentation are committed.

## Verification

- `python3 build/outfit-front-b/verify.py`: passed. 36 RGBA PNGs at 320×320; 36 importer matches; 36 unique GUIDs with no collision against other project metadata; 27 exact PIL alpha-composite matches; 18 distinct adjacent blink-frame pairs. Overlay edges do not touch the canvas border. Monocle clear-interior alpha maximum is 25/255, and both blink states visibly and numerically change inside the lens.
- Initial lens verification caught a Lanczos alpha overshoot to 27/255. The final downsampled interior is clamped to 25/255; all validation checks then passed.
- `python3 ci/check_current_game_names.py --self-test`: 14 passed, no failures or skips.
- `python3 ci/check_current_game_names.py`: 0 violations; rerun after staging to include every new tracked asset.
- `git diff --cached --check`: 384 trailing-whitespace warnings, all inherited verbatim from the source Unity `.meta` files. These were retained to honor the requested exact metadata copy with only GUID replacement. No other whitespace errors; documentation-only check passed.
- No tracked source asset, scene, settings or package changes before staging; additions only.

Mac Unity compilation, full EditMode/PlayMode suites, Unity screenshots, timed runtime animation and device acceptance were **not run**. This task explicitly assigns a Linux image worker; `Unity` is absent from PATH and the repository's configured macOS Unity executable is absent. Unity test counts for this delivery are therefore 0 run, not historical passing counts. The contact sheet verifies delivered artwork and frame composition, not runtime acceptance. Integration must still import and exercise these assets on the Mac.
