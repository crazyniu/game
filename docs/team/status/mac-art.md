# Status: mac-art

ACTIVE: resumed by lead on 2026-11-04. Layout first, then remaining gate and gift assemblies.

Branch: `mac-art/t48`. Last completed pushed milestones before wrap-up: initial HUD `0cb3741`, complete HUD `79aef75`, panels/Aviary `09efd6d`. This file and all remaining owned work are included in the subsequent `wip:` wrap-up commit (read branch HEAD for its hash).

## Done: static assets, not Unity acceptance
All names below live under `PerchSort/Assets/Resources/V7/Art/`.

### HUD
- `badge_red.png`
- `btn_green.png`
- `btn_green_pill.png`
- `btn_green_pressed.png`
- `btn_purple.png`
- `btn_purple_pressed.png`
- `chain_bonus.png`
- `check_green.png`
- `coin.png`
- `coin_pill.png`
- `flag.png`
- `footprints.png`
- `gift_feather.png`
- `gift_feather_counter.png`
- `gift_flame.png`
- `gift_grumpy_eyes.png`
- `gift_moon.png`
- `gift_moon_ray.png`
- `gift_sun_note.png`
- `gift_watering_can.png`
- `gift_wind.png`
- `icon_aviary.png`
- `icon_basket.png`
- `icon_bell.png`
- `icon_branch_plus.png`
- `icon_bulb.png`
- `icon_check.png`
- `icon_close.png`
- `icon_egg_eye.png`
- `icon_gear.png`
- `icon_home.png`
- `icon_next_arrow.png`
- `icon_play.png`
- `icon_rain_sprout.png`
- `icon_replay.png`
- `icon_scarecrow.png`
- `icon_sound_off.png`
- `icon_sound_on.png`
- `icon_undo.png`
- `padlock.png`
- `star_empty.png`
- `star_empty_small.png`
- `star_full.png`
- `star_full_small.png`

### Panels / Shop / Aviary
- `panel_wood.png`
- `card_wood.png`
- `ribbon_green.png`
- `ribbon_green_left.png`
- `ribbon_green_middle.png`
- `ribbon_green_right.png`
- `leaf_badge.png`
- `name_ribbon.png`
- `stat_pill.png`
- `aviary_frame.png`
- `aviary_board.png`
- `silhouette_frame.png`
- `house_count_chip.png`
- `panel_rope_left.png`
- `panel_rope_right.png`
- `tab_selected_ring.png`

### Map
- `map_bg_w1.png`
- `map_bg_w2.png`
- `map_bg_w3.png`
- `map_bg_w4.png`
- `node_stone_locked.png`
- `node_wood.png`
- `node_wood_current_glow.png`
- `world_sign_w1.png`
- `world_sign_w2.png`
- `world_sign_w3.png`
- `world_sign_w4.png`

Four backgrounds are 1080x3000 RGBA with a baked 160 px top alpha fade. Stack at 2840 px stride. All three adjacent joins and four repeated-world joins verified by ordinary alpha compositing. Topmost segment needs viewport clamping at local y>=160 or opaque-source backing. Measured 40 node centers and anchors: `art/v7/runtime/map/world_paths.json`. The current node glow is a whole-node replacement, not an additive-only overlay. Records and QA: `art/v7/runtime/map/`.

### Effects and particles
- `light_rays.png`
- `sparkle.png`
- `confetti_1.png`
- `confetti_2.png`
- `confetti_3.png`
- `confetti_4.png`
- `glow_soft.png`
- `zzz.png`
- `leaf_fall_w1.png`
- `leaf_fall_w2.png`
- `leaf_fall_w3.png`
- `leaf_fall_w4.png`
- `cloud_soft_1.png`
- `cloud_soft_2.png`
- `cloud_soft_3.png`
- `firefly.png`
- `leaf_wipe.png`
- `gift_trail.png`
- `house_glow.png`
- `gate_light.png`

