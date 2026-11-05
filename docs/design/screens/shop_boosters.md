# Shop — boosters

Status: **draft**. Stage: screen specification. Portrait phones, touch/Android back; mouse click/vertical wheel in Editor. Design 1080x1920, phones 9:16–9:20; review 720x1280 and 1440x3200 with safe areas. No keyboard/controller contract. No Unity/runtime acceptance claimed.

Authority: [art direction](../art-direction.md), [patterns](../patterns.md), [brief and its checked-in addenda](../../work-log/ui-redesign/brief.md), [conventions](../../CONVENTIONS.md). Geometry: `art/approved_ui/layout.json:screens.shop_boosters` (manual visible bounds, ±8 px), `PerchSort/Assets/Resources/Art/{layout_metrics,glyph_metrics}.json`. Read `ui_shop_boosters.png` for plush/wood style, not its retired purple discs. This spec shares the glyph-size, numeric-bounds and missing-addendum decisions in [map](map.md).

## 1. Player need
The player arrives wanting to spend earned coins on optional help, see stock and understand what is still locked. They came from the map or tapped a zero-stock level tool. No ads, cash purchase or coin-pack flow.

## 2. Navigation
- Map Shop opens this tab. A zero-stock tool opens it focused on that item, without buying or using it. Logic retains the origin and board state; closing returns there, never restarts the level.
- Tabs switch to [outfits](shop_outfits.md) and [birdhouses](shop_houses.md), not the aviary. Close-X/Android back dismiss shop; no background-tap exit on this full page. Coin pill is display-only.
- An eligible card opens [purchase confirmation](shop_confirm.md). Cancel/back closes only confirmation; success closes confirmation and keeps this tab/offset. While confirmation is open, all shop input is blocked.
- Notifications wait until shop closes. Focus loss pauses audio/animation; hide cancels effects. Re-entry renders committed stock/wallet, not an unfinished counter.

## 3. Layout
Top-left design px; fixed chrome inside safe area, forest bleeds beyond it. Reuse `page`, `tabs`, `list-grid`, `card-button`, `coin-pill` and `wood-disc`. Do not reimplement those patterns here.

| Component | Pattern / asset | Size provenance | Anchor / position | Layer |
|---|---|---|---|---|
| Forest | `page`; existing noon forest | Design canvas 1080x1920 | Screen fill | background |
| Currency | `coin-pill`; `coin_pill`, coin | Library 190x76 | (48,150), safe-area adjusted | fixed chrome |
| Close | Page `wood-disc`; wood + `icon_close` | Library 108x108 | Measured shop center (965,197), safe-area adjusted | fixed chrome |
| Three tabs | `tabs` / `wood-disc`; bulb, hanger, home | Library page discs 108; selected ring 190 and pill 70x28 | Library centers (350,330), (540,330), (725,330) | fixed chrome |
| Six card slots | `list-grid`, `card-button`; `card_wood` or derived fitted frame | Candidate uniform 466x418, measured first card | Measured column starts x64/558; row starts y456/896/1338; clipped to viewport | scroll content |
| Booster illustration | Existing `tool_hint`, `tool_branch`, `tool_owl_eye`, `tool_bell`, `tool_scarecrow`, `tool_rain` on wood | Measured illustration envelope 240x240 | Upper card content; opaque normalization from glyph metrics | card content |
| Owned badge | `tool-disc` badge treatment; `badge_red` | Library 64 | Card upper-right; fully inside viewport | above illustration |
| Price / lock chip | `card-button`; `btn_green_pill`, coin; `stat_pill`, padlock | Measured price 266x98; lock chip 140x74 | Card bottom / illustration side; measured inner regions govern | card content |
| Confirmation | `modal-panel` | Confirmation spec | Over all cards/chrome | overlay |

Viewport uses measured grid envelope (62,452,956,1340), clipped further by safe-area edges; it stays below tabs and shows a right scroll indicator (`list-grid`). No bottom navigation bar on shop. Slots scroll if content cannot fit; no card is squeezed to keep all six visible. Counts/prices are display children: the card is the one purchase hit target, not a separate primary button per item.

[Decision needed: fitted shop-card pattern] The measured reference card envelope and reusable `card_wood` interior do not establish a valid final composite. Native frame is 640x640 with inner rect (75,92,486,480); a naive 466x418 stretch gives an inner height about 314, insufficient for a 240-high illustration plus 98-high price with clearances. The asset metadata does supply nine-slice borders (120,115,120,120). Options: measure/reflow the existing nine-sliced frame within the listed envelopes, or approve a less-dense measured card layout. Recommend reuse/reflow first, then publish its inner/price/badge bounds as a `card-button` variant; request derived art only if that measured fit fails. Never stretch the frame or silently overlap its children. Same dependency applies to the cosmetic tabs; tables describe sourced envelopes, not a claim the fit already passes.

