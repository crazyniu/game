# Visible text inventory

Source root: `PerchSort/Assets/Scripts/`. Reference root: `art/ui_design/ (branch ui-design-review) `. Art root: `PerchSort/Assets/Resources/Art/labels/`.

89 labels: 78 new sprites, 11 other-owner existing name sprites. All title canvases are 2040 × 360. Caption heights are three times the live layout height (or the closest corresponding reference layout); cream captions share a regular capsule silhouette with semicircular ends, uniform generated velvet material and a 66% lettering-height ratio. Original canvases are preserved. Import as single, full-rect sprites, centered pivot, preserve aspect ratio; these lettered plates are not stretchable 9-slice surfaces.

Location distinguishes live text from reference-only copy. Reference-only entries are supplied for the Unity worker; current code has removed many headings and cosmetic labels. Species and unknown-name sprites are owned by another job and were not written by this job. Concurrent owner updates were observed; see protected_changes.txt. Runtime numbers (including numeric punctuation and the `x` multiplier), GameObject names, inaccessible/internal IDs and icon-button accessible names are excluded. `BoosterEffect` and `GiftEffect` are unused helpers, not rendered. Shop catalog has cosmetic IDs/prices but no display names. World keys are untranslated internal IDs; current world signs are pictograms, so no invented world names are included. StuckHint and SleepBubble are icon-only. The fixed Zzz sprite is included.

Contact sheet order is this table, left to right then top to bottom. `review_*.jpg` use the same order in groups of 12, with checkerboard, dark and light columns. Native 2× oversampling was waived by the art lead due to the tool limit; see `resolution_check.txt` and `REPORT.md`.

