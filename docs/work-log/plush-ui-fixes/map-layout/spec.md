# Map layout correction contract

Reference: `docs/design/layout/map.json`, its overlay, and `origin/ui-design-review:art/ui_design/map.png`. The reference state is Forest trail: current level 8, 1240 coins. Coordinates are absolute top-left design pixels on 1080 x 1920.

Ownership: MapView, MapModel, MapMotion, MapFakeData, map-only layout metrics and GameLayout rect cases, plus this evidence directory. Rules, saves, authored levels, state names and source artwork remain unchanged.

Place the measured visible bounds, cropping transparent padding at runtime. Child placement is `measured child center - measured parent top-left`; Unity anchored Y is the negative of that coordinate. Scrolling adds the content offset to the viewport top. Use existing painted number glyphs and the existing painted title.

The scenery uses contiguous cropped horizontal bands from the existing Art map backgrounds, with their world-transition bands retained. Their heights follow the node rows so each node remains on the existing painted path. No image files are generated or edited.

Validation is limited to silent state captures, rendered rect comparison, the capture-time scroll event probe, and the requested naming guard. No EditMode or PlayMode suites.

## Element mapping

| JSON element | Measured rect | Runtime object / visible part |
| --- | --- | --- |
| `forest` | `[0, 0, 1080, 1920]` | Map scenery (clipped scrolling trail bands) |
| `gate` | `[455, 203, 490, 365]` | World 2 gate / Gate arch (opaque source bounds) |
| `gate_lock` | `[670, 422, 64, 76]` | World 2 gate / Gate padlock (opaque source bounds) |
| `coin_pill` | `[91, 89, 216, 90]` | Coin pill |
| `coin_count` | `[160, 108, 119, 47]` | Coins / painted brown glyphs |
| `coin` | `[38, 78, 112, 108]` | Coin |
| `coin_leaf` | `[69, 103, 52, 54]` | Coin / painted leaf within sprite |
| `world_ribbon` | `[315, 72, 464, 131]` | World ribbon |
| `ribbon_leaf_l` | `[300, 109, 75, 122]` | Ribbon left leaves |
| `ribbon_leaf_r` | `[729, 119, 80, 75]` | Ribbon right leaves |
| `world_title` | `[378, 96, 345, 68]` | World title / painted caption source region |
| `level_10` | `[633, 580, 221, 152]` | Forty level path / Node 10 / Level 10 |
| `num_10` | `[697, 590, 99, 62]` | Node 10 / Level number / painted glyphs |
| `level_9` | `[632, 753, 251, 176]` | Forty level path / Node 9 / Level 9 |
| `num_9` | `[722, 767, 76, 70]` | Node 9 / Level number / painted glyphs |
| `level_8` | `[249, 918, 326, 224]` | Forty level path / Node 8 / Level 8 |
| `num_8` | `[366, 951, 105, 110]` | Node 8 / Level number / painted glyphs |
| `level_7` | `[482, 1127, 308, 228]` | Forty level path / Node 7 / Level 7 |
| `num_7` | `[598, 1151, 83, 87]` | Node 7 / Level number / painted glyphs |
| `level_6` | `[249, 1320, 309, 232]` | Forty level path / Node 6 / Level 6 |
| `num_6` | `[365, 1349, 87, 87]` | Node 6 / Level number / painted glyphs |
| `lock_10` | `[711, 649, 70, 73]` | Level 10 / lock baked in node_stone_locked |
| `lock_9` | `[722, 838, 74, 77]` | Level 9 / lock baked in node_stone_locked |
| `yellow_bird` | `[265, 618, 295, 331]` | Current node / Sunny chick / front frame opaque bounds |
| `sparkle_l` | `[210, 899, 48, 50]` | Current node / sparkle_l |
| `sparkle_r` | `[576, 895, 47, 53]` | Current node / sparkle_r |
| `star_7_1` | `[536, 1226, 69, 63]` | Node 7 / Star 1 |
| `star_7_2` | `[604, 1238, 70, 63]` | Node 7 / Star 2 |
| `star_7_3` | `[671, 1225, 70, 66]` | Node 7 / Star 3 |
| `star_6_1` | `[302, 1423, 74, 66]` | Node 6 / Star 1 |
| `star_6_2` | `[373, 1436, 71, 66]` | Node 6 / Star 2 |
| `star_6_3` | `[441, 1422, 73, 66]` | Node 6 / Star 3 |
| `nav_bar` | `[51, 1733, 978, 171]` | Padded navigation rim + cream inner pill |
| `nav_shop` | `[246, 1763, 108, 108]` | Shop |
| `nav_aviary` | `[486, 1763, 108, 108]` | Aviary |
| `nav_settings` | `[726, 1763, 108, 108]` | Settings |
| `icon_shop` | `[272, 1782, 56, 62]` | Shop / Icon |
| `icon_aviary` | `[507, 1781, 68, 63]` | Aviary / Icon |
| `icon_settings` | `[748, 1783, 64, 63]` | Settings / Icon |
| `aviary_bird` | `[530, 1813, 28, 24]` | Aviary / Icon / bird within sprite |
