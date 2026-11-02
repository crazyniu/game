# T33 art delivery report

Date: 2026-11-03. Worker: Codex, task `task_c00c74f24826`.

Delivered **108/108 requested runtime PNGs**; **missing files: none**. The original T33 scope supplies 85 files; the coordinator's v5.3 addendum supplies 23. This report covers raster production and static asset QA, not runtime acceptance.

## Authority and ownership

- [T33 task](T33_v5_art_birds.md), [GAME_DESIGN_V5](../docs/GAME_DESIGN_V5.md) v5.3, and the coordinator messages recorded in [addendum.md](../art/v5/t33/addendum.md).
- The later Hoot correction is applied: pale tawny/cream round owl, heart-shaped white facial disc, amber eyes, star-patterned nightcap, grey-brown speckled wings, **no ear tufts**.
- New runtime PNGs only in `PerchSort/Assets/Resources/V5/Art/`; raw, prompts, tools and QA only in `art/v5/t33/`; this report only in `tasks/`.
- No code, runtime data, scenes, import metadata or original artwork changed by this worker; no commit or push.
- The start-of-task [baseline](../art/v5/t33/qa/baseline.json) hashes 93 existing art-related files. All **34 existing runtime PNGs** remain byte-identical. The one observed change outside ownership is `art/v5/tools/process_t31_limb.py`, owned by the concurrent limb task; this worker did not write it.

## Delivered groups

| Group | Files | Delivery |
| --- | ---: | --- |
| Original bird frames | 64 | Seven species x eight poses; Sunny and Sky x four missing poses |
| Special | 8 | Zzz, five egg frames, twig, tintable roof cloth |
| Boosters / badges | 8 | Six booster pictograms and two badges |
| World 1 scenes / particles | 5 | Three backgrounds, petal, raindrop |
| Hoot | 8 | Idle, blink, two head-turn quirks, two flight poses, cheer, sleep |
| v5.3 additions besides Hoot | 15 | Bow, fluff, swivel knot, nine gift icons, two dial icons, night overlay |
| **Total** | **108** | **No missing filenames** |

The exact delivered paths are in [delivered_files.txt](../art/v5/t33/delivered_files.txt) and the per-file table below.

## Static verification

[verification.json](../art/v5/t33/qa/verification.json) records every final file's dimensions, mode, alpha extrema, visible/solid bounds, baseline where applicable, SHA-256 and check results.

- **108/108** decode successfully, have the required dimensions and are RGBA.
- **104** cutout sprites have alpha min/max **0/255**; three scene backgrounds are opaque **255/255**; night overlay is **140/140**, or **54.90%** opacity, with a generated deep-blue color gradient and sparse stars.
- **56/56 newly delivered standing bird frames** and **5/5 egg frames** have the solid contact baseline at **y=244** (alpha >=128).
- **16 flight frames** have tucked feet and different up/down wing positions. Their solid bottom is deliberately not forced to y=244: the idle canvas/body registration is retained while wings and tucked feet move. All final bird solid bounds stay inside the canvas.
- RGB is zero wherever alpha is zero. Light/dark contact inspection found no baked checkerboard, rectangular matte or unwanted surrounding halo in the processed outputs. Intended feather rim lighting, soft Zzz shading and the firefly abdomen glow remain.
- Sleep frames have closed eyes and no baked Zzz; the separate Zzz sprite is used in the board QA.
- Cloth RGB is neutral grayscale for runtime tinting. Twig root/top starts around y=32 on its 420x90 canvas.
- The initial Rook flight generations had an extra trinket-holding wing. Both were rejected; the selected `_v2` raw generations have exactly two wings, tucked feet and the trinket in the beak. [selections.json](../art/v5/t33/selections.json) records this choice.
- Hoot and Cocoa remain distinct in silhouette and grayscale: smooth pale head with cap/large heart disc versus darker brown body with ear tufts.

The final verifier reports zero failed file checks. An intermediate QA read raced a staged PNG write and failed with a truncated-file error; it did not affect runtime files. Staged writes now use atomic replacement, and final verification was rerun after production completed.