## 4. States
The fake source must expose:
- First visit: current progression, starter stock supplied by logic, locks intact; no invented free-first-purchase promotion. Revisit; focused Firefly/Sapling/Owl Eye/Bell/Scarecrow/Rain entry, including the last row.
- Typical coins 240, stock 3/2/1/1/0/0; all six unlock combinations at thresholds 2/4/7/12/22/32 and one level below each. All locked (fresh progress) and all unlocked.
- Coins 0, one below price, exactly price, exactly one affordable item, enough for all, 99999, 2147483647; stock 0, 1, typical, 99999, 2147483647. Maximum purchase availability is logic-supplied; no UI inventory cap or integer overflow.
- Empty catalog/missing item/price/art/stock/wallet, and source error: no fabricated free item or zero price. Keep chrome and close available; missing offer is non-actionable. Empty/error visual is pending the shared state-pattern decision below.
- Each affordable item confirmation; refusal for locked/insufficient; accepted purchase (stock increases by supplied quantity, balance decreases); stale-offer errors, repeated taps, busy input, cancel/back. Exercise every confirmation state in its spec.
- Full catalog and top/bottom scroll limits, all normal/pressed/disabled states, muted/reduced-motion/instant mode, focus loss/hide during effects, long internal/display names (including localized names) with no visible name labels. Offline is identical: all catalog/economy data is local; there is no network-loading flow or offline toast.
- No gameplay selection/move/win/lose shown here; no automatic use of bought boosters; no timer/auto-dismiss.

[Decision needed: booster bundles] Meta.Shop defines single and three-pack offers; GAME_DESIGN §8.3 offers both, but current ShopView renders singles only. Options: a measured quantity selector within confirmation, or separate single/three-pack cards. Recommend selector within confirmation so this grid stays six kinds. Library owner must supply its pattern/measurements; fake source must reach quantities 1 and 3 and exact affordability for both. Existing pack prices are 75/150/200/200/250/250; the view displays catalog prices, never computes them.

[Decision needed: local empty/error presentation] No empty/error pattern or art is defined. Options: approved icon-only placeholder inside an existing measured card slot, or an approved baked status badge. Recommend icon-only placeholder, shared by all shop tabs; no invented live error paragraphs. Missing-price/unknown-item/source-error states remain explicit approval blockers, not silently treated as valid empty inventory.

## 5. Interaction map
| Input | View event / result | Feedback / sound |
|---|---|---|
| Tap/click affordable unlocked card | `OfferRequested(itemId)` → logic supplies confirmation | `button-press / release`, `ui-tap`; `panel-open` |
| Tap locked card | `OfferInspectRequested(itemId)`; logic supplies refusal, never purchase | Existing padlock/unlock number, `refuse`; refusal-motion decision below |
| Tap unaffordable card | Same inspection/refusal path | Price red + shake (`card-button`), `refuse`; never a new coin-store route |
| Tap a tab / selected tab | `TabRequested(kind)`; selected-tab repeat is idempotent | `button-press / release`, `ui-tap`; selected ring/pill moves to supplied tab |
| Tap close / Android back | `CloseRequested` / `BackRequested`; confirmation gets back first | `panel-close`; `ui-tap` only for pointer button |
| Drag/wheel | View-local scroll; never opens a card on drag release | Clamped content/indicator; sound none |
| Purchase result supplied | Refresh stock/coins once; remain on tab | `counter`, `badge-pop`; `coin-spend` at actual debit counter start |

Disabled/busy targets are fixed-size, alpha 0.45 and inert (no event/sound); locked inspection is not a purchase-enabled state. No haptics specified.

## 6. View contract
Proposed presentation boundary; current direct GameRoot access is not the target:
- `Show(ShopPresentation, origin, focusId)`, `SetTab(kind, offers, offset)`, `SetWallet(value)`, `SetOffer(OfferPresentation)`, `SetInputEnabled(bool)`, `ShowConfirmation(model)`, `ShowRefusal(itemId, reason)`, `ApplyPurchaseResult(result)`, `Hide()`.
- Logic supplies catalog ID/order/kind, quantity options, price, unlock level, stock, eligibility/affordability, unavailable reason, focus ID, wallet, origin and audio/motion settings. Sources are Meta.Shop/Unlocks and committed profile/session; update on show, tab change and transaction/profile change. Each missing field uses §4, never a guessed default. Art keys come from the catalog-to-art adapter.
- Events: §5; confirmation forwards its own purchase/cancel events. No `Purchase`/unlock arithmetic, persistence, starter grant or state classification in the view. Focus, offset and tween progress alone may be local. Completion of a visual effect never changes stock.

