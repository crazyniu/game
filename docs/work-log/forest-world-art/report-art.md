# T36 Worlds 2–4 art delivery report

Date: 2026-11-03. Worker: Codex. Task: `task_698866964502`. Branch at start: `t39-polish`.

Delivered **27/27 requested runtime PNGs**: 12 backgrounds, 12 scene-kit sprites and 3 ambient particles. **Missing files: none.** The [exact filename manifest](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v5/t36/delivered_files.txt) lists every destination. This is raster production and static QA; Unity acceptance remains pending.

## Scope and preservation

The current dispatch and [T36 task](spec.md) explicitly authorize these new assets, with style/world references in [GAME_DESIGN_V5 §§4 and 9](../../GAME_DESIGN.md), [UI_SPEC §4.1](../../UI_SPEC.md#approved-forest-style), and the accepted [T33 delivery](../bird-art/report-art.md). No gameplay semantics were changed.

Only new runtime PNGs under `PerchSort/Assets/Resources/Art/`, supporting files under `art/v5/t36/`, and this report were written. Birdhouses, HUD/UI, map, shop, game code, data, scenes and importer metadata are outside this delivery. No commit or push was made. All **146 pre-existing runtime art files** match the [start baseline](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v5/t36/qa/baseline.json) byte-for-byte; concurrent workers' files were preserved.

## Generation and sizing

Built-in `image_gen.imagegen` produced all 27 raster sources, one call per asset. [Exact prompts](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v5/t36/prompts.md), [job specifications](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v5/t36/jobs.json), [generation log](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v5/t36/generation_log.json), and each raw PNG/JSON in [raw](https://github.com/crazyniu/game/blob/3e82c64734f3e7291a332d1c0e90263954ce40cd/art/v5/t36/raw) retain references and provenance. References are project-owned World 1 art and the new world noon scenes; no competitor images were used.

The generator returned approximate canvas sizes, so final files were sized deterministically. Backgrounds retain the complete image with full-frame resizing to 1080×1920 and opaque RGBA conversion. Cutouts use the same low-alpha cleanup as T33, followed by resizing and registration; hidden RGB is zero where alpha is zero. Perches are uniformly scaled from their solid horizontal bounds, then vertically translated. Their shapes were not procedurally painted or flattened. Top limbs are uniformly resized from the top edge; excess lower transparent canvas is discarded. Particles are uniformly fitted with at least 3px clear margins. Every transform is recorded in [verification.json](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v5/t36/qa/verification.json).

[prepare_worlds.py](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v5/t36/tools/prepare_worlds.py) reproduces the derived files and QA. Publication uses exclusive creation and refuses differing existing destinations:

```bash
python3 art/v5/t36/tools/prepare_worlds.py --publish
```

Two early staging processes were stopped before publication while changing slow PNG optimization to ordinary lossless compression and correcting the contact-sheet height. The final sequential staging and publication completed successfully; there are no unfinished PNG writes or failed final checks.

## Verification and visual evidence

[Automated verification](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v5/t36/qa/verification.json) reports **PASS**, with zero failed file checks. All 27 files decode, match requested dimensions and use RGBA. The 12 backgrounds have alpha **255/255**; the 15 cutouts have alpha **0/255** with antialiased edges. [Per-file visual review](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v5/t36/qa/visual_review.json) records composition and material checks separately from numerical checks.

The backgrounds retain top-corner canopies, low distant hills and a calm middle band at y=550–1500. Bamboo uses narrow leaves and cane nodes, maple uses lobed red/amber leaves, and snow pine uses teal needles and rounded snow caps. Lanterns remain small in the far lower background; windy leaves and aurora ribbons remain outside the board band. Drizzle retains a few fine rain streaks at the far side margins, with the central board clear. No background contains birds, playable branches, UI or text. Style consistency is a visual judgment against the accepted soft dimensional cartoon art, not a claim of pixel identity.

| World | Light/dark contact sheet | Six-species static composite | Perch geometry |
| --- | --- | --- | --- |
| Bamboo | [W2 contact](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v5/t36/qa/w2_contact_light_dark.png) | [W2 composite](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v5/t36/qa/w2_six_species_annotated.png) | [W2 surfaces](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v5/t36/qa/w2_perch_geometry.png) |
| Maple | [W3 contact](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v5/t36/qa/w3_contact_light_dark.png) | [W3 composite](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v5/t36/qa/w3_six_species_annotated.png) | [W3 surfaces](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v5/t36/qa/w3_perch_geometry.png) |
| Snow pine | [W4 contact](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v5/t36/qa/w4_contact_light_dark.png) | [W4 composite](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v5/t36/qa/w4_six_species_annotated.png) | [W4 surfaces](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v5/t36/qa/w4_perch_geometry.png) |

Each composite reuses existing Sunny, Sky, Berry, Mint, Cocoa and Snow idle PNGs on three staggered branches, with edge plants hiding roots. Actual resized solid foot bounds were placed at the local perch surface; all 18 recorded vertical contact errors are zero in the static composition. These placements are illustrative, not level data or an approved runtime layout. Unannotated versions are supplied alongside the labeled composites.

[64px grayscale review](https://github.com/crazyniu/game/blob/b53b70fe91bfa248a4c072d78351ee649212637c/art/v5/t36/qa/recognition_64_grayscale.png) includes each kit and particle. Bamboo leaf, maple leaf and snowflake remain distinct by silhouette. Differences between maple and pine twigs become subtle at a 64px long-edge thumbnail; their native-size materials and wider world foliage remain distinguishable. Light/dark inspection found no baked checkerboard or rectangular matte.

## Perch and top-limb measurements

Coordinates use the PNG top-left origin, y downward. Solid contours use alpha ≥128; these are measured image surfaces, not Unity collider measurements.

| Sprite | Evaluated x interval | Solid surface y min / median / max |
| --- | --- | --- |
| branch_left_w2 | 85–554 | 39 / 40 / 40 |
| branch_left_w3 | 85–554 | 40 / 40 / 40 |
| branch_left_w4 | 85–554 | 38 / 40 / 41 |
| twig_left_w2 | 60–364 | 29 / 32 / 36 |
| twig_left_w3 | 60–364 | 31 / 32 / 33 |
| twig_left_w4 | 60–364 | 31 / 32 / 35 |

Every perch has solid pixels at its left edge and a naturally tapered closed right tip. Main-branch surfaces stay close to y=40 without central decorative leaves obstructing bird feet. Twigs retain natural node/snow variation; exact flatness is not claimed. Suggested left-root/foot-line pivots are `(0, 0.666667)` for branches and `(0, 0.644444)` for twigs in normalized bottom-left coordinates; these suggestions have **not** been installed in importer metadata.

The top limbs contain no rope, hook or hanging knot. Their final nonzero-alpha bottoms are **y=185, 188 and 191** for W2/W3/W4 respectively, and all three have **zero nontransparent pixels at or below y=260**. Visible/solid bounds and pivot suggestions for every file are in the verification record.

## Per-file delivery

| File | Size | Mode | Alpha min/max | Final static checks |
| --- | --- | --- | --- | --- |
| bg_w2_mist.png (`../PerchSort/Assets/Resources/Art/bg_w2_mist.png`, not retained) | 1080×1920 | RGBA | 255/255 | PASS |
| bg_w2_noon.png (`../PerchSort/Assets/Resources/Art/bg_w2_noon.png`, not retained) | 1080×1920 | RGBA | 255/255 | PASS |
| bg_w2_lanterns.png (`../PerchSort/Assets/Resources/Art/bg_w2_lanterns.png`, not retained) | 1080×1920 | RGBA | 255/255 | PASS |
| bg_w2_drizzle.png (`../PerchSort/Assets/Resources/Art/bg_w2_drizzle.png`, not retained) | 1080×1920 | RGBA | 255/255 | PASS |
| branch_left_w2.png (`../PerchSort/Assets/Resources/Art/branch_left_w2.png`, not retained) | 640×120 | RGBA | 0/255 | PASS |
| twig_left_w2.png (`../PerchSort/Assets/Resources/Art/twig_left_w2.png`, not retained) | 420×90 | RGBA | 0/255 | PASS |
| bush_edge_w2.png (`../PerchSort/Assets/Resources/Art/bush_edge_w2.png`, not retained) | 360×300 | RGBA | 0/255 | PASS |
| limb_top_plain_w2.png (`../PerchSort/Assets/Resources/Art/limb_top_plain_w2.png`, not retained) | 1080×320 | RGBA | 0/255 | PASS |
| bg_w3_morning.png (`../PerchSort/Assets/Resources/Art/bg_w3_morning.png`, not retained) | 1080×1920 | RGBA | 255/255 | PASS |
| bg_w3_noon.png (`../PerchSort/Assets/Resources/Art/bg_w3_noon.png`, not retained) | 1080×1920 | RGBA | 255/255 | PASS |
| bg_w3_sunset.png (`../PerchSort/Assets/Resources/Art/bg_w3_sunset.png`, not retained) | 1080×1920 | RGBA | 255/255 | PASS |
| bg_w3_windy.png (`../PerchSort/Assets/Resources/Art/bg_w3_windy.png`, not retained) | 1080×1920 | RGBA | 255/255 | PASS |
| branch_left_w3.png (`../PerchSort/Assets/Resources/Art/branch_left_w3.png`, not retained) | 640×120 | RGBA | 0/255 | PASS |
| twig_left_w3.png (`../PerchSort/Assets/Resources/Art/twig_left_w3.png`, not retained) | 420×90 | RGBA | 0/255 | PASS |
| bush_edge_w3.png (`../PerchSort/Assets/Resources/Art/bush_edge_w3.png`, not retained) | 360×300 | RGBA | 0/255 | PASS |
| limb_top_plain_w3.png (`../PerchSort/Assets/Resources/Art/limb_top_plain_w3.png`, not retained) | 1080×320 | RGBA | 0/255 | PASS |
| bg_w4_morning.png (`../PerchSort/Assets/Resources/Art/bg_w4_morning.png`, not retained) | 1080×1920 | RGBA | 255/255 | PASS |
| bg_w4_noon.png (`../PerchSort/Assets/Resources/Art/bg_w4_noon.png`, not retained) | 1080×1920 | RGBA | 255/255 | PASS |
| bg_w4_bluehour.png (`../PerchSort/Assets/Resources/Art/bg_w4_bluehour.png`, not retained) | 1080×1920 | RGBA | 255/255 | PASS |
| bg_w4_aurora.png (`../PerchSort/Assets/Resources/Art/bg_w4_aurora.png`, not retained) | 1080×1920 | RGBA | 255/255 | PASS |
| branch_left_w4.png (`../PerchSort/Assets/Resources/Art/branch_left_w4.png`, not retained) | 640×120 | RGBA | 0/255 | PASS |
| twig_left_w4.png (`../PerchSort/Assets/Resources/Art/twig_left_w4.png`, not retained) | 420×90 | RGBA | 0/255 | PASS |
| bush_edge_w4.png (`../PerchSort/Assets/Resources/Art/bush_edge_w4.png`, not retained) | 360×300 | RGBA | 0/255 | PASS |
| limb_top_plain_w4.png (`../PerchSort/Assets/Resources/Art/limb_top_plain_w4.png`, not retained) | 1080×320 | RGBA | 0/255 | PASS |
| leaf_bamboo.png (`../PerchSort/Assets/Resources/Art/leaf_bamboo.png`, not retained) | 48×96 | RGBA | 0/255 | PASS |
| leaf_maple.png (`../PerchSort/Assets/Resources/Art/leaf_maple.png`, not retained) | 64×64 | RGBA | 0/255 | PASS |
| snowflake.png (`../PerchSort/Assets/Resources/Art/snowflake.png`, not retained) | 48×48 | RGBA | 0/255 | PASS |

## Remaining integration

The Linux development machine has no Unity Editor. Unity import settings and pivots, texture compression/filtering on device, assembled runtime screenshots, compile/EditMode/PlayMode tests, ambient-particle playback and timed runtime samples were **not run**. These remain for the coordinator's Mac integration workflow. The supplied images are static assets and composites, not evidence of playable animation or release acceptance.
