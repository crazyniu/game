# Board HUD art delivery

The original delivery record below describes the layout-size assets. The **High-resolution export** section supersedes its texture sizes, PPU, rope period, footprint design and gift glyph treatment.

Delivered all 18 requested sprites and 18 matching `.meta` files under `PerchSort/Assets/Resources/Art/`, on `mac-board-hud-art/work`. All files are new derivatives; retained artwork is unchanged.

## Assets

| PNG basename | Pixels | Content |
| --- | --- | --- |
| goal_house_single | 240x240 | Neutral honey-wood roof, green moss, empty dark arch, rope attachment |
| goal_house_door_open | 240x240 | Matching house with right-hinged door swung outward |
| goal_rope_single | 40x420 | Single twisted rope, two upper leaves, repeating lower strip |
| badge_ring_gold | 64x64 | Gold portrait rim, transparent center |
| badge_check | 32x32 | Green completion disc with cream check |
| gift_coin_wake | 48x48 | Sun song |
| gift_coin_reveal | 48x48 | Wind |
| gift_coin_encore | 48x48 | Feather with return arrow |
| gift_coin_grow | 48x48 | Watering can |
| gift_coin_scare | 48x48 | Angry eyes |
| gift_coin_lightstep | 48x48 | Feather |
| gift_coin_hatch | 48x48 | Flame |
| gift_coin_nightcall | 48x48 | Moon with swap arrows |
| gift_coin_moonbeam | 48x48 | Moon with three rays |
| move_pill_frame | 380x128 | Cream pill, honey-wood rim, divider; empty content areas |
| icon_footprint | 64x64 | Two brown three-toed bird tracks |
| dial_frame | 240x96 | Empty honey-wood pill |
| sleep_bubble | 320x140 | Empty cream bubble, warm rim, tail and soft shadow |

## Art and export

Inspected all three approved board images extracted from `origin/ui-design-review` at `1aa68eaa12daeeadd8b77fb421a7dab718fb8f93`. The manifest's neutral wood roof with moss takes precedence over the turquoise roof in the direction images. Wood grain, warm rims, cream surfaces and gold coins follow that shared kit. Existing source art was not overwritten.

Generated each sprite with the built-in image_gen tool using real transparency. The open-door state was edited from the generated empty house. The complete [prompt set](prompts.md) records every request. Original generation outputs remain in the image tool's output storage; the final project assets are self-contained.

Exported with a temporary macOS Core Graphics utility outside the repository. Resampling used premultiplied alpha; pixels with alpha below 13 were cleared. No background color removal or opaque checkerboard was used. Art remains straight RGBA in the PNGs. Existing art, scripts, scene and project settings are unchanged.

The rope's top 60 rows retain its leafy cap. Rows 60–79 transition into the repeating portion. In top-origin PNG coordinates, rows 80–419 repeat a 10-pixel rope period exactly; extend with a crop of this lower portion, not the leafy cap. No extra runtime sprite or alternate resource root was introduced.

## Import metadata

All 18 importers copy the settings of `btn_wood.png.meta`, including Single Sprite, Full Rect mesh, 100 pixels per unit, centered pivot, bilinear filtering, clamp wrapping, disabled mipmaps, alpha transparency, 2048 maximum size and its platform compression settings. Only GUIDs, sprite identity/name fields and the asset-local full-canvas rectangle differ. Each asset GUID is new and appears once in the workspace metadata. The original button metadata is unchanged.

## Inspection and evidence

All sheets show one exported sprite pixel per JPEG pixel when opened at 100 percent:

- [Light contact sheet](evidence/contact-light.jpg): all 18 sprites at final size on warm off-white.
- [Dark contact sheet](evidence/contact-dark.jpg): the same sprites on a dark blue-black background.
- [Board contact sheet](evidence/contact-board.jpg): the same sprites over an unscaled crop of approved `board_few_branches.png` (source rectangle x=190, y=420, width=700, height=1320). This is an offline contact sheet, not a proposed board layout or runtime capture.

Visual inspection at final size found readable and distinct gift symbols, empty usable frame interiors, a clear ring opening and no visible pale matte fringe on either solid background. The two house states retain matching overall framing. Phone-size readability here means the requested final pixel sizes viewed at 100 percent; no physical device was used.

Export inventory confirmed 18/18 exact dimensions and RGBA PNGs, transparent pixels in every asset, zero pixels with alpha 1–12, and alpha 0 at the ring center. The lower rope comparison found zero differing channel values at a 10-pixel offset. Metadata comparison found 18/18 importer setting matches and 18 unique asset GUIDs. These are bounded asset inspections, not automated game tests.

Commands used for production/inspection: `git switch -c mac-board-hud-art/work`; three `git show origin/ui-design-review:art/ui_design/<name>.png > /tmp/<name>.png` extractions; built-in image_gen calls; `swift /tmp/export-board-hud.swift`; `swift /tmp/inspect-board-hud.swift`; a one-off metadata comparison against `btn_wood.png.meta`; and `git diff --cached --check` before commit. Temporary export/inspection utilities are not repository code or deliverables.

