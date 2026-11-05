# Animated outfit art status

Blocked before generation. Completed outfits: none. Accepted PNG frames: 0/243. Completed actions and review GIFs: 0/54. Rejected generated frames: 0; one animation request was rejected before any output was produced. No runtime assets, metadata, code, scene or settings were changed.

## Generation blocker

The available PixelLab `animate_image_pixminimax` tool rejected the Sunny straw-hat cheer pilot with: `subscription expired (2026-11-03). renew at pixellab.ai/account or use credits via the v1/v2 REST API`. No job ID or output was returned. Subscription renewal was requested in the worker conversation. No alternate credit billing or API credential route was attempted.

This generator accepts images up to 256 x 256 and generates frames in multiples of four. It accepts first/last images and a motion description, but no complete pose-reference sequence. The pilot reduced the input uniformly to 256 x 256 and requested eight generated frames to evaluate before selecting the required six. It never ran. Exact reference poses, plush appearance, attachment, scale, baseline and alpha quality remain unverified. PixelLab's separate outfit-transfer tool accepts pose frames but caps batches at three for frames above 80 px and also caps dimensions at 256 px; it does not directly meet the six-frame single-action generation contract. The built-in image tool exposes still-image generation, not an animated GIF output control.

## Inventory and artifacts

All 27 reference outfit stills are present as 320 x 320 RGBA. All 81 unique bare cheer/blink reference frames are present. The inventory lists every one of the 243 missing output paths.

- `art/cosmetics/qa/animated-outfits/inventory.json`: exact reference and missing-frame inventory.
- `art/cosmetics/qa/animated-outfits/generation-status.json`: service rejection and capability limits.
- `art/cosmetics/qa/animated-outfits/sunny-reference.jpg`: reference-only inspection sheet, not generated animation evidence.
- `art/cosmetics/raw/animated-outfits/sunny_outfit_1_cheer_input.png`: resized pilot input, not a generated or accepted frame.
- `art/cosmetics/prompts/animated-outfits/sunny_outfit_1_cheer.txt`: attempted motion prompt.

There are no outfit comparison contact sheets or GIFs because no outfit action was generated.

## Exact completion list

Complete: none.

Missing both cheer and blink for every listed outfit:

| Tier | Missing outfits |
| --- | --- |
| 1 | sunny_outfit_1, sky_outfit_1, berry_outfit_1, mint_outfit_1, cocoa_outfit_1, snow_outfit_1, ember_outfit_1, ink_outfit_1, hoot_outfit_1 |
| 2 | sunny_outfit_2, sky_outfit_2, berry_outfit_2, mint_outfit_2, cocoa_outfit_2, snow_outfit_2, ember_outfit_2, ink_outfit_2, hoot_outfit_2 |
| 3 | sunny_outfit_3, sky_outfit_3, berry_outfit_3, mint_outfit_3, cocoa_outfit_3, snow_outfit_3, ember_outfit_3, ink_outfit_3, hoot_outfit_3 |

## Verification boundary

Completed checks: `python3 ci/check_current_game_names.py --self-test` (9 passed), `python3 ci/check_current_game_names.py` (178 tracked text files inspected, zero violations), and `git diff --cached --check` (passed). The asset inventory was inspected with Pillow; this is input availability checking, not animation acceptance.

No implementation or accepted artwork was produced. Unity compilation, EditMode, PlayMode, runtime screenshots and timed acceptance: NOT_RUN. No passing test counts are claimed. Two Unity Editor processes were observed and left untouched. The blocked-status commit is not a completed tier delivery.
