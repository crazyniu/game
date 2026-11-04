# Status: mac-art

DONE: T48 static runtime art and measured layout delivered. Unity integration and animation acceptance remain with code/QA.

Role: produce approved-reference runtime art and measured layout metadata; no Unity or gameplay code.

Branch: `mac-art/t48`. Pushed milestones: `0cb3741` HUD surfaces; `79aef75` HUD glyphs/badges; `09efd6d` panels/Aviary; `924072c` preserved map/effects/houses; `83d6043` measured layout; `9f60753` house review and gift parts; `36e59c7` gates and complete inventory. This status is included in the final layout delivery commit (read branch HEAD for its hash).

Owned paths: `PerchSort/Assets/Resources/V7/Art/**`, `art/v7/runtime/**`, `art/v7/layout_v7.json`, `tasks/T48_report.md`, and this status file. Approved originals, other workers' code and Unity state are preserved.

## Done: static art

120 final RGBA PNGs: 119 requested plus the derived nest foreground needed for correct depth. Every export has real transparent pixels. Exact inventory is `art/v7/runtime/delivery_inventory.json`; reports/provenance/QA are grouped under `art/v7/runtime/`. Review entry point: `art/v7/runtime/qa/README.md`.

All names below are in `PerchSort/Assets/Resources/V7/Art/`.

### hud (44)

`coin_pill.png`, `btn_purple.png`, `btn_purple_pressed.png`, `btn_green.png`, `btn_green_pressed.png`, `btn_green_pill.png`, `badge_red.png`, `coin.png`, `star_full.png`, `star_empty.png`, `star_full_small.png`, `star_empty_small.png`, `footprints.png`, `flag.png`, `padlock.png`, `check_green.png`, `chain_bonus.png`, `icon_gear.png`, `icon_undo.png`, `icon_bulb.png`, `icon_branch_plus.png`, `icon_egg_eye.png`, `icon_bell.png`, `icon_scarecrow.png`, `icon_rain_sprout.png`, `icon_basket.png`, `icon_aviary.png`, `icon_home.png`, `icon_replay.png`, `icon_sound_on.png`, `icon_sound_off.png`, `icon_close.png`, `icon_play.png`, `icon_next_arrow.png`, `icon_check.png`, `gift_sun_note.png`, `gift_wind.png`, `gift_grumpy_eyes.png`, `gift_watering_can.png`, `gift_feather.png`, `gift_flame.png`, `gift_moon.png`, `gift_moon_ray.png`, `gift_feather_counter.png`.

### panels (16)

`panel_wood.png`, `card_wood.png`, `ribbon_green.png`, `ribbon_green_left.png`, `ribbon_green_middle.png`, `ribbon_green_right.png`, `leaf_badge.png`, `name_ribbon.png`, `stat_pill.png`, `tab_selected_ring.png`, `aviary_frame.png`, `aviary_board.png`, `silhouette_frame.png`, `house_count_chip.png`, `panel_rope_left.png`, `panel_rope_right.png`.

### houses (16)

`house_body.png`, `house_rope.png`, `house_door_leaf.png`, `house_door_open.png`, `house_roof_teal.png`, `house_roof_red.png`, `house_roof_yellow.png`, `house_roof_purple.png`, `house_roof_green.png`, `house_roof_blue.png`, `house_roof_orange.png`, `house_roof_pink.png`, `house_beam_w1.png`, `house_beam_w2.png`, `house_beam_w3.png`, `house_beam_w4.png`.

### map (11)

`map_bg_w1.png`, `map_bg_w2.png`, `map_bg_w3.png`, `map_bg_w4.png`, `world_sign_w1.png`, `world_sign_w2.png`, `world_sign_w3.png`, `world_sign_w4.png`, `node_wood.png`, `node_wood_current_glow.png`, `node_stone_locked.png`.

### effects (20)

`light_rays.png`, `sparkle.png`, `confetti_1.png`, `confetti_2.png`, `confetti_3.png`, `confetti_4.png`, `glow_soft.png`, `zzz.png`, `leaf_fall_w1.png`, `leaf_fall_w2.png`, `leaf_fall_w3.png`, `leaf_fall_w4.png`, `cloud_soft_1.png`, `cloud_soft_2.png`, `cloud_soft_3.png`, `firefly.png`, `leaf_wipe.png`, `gift_trail.png`, `house_glow.png`, `gate_light.png`.

### gift (4)

`gift_nest.png`, `gift_nest_front.png`, `gift_box_base.png`, `gift_box_lid.png`.

### gates (9)

`gate_arch.png`, `gate_door_left.png`, `gate_door_right.png`, `gate_padlock.png`, `gate_backdrop_w2.png`, `gate_backdrop_w3.png`, `gate_backdrop_w4.png`, `gate_locked.png`, `gate_open.png`.

## Integration contracts

- HUD: Scarecrow is `icon_scarecrow` over `btn_purple`; Minty's badge is `gift_watering_can`. Coin number frame `[142,32,140,60]` on 320x128 sprite. Numbers/text are runtime-rendered.
- Layout: 17 approved screens, 223 measured bounds, 40 actual map-node centers. K1..6 intermediate multi-house layouts are inferred, not observed approved screens.
- Houses: shared 512x512 frame; roof suspension `(256,58)`, door center `(253,306)`, left hinge `(145,306)`. Open fallback includes body, excludes roof. Preserve canvas padding and aspect ratio.
- Gifts: shared 768x768 frame. Draw nest/base/lid/nest_front. Lid pivot `(384,390)`; open translation `(0,-130)`.
- Gates: shared 1024x1024 frame. Opening xyxy `[240,250,768,854]`; hinges `(246,610)` and `(778,610)`. Gate light unchanged, placed in xywh `[192,215,640,640]`. Open fallback is bamboo; use separate backdrops for other worlds. Exact pivots/layers in `runtime/gates/delivery.json`.
- Map: 1080x3000 backgrounds with 160 px top alpha fade, 2840 px stacking stride. Topmost viewport requires local y>=160 or opaque-source backing. Current glow node is a full-node replacement.
- Panels: slice borders and rope anchors in `runtime/panels/delivery.json`; runtime stretching untested.

## Half done / next

No unfinished owned files remain. Final layout includes all 120 sprites and all assembly contracts; all current sprite SHA256 hashes match its inventory. Next: coordinator review/merge, then code/QA import and runtime verification. No further art work started.

## Blocker / verification limits

No external blocker. Static art QA is complete; Unity import, runtime layout, clipping, hit areas, additive materials and timed animation are NOT_RUN and belong to code/QA integration. Static open/closed frames do not prove animation. No Unity process or `.unity-lock` was created or modified by mac-art. No question for the lead.
