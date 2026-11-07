# Task: rebuild the map screen (PerchSort/game/map_view.gd)

Follow docs/work-log/ui-rebuild/spec.md (shared contract) first.

Target look: `map_screen()` in tools/kit/style_preview.py (approved). The painted world maps
(map_bg_continuous_w1..w4) stay as the background and keep scrolling as today.
- Level nodes: cleared = cream circle 132 with the ink number (cap 52) and three stars (46) under it, gold for earned;
  current = green circle 168 with the white number (cap 64) and the level's bird standing on top of it, alive
  (BirdView, breathing and idle); locked = cream circle 132, dimmed, with a lock icon. All nodes of one kind share
  one size. A gentle bounce on the current node.
- Top bar: world title word in a cream pill at the top left, coins counter (coin art + number) at the top right; both
  stay fixed while the map scrolls; a soft top shade under them is fine if it stays subtle.
- Bottom: shop (orange, icon shop), aviary (orange, icon aviary), settings (cream, icon gear) as secondary buttons
  centred on y 1792, 200 apart. No bar behind them.
- World gates between worlds: keep the gate art and its behaviour (locked/open, tap to unlock); make the locked state
  read clearly in the new style (lock icon on a cream circle).
- Keep every signal (level_tapped, shop_requested, aviary_requested, settings_requested, gate_tapped) and preview
  states (forest_trail, first_level, autumn_world, all_cleared, gate_locked).
Report: docs/work-log/ui-rebuild/map.md.
