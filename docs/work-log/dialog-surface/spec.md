# Dialog border and background comparison

The user clarified that the unresolved mismatch concerns dialog borders and backgrounds, not only controls and lettering. Compare actual Shop, Coin Shop and Chest pages with all seven unapproved family dialogs. Preserve every accepted artwork byte, canonical page and gameplay/rule path.

Material gate: inspected the actual reference renders and clean background_shop_forest source before implementation. The current border and interior already use the exact Shop/PurchaseDialog panel_shop_forest via ShopSkin.product_card. No new material is missing. Do not redraw or recolour it. Use the accepted Shop/Coin Shop sunlit forest with its existing cover fit for the surrounding field, without the purchase overlay's dimming. Keep Home/Chest pages unchanged. The comparison must report remaining visual ambiguity rather than claim that this source match settles the user's border concern.

Scope: FamilyDialog.decorate background construction only, focused regression assertions, canonical capture runner and evidence. No popup callbacks, layout, rewards, badges, animation, shared helpers or image pixels change.

Verification: compare exact atlas/region/corner scale/margins/white modulation; all family tests, aggregate suite; bilingual 42-state full-resolution real GL captures plus Shop/Coin Shop/PurchasePreview/Chest references; retained motion cases and pointer paths. Independent visual review and user acceptance remain distinct.
