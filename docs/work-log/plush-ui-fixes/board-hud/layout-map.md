# Board HUD element map

All rectangles are absolute top-left pixels on 1080 x 1920. Runtime anchors are top-left; anchoredPosition is (rect.center.x, -rect.center.y) relative to the parent. Image rectangles compensate existing transparent canvas padding.

## board_base

| JSON element | Measured rect | Runtime object / scope |
| --- | --- | --- |
| forest | [0, 0, 1080, 1920] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_1 | [0, 787, 532, 133] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_2 | [578, 925, 502, 138] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_3 | [0, 1117, 533, 134] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_4 | [580, 1254, 500, 137] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_r1_yellow | [57, 688, 135, 139] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_r1_blue | [184, 693, 133, 134] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_r1_owl | [311, 707, 130, 121] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_r2_yellow_l | [665, 829, 138, 139] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_r2_blue | [792, 834, 132, 134] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_r2_yellow_r | [918, 832, 136, 136] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_r3_yellow_l | [57, 1015, 135, 143] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_r3_blue | [185, 1021, 132, 138] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_r3_yellow_r | [311, 1021, 134, 138] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_r4_yellow_l | [666, 1155, 137, 140] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_r4_blue | [793, 1162, 132, 134] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_r4_yellow_r | [918, 1158, 136, 138] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| goal_rope | [548, 0, 59, 184] | Species goals/Single house rope/Rope artwork |
| goal_house | [413, 170, 329, 232] | Species goals/Single goal house/Birdhouse/House art crop/House body |
| house_opening | [533, 256, 96, 99] | Baked into existing sprite; no independent RectTransform |
| coin_pill | [93, 59, 248, 105] | Board controls/Coin pill |
| move_pill | [32, 164, 406, 116] | Board controls/Move pill |
| sun_pill | [34, 281, 206, 97] | Board controls/Day night counter/Phase pill |
| settings | [953, 77, 114, 108] | Board controls/Settings |
| goal_ring_yellow | [370, 401, 102, 96] | Species goals/Single goal house/sunny goal info/Gold ring |
| goal_ring_blue | [509, 400, 103, 98] | Species goals/Single goal house/sky goal info/Gold ring |
| goal_ring_owl | [649, 399, 103, 100] | Species goals/Single goal house/hoot goal info/Gold ring |
| coin_disc | [37, 49, 132, 121] | Board controls/Coin disc |
| move_divider | [180, 182, 7, 79] | Board controls/Move divider/Painted divider |
| sun_icon | [55, 289, 86, 77] | Board controls/Day night counter/Sun moon |
| settings_gear | [978, 98, 66, 64] | Board controls/Settings/Icon |
| goal_bird_yellow | [384, 410, 78, 74] | Species goals/Single goal house/sunny goal info/Portrait crop/Front portrait |
| goal_bird_blue | [521, 409, 81, 77] | Species goals/Single goal house/sky goal info/Portrait crop/Front portrait |
| goal_bird_owl | [661, 408, 80, 77] | Species goals/Single goal house/hoot goal info/Portrait crop/Front portrait |
| coin_leaf | [72, 75, 59, 64] | Baked into existing sprite; no independent RectTransform |
| coin_value | [175, 80, 127, 54] | Board controls/Coins * |
| footprints | [49, 201, 68, 44] | Board controls/Painted footprints |
| move_value | [122, 195, 41, 50] | Board controls/Counted moves * |
| move_star_1 | [206, 173, 54, 51] | Board controls/Reachable star 0 |
| threshold_1 | [209, 229, 46, 33] | Board controls/Star threshold 0 |
| move_star_2 | [280, 173, 54, 51] | Board controls/Reachable star 1 |
| threshold_2 | [282, 229, 47, 33] | Board controls/Star threshold 1 |
| move_star_3 | [354, 173, 54, 51] | Board controls/Reachable star 2 |
| threshold_3 | [355, 229, 51, 33] | Board controls/Star threshold 2 |
| sun_value | [157, 302, 43, 49] | Board controls/Day night counter/Moves until phase |
| goal_badge_yellow | [429, 455, 58, 52] | Species goals/Single goal house/sunny goal info/Species gift badge |
| goal_badge_blue | [572, 456, 57, 52] | Species goals/Single goal house/sky goal info/Species gift badge |
| goal_badge_owl | [710, 456, 57, 52] | Species goals/Single goal house/hoot goal info/Species gift badge |
| goal_gift_yellow | [439, 464, 38, 39] | Baked into existing sprite; no independent RectTransform |
| goal_gift_blue | [582, 465, 37, 39] | Baked into existing sprite; no independent RectTransform |
| goal_gift_owl | [720, 465, 37, 39] | Baked into existing sprite; no independent RectTransform |
| tools_bar | [194, 1727, 731, 148] | Board controls/Padded cream bar |
| tool_undo | [246, 1746, 110, 108] | Board controls/Undo |
| tool_bulb | [486, 1746, 110, 108] | Board controls/Firefly |
| tool_branch | [726, 1746, 110, 108] | Board controls/Sapling |
| tool_icon_undo | [271, 1769, 58, 58] | Board controls/Undo/Icon |
| tool_icon_bulb | [502, 1760, 74, 75] | Board controls/Firefly/Icon |
| tool_icon_branch | [743, 1769, 76, 64] | Board controls/Sapling/Icon |