Twenty effects have real alpha, recorded sizes and approved-screen overlay QA. Records: `art/v7/runtime/effects/delivery.json`, `report.md`, `qa/`. Gate light is exported but not yet aligned to the unfinished gate assembly; aperture anchor is normalized top-left (0.50,0.25), separate from its central transform pivot.

## Half done / WIP
### Houses: static review complete
- `house_beam_w1.png`
- `house_beam_w2.png`
- `house_beam_w3.png`
- `house_beam_w4.png`
- `house_body.png`
- `house_door_leaf.png`
- `house_door_open.png`
- `house_roof_blue.png`
- `house_roof_green.png`
- `house_roof_orange.png`
- `house_roof_pink.png`
- `house_roof_purple.png`
- `house_roof_red.png`
- `house_roof_teal.png`
- `house_roof_yellow.png`
- `house_rope.png`

`art/v7/runtime/houses/delivery.json` records shared 512x512 canvases, body peak (256,96), roof anchor (256,58), door center (253,306), left hinge (145,306), rope and beam anchors. Sources and QA are in `art/v7/runtime/houses/`. Worker caught blue/green roof color drift and an open-door hinge mismatch and regenerated replacements; parent reviewed the replacements and shared-canvas assembly seams on resume. House report finalized: `art/v7/runtime/houses/report.md`. Do not call this accepted runtime animation.

### Gate work in progress; gift parts delivered
Saved under `art/v7/runtime/assemblies_wip/raw/`:
- `gate_arch.png`: empty-opening arch generated, not sized/aligned to leaves.
- `gift_nest.png`: generated; exterior glow requires alpha cleanup.
- `gift_box_base.png`: generated, not aligned to lid/nest.
- `gift_box_lid.png`: generated; exterior glow requires cleanup and lift anchor.

Exact prompts/reference/source paths preserved in `assemblies_wip/sources.json`. Gate arch is still being prepared. Gift runtime parts are now delivered: `gift_nest.png`, `gift_nest_front.png`, `gift_box_base.png`, `gift_box_lid.png`; shared 768x768 canvas, nest/base/lid/front draw order and lid lift (0,-130). Metadata and reviewed closed/open QA: `art/v7/runtime/gift/`.

### Layout: ready for early integration
`art/v7/layout_v7.json` is reviewed with schema_version 1: 107 asset records, 17 screens, 223 measured boxes, 40 map nodes, coin text frame, slice borders and integrated house anchors. Gate/gift assemblies remain explicitly pending. Intermediate house arrangements are inferred, not observed. Geometry, asset, slice and path checks passed. Measurement evidence: `art/v7/runtime/measurements/` (17 screens, 223 measured boxes). Do not treat inferred K1..6 adaptations as measured approved layouts.

`art/v7/runtime/hud_extra_provenance.json` describes rejected experimental cutouts; final next-arrow/chain assets came from generation_manifest.json instead. Retained only as WIP history, not authoritative provenance.

## Next, resumed work
1. DONE: house replacements reviewed, report and layout anchors finalized.
2. Prepare gate_arch and gift raw images into aligned runtime parts with clean alpha.
3. Generate missing `gate_door_left.png`, `gate_door_right.png`, `gate_padlock.png`, `gate_backdrop_w2.png`, `gate_backdrop_w3.png`, `gate_backdrop_w4.png`; assemble `gate_locked.png` and `gate_open.png`, record hinges, opening mask and gate-light placement.
4. DONE: gift parts prepared and reviewed, including a foreground nest split; add metadata to final layout.
5. Complete layout_v7.json, reconcile all dimensions/filenames/anchors, and review each group against approved screens; consolidate final T48_report.md and QA index.
6. Commit/push remaining groups on resume. Unity import, runtime views and timed animations belong to code/QA workers and remain NOT_RUN.

## Blocker and shutdown
Credits restored; layout, gift and gate workers resumed with separate ownership. No current external blocker. Remaining assembly work is in progress. No Unity process was started and no `.unity-lock` was created by mac-art, so no external Unity process or lock was touched. Existing originals and other roles' code remain untouched. T48 is INCOMPLETE.
