# T54I Map viewport, bottom bar collisions and clipping (user feedback 2026-11-04)
Read tasks/T54_contract.md. Owned: `PerchSort/Assets/Scripts/Game/MapView.cs`, `MapModel.cs`, `MapMotion.cs`, `GateView.cs`, `LeafTransition.cs`, new `Tests/**/T54I*.cs`. Others read-only.
Problems seen on the map near level 34-40:
- I1 The bottom bar (shop, aviary, settings discs) sits directly on top of level node 34 and its stars; nodes can scroll under the bar. The scroll viewport must end above the bar (reserve the bar
  height plus margin) with a soft fade, so no node is ever hidden or touched by the bar; the player node must never be placed under it when the map auto-focuses.
- I2 The player chick on the last visible node (40) is cut by the top edge and the top-right gear overlaps it. Reserve a top margin (coin pill and gear zone) so the current node and its chick are always fully visible, at
  the top end of the world too; auto-focus puts the current node about 40 % from the top, clamped.
- I3 Node discs are fine in size now; check node 34-40 spacing so labels and star trios never overlap the bar or each other.
- I4 Gear duplicates (top-right and bottom bar) stay as in the approved image `ui_map.png`; do not remove either.
Evidence: screenshots (and side by side with the approved map image) at current level 1, 4, 10, 11, 34, 40, plus scrolling to both ends.
Tests: PlayMode assertion that for every level node the rect never intersects the bottom-bar rect or the top HUD rect once the viewport is clamped, for current levels 1, 10, 34, 40.