## Visual review artifacts

- [Full light/dark contact sheet](../art/v5/t33/qa/contact.png), with all 108 new assets.
- [64px grayscale idles](../art/v5/t33/qa/grayscale_64_all.png): original nine plus Hoot, placed beside Cocoa.
- [Hoot/Cocoa 64px comparison](../art/v5/t33/qa/hoot_cocoa_grayscale_64.png).
- [48px icons](../art/v5/t33/qa/icons_48.png), [special sprites](../art/v5/t33/qa/special_light_dark.png), [v5.3 accessories and gifts](../art/v5/t33/qa/addendum_light_dark.png).
- Ten `qa/bird_{species}_frames.png` pose strips include the preserved Sunny/Sky reference frames.
- [Golden-hour board composite](../art/v5/t33/qa/w1_board_composite.png): six idle species, egg, sleeping Ember plus separate Zzz, and Rook on branches.
- [Night-overlay composite](../art/v5/t33/qa/night_overlay_composite.png).
- These are explicitly labeled static asset composites. They are **not Unity screenshots or proof of playable animation**.

## Body scale and precision limits

[Annotated body bounds](../art/v5/t33/qa/body_bbox_annotations.png) and [body_bounds.json](../art/v5/t33/qa/body_bounds.json) use visible crown-to-belly landmarks, excluding crest/hat, wings and feet. These are manual measurements with roughly +/-3px uncertainty, not segmentation ground truth.

| Species | Body top y | Body bottom y | Height px |
| --- | ---: | ---: | ---: |
| Sunny (preserved) | 66 | 222 | 157 |
| Sky (preserved) | 58 | 224 | 167 |
| Berry | 77 | 224 | 148 |
| Mint | 65 | 223 | 159 |
| Cocoa | 66 | 224 | 159 |
| Hoot | 73 | 223 | 151 |
| Snow | 65 | 223 | 159 |
| Ember | 75 | 223 | 149 |
| Ink | 64 | 223 | 160 |
| Rook | 65 | 223 | 159 |

The species share the 256px canvas, common foot line and approximately matched body scale; their body heights are not numerically identical. The new identities span 148–160px, while the preserved elongated Sky measures 167px.

Egg wobble directions and the stationary nest placement pass visual review. The requested +/-10-degree tilt was supplied in the generation prompts, but the generated frames are not an exact rigid rotation of a common segmented egg layer; exact angular and pixel-for-pixel nest-lock acceptance is **not proven**. An upper-shell centerline fit over y=65..170 gives an approximate visual lean proxy of -0.6 degrees idle, +6.8 left and -8.2 right, which is not a full 3D orientation measurement. The five nest contact lines are exactly y=244.

Quirk poses, blink/idle differences, flight/body stability and sleep silhouettes were reviewed as static keyframes. Playback timing, pose jitter during interpolation, particle motion and final compositing order still require runtime review.

## Provenance and reproducibility

- Built-in `image_gen.imagegen` generated every new raster. References are this project's accepted art or newly generated character references; no competitor images were supplied.
- [Exact prompts](../art/v5/t33/prompts.md), [generation log](../art/v5/t33/generation_log.json), per-generation JSON and 110 source PNGs in `raw/` preserve all 108 selected assets plus the two rejected Rook flight drafts.
- [process_t33.py](../art/v5/t33/tools/process_t33.py) cleans low-alpha generation residue, zeros hidden RGB, resizes/registers sprites, normalizes the roof cloth, builds contact sheets and publishes only absent or byte-identical files.
- The night-overlay source's low-alpha bottom fringe is cropped before resizing, then its full-frame alpha is set to 140. It remains an overlay, with no additional landscape.
- [verify_t33.py](../art/v5/t33/tools/verify_t33.py) creates the board/comparison artifacts and checks runtime PNGs against the exact manifest.

Commands run successfully:

