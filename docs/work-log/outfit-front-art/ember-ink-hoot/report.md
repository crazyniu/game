# Ember, Ink and Hoot front outfit delivery

## Delivered

- Nine transparent accessory overlays and 27 front-expression composites: 36 new 320×320 RGBA PNGs in `PerchSort/Assets/Resources/Birds`.
- 36 new `.png.meta` files, copied byte-for-byte from each species' front metadata except for independently generated GUIDs.
- [Cream comparison sheet](../sheet_c.jpg), 1280×2880. Rows: Ember 1–3, Ink 1–3, Hoot 1–3. Columns: original side outfit, front, blink 01, blink 02.
- [Exact generation prompts](prompts.md), using the built-in image tool only.

Existing bird art, source code, scene, settings, packages, saved-data contracts and outfit identifiers are unchanged. No mirroring was used. No fonts, generated labels or procedural replacement artwork were used.

## Generation and inspection

The nine initial edits used 1280×1280 front inputs and matching side accessory references. Despite the requested canvas size, tool outputs were 1254×1254; they were normalized geometrically to the 1280×1280 work canvas. Differences against the upscaled fronts were inspected, then accessory color/region masks removed regenerated bird pixels. Only generated accessory colors were retained. PIL handled masks, scaling, placement, alpha cleanup and final composition.

Both spectacles were regenerated as accessory-only cutouts after the first front edits. Lens-core opacity was capped at 32/255 before resampling, preserving the painted rims and faint glass reflections without retaining eyes. All final frames use the original species expression plus the same final 320×320 overlay.

Hoot's graduation cap was regenerated to cover its built-in nightcap. The generated cap and tassel were fitted geometrically; the broader tassel covers the original white pompom. The original front and blink sprites remain untouched.

Visual review at native scale and enlarged views checked all nine items against the side references: red triangular bandana, flame headband, raised silver visor; star nightcap, silver moon spectacles, small purple wizard hat; graduation mortarboard/tassel, brown-gold spectacles, blue star cape/clasp. Accessories remain seated on the head/neck/shoulders. Open, half-closed and closed eyes remain readable. Cream and muted-dark backgrounds were used to inspect edges; no visible accessory halo or floating remnants remained in the reviewed composites.

## Verification

| Command / check | Observed result |
| --- | --- |
| `python3 build/outfit-front-art/verify.py` | Passed: 36 RGBA PNGs at 320×320, 36 metadata matches excluding GUID, 36 GUIDs unique across Assets, 27 exact alpha composites, four transparent lens-core regions. Transparent overlay pixels have zero RGB; all overlays have empty canvas edges. |
| `python3 ci/check_current_game_names.py --self-test` | 14 tests passed. |
| `python3 ci/check_current_game_names.py` | Passed with zero violations; rerun after staging new deliverables. |
| `git diff --check --cached` | Reports inherited trailing whitespace in copied Unity metadata. Retained intentionally to satisfy the exact metadata-copy contract; no metadata formatting was changed. |
| `git diff --check --cached -- . ':!*.meta'` | Passed for non-metadata deliverables. |
| `flock /tmp/unity.lock bash scripts/run_tests.sh` | Failed before compilation: `/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity: No such file or directory`. No EditMode or PlayMode tests ran. |
| `flock /tmp/unity.lock bash scripts/screenshots.sh` | Failed before capture with the same missing Mac Unity executable. No runtime screenshots were produced. |

The working tree was clean before this assignment. No Unity process was present before the serialized attempts; neither command changed existing scene/meta/settings/package files. Extraction scripts, difference masks, generation intermediates and inspection images are ignored working artifacts under `build/outfit-front-art`, not committed assets.

## Integration boundary

This delivery supplies artwork only. The coordinator must perform the applicable Mac import, full Unity suites and actual runtime/timed visual review after integration. The comparison sheet demonstrates authored expressions and accessory stability; it is not runtime-animation evidence. No historical test counts are claimed as new results.
