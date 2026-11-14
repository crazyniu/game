# World switching on the world page

User request (2026-11-14): tapping a world card also opens its level page, and the level page gets two
buttons that switch to the left and right world, so the player never has to go back to change worlds.

Contract, implemented in the canonical `game/map_view.gd`:

- Card tap: an open card opens its world at once; a closed card is first centred, a second tap on it
  answers like Play on a locked world (`gate_tapped`, error sound).
- World page: `WorldPrevious` and `WorldNext`, 112 x 112, boxes at x 24 and 944, centre y 937 (side edges, in the gap between
  tile rows 2 and 3, far from Back and clear of the current bird). Same role, same size in every state; the first/last world dims the
  matching arrow. Next into a closed world plays the error sound and emits `gate_tapped`.
- Art: `btn_world_previous` is `btn_forest_back` hue-shifted from orange to green so it does not read as Back;
  `btn_world_next` is the same tile flipped, so the pair is a mirror pair. Both are derived, the original is unchanged.
- Design text: `docs/design/map.md` (Interaction).
