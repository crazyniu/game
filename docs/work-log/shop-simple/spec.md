# Simple shop

Clean but still glossy (not the plain settings look). Applies to the Boosters, Outfits and Nests tabs of
`game/shop_view.gd`.
- Backdrop: the open-sea `island_map_backdrop`; a light translucent veil groups the cards instead of a framed sheet.
- One card style (glossy soft cream tile); the equipped nest keeps the green outline and check.
- Words from `tools/island/bake_labels.py` (Lilita One / full Noto Sans CJK SC Bold): names in warm brown, one-line uses
  in a lighter brown, "Equipped" as a quiet state word. Tabs and the Equip / Open buttons are baked by
  `tools/island/bake_buttons.py` with the same words; nothing is stretched.
- Boosters: picture, name, use, gold price; the owned count badge sits in the card's top-right corner.
- Outfits: the same veil; a large middle card (bird, name, hanger n/m outfits, Open), neighbours peeking at the sides,
  page dots; no arrows, plank or roster strip. Swipe, tap a neighbour or a dot to turn.
- Unused baked pieces from the earlier settings work were removed.
