# T48 panels and Aviary delivery

Delivered 16 PNG files in `PerchSort/Assets/Resources/V7/Art/`: panel_wood, card_wood, ribbon_green and its left/middle/right slices, leaf_badge, name_ribbon, stat_pill, tab_selected_ring, aviary_frame, aviary_board, silhouette_frame, house_count_chip, panel_rope_left, panel_rope_right.

All original approved screens are preserved. Every generated object used a matching approved screen as the built-in image-tool reference. No CLI/API fallback, Unity, runtime code or git operations were used by this worker. Exact accepted prompts are in `prompts.md`; input/output mappings are in `sources.json`; generated originals are in `raw/`.

The first card attempt retained an unwanted large centered leaf badge and was rejected. The accepted second attempt references the shop card and leaves its cream surface empty. Other objects use their first generated result. Ropes are new required suspension parts referencing the house rope material in the approved settings screen; the approved screen does not itself display panel suspension ropes.

## Dimensions and composition

`delivery.json` records dimensions, reference screen, alpha extrema, QA placement and 9-slice borders. Borders are `[left,bottom,right,top]` pixels. Coordinates are top-left pixels except normalized Unity pivots. Panel/card/aviary board center regions are empty cream surfaces; numbers, names and icons are not baked in. The house count chip intentionally includes its decorative house at the left.

The green banner has its full 640x200 sprite and contiguous 180/280/180 px left/middle/right slices. Stitch all three at unchanged height; stretch only the middle. Name-ribbon horizontal borders preserve the folded ends. Nine-slicing a textured surface changes its texture density; prefer sizes near the reference until checked in Unity.

Ropes use 64x320 canvases and normalized top-center pivots `(0.5,1)`. Top anchor `(32,4)`; bottom board attachment `(32,306)`. Suggested matching settings-board attachment positions in 1080x1920 screen coordinates: left `(250,860)`, right `(826,860)`. The bottom loop overlaps the board edge. The two independent generations differ slightly in twist spacing; they share material, length and anchors. Rope motion itself is not demonstrated.

## Verification

All 16 outputs are RGBA with alpha extrema `(0,255)`, real transparent exterior pixels, and preserved antialiased boundaries. Both Aviary ring centers and the selected-tab ring center have alpha zero. Contact sheet and dark/light alpha sheets were inspected visually; no baked forest/checkerboard background or visible white halo was found. Each source sprite has a matching-screen QA overlay; the three ribbon slices have an assembled `ribbon_slices_overlay.jpg`. `overlays_sheet.jpg` summarizes the matching-screen placement checks. These composites overlay empty surfaces over approved artwork for comparison; they are not reconstructed complete screens or Unity screenshots.

Static art QA is complete for this owned group. Unity import, sliced-image stretching in the target runtime, text rendering, animation and product acceptance remain for integration. The wooden board shapes and textures are regenerated approximations from the approved screens, not pixel-identical cutouts.
