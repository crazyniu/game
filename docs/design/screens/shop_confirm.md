# Shop — purchase confirmation

Status: **draft**. Stage: screen specification. Portrait phones; touch/Android back, mouse in Editor; no keyboard/controller traversal contract. Design 1080x1920, 9:16–9:20; review 720x1280 and 1440x3200 with safe areas. This is a modal, not a receipt or new shop page.

Authority: [art direction](../art-direction.md), [patterns](../patterns.md), [brief](../../work-log/ui-redesign/brief.md), [conventions](../../CONVENTIONS.md). Measured reference: `art/approved_ui/layout.json:screens.shop_confirm` (±8 px manual visible bounds), `ui_shop_confirm.png`; container/glyph measurements: `PerchSort/Assets/Resources/Art/{layout_metrics,glyph_metrics}.json`. Purple discs and round confirm in the reference are superseded. Shared unresolved glyph/numeric/source decisions are in [map](map.md); card/refusal/error/bundle decisions are in [boosters](shop_boosters.md).

## 1. Player need
The player arrives wanting to check the selected item and coin cost before spending. They just selected an unowned cosmetic or an eligible booster offer. Owned cosmetics equip directly and do not open this purchase modal.

## 2. Navigation
- Opens over boosters, outfits or birdhouses, retaining origin tab, focus and scroll offset. Only one confirmation at a time; all content beneath is input-blocked.
- Confirm raises a purchase request; only logic decides/commits success. Success closes just this modal and updates the originating card/wallet. Cosmetic purchase does not silently equip it; that remains a subsequent explicit card action.
- Close-X/Android back before submission cancels without spending. Dim tap is consumed, not a dismissal or purchase. No tap-through to a card or page close.
- During a submitted synchronous local transaction, ignore duplicate confirm/cancel/back until the result; there is no network wait/retry loop. Once resolved, back works normally. Notifications remain queued behind shop.
- Focus loss pauses motion/music. Hide destroys transient presentation, never rolls back a committed purchase; re-entry shows latest supplied state, with no replayed debit or stale completion callback.

## 3. Layout
Top-left design px. Reuse `modal-panel`, `close-x`, `primary-bar`; do not duplicate their definitions. Dim covers full screen including every animated bird and shop header.

| Component | Pattern / asset | Size source | Anchor / position | Layer |
|---|---|---|---|---|
| Dim | `modal-panel`; black | Full screen, library alpha 0.72 | Screen stretch, outside safe area too | above shop |
| Panel + shadow | `modal-panel`; `panel_wood` or measured derived frame | Reference candidate 568x652, **not a valid final fit**; decision below | Reference center (536,998), safe-area centered/clamped | above dim |
| Product preview | Booster/tool art, living outfitted bird, or house-style art | Reference envelope 302x294 | Reference center (543,881); preserve aspect, recompute from approved panel inner rect | above panel |
| Price receipt | `coin-pill` display treatment; `coin_pill`, coin | Reference envelope 316x104 | Reference center (538,1084); measured number band, not whole image | above panel |
| Close | `close-x`; wood + `icon_close` | Library 100x100 | Panel top-right, never covered by preview/leaf decoration | top control |
| Confirm | `primary-bar`; `btn_green_pill`, `icon_check` | Library 520x120, icon 64 | One standalone footer action; fully inside measured cream with at least 28 px side/bottom clearance | above panel |
| Booster quantity choice | Pattern pending bundle decision in boosters | No measured control exists | Between preview and receipt only after measured fit approval | panel content |

The close at the corner is not a second footer action/sibling primary; never add competing green bars. Leave the origin's wallet visible behind dim rather than invent another live balance label. All children must respect `modal-panel` measured inner rect and its 24 px content clearance.

[Decision needed: confirmation panel fit] The measured 568-wide panel cannot contain a 520-wide bar plus two 28 px margins even before its wood frame (576 required). Current runtime uses 640x740; native `panel_wood` has inner rect (88,119,645,317) on 800x540 and authored nine-slice borders (150,100,150,110). Neither its stretched nor sliced final inner geometry has been measured here; a naive linear mapping gives only 516 inner width. Options: measure/reflow a wider/taller composition using the existing nine-sliced panel, or revise the global primary pattern. Recommend reflow/re-measure while retaining the 520x120 primary; request derived art only if the reusable frame cannot fit. Final panel/child rectangles must be published by the library/asset owner before shell implementation; no guessed enlargement or shrinking the bar to hide the conflict. Quantity selector also depends on this fit.

