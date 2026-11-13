# Daily draw and coin purchase implementation

Status: implemented and self-checked in an isolated cloud worktree on top of `57445427`. No commit, push, merge or deployment was made in this phase.

## Scope and material gate

The confirmed references are `art/approved_ui/lucky_spin.png` and `art/approved_ui/coin_shop.png`. Their sea setting is superseded only by the already approved `background_shop_forest.png`. Foreground images extracted and accepted by the user are retained byte-for-byte. No accepted cutout was repainted, retouched or replaced by similar artwork.

The material gate completed before implementation:

- 25 accepted original-pixel transparent cutouts: both Chinese titles, close controls, eight draw prizes, six purchase illustrations, six complete price buttons, and the complete Chinese Draw button.
- Four accepted generated missing parts: blank draw board, center chest, available star and used star.
- 19 generated missing parts self-accepted under the latest user direction: 15 painted numeral/symbol glyphs, blank read-only balance panel, and English Daily Draw, Coin Shop and Draw siblings.
- The unsuccessful original filled-star extraction was excluded entirely.

The [asset manifest](draw-purchase-assets.json) records origins, dimensions, exact hashes, acceptance and alpha bounds. `python3 ci/check_draw_purchase_assets.py` verifies all 48 shipped PNGs remain identical to their accepted or self-checked files. Original hard alpha edges remain unchanged; they were not silently softened or recolored. The original Chinese purchase title remains `金币商店`, and the original feather-embossed currency pictures are retained.

## Measured layout

The reference canvas is 916 × 1717. A page-local canvas uses a uniform scale of `1920 / 1717` and horizontal centering within 1080 × 1920. This preserves every sprite's aspect ratio rather than stretching the reference to a different portrait shape. The approved forest fills the viewport with aspect-preserving cover.

Source-space measurements:

| Item | Rectangle x, y, width, height |
| --- | --- |
| Draw title | 121, 111, 673, 218 |
| Draw close | 23,52,116,119; current shared coin-shop navigation sprite |
| Five attempts | Centers 214, 336, 458, 580, 702; y 341; box 108 × 104 |
| Draw board | 39, 486, 838, 846 |
| Draw action | 182, 1366, 553, 202 |
| Purchase title | 135, 115, 648, 199 |
| Purchase close | 23, 50, 116, 119 |
| Read-only balance | 619, 48, 270, 92 |
| Purchase amount rows | y 506, 915, 1346; height 50 |
| Price columns | x 117, 518 |
| Price rows | y 560, 961, 1395; identical box 276 × 108 |

The six purchase objects retain their exact source crop positions, including their differing natural sizes. There are no repeated product backplates or new supporting cards. The draw board uses the accepted separate blank board beneath the accepted prize crops. English artwork is fitted to the Chinese sprite's visible bounds without changing either file.

`DrawPurchaseArt` owns the two pages' uniform source canvas, local painted glyph fitting and complete translated sprites. Its tint-only button feedback is also reused by the explicit `MenuNavigation` helper. Shared ShopSkin, UI text catalogs, Home, core rules, profile schema and economy files are unchanged.

## Later non-gameplay integration

The user's navigation-consistency request explicitly makes the coin-shop Close the shared
control on revised non-gameplay pages. Main Draw now uses that original sprite and the
coin-shop dimensions; its former `btn_draw_close` PNG is retained unchanged in the asset
library. See `menu-navigation.md` for actual game-space sizing and frozen-screen boundaries.
The result dialog and purchase confirmations have their own later task reports in
`../work-log/draw-result/` and `purchase-dialogs.md`. Component verification below records
its original scope; the later combined report owns final integrated acceptance.

## Preserved behavior

- The eight outer prizes and their clockwise order are read from `Progress.DRAW_PRIZES`, with the original coin values and mystery multipliers.
- The existing callback grants/returns one result. The running ring, slowing cadence, booster reveal, result dialog and restore flow remain intact.
- A running draw disables Draw and Close without resizing them. Duplicate clicks do not consume extra attempts. Exhaustion leaves Draw dimmed and disabled.
- A mystery sprite is restored from the same accepted cutout after the reward is dismissed. Removing the owner cancels its running tween callbacks.
- The six packages remain 600/¥6, 2000/¥18, 3600/¥30, 9000/¥68, 20000/¥128 and 33000/¥198.
- Purchase remains explicitly labelled simulation. No payment service, currency grant, profile mutation or save operation was added.
- Purchase opens above the originating surface and closes to the same live instance. Its own counter is read-only and caps only the display at the existing maximum.
- The existing simulation dialog is retained. A page-local use of the existing painted OK label avoids the shared product-ink shader washing out its outline; no shared shader or locale asset was changed.

## Verification

Engine: Godot 4.7.2, GL Compatibility on the existing authorized cloud desktop (Mesa llvmpipe). Data/config/cache were isolated under ignored `build/draw-purchase-review/`. No user computer or new display server was used.

Commands:

1. `python3 ci/check_draw_purchase_assets.py`: 48 identical RGBA PNGs, recorded alpha bounds clear all canvas edges.
2. `bash scripts/run_tests.sh`, with `GODOT` set to the cloud 4.7.2 executable: naming tests and guard, painted-word guard, every-script/every-level smoke checks, and all 100 core solution replays passed.
3. `godot --headless --path PerchSort -s res://tests/run_draw_purchase.gd`: 179 focused assertions passed. Covers all six package simulations at zero/normal/max/over-max balances, both dismissal actions, unchanged profile, fixed press geometry, locale changes, page re-entry, restoration, draw duplicate lockout, exhausted state, accepted mystery restoration, and removal during animation.
4. Real OpenGL captures of 14 states: Chinese and English versions of draw normal/empty/full; purchase normal/empty/max/simulation.
5. Twenty-four actual draw-animation frames at requested 0.25-second spacing over six seconds. The running highlight changes prize cells and the result is shown; these are real game frames, not generated mockups.

The aggregate suite was rerun after the final OK-label correction and after staging newly introduced source files for naming coverage. Exact results are retained with the evidence.

## Actual desktop pointer checks

- Clicked Draw twice quickly. One attempt was consumed, Draw and Close became disabled, and the ring completed one result.
- Confirmed the result. The board returned with two attempts and usable controls.
- Closed Draw. The same Home returned without a leftover dim layer.
- Opened purchase from Home's counter, chose ¥30, observed `模拟购买` and +3600, confirmed, and saw the unchanged balance.
- Closed purchase and returned to Home.

A page-local OK-label contrast problem was found in this review, corrected, and recaptured in both languages. The QA window was then closed; other desktop windows were preserved.

## Evidence and limits

[Evidence folder](../work-log/draw-purchase/evidence/) contains original engine captures, before/reference comparisons, full/focused test logs, animation contact sheet and frame hashes. Original engine captures are 456 × 812 because the existing desktop constrained the requested 1080 × 1920 viewport. They were not enlarged or retouched for delivery. The comparison sheets downsample the large reference to that native runtime height.

The source layout, original foreground, forest-only background, open six-package hierarchy, complete labels and values, and normal/disabled geometry were visually checked. A new user-approved runtime baseline was not invented.

Limitations: native full-resolution/device/safe-area testing and audio listening were not performed; the audio driver was Dummy. The unsupported V-Sync warning appears in graphical logs. Motion/live shutdown reports two ObjectDB instances and one resource still in use. The focused headless harness reports four ObjectDB instances and two resources at shutdown; assertions pass, but this is not described as a clean shutdown. The headless icon-shader diagnostic is also disclosed in its log. Initial wrong-engine/environment and test-loader attempts were corrected before the successful final runs.
