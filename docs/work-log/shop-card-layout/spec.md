# Shop card layout

The current task replaces the rejected shelf presentation on the three main Shop tabs with the existing forest card family. Later user instructions request card grouping, readable product lettering, consistent coin-shop close/back controls, uninterrupted development and no code upload. This task preserves all prices, item order, eligibility, inventory, quantities, purchases and navigation.

## Materials checked before implementation

The clean standalone `background_shop_forest`, `panel_shop_forest`, `btn_shop_gold`, `badge_shop_stock`, current booster/bird/house illustrations and painted localized word sprites already exist. Their actual pixels were inspected before implementation. The forest and teal/gold card match the current `art/approved_ui/shop.png` direction. No source design is cropped into runtime and no accepted cutout is changed. The word shaders only recolor existing painted strokes for legibility, retaining their dark outlines. `btn_purchase_close` is reused unchanged.

## Measured layout

On the 1080 x 1920 design canvas: a coin-shop-sized close control (approximately 130 x 133) at (48,48), balance at the current right alignment, floating heading (220,208,640,112), viewport (48,352,984,1240), two 480 x 392 cards per row with 24 px gaps, three icon-only 200 x 168 tabs centered at x=252/540/828, y=1752. A card reserves (100,28,280,224) for art, (40,256,400,48) for its name, and (100,316,280,60) for its price/action. Cards are equal in every state. Titles and cards do not gain cream backplates.

Every category computes its content height from item count, so six boosters fit without scrolling and later rows stay reachable for more than six entries. Hidden cached pages must not change the visible content height. The existing purchase flow is owned by a separate task and remains untouched.