## 4. States
Fake source must reach:
- Every one of the six booster kinds; quantity 1 and 3 pending selector pattern; all 27 outfits and four houses by catalog identity. Typical selected Firefly at 30 coins; representative outfit 300/500/800 and each house 600. First confirmation and later confirmations have identical behavior, no promotion.
- Wallet 0, one below cost, exactly cost, 99999 and 2147483647; offer price five-digit stress; stock 0, typical and integer maximum supplied by logic. Unaffordable/locked/maximum-stock states must not submit. Show these also as stale changes while a previously valid modal is open.
- Product available/unavailable, missing model, removed/unknown ID, missing preview/price, empty catalog. Missing price is never rendered as zero or made actionable. Shared empty/error presentation in boosters applies; close remains available.
- Idle, pointer down/release outside (cancel press), eligible confirm, busy/double tap, committed success, failure. Results cover `NotEnoughCoins`, `Locked`, `AlreadyOwned`, `UnknownItem`; `NotOwned`/`NotEquippable` are cosmetic equip results and must not masquerade as purchase failures. Recoverable insufficient/locked state disables confirm until logic supplies a valid offer; unavailable/already-owned offer returns to refreshed originating card, no debit.
- Cancel-X, system back, dim tap, origin tab beneath modal tapped/scrolled (blocked), background/hide during open/submission/result/close, normal/reduced/instant mode and muted/unmuted audio.
- Long catalog/accessibility names and localized metadata; no live name label appears. Outfitted-bird preview remains front-facing and continuously animated. No win/lose/gameplay move here, no auto-dismiss timer, no offline wait (transaction is local).

## 5. Interaction map
| Input | Event / result | Feedback / sound |
|---|---|---|
| Tap/click confirm, eligible | `PurchaseRequested(offerId, quantity)` once; controller supplies busy then result | `button-press / release`, `ui-tap`; no debit effect yet |
| Supplied busy/disabled confirm | No request accepted | Same footprint, alpha 0.45; no tap sound/event |
| Quantity selection, once approved | `QuantityRequested(quantity)`; controller replaces quote/eligibility | Library control feedback and `ui-tap`; exact motion pending its pattern, no local price arithmetic |
| Close-X / Android back while idle | `CancelRequested` / `BackRequested`; restore shop | `panel-close`; `ui-tap` for X, none for system back |
| Dim tap, shop input under modal | No event | Modal stays visible; sound none |
| Success result | `Close()` from controller; card/wallet update in origin | `panel-close`, origin `counter`/`badge-pop`, `coin-spend` at committed counter start |
| Failure result | `ShowUnavailable(reason)` or controller closes to refreshed card | `refuse`; shared refusal/empty-error pattern; no coin sound, stock change or fake receipt |

No haptics specified. Feedback sounds use the pattern tables; there is no new `sfx_purchase`/popup jingle and no booster effect sound on purchase.

## 6. View contract
Proposed shell API, replacing direct root transactions:
- `Show(ConfirmationPresentation)`: controller supplies stable item/offer ID, kind, quantity, preview/frame art keys, price, eligibility/reason, owned/stock availability, origin identity, motion/audio flags. Catalog is Meta.Shop; wallet/unlocks/ownership are profile/session data supplied through the adapter. Missing fields use §4, never invented labels/default prices.
- `SetOffer(model)` on quantity/profile/catalog changes; `SetBusy(bool)` around request handling; `ShowUnavailable(reason)` on rejected/stale request; `Close()` / `Hide()` on controller navigation. Model update, not UI arithmetic, changes cost and confirm eligibility.
- Events: `PurchaseRequested`, `QuantityRequested`, `CancelRequested`, `BackRequested`, optional presentation completion/cancellation. No wallet/profile mutation, rollback, auto-equip, grant or retry in the view. Selection is a presentation identity, not a pending game-state transaction.
- Success is a committed result from logic; animation completion is not success. On dismissal, remove event handlers/tweens and block stale callbacks. Repeated pointer input cannot submit the same busy request twice.

