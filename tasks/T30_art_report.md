# T30-A v5 level-1 art delivery report

Date: 2026-11-03. Scope: [T30 art task](T30_v5_art.md), style and dimensions from [GAME_DESIGN_V5](../docs/GAME_DESIGN_V5.md), sections 3–4. Result: **33/33 required runtime PNGs delivered; no missing files.**

## Delivery and provenance

- Final sprites: [`PerchSort/Assets/Resources/V5/Art/`](../PerchSort/Assets/Resources/V5/Art/), exact filenames listed below. No right-branch sprite; the left branch is intended to be mirrored.
- 33 unmodified image-tool outputs: [`art/v5/raw/`](../art/v5/raw/).
- Exact prompts and identity references: [`prompts.md`](../art/v5/prompts.md), [`manifest.json`](../art/v5/manifest.json), and [`generation_log.json`](../art/v5/generation_log.json). Every asset was produced through a separate built-in `image_gen.imagegen` call; each bird variant uses its own species idle as its edit reference. No competitor artwork was supplied.
- Reproducible post-processing: [`process_assets.py`](../art/v5/tools/process_assets.py). Low-alpha generation haze is suppressed, hidden RGB is zeroed, canvases are resized/padded, and UI bases are normalized to their exact aspect ratios. The generated sparkle retains its soft alpha falloff. All runtime art derives from generated raster images; no placeholder shapes are substituted.
- Review images: [all-assets light/dark contact sheet](../art/v5/contact_sheet.png), [64 px grayscale bird strip](../art/v5/bird_idles_grayscale_64.png), [native-size bird frames](../art/v5/qa/bird_frames.png), [48 px icons](../art/v5/qa/icons_48.png), [level-1 asset composite](../art/v5/qa/level1_asset_composite.png), and [hand anchor](../art/v5/qa/hand_fingertip.png).
- [Offline frame preview GIF](../art/v5/qa/bird_frame_preview.gif) contains an actual blink swap and alternating up/down frames (120 ms blink, 70 ms per wing frame). This is an asset preview, not proof of Unity animation, flight paths, input locks, or runtime timing.

## File validation

Run `python3 art/v5/tools/process_assets.py`, then `python3 art/v5/tools/verify_assets.py` from the repository root. Both completed successfully on 251. Verification covers all 33 filenames and raw originals, exact dimensions, RGBA channels, transparent/opaque alpha ranges, zero hidden RGB, standing baselines, distinct flight images, icon holes, branch contact samples, and GIF frame count. [Machine-readable metrics](../art/v5/qa/asset_metrics.json) include visible/solid bounds and alpha pixel counts; [verification and SHA-256 hashes](../art/v5/qa/verification.json) record the checked final files.

| Runtime PNG | Pixels | Mode | Alpha min / max |
| --- | --- | --- | --- |
| `bg_forest_day.png` | 1080 × 1920 | RGBA | 255 / 255 |
| `branch_left.png` | 640 × 120 | RGBA | 0 / 255 |
| `bush_edge.png` | 360 × 300 | RGBA | 0 / 255 |
| `birdhouse.png` | 300 × 340 | RGBA | 0 / 255 |
| `bird_sunny_idle.png` | 256 × 256 | RGBA | 0 / 255 |
| `bird_sky_idle.png` | 256 × 256 | RGBA | 0 / 255 |
| `bird_sunny_blink.png` | 256 × 256 | RGBA | 0 / 255 |
| `bird_sunny_fly_up.png` | 256 × 256 | RGBA | 0 / 255 |
| `bird_sunny_fly_down.png` | 256 × 256 | RGBA | 0 / 255 |
| `bird_sky_blink.png` | 256 × 256 | RGBA | 0 / 255 |
| `bird_sky_fly_up.png` | 256 × 256 | RGBA | 0 / 255 |
| `bird_sky_fly_down.png` | 256 × 256 | RGBA | 0 / 255 |
| `btn_square_green.png` | 256 × 256 | RGBA | 0 / 255 |
| `btn_square_wood.png` | 256 × 256 | RGBA | 0 / 255 |
| `btn_round_purple.png` | 256 × 256 | RGBA | 0 / 255 |
| `icon_undo.png` | 192 × 192 | RGBA | 0 / 255 |
| `icon_hint.png` | 192 × 192 | RGBA | 0 / 255 |
| `icon_add_branch.png` | 192 × 192 | RGBA | 0 / 255 |
| `icon_settings.png` | 192 × 192 | RGBA | 0 / 255 |
| `icon_restart.png` | 192 × 192 | RGBA | 0 / 255 |
| `icon_sound_on.png` | 192 × 192 | RGBA | 0 / 255 |
| `icon_sound_off.png` | 192 × 192 | RGBA | 0 / 255 |
| `icon_close.png` | 192 × 192 | RGBA | 0 / 255 |
| `icon_next.png` | 192 × 192 | RGBA | 0 / 255 |
| `icon_home.png` | 192 × 192 | RGBA | 0 / 255 |
| `coin.png` | 128 × 128 | RGBA | 0 / 255 |
| `pill_cream.png` | 260 × 90 | RGBA | 0 / 255 |
| `panel_win.png` | 860 × 900 | RGBA | 0 / 255 |
| `ribbon.png` | 900 × 200 | RGBA | 0 / 255 |
| `star_full.png` | 192 × 192 | RGBA | 0 / 255 |
| `star_empty.png` | 192 × 192 | RGBA | 0 / 255 |
| `sparkle.png` | 128 × 128 | RGBA | 0 / 255 |
| `hand_pointer.png` | 200 × 200 | RGBA | 0 / 255 |

