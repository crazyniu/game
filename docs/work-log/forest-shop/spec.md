# Approved forest shop implementation

The user selected the SECOND latest generated screen, the clean two-column deep-teal/rattan
Boosters image at `/Users/apple/.codex/generated_images/01a1cad1-6a3e-7c53-920e-f87dbb76cd58/exec-35d518f3-e361-4d7e-aca6-25092373bfab.png`,
authorized implementation, and required colors to match Home. Use Home's teal roof, warm honey/gold
and soft ivory accents within the approved forest setting. No extra subtitle. Preserve prices,
eligibility, inventory, profile fields, signals, current word sprites and locale switching.

## Ownership

- Coordinator: `game/shop_skin.gd`, new independently generated forest UI assets, a Shop-only
  ivory label material if needed, integration, documentation and focused runtime checks.
- Shop worker: only `game/shop_view.gd` (three categories, caching/scroll layout, quantity popup).
- Coin worker: only `game/coin_shop_view.gd` (same six static packages and simulation flow).
- Outfit worker: only `game/aviary_view.gd`, and only presentation when `_from_shop` is true.
  Preserve standalone Aviary presentation and behavior.
- Workers do not spawn workers, generate art, run the game/tests or modify shared helpers/docs.
  All work uses the current tree; do not reset, pull, switch branches, commit, merge or release.

All other pages remain paused. Restored Settings is frozen. No changes to global UI/Modal/word
rendering, rewards, gameplay or saves. Existing `ShopSkin.card/button/modal` remain unchanged for
non-shop consumers; the new shop surfaces use focused APIs below.

## Shared component API (coordinator implements)

- `ShopSkin.FOREST_BACKGROUND`: bare texture name of the new quiet forest painting.
- `ShopSkin.product_card(parent, rect, selected := false) -> Control`: deep-teal blank rattan card;
  child `Skin`, same dimensions for selected states, no text/icon built in.
- `ShopSkin.product_word(parent, label_id, bounds) -> TextureRect`: existing localized painted
  semantic word rendered in readable ivory, preserving its shape and automatic locale update.
- `ShopSkin.product_heading(parent, label_id, bounds) -> TextureRect`: current gold heading family,
  floating without a wooden sign backing; preserve full sprite proportions.
- `ShopSkin.product_price(parent, rect, value) -> Control`: gold painted face and white digits only.
- `ShopSkin.product_button(parent, name, rect, callback, kind := "gold") -> Button`: common dimensional
  action, supports gold and green, keeps size and dims the complete contents on press.
- `ShopSkin.product_modal(host, title_id, heights, instant, action_height := 0.0) -> Modal`: preserves
  current lifecycle/metadata and builds a teal/rattan frame with gold heading, no extra wooden plaque.
- Existing `counter` and `amount` APIs remain available and unchanged.

Main Shop uses 2 columns x 3 visible rows for six boosters, with 480 x 392 product cards, 24 px gaps,
content at (48,352), header title around (280,208,520,112), and three equal 200 x 168 category frames
centered at x=252/540/828, y=1752. Outfit species and nests use the same two-column product family;
all nine items remain reachable through scrolling. Set the visible page's content height correctly
on tab switches and warming. Do not reintroduce shelves, pedestals, perches inside product grids,
large cream backplates, extra captions, generic skin replacement or font-rendered words.

The outfit detail may keep its existing hero bird/perch and 3 x 2 choice geometry, but shop-route
tiles, prices, headings, caption and confirmation must use the new family. Coin packages use the
same forest, teal cards and gold actions. Real ¥ must remain readable. No real payment or grants.

The user's later self-check request authorizes focused native runtime captures, representative
click/quantity/purchase/equip/return checks and brief animation checks. Coordinator runs these after
integration; no unrelated full-game suite or export. User visual acceptance remains pending.
