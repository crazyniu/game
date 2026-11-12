# Coin shop

The current page is the six-package simulation documented in [Daily draw and coin purchase](draw-purchase.md), implemented in `PerchSort/game/coin_shop_view.gd`.

The accepted `art/approved_ui/coin_shop.png` foreground is retained as independent original-pixel cutouts over the approved forest background. No cream product cards, timber supports, sea, promotional claims, discounts or new purchase features are present. The original Chinese title is `金币商店`; English is `Coin Shop`. Currency images carry the accepted feather emblem.

Fixed packages: 600/¥6, 2000/¥18, 3600/¥30, 9000/¥68, 20000/¥128, 33000/¥198. The read-only balance shows at most the existing display maximum. Every complete price action has the same 276 × 108 source-space hit box; press and disabled feedback do not resize it.

The existing host contract remains `setup(balance)`, `closed`, `purchase_requested(pack)` and `show_preview_purchase(pack)`. The origin stays mounted below the purchase page. Selecting a package opens `模拟购买` / `Purchase Preview`, showing the selected illustration and painted +amount. Confirm or Close returns to the same grid; no payment, currency grant or persistence occurs.

Canonical fake-data states remain `packs`, `empty_balance`, `max_balance` and `purchase_preview`. Both locales were rendered in real Godot and the focused test covers every package, both dialog dismissal routes, balance limits, duplicate page entry, locale changes and unchanged profile data. See the linked implementation document for full evidence and limitations.
