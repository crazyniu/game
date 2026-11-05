# Board HUD art delivery

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
