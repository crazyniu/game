# Map, shop and aviary redesign

Files changed: `PerchSort/game/map_view.gd`, `shop_view.gd`, `aviary_view.gd`. Layout constants sit at the top of each file, derived from `docs/design/ui-system.md`. PerchSort/layout rects are no longer used by these three screens.

## Map
- Top shade (y 0-280) and a bottom shade behind the nav bar. Coin pill (coin 104 overlapping the pill, number left-aligned 24 after the coin, cap 52) and the world ribbon (480 wide) on a second row 16 below the pill: a 480 ribbon beside a 4-digit pill would collide.
- Nodes 216 wide (current 260), no longer stretched ovals. Level number centred on the node (cap 52 cream, current node 72). Cleared nodes: three 36 px stars on the lower rim. Locked nodes: number on the upper face; the stone sprite already carries its own painted padlock, so no extra badge was added.
- Current bird: live BirdView at 70 % of the node width, standing on the node. Overscroll rooms (160 at the top, 200 at the bottom) fade into the painting, so the current node and bird are never under the HUD or the nav bar.
- Gate moved right of the trail (centre x 870, 300x224) so it clears the nodes and the bird of the last node of a world.
- Nav bar 840x160, 48 from the bottom, three 128 discs evenly spaced.

## Shop
- Header: coin pill and a 112 close disc (same size as the board gear). Tabs are 128 discs 64 apart. Cards are 488 wide with 24 gutters. Name ribbons share one width (380). Price pills are 80 high with a 64 coin and a cap 40 number. Stock badge 52 with a white 30 number. Pager discs 96.
- Pictures fit their areas without stretching. Outfit birds keep breathing and blinking.
- The confirm popup now uses `Modal` (dim 0.72, shadow, ribbon across the top edge, 96 close disc at the corner, 720 wide, 160 primary disc 40 below). The ribbon, picture and price pill are stacked 32 apart. The outfit picture is a live BirdView. Without enough coins the primary disc is dimmed but still shakes the price.

## Aviary
- Header matches the shop. The board art is fitted uniformly (it was stretched before) between y 168 and 1872.
- List: ring 260 with the bird at 80 %, name plates all 222x64, count chip 190x80, ability coin 80. Three rows fit exactly, with no peek of the fourth row. The scroll track sits in its own gutter, and the cream edge fades were removed because they showed as rectangles over the arched opening.
- Detail: name ribbon, caption plate, hero bird standing on the card's top edge, card of 4 outfit tiles, ability row and house chip, with 32 px between groups. The block is centred vertically.

## States looked at (two passes each)
map_view: forest_trail, first_level, autumn_world, all_cleared, gate_locked. shop_view: boosters, outfits, houses, confirm_booster, confirm_outfit, locked_tools, insufficient. aviary_view: list, list_some_unmet, list_scrolled, detail_berry, detail_crow, detail_hoot. Smoke test: `smoke checks: 0 failures`.

## Remaining issues and missing art
- There is no shop title ribbon sprite, so the shop has no title (icon-only tabs).
- `tab_selected_ring` is a ring plus a green bar and does not centre cleanly on a 128 disc, so a few pixels of offset remain.
- The crow detail card holds only the house chip, so the screen is sparse.
- Autumn world: the open-gate art is slightly different from the closed gate.
- A node that sits under the HUD row while scrolling is dimmed by the shade but can still sit behind the ribbon.
- Shop and aviary backgrounds still show the corner tree crowns, which are baked into `bg_level_w1_noon`.
