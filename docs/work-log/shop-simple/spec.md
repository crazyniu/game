# Shop layout (rebuilt)

New layout, not a restyle of the old one. `game/shop_view.gd`:
- Header: close tile, feather bar, the title word "Shop" (baked title style: white, brown outline) over the sky.
- Content fits one screen on every tab (no scrolling, no scroll rail):
  Boosters: a list of six rows (picture with its owned count, name over one-line use, gold price on the right).
  Outfits: a 3 x 3 grid of bird kinds (bird in its current outfit, name, hanger n/m); a met kind opens its outfit page.
  Nests: a 3 x 3 grid of houses (picture, name, price / Equip / the quiet word "Equipped"; the equipped one has a green rim and check).
- Tabs: a tab bar docked at the bottom, picture over word; the current tab's picture is raised and its word is white on a green pill.
- Surfaces: frosted white glass cards over the open sea (no cream cards, no veil); drawn shapes, never stretched.
- Words from `tools/island/bake_labels.py` (one font per language); Equip / Open from `tools/island/bake_buttons.py`.