| String | Where shown (file:line) | Plate type | Final file | Canvas (px) | Status |
|---|---|---|---|---|---|
| Zzz | Game/BirdView.cs:123 (zzz sprite) | small caption | label_small_zzz.png | 301 × 108 | exported; oversampling waived by art lead |
| Cheerful. Always first to sing. | Game/AviaryView.cs:12 | cream caption plate | label_caption_cheerful_always_first_to_sing.png | 1892 × 210 | exported; oversampling waived by art lead |
| Sleepy-eyed. Never in a hurry. | Game/AviaryView.cs:12 | cream caption plate | label_caption_sleepy_eyed_never_in_a_hurry.png | 1936 × 210 | exported; oversampling waived by art lead |
| Show-off. Loves an encore. | Game/AviaryView.cs:12 | cream caption plate | label_caption_show_off_loves_an_encore.png | 1957 × 210 | exported; oversampling waived by art lead |
| Gardener. Makes branches grow. | Game/AviaryView.cs:13 | cream caption plate | label_caption_gardener_makes_branches_grow.png | 2064 × 210 | exported; oversampling waived by art lead |
| Grumpy. Scares crows away. | Game/AviaryView.cs:13 | cream caption plate | label_caption_grumpy_scares_crows_away.png | 1824 × 210 | exported; oversampling waived by art lead |
| Light on her feet. Takes a shortcut. | Game/AviaryView.cs:13 | cream caption plate | label_caption_light_on_her_feet_takes_a_shortcut.png | 2271 × 210 | exported; oversampling waived by art lead |
| Hot-headed. Cracks eggs open. | Game/AviaryView.cs:14 | cream caption plate | label_caption_hot_headed_cracks_eggs_open.png | 2088 × 210 | exported; oversampling waived by art lead |
| Trouble in black feathers. | Game/AviaryView.cs:14 | cream caption plate | label_caption_trouble_in_black_feathers.png | 1866 × 210 | exported; oversampling waived by art lead |
| Night owl. Flips the sky. | Game/AviaryView.cs:14 | cream caption plate | label_caption_night_owl_flips_the_sky.png | 1797 × 210 | exported; oversampling waived by art lead |
| Dreamy. Calls the moon. | Game/AviaryView.cs:14 | cream caption plate | label_caption_dreamy_calls_the_moon.png | 1843 × 210 | exported; oversampling waived by art lead |
| Wakes every sleeping bird | Game/AviaryView.cs:18 | cream caption plate | label_caption_wakes_every_sleeping_bird.png | 2633 × 270 | exported; oversampling waived by art lead |
| Shows every egg's secret | Game/AviaryView.cs:18 | cream caption plate | label_caption_shows_every_egg_s_secret.png | 2534 × 270 | exported; oversampling waived by art lead |
| Next move is free | Game/AviaryView.cs:18 | cream caption plate | label_caption_next_move_is_free.png | 2195 × 270 | exported; oversampling waived by art lead |
| Branches grow one seat | Game/AviaryView.cs:18 | cream caption plate | label_caption_branches_grow_one_seat.png | 2611 × 270 | exported; oversampling waived by art lead |
| Scares nearby crows away | Game/AviaryView.cs:19 | cream caption plate | label_caption_scares_nearby_crows_away.png | 2806 × 270 | exported; oversampling waived by art lead |
| Next landing breaks the rules | Game/AviaryView.cs:19 | cream caption plate | label_caption_next_landing_breaks_the_rules.png | 3117 × 270 | exported; oversampling waived by art lead |
| Every egg hatches now | Game/AviaryView.cs:19 | cream caption plate | label_caption_every_egg_hatches_now.png | 2265 × 270 | exported; oversampling waived by art lead |
| Day and night swap | Game/AviaryView.cs:19 | cream caption plate | label_caption_day_and_night_swap.png | 2114 × 270 | exported; oversampling waived by art lead |
| Three moves of night | Game/AviaryView.cs:19 | cream caption plate | label_caption_three_moves_of_night.png | 2268 × 270 | exported; oversampling waived by art lead |
| Find a helpful move | Game/PanelsView.cs:413 | cream caption plate | label_caption_find_a_helpful_move.png | 1704 × 216 | exported; oversampling waived by art lead |
| Add one branch | Game/PanelsView.cs:414 | cream caption plate | label_caption_add_one_branch.png | 1797 × 216 | exported; oversampling waived by art lead |
| See inside every egg | Game/PanelsView.cs:415 | cream caption plate | label_caption_see_inside_every_egg.png | 1729 × 216 | exported; oversampling waived by art lead |
| Wake sleeping birds | Game/PanelsView.cs:416 | cream caption plate | label_caption_wake_sleeping_birds.png | 1757 × 216 | exported; oversampling waived by art lead |
| Send crows away | Game/PanelsView.cs:417 | cream caption plate | label_caption_send_crows_away.png | 1743 × 216 | exported; oversampling waived by art lead |
| Grow branches fully | Game/PanelsView.cs:418 | cream caption plate | label_caption_grow_branches_fully.png | 1657 × 216 | exported; oversampling waived by art lead |
| Wakes all sleepers | Game/PanelsView.cs:425 | cream caption plate | label_caption_wakes_all_sleepers.png | 1726 × 228 | exported; oversampling waived by art lead |
| Reveals every egg | Game/PanelsView.cs:426 | cream caption plate | label_caption_reveals_every_egg.png | 1698 × 228 | exported; oversampling waived by art lead |
| One free move | Game/PanelsView.cs:427 | cream caption plate | label_caption_one_free_move.png | 1457 × 228 | exported; oversampling waived by art lead |
| Grows branches | Game/PanelsView.cs:428 | cream caption plate | label_caption_grows_branches.png | 1613 × 228 | exported; oversampling waived by art lead |
| Sends crows away | Game/PanelsView.cs:429 | cream caption plate | label_caption_sends_crows_away.png | 1806 × 228 | exported; oversampling waived by art lead |
| Land anywhere | Game/PanelsView.cs:430 | cream caption plate | label_caption_land_anywhere.png | 1597 × 228 | exported; oversampling waived by art lead |
| Hatches every egg | Game/PanelsView.cs:431 | cream caption plate | label_caption_hatches_every_egg.png | 1767 × 228 | exported; oversampling waived by art lead |
| Switches day/night | Game/PanelsView.cs:432 | cream caption plate | label_caption_switches_day_night.png | 1734 × 228 | exported; oversampling waived by art lead |
| Moonlit moves | Game/PanelsView.cs:433 | cream caption plate | label_caption_moonlit_moves.png | 1466 × 228 | exported; oversampling waived by art lead |
| Never goes home | Game/PanelsView.cs:269 | cream caption plate | label_caption_never_goes_home.png | 1621 × 228 | exported; oversampling waived by art lead |
| Firefly | Game/PanelsView.cs:405 | name ribbon | label_booster_firefly.png | 1560 × 468 | exported; oversampling waived by art lead |
| Sapling | Game/PanelsView.cs:405 | name ribbon | label_booster_sapling.png | 1560 × 468 | exported; oversampling waived by art lead |
| Owl Eye | Game/PanelsView.cs:405 | name ribbon | label_booster_owl_eye.png | 1560 × 468 | exported; oversampling waived by art lead |
| Morning Bell | Game/PanelsView.cs:405 | name ribbon | label_booster_morning_bell.png | 1560 × 468 | exported; oversampling waived by art lead |
| Scarecrow | Game/PanelsView.cs:405 | name ribbon | label_booster_scarecrow.png | 1560 × 468 | exported; oversampling waived by art lead |
| Spring Rain | Game/PanelsView.cs:405 | name ribbon | label_booster_spring_rain.png | 1560 × 468 | exported; oversampling waived by art lead |
| Advance the day/night cycle | Game/GameRoot.cs:427 | small caption | label_small_advance_the_day_night_cycle.png | 1750 × 180 | exported; oversampling waived by art lead |
| Bring the flock together | Game/GameRoot.cs:427 | small caption | label_small_bring_the_flock_together.png | 1653 × 180 | exported; oversampling waived by art lead |
| Hatch, then gather | Game/GameRoot.cs:430 | small caption | label_small_hatch_then_gather.png | 1356 × 180 | exported; oversampling waived by art lead |
| Wake, then gather | Game/GameRoot.cs:431 | small caption | label_small_wake_then_gather.png | 1337 × 180 | exported; oversampling waived by art lead |
| Use the new seats | Game/GameRoot.cs:432 | small caption | label_small_use_the_new_seats.png | 1052 × 180 | exported; oversampling waived by art lead |
| Daily Gift | refs/daily_gift.png; Game/PanelsView.cs:231 (art/reference copy) | title ribbon | label_title_daily_gift.png | 2040 × 360 | exported; oversampling waived by art lead |
| Well done! | refs/board_win_panel.png; Game/WinPanel.cs:40 (art/reference copy) | title ribbon | label_title_well_done.png | 2040 × 360 | exported; oversampling waived by art lead |
| New friend | refs/new_friend.png; Game/PanelsView.cs:231 (art/reference copy) | title ribbon | label_title_new_friend.png | 2040 × 360 | exported; oversampling waived by art lead |
| World Complete | refs/world_complete.png; Game/PanelsView.cs:313 (art/reference copy) | title ribbon | label_title_world_complete.png | 2040 × 360 | exported; oversampling waived by art lead |
| World Unlocked | refs/world_unlock.png; Game/PanelsView.cs:231 (art/reference copy) | title ribbon | label_title_world_unlocked.png | 2040 × 360 | exported; oversampling waived by art lead |
| Aviary | refs/aviary_list.png; Game/AviaryView.cs:45 (art/reference copy) | title ribbon | label_title_aviary.png | 2040 × 360 | exported; oversampling waived by art lead |
| Settings | refs/settings.png (reference-only title; no current text renderer) | title ribbon | label_title_settings.png | 2040 × 360 | exported; oversampling waived by art lead |
| Forest Trail | refs/map.png; Game/MapView.cs:58 (art/reference copy) | title ribbon | label_title_forest_trail.png | 2040 × 360 | exported; oversampling waived by art lead |
| Hint | refs/shop_boosters.png; Game/ShopView.cs:133,209 (reference-only card copy) | name ribbon | label_card_hint.png | 1368 × 294 | exported; oversampling waived by art lead |
| Extra Branch | refs/shop_boosters.png; Game/ShopView.cs:133,209 (reference-only card copy) | name ribbon | label_card_extra_branch.png | 1368 × 294 | exported; oversampling waived by art lead |
| Reveal | refs/shop_boosters.png; Game/ShopView.cs:133,209 (reference-only card copy) | name ribbon | label_card_reveal.png | 1368 × 294 | exported; oversampling waived by art lead |
| Bell | refs/shop_boosters.png; Game/ShopView.cs:133,209 (reference-only card copy) | name ribbon | label_card_bell.png | 1368 × 294 | exported; oversampling waived by art lead |
| Shoo | refs/shop_boosters.png; Game/ShopView.cs:133,209 (reference-only card copy) | name ribbon | label_card_shoo.png | 1368 × 294 | exported; oversampling waived by art lead |
| Rain | refs/shop_boosters.png; Game/ShopView.cs:133,209 (reference-only card copy) | name ribbon | label_card_rain.png | 1368 × 294 | exported; oversampling waived by art lead |
| Mushroom | refs/shop_houses.png; Game/ShopView.cs:133,209 (reference-only card copy) | name ribbon | label_card_mushroom.png | 1368 × 294 | exported; oversampling waived by art lead |
| Teapot | refs/shop_houses.png; Game/ShopView.cs:133,209 (reference-only card copy) | name ribbon | label_card_teapot.png | 1368 × 294 | exported; oversampling waived by art lead |
| Lantern | refs/shop_houses.png; Game/ShopView.cs:133,209 (reference-only card copy) | name ribbon | label_card_lantern.png | 1368 × 294 | exported; oversampling waived by art lead |
| Acorn | refs/shop_houses.png; Game/ShopView.cs:133,209 (reference-only card copy) | name ribbon | label_card_acorn.png | 1368 × 294 | exported; oversampling waived by art lead |
| Straw Hat | refs/shop_outfits.png; Game/ShopView.cs:133,209 (reference-only card copy) | name ribbon | label_card_straw_hat.png | 1368 × 294 | exported; oversampling waived by art lead |
| Cozy Scarf | refs/shop_outfits.png; Game/ShopView.cs:133,209 (reference-only card copy) | name ribbon | label_card_cozy_scarf.png | 1368 × 294 | exported; oversampling waived by art lead |
| Round Glasses | refs/shop_outfits.png; Game/ShopView.cs:133,209 (reference-only card copy) | name ribbon | label_card_round_glasses.png | 1368 × 294 | exported; oversampling waived by art lead |
| Flower | refs/shop_outfits.png; Game/ShopView.cs:133,209 (reference-only card copy) | name ribbon | label_card_flower.png | 1368 × 294 | exported; oversampling waived by art lead |
| Best | refs/level_card.png (reference copy) | cream caption plate | label_caption_best.png | 532 × 180 | exported; oversampling waived by art lead |
| Sound | refs/settings.png (reference copy) | cream caption plate | label_caption_sound.png | 683 × 180 | exported; oversampling waived by art lead |
| Restart | refs/settings.png (reference copy) | cream caption plate | label_caption_restart.png | 811 × 180 | exported; oversampling waived by art lead |
| Home | refs/settings.png (reference copy) | cream caption plate | label_caption_home.png | 654 × 180 | exported; oversampling waived by art lead |
| Free move | refs/aviary_detail.png (reference copy) | cream caption plate | label_caption_free_move.png | 1511 × 270 | exported; oversampling waived by art lead |
| Wakes sleepers | refs/goal_info.png (reference copy) | cream caption plate | label_caption_wakes_sleepers.png | 1654 × 228 | exported; oversampling waived by art lead |
| Reveals eggs | refs/unlock_card.png (reference copy) | cream caption plate | label_caption_reveals_eggs.png | 1377 × 216 | exported; oversampling waived by art lead |
| Autumn Forest | refs/world_unlock.png (reference copy) | cream caption plate | label_caption_autumn_forest.png | 1699 × 228 | exported; oversampling waived by art lead |
| Green Forest | refs/level_card.png (reference copy) | cream caption plate | label_caption_green_forest.png | 1574 × 228 | exported; oversampling waived by art lead |
| Sunny | Meta/Aviary.cs:34; Game/AviaryView.cs:81,110; Game/PanelsView.cs:126 | name ribbon | label_name_sunny.png | 640 × 200 | other-owner existing; not written by this job |
| Skye | Meta/Aviary.cs:34; Game/AviaryView.cs:81,110; Game/PanelsView.cs:126 | name ribbon | label_name_sky.png | 640 × 200 | other-owner existing; not written by this job |
| Berry | Meta/Aviary.cs:35; Game/AviaryView.cs:81,110; Game/PanelsView.cs:126 | name ribbon | label_name_berry.png | 640 × 200 | other-owner existing; not written by this job |
| Minty | Meta/Aviary.cs:35; Game/AviaryView.cs:81,110; Game/PanelsView.cs:126 | name ribbon | label_name_mint.png | 640 × 200 | other-owner existing; not written by this job |
| Cocoa | Meta/Aviary.cs:36; Game/AviaryView.cs:81,110; Game/PanelsView.cs:126 | name ribbon | label_name_cocoa.png | 640 × 200 | other-owner existing; not written by this job |
| Flurry | Meta/Aviary.cs:36; Game/AviaryView.cs:81,110; Game/PanelsView.cs:126 | name ribbon | label_name_snow.png | 640 × 200 | other-owner existing; not written by this job |
| Ember | Meta/Aviary.cs:37; Game/AviaryView.cs:81,110; Game/PanelsView.cs:126 | name ribbon | label_name_ember.png | 640 × 200 | other-owner existing; not written by this job |
| Rook | Meta/Aviary.cs:37; Game/AviaryView.cs:81,110; Game/PanelsView.cs:126 | name ribbon | label_name_crow.png | 640 × 200 | other-owner existing; not written by this job |
| Inky | Meta/Aviary.cs:38; Game/AviaryView.cs:81,110; Game/PanelsView.cs:126 | name ribbon | label_name_ink.png | 640 × 200 | other-owner existing; not written by this job |
| Hoot | Meta/Aviary.cs:38; Game/AviaryView.cs:81,110; Game/PanelsView.cs:126 | name ribbon | label_name_hoot.png | 640 × 200 | other-owner existing; not written by this job |
| ? | Game/AviaryView.cs:78,81 | name ribbon | label_name_unknown.png | 640 × 200 | other-owner existing; not written by this job |