```bash
python3 art/v5/t33/tools/process_t33.py
python3 art/v5/t33/tools/verify_t33.py
```

Publication uses exclusive file creation; any differing existing destination is rejected. Existing art files were never replaced.

## Not performed / remaining integration

The Linux development machine has no Unity Editor. Unity import settings, generated .meta files, pivots in the importer, in-game screenshots, timed animation samples, compile and EditMode/PlayMode tests were **not run**. The coordinator's Mac workflow must verify these after integration; these static checks do not imply runtime acceptance. Precise geometric egg tilt and numerical body-height equivalence remain review limits described above, not claimed passes.

## Per-file final checks

All rows below passed dimensions, RGBA, expected alpha and hidden-RGB checks. Standing baseline entries also passed y=244; flight entries show the actual solid-bottom bound instead of claiming a standing baseline.

| File | Pixels | Mode | Alpha min/max | Solid bottom y | Result |
| --- | --- | --- | --- | --- | --- |
| [bird_sunny_quirk1.png](../PerchSort/Assets/Resources/V5/Art/bird_sunny_quirk1.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_sunny_quirk2.png](../PerchSort/Assets/Resources/V5/Art/bird_sunny_quirk2.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_sunny_cheer.png](../PerchSort/Assets/Resources/V5/Art/bird_sunny_cheer.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_sunny_sleep.png](../PerchSort/Assets/Resources/V5/Art/bird_sunny_sleep.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_sky_quirk1.png](../PerchSort/Assets/Resources/V5/Art/bird_sky_quirk1.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_sky_quirk2.png](../PerchSort/Assets/Resources/V5/Art/bird_sky_quirk2.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_sky_cheer.png](../PerchSort/Assets/Resources/V5/Art/bird_sky_cheer.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_sky_sleep.png](../PerchSort/Assets/Resources/V5/Art/bird_sky_sleep.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_berry_idle.png](../PerchSort/Assets/Resources/V5/Art/bird_berry_idle.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_berry_blink.png](../PerchSort/Assets/Resources/V5/Art/bird_berry_blink.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_berry_quirk1.png](../PerchSort/Assets/Resources/V5/Art/bird_berry_quirk1.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_berry_quirk2.png](../PerchSort/Assets/Resources/V5/Art/bird_berry_quirk2.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_berry_fly_up.png](../PerchSort/Assets/Resources/V5/Art/bird_berry_fly_up.png) | 256x256 | RGBA | 0/255 | 223 (airborne) | PASS |
| [bird_berry_fly_down.png](../PerchSort/Assets/Resources/V5/Art/bird_berry_fly_down.png) | 256x256 | RGBA | 0/255 | 242 (airborne) | PASS |
| [bird_berry_cheer.png](../PerchSort/Assets/Resources/V5/Art/bird_berry_cheer.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_berry_sleep.png](../PerchSort/Assets/Resources/V5/Art/bird_berry_sleep.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_mint_idle.png](../PerchSort/Assets/Resources/V5/Art/bird_mint_idle.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_mint_blink.png](../PerchSort/Assets/Resources/V5/Art/bird_mint_blink.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_mint_quirk1.png](../PerchSort/Assets/Resources/V5/Art/bird_mint_quirk1.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_mint_quirk2.png](../PerchSort/Assets/Resources/V5/Art/bird_mint_quirk2.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_mint_fly_up.png](../PerchSort/Assets/Resources/V5/Art/bird_mint_fly_up.png) | 256x256 | RGBA | 0/255 | 224 (airborne) | PASS |
| [bird_mint_fly_down.png](../PerchSort/Assets/Resources/V5/Art/bird_mint_fly_down.png) | 256x256 | RGBA | 0/255 | 231 (airborne) | PASS |
| [bird_mint_cheer.png](../PerchSort/Assets/Resources/V5/Art/bird_mint_cheer.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_mint_sleep.png](../PerchSort/Assets/Resources/V5/Art/bird_mint_sleep.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_cocoa_idle.png](../PerchSort/Assets/Resources/V5/Art/bird_cocoa_idle.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_cocoa_blink.png](../PerchSort/Assets/Resources/V5/Art/bird_cocoa_blink.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_cocoa_quirk1.png](../PerchSort/Assets/Resources/V5/Art/bird_cocoa_quirk1.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_cocoa_quirk2.png](../PerchSort/Assets/Resources/V5/Art/bird_cocoa_quirk2.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_cocoa_fly_up.png](../PerchSort/Assets/Resources/V5/Art/bird_cocoa_fly_up.png) | 256x256 | RGBA | 0/255 | 223 (airborne) | PASS |
| [bird_cocoa_fly_down.png](../PerchSort/Assets/Resources/V5/Art/bird_cocoa_fly_down.png) | 256x256 | RGBA | 0/255 | 235 (airborne) | PASS |
| [bird_cocoa_cheer.png](../PerchSort/Assets/Resources/V5/Art/bird_cocoa_cheer.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_cocoa_sleep.png](../PerchSort/Assets/Resources/V5/Art/bird_cocoa_sleep.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_hoot_idle.png](../PerchSort/Assets/Resources/V5/Art/bird_hoot_idle.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_hoot_blink.png](../PerchSort/Assets/Resources/V5/Art/bird_hoot_blink.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_hoot_quirk1.png](../PerchSort/Assets/Resources/V5/Art/bird_hoot_quirk1.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_hoot_quirk2.png](../PerchSort/Assets/Resources/V5/Art/bird_hoot_quirk2.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_hoot_fly_up.png](../PerchSort/Assets/Resources/V5/Art/bird_hoot_fly_up.png) | 256x256 | RGBA | 0/255 | 226 (airborne) | PASS |
| [bird_hoot_fly_down.png](../PerchSort/Assets/Resources/V5/Art/bird_hoot_fly_down.png) | 256x256 | RGBA | 0/255 | 247 (airborne) | PASS |
| [bird_hoot_cheer.png](../PerchSort/Assets/Resources/V5/Art/bird_hoot_cheer.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_hoot_sleep.png](../PerchSort/Assets/Resources/V5/Art/bird_hoot_sleep.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_snow_idle.png](../PerchSort/Assets/Resources/V5/Art/bird_snow_idle.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_snow_blink.png](../PerchSort/Assets/Resources/V5/Art/bird_snow_blink.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_snow_quirk1.png](../PerchSort/Assets/Resources/V5/Art/bird_snow_quirk1.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_snow_quirk2.png](../PerchSort/Assets/Resources/V5/Art/bird_snow_quirk2.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_snow_fly_up.png](../PerchSort/Assets/Resources/V5/Art/bird_snow_fly_up.png) | 256x256 | RGBA | 0/255 | 220 (airborne) | PASS |
| [bird_snow_fly_down.png](../PerchSort/Assets/Resources/V5/Art/bird_snow_fly_down.png) | 256x256 | RGBA | 0/255 | 239 (airborne) | PASS |
| [bird_snow_cheer.png](../PerchSort/Assets/Resources/V5/Art/bird_snow_cheer.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_snow_sleep.png](../PerchSort/Assets/Resources/V5/Art/bird_snow_sleep.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_ember_idle.png](../PerchSort/Assets/Resources/V5/Art/bird_ember_idle.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_ember_blink.png](../PerchSort/Assets/Resources/V5/Art/bird_ember_blink.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_ember_quirk1.png](../PerchSort/Assets/Resources/V5/Art/bird_ember_quirk1.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_ember_quirk2.png](../PerchSort/Assets/Resources/V5/Art/bird_ember_quirk2.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_ember_fly_up.png](../PerchSort/Assets/Resources/V5/Art/bird_ember_fly_up.png) | 256x256 | RGBA | 0/255 | 224 (airborne) | PASS |
| [bird_ember_fly_down.png](../PerchSort/Assets/Resources/V5/Art/bird_ember_fly_down.png) | 256x256 | RGBA | 0/255 | 242 (airborne) | PASS |
| [bird_ember_cheer.png](../PerchSort/Assets/Resources/V5/Art/bird_ember_cheer.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_ember_sleep.png](../PerchSort/Assets/Resources/V5/Art/bird_ember_sleep.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_ink_idle.png](../PerchSort/Assets/Resources/V5/Art/bird_ink_idle.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_ink_blink.png](../PerchSort/Assets/Resources/V5/Art/bird_ink_blink.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_ink_quirk1.png](../PerchSort/Assets/Resources/V5/Art/bird_ink_quirk1.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_ink_quirk2.png](../PerchSort/Assets/Resources/V5/Art/bird_ink_quirk2.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_ink_fly_up.png](../PerchSort/Assets/Resources/V5/Art/bird_ink_fly_up.png) | 256x256 | RGBA | 0/255 | 222 (airborne) | PASS |
| [bird_ink_fly_down.png](../PerchSort/Assets/Resources/V5/Art/bird_ink_fly_down.png) | 256x256 | RGBA | 0/255 | 237 (airborne) | PASS |
| [bird_ink_cheer.png](../PerchSort/Assets/Resources/V5/Art/bird_ink_cheer.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_ink_sleep.png](../PerchSort/Assets/Resources/V5/Art/bird_ink_sleep.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_crow_idle.png](../PerchSort/Assets/Resources/V5/Art/bird_crow_idle.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_crow_blink.png](../PerchSort/Assets/Resources/V5/Art/bird_crow_blink.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_crow_quirk1.png](../PerchSort/Assets/Resources/V5/Art/bird_crow_quirk1.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_crow_quirk2.png](../PerchSort/Assets/Resources/V5/Art/bird_crow_quirk2.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_crow_fly_up.png](../PerchSort/Assets/Resources/V5/Art/bird_crow_fly_up.png) | 256x256 | RGBA | 0/255 | 224 (airborne) | PASS |
| [bird_crow_fly_down.png](../PerchSort/Assets/Resources/V5/Art/bird_crow_fly_down.png) | 256x256 | RGBA | 0/255 | 250 (airborne) | PASS |
| [bird_crow_cheer.png](../PerchSort/Assets/Resources/V5/Art/bird_crow_cheer.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [bird_crow_sleep.png](../PerchSort/Assets/Resources/V5/Art/bird_crow_sleep.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [egg_idle.png](../PerchSort/Assets/Resources/V5/Art/egg_idle.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [egg_wobble_l.png](../PerchSort/Assets/Resources/V5/Art/egg_wobble_l.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [egg_wobble_r.png](../PerchSort/Assets/Resources/V5/Art/egg_wobble_r.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [egg_crack.png](../PerchSort/Assets/Resources/V5/Art/egg_crack.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [egg_shell.png](../PerchSort/Assets/Resources/V5/Art/egg_shell.png) | 256x256 | RGBA | 0/255 | 244 | PASS |
| [zzz.png](../PerchSort/Assets/Resources/V5/Art/zzz.png) | 128x128 | RGBA | 0/255 | — | PASS |
| [twig_left.png](../PerchSort/Assets/Resources/V5/Art/twig_left.png) | 420x90 | RGBA | 0/255 | — | PASS |
| [roof_band.png](../PerchSort/Assets/Resources/V5/Art/roof_band.png) | 300x60 | RGBA | 0/255 | — | PASS |
| [icon_firefly.png](../PerchSort/Assets/Resources/V5/Art/icon_firefly.png) | 192x192 | RGBA | 0/255 | — | PASS |
| [icon_sapling.png](../PerchSort/Assets/Resources/V5/Art/icon_sapling.png) | 192x192 | RGBA | 0/255 | — | PASS |
| [icon_owl_eye.png](../PerchSort/Assets/Resources/V5/Art/icon_owl_eye.png) | 192x192 | RGBA | 0/255 | — | PASS |
| [icon_bell.png](../PerchSort/Assets/Resources/V5/Art/icon_bell.png) | 192x192 | RGBA | 0/255 | — | PASS |
| [icon_scarecrow.png](../PerchSort/Assets/Resources/V5/Art/icon_scarecrow.png) | 192x192 | RGBA | 0/255 | — | PASS |
| [icon_rain.png](../PerchSort/Assets/Resources/V5/Art/icon_rain.png) | 192x192 | RGBA | 0/255 | — | PASS |
| [badge_count.png](../PerchSort/Assets/Resources/V5/Art/badge_count.png) | 64x64 | RGBA | 0/255 | — | PASS |
| [badge_plus.png](../PerchSort/Assets/Resources/V5/Art/badge_plus.png) | 64x64 | RGBA | 0/255 | — | PASS |
| [bg_w1_morning.png](../PerchSort/Assets/Resources/V5/Art/bg_w1_morning.png) | 1080x1920 | RGBA | 255/255 | — | PASS |
| [bg_w1_golden.png](../PerchSort/Assets/Resources/V5/Art/bg_w1_golden.png) | 1080x1920 | RGBA | 255/255 | — | PASS |
| [bg_w1_rain.png](../PerchSort/Assets/Resources/V5/Art/bg_w1_rain.png) | 1080x1920 | RGBA | 255/255 | — | PASS |
| [petal.png](../PerchSort/Assets/Resources/V5/Art/petal.png) | 64x64 | RGBA | 0/255 | — | PASS |
| [raindrop.png](../PerchSort/Assets/Resources/V5/Art/raindrop.png) | 32x64 | RGBA | 0/255 | — | PASS |
| [mama_bow.png](../PerchSort/Assets/Resources/V5/Art/mama_bow.png) | 96x96 | RGBA | 0/255 | — | PASS |
| [chick_fluff.png](../PerchSort/Assets/Resources/V5/Art/chick_fluff.png) | 128x128 | RGBA | 0/255 | — | PASS |
| [swivel_knot.png](../PerchSort/Assets/Resources/V5/Art/swivel_knot.png) | 160x160 | RGBA | 0/255 | — | PASS |
| [gift_wake.png](../PerchSort/Assets/Resources/V5/Art/gift_wake.png) | 128x128 | RGBA | 0/255 | — | PASS |
| [gift_breeze.png](../PerchSort/Assets/Resources/V5/Art/gift_breeze.png) | 128x128 | RGBA | 0/255 | — | PASS |
| [gift_encore.png](../PerchSort/Assets/Resources/V5/Art/gift_encore.png) | 128x128 | RGBA | 0/255 | — | PASS |
| [gift_gardener.png](../PerchSort/Assets/Resources/V5/Art/gift_gardener.png) | 128x128 | RGBA | 0/255 | — | PASS |
| [gift_stare.png](../PerchSort/Assets/Resources/V5/Art/gift_stare.png) | 128x128 | RGBA | 0/255 | — | PASS |
| [gift_lightstep.png](../PerchSort/Assets/Resources/V5/Art/gift_lightstep.png) | 128x128 | RGBA | 0/255 | — | PASS |
| [gift_warm.png](../PerchSort/Assets/Resources/V5/Art/gift_warm.png) | 128x128 | RGBA | 0/255 | — | PASS |
| [gift_night.png](../PerchSort/Assets/Resources/V5/Art/gift_night.png) | 128x128 | RGBA | 0/255 | — | PASS |
| [gift_moonbeam.png](../PerchSort/Assets/Resources/V5/Art/gift_moonbeam.png) | 128x128 | RGBA | 0/255 | — | PASS |
| [dial_sun.png](../PerchSort/Assets/Resources/V5/Art/dial_sun.png) | 128x128 | RGBA | 0/255 | — | PASS |
| [dial_moon.png](../PerchSort/Assets/Resources/V5/Art/dial_moon.png) | 128x128 | RGBA | 0/255 | — | PASS |
| [night_overlay.png](../PerchSort/Assets/Resources/V5/Art/night_overlay.png) | 1080x1920 | RGBA | 140/140 | — | PASS |
