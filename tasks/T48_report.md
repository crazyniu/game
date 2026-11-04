# T48 runtime art delivery report

Status: IN PROGRESS. Branch `mac-art/t48` from `b5e601f`.

## Early HUD surfaces
`PerchSort/Assets/Resources/V7/Art/`: `coin_pill.png` (320x128), `btn_purple.png` and `btn_purple_pressed.png` (160x160), `btn_green.png` (220x220), `badge_red.png` (56x56), `btn_green_pill.png` (320x112).

These are RGBA empty surfaces for runtime labels/glyphs. Source art and exact generation prompts are retained in `art/v7/runtime/`. Built-in tool only, approved v7 screens supplied as references. Processing is trimming/resizing, not illustration synthesis. Further QA and remaining groups are pending.

Green-pill 9-slice borders: left/right 54, top/bottom 42 pixels; keep rounded ends fixed (layout contract is authoritative). Coin text frame will be included in final layout. Unity imports, runtime compositing, motion and tests NOT_RUN (mac-art does not open Unity).

## HUD completion
All 18 separate glyphs (96x96), nine gift badges (96x96), coin, two star sizes, footprints/flag/padlock/check/chain, green pressed state delivered. Atlas cut-outs retain original RGB; generated white glyphs use approved sheet references. Minty's gift is `gift_watering_can.png`; Scarecrow is a white crow with red cross for composition over `btn_purple.png`.

`art/v7/runtime/qa/hud_64.png` shows color/grayscale legibility; `qa/hud_overlays/` shows every HUD sprite over the approved level screen (inspection placements, not final runtime layouts). Solid glyph halo alpha removed during preparation. Detailed prompts/provenance in `generation_manifest.json` and `extracted/provenance.json`. Do not use `hud_extra_provenance.json` experimental cutouts; final next/chain were regenerated instead.

## Panels and Shop/Aviary
16 panel-related PNGs delivered, including ribbon's three slices, transparent frame rings, two separate hanging ropes and the shop selection ring. `art/v7/runtime/panels/delivery.json` records all dimensions, nine-slice borders (left,bottom,right,top), pivots and rope anchors. `panels/qa/` contains alpha checks and per-sprite overlays. Empty card corrected to remove an unwanted generated center badge. Runtime nine-slice stretching and rope animation remain NOT_RUN.

## End-of-day WIP wrap-up

Paused by lead before T48 completion. Map (11 PNGs), effects (20 PNGs), exported houses, measurement evidence and the draft layout are preserved in the wrap-up commit. Gate arch and daily gift raw results are in `art/v7/runtime/assemblies_wip/`, not the runtime folder. House replacement review, gate/door/backdrop generation, gift assembly, final layout and aggregate QA remain unfinished. See `docs/team/status/mac-art.md` for exact filenames and next actions. No Unity or lock created by this worker. Sub-workers stopped.

## Resumed 2026-11-04

Reviewed HUD 64-pixel color/grayscale sheet, panel contact sheet, all four measured map paths and effects light/dark sheet. Existing exports are preserved. The first delivery is the measured layout with completed house anchors; gate and gift assemblies remain explicitly pending until their separate milestone.
