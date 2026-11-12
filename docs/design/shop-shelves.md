# Shop shelf composition

Reconstruction of the owner's approved runtime composition and subsequent marked corrections. Same canonical shop, same profile data, actions and translations.

1080 × 1920 canvas. Scroll viewport (60,390,960,1240), one-column rows 960 × 238 with 14px gap. Fixed three icon-only tabs at (152,1668,776,168). Names and action centers share y=95 within the single-color, 74% opacity jungle-green row. Product visible contour rests at y=188, just inside the wood shelf top y=184. Actions are 286 × 100. Long lists derive height from their full catalog, not six entries.

Verified existing independent runtime assets: prop_shop_shelf, panel_shop_sign, island_chevron, all booster art and all nine house bodies. Shelf source edge area (0,370,1760,350) excludes export padding, not design-image pixels. Clean assets are reused without modification. House visible contours exclude alpha <=32 noise. No mockup pixels are cut into runtime assets.

Stock: latest owner correction removes the circular badge entirely. Plain white ×quantity is centered under the product name at (280,112,340,52), with no backing, outline, glow or gradient. Names use (280,30,340,76); the pair forms a centered information block. Existing zero visibility and count cap remain unchanged. This follows the owner’s explicit plain-number preference.

Shared contract: ShopSkin.product_price uses feather_amount; callers must not center its digits independently. Own shop_view.gd and shop geometry tests only. Shared button source belongs to the button owner. Final acceptance needs actual viewport captures in English and Chinese plus pointer/scroll checks.
