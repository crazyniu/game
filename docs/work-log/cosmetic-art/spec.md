# Cosmetic art: outfits and birdhouse styles (Codex image worker)

Context: `Meta/Shop.cs` already sells 27 outfits (9 wearable species x 3 tiers at 300, 500, 800 coins) and 4 birdhouse styles (600 coins); there is no artwork for any of them. This task produces the art only, as new derived files. The approved originals and the existing sprites are never edited. Code integration is a separate later task, so also write a small `art-manifest.json` listing every file, its size and pivot.

## Style
Match the approved plush style exactly (see `art/approved_ui/ui_shop_outfits.png` and `ui_aviary_detail.png`): soft fur, no outlines, same face, colours and proportions as the species' existing `<species>_idle.png` in `PerchSort/Assets/Resources/Birds/`. The accessory must look physically worn (hat sits on the head, scarf wraps the neck, glasses on the beak bridge), cute, readable at 64 px.

## Outfit images (27)
Each is the COMPLETE bird wearing the accessory, 320x320 RGBA, same canvas, feet baseline, scale and pivot (0.5, 0.5) as `<species>_idle.png`, so later code can swap it in place. Names `Resources/Birds/<species>_outfit_<tier>.png`, tier 1, 2 or 3 (two-digit not needed). Accessory list (exact):
| Species | Tier 1 (300) | Tier 2 (500) | Tier 3 (800) |
|---|---|---|---|
| sunny | straw sun hat with a white daisy | small blue bow tie | golden crown |
| sky | cream knitted scarf | brown aviator goggles pushed up on the head | tiny black top hat |
| berry | round gold-rimmed glasses | pink bow on the crest | sparkling silver tiara |
| mint | pink flower on the leaf crest | mushroom-cap hat | acorn-cap helmet |
| cocoa | tiny red beanie | striped bow tie | monocle with a gold chain |
| snow | pale-blue earmuffs | snowflake hair clip | ice-crystal crown |
| ember | red bandana | flame-pattern headband | small silver knight helmet |
| ink | purple star nightcap | round moon-shaped spectacles | tiny wizard hat with stars |
| hoot | graduation mortarboard cap | round reading glasses | dark blue night-sky cape with stars |
Tier 1 of the four species in the approved shop image (sunny hat, sky scarf, berry glasses, mint flower) must closely match that reference.

## Birdhouse styles (4)
Replace the house body and roof of the goal birdhouses. The existing house is `Resources/Art/house_body.png` plus a coloured roof (`house_roof_*.png`) plus door parts (`house_door_open.png`, `house_door_leaf.png`). For each style deliver `Resources/Art/house_style_<style>_body.png` with the roof baked in, SAME canvas size and SAME door-hole position, size and pivot as `house_body.png` plus its roof composite, so the bird peeking out and the door motion still line up. Styles: `mushroom` (red cap with white dots), `teapot` (blue-and-white porcelain with a lid roof), `lantern` (warm paper lantern in a wooden frame), `acorn` (acorn-cap roof on a brown shell). Provide `house_style_<style>_door_open.png` only if the door part differs.

## Quality gates (every image)
Real RGBA transparency (no white or black fringe on dark and light checks), consistent lighting from the top left, 64 px and grayscale recognisable, import settings copied from `sunny_idle.png.meta` (and from `house_body.png.meta` for houses). Reject and regenerate drifting faces, colours or proportions. Keep prompts, raw outputs and QA sheets under `art/cosmetics/` (raw, qa, prompts). Commit downscaled JPEG contact sheets per species (3 outfits plus the base) and per house style to `docs/work-log/cosmetic-art/evidence/`.

## Delivery order (push after each batch)
1. Tier 1 for the nine species plus the four house styles. 2. Tier 2. 3. Tier 3. Write `docs/work-log/cosmetic-art/report.md` with counts, rejected images and anything missing. English only; no version or task numbers in any file name.
