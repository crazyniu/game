# Map carousel restyle

Rebuild the world-card carousel in `PerchSort/game/map_view.gd` from `art/approved_ui/map_carousel.png`, measured into 1080 x 1920 boxes.

- Back tile 128 px at (40, 70); feather bar 430 px wide, top 72, right margin 40.
- Card 626 x 844 centred at (540, 956); neighbours at 0.72 scale, 540 px apart, both visible at the screen edges.
- World number badge 140 px on the card's top edge: green on the centred card, gold on the neighbours.
- Card picture: one scenic painting per world (`world/card_scene_w<N>`), not the level-path painting.
- Name on a wooden plank with hibiscus ends (`island_name_plank`), plank centre 741 px below the card top.
- Stars chip 400 x 104 at y 1423 (cream pill with round ends); Play pill 590 x 170 at y 1546, triangle and word centred together.
- Ten page dots at y 1771, 54 px apart, 32 px, cream with the current one green.
- No bottom button row (the design has none; shop and aviary stay on the home page).
- Backdrop `island_map_backdrop`: open sea, palm fronds in the top corners, hibiscus and sand at the bottom.
- Locked cards: dimmed picture and padlock only. The design's "Needs 15 stars" tag is not shown because worlds unlock by clearing the previous world, not by stars; rules are unchanged.
