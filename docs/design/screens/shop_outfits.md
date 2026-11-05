# Shop — outfits

Status: **draft**. Stage: screen specification. Portrait phones; touch/Android back, mouse/vertical wheel for Editor review. Design 1080x1920, 9:16–9:20; review 720x1280 and 1440x3200 with safe areas. No keyboard/controller navigation contract; no runtime acceptance claimed.

Authority: [art direction](../art-direction.md), [patterns](../patterns.md), [brief](../../work-log/ui-redesign/brief.md), [conventions](../../CONVENTIONS.md). Measurements: `art/approved_ui/layout.json:screens.shop_outfits` (visible bounds ±8 px), `assets.aviary_frame`, and `PerchSort/Assets/Resources/Art/{layout_metrics,glyph_metrics}.json`. `ui_shop_outfits.png` supplies style and preview composition, not catalog prices or purple buttons. Shared glyph/numeric/missing-addendum decisions: [map](map.md). Shared card-fit/refusal/empty-error decisions: [boosters](shop_boosters.md).

## 1. Player need
The player arrives wanting to preview, buy or change a bird's outfit without changing gameplay. They were browsing another shop tab; outfits are cosmetic, never required progress.

## 2. Navigation
- Select hanger tab from boosters/birdhouses. Shop origin remains map or level. Tabs go to [boosters](shop_boosters.md) / [birdhouses](shop_houses.md); close-X/Android back returns to the original map/level, not the previously viewed tab. No dim-tap exit on a full page.
- Unowned eligible card → [purchase confirmation](shop_confirm.md). Cancel returns here at the same offset; success returns here with owned check, not automatic equip. A subsequent owned-card tap requests equip; tapping the equipped card requests the default outfit. No second equip-confirm popup.
- The catalog is not species-discovery gated: Meta.Shop currently permits any outfit purchase. Do not add an unseen-species lock or unlock level. Rook/crow has no outfit offers.
- Shop notifications wait until closure. Focus loss pauses motion/audio; hide cancels callbacks; reshow uses committed ownership/equipment. Logic owns return navigation and any board refresh.

## 3. Layout
Top-left design px. Same `page`/`tabs` chrome as the other shop tabs; do not establish a second control family.

| Component | Pattern / asset | Size provenance | Anchor / position | Layer |
|---|---|---|---|---|
| Forest | `page`; existing noon forest | 1080x1920 design canvas | Full screen bleed | background |
| Currency | `coin-pill`; coin + pill | Library 190x76 | (48,150), safe-area adjusted | fixed chrome |
| Close / tabs | Page `wood-disc`, `tabs`; close/bulb/hanger/home | Library discs 108; selection ring 190 and pill 70x28 | Close measured (965,197); tab centers (350,330)/(540,330)/(725,330), hanger selected | fixed chrome |
| Outfit cards | `list-grid`, `card-button`; fitted `card_wood` | Candidate uniform 476x590 from measured first card | Measured x58/552; first y456, next y1076, continuation uses measured pitch 620 | scroll content |
| Living preview | Species drawn frames with matching outfit overlays | Measured bird envelope 354x386 | Measured first centers (295,699)/(789,699); row pitch 620; preserve aspect and feet anchor | card content |
| Ownership / equipment | Owned check in price area; `aviary_frame` for equipped | Check uses shared glyph normalization; ring native measured 320x320 | Check replaces price, ring centered behind preview; never covers face | card content |
| Unowned price | `card-button`; coin + `btn_green_pill` | Measured outfit price 278x100 | Measured first centers (301,952)/(795,952), subsequent rows +620 | card content |
| Confirmation | `modal-panel` | Confirmation spec | Above all chrome/animated previews | overlay |

Use measured viewport envelope (58,456,964,1218), intersect safe area, keep below tabs, right scroll indicator. Two columns, 27 offers in catalog order: 14 rows, last row one card; no invented empty purchasable card. All cards of the outfit role share the same approved size in all states. The stock square frame is not stretched to these candidate rectangles: resolve the shared fitted-card decision and publish actual inner bounds first. Overlay artwork must fit at every pose, not only idle. Price/owned area is a display child of one card hit target, not a second primary button.

