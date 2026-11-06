# screen-fit — task contract

YOUR FILES: PerchSort/game/map_view.gd, PerchSort/game/shop_view.gd, PerchSort/game/aviary_view.gd, report docs/work-log/screen-fit/report.md. Topic: screen-fit.
Make each screen match its mockup more closely (side-by-side every preview state, mockup left, capture right; iterate):
1. Shop (art/approved_ui/shop_boosters, shop_outfits, shop_houses, shop_confirm): item pictures (booster medallions, outfit birds, houses) fill the card like the mockup (they are noticeably smaller now); card sizes and gaps per layout JSON; hide a 0 stock badge; confirm popup title uses the big painted title ribbon look if a fitting label_title_* sprite exists, else keep; the equipped card has a visible soft gold glow.
2. Aviary (aviary_list, aviary_detail): the first row of the list starts where the mockup's does (it is ~90 px too low) and three full rows are visible like the mockup; the detail bird is larger like the mockup.
3. Map (map): the current-level bird must not cover the world gate in any state (gate_locked shows it overlapping); the bottom nav bar should look like the mockup's orange felt-framed bar (use an existing panel/nine-slice sprite; list it if none fits); node spacing/sizes closer to the mockup where the painted path allows.
Keep the public APIs and preview states. Also check touch scrolling works on map and aviary list by simulating InputEventScreenDrag/mouse drag in a short headless script (report result).
