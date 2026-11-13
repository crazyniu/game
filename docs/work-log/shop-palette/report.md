# Shop in the home style: report

## Approach
Rebuilt from a fixed size table instead of tuning numbers by eye. Codex painted one button family from the home Play
button (`btn_plate_green`, `btn_key_green`, `btn_key_cream`, `field_quantity`, `panel_card_cream` in `PerchSort/art/island`),
all drawn at native ratio and only scaled uniformly. `PerchSort/game/button_kit.gd` (ButtonKit) holds the table:
PRIMARY 360x136, SECONDARY 288x108 (PRIMARY / 1.25), KEY 108x108, TAB 196x192, FIELD 184x106; text cap 0.48 x height.

## Changed
- `shop_view.gd`: home jungle backdrop, painted cream cards, booster rows (picture with owned-count badge, gold name,
  price button), two-column outfit and nest cards, square tabs (green selected, cream others), purchase popup on PurchaseDialog.
- `purchase_dialog.gd`: one 860 px painted card, title sign over its top, close on its corner, action inside the card.
  Used by shop, aviary outfit purchase and coin preview. Buy shows feather and price only, no word.
- `aviary_view.gd`, `coin_shop_view.gd`: their purchase buttons use ButtonKit PRIMARY.
- `shop_product_word.gdshader` gold item names; `shop_caption_word.gdshader` deep green captions.
- Tests updated to the new contract: run_shop, run_shop_cards, run_shop_amounts, run_shop_cards_gui,
  run_purchase_buttons, run_purchase_dialogs, run_purchase_close.

## Lettering
All shop and purchase-popup words are AI-painted (Codex) in one style taken from the approved design: titles and names
bright yellow with a thick dark-green outline, captions deep green. They are shown exactly as painted (no shader
recolouring, no cutting from older word art), as painted ids `lettering_<key>` in `art/locale/{en,zh}/island_words`:
3 shop headers, 6 boosters, 9 nests, 9 birds, 50 outfits, the coin preview title and 6 captions, English and Chinese
(158 images). A missing word logs an error and shows nothing. Hanging title sign `sign_hanging.png` and gold-rimmed card
`panel_card_cream.png` are Codex paintings from the approved design; the wide Buy button keeps its round ends.

## Verification
- `bash scripts/run_tests.sh`: naming 0 violations, painted words 0 problems, smoke 0 failures, core 0 failures.
- run_shop, run_shop_cards, run_shop_amounts (2010 checks), run_shop_cards_gui, run_purchase_buttons (128),
  run_purchase_dialogs (98), run_menu_navigation (107): 0 failures.
- Evidence: `evidence/compare_*.png` (approved design left, runtime right), `button_kit.png`, `runtime_aviary_buy.png`.

## Boundaries
- run_purchase_close: about 15 random "double pointer closes exactly once" cases fail; the same kind fails on the
  untouched commit f425a14. All geometry checks pass.
- run_aviary_actions hangs on a stale skin assertion on both this branch and f425a14 (pre-existing).
- Only the shop and purchase popups use ButtonKit so far. Aviary page, map, settings and other popups still use their
  old buttons. Coin Shop page itself is unchanged.
- Chinese captures and motion frame sequences were not produced in this pass. Not committed; awaiting the user's review.