## 4. States
The fake source/state switcher must reach:
- First shop visit, no outfits owned, none equipped; later visit with mixed ownership; every species' three offers, including unencountered species purchasable and no crow offers.
- Typical 240 coins (none affordable), enough for one 300-coin item, one below/exactly each 300/500/800 price, 0, 99999, 2147483647. Price five-digit stress and missing price. Shared numeric decision governs maxima; never cap the saved balance.
- Unowned/affordable; unowned/insufficient; owned/unequipped; owned/equipped; all 27 owned; maximum nine simultaneously equipped (one per wearable species); swapping two outfits of one species; toggling back to default; other species unchanged.
- Empty catalog versus all owned (not the same state), one offer, all 27, missing preview/pose/overlay, missing ownership/equipment/wallet, local source error. Unknown equipment ID must not mark an arbitrary card. Missing eligibility data disables purchase/equip; shared empty/error presentation remains an approval dependency.
- Normal/pressed/disabled/busy cards; rejected stale purchase/equip; repeated taps; all confirmation states from its spec for representative species and every price tier. Equip results include `NotOwned`, `UnknownItem`, `NotEquippable` without optimistic visual success.
- Scroll first/middle/last singleton row; switching tabs and returning; confirmation/back retains this offset; manual drag never buys/equips. Every bird stays within its card while breathing/acting.
- Longest species/cosmetic names and oversized localized metadata; normal/reduced/instant motion; mute/unmute; focus loss/hide during panel/purchase/equip effects. No live name text is added. Offline equals online because data/transactions are local; no loading spinner/network error. No gameplay moves/win/lose, timers or automatic dismissal.

## 5. Interaction map
| Input | Event / result | Feedback / sound |
|---|---|---|
| Tap/click unowned eligible card | `OfferRequested(itemId)` → supplied confirmation | `button-press / release`, `ui-tap`; `panel-open` |
| Tap unowned insufficient/unavailable card | `OfferInspectRequested(itemId)`; controller supplies reason | `card-button` price/refusal feedback, `refuse`; never auto-opens coin store |
| Tap owned unequipped card | `EquipRequested(itemId)`; controller supplies new equipment model | `button-press / release`, `ui-tap`; equipped ring appears only after acceptance |
| Tap equipped card | `DefaultOutfitRequested(speciesId)` | Same press/release/`ui-tap`; ring removed only after accepted update |
| Tab / close / Android back | `TabRequested(kind)` / `CloseRequested` / `BackRequested` | Pattern press/release, `ui-tap` for pointer; `panel-close` on exit; back closes confirmation first |
| Drag/wheel | View-local clamped scroll; release does not select | Indicator/content movement; sound none |
| Accepted purchase / rejected result | Apply supplied ownership/wallet or refusal | `counter` + `badge-pop`, `coin-spend` only on committed purchase; failure `refuse` |

Busy/disabled targets are alpha 0.45, same footprint, no events or tap sound. No haptics. Cosmetic equip does not emit a coin sound, booster SFX or purchase popup.

## 6. View contract
Proposed boundary, not existing API names:
- `Show(OutfitShopPresentation, origin, offset)`, `SetOffers(offers)`, `SetWallet(value)`, `SetOwnership(itemId,owned)`, `SetEquipment(speciesId,itemIdOrDefault)`, `SetInputEnabled(bool)`, `ShowConfirmation(model)`, `ShowRefusal(itemId,reason)`, `Hide()`.
- Adapter supplies all 27 IDs/order, species IDs, portrait/animation/overlay art keys, price, ownership/equipped/eligibility/reason, wallet, origin and settings. Catalog source: Meta.Shop/Aviary; committed collection source: profile ledger. Update on show, tab change, purchase, equip/default and profile change. Missing fields use §4; no UI inference from price/check color or sprite filename.
- Events are every action in §5. Controller, not view, enforces one outfit per species, default toggles, affordability and persistence; view never reads GameRoot.Collection or calls Meta.Shop.Purchase/Equip. It may retain offset, selected presentation ID and animation phase only.
- Missing animation/overlay is reported as an asset gap, not disguised by mirroring a still. Completion events are presentation-only and cannot grant/equip anything.

