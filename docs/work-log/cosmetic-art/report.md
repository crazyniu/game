# Cosmetic art delivery

## Entry delivery

Delivered nine complete entry-tier outfit sprites and four house-style body sprites. All birds are 320 x 320 RGBA; houses are 512 x 512 RGBA with baked roofs. Pivots are (0.5, 0.5). The tier digits in resource filenames are the expressly required shop tiers, not game versions or task numbers.

Generation used the built-in image tool with original idle sprites and approved shop references. Prompts and unmodified raw outputs are under `art/cosmetics/prompts` and `art/cosmetics/raw`; the generation log records each attempt. `art/cosmetics/art-manifest.json` lists deliverables, geometry and source images. `art/cosmetics/prepare.py` performs only delivery sizing/alignment and evidence-sheet preparation. It does not paint artwork.

Reviewed contact sheets in `evidence/` compare each bird with its base and each house with the original roof/body composite, on light and dark backgrounds and at actual 64 px in color and grayscale. No obvious matte fringe remains at delivery scale. Light comes from the upper left. Import metadata copies the requested templates with new GUIDs and renamed sprite identifiers. Approved originals and existing resources are unchanged.

Bird sizing uses foot spacing and baseline registration. House sizing registers the connected dark doorway to the original aperture, preserving the centered pivot. Measurements are recorded in `art/cosmetics/qa/inspection.json`. These are generated derivatives, not pixel-identical copies of the original feathers.

Rejected and regenerated: Sunny straw-hat initial framing/body presentation; Sky scarf initial open-beak face drift; lantern initial ordinary-house silhouette. Rejected raw outputs are retained and excluded from Resources. See `art/cosmetics/generation-log.json` for exact names and reasons.

House door leaves retain the shared wooden door design, so no optional style-specific door-open image is supplied. Integration must reuse the shared leaf over the new body and omit the original `house_door_open` full-facade overlay and separate colored roof; those existing images contain the original house appearance. This worker does not change integration code.

## Middle delivery

Added all nine middle-tier outfits in the specified accessory order. All passed the same light/dark, 64 px and grayscale review, with no additional rejected generations. Current delivery count is eighteen outfits plus four houses. The house aperture bounding boxes exactly match the original measured dark opening `(148, 204)..(353, 405)` after sizing. `art/cosmetics/qa/house-alignment.jpg` shows the shared door leaf over each house and an aperture guide; it is explicitly an offline alignment check.

Remaining at this checkpoint: nine highest-tier outfits, final four-column species sheets, serialized Unity verification and runtime integration acceptance. Naming guard self-test passed (8 tests), and tracked-tree checks passed with zero violations. Unity is currently occupied by other serialized workers; no Editor was stopped. No runtime outfit/house integration or animation acceptance is claimed.
