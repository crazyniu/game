# T48 map asset delivery

## Delivered

Eleven production PNGs are in `PerchSort/Assets/Resources/V7/Art/`:

- `map_bg_w1.png` through `map_bg_w4.png`: 1080 x 3000 RGBA; spring oak, mist bamboo, amber maple, snowy pine. Scenery only, clear tan S-path, no baked UI, nodes, labels, birds or gates.
- `node_wood.png`, `node_wood_current_glow.png`, `node_stone_locked.png`: 320 x 288 RGBA. Wooden nodes are empty for code-rendered numbers/stars; locked stone includes its padlock. The current-node file is a whole brighter-rim node replacement, not an additive-only overlay.
- `world_sign_w1.png` through `world_sign_w4.png`: 384 x 320 RGBA. Matching rope-hung honey-wood signs with oak/acorn, bamboo, maple-leaf and snowy-pine pictograms. No text.

All generations used the built-in image tool and approved `art/approved_ui/ui_map.png` as a reference. Background variants additionally referenced the first generated layout; signs referenced the first sign for a consistent family. `prompts.json` records exact prompts and tool-generated source paths. Opaque originals are preserved under `raw/w1.png` through `raw/w4.png`. No CLI/API-key fallback, Unity, commits or pushes were used in this delegated subtask.

## Stacking contract

The delivered backgrounds include a **160 px top alpha fade**: at local row y=0 alpha is 0; at y=159 alpha is 255; every later row, including the bottom edge, is opaque. RGB artwork is untouched apart from proportional resizing. A later world sits ABOVE the earlier one.

- Stack vertical origins 2840 px apart (3000 minus 160).
- Draw the upper image first, then ordinary alpha-composite the lower image at y=2840 relative to it. No custom blend shader is needed.
- In a four-world map ordered top-to-bottom W4, W3, W2, W1, origins are `[0, 2840, 5680, 8520]`; total height is 11520. Add each world's local node coordinate to its origin.
- **Topmost edge requirement:** clamp the visible topmost viewport to local y >= 160, or back its first 160 px with the matching preserved opaque source. Otherwise the top fade exposes the underlying canvas. This is required integration work, not a fully opaque standalone-top guarantee.
- A 3000 px hard-edge stride is not supported. The raw edge textures differ (mean RGB error 8.784–9.291); no claim of pixel-identical hard tiling is made.

`verify_stacking.py` composites the final delivered RGBA files with standard `Image.alpha_composite`. All three adjacent-world joins and all four same-world repeats were inspected in `qa/final_rgba_all_joins.jpg`. Every tested join region has alpha extrema `[255,255]`, with no transparent gap and no abrupt color edge visible in the static composite. Runtime scrolling remains untested.

## Actual path measurements

`world_paths.json` records 10 node centers for each actual generated background, top/bottom path anchors, sampled path centerline and edges, source hashes and stacking parameters. Heights are deliberate layout samples; x coordinates come from the rendered tan-path mask, not an invented sine curve. The tan path was segmented by RGB color, then its continuous interval selected using the previous row's center. Its midpoint gives each node x coordinate. The 40 node placements were visually checked against `qa/all_paths.jpg`. Approximate horizontal uncertainty is 4 px at foliage-softened edges. Suggested node display size is 180 x 150 px, independent of the sprite's 320 x 288 storage canvas. Level indices ascend from bottom to top; these are visual node placements, not gameplay authoring.

## Alpha and sprite QA

The built-in tool supplied real RGBA cut-outs. Isolated low-alpha matte noise outside the main connected silhouette was removed; a one-source-pixel envelope retained antialiasing. Source RGB was not recolored. Assets were trimmed, proportionally resized and padded. All seven sprites have fully transparent exterior pixels, fully opaque interior pixels, and partial-alpha edges. `sprites.json` records exact source/output bounds, sizes, hashes and processing details.

`qa/sprite_sheet_dark.jpg` and `qa/sprites_64px_color_gray.png` were visually inspected: no black or white rectangular backgrounds or visible gray halos remain; all four world pictograms and the padlock are distinguishable at 64 px in color and grayscale. The two wooden node states intentionally share their silhouette, with the current node distinguished by its brighter rim. `qa/all_sprites_over_approved.jpg` places each sprite over the actual approved map; individual composites are also saved. No runtime layout or motion acceptance is claimed.

Sprite pivots are canvas center `(0.5,0.5)` unless code specifies otherwise. Place code-rendered wooden-node numbers in the top-face region around canvas `(160,112)`, keeping stars below that area. World signs are complete sprites with baked short rope loops; they are not separated hanging-animation parts.

## Ownership

Only the assigned map production filenames and `art/approved_ui/runtime/map/**` were written. Gates, global layout, task report, status, commits and pushes remain with the parent agent.
