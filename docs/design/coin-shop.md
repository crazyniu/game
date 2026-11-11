# Coin shop

Status: static UI shell with the authorized package-material repair, 2026-11-11. User runtime approval is pending.

## Intent and direction

The player opens the coin counter to compare six coin packages and choose a price. Reuse the current [island style](island-style.md) and the shared controls from the [shop](shop.md): turquoise sea, a gold painted heading on a rope-free honey-wood plaque, and equal green price actions. The user rejected six large repeated cream cards. The page now presents the individual coin piles, pouch, basket and chests in open space, with small timber supports carrying their amounts. Container shapes and increasing treasure scale distinguish the packages. There is no enclosing panel, promotional copy, discount claim, countdown, extra tab, or subtitle.

This is a clickable preview. Package values and prices are fixed. Choosing a package opens a result headed `Purchase Preview` / `模拟购买`; it does not charge money or grant saved currency.

## Navigation and layers

The shared `FeatherBar` plus opens the page; the whole counter may be its hit target. The originating screen and any existing shop confirmation remain mounted below it, with their state preserved. The coin page occupies a separate modal layer above the ordinary popup layer. Its close tile removes this page and restores input to the previous surface without navigating or rebuilding it.

The coin page's own counter is read-only. The purchase preview overlays the treasure displays; closing or acknowledging the preview returns to the same six-package grid. No asynchronous payment, loading, failure, or offline state is introduced in this shell.

## Measured composition

Coordinates use the canonical portrait 1080 × 1920 canvas. The same layout scales with the existing project viewport. There is no scrolling.

| Element | Pattern/source | Box (x, y, width, height) |
| --- | --- | --- |
| Background | `background_shop_sea` | 0, 0, 1080, 1920 |
| Close | Existing island close tile | 48, 48, 112, 112 |
| Balance | Shared `FeatherBar`, width 320 | Top-right, nominal 712, 48; retain existing painted-edge alignment |
| Title plaque | `ShopSkin.plaque` / `panel_shop_sign` | 280, 208, 520, 132 |
| Open package groups | Transparent `Control`; no enclosing surface | Columns x 48 and 552; rows y 392, 864, 1336; each 480 × 448 |
| Timber support / amount plaque | Existing `panel_shop_sign`, uniform aspect ratio | Group-local 68, 224, 344, 101 |
| Coin amount | Existing cream-white painted digits | Group-local 116, 260, 248, 56 |
| Price action | `ShopSkin.button`, green | Group-local 72, 336, 336, 112 |

The transparent groups retain the original grid footprint; only a 344 × 101 timber support sits behind each amount. It is not a replacement package card. Draw the support first, then the treasure, amount and price action. All six price actions have the same dimensions and visual treatment in normal and pressed states. Pressing dims the face without shrinking it. Prices and the painted yuan symbol are cream-white on green; amounts use cream-white painted digits on warm wood. The page title stays inside the plaque's fixed word box by its visible ink.

The yuan sprite fits its actual painted region `(277, 47, 229, 253)` into the 40 × 56 symbol box; its 768 × 342 source canvas includes faint surrounding pixels and must not determine the glyph scale. The small chest uses only the left 480 pixels of its 512 × 496 production sprite to exclude two detached leaves at the right edge. Preserve the original chest scale and placement in both package and simulation views. These are page-local texture presentation settings; retained source artwork is unchanged.

Illustration boxes are group-local and keep each source's aspect ratio. Their bottoms align at y 256 above the amount, while the larger containers rise above their supports. The first row starts below the title; the final purchase actions end at y 1784, leaving 136 pixels at the bottom of the design canvas.

| Illustration | Box (x, y, width, height) |
| --- | --- |
| Small stack | 112, 24, 256, 232 |
| Coin cluster | 96, 8, 288, 248 |
| Coin pouch | 92, -8, 296, 264 |
| Coin basket | 88, -16, 304, 272 |
| Small treasure chest | 80, -24, 320, 280 |
| Large treasure chest | 72, -32, 336, 288 |

## Fixed package data

Read order is left to right, then top to bottom. Prices are yuan in both existing locale packs; the shell performs no currency conversion.

| Package | Coins | Price |
| --- | ---: | ---: |
| Small stack | 600 | ¥6 |
| Coin cluster | 2000 | ¥18 |
| Coin pouch | 3600 | ¥30 |
| Coin basket | 9000 | ¥68 |
| Small treasure chest | 20000 | ¥128 |
| Large treasure chest | 33000 | ¥198 |

