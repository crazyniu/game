# Shop purchase quantity

Status: layout direction and direct implementation approved by the user on 2026-11-07; source implementation delivered, runtime approval pending.

## Scope and authority

The user approved an item illustration, editable quantity with minus/plus controls, and an orange Buy button containing the total price, all inside the existing cream modal. Keep the current visual style. Quantity applies to repeatable boosters; outfits and houses retain their existing single-ownership rules. Their purchase action should also say Buy rather than show a check.

AGENTS.md assigns game code to Claude and art to Codex by default. After this boundary was explained, the user explicitly authorized Codex to modify this shop code. The exception is scoped to this purchase change; it does not amend the project-wide instructions. No tests, lint, builds, captures or runtime automation are authorized.

## Existing style

Use the current `docs/design/ui-system.md`, `game/ui.gd`, `game/popup.gd`, and `game/shop_view.gd`, not historical Unity screen specifications.

- Panel: cream #fff7e8, rim #e9d6b0, radius 56, bottom edge 12, existing shadow and dim backdrop.
- Text: existing brown #5a3a1e; white on the orange Buy button.
- Buy: orange #ffa53c, lower edge #da7a14, pill radius 64, edge 8, existing shop press feedback.
- Quantity: cream round minus/plus controls, brown painted symbols. The input has a warm rim, cream face and radius 32 so it reads as a field rather than a third button. A brown caret is shown only while editing.
- Existing red close button, item pictures, numeral sprites and modal transitions are reused. No explanatory captions, additional confirmation, separate price line or floating action below the panel.

## Layout

Coordinates below are design pixels on the existing 1080 x 1920 canvas. The shared modal still computes vertical placement. Booster body groups are 240, 128 and 128 high with the existing 32-pixel gaps; there is no external action row.

| Element | Rectangle / dimensions |
| --- | --- |
| Panel | x=108, y=566, width=864, height=788 |
| Title | Existing shared title bounds, cap 64; top at panel y+56 |
| Close | Existing 128 circle, centre at panel right-32, panel top+40 |
| Illustration | x=420, y=726, 240 x 240; preserve aspect ratio |
| Quantity row | x=284, y=998, 512 x 128 |
| Minus | x=284, y=998, 128 x 128 |
| Editable quantity | x=436, y=998, 208 x 128; numeral cap 44 |
| Plus | x=668, y=998, 128 x 128 |
| Buy | x=284, y=1158, 512 x 128 |

The illustration is reduced from the current 320-pixel popup slot to 240. Minus and plus have identical dimensions and keep those dimensions while disabled. Centre Buy, the existing 64-pixel coin and the total together, with 24 pixels between word and coin and 12 between coin and number. Buy and total use cap 44. The Buy bar has no sibling primary action; it is not a scaled-up round control.

For single-ownership goods omit the quantity group and let the shared modal recalculate height. Keep their existing live bird or house presentation; do not change inventory rules or equipped check badges.

## Interaction and data contract

- Open with quantity 1. Minus/plus adjust by one; tapping the middle field permits direct integer input on desktop and numeric keyboard input on mobile.
- Use existing painted numeral textures for visible text, including during editing. Do not display a native font-backed value on top of the artwork.
- The quantity field is the sole editable field. Enter or keyboard Done commits editing; it does not purchase. Close cancels without spending.
- Minus is disabled at 1. Available quantity is supplied from the current balance and existing unit price. Plus is disabled at the affordable maximum; an entered quantity is committed within the affordable range. With insufficient coins for one, retain quantity 1 and disable Buy.
- Update the button's total immediately for every committed quantity change. Example: Owl Eye costs 80; quantity 5 displays 400 beside the coin on Buy.
- On Buy, emit one purchase request containing item ID and quantity. Existing purchase authority must check affordability and apply the entire purchase together; do not purchase by looping separate transactions.
- The user authorized direct implementation after approving this layout. The canonical view also exposes fake-profile previews using the same controls. Preserve catalog prices, package discounts, unlocks, save keys and owned/equipped semantics. Runtime visual approval remains pending.
- Reuse current modal entry/exit, `ui_tap`, `collect`, `error` and menu music behavior. Disable the purchase action during closing so one click dispatches once.

## Asset inventory

| Asset | Source | Status |
| --- | --- | --- |
| Panel, orange pill, quantity surfaces | Existing shared shape style | Reuse |
| Item illustrations, coin and close icon | Existing artwork | Reuse |
| Digits, minus and plus | Existing locale numeral cells | Reuse |
| Buy lettering | `shop.buy`, `PerchSort/art/locale/en/label_shop_buy.png` | Prepared by the existing offline lettering pipeline |

Buy uses bundled Nunito weight 800, whole-phrase transparent white lettering, cap 128, line height 208, baseline 160 and padding 8 in the source texture; render at cap 44 in the button. It follows the same localization manifest and runtime tinting as existing labels. No AI redraw, new font or restyled illustration is needed.

## User-owned acceptance

No validation commands are part of this delivery. The canonical preview interface includes `confirm_booster`, `confirm_multiple`, `confirm_maximum`, `insufficient` and `confirm_outfit`; the quantity field can be edited in the booster previews. The user will assess style consistency, input, live totals, disabled sizes and purchase behavior in the game. Source delivery does not establish runtime acceptance.