## board_stuck_hint

| JSON element | Measured rect | Runtime object / scope |
| --- | --- | --- |
| forest | [0, 0, 1080, 1920] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_1 | [0, 788, 531, 121] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_2 | [579, 928, 501, 123] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_3 | [0, 1120, 533, 125] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_4 | [578, 1255, 502, 122] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| goal_rope | [550, 0, 55, 181] | Species goals/Single house rope/Rope artwork |
| goal_house | [415, 173, 328, 225] | Species goals/Single goal house/Birdhouse/House art crop/House body |
| house_body | [448, 250, 271, 146] | Baked into existing sprite; no independent RectTransform |
| house_hole | [515, 257, 126, 115] | Baked into existing sprite; no independent RectTransform |
| house_roof | [415, 173, 328, 145] | Baked into existing sprite; no independent RectTransform |
| house_sill | [447, 366, 272, 31] | Baked into existing sprite; no independent RectTransform |
| coin_pill | [129, 59, 214, 102] | Board controls/Coin pill |
| coin_disc | [35, 49, 132, 120] | Board controls/Coin disc |
| coin_leaf | [68, 76, 65, 62] | Baked into existing sprite; no independent RectTransform |
| coin_value | [175, 79, 130, 55] | Board controls/Coins * |
| move_pill | [32, 166, 409, 114] | Board controls/Move pill |
| move_feet | [51, 200, 62, 44] | Board controls/Painted footprints |
| move_value | [121, 196, 42, 52] | Board controls/Counted moves * |
| move_divider | [180, 185, 7, 74] | Board controls/Move divider/Painted divider |
| star_1 | [206, 174, 59, 48] | Board controls/Reachable star 0 |
| threshold_1 | [208, 229, 54, 34] | Board controls/Star threshold 0 |
| star_2 | [285, 174, 59, 48] | Board controls/Reachable star 1 |
| threshold_2 | [280, 229, 54, 34] | Board controls/Star threshold 1 |
| star_3 | [355, 174, 59, 48] | Board controls/Reachable star 2 |
| threshold_3 | [354, 229, 55, 34] | Board controls/Star threshold 2 |
| extra_pill | [34, 283, 205, 95] | Board controls/Day night counter/Phase pill |
| extra_sun | [57, 289, 85, 77] | Board controls/Day night counter/Sun moon |
| extra_value | [156, 302, 44, 51] | Board controls/Day night counter/Moves until phase |
| settings | [953, 77, 114, 108] | Board controls/Settings |
| settings_gear | [982, 100, 59, 61] | Board controls/Settings/Icon |
| goal_yellow | [369, 400, 102, 97] | Species goals/Single goal house/sunny goal info/Gold ring |
| goal_yellow_bird | [382, 414, 76, 70] | Species goals/Single goal house/sunny goal info/Portrait crop/Front portrait |
| goal_yellow_gift | [431, 457, 57, 53] | Species goals/Single goal house/sunny goal info/Species gift badge |
| goal_yellow_gift_icon | [441, 466, 37, 37] | Baked into existing sprite; no independent RectTransform |
| goal_blue | [507, 400, 102, 97] | Species goals/Single goal house/sky goal info/Gold ring |
| goal_blue_bird | [520, 414, 73, 69] | Species goals/Single goal house/sky goal info/Portrait crop/Front portrait |
| goal_blue_gift | [572, 457, 56, 53] | Species goals/Single goal house/sky goal info/Species gift badge |
| goal_blue_gift_icon | [582, 466, 36, 37] | Baked into existing sprite; no independent RectTransform |
| goal_owl | [646, 400, 102, 97] | Species goals/Single goal house/hoot goal info/Gold ring |
| goal_owl_bird | [659, 414, 72, 71] | Species goals/Single goal house/hoot goal info/Portrait crop/Front portrait |
| goal_owl_gift | [710, 457, 56, 53] | Species goals/Single goal house/hoot goal info/Species gift badge |
| goal_owl_gift_icon | [720, 466, 36, 37] | Baked into existing sprite; no independent RectTransform |
| bird_01 | [57, 688, 132, 139] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_02 | [186, 695, 130, 133] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_03 | [312, 710, 132, 119] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_04 | [664, 828, 135, 140] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_05 | [792, 837, 131, 131] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_06 | [915, 832, 136, 138] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_07 | [57, 1015, 134, 141] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_08 | [184, 1021, 132, 136] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_09 | [311, 1016, 136, 142] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_10 | [663, 1155, 136, 141] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_11 | [793, 1163, 130, 134] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_12 | [916, 1158, 136, 140] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| tool_bar | [195, 1727, 731, 149] | Board controls/Padded cream bar |
| tool_undo | [247, 1748, 108, 108] | Board controls/Undo |
| tool_hint | [487, 1748, 108, 108] | Board controls/Firefly |
| tool_branch | [727, 1748, 108, 108] | Board controls/Sapling |
| undo_glyph | [267, 1771, 63, 54] | Board controls/Undo/Icon |
| hint_bulb | [505, 1757, 69, 83] | Board controls/Firefly/Icon |
| branch_glyph | [740, 1776, 80, 51] | Board controls/Sapling/Icon |
| branch_plus | [780, 1768, 36, 39] | Baked into branch icon |
| undo_glow | [213, 1713, 175, 175] | Board controls/Recommended tool gold glow |
| pointer_shadow | [273, 1644, 58, 66] | Existing arrow outline; separate diffuse shadow art unavailable |
| undo_pointer | [276, 1646, 48, 56] | Board controls/Recommended tool plush pointer |

