# T49 eggs and overlays delivery

Status: DONE for static asset scope; ready for current Codex coordinator review/integration. Branch `mac-art/world`. No merge to main performed. Stop after this delivery for the requested review/testing pause.

Asset delivery SHA: `34eddebeca3b75fa5e39019090c8b486ba99911f`. The follow-up handoff commit adds this SHA to the report/status.

## Files and alignment

Seven derived PNGs and seven new `.png.meta` files under `PerchSort/Assets/Resources/Birds/`. No species files, shared QA scripts, runtime code or scenes changed.

| File | Canvas | Alpha bounds xyxy | Baseline / pivot |
|---|---|---|---|
| `egg_idle.png` | 320x320 | [24, 55, 296, 300] | y=300; Unity(0.5,0.0625) |
| `egg_wobble_l.png` | 320x320 | [24, 58, 296, 300] | y=300; Unity(0.5,0.0625) |
| `egg_wobble_r.png` | 320x320 | [24, 58, 296, 300] | y=300; Unity(0.5,0.0625) |
| `egg_crack.png` | 320x320 | [24, 40, 296, 300] | y=300; Unity(0.5,0.0625) |
| `egg_shell.png` | 320x320 | [24, 117, 310, 300] | y=300; Unity(0.5,0.0625) |
| `mama_bow.png` | 128x128 | [8, 27, 120, 101] | center (64,64); Unity(0.5,0.5) |
| `chick_fluff.png` | 128x128 | [12, 26, 116, 102] | center (64,64); Unity(0.5,0.5) |

Bounds are left/top inclusive, right/bottom exclusive. The nest's last nonzero row is 299, with the perch/support coordinate at y=300 and 20px transparent foot inset. All five bottom bands y=280..319 are byte-identical. All five frame hashes are distinct. Idle body reference 180x225; projected wobble bounds 186x222 and 12px lateral shift; cracked body keeps 180px width with 15px additional opening height. The broken shell keeps the lower cup diameter approximately 180px, with its detached cap to the right. The identical 272x110 nest anchors every state; no whole-sprite resizing between frame swaps.

Importer metas specify Single Sprite, Full Rect, bilinear/clamp, no mipmaps, alpha transparency and uncompressed default texture storage. Egg pivots are at support coordinate (160,300); overlay pivots are centered. Import settings were written but not exercised in Unity.

## Overlay anchor contract

Both overlay sources are 128x128. Their attachment is source center (64,64). On a 320x320 bird frame, draw bow at 72x72 and fluff at 88x88, centered at the following anchors. Coordinates are top-left bird-frame pixels. Scale positions and display dimensions together with the bird; mirror the complete bird/overlay group for facing changes.

| Current bird idle | Bow center | Fluff center | Bow center on 200px body | Fluff center on 200px body |
|---|---|---|---|---|
| sunny | [104, 140] | [165, 244] | [65.0, 87.5] | [103.125, 152.5] |
| sky | [215, 123] | [163, 243] | [134.375, 76.875] | [101.875, 151.875] |
| berry | [100, 142] | [161, 241] | [62.5, 88.75] | [100.625, 150.625] |
| mint | [110, 114] | [166, 243] | [68.75, 71.25] | [103.75, 151.875] |

At nominal 200px adult display the bow canvas is 45px and fluff canvas 55px. The existing chick 0.7 parent scale should scale the whole group, giving 140px bird canvas and 38.5px fluff canvas. This preserves relative attachment positions. The bow avoids eyes and signature crests; fluff is a small lower-chest marker. Only these four idle silhouettes were measured and composited; other species/animated frames need integration review rather than assumed attachment invariance.

**Required integration:** read-only inspection of `BirdView.cs` at `3e82c64734f3e7291a332d1c0e90263954ce40cd` found hardcoded bow center (105,32)/size48 and fluff center (75,103)/size75 on the 200px body. Those old coordinates do not implement this contract. Current Codex/mac-ui must consume the recommended anchor/display-size data or equivalent reviewed placements. This asset task did not change that code, and the QA composites are not evidence that current runtime already places the overlays correctly.

## Sources and preparation

Eight built-in image-tool calls, all with approved `ui_level_k7.png` references: one shared nest, five egg parts, two overlays. Wobble/crack/shell generations also reference the derived idle egg; overlays reference the current Sunny snapshot. No CLI/API-key fallback, no sub-workers and no cross-server Dispatch. Raw outputs and exact prompts are in `generation.json` and `raw/`. Approved originals remain unchanged.

Representative Sunny/Skye/Berry/Minty PNGs were read-only copied from `origin/main` commit `3e82c64734f3e7291a332d1c0e90263954ce40cd` into `references/` for QA. Source paths and commit are recorded in `references/sources.json`; no runtime species checkout or modification occurred.

Generated nest/bow/fluff had unwanted low-alpha exterior haze. Alpha is remapped linearly from threshold 160 to 255; egg parts use threshold 32 for distant noise. Original RGB remains unchanged. Transparent cutouts are trimmed/resized/aligned. Shared nest front-rim pixels are reused over egg parts for occlusion; no production artwork is drawn. Exact crop/placement/alpha method, bounds, counts and SHA256 inventory: `manifest.json`. `prepare.py` is isolated asset preparation and QA, not runtime code.

## Static evidence and limits

- `qa/egg_contact_strip.png`: all five states over light/dark grounds.
- `qa/egg_game_size_64_grayscale.png`: 200px gameplay size plus 64px color/grayscale; question mark, wobble direction, cracks and empty cup are distinct.
- `qa/overlay_bird_composites.png`: recommended anchors on four current bird snapshots.
- `qa/overlay_game_size.png`: 200px mama and 140px chick examples.
- `qa/overlays_alpha_64_grayscale.png`: overlay light/dark transparency and 64px grayscale readability.

Checks passed: 7 exact PNG names, 7 metas, expected sizes, RGBA extrema (0,255), transparent margins, nonempty antialiasing, distinct egg hashes, shared support band and baseline 300. Contact/size/overlay sheets visually reviewed; no broad exterior glow or baked background remains. Generated fleck placement varies slightly between states; fixed nest/scale anchors prevent whole-sprite jumping. Wobble readability is subtler at 64px than at 200px. Static frames do not prove timed animation.

Unity import, animation timing, all-species/all-pose overlay attachment, device screenshots and runtime acceptance: NOT_RUN per explicit scope. No Unity process, lock, code tests or shared QA changes. Coordinator review/merge and testing remain pending. No further work started after this delivery.