## Acceptance boundary

Unity, game test suites, naming-guard tests and runtime screenshots were not run, as explicitly requested for this image-only task. No code was changed. Unity import, runtime loading/wiring, actual device appearance, motion and integrated visual acceptance remain unverified and belong to the coordinator's integration step. No release or main-branch action was performed.

## High-resolution export

Re-generated all 18 sprites with built-in image_gen and overwrote their canonical PNGs in place on `board-hud-hires`. Every selected master has an edge of at least 1024 pixels and exceeds its exported subject dimensions. No old PNG was enlarged. Ten assets (nine coins and the footprint) used two attempts each; the other eight used one. The open house derives from the newly generated empty-house master, and all coins derive from the newly generated wake coin for consistent gold treatment. The appended [high-resolution prompt set](prompts.md#high-resolution-generation) records the requests.

| PNG file | Texture size | Generated master size |
| --- | --- | --- |
| goal_house_single.png | 720x720 | 1254x1254 |
| goal_house_door_open.png | 720x720 | 1254x1254 |
| goal_rope_single.png | 120x1260 | 1224x1285 |
| badge_ring_gold.png | 192x192 | 1254x1254 |
| badge_check.png | 96x96 | 1254x1254 |
| move_pill_frame.png | 1140x384 | 2164x727 |
| icon_footprint.png | 192x192 | 1254x1254 |
| dial_frame.png | 720x288 | 1983x793 |
| sleep_bubble.png | 960x420 | 1896x830 |
| gift_coin_wake.png | 144x144 | 1254x1254 |
| gift_coin_reveal.png | 144x144 | 1254x1254 |
| gift_coin_encore.png | 144x144 | 1254x1254 |
| gift_coin_grow.png | 144x144 | 1254x1254 |
| gift_coin_scare.png | 144x144 | 1254x1254 |
| gift_coin_lightstep.png | 144x144 | 1254x1254 |
| gift_coin_hatch.png | 144x144 | 1254x1254 |
| gift_coin_nightcall.png | 144x144 | 1254x1254 |
| gift_coin_moonbeam.png | 144x144 | 1254x1254 |

Export used Pillow Lanczos resampling in premultiplied `RGBa`, followed by conversion to straight RGBA and clearing every pixel with alpha below 13. House and frame content was cropped from the new masters and downsampled into the previous visible bounds multiplied by three, preserving the established canvas padding. No resize enlarged either dimension. The texture limit remains 2048.

All 18 metadata files retain their GUID, sprite IDs, pivots, borders and other importer fields. The only changes are `spritePixelsToUnits: 100` to `300` and the sprite rectangle width/height multiplied by three. Texture dimensions divided by PPU therefore remain unchanged. No Unity layout or runtime code changed.

The rope uses generated art cropped to a 180-pixel-wide strip. Its generated rows 442–476 supply a 35-pixel source twist downsampled to a 30-pixel export tile. Export rows 0–179 retain the leafy top, 180–239 blend into the tile, and rows 240–1259 repeat every 30 pixels exactly (PNG top-origin coordinates). Extend only this lower portion.

The footprint is now one connected dark-brown track with a round pad, three forward toes, one short back toe and a thin lighter inner highlight. Gift coins retain gold discs with simplified white glyphs and dark-brown outlines. The second pass targeted a shared 6-pixel outline at 144 pixels; generated raster stroke width is approximate rather than a mathematically uniform vector stroke.

[High-resolution contact sheet](evidence/contact-hires.jpg) shows every sprite four times: cream and dark backgrounds, each at layout size and 1.5x display scale. The JPEG is 1750x3900; inspect at 100 percent for the labeled pixel scales. This is an offline asset sheet, not a Unity or phone screenshot. Visual inspection found no visible pale halos, preserved empty frame interiors, a transparent ring opening and distinct white gift symbols.

Bounded export inspection: 18/18 exact RGBA dimensions, zero alpha 1–12 pixels, transparent ring center, zero differing channels between lower-rope rows offset by 30, and 18/18 metadata comparisons identical after excluding only PPU and rectangle dimensions. Commands: `build/board-hud-hires/venv/bin/python build/board-hud-hires/export.py`, one-off PNG/metadata inventory inspection, and `git diff --cached --check`. Production utilities and inventories remain ignored under `build/`.

**Remaining design deviations:** after the two-attempt ceiling, the dark glyph bounding boxes in the 144-pixel exports remain offset from canvas center by approximately `(0.5, 3)` pixels for lightstep, `(-2.5, -0.5)` for nightcall, and `(-6, 0.5)` for moonbeam. These three do not fully meet the requested 2-pixel centering tolerance. The remaining six are within 2 pixels per axis. Reported offsets use opaque dark-glyph bounds, excluding the gold rim. No third generation was attempted, and exact common stroke-width compliance is not claimed. These are explicit coordinator review items; the export itself is delivered, not full design acceptance.

Unity, naming guards and all game tests were deliberately not run, per this bounded image-only assignment. Unity import and integrated visual acceptance remain with the coordinator. No PR, merge, main-branch change or release action was performed.