## board_sleeping_bubble

| JSON element | Measured rect | Runtime object / scope |
| --- | --- | --- |
| forest | [0, 0, 1080, 1920] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_ul | [0, 787, 531, 94] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_ur | [576, 923, 504, 116] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_ll | [0, 1119, 533, 94] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_lr | [579, 1256, 501, 108] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| house_rope | [549, 0, 58, 185] | Species goals/Single house rope/Rope artwork |
| goal_house | [413, 168, 330, 236] | Species goals/Single goal house/Birdhouse/House art crop/House body |
| house_roof | [413, 168, 330, 154] | Baked into existing sprite; no independent RectTransform |
| house_hole | [513, 250, 127, 117] | Baked into existing sprite; no independent RectTransform |
| coin_pill | [91, 58, 252, 103] | Board controls/Coin pill |
| coin | [36, 47, 130, 126] | Board controls/Coin disc |
| coin_leaf | [71, 75, 58, 67] | Baked into existing sprite; no independent RectTransform |
| coin_value | [175, 79, 128, 56] | Board controls/Coins * |
| moves_pill | [31, 164, 407, 117] | Board controls/Move pill |
| move_feet | [50, 198, 64, 49] | Board controls/Painted footprints |
| moves_value | [122, 195, 40, 51] | Board controls/Counted moves * |
| moves_divider | [180, 181, 7, 80] | Board controls/Move divider/Painted divider |
| threshold_star_1 | [206, 171, 57, 54] | Board controls/Reachable star 0 |
| threshold_1 | [210, 229, 46, 33] | Board controls/Star threshold 0 |
| threshold_star_2 | [280, 171, 57, 54] | Board controls/Reachable star 1 |
| threshold_2 | [282, 229, 48, 33] | Board controls/Star threshold 1 |
| threshold_star_3 | [352, 171, 57, 54] | Board controls/Reachable star 2 |
| threshold_3 | [355, 229, 52, 33] | Board controls/Star threshold 2 |
| sun_pill | [33, 281, 209, 99] | Board controls/Day night counter/Phase pill |
| sun_icon | [55, 287, 88, 80] | Board controls/Day night counter/Sun moon |
| sun_value | [157, 301, 44, 52] | Board controls/Day night counter/Moves until phase |
| settings | [951, 77, 115, 109] | Board controls/Settings |
| gear | [977, 98, 65, 64] | Board controls/Settings/Icon |
| goal_ring_yellow | [370, 399, 102, 100] | Species goals/Single goal house/sunny goal info/Gold ring |
| goal_bird_yellow | [380, 410, 78, 79] | Species goals/Single goal house/sunny goal info/Portrait crop/Front portrait |
| gift_badge_yellow | [430, 453, 59, 56] | Species goals/Single goal house/sunny goal info/Species gift badge |
| gift_yellow | [440, 463, 39, 41] | Baked into existing sprite; no independent RectTransform |
| goal_ring_blue | [508, 399, 103, 100] | Species goals/Single goal house/sky goal info/Gold ring |
| goal_bird_blue | [520, 409, 78, 80] | Species goals/Single goal house/sky goal info/Portrait crop/Front portrait |
| gift_badge_blue | [570, 453, 60, 57] | Species goals/Single goal house/sky goal info/Species gift badge |
| gift_blue | [580, 463, 40, 42] | Baked into existing sprite; no independent RectTransform |
| goal_ring_owl | [647, 398, 104, 102] | Species goals/Single goal house/hoot goal info/Gold ring |
| goal_bird_owl | [658, 409, 80, 80] | Species goals/Single goal house/hoot goal info/Portrait crop/Front portrait |
| gift_badge_owl | [709, 453, 59, 57] | Species goals/Single goal house/hoot goal info/Species gift badge |
| gift_owl | [719, 463, 39, 42] | Baked into existing sprite; no independent RectTransform |
| ul_yellow | [57, 688, 135, 140] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| ul_blue | [181, 693, 136, 135] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| ul_owl | [311, 708, 134, 120] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| ur_yellow_l | [666, 829, 135, 140] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| ur_blue | [790, 834, 132, 135] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| ur_yellow_r | [917, 831, 134, 138] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| ll_yellow_l | [57, 1016, 135, 143] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| ll_blue | [182, 1021, 135, 138] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| ll_yellow_r | [309, 1018, 137, 141] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| lr_yellow_l | [665, 1154, 136, 142] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| lr_blue | [790, 1160, 133, 137] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| lr_yellow_r | [917, 1156, 135, 141] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| sleep_bubble | [314, 556, 182, 155] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| sleep_moon | [367, 579, 80, 79] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| sleep_zzz | [426, 698, 43, 30] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| tools_bar | [195, 1727, 731, 149] | Board controls/Padded cream bar |
| undo | [245, 1745, 111, 111] | Board controls/Undo |
| undo_glyph | [270, 1771, 58, 53] | Board controls/Undo/Icon |
| hint | [485, 1745, 112, 111] | Board controls/Firefly |
| hint_glyph | [505, 1762, 71, 74] | Board controls/Firefly/Icon |
| branch | [725, 1745, 112, 111] | Board controls/Sapling |
| branch_glyph | [746, 1772, 73, 57] | Board controls/Sapling/Icon |

