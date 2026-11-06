# Plush UI art delivery

Delivered forty `Art/labels/label_title_level_<N>.png` sprites for `Level 1` through `Level 40`, standalone `Art/labels/label_word_best.png`, and leaf-free replacements of `Art/panel_wood.png` and `Art/card_wood.png`. All runtime paths are under `PerchSort/Assets/Resources/`. No metadata or runtime code was added or changed.

## Evidence

- [All forty titles](art/sheet_levels.jpg), 2080 × 1060: numerical order, left to right, top to bottom.
- [Best and panel comparisons](art/sheet_best_panels.jpg), 1600 × 1120: panel on the first row, card on the second, Best beside existing brown digits 28 on the third. Each panel row shows old/new on dark, then old/new on light.

These are offline art comparisons, not Unity screenshots or runtime approval.

## Production and measurements

Used the built-in image generation tool, with transparent output enabled, for every painted element. [The specification](spec.md#production-prompt-set) records the prompts and references. Twenty generations painted two complete level ribbons each. An initial four-title trial was discarded because its source resolution was lower. A separate edit of Daily Gift produced a common blank ribbon surface for final geometric registration. No fonts, font baking, procedural lettering, or assembled character fragments were used.

Every title is RGBA, exactly **2040 × 360**. The reference alpha rectangle `(18,39)-(2022,320)` governed sizing. Whole generated phrases, including their outlines, shadows and surrounding painted ribbon material, were cropped, scaled and placed over the common painted ribbon using feathered geometric cutouts. This corrects generation-to-generation letter-size drift while keeping the leaves and ribbon ends byte-identical across all forty outputs. The reference D's measured cream-fill cap height is 171 pixels; final L measurements are 170–172 pixels, a one-pixel raster tolerance. Full feathered/outlined letter extents are larger than this measurement. The original Daily Gift sprite is unchanged.

Best is **280 × 150**, with alpha bounds `(6,26)-(274,126)`: 100-pixel cap height and baseline 126. Its complete painted word was downscaled from a 1857 × 694 substantive source region. The spelling B-e-s-t and its alignment/material beside brown 2 and 8 were inspected at 300%.

Both panels were generated as constrained leaf-removal edits, registered to their original body geometry, and applied only around the former leaves with feathered geometric cutouts. All pixels outside those repair masks remain unchanged. No visible green leaf pixels remain. The original horizontal and vertical centerlines are byte-identical, preserving the original frame and cream transitions there.

| Asset | Canvas | Body registration rectangle | Existing slice borders: left, bottom, right, top |
| --- | --- | --- | --- |
| panel_wood | 800 × 540 | `(4,38)-(796,519)` | `(150,100,150,110)` |
| card_wood | 640 × 640 | `(16,49)-(625,636)` | `(120,115,120,120)` |

Measured centerline frame bands, using alpha ≥128 for the body and the longest cream-color run (R≥180, G≥140, B≥100), are unchanged: panel left/right 52/60 pixels, top/bottom 61/68; card left/right 60/65, top/bottom 61/62. These color-transition measurements include the existing soft shading and are not new import borders. Existing `.meta` files and importer code remain byte-identical.

Inspected **every numeral group, 1–40, at precisely 300% of final PNG pixels** and checked all complete title spellings on the family sheet. Every digit passed, including repeated digits and the open counters in 0, 4, 6, 8 and 9; no incorrect numeral required regeneration. Review crops, measurement JSON and geometry-only processing scripts remain in ignored `build/plush-ui-fixes/`.

## Verification

| Command/check | Result |
| --- | --- |
| `python3 ci/check_current_game_names.py --self-test` | 14 passed |
| `python3 ci/check_current_game_names.py` | 0 violations; run with all delivery paths staged |
| `python3 build/plush-ui-fixes/verify_art.py` | Passed: 43 PNGs, exact canvases, alpha/margins, forty titles, shared ends, Best metrics, unchanged panel centerlines and pixels outside repair masks, no remaining leaf pixels, no new metadata, both JPEG sheets |
| Protected-state SHA-256 comparison | 1,924 scene/metadata/settings/package/agent-instruction files unchanged; AGENTS.md and CLAUDE.md identical |
| `git diff --check` and `git diff --cached --check` | Passed |
| `flock /tmp/unity.lock bash scripts/run_tests.sh` | Failed before Unity launch: configured Mac executable missing |
| `flock /tmp/unity.lock bash scripts/screenshots.sh` | Failed before Unity launch: configured Mac executable missing |

No Unity Editor process held the local lock. This assigned workspace runs Linux; `/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity` does not exist. Both scripts exited 1. EditMode and PlayMode each executed **zero cases**; no result XML or skipped/passing counts exist. No runtime screenshots were produced. Mac import, compilation, full suites and runtime visual acceptance remain outstanding.

## Source-resolution boundary

The tool returned 2108–2111 × 745–746 pixels per two-title sheet despite requests for 4080 × 1440. The common blank ribbon source is 2172 × 724. Panel and card sources are 1526 × 1030 and 1254 × 1254 despite larger requests. These sources **do not meet the required native 2× oversampling** for their delivered canvases. Best does meet it. Final requested canvases are correct, and no synthetic enlargement is counted as native oversampling. Acceptance of this source-resolution exception remains with the assigning coordinator/user.

The assigned `plush-fix-art` branch contains only the 43 runtime PNGs, two evidence JPEGs, this report and the specification. No source references, game rules, saves, levels, scene, settings, packages or metadata were changed. No peer branch or main was merged or pushed.
