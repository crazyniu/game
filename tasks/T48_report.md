# T48 runtime art delivery

Status: DONE for static art/layout scope. Branch `mac-art/t48`. Approved original screens remain unchanged. This is static sprite and layout delivery; Unity import, animation and runtime acceptance are NOT_RUN.

## Delivered milestones

- `0cb3741`: initial HUD surfaces.
- `79aef75`: complete HUD glyphs and gift badges.
- `09efd6d`: panels and Aviary surfaces.
- `924072c`: preserved map, effects, houses and measurement work at the lead's pause.
- `83d6043`: measured layout with integrated house anchors, ready for early integration.
- `9f60753`: final house review and aligned gift popup parts.
- `36e59c7`: gates and complete static sprite inventory.
- Final layout delivery commit: all 120 assets and every assembly contract (this report is included in that commit).

## Layout contract

[`art/v7/layout_v7.json`](../art/v7/layout_v7.json) uses 1080x1920 source pixels, top-left origin, and records 17 screens / 223 measured element boxes. Bounds are visible art, not hit targets; approximately 8 px uncertainty for ordinary edges and 16 px for foliage/glow. The approved K3 master contains one house, while K7 contains seven. Intermediate K1..6 multi-house arrangements are explicitly inferred centered adaptations, not measurements of approved screens. Bird positions are visual references, not level data.

`assets` records actual stored dimensions, alpha bounds, hashes, slice borders and extensionless Resources keys. Coin number frame is `[142,32,140,60]` on its 320x128 sprite. Green price-pill slice borders are `[54,42,54,42]` in left,bottom,right,top order. Geometry, dimensions, positive slice centers, map path intervals and HUD references are checked by `art/v7/runtime/measurements/assemble_layout.py`.

## HUD and gifts

44 sprites include six button/pill surfaces, count badge, 18 separate white glyphs, nine gift badges, coin, stars in two sizes, footprints, flag, padlock, check and chain. Scarecrow composes `icon_scarecrow` over `btn_purple`; Minty uses `gift_watering_can`. Labels and counts are code-rendered. Exact prompts and provenance: `runtime/generation_manifest.json` and `runtime/extracted/provenance.json`. `hud_extra_provenance.json` contains rejected experimental cutouts only; final next-arrow and chain came from generation_manifest.

Reviewed `runtime/qa/hud_64.png` for color/grayscale recognition and `runtime/qa/hud_overlays/` for approved-screen comparisons. Overlay positions are inspection placements, not final runtime layout.

## Houses

16 sprites: roofless body, eight colored roofs, round door leaf, open fallback, rope and four seasonal beams. Shared house canvas is 512x512; preserve transparent padding on import. Body peak `(256,96)`, roof suspension `(256,58)`, dark door center `(253,306)`, left hinge `(145,306)`; leaf Unity pivot `(0.283203125,0.40234375)`. Compose body/roof/door at zero offset. Open fallback contains the body and open door but no roof. Rope 64x256 top/bottom anchors `(32,4)`/`(32,252)`. Beam anchors and per-world heights are in `runtime/houses/delivery.json` and the layout.

Replacement blue/green roof colors and left-hinged open fallback reviewed in `runtime/houses/qa/assembled_sheet.jpg`. These aligned static parts enable animation; they do not demonstrate motion.

## Panels, shop and Aviary

16 sprites include wooden board/card, full green ribbon and its three pieces, leaf badge, name ribbon, stat pill, selection ring, Aviary rings/board, house chip and two ropes. `runtime/panels/delivery.json` owns slice borders, pivots and settings attachments; `runtime/panels/report.md` describes composition. Nine-slice stretching in Unity remains untested. Generations are reference-derived approximations, not pixel-identical cutouts.

## Map

Four 1080x3000 backgrounds, three nodes and four world signs; measured 40 node centers and actual path intervals in `runtime/map/world_paths.json` and layout. Backgrounds have a 160 px top alpha fade and must stack at 2840 px stride. Topmost viewport must start at local y>=160 or use matching opaque-source backing. All adjacent-world and repeated-world joins were checked with ordinary alpha composition; a 3000 px hard-edge stride is unsupported. Current glow node is a full replacement, not an additive overlay. `runtime/map/report.md` records QA and bounds.

Nine gate sprites are delivered: empty arch, two outer-hinged leaves, padlock, three aperture backdrops and locked/open fallbacks. Shared canvas 1024x1024; opening bounds xyxy `[240,250,768,854]`, outer hinges `(246,610)` and `(778,610)`. Preserve canvas padding. Gate-light draw rect `[192,215,640,640]` aligns its source to `(512,375)`. `runtime/gates/delivery.json` records pivots, layers and fallback transforms; `runtime/gates/report.md` records preparation. Closed/open assembly and three seasonal light composites were reviewed. Open fallback includes bamboo; other worlds use separate backdrop layers.

## Effects

20 sprites: rays, sparkle, four confetti, soft glow, ZZZ, four seasonal particles, three clouds, firefly, leaf wipe, gift trail, house glow and gate light. `runtime/effects/delivery.json` records dimensions/provenance and `runtime/effects/report.md` records compositing. Glow tails retain partial alpha; additive material/intensity require runtime verification. Gate-light source is at normalized top-left `(0.50,0.25)` and must be aligned with the final aperture. Leaf wipe must be scaled/positioned by code to occlude the portrait screen.

## Production and verification boundaries

All new artwork uses only the built-in image tool with approved screenshots as references. Raw generations and exact prompts are retained. Pillow operations are cutout/alpha preparation, trimming, sizing, alignment and QA composition; no new artwork is drawn. Static QA checks RGBA, transparent exterior, antialiasing, light/dark backgrounds and small-icon recognition. No game code, approved originals, scenes, Unity session or lock were changed. Runtime text, clipping, layering, hit areas, animation and rendered device acceptance belong to integration/QA.

## Daily gift

Four aligned 768x768 parts: `gift_nest`, `gift_box_base`, `gift_box_lid`, plus derived `gift_nest_front` for occlusion. Draw in that order. Closed lid pivot `(384,390)`, open translation `(0,-130)`; preserve canvas padding. Exact preparation and anchors in `runtime/gift/delivery.json`. Closed/open composites and matching approved-popup overlays reviewed; the box sits inside the nest without code-side pixel cropping.

## Final verification and integration handoff

Layout status is `delivered_static_layout`; house, gate, gift and settings-rope contracts are complete. All 120 sprite hashes match the actual exports. All 120 PNGs (119 requested plus the necessary nest foreground split) are present, RGBA, with transparent exterior pixels; exact group inventory in `runtime/delivery_inventory.json`. Review links in `runtime/qa/README.md`. See `docs/team/status/mac-art.md` for current handoff.
