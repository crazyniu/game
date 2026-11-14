# Shop forest cards

## Historical scope — superseded visual layout

This page records the earlier forest-card implementation stage, not the current
Shop layout contract. The later [shop shelf composition](shop-shelves.md) replaced
its no-shelves requirement and product geometry. Current layout is owned by the
canonical `shop_view.gd` and subsequent approved corrections; this historical
page must not force all categories back to the old two-column grid. Runtime
implementation alone is not evidence of new owner visual acceptance.

The interaction/data-preservation requirements below remain applicable. The
coordinates, materials and lettering below describe the earlier stage only.

## Earlier direction and scope

The visual reference for this earlier stage was `art/approved_ui/shop.png`: a bright forest setting with two columns of deep-teal cards, narrow honey-gold rattan edges and three icon-only category tabs. At that stage the coastal three-column proposal and shelf presentation were not the implementation target; the later shelf contract superseded this restriction.

The user requested card grouping, improved product lettering and colors, navigation controls matching the coin shop, and completion of the confirmed non-gameplay UI without additional confirmation or code upload. The later report of missing tabs and content emerging below the navigation requires fixed visible tabs and a clipped item viewport. This authorizes the current implementation and self-checks; it does not claim a new owner acceptance of the runtime captures.

The canonical implementation remains `PerchSort/game/shop_view.gd` for Boosters, Outfits and Nests. Purchase dialogs are handled by their separate task. Existing economics, item order, inventory, unlock rules, saved fields, quantity behavior and equip/navigation contracts remain unchanged.

## Historical composition

All measurements use the 1080 x 1920 design canvas.

- Close begins at (48,48), using `MenuNavigation.SIZE` and the unchanged coin-shop close artwork. Its dimensions are approximately 130 x 133, derived from the coin-shop canvas scale rather than a separate Shop-specific size.
- The existing compact feather balance remains aligned to the upper-right. One floating painted category heading occupies (220,208,640,112), without a sign backing or subtitle.
- The item viewport is (48,352,984,1240), ending at y=1592. It explicitly clips all item content.
- Equal cards measure 480 x 392, with 24px horizontal and vertical gaps. The current six boosters occupy two columns and three rows. Outfits and nests use the same geometry and additional rows.
- Each card reserves (100,28,280,224) for its illustration, (40,256,400,48) for its localized painted name, and (100,316,280,60) for its numeric price or ownership action. Booster stock is a small badge near the top-right.
- The fixed tab region begins at y=1668, outside the scroll container. Three equal 200 x 168 category frames are centered at x=252, 540 and 828, y=1752. Their lantern, outfit hat and birdhouse icons retain their proportions. Selection changes the face tint, not control size. There are no visible tab labels or broad cream backplates.

## Scrolling and interaction

Each category derives its scrollable height from its item count. Six boosters fit within the viewport, and adding more than six produces reachable additional rows. All nine outfit species and all nine nests remain available. Hidden cached pages and background outfit preparation must not change the active category's content height.

Switching categories resets to the top. Cards cannot render or receive item interaction through the clipped boundary into the fixed navigation area. Dragging does not trigger a purchase. Existing live outfit birds, locked/unmet states, owned/equipped states, item actions and return navigation remain supported.

## Historical materials and lettering

Reuse the existing clean standalone assets: `background_shop_forest`, `panel_shop_forest`, `btn_shop_gold`, `badge_shop_stock`, current booster/bird/house illustrations, category icons and localized painted words. The existing illustrations and accepted cutouts remain unchanged. The reference image is not cropped into runtime.

Product names use the Shop-only `shop_product_word.gdshader`: warm ivory faces retain the original painted dark outlines and relief for contrast against teal. Category headings keep the existing painted gold treatment. Prices and owned counts remain painted numeric glyphs. No runtime font-rendered words, new descriptions, promotional copy, shelves, display stands or repeated plaques are introduced.

The six booster unit prices remain Firefly 30, Sprout 60, Owl Eye 80, Wake Bell 80, Scarecrow 100 and Spring Rain 100. Screenshots use illustrative profile data; production values continue to come from the existing Shop/Profile sources.

## Historical evidence

Implementation checks, screenshots, comparison images and limitations from that stage are recorded in `docs/work-log/shop-card-layout/report.md`. Validation results belong there rather than serving as an additional visual-approval claim in this specification. No protected Home, Settings or gameplay redesign is part of this Shop change.
