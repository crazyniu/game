# Shop UI delivery

Implemented the user's latest shop direction on `island-ui-redesign`: a six-card booster grid, consistent small cream corners,24-pixel card gaps, a top-centred rope-free category heading, compact right-aligned balance, white numeric-only purchase faces, and rounded-square stock badges.

The nest tab now uses newly painted dimensional gold English/Chinese names matching the approved bird-name style. The hanger icon is removed from outfit counts. Outfit detail and purchase dialogs share the shop skin; the detail's unnecessary coin-style ability icon and extra Outfits heading are removed.

## Source

`shop_view.gd` owns grids, cached categories, quantity controls, and preview states. `shop_skin.gd` supplies common cards, actions, amounts, headings, and counters. Shop-mode detail changes live in `aviary_view.gd`; the standalone aviary presentation remains separate through its existing mode. `ui_text.gd` supports painted word regions in the existing locale manifests. `popup.gd` allows the shop to supply its own heading without first drawing the old decoration.

Rules, prices, levels, persisted fields, and enum orders are unchanged.

## Simple verification

The final short real-input check used a temporary profile and suppressed persistence:

```sh
/Applications/Godot.app/Contents/MacOS/Godot --path PerchSort --resolution 1080x1920 --script res://tests/run_shop.gd -- --shop-evidence=/Users/apple/Documents/projects/game/build/screens/shop
```

Result:10 key checks passed, zero failures, exit0, and no errors in the final log. Covered category/header navigation, retained outfit content, outfit buy/equip, language change and return, nest buy/equip, booster quantity/purchase, and close-to-Home. A language-change child-removal error and a missing legacy nest heading were corrected during this check. Subsequent artwork and spacing changes were checked with targeted native captures, without rerunning a broad suite.

On the checked cold advanced-outfit fixture the tab handler took1.94ms and the first drawn frame119.37ms. Cached content is retained; this is not a claim of zero first-use rendering delay.

Displayed captures were inspected for all three tabs, shop outfit detail, booster purchase, and outfit purchase. English/Chinese nest words were checked in source sheets and runtime. The painted-word guard passes with13 remaining legacy call sites elsewhere; `git diff --check` passes.

Evidence: [Shop surfaces](evidence/shop-surfaces.jpg), [latest nest and outfit tabs](evidence/shop-categories.jpg), [short interaction log](evidence/shop-check.log). Full captures remain in `build/screens/shop/`.

No full level/core suite, device acceptance, audio listening assessment, package, commit, or release was performed. No runtime capture was promoted to the user-approved baseline.
