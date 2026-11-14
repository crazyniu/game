# Non-gameplay dialog family

## Historical scope — superseded component prescription

This page records an earlier correction, not the current FamilyDialog material
or geometry contract. The later [dialog layout correction](dialog-layout.md)
records rejection of this panel; the subsequent
[dialog-family restyle report](../work-log/dialog-family-restyle/report.md)
records another integrated composition. Do not reintroduce the old Shop panel,
transparent heading, icon-only Home/Replay actions or fixed dimensions below
merely to comply with this historical page.

Preserve the interaction and data safeguards below. Current runtime code and
later approved corrections establish the implementation baseline, not automatic
owner visual acceptance; no new visual approval is asserted here.

## Earlier correction

The user rejected the mixed old flowered title plaque, shop field, action skins and text in the FamilyDialog screenshots. Reuse the established Home/Chest forest material family. Keep accepted source images unchanged; never replace a reference image or approved runtime baseline.

## Historical components

- Full 1080 × 1920 background_shop_forest through DrawPurchaseArt.background, exactly the accepted Shop/Coin Shop forest and cover fit. Do not use the superseded dark Home background.
- Forest panel: panel_shop_forest via ShopSkin.product_card, width 960 and fixed corner scale 0.35, exactly the Shop and purchase confirmation container. Source region (37, 37, 1312, 1052), patch margins 240/300/240/260 and unmodified colour. Content-driven height remains inside the canvas. Container style still awaits user confirmation; sharing a texture is evidence of source consistency, not proof of visual approval.
- Transparent gold heading: ShopSkin.product_heading, one 680 × 112 visible-ink box centered at the panel top. No flowered wooden plank. Level ordinal remains visible alongside its heading.
- Actions: home_play_plate with Chest nine-patch margins 220/100/220/100. Primary 520 × 176; secondary 416 × 140.8. Primary/secondary ratio is exactly 1.25, independent of enabled state. Play and OK reuse ChestLettering; Next uses the existing rounded white painted word. Home and Replay use the established white control symbols, with unchanged callbacks.
- Rating stars: island_star_full and island_star_empty, alpha-fitted equal 180 × 180 boxes with 36 px spacing. Filled count is data; no different middle-star scale.
- Transparent feather rewards retain icon + amount; stat/reward numerals use the accepted Home digits, with a simple white plus sign. Supporting lines use ShopSkin.product_word at fixed 54 px visible height. Bird and world names use the same gold heading family at 76 px.
- Hero birds remain animated and centered. Win hero feet sit 62 px above the panel rim; two companions use symmetrical side positions. No mirrored animation.
- Level-card Close uses MenuNavigation; celebration action-only dismissal remains unchanged.

## Behavior and acceptance safeguards

All seven functions keep their data, reward values, callback order, disabled state and duplicate-tap lockout. Validate both languages, zero/multiple/full rewards, level 100 and full roster, all unlock descriptions, repeated actions, close, and interrupted animation. Render actual 1080 × 1920 images and compare every state. User visual approval is still pending. No font rendering, gameplay changes, publication or source-image modifications.