## 7. Motion and sound
| Trigger | Pattern / duration | Sound / music |
|---|---|---|
| Open/close | `panel-open` 0.18 s / `panel-close` 0.12 s; no child stagger | `ui-tap` for originating pointer action; keep `bgm-menu` |
| Card/tab/close press | `button-press / release` | `ui-tap` on pointer down |
| Confirm opens / closes | `panel-open` / `panel-close` | No extra invented popup sound; music unchanged |
| Purchase committed | `counter` 0.5 s, `badge-pop` 0.3 s | `coin-spend`; no `booster-<kind>` until actually used on board |
| Refusal | `card-button` red-price/shake; locked keeps lock | `refuse` |

[Decision needed: refusal motion row] Library names shake but supplies no timing/amplitude/reduced fallback. Options: promote current 0.25 s, 8 px shop shake, or approve a new shared row. Recommend promote current motion and static lock/red-price plus sound in reduced mode; add a non-color refusal icon to that row. Until specified, color alone is not acceptance. This is shared across shop tabs/confirmation. No counter or tab animation may outlive hide; mute affects UI/SFX/Music. Shop requests menu music even when entered from a level (1 s crossfade); returning restores the origin's music; opening confirmation never restarts music.

## 8. Accessibility
NOT ASSESSED: no formal contrast/accessibility/device target. Assume minimum 88x88 design-px hit target; cards and 108 discs exceed it. Prices are not tiny independent hit targets. Lock plus unlock numeral, owned count and distinct tool silhouettes avoid color-only meaning; insufficient state needs the shared refusal icon. Spoken names identify tool, quantity, cost, stock and unlock condition. Apply map's shared opaque-glyph decision; primary icons remain 64 px. Reduced/instant motion uses library fallbacks, never delays state updates.

## 9. Text
| Element | Limit / rendering |
|---|---|
| Wallet | Live 0–99999, five digits; full int maximum pending shared numeric decision |
| Stock badge | Live nonnegative int; 0 is a numeral, not a free offer; all ten digits require the shared numeric decision |
| Price | Live digits, current singles 30–100 (three), packs 75–250 (three); reserve five-digit fixture in measured number band |
| Unlock level | Catalog-fixed 2/4/7/12/22/32, at most two digits; baked badge or number-only presentation |
| Quantity | Numerals 1/3; any multiplication glyph baked; selector geometry pending bundle decision |
| Names, tab labels, descriptions, status prose | Zero visible characters; icon-only. Long internal/accessibility/localized names must not create labels. Any approved future fixed status art is baked and remeasured, not live text |

All numeric rectangles come from scaled measured inner bands, with currency icon area excluded. Fredoka, fixed role sizes, no auto-size to hide overflow. An implementation cannot declare fit from the candidate envelope alone.

## 10. Acceptance criteria
1. At 720x1280 and 1440x3200 with safe areas, all six kinds and their lock/price/count are reachable by scroll, never under tabs/close; dragging does not open confirmation.
2. All page discs are 108x108; all six card roles use one approved measured size in normal/pressed/locked/unaffordable/busy states. Disabled is alpha 0.45 and cannot purchase. Only press feedback transforms visuals temporarily; layout/hit bounds do not change.
3. Every text/numeral remains inside its measured container with margin for 99999 and the longest name fixtures; glyph opaque dimensions match their resolved role, not their canvas. Full-int values cannot pass until the shared numeric decision is resolved.
4. Locked/insufficient card never debits coins or stock; a confirmed eligible offer changes each exactly once; cancelling changes neither. Busy repeated taps create no duplicate request.
5. Confirmation visibly sits above every card/chrome/bird: black dim alpha 0.72, panel shadow offset +14 px/alpha 0.35 and no underlying input. Shop cards retain clearly bounded cream interiors against the forest.
6. Entry is fully visible in the 0.18 s panel transition with all controls present from its first frame; close takes 0.12 s, committed counter 0.5 s and badge pop 0.3 s. Later implementation must provide timed evidence, not claim the source timings as measured runtime performance.
7. `bgm-menu` is present on this page from either origin, obeys the single mute control and is not restarted by confirmation; `coin-spend` occurs on committed debit only. Bought boosters are not automatically applied.
8. Approval is blocked until card-fit, bundle, refusal and empty/error patterns plus the shared map decisions are resolved; no guessed geometry or hidden omission of three-packs counts as complete.

## Differences from the current build
- ShopView uses 140 px tabs, unequal per-row/column card dimensions, a full noon backdrop and direct root reads/actions; target uses library chrome, one measured card role and presentation calls/events. The existing local catalog and thresholds remain unchanged.
- Current cards expose single boosters only, despite catalog three-packs. Current stock is unbounded int text in 64 px badges; there is no approved maximum-value fit.
- Existing purchase counter/badge timings are reusable; cosmetic/refusal and page transitions need the same shared presentation standards. No runtime changes are made by this document.
