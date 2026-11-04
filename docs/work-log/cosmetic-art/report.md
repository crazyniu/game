# Cosmetic art delivery

## Delivered artwork

Delivered all 27 complete outfit sprites and four house-style body sprites. Missing required images: none. All birds are 320 x 320 RGBA; houses are 512 x 512 RGBA with baked roofs. Pivots are (0.5, 0.5). The tier digits in resource filenames are the expressly required shop tiers, not game versions or task numbers.

Generation used the built-in image tool with original idle sprites and approved shop references. Prompts and all 39 unmodified raw outputs are under `art/cosmetics/prompts` and `art/cosmetics/raw`; the generation log records each attempt. `art/cosmetics/art-manifest.json` lists every deliverable, size, pivot and raw source. `art/cosmetics/prepare.py` performs only delivery sizing/alignment and evidence-sheet preparation. It does not paint artwork.

Reviewed contact sheets in `evidence/` compare each bird with its base and each house with the original roof/body composite, on light and dark backgrounds and at actual 64 px in color and grayscale. No obvious matte fringe remains at delivery scale. Light comes from the upper left. Import metadata copies the requested templates with new GUIDs and renamed sprite identifiers. Approved originals and existing resources are unchanged.

Bird sizing uses foot spacing and baseline registration. House sizing registers the connected dark doorway to the original aperture, preserving the centered pivot. Measurements are recorded in `art/cosmetics/qa/inspection.json`. These are generated derivatives, not pixel-identical copies of the original feathers.

All nine species have four-column JPEG contact sheets: base plus three outfits. `art/cosmetics/qa/verification.json` records 31 RGBA/size/import-template checks and confirms no opaque canvas-edge clipping. The house aperture bounding boxes exactly match the original measured dark opening `(148, 204)..(353, 405)` after sizing. `art/cosmetics/qa/house-alignment.jpg` shows the shared door leaf over each house and an aperture guide; it is explicitly an offline alignment check.

## Rejected and unused generations

Seven generations were rejected and replaced; none is selected by the manifest:

- `sunny_straw_hat.png`: initial framing/body presentation drift.
- `sky_scarf.png`: closed beak changed to an open smile.
- `house_lantern.png`: ordinary-house gable silhouette, insufficient lantern identity.
- `mint_mushroom_hat.png` and `mint_acorn_helmet.png`: torso narrowed by about seven percent, detected during final cross-tier review.
- `mint_mushroom_hat_proportions.png` and `mint_acorn_helmet_proportions.png`: retries still followed the narrowed accessory reference. Fresh edits from an enlarged original produced the selected `body_match` images; torso sample widths now match within two pixels.

One additional output, `hoot_mortarboard_proportions.png`, is unused. A width concern was traced to removal of the original nightcap pompom; the delivered original mortarboard's torso matched the base. This retry was unnecessary and was not substituted. All rejected and unused raw outputs and prompts remain available for audit.

## Integration boundary

House door leaves retain the shared wooden door design, so no optional style-specific door-open image is supplied. Integration must reuse the shared leaf over the new body and omit the original `house_door_open` full-facade overlay and separate colored roof; those existing images contain the original house appearance. This worker does not change integration code.

## Ordered delivery and verification

- Entry outfits plus houses pushed at `a459399331088d998e5d26208c93114a72a62776`.
- Middle outfits pushed at `9581b3c8d39ab6301eefe3edb6bba89e464740b4`.
- Highest outfits and the cross-tier Mint correction form the final art batch. The accepted middle-tier Mint sprite is corrected in place; no alternate runtime copy is retained.

Commands completed: `PYTHONDONTWRITEBYTECODE=1 python3 ci/check_current_game_names.py --self-test` (8 passed), `PYTHONDONTWRITEBYTECODE=1 python3 ci/check_current_game_names.py` (1592 tracked text files, zero violations), and `build/cosmetic-tools/bin/python art/cosmetics/prepare.py` for delivery sizing/contact sheets. A focused image/import-template audit passed all 31 sprites. The plain Git whitespace check reported inherited trailing spaces in Unity metadata templates; the check with `core.whitespace=-blank-at-eol` passed, preserving the requested template formatting.

Scene, settings, package and metadata hashes were recorded before Unity in ignored `build/cosmetic-verification/before-unity.json`. `flock /tmp/unity.lock bash scripts/run_tests.sh` is queued behind other Mac workers, with console output directed to `build/cosmetic-verification/tests.log`. Full suite results and stable-state screenshots are pending at this checkpoint. No Editor was stopped. No runtime outfit/house integration or animation acceptance is claimed; integration is explicitly a separate assignment.
