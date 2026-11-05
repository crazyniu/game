# T44 HUD runtime art delivery

2026-11-03. **PARTIAL / NOT COMPLETE: 81 of 85 required PNGs delivered, plus 2 coin companion PNGs.** The built-in image tool returned no images after six connection failures; the coordinator directed delivery of the deterministic derivatives with the missing artwork explicitly listed. These are static asset candidates and QA composites, not Unity screenshots or animation evidence.

## Authority and scope

- Task: [T44_hud_runtime_art.md](spec.md); current dispatch selects branch `t44-hud-art`, superseding the task file's older branch name.
- Approved visual reference: [T40 notes](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/mock/spec_notes.md), [K3 A](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/mock/level_k3_a.png), the other approved T40 screens, and [GAME_DESIGN_V5 §3.0 / §10](../../GAME_DESIGN.md). The README/router/evidence/UI ownership documents were read before production.
- Coordinator `msg_ae57f91a98df` explicitly authorized Pillow crop/resize, alpha masks, 9-slicing, roof/body separation, pressed shading and QA compositing; approved aligned 88×90 house layers, 120×32 shadow, 360×360 path, the cream tray with two knots, and Encore without `+1`.
- Coordinator `msg_3dc211a89a75` prohibited the separately billed CLI/API fallback, allowed only three additional built-in retries, and required a partial delivery if they failed. No API credentials were read or printed, and no fallback API was called.
- Only new files in `PerchSort/Assets/Resources/Art/`, `art/v6/runtime/`, and this report were written. No original art, T40 file, game code, runtime level data, scene, settings, or other worker's files were edited. No commit or push was made.

## Delivered files

Runtime destination: [V6/Art](../../../PerchSort/Assets/Resources/Art). All 83 PNGs use RGBA and have actual zero-alpha pixels. No English BEST/PAR labels, level numbers, stock numbers, or reward text are baked into them. Plus, cross, check, and question-mark mechanic symbols are pictograms.

The [asset manifest](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/runtime/asset_manifest.json) records each file's dimensions, alpha bounds, pivot, provenance, border and SHA-256. The [required inventory](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/runtime/required_assets.json) lists all 85 required filenames and exact sizes, including the four missing PNGs. The [delivery file list](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/runtime/qa/delivered_files.txt) lists every delivered runtime PNG.

| Group | Delivered filenames, excluding `.png` | Canvas size |
| --- | --- | --- |
| Cream square | `ui_btn_square`, `ui_btn_square_pressed` | 128×128 |
| Primary green | `ui_btn_primary_green`, `ui_btn_primary_green_pressed` | 200×200 |
| Coin chip | `ui_chip_coin` | 190×72 |
| Leaf level badge | `ui_badge_level` | 96×96 |
| Stock/plus badges | `ui_badge_count`, `ui_badge_plus` | 40×40 |
| Tray / rope | `ui_tray_plank`, `ui_rope_short` | 920×150 / 16×80 |
| Panel / ribbon / card / pill / close | `ui_panel_cream`, `ui_ribbon_green`, `ui_card_cream`, `ui_pill_small`, `ui_close_x` | 820×760 / 860×150 / 760×620 / 220×64 / 72×72 |
| Tools and navigation | `ico_undo`, `ico_hint`, `ico_add_branch`, `ico_peek_eggs`, `ico_wake_bell`, `ico_scarecrow`, `ico_rain_sprout`, `ico_settings`, `ico_shop`, `ico_aviary`, `ico_home`, `ico_replay`, `ico_play`, `ico_next`, `ico_sound_on`, `ico_check`, `ico_lock` | 128×128; each also has a 64×64 `_64` PNG |
| Coin companions, additional | `ico_coin`, `ico_coin_64` | 128×128 / 64×64 |
| Goal house layers | `goal_house_roof`, `goal_house_body` | **88×90 each**, aligned |
| Progress / check | `goal_dot_empty`, `goal_dot_full`, `goal_check` | 14×14 / 14×14 / 28×28 |
| Nine complete gift badges | `gift_wake`, `gift_breeze`, `gift_stare`, `gift_gardener`, `gift_light_step`, `gift_warm`, `gift_night`, `gift_moonbeam`, `gift_encore` | 44×44 |
| Stars | `star_full`, `star_empty`, `star_small_full`, `star_small_empty` | 96×96 / 96×96 / 32×32 / 32×32 |
| Map nodes | `node_cleared`, `node_current`, `node_open`, `node_locked` | 120×120 |
| Shadow / path | `node_bounce_shadow`, `map_path_segment` | **120×32 / 360×360** |
| Gates | `gate_locked`, `gate_open` | 720×300 |
| Mechanics | `mechanic_egg`, `mechanic_sleep`, `mechanic_crow`, `mechanic_swivel`, `mechanic_growing`, `mechanic_night` | 56×56 |