## 7. Motion and sound
| Trigger | Pattern / timing | Sound / music |
|---|---|---|
| Open/close | `panel-open` 0.18 s / `panel-close` 0.12 s, library easing; all controls present immediately | Originating `ui-tap`; existing `bgm-menu` continues, no restart/duck invented |
| Press/release | `button-press / release` | `ui-tap` on accepted pointer down |
| Success | Close plus origin `counter` 0.5 s / `badge-pop` 0.3 s | `coin-spend` only after commit |
| Rejection | Shared refusal row pending in boosters | `refuse`; no music change |
| Bird preview idle | Breathing period 2.4–3.2 s, drawn actions every 1–3 s; 10–14 fps source sequences | No additional chirp required |

No per-control stagger or auto-dismiss. `GameMotion.Instant` and reduced-motion pattern fallbacks cannot hold the shop busy. All sound follows the single mute control; focus loss pauses audio, first input retries playback per conventions.

## 8. Accessibility
NOT ASSESSED: no formal contrast/accessibility target. Assume minimum 88x88 design-px targets; close is 100 and confirm 520x120. Nonvisual names identify product, quantity, price and confirm/cancel intent. Lock/owned/unavailable states need shape/icon cues, not red alone (shared refusal-pattern blocker). Apply shared glyph normalization from map; check icon stays 64 px opaque maximum dimension on the primary, secondary glyphs uniform after decision. Reduced motion does not change purchase semantics.

## 9. Text
| Element | Limit / rendering |
|---|---|
| Price | Live numeric, normal catalog 30–800 (three digits), five-digit stress fixture; fixed numeric role, no auto-shrink |
| Quantity | Live 1/3; multiplication mark baked if used; selector geometry unresolved |
| Product name/description / confirm/cancel / error prose | Zero visible characters; icon-only. Long names affect accessible metadata only. Any subsequently approved fixed error badge must be baked/re-measured |
| Wallet/stock visible through dim | Owned/rendered by origin spec; not duplicated or re-laid out here |

Price number must use the measured currency right-hand region, excluding coin art. `coin_pill` native 320x128 inner (102,27,115,74) is the measured starting point; a wider number area cannot be inferred from transparent canvas. Shared numeric-fit decision remains required.

## 10. Acceptance criteria
1. At 720x1280 and 1440x3200 with notch/home-indicator simulation, close and confirm remain wholly visible and usable; dim covers every pixel behind the modal and consumes input.
2. The panel visibly separates from the shop with black dim alpha 0.72 and shadow offset +14 px/alpha 0.35; no card, tab, counter or animated bird renders above it.
3. Close stays 100x100 and confirm 520x120 in every state; disabled alpha 0.45 is inert. Confirm has at least 28 px cream clearance at both sides/bottom. This criterion cannot pass until panel-fit approval; never substitute a smaller button.
4. No numeral/text leaves its measured container at five-digit cost or long-name fixtures; primary check glyph is 64 px opaque maximum dimension and same-role glyphs are uniform despite different canvas padding.
5. Exactly-cost purchase emits one request and debits the controller-provided cost once; double-tap during busy cannot duplicate it. Cancel/back/dim tap do not debit. Stale locks/ownership/wallet cannot be bypassed by keeping the modal open.
6. Successful purchase returns to the same origin tab/offset and updates stock/ownership and wallet; no automatic booster use or cosmetic equip. Rejected purchase gives refusal feedback without coin-spend audio.
7. Open completes in 0.18 s and close in 0.12 s; counter effects occur in the origin over 0.5 s after commit. Ten seconds of outfit-preview observation shows continuous breathing and 1–3 s drawn actions without mirroring. Timed runtime evidence is a later-stage requirement.
8. Muting silences UI/SFX/music; menu music continues through opening/closing without restart. Hide/background cannot leave stale tweens or a duplicate transaction on return.

## Differences from the current build
- Current ShopView creates a 640x740 panel with preview/price/manual positions and a 520-wide primary without proving measured cream fit. Its opening scale starts at 0.9; target uses the shared `panel-open` pattern and a verified panel composition.
- Current confirmation only selects singles, calls BuyBooster/BuyCosmetic directly, disposes instantly on close, and has no explicit busy/stale-result presentation contract. Target delegates eligibility/transactions and requires standard close/cancellation behavior.
- Current fixed-size cosmetic preview is a sprite; target requires a living front-facing bird for outfit previews. Reference round green confirm/purple X are not retained.