## Outfit ribbon additions

23 additional painted outfit labels (112 inventory entries including the original 89 above). Each PNG is RGBA on a 1368 × 294 canvas. The four existing outfit ribbons remain unchanged. No `.meta` files are supplied, per coordinator instruction; Unity and `GameArtImporter` own import settings.

[All 27 outfit ribbons](outfit-labels.jpg): the four existing references first (Straw Hat, Cozy Scarf, Round Glasses, Flower), followed by the rows below, left to right and top to bottom. New labels use Straw Hat as the common ribbon/letter/leaf scale reference. Source-resolution and Unity-verification limits are recorded in [the outfit label report](../outfit-labels/report.md).

| Outfit ID | Exact painted text | Final file | Canvas (px) | Review |
|---|---|---|---|---|
| outfit.sunny.2 | Blue Bow Tie | label_card_blue_bow_tie.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.sunny.3 | Little Crown | label_card_little_crown.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.sky.2 | Aviator Goggles | label_card_aviator_goggles.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.sky.3 | Tiny Top Hat | label_card_tiny_top_hat.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.berry.2 | Pink Bow | label_card_pink_bow.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.berry.3 | Sparkle Tiara | label_card_sparkle_tiara.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.mint.2 | Mushroom Cap | label_card_mushroom_cap.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.mint.3 | Acorn Cap | label_card_acorn_cap.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.cocoa.1 | Red Beanie | label_card_red_beanie.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.cocoa.2 | Plaid Bow Tie | label_card_plaid_bow_tie.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.cocoa.3 | Monocle | label_card_monocle.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.snow.1 | Earmuffs | label_card_earmuffs.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.snow.2 | Snowflake Clip | label_card_snowflake_clip.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.snow.3 | Ice Tiara | label_card_ice_tiara.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.ember.1 | Red Bandana | label_card_red_bandana.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.ember.2 | Flame Headband | label_card_flame_headband.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.ember.3 | Knight Helmet | label_card_knight_helmet.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.ink.1 | Star Nightcap | label_card_star_nightcap.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.ink.2 | Silver Glasses | label_card_silver_glasses.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.ink.3 | Little Star Hat | label_card_little_star_hat.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.hoot.1 | Graduation Cap | label_card_graduation_cap.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.hoot.2 | Reading Glasses | label_card_reading_glasses.png | 1368 × 294 | spelling and i-dots checked at 300% |
| outfit.hoot.3 | Wizard Cape | label_card_wizard_cape.png | 1368 × 294 | spelling and i-dots checked at 300% |
