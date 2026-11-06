# Screen fit report

Files: `PerchSort/game/shop_view.gd`, `aviary_view.gd`, `map_view.gd`. Public APIs and preview states unchanged.

## Shop
- Item pictures fill their cards: tool art fits the medallion by visible pixels, house sprites use their solid bounds (faint shadows no longer shrink them), outfit birds scaled 1.5x.
- Card frames are placed by visible body (transparent margins of `card_wood` / `panel_wood` compensated), so cards sit within about 5 px of the layout rects.
- Equipped card has a soft gold glow (blurred gold StyleBoxFlat shadow). A 0 stock badge is hidden.
- Confirm modal now skips its pop-in when previewing in a capture (`instant`), uses the measured panel body, and caps long titles. No `label_title_*` sprite matches the card names (only "Firefly", "Sapling"... exist, which differ from "Hint", "Extra Branch"), so the green ribbon plus painted card name is kept.

## Aviary
- Board art is placed so its cream opening starts at the mockup's panel top; the first ring starts at the layout y (332). The window shows exactly three rows. Detail hero bird is larger (fit 780x625), detail panel frame is padding-corrected.
- Remaining: the art's frame has a leaf crest and legs the mockup lacks; the mockup's wings-open hero pose has no sprite, so the bird looks narrower. Detail shows 4 outfit tiles (default plus 3), mockup shows 3. A few pixels of the next row can peek at the bottom edge.

## Map
- Nodes are mockup sized (cleared 306x226, current 326x224, locked 250x176); stars larger.
- The last node of a world (10/20/30) puts its bird beside the node, smaller, so it never covers the gate; gate x moved to the mockup (center 700).
- Nav bar is `panel_wood` with thin borders (orange rim, cream inside). No dedicated felt bar sprite exists.
- Remaining: node 11 of the next world overlaps the gate top (painted path geometry), title reads the generic world name rather than "Forest Trail".

## Evidence
Side by side captures (mockup left) of shop boosters, locked tools, outfits, houses, confirm, aviary list, list_scrolled, detail berry/hoot, map forest_trail and gate_locked were viewed.
Touch scrolling: headless script sending InputEventScreenTouch and ScreenDrag. Map: offset 9600 -> 9200 on a 400 px downward drag; aviary list: scroll 0 -> 400 on a 400 px upward drag. Both work (touch is emulated as mouse).
Smoke: `smoke checks: 0 failures`, no SCRIPT ERROR.