The additional coin pair allows the asset-only K3 assembly to include the approved coin pictogram without reaching back into T40 source art during composition.

## Missing files and image generation

| Missing file | Required canvas | Reason |
| --- | --- | --- |
| `ico_footsteps.png` | 28×28 | Built-in generation failed twice |
| `ico_flag.png` | 28×28 | Built-in generation failed twice |
| `ico_sound_off.png` | 128×128 | Built-in edit failed twice |
| `ico_sound_off_64.png` | 64×64 | Depends on the missing 128 px image |

All six calls returned `image generation failed: connection failed: error sending request`. The first flag/footsteps requests were independent; muted sound was a separate edit using the viewed T40 sound source. The three authorized retries were separated by production work. No generated artifact was returned, and there is no code-drawn or renamed stand-in for any missing icon. [imagegen_attempts.json](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/runtime/imagegen_attempts.json) retains the exact prompts, reference, attempt counts and error.

The coordinator must retry those three illustrations through the built-in tool later; the fourth missing PNG is then a deterministic downsample. BEST/PAR text was not substituted.

## Import and composition contract

Borders below use **Unity left, bottom, right, top** ordering. Values are source pixels; preserve their relationship to the UI's sprite pixels-per-unit setting. Border declaration is recorded here and in the manifest; Unity `.meta` imports have not been authored or verified.

| Sprite | Border L/B/R/T |
| --- | --- |
| `ui_btn_square`, pressed | 36 / 36 / 36 / 36 |
| `ui_btn_primary_green`, pressed | 48 / 48 / 48 / 48 |
| `ui_chip_coin` | 28 / 28 / 28 / 28 |
| `ui_tray_plank` | 120 / 34 / 120 / 34 |
| `ui_panel_cream`, `ui_card_cream` | 80 / 80 / 80 / 80 |
| `ui_pill_small` | 24 / 24 / 24 / 24 |

All other sprites use Simple/aspect-preserving placement, not nine-slicing. Pressed bases preserve their normal alpha/silhouette and apply RGB×0.90. Disabled opacity remains a runtime operation. `ui_close_x` is a complete cream close button; do not add a second base. `ui_badge_plus` follows the approved cream circular badge with a green plus. Gift PNGs already include their cream circular badge.

The default normalized pivot is `(0.5,0.5)`. Rope uses top-center `(0.5,1)` and gates bottom-center `(0.5,0)`. Bounds in the manifest distinguish the full PNG frame from visible artwork.

The house layers share an **88×90** canvas and center pivot. Tint only the neutral roof fill; the body's layer includes the original warm-brown roof contour so tint does not recolor that contour. Order: portrait, roof, body; the face needs an arch mask. The clear doorway's measured top-left bounds are **(23,43)–(65,82)**, with center **(44,62.5)**; in bottom-left coordinates its center is **(44,27.5)**, normalized **(0.5,0.30556)**. Alpha at `(44,62)` is zero. No face is baked in. The former painted dark doorway is now a real hole, so the portrait/background layer affects its appearance.

Tray knots are cropped from the original V5 house hanger and placed at local **(100,18)** and **(820,18)**. They remain inside the horizontal 120 px fixed regions when the tray stretches. The cream surface itself comes from T40, as explicitly approved.

`ui_rope_short` and `map_path_segment` repeat vertically. Both have byte-identical first and last RGBA rows. Rope uses a narrow three-row seam blend; the path uses a sand-only crop from the approved generated map, a 32-row seam blend and alpha-feathered side edges. The path is a straight texture strip, not a complete S-curve or map background.

