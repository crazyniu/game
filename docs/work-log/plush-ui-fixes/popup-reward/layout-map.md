# Layout element mapping

Every original layout element is listed. `measure` entries resolve to actual capture objects; `external` entries belong to the board or shared frame; `baked-or-missing` entries have no independently positioned sprite in the permitted art.

## daily_gift

| Element | Reference rect (x, y, w, h) | Runtime object or retained feature | Scope |
| --- | --- | --- | --- |
| board | 0, 0, 1080, 1920 | board_dimmed | board; external |
| coin_pill | 38, 48, 302, 118 | coin_pill | board; external |
| hud_coin | 38, 49, 125, 115 | coin_leaf_gold | board; external |
| hud_240 | 174, 79, 129, 56 | glyph_brown_digits | board; external |
| moves_pill | 33, 166, 404, 113 | moves_stars_pill | board; external |
| footprints | 49, 199, 60, 46 | icon_bird_footprints | board; external |
| moves_7 | 115, 193, 46, 55 | glyph_brown_digits | board; external |
| moves_divider | 180, 184, 8, 78 | divider_brown | board; external |
| star_1 | 210, 171, 61, 53 | star_gold_plush | board; external |
| threshold_1 | 209, 228, 45, 34 | glyph_brown_digits | board; external |
| star_2 | 276, 171, 61, 53 | star_gold_plush | board; external |
| threshold_2 | 278, 228, 51, 34 | glyph_brown_digits | board; external |
| star_3 | 345, 171, 61, 53 | star_gold_plush | board; external |
| threshold_3 | 349, 228, 58, 34 | glyph_infinity | board; external |
| sun_pill | 33, 280, 206, 97 | sun_counter_pill | board; external |
| sun | 49, 286, 100, 82 | icon_sun_gold | board; external |
| sun_4 | 156, 300, 44, 51 | glyph_brown_digits | board; external |
| settings | 952, 77, 113, 109 | btn_orange_settings | board; external |
| gear | 982, 99, 57, 61 | icon_gear_cream | board; external |
| rope | 548, 0, 52, 180 | goal_house_rope | board; external |
| goal_house | 419, 172, 318, 231 | goal_house_plush | board; external |
| house_roof | 419, 173, 318, 135 | house_roof_teal | board; external |
| house_opening | 535, 271, 82, 86 | house_opening | board; external |
| goal_ring_yellow | 370, 400, 99, 99 | portrait_ring_gold | board; external |
| goal_bird_yellow | 388, 412, 63, 74 | bird_yellow_front | board; external |
| goal_badge_yellow | 429, 454, 57, 53 | badge_gift_gold | board; external |
| goal_gift_yellow | 441, 467, 35, 33 | icon_gift_purple | board; external |
| goal_ring_blue | 511, 400, 100, 99 | portrait_ring_gold | board; external |
| goal_bird_blue | 528, 407, 67, 81 | bird_blue_front | board; external |
| goal_badge_blue | 571, 454, 56, 53 | badge_gift_gold | board; external |
| goal_gift_blue | 583, 467, 34, 33 | icon_gift_purple | board; external |
| goal_ring_owl | 650, 400, 100, 100 | portrait_ring_gold | board; external |
| goal_bird_owl | 667, 412, 67, 75 | bird_owl_front | board; external |
| goal_badge_owl | 711, 454, 56, 53 | badge_gift_gold | board; external |
| goal_gift_owl | 723, 467, 34, 33 | icon_gift_purple | board; external |
| bird_left_upper_yellow | 59, 723, 52, 102 | bird_yellow_front | board; external |
| bird_right_upper_yellow | 968, 831, 82, 137 | bird_yellow_front | board; external |
| bird_left_lower_yellow | 59, 1051, 52, 103 | bird_yellow_front | board; external |
| bird_right_lower_yellow | 968, 1157, 82, 137 | bird_yellow_front | board; external |
| tools_bar | 196, 1727, 729, 150 | toolbar_cream_plush | board; external |
| tool_undo | 246, 1748, 112, 107 | btn_orange_undo | board; external |
| icon_undo | 269, 1770, 66, 63 | icon_undo | board; external |
| tool_hint | 487, 1748, 111, 106 | btn_orange_hint | board; external |
| icon_hint | 510, 1763, 66, 74 | icon_hint | board; external |
| tool_branch | 726, 1748, 112, 106 | btn_orange_branch | board; external |
| icon_branch | 744, 1769, 80, 65 | icon_branch_plus | board; external |
| modal_panel | 110, 626, 860, 720 | Panel/Panel surface | popup-frame; external |
| modal_frame | 110, 626, 860, 720 | Panel/Panel surface | popup-frame; external |
| cream_inset | 145, 660, 789, 652 | Panel/Panel surface | popup-frame; external |
| ribbon | 198, 548, 689, 158 | Panel/Title ribbon | popup-frame; external |
| leaves_left | 198, 574, 112, 115 | Panel/Title ribbon | popup-frame; external |
| leaves_right | 772, 574, 115, 115 | Panel/Title ribbon | popup-frame; external |
| ribbon_face | 295, 552, 478, 123 | Panel/Title ribbon | popup-frame; external |
| daily_title | 353, 569, 372, 79 | Panel/Title ribbon | popup-frame; external |
| gift_hero | 229, 710, 622, 453 | gift_box, gift_bow, gift_nest, gift_nest_front, gift_coin_*, hero_leaves_*, hero_leaf_front | popup-reward; measure |
| gift_box | 361, 838, 348, 186 | gift_box | popup-reward; measure |
| gift_bow | 379, 711, 364, 227 | gift_bow | popup-reward; measure |
| gift_nest | 244, 994, 591, 169 | gift_nest | popup-reward; measure |
| gift_coin_left | 331, 963, 135, 128 | gift_coin_left | popup-reward; measure |
| gift_coin_middle | 456, 958, 174, 145 | gift_coin_middle | popup-reward; measure |
| gift_coin_right | 628, 975, 125, 90 | gift_coin_right | popup-reward; measure |
| gift_coin_back_left | 299, 912, 74, 92 | gift_coin_back_left | popup-reward; measure |
| gift_coin_back_right | 698, 925, 76, 88 | gift_coin_back_right | popup-reward; measure |
| hero_leaves_left | 230, 925, 109, 110 | hero_leaves_left | popup-reward; measure |
| hero_leaves_right | 741, 964, 107, 88 | hero_leaves_right | popup-reward; measure |
| hero_leaf_front | 304, 1072, 121, 59 | hero_leaf_front | popup-reward; measure |
| spark_1 | 318, 761, 45, 47 | spark_1 | popup-reward; measure |
| spark_2 | 266, 835, 43, 52 | spark_2 | popup-reward; measure |
| spark_3 | 714, 727, 40, 52 | spark_3 | popup-reward; measure |
| spark_4 | 770, 834, 44, 51 | spark_4 | popup-reward; measure |
| reward_pill | 327, 1175, 424, 116 | Reward/* | popup-reward; measure |
| reward_coin | 327, 1175, 124, 116 | Reward/Reward coin | popup-reward; measure |
| reward_leaf | 354, 1197, 74, 68 | icon_leaf_gold_relief | popup-reward; baked-or-missing |
| reward_50 | 502, 1199, 166, 68 | Reward/Reward amount/* | popup-reward; measure |
| accept | 477, 1391, 127, 129 | @action | popup-reward; measure |
| check | 506, 1423, 69, 61 | @action/Icon | popup-reward; measure |

## new_friend

| Element | Reference rect (x, y, w, h) | Runtime object or retained feature | Scope |
| --- | --- | --- | --- |
| board | 0, 0, 1080, 1920 | board_dimmed | board; external |
| coins | 37, 49, 305, 119 | coin_pill | board; external |
| coin | 38, 50, 115, 115 | coin_leaf_plush | board; external |
| coin_leaf | 65, 75, 62, 64 | glyph_leaf | board; external |
| coins_num | 172, 79, 134, 54 | glyph_brown_digits | board; external |
| moves | 33, 165, 406, 115 | moves_stars_pill | board; external |
| footprints | 47, 199, 68, 44 | bird_footprints | board; external |
| moves_num | 122, 193, 40, 59 | glyph_brown_digits | board; external |
| move_rule | 177, 183, 7, 80 | separator_brown | board; external |
| star1 | 205, 174, 55, 49 | star_gold_plush | board; external |
| threshold1 | 210, 228, 45, 34 | glyph_brown_digits | board; external |
| star2 | 278, 174, 55, 49 | star_gold_plush | board; external |
| threshold2 | 280, 228, 50, 34 | glyph_brown_digits | board; external |
| star3 | 352, 174, 55, 49 | star_gold_plush | board; external |
| threshold3 | 350, 228, 55, 34 | glyph_brown_infinity | board; external |
| sun_pill | 35, 279, 205, 100 | sun_counter_pill | board; external |
| sun | 55, 290, 80, 74 | icon_sun_gold | board; external |
| sun_num | 153, 302, 43, 51 | glyph_brown_digits | board; external |
| settings | 952, 78, 117, 106 | btn_orange_settings | board; external |
| gear | 981, 101, 57, 58 | icon_gear_white | board; external |
| rope | 559, 0, 40, 181 | goal_house_rope | board; external |
| house | 416, 173, 324, 230 | goal_house_plush | board; external |
| house_roof | 416, 173, 324, 145 | goal_house_teal_roof | board; external |
| house_hole | 536, 277, 84, 80 | goal_house_entrance | board; external |
| goal_ring1 | 371, 399, 103, 98 | portrait_ring_gold | board; external |
| goal_bird1 | 383, 409, 78, 78 | bird_yellow | board; external |
| gift_badge1 | 429, 457, 57, 50 | badge_gift_purple | board; external |
| gift_icon1 | 441, 466, 33, 35 | icon_gift | board; external |
| goal_ring2 | 510, 400, 105, 98 | portrait_ring_gold | board; external |
| goal_bird2 | 523, 411, 78, 79 | bird_blue | board; external |
| gift_badge2 | 570, 457, 56, 51 | badge_gift_purple | board; external |
| gift_icon2 | 582, 466, 32, 36 | icon_gift | board; external |
| goal_ring3 | 650, 399, 103, 99 | portrait_ring_gold | board; external |
| goal_bird3 | 663, 407, 78, 84 | bird_wizard | board; external |
| gift_badge3 | 710, 457, 56, 50 | badge_gift_purple | board; external |
| gift_icon3 | 722, 466, 32, 35 | icon_gift | board; external |
| left_bird1 | 58, 716, 57, 106 | bird_yellow | board; external |
| right_bird2 | 967, 831, 82, 133 | bird_yellow | board; external |
| left_bird3 | 58, 1049, 57, 105 | bird_yellow | board; external |
| right_bird4 | 967, 1158, 82, 135 | bird_yellow | board; external |
| tools | 196, 1728, 728, 147 | toolbar_cream_plush | board; external |
| undo | 248, 1747, 103, 104 | btn_orange | board; external |
| undo_icon | 271, 1767, 61, 62 | icon_undo | board; external |
| hint | 488, 1747, 103, 104 | btn_orange | board; external |
| bulb_icon | 514, 1765, 51, 67 | icon_hint_bulb | board; external |
| branch | 729, 1747, 103, 104 | btn_orange | board; external |
| branch_icon | 744, 1768, 74, 58 | icon_branch_plus | board; external |
| panel | 111, 620, 859, 719 | Panel/Panel surface | popup-frame; external |
| frame | 111, 620, 859, 719 | Panel/Panel surface | popup-frame; external |
| cushion | 140, 648, 800, 664 | Panel/Panel surface | popup-frame; external |
| title_rib | 219, 551, 643, 138 | Panel/Title ribbon | popup-frame; external |
| title_leaf_l | 218, 575, 110, 91 | Panel/Title ribbon | popup-frame; external |
| title_leaf_r | 751, 576, 110, 91 | Panel/Title ribbon | popup-frame; external |
| title_text | 348, 570, 378, 77 | Panel/Title ribbon | popup-frame; external |
| minty | 379, 729, 318, 360 | Bird * | popup-reward; measure |
| name_rib | 307, 1089, 466, 104 | Species name | popup-reward; measure |
| name_leaf_l | 307, 1092, 69, 96 | name_leaf_l | popup-reward; measure |
| name_leaf_r | 704, 1092, 69, 96 | name_leaf_r | popup-reward; measure |
| name_text | 440, 1114, 209, 69 | label_minty | popup-reward; baked-or-missing |
| reward | 380, 1208, 320, 88 | Reward/* | popup-reward; measure |
| reward_coin | 382, 1210, 95, 85 | Reward/Reward coin | popup-reward; measure |
| reward_leaf | 404, 1227, 53, 51 | glyph_leaf_gold | popup-reward; baked-or-missing |
| reward_num | 519, 1227, 123, 50 | Reward/Reward amount/* | popup-reward; measure |
| accept | 478, 1388, 124, 124 | @action | popup-reward; measure |
| check | 505, 1417, 72, 66 | @action/Icon | popup-reward; measure |

## unlock_card

| Element | Reference rect (x, y, w, h) | Runtime object or retained feature | Scope |
| --- | --- | --- | --- |
| board | 0, 0, 1080, 1920 | board_dimmed | board; external |
| coin_pill | 79, 59, 262, 101 | coin_pill | board; external |
| moves_pill | 33, 166, 407, 113 | moves_stars_pill | board; external |
| sun_pill | 35, 281, 204, 99 | sun_counter_pill | board; external |
| settings | 952, 76, 113, 109 | btn_orange_settings | board; external |
| rope | 554, 0, 51, 180 | goal_house_rope | board; external |
| coin | 38, 50, 112, 120 | coin_leaf_plush | board; external |
| moves_div | 179, 181, 8, 80 | pill_divider_brown | board; external |
| sun | 53, 289, 78, 77 | icon_sun_gold | board; external |
| house | 417, 172, 325, 229 | goal_house_plush | board; external |
| coin_leaf | 64, 76, 60, 66 | icon_leaf | board; external |
| coins | 173, 79, 130, 53 | glyph_brown_240 | board; external |
| feet | 50, 201, 63, 43 | icon_bird_footprints | board; external |
| moves | 120, 194, 44, 52 | glyph_brown_7 | board; external |
| star1 | 207, 175, 53, 49 | star_gold_plush | board; external |
| star2 | 280, 174, 56, 49 | star_gold_plush | board; external |
| star3 | 355, 175, 53, 49 | star_gold_plush | board; external |
| n12 | 207, 229, 49, 33 | glyph_brown_12 | board; external |
| n16 | 282, 229, 50, 33 | glyph_brown_16 | board; external |
| ninf | 355, 232, 54, 28 | glyph_brown_∞ | board; external |
| sun_n | 151, 303, 49, 50 | glyph_brown_4 | board; external |
| gear | 983, 99, 54, 59 | icon_gear_cream | board; external |
| roof | 417, 172, 325, 146 | goal_house_roof_teal | board; external |
| house_hole | 531, 269, 93, 85 | goal_house_dark_opening | board; external |
| goal_y_ring | 371, 401, 103, 96 | portrait_ring_gold | board; external |
| goal_b_ring | 509, 401, 104, 96 | portrait_ring_gold | board; external |
| goal_o_ring | 648, 400, 103, 99 | portrait_ring_gold | board; external |
| tools | 195, 1727, 728, 145 | toolbar_cream_plush | board; external |
| goal_y | 382, 413, 78, 76 | bird_yellow | board; external |
| goal_b | 523, 411, 79, 79 | bird_blue | board; external |
| goal_o | 660, 413, 78, 77 | bird_owl_wizard | board; external |
| undo | 247, 1750, 108, 102 | btn_orange_undo | board; external |
| hint | 487, 1750, 108, 102 | btn_orange_bulb | board; external |
| branch | 728, 1750, 108, 103 | btn_orange_branch_plus | board; external |
| goal_y_badge | 429, 454, 55, 52 | badge_gift_gold | board; external |
| goal_b_badge | 572, 454, 56, 53 | badge_gift_gold | board; external |
| goal_o_badge | 710, 455, 55, 52 | badge_gift_gold | board; external |
| undo_icon | 270, 1770, 57, 56 | icon_undo | board; external |
| hint_icon | 518, 1762, 45, 69 | icon_bulb | board; external |
| branch_icon | 747, 1770, 72, 53 | icon_branch_plus | board; external |
| goal_y_gift | 439, 464, 36, 33 | icon_gift_magenta | board; external |
| goal_b_gift | 584, 464, 34, 34 | icon_gift_magenta | board; external |
| goal_o_gift | 721, 465, 35, 33 | icon_gift_magenta | board; external |
| bird_l1 | 59, 723, 52, 102 | bird_yellow | board; external |
| bird_r1 | 968, 831, 81, 136 | bird_yellow | board; external |
| bird_l2 | 59, 1053, 53, 101 | bird_yellow | board; external |
| bird_r2 | 968, 1157, 82, 137 | bird_yellow | board; external |
| leaf_below | 626, 1324, 35, 12 | board_branch_leaf | board; external |
| modal | 109, 545, 861, 952 | Panel/Panel surface | popup-frame; external |
| panel | 109, 599, 861, 725 | Panel/Panel surface | popup-frame; external |
| frame | 109, 599, 861, 725 | Panel/Panel surface | popup-frame; external |
| cream | 151, 634, 779, 655 | Panel/Panel surface | popup-frame; external |
| halo | 249, 673, 584, 489 | halo | popup-reward; measure |
| eye | 388, 684, 303, 162 | eye/Illustration crop | popup-reward; measure |
| eye_iris | 458, 710, 129, 108 | owl_eye_iris_brown | popup-reward; baked-or-missing |
| spark_l1 | 302, 775, 69, 70 | spark_l1 | popup-reward; measure |
| spark_r1 | 717, 799, 66, 70 | spark_r1 | popup-reward; measure |
| spark_l2 | 330, 894, 58, 55 | spark_l2 | popup-reward; measure |
| spark_r2 | 695, 902, 58, 58 | spark_r2 | popup-reward; measure |
| nest_leaf_l | 272, 977, 139, 117 | nest_leaf_l | popup-reward; measure |
| nest_leaf_r | 666, 977, 134, 118 | nest_leaf_r | popup-reward; measure |
| egg | 398, 853, 285, 292 | egg/Illustration crop | popup-reward; measure |
| nest | 313, 1029, 453, 136 | nest | popup-reward; measure |
| caption_plate | 222, 1165, 632, 105 | Booster caption | popup-reward; measure |
| caption | 313, 1182, 461, 66 | label_reveals_eggs | popup-reward; baked-or-missing |
| ribbon | 302, 545, 478, 118 | Panel/Title ribbon | popup-frame; external |
| leaf_l | 242, 568, 137, 99 | Panel/Title ribbon | popup-frame; external |
| leaf_r | 716, 568, 122, 100 | Panel/Title ribbon | popup-frame; external |
| title | 372, 558, 330, 84 | Panel/Title ribbon | popup-frame; external |
| accept | 478, 1373, 124, 124 | @action | popup-reward; measure |
| check | 505, 1406, 71, 61 | @action/Icon | popup-reward; measure |

## world_complete

| Element | Reference rect (x, y, w, h) | Runtime object or retained feature | Scope |
| --- | --- | --- | --- |
| board | 0, 0, 1080, 1920 | board_dimmed | board; external |
| coins | 36, 49, 308, 118 | coin_pill | board; external |
| coin | 37, 49, 129, 118 | coin_leaf_plush | board; external |
| coin_leaf | 72, 77, 63, 61 | icon_leaf | board; external |
| coin_value | 173, 78, 128, 56 | glyph_brown_digits | board; external |
| moves | 31, 166, 408, 115 | moves_pill | board; external |
| feet | 50, 200, 63, 44 | icon_bird_footprints | board; external |
| move_value | 119, 195, 43, 52 | glyph_brown_digits | board; external |
| hud_star1 | 205, 174, 55, 50 | star_gold_plush | board; external |
| threshold1 | 210, 229, 44, 34 | glyph_brown_digits | board; external |
| hud_star2 | 281, 174, 55, 50 | star_gold_plush | board; external |
| threshold2 | 285, 229, 45, 34 | glyph_brown_digits | board; external |
| hud_star3 | 356, 174, 55, 50 | star_gold_plush | board; external |
| threshold3 | 355, 229, 49, 34 | glyph_brown_digits | board; external |
| sun_pill | 34, 283, 205, 96 | sun_counter_pill | board; external |
| sun | 53, 289, 91, 78 | icon_sun | board; external |
| sun_value | 157, 302, 41, 49 | glyph_brown_digits | board; external |
| settings | 953, 79, 111, 105 | btn_orange | board; external |
| gear | 979, 98, 63, 65 | icon_settings | board; external |
| rope | 551, 0, 47, 178 | goal_house_rope | board; external |
| house | 417, 170, 324, 231 | goal_house | board; external |
| house_hole | 541, 276, 92, 77 | goal_house_opening | board; external |
| bg_yellow_l1 | 60, 724, 51, 101 | bird_yellow | board; external |
| bg_yellow_l2 | 61, 1057, 50, 91 | bird_yellow | board; external |
| bg_yellow_r1 | 969, 833, 82, 135 | bird_yellow | board; external |
| bg_yellow_r2 | 924, 1158, 127, 132 | bird_yellow | board; external |
| bg_yellow_lower | 677, 1257, 121, 38 | bird_yellow | board; external |
| bg_blue_lower | 802, 1257, 117, 38 | bird_blue | board; external |
| tools | 195, 1728, 729, 146 | toolbar_cream_plush | board; external |
| undo_disc | 246, 1750, 109, 105 | btn_orange | board; external |
| undo_glyph | 267, 1770, 65, 58 | icon_undo | board; external |
| hint_disc | 488, 1750, 109, 105 | btn_orange | board; external |
| hint_glyph | 513, 1764, 55, 71 | icon_bulb | board; external |
| branch_disc | 746, 1750, 109, 105 | btn_orange | board; external |
| branch_glyph | 763, 1770, 76, 57 | icon_branch_plus | board; external |
| panel | 110, 462, 860, 798 | Panel/Panel surface | popup-frame; external |
| frame | 110, 462, 860, 798 | Panel/Panel surface | popup-frame; external |
| cream | 144, 494, 792, 725 | Panel/Panel surface | popup-frame; external |
| ribbon | 202, 394, 675, 138 | Panel/Title ribbon | popup-frame; external |
| ribbon_left | 204, 406, 86, 87 | Panel/Title ribbon | popup-frame; external |
| ribbon_right | 797, 406, 81, 84 | Panel/Title ribbon | popup-frame; external |
| title | 277, 414, 522, 73 | Panel/Title ribbon | popup-frame; external |
| star1 | 255, 543, 152, 134 | star1 | popup-reward; measure |
| star2 | 466, 543, 152, 134 | star2 | popup-reward; measure |
| star3 | 677, 543, 152, 134 | star3 | popup-reward; measure |
| arch | 162, 691, 760, 409 | arch, perch, foliage_* | popup-reward; measure |
| flower_top | 499, 692, 90, 89 | flower_cream | popup-reward; baked-or-missing |
| flower_left | 197, 1018, 74, 72 | flower_cream | popup-reward; baked-or-missing |
| flower_right | 816, 1017, 72, 75 | flower_cream | popup-reward; baked-or-missing |
| yellow | 244, 813, 212, 239 | Bird sunny | popup-reward; measure |
| blue | 441, 817, 209, 236 | Bird sky | popup-reward; measure |
| pink | 632, 825, 220, 226 | Bird berry | popup-reward; measure |
| reward | 335, 1101, 407, 102 | Reward/* | popup-reward; measure |
| reward_coin | 347, 1102, 110, 98 | Reward/Reward coin | popup-reward; measure |
| reward_leaf | 375, 1121, 57, 57 | icon_leaf | popup-reward; baked-or-missing |
| reward_value | 505, 1119, 171, 58 | Reward/Reward amount/* | popup-reward; measure |
| accept | 478, 1308, 124, 124 | @action | popup-reward; measure |
| check | 504, 1340, 72, 64 | @action/Icon | popup-reward; measure |

## world_unlock

| Element | Reference rect (x, y, w, h) | Runtime object or retained feature | Scope |
| --- | --- | --- | --- |
| board | 0, 0, 1080, 1920 | board_dimmed | board; external |
| coin_pill | 37, 50, 304, 119 | coin_pill | board; external |
| coin | 38, 51, 124, 117 | coin_leaf_plush | board; external |
| coin_leaf | 65, 79, 67, 58 | icon_leaf | board; external |
| coins | 174, 79, 129, 54 | glyph_brown_digits | board; external |
| moves_pill | 33, 165, 407, 114 | moves_pill | board; external |
| bird_feet | 51, 199, 63, 43 | icon_bird_footprints | board; external |
| moves | 122, 195, 41, 53 | glyph_brown_digits | board; external |
| moves_divider | 180, 182, 7, 80 | moves_divider | board; external |
| star_1 | 205, 173, 54, 49 | star_gold_plush | board; external |
| threshold_1 | 208, 228, 50, 33 | glyph_brown_digits | board; external |
| star_2 | 279, 173, 53, 49 | star_gold_plush | board; external |
| threshold_2 | 282, 228, 49, 33 | glyph_brown_digits | board; external |
| star_3 | 353, 173, 54, 49 | star_gold_plush | board; external |
| threshold_3 | 356, 228, 50, 33 | glyph_brown_digits | board; external |
| sun_pill | 35, 281, 202, 97 | sun_pill | board; external |
| sun | 53, 287, 94, 80 | icon_sun | board; external |
| sun_count | 155, 301, 47, 51 | glyph_brown_digits | board; external |
| settings | 951, 75, 116, 112 | btn_orange | board; external |
| gear | 981, 100, 64, 61 | icon_gear | board; external |
| goal_rope | 556, 0, 52, 180 | goal_house_rope | board; external |
| goal_house | 415, 172, 324, 228 | goal_house | board; external |
| goal_roof | 415, 172, 324, 145 | goal_house_roof | board; external |
| goal_hole | 529, 266, 104, 88 | goal_house_opening | board; external |
| bird_left_upper | 59, 721, 51, 104 | bird_yellow | board; external |
| bird_right_upper | 970, 830, 82, 137 | bird_yellow | board; external |
| bird_left_lower | 59, 1047, 51, 106 | bird_yellow | board; external |
| bird_right_lower_a | 680, 1262, 116, 31 | bird_yellow | board; external |
| bird_right_lower_b | 802, 1262, 116, 31 | bird_blue | board; external |
| bird_right_lower_c | 934, 1156, 120, 137 | bird_yellow | board; external |
| tools_bar | 194, 1728, 729, 146 | bar_padded_cream | board; external |
| undo | 246, 1750, 116, 105 | btn_orange | board; external |
| undo_glyph | 271, 1775, 65, 48 | icon_undo | board; external |
| hint | 484, 1750, 117, 105 | btn_orange | board; external |
| hint_glyph | 512, 1765, 63, 69 | icon_bulb | board; external |
| branch | 727, 1750, 117, 105 | btn_orange | board; external |
| branch_glyph | 747, 1770, 77, 63 | icon_branch_plus | board; external |
| modal | 109, 463, 862, 800 | Panel/Panel surface | popup-frame; external |
| panel_inner | 132, 489, 814, 747 | Panel/Panel surface | popup-frame; external |
| ribbon | 163, 400, 754, 136 | Panel/Title ribbon | popup-frame; external |
| ribbon_body | 282, 400, 513, 135 | Panel/Title ribbon | popup-frame; external |
| leaves_left | 163, 426, 133, 109 | Panel/Title ribbon | popup-frame; external |
| leaves_right | 789, 426, 128, 107 | Panel/Title ribbon | popup-frame; external |
| title | 301, 418, 478, 65 | Panel/Title ribbon | popup-frame; external |
| portal | 172, 541, 729, 559 | portal_arch, portal_forest, Forest opening/portal_forest, door_left, door_right, portal_leaves_*, portal_crown | popup-reward; measure |
| portal_arch | 224, 566, 632, 534 | portal_arch | popup-reward; measure |
| portal_forest | 327, 716, 429, 374 | portal_forest, Forest opening/portal_forest | popup-reward; measure |
| door_left | 174, 711, 166, 379 | door_left | popup-reward; measure |
| door_right | 739, 710, 161, 380 | door_right | popup-reward; measure |
| knob_left | 212, 883, 51, 60 | door_knob_gold | popup-reward; baked-or-missing |
| knob_right | 811, 884, 53, 57 | door_knob_gold | popup-reward; baked-or-missing |
| portal_crown | 399, 598, 279, 134 | portal_crown | popup-reward; measure |
| maple_leaf | 475, 613, 139, 91 | maple_leaf_red | popup-reward; baked-or-missing |
| autumn_leaves_top | 228, 541, 627, 198 | portal_leaves_* | popup-reward; measure |
| autumn_leaves_bottom | 259, 1049, 553, 52 | portal_leaf_scatter | popup-reward; baked-or-missing |
| caption_plate | 277, 1115, 532, 91 | World caption | popup-reward; measure |
| caption | 336, 1132, 413, 53 | label_autumn_forest | popup-reward; baked-or-missing |
| play | 476, 1308, 128, 127 | @action | popup-reward; measure |
| play_glyph | 518, 1337, 54, 64 | @action/Icon | popup-reward; measure |

