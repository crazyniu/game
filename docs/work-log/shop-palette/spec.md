# Shop in the home style

Status: built as a clickable shell on the canonical `ShopView`; awaiting the user's visual acceptance.

## Authority
The user approved three design images (2026-11-12): Boosters, Outfits and Nests, generated from the home screen and the bird outfit page as style references. They replace `art/approved_ui/shop.png`, `shop_outfits.png` and `shop_nests.png` and are a style direction, not a pixel target.

## Contract
- Same bright home-jungle backdrop as Home (`background_home_jungle`), no dark forest panels.
- Cream rounded cards with one thin gold rim and a soft shadow; nothing else on the card.
- Boosters: one full-width row per item (picture, brown name, beige count chip, green feather price pill).
- Outfits and Nests: two columns of tall cards (picture, brown name, one pill). Outfits show the bird on its branch and the owned count `n/6` with a chevron; Nests show the feather price, Equip or Equipped.
- Tabs: three equal rounded green tiles; the selected one is bright lime with a gold rim, the others deep green with a light rim.
- Words stay painted label images; item names use the gold lettering with a dark outline of the titles and the aviary (`shop_product_word.gdshader`). No brown text. Count chips are deep green with painted cream numerals, chevrons are white.
- One pill size (288x76) for every price, Equip, Equipped and owned-count pill; the confirm popup Buy button is 352x96 and the step buttons 136x96.
- Logic, prices, save fields and purchase flows are unchanged.
