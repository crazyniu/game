# T49 world asset delivery

Status: IN PROGRESS. Branch `mac-art/world`, based on accepted T48 at `825ca1d`. World assets only; birds/eggs remain owned by mac-birds. No Unity or runtime code changes.

## W1 first delivery

Four 1080x1920 spring backgrounds: morning, noon, golden, rain. Built-in image-tool generations reference the approved `art/v7/ui7_level_k3_v2.png`; preserve pale haze, empty playfield and top-corner leaves. No birds, perches or UI. Exact prompts/raw sources are under `art/v7/world/backgrounds/`; `delivery.json` records dimensions, alpha and hashes. Only proportional center-fit resizing used. Backgrounds are opaque RGBA.

W1 transparent pieces delivered: branch620x90, twig400x80, crown600x400, trunk160x1920. Branch flat top y=17, usable x60..580; twig top y=18, usable x60..370. Crown attachment(138,319). Exact alpha/pivots in `art/v7/world/pieces/w1/metadata.json`. Crop/clip trunk above crown attachment in scene; mirror left pieces for the right side. W4, W2 and W3 follow after W1 is pushed. Lead addendum: six coloured 256x256 booster illustrations are also in progress, in V7/Art. Rendering-style matching is static reference review, not runtime acceptance.

## W4 snow delivery

Four backgrounds: morning, noon, bluehour, aurora. All reference approved K7 snowy atmosphere; aurora alone carries aurora light. Four transparent pieces exported at the same sizes as W1. Branch nominal flat top y30 (snow edge28..30); twig y22 (20..22). Exact pivots/anchors in `pieces/w4/metadata.json`.

`art/v7/world/qa/w1_comparison.png` and `w4_comparison.png` compare the approved screens with world-only compositions using measured branch-row heights. Bird/HUD reconstruction is outside this takeover. Flat branches use mean observed row heights; these QA placements are not new runtime level data. Snow edges have2px local variation. Static references match palette/material direction, not pixel-identical segmentation.

## W2 bamboo delivery

Four backgrounds: mist, noon, lanterns, drizzle. Bamboo foliage/culms adapt the approved pale spring rendering; lanterns remain distant in lower forest haze. Four transparent bamboo pieces delivered, with branch nominal y9 (node tops6..9), twig y19 (18..19), crown attachment(96,332). The small top variation comes from bamboo nodes, not runtime motion. Metadata and static noon composition in `pieces/w2/`.

## Booster addendum

Delivered six 256x256 transparent RGBA illustrations under `V7/Art`: `booster_hint_illus`, `booster_sapling_illus`, `booster_owl_eye_illus`, `booster_bell_illus`, `booster_scarecrow_illus`, `booster_rain_illus`. Seven built-in calls including one owl-eye correction. Approved shop/unlock screenshots referenced every time. The green plus and amber eye follow the lead's explicit coloured-symbol request. No button discs or text baked in. Root reviewed light/dark/64px colour/grayscale contact. Prompts, originals, bounds and QA: `art/v7/world/boosters/`. Runtime scale/layering remains untested.
