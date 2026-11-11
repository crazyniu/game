# Coin shop delivery

Implemented the requested static coin-package UI shell in the canonical game. User runtime acceptance is pending.

## Behavior

- The shared resource counter and its painted plus open the coin shop on top of the current screen. The six packages use the fixed amounts and yuan prices in the [screen contract](../../design/coin-shop.md).
- The original page, shop tab, scroll position, and any existing confirmation remain mounted underneath. Close removes only the coin page. The coin page's own counter is read-only.
- Each identical green price action opens a result labelled `Purchase Preview` / `模拟购买`, showing the chosen illustration and painted `+amount`. The existing painted OK action returns to the package grid.
- There is no payment integration, coin grant, profile write, save-schema change, rule change, or level change.
- The page reuses the sea background, cream card nine-patch, wooden plaque, close tile, resource counter, painted digits, button feedback and menu music. All new words and the yuan symbol are painted artwork.

## Source and assets

Runtime: `PerchSort/game/coin_shop_view.gd`, the counter signal/hit target in `game/feather_bar.gd`, and automatic counter-signal routing plus a separately mounted coin page in `game/main.gd`. Both locale manifests register the new words; `tools/island/word_catalog.json` records their text and reuse IDs. Existing unrelated working-tree changes were preserved.

Production artwork: six `PerchSort/art/island/icon_coin_*.png` package sprites (`stack`, `cluster`, `pouch`, `basket`, `chest`, `hoard`), `icon_coin_yuan.png`, and `gold_coin_shop_title.png` / `gold_coin_purchase_preview.png` in both existing language packs. These are new production assets, not pixels extracted from the attached screenshot or a design mockup. No retained game artwork was overwritten.

## User review entry

Restart the game and tap the home resource counter or green plus. Direct fake-data previews use the existing launcher:

```sh
./run.sh --preview=coin_shop_view:packs
./run.sh --preview=coin_shop_view:empty_balance
./run.sh --preview=coin_shop_view:max_balance
./run.sh --preview=coin_shop_view:purchase_preview
```

These commands are supplied for the user; they were not run during implementation.

## Work performed and boundary

Read the owning source and existing design contracts, generated two transparent production atlases with built-in Codex image generation, and used Pillow via `uv run --no-project --with pillow python` to cut the new sprites and install their locale entries. Both original generated atlas files remain outside the game under `/Users/apple/.codex/generated_images/01a1ca72-24e2-7471-a13f-7fb6036a1fac/`.

A separate bounded source review reported no actionable findings in data/layout, GDScript usage, counter entry, GUI layering, preservation of the originating state, or the no-save simulation boundary. This is source review only.

The user subsequently reported a parse error when opening the page. The original source review missed an API mismatch: three calls supplied the explicit cap-height argument to the shorter `UI.number` wrapper, and the width call also supplied an unsupported style argument to `UI.number_width`. All four now use `UiText.number` / `UiText.number_width`, whose signatures accept these arguments. This fixes the reported source mismatch without changing the layout or adding a fallback around script loading. Runtime retry remains with the user; the later `new()` error was downstream of the failed script parse.

Per the user's explicit instruction, no tests, naming guards, lint, builds, launches, GUI automation, screenshots, comparison captures, or timed evidence programs were run. Runtime appearance, interaction, animation timing, music and mute behavior, device scaling, and user visual approval remain **NOT ASSESSED**. No release or deployment was performed.

## Generation prompts

Tool: built-in `image_gen`, transparent background requested for each production atlas. The following are the submitted prompts.

### Coin packages

```text
Create ONE production sprite atlas on a real transparent background, 1536x1024 landscape, exactly 3 columns by 2 rows of isolated original game item illustrations. Each cell is 512x512 and each object must be completely inside its own cell with at least 52 pixels transparent padding; no borders, no dividers, no labels, no words, no digits. Polished cheerful tropical casual mobile bird puzzle game art: chunky rounded glossy gold, warm upper-left lighting, soft painted shading, strong clean silhouette, no realistic photography, no black drop shadow extending outside. Each coin is round GOLD with ONE embossed curved feather emblem, not dollar signs or crowns. Six packs grow visibly in abundance: top-left three loose feather coins slightly overlapping; top-middle a small neat pile of roughly ten feather coins; top-right a small soft tan drawstring pouch brimming with feather coins; bottom-left a woven honey-coloured wicker basket full of feather coins; bottom-middle a small rounded open orange wooden treasure chest with gold bands full of feather coins; bottom-right a larger rounded open honey-orange treasure chest overflowing with feather coins and 2-3 tiny jewel accents, still contained in same cell. All six objects are viewed front-on from slightly above, coherent identical family, occupying similar overall bounding box about 380x340 centered per cell, baseline at local y418. No cards, no UI, no text, no numbers, no background scenery, no birds, no design mockup, no promotional ribbons, no glow disks, no reflections beneath. This atlas is original production artwork and NOT a screenshot to slice.
```

### Painted lettering

```text
Create ONE clean production UI LETTERING ATLAS for a tropical mobile game, true transparent background, landscape 1536x1024, exactly TWO COLUMNS x THREE ROWS equal cells (768x341). All content is standalone PAINTED LETTERING, no panels or UI mockup. Upper row: left cell contains exact Chinese text 金币商店, right cell exact English text Coin Shop. Middle row: left cell exact Chinese text 模拟购买, right cell exact English text Purchase Preview. Bottom row: left cell contains only the SINGLE currency symbol ¥ large; right cell EMPTY transparent. Titles and middle-row lettering: chunky rounded golden-yellow puffy painted letters with a restrained darker honey-brown narrow outline and subtle bright upper-left edge, friendly glossy casual bird sorting game style. Every text is one SINGLE horizontal line centered inside its own cell, with 60px horizontal and 80px vertical transparent padding, fully separated, no overlapping cells, no descenders outside cells. Bottom-left ¥ symbol is cream-white puffy painted ink with a dark GREEN narrow outline suitable for a green price button; clean traditional ¥ shape two horizontal bars. No extra characters, no decorations, no birds, no coins, no leaves, no sparkles, no rectangle, no colored backdrop, no cast shadow outside the lettering. Spell every letter and Chinese character exactly: 金 币 商 店 ; 模 拟 购 买 ; Coin Shop ; Purchase Preview. The Chinese four characters must be equally sized. Transparent PNG asset.
```

## Pattern reference

The entry flow is consistent with the official [Royal Match coin-purchase help](https://dreamgames.helpshift.com/hc/en/3-royal-match/faq/22-how-can-i-purchase-coins/): open the shop from the home resource counter, select a coin package, then purchase. The two-column grid, package quantities and prices here are project-specific design choices, not copied pricing or a claim about that game's current layout.
