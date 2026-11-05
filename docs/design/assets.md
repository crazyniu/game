# Asset specs and manifest

Approved direction for the board: art/ui_design board_few_branches, board_many_branches, tree_worlds, board_sleeping_bubble (user, 2026-11-05) and docs/design/art-direction.md (natural painted forest, not plastic; plush birds). Every asset is its own engine-ready file at final size; derived files only.

## Board scenery (one painting per tree layout)
| Asset | Size | Spec | Status |
|---|---|---|---|
| board_tree_w<N>_rows<R> (N=1..4, R=2..5) | 1080x1920 RGBA, transparent sky | ONE painting of both edge trees: left and right trunks hugging the screen edges (visible width about 120 px, wider roots at the bottom), crowns at the top corners, and exactly R branches per side growing out of the trunks at the feet-line heights of docs/design/screens/board.md (left and right columns); every branch about 530 px long from the edge, thick at the root (about 70 px) tapering to a rounded tip, flat top where feet stand at the listed y, bark continuous from trunk into branch, vines/moss/leaves across joints; world dressing: w1 green summer, w2 autumn (orange/red leaves), w3 misty deep forest (dark green, moss, light shafts), w4 snow (snow caps, icicles). Natural painterly, no glossy plastic. | needed |
| board_tree_w<N>_rows<R>_odd | same | identical painting but the LAST right branch absent (bark healed, a small knot or leaf cluster there) | needed |
| board_background_w<N>_<variant> | existing | keep the existing sky/forest backgrounds | existing |

## Board HUD
| Asset | Size | Spec | Status |
|---|---|---|---|
| goal_house_single | 240x240 | one rope-hung wooden birdhouse, dark empty doorway, roof neutral honey-wood with green moss (no per-species colour), soft shadow | needed |
| goal_house_door_open | 240x240 | same house, door swung open | needed |
| goal_rope_single | 40x420 | single rope with two leaves at the top, tileable lower part | needed |
| badge_ring_gold | 64x64 | gold ring frame for a species face, transparent centre | needed |
| badge_check | 32x32 | green check disc | existing icon_check can be tinted; needed as own asset |
| gift_coin_<gift> (9) | 48x48 | gold coin with the gift icon (sun song, wind, encore feather, watering can, angry eyes, feather, flame, moon swap, moon beam) | partial (gift_* icons exist) |
| move_pill_frame | 380x128 | cream pill with honey-wood rim, divider line, room for footprint, number, three stars with numbers | needed |
| icon_footprint | 64x64 | two brown bird footprints | needed |
| dial_frame | 240x96 | small honey-wood pill for the day/night dial | needed |
| sleep_bubble | 320x140 | cream rounded bubble with tail, soft shadow | needed |
| wood discs, tool glyphs, coin pill, badges, stars | existing | btn_wood, tool_*, icon_*, coin_pill, badge_red, star_full | existing |
