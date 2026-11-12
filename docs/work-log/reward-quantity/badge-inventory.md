# Quantity and status badge inventory

The user's IMG_5751 marks the two green +1 circles attached to victory boosters. The visible highlight is baked into `island_badge_green`; `Island.plus_badge` does not add a glow node or animate opacity. Its sole production call was `popups._drops`. The local correction removes that call, not the shared asset/helper.

Retained, out-of-scope shared roles:

- Home notifications: `HomeView._badge` calls red `Island.badge`, scaled from 66 to 48 design pixels. Home is frozen.
- Chest ready count: `popups.chest` calls green `Island.badge`.
- Map world labels and normal/current level discs directly use green/gold badge images. Current Map nodes additionally have a separate `glow_soft` selection halo. This is not the marked reward count.
- Completed worlds, equipped Aviary outfits and owned Shop nests use `Island.check_badge`.
- Shop inventory counts use the separate `badge_shop_stock` art and preserve a visible zero.
- Generic challenge rewards already show plain `FamilyDialog.number` amounts under icons; Draw results already show separate painted `DrawPurchaseArt.number` amounts. Their hero/celebration rays are not quantity badges and are untouched.

No replacement PNG, global shader or shared Island change is introduced. Existing cream glyphs, including plus and 0 through 9 in both locales, retain their bytes. Their pre-painted dark edge provides contrast without a new glow or glossy backing.