## 7. Motion and sound
| Trigger | Pattern / timing | Sound / music |
|---|---|---|
| Page open/close | `panel-open` 0.18 s / `panel-close` 0.12 s; no child stagger | `bgm-menu`; 1 s page crossfade, no tab restart |
| Card/tab/close | `button-press / release` | `ui-tap` |
| Purchase result | `counter` 0.5 s / `badge-pop` 0.3 s on owned cue | `coin-spend` at counter start |
| Equip/default accepted | Replace supplied equipment cue/overlay, no invented transition | Original action `ui-tap`, no second sound |
| Rejection | Shared refusal-pattern decision in boosters | `refuse` |
| All visible birds | Breath 2.4–3.2 s, drawn actions every 1–3 s at source 10–14 fps | No new idle sound required |

Birds face the viewer at rest; outfit follows every frame/anchor. Never mirror or crossfade poses; no static cosmetic card accepted as an animated bird. Cancel offscreen/hidden presentation safely. Reduced/instant motion follows library fallbacks without blocking input; focus loss pauses audio, mute silences all groups. Confirmation keeps the current menu track; returning to the board restores game music.

## 8. Accessibility
NOT ASSESSED: no formal contrast/device/accessibility standard. Assume at least 88x88 design-px touch targets; cards and 108 discs exceed it. Owned=check, equipped=ring plus owned check, unaffordable=shared non-color refusal cue, never just green/red. Spoken name includes species/outfit, cost or owned/equipped state. No smaller disabled cards. Shared glyph-size decision applies; target glyph bounds come from `glyph_metrics.json`, not transparent canvas. Reduced motion cannot change equip/purchase results.

## 9. Text
| Element | Limit / rendering |
|---|---|
| Wallet | Live digits, 0–99999 (five); full integer maximum pending shared measured numeric policy |
| Unowned price | Live 300/500/800 (three digits); five-digit stress fixture; measured pill number band only |
| Owned/equipped/default labels | Zero characters; check/ring/absence of ring, not live words |
| Species/outfit/tab names and descriptions | Zero visible characters; long/localized names are metadata/accessibility only, never a live ribbon added to these cards |
| Modal text | Confirmation spec |

Fixed text, if later approved, must be baked into its measured image; live text is numbers only, Fredoka with fixed role size. No auto-shrink to make overflow look valid.

## 10. Acceptance criteria
1. At 720x1280 and 1440x3200 with safe areas, all 27 offers including the singleton last row are reachable, nothing under fixed chrome receives card input, and every pose stays within its card.
2. All outfit cards have one approved measured size in unowned/owned/equipped/insufficient/disabled states; all page discs are 108x108. Disabled alpha 0.45 is inert; press feedback does not change layout/hit bounds.
3. No text/numeral leaves its measured container for 99999 coins, five-digit prices or long-name metadata; same-role glyph opaque dimensions are uniform despite source padding. Full integer maxima require shared numeric approval, not clipping.
4. Owned tap equips only the selected species; equipped tap restores its default; another species is unchanged. An unowned or rejected action cannot acquire an equipped ring, and buying alone does not equip.
5. Cards visibly separate cream content/wood rims from forest; confirmation has black dim alpha 0.72 and shadow offset +14 px/alpha 0.35 above every bird/chrome object and blocks underlying input.
6. Page open completes in 0.18 s, close in 0.12 s; committed purchase counter 0.5 s, ownership cue pop 0.3 s. Ten seconds of normal-motion evidence shows continuous breathing plus drawn idle actions every 1–3 s without mirrored stills. These are later runtime checks, not results of this docs task.
7. Close/back after purchase/equip returns to the original map/level with committed state and no restarted level; confirmation cancel preserves tab/offset and wallet. Menu music stays present and follows mute/focus loss.
8. Empty catalog, unavailable art and missing wallet never produce free purchases or optimistic equipment; all-owned remains a functional equip catalog. Shared card-fit/error/refusal/glyph/numeric decisions must be resolved before approval.

## Differences from the current build
- Current ShopView uses 455x575 cosmetic cards, 380-square static previews, 140 tabs and a check/ring composed by direct collection reads. Target uses measured outfit envelopes, pattern chrome, living front-facing previews and a data/event contract.
- Current viewport starts at y450 with 1200 height and does not expose the library scroll indicator contract. Target uses sourced viewport bounds and verifies the final row/safe-area clipping.
- Retain the actual 300/500/800 catalog prices, direct owned-card equip/default behavior and lack of species-discovery purchase locks. The reference's 60/80 prices do not replace the economy.