## Bird registration and foot lines

Coordinates below use the top-left of the 256 × 256 PNG, with integer pixel indices. All eight raw bird canvases were 1254 × 1254. Every frame uses one common 238 × 238 resize, positioned at (9,16) for Sunny or (9,15) for Sky; flight frames are never independently fitted to their wider wing bounds. The shared standing contact anchor is **(128,244)**, equivalent to normalized bottom-left pivot **(0.5,0.046875)**. This is an integration coordinate; this worker did not write Unity metadata.

| Frame | Visible foot-bottom y | Foot state |
| --- | --- | --- |
| `bird_sunny_idle.png` | 244 | Standing |
| `bird_sunny_blink.png` | 244 | Standing |
| `bird_sunny_fly_up.png` | approximately 212 | Tucked against belly |
| `bird_sunny_fly_down.png` | approximately 214 | Tucked against belly |
| `bird_sky_idle.png` | 244 | Standing |
| `bird_sky_blink.png` | 244 | Standing |
| `bird_sky_fly_up.png` | approximately 215 | Tucked against belly |
| `bird_sky_fly_down.png` | approximately 215 | Tucked against belly |

Standing positions are measured at alpha ≥128. Tucked-foot positions are visually checked against the orange toe regions; the lower wing/belly silhouette is not a foot line. Flight feet intentionally do not reach the standing baseline. Tuft tops stay at y=32 for Sunny and y=22–23 for Sky, with at most 1 px raster variation; identities and torso scale remain visually consistent. Tiny generated fur and contour variations remain between poses, so these are registered raster poses, not skeletal animation.

At 64 px grayscale, Sunny remains recognizable by the broad rounded silhouette, open smile, and curl; Sky remains recognizable by the narrow egg silhouette, round stalked bun, half-closed eyes, and contrasting white belly. This passed an author visual comparison on light and dark backgrounds, not a user recognition study.

## Edge, anchor, and style checks

- All 32 foreground assets have real zero-alpha pixels and antialiased edges. The background is fully opaque. The light/dark contact sheet shows no baked checkerboard or obvious white cutout halo at delivery size; intentional fur rim lighting remains.
- The branch begins at x=0 with an opaque clean root edge. At x=110,230,350,470 its top surface is y=38,40,40,40, respectively. Use approximately y=40 as the stand line; the 1–2 tip leaves do not define the standing surface.
- The bush begins at the left canvas edge and covers a root. The house has a green roof, dark round hole, perch peg and short rope. Forest playing band stays clear; no branches, birds or UI are baked into its background.
- **Hand fingertip contact: (23,6)** in the 200 × 200 image, chosen at the alpha ≥128 northwest-facing tip boundary. Normalized bottom-left anchor: approximately **(0.115,0.97)**. The annotated hand QA image marks it with a cross.
- Buttons contain no icons, text or labels. Final icons have white faces and shaded dark-brown outlines; settings/restart centers and the home doorway are genuinely transparent. The coin uses an embossed leaf. The panel and ribbon are blank. Full/empty star silhouettes share the same final solid bounds.
- The portrait asset composite checks branch contact, contrast and relative scale using the v5 design canvas. It is explicitly marked as an asset composite and uses a QA font; it is not a game screenshot or a new layout specification.

## Boundaries and remaining verification

No requested artwork is missing. No code, scenes, runtime level data, packages, project settings, existing original artwork, or hand-authored `.meta` files were changed by this worker. All worker-created files remain within `art/v5/**`, `PerchSort/Assets/Resources/V5/Art/**`, and this report; no commit or push was performed by this worker.

Unity Editor is unavailable on 251. Unity import settings, platform texture compression, real UI composition, live wingbeat timing, frame swaps during movement, clipping under animation, and timed runtime samples have **not been tested** and remain with the coordinator/Mac workflow. Static asset validation and the offline GIF do not establish playable animation acceptance.