## Static QA and comparison

[validation.json](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/runtime/qa/validation.json) reports **83 delivered PNGs**, **85 required**, **2 additional**, **4 missing**, and no size/mode/alpha/border/seam errors among delivered files. `all_required_delivered` is explicitly false. Read-only source hashes are recorded and checked; the earlier T40 hash snapshot is also unchanged.

- [Cream contact sheet](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/runtime/qa/contact_cream.png) and [sky contact sheet](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/runtime/qa/contact_sky.png): all delivered sprites and sizes.
- [Native-size grayscale sheet](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/runtime/qa/icons_grayscale_native.png): tools at 64 px, mechanics at 56 px, gifts at 44 px.
- [Nine-slice stretch sheet](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/runtime/qa/nine_slice_stretch.png): actual recorded borders exercised at alternate widths/heights; contours and knots remain intact.
- [Vertical repeats](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/runtime/qa/vertical_repeat.png): three path tiles and twelve rope tiles.
- [Required K3 assembly](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/runtime/qa/level_k3_assembled.png): **1080×1920**, composed only from the delivered V6 sprites over `V5/Art/bg_forest_day.png` with the existing birds, branches and root bushes. Dynamic numbers are intentionally blank; no mockup screenshot is baked into the assembly.
- [Comparison preview](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/runtime/qa/level_k3_comparison_preview.png), [full comparison](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/runtime/qa/level_k3_comparison.png), and [same-background assembly](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/runtime/qa/level_k3_same_background.png): T40 used `bg_w1_morning.png`, whereas T44 explicitly requested `bg_forest_day.png`; the third panel isolates that background difference.

Visual inspection finds the 17 available tool/navigation silhouettes distinct at 64 px in color and grayscale. This is a designer inspection, not a user recognition study. Gift detail is denser at 44 px: Encore's footprint/feather reads first; its crossed empty counter is secondary detail. The 56 px sleeping bird is less explicit than the mock's 80 px bird plus `zzz`; no letters were baked into the new icon. The branch-plus growing icon shares the Add Branch silhouette and relies on its level-card context.

K3 preserves the T40 branch/bird frames, 60 px tip gap, 260 px goal pitch, 44 px gift badges, and bottom control centers. Differences include the required background, new knots, blank dynamic numbers, neutral-tinted roof layering, real transparent door holes, and small edge resampling differences. This is not claimed as pixel-identical to T40.

## Review limitations and next steps

1. The four missing files prevent completion of T44. Generate and inspect them before accepting the full set.
2. The ribbon and gates preserve the approved artwork's aspect ratio inside their requested canvases. Ribbon alpha>16 bounds are approximately **(204,2)–(655,148)** in 860×150. Locked gate bounds are **(205,2)–(514,298)**; open gate bounds **(207,2)–(512,298)** in 720×300. Thus the gates' visible art is about 309 / 305 px wide, not 720 px; integration must assess path coverage using those bounds. A genuinely wide gate requires a separately successful generation rather than stretching the approved gate geometry.
3. Mechanic egg/sleep/crow/swivel/night reuse existing V5 art; growing reuses T40 branch-plus art. They are disclosed deterministic derivatives, not newly generated pictograms or new gameplay authority. The sand strip is likewise a crop of T40 art. No claim is made that nonexistent dedicated mockup counterparts were matched.
4. Unity import settings, sprite borders in the Editor, clipping, portrait occlusion, safe areas, input targets, device rendering and all animated behavior are **NOT_RUN**. This machine has no Unity Editor. No Mac test run or screenshot capture was attempted for this static-only worker task.
5. Current/open nodes intentionally use the same cream base as T40; code must add the current bird/bounce and compose locked-node lock, numbers and cleared stars. Static shadow and pressed files prove no motion or interaction.

Reproduction: `python3 art/v6/runtime/build_runtime_art.py`. The script uses Pillow and NumPy already available in this environment and writes only the two owned art directories. It reproduces deterministic assets/QA; it does not generate missing art or call any API. The [prompt/error record](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v6/runtime/imagegen_attempts.json) separately captures the built-in tool attempts.