Illustrations progress from a small stack to a full chest while retaining the game's own embossed feather coin, rounded painted material and consistent upper-left light. Their display height increases from 232 to 288 pixels; the support and purchase-control sizes stay fixed. The role descriptions above identify assets; they are not visible labels.

## View and fake-data contract

Canonical view: `PerchSort/game/coin_shop_view.gd`, with no alternate scene or replacement resource root.

- `setup(balance: int)` shows the incoming balance and fixed packages.
- `closed` requests removal of the page.
- `purchase_requested(pack: Dictionary)` reports the selected fixed package.
- `show_preview_purchase(pack: Dictionary)` presents a shared-style modal containing the simulation heading, selected coin illustration, painted `+amount`, and an existing painted OK action.
- The local simulation controller connects the purchase signal to the preview method. It does not call payment services, mutate `Profile`, save data, or change the originating balance.
- `preview(main, state)` uses this same production view and signals. Typical preview balance is 1280.

| Preview state | Fake data / presentation |
| --- | --- |
| `packs` | Six packages, balance 1280 |
| `empty_balance` | Same packages and active price actions, balance 0 |
| `max_balance` | Same packages, counter display maximum 99999 |
| `purchase_preview` | Selected package's simulation result over the grid |

The display cap remains a presentation rule; it is never written back to saved data. Balance size does not affect paid-package eligibility in this preview.

## Motion, sound, and text

| Trigger | Feedback |
| --- | --- |
| Open page | Single opacity fade over 0.18 s; no staggered package delay |
| Price press | Existing `Island.PRESSED` dimming and `ui_tap`; fixed geometry |
| Show preview | Existing shared modal appearance and coin reward feedback |
| Close preview | Shared modal close transition and `ui_back` |
| Close page | Return to preserved surface; existing back feedback |
| Music | Reuse `bgm_menu` without restarting it; respect the existing mute controls and restore the applicable page music on exit |

Instant previews skip motion. Tweens belong to their visible view and end when that view is removed. There are no birds or fabricated character animation on this screen.

All words and currency symbols are painted sprites. Reuse existing painted numbers and OK words. New exact labels are `Coin Shop` / `金币商店` and `Purchase Preview` / `模拟购买`. Amounts contain at most five digits; prices contain `¥` plus at most three digits. No font-rendered fallback is allowed.

## Asset manifest

| Asset | Status / use |
| --- | --- |
| Existing sea, blank wooden plaque, close tile, counter | Reuse existing clean game assets; the plaque also provides the small package supports |
| Existing `ShopSkin.button` and modal surface | Keep the shared dimensional price actions and explicit simulation popup |
| Existing painted numerals and OK labels | Reuse current locale packs |
| `PerchSort/art/island/icon_coin_{stack,cluster,pouch,basket,chest,hoard}.png` | Delivered: six independent pack sprites cut from a generated production atlas |
| `PerchSort/art/locale/{en,zh}/island_words/gold_coin_shop_title.png` and `gold_coin_purchase_preview.png` | Delivered: localized painted words cut from a generated lettering atlas |
| `PerchSort/art/island/icon_coin_yuan.png` | Delivered: standalone painted currency symbol |

This material repair reuses the delivered production assets without editing them or generating new art. Reference images guide style only; no pixels are copied, cropped, traced, or sliced from a design image into the game. Keep retained originals unchanged. The original generation paths and prompts remain in the [coin-shop implementation report](../work-log/coin-shop/report.md).

## User acceptance boundary

The user owns testing and visual approval. No tests, lint, builds, screenshots, UI automation, comparison captures, or timing programs are requested for this change. The following are criteria for the user's review, not claimed results:

- The counter opens the page and close returns to the exact originating state.
- All six treasure displays and complete price actions fit the portrait viewport without scrolling or overlap.
- Coin amounts and yuan prices match the fixed table, in both installed languages.
- All six price actions retain 336 × 112 geometry while pressed.
- Every package opens a clearly labelled simulation result, without changing saved coins.
- The page shows distinct treasure silhouettes against open sea, with compact timber amount supports and the current painted heading/button family; large cream package cards are absent.
- Opening uses the specified 0.18-second fade, and music follows the existing mute controls.

Runtime layout, interaction, motion, audio, timing, contrast, safe areas, and smallest/largest device behavior: **NOT ASSESSED**. No formal accessibility or device performance target is supplied; existing portrait scaling and touch conventions apply, with new action targets at least 112 pixels high.
