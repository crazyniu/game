# Shop and settings buttons

Every worded button on the shop (three tabs, Equip, Equipped, Open) and settings (Help, Restart, Home, language
switch) is one complete picture per language, baked by `tools/island/bake_buttons.py` into `PerchSort/art/buttons/`:
glossy base, icon or picture, and painted word centred on the measured face of the base. Runtime shows the single
picture through `Island.word_button` / `Island.word_picture` (name + `_` + locale). Buttons with changing numbers
(price pills, counters) stay composed.

Base: `art/island/island_pill_glossy.png`, the home page's Play base with its triangle painted out (derived file).
Help icon: `art/island/island_question.png` (local Codex, settings design as style reference).

Layout from `art/approved_ui/shop.png`, `shop_outfits.png`, `shop_nests.png`, `settings.png`:
- Shop: smaller title sign (560), tabs 322 x 124 at y 418, sheet from y 500; booster pictures 236 tall with the
  count badge on the picture; nest cards 520 tall with larger houses and names.
- Outfits tab: wooden-framed panel, centre card 556 x 790 with a larger bird, wooden name plank, cream count chip,
  baked Open, side cards with planks, dots, roster of faces without plates (selected one on a green plate).
- Settings: language heading between leaves under a thin rule, one baked switch (green half under the current
  language, each half a tap target), baked Help 620 x 172.

## Calm palette (second round)
- One green: `tools/island/soften_palette.py` moves the glossy button base and the switch to the design green
  (67, 182, 22) at matching brightness (`island_pill_glossy`, `island_toggle_on_soft`).
- Soft rims: `island_grid_card_soft`, `island_row_card_soft`, `island_pill_cream_soft` replace the yellow rim and dark
  orange outline with a light tan on every screen; baked cream tabs and the language track use the same rim.
- Warm brown text: brown words and digits are drawn through `game/warm_ink.gdshader` (warm chocolate, lighter at the
  top) instead of the painted near-black; the baked buttons get the same colours.
- Shop scrolling: card tap areas pass wheel and drag events on to the scroll view; a mouse drag scrolls the grid and is
  never taken as a tap.