## board_many_branches

| JSON element | Measured rect | Runtime object / scope |
| --- | --- | --- |
| forest | [0, 0, 1080, 1920] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| tree_left | [0, 0, 208, 1920] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| tree_right | [1016, 685, 64, 1235] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| canopy_left | [0, 0, 480, 320] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| canopy_right | [656, 0, 424, 285] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| rope | [546, 0, 62, 186] | Species goals/Single house rope/Rope artwork |
| coin_pill | [39, 51, 304, 115] | Board controls/Coin pill + Coin disc (compound bounds) |
| coin_disc | [39, 51, 129, 116] | Board controls/Coin disc |
| coin_leaf | [73, 76, 56, 64] | Baked into existing sprite; no independent RectTransform |
| coins | [173, 78, 131, 54] | Board controls/Coins * |
| move_pill | [33, 167, 408, 111] | Board controls/Move pill |
| footprint | [49, 201, 64, 44] | Board controls/Painted footprints |
| moves | [121, 194, 44, 52] | Board controls/Counted moves * |
| move_divider | [179, 181, 9, 81] | Board controls/Move divider/Painted divider |
| star_1 | [206, 173, 55, 50] | Board controls/Reachable star 0 |
| threshold_1 | [208, 229, 49, 32] | Board controls/Star threshold 0 |
| star_2 | [280, 173, 55, 50] | Board controls/Reachable star 1 |
| threshold_2 | [280, 229, 51, 32] | Board controls/Star threshold 1 |
| star_3 | [354, 173, 55, 50] | Board controls/Reachable star 2 |
| threshold_3 | [354, 229, 54, 32] | Board controls/Star threshold 2 |
| sun_pill | [34, 284, 205, 94] | Board controls/Day night counter/Phase pill |
| sun | [54, 290, 91, 78] | Board controls/Day night counter/Sun moon |
| sun_count | [156, 300, 45, 53] | Board controls/Day night counter/Moves until phase |
| settings | [952, 76, 116, 109] | Board controls/Settings |
| gear | [976, 98, 69, 65] | Board controls/Settings/Icon |
| goal_house | [414, 169, 330, 232] | Species goals/Single goal house/Birdhouse/House art crop/House body |
| house_body | [445, 233, 270, 167] | Baked into existing sprite; no independent RectTransform |
| house_roof | [414, 169, 330, 147] | Baked into existing sprite; no independent RectTransform |
| house_opening | [521, 271, 108, 85] | Baked into existing sprite; no independent RectTransform |
| goal_yellow | [370, 400, 103, 97] | Species goals/Single goal house/sunny goal info/Gold ring |
| portrait_yellow | [382, 410, 79, 75] | Species goals/Single goal house/sunny goal info/Portrait crop/Front portrait |
| gift_yellow | [429, 455, 60, 52] | Species goals/Single goal house/sunny goal info/Species gift badge |
| gift_icon_yellow | [439, 464, 40, 36] | Baked into existing sprite; no independent RectTransform |
| goal_blue | [509, 400, 103, 97] | Species goals/Single goal house/sky goal info/Gold ring |
| portrait_blue | [521, 410, 79, 75] | Species goals/Single goal house/sky goal info/Portrait crop/Front portrait |
| gift_blue | [571, 455, 58, 53] | Species goals/Single goal house/sky goal info/Species gift badge |
| gift_icon_blue | [581, 464, 38, 37] | Baked into existing sprite; no independent RectTransform |
| goal_owl | [649, 400, 103, 97] | Species goals/Single goal house/hoot goal info/Gold ring |
| portrait_owl | [659, 409, 82, 77] | Species goals/Single goal house/hoot goal info/Portrait crop/Front portrait |
| gift_owl | [709, 455, 59, 53] | Species goals/Single goal house/hoot goal info/Species gift badge |
| gift_icon_owl | [719, 464, 39, 37] | Baked into existing sprite; no independent RectTransform |
| branch_L1 | [0, 710, 535, 112] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_L2 | [0, 874, 535, 98] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_L3 | [0, 1044, 535, 91] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_L4 | [0, 1200, 535, 91] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_L5 | [0, 1364, 535, 89] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_R1 | [549, 809, 531, 105] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_R2 | [549, 971, 531, 100] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_R3 | [549, 1131, 531, 100] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_R4 | [549, 1292, 531, 100] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| branch_R5 | [549, 1453, 531, 100] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_L1_1 | [75, 616, 126, 151] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_L1_2 | [201, 594, 127, 173] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_L1_3 | [329, 632, 123, 135] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_L2_1 | [71, 823, 137, 104] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_L2_2 | [203, 833, 125, 94] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_L2_3 | [328, 817, 128, 110] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_L3_1 | [74, 980, 126, 107] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_L3_2 | [201, 982, 128, 105] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_L3_3 | [329, 984, 138, 104] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_L5_1 | [71, 1292, 130, 116] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_L5_2 | [201, 1297, 128, 111] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_L5_3 | [329, 1312, 123, 96] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_R1_1 | [629, 713, 127, 135] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_R1_2 | [757, 713, 129, 135] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_R1_3 | [885, 711, 132, 137] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_R2_1 | [629, 902, 128, 106] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_R2_2 | [757, 902, 129, 106] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_R2_3 | [885, 896, 132, 112] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_R3_1 | [629, 1070, 128, 99] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_R3_2 | [758, 1062, 128, 107] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_R3_3 | [886, 1059, 132, 110] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_R4_1 | [629, 1219, 128, 109] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_R4_2 | [757, 1218, 129, 110] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_R4_3 | [885, 1219, 142, 109] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_R5_1 | [629, 1380, 128, 108] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_R5_2 | [758, 1380, 128, 108] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| bird_R5_3 | [886, 1380, 132, 108] | Outside HUD ownership: board, birds, scenery or sleep bubble |
| tools | [194, 1727, 730, 145] | Board controls/Padded cream bar |
| undo | [245, 1748, 111, 106] | Board controls/Undo |
| undo_icon | [268, 1770, 65, 54] | Board controls/Undo/Icon |
| hint | [486, 1748, 109, 106] | Board controls/Firefly |
| hint_icon | [503, 1761, 77, 74] | Board controls/Firefly/Icon |
| branch_plus | [726, 1748, 109, 106] | Board controls/Sapling |
| branch_plus_icon | [744, 1769, 73, 62] | Board controls/Sapling/Icon |
