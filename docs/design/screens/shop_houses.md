# Shop — birdhouses

Status: **draft**. Stage: screen specification. Portrait phones; touch/Android back and mouse/vertical wheel for Editor review. Design 1080x1920, 9:16–9:20; review 720x1280 and 1440x3200 with safe areas. No keyboard/controller navigation contract or runtime acceptance claimed.

Authority: [art direction](../art-direction.md), [patterns](../patterns.md), [brief](../../work-log/ui-redesign/brief.md), [conventions](../../CONVENTIONS.md). No approved birdhouse-shop reference is supplied. Reuse the measured cosmetic-page composition in `art/approved_ui/layout.json:screens.shop_outfits` (±8 px visible-bound uncertainty), not a guessed new layout; existing previews are `Resources/Art/house_style_{mushroom,teapot,lantern,acorn}_body`. Container/glyph measurements: `PerchSort/Assets/Resources/Art/{layout_metrics,glyph_metrics}.json`. Shared glyph/numeric/source decisions: [map](map.md); shared card-fit/refusal/empty-error decisions: [boosters](shop_boosters.md).

## 1. Player need
The player arrives wanting to decorate the birds' home, compare styles and choose one they own. They were browsing the shop. This changes appearance only, never rules, unlock requirements or the number of goal houses.

## 2. Navigation
- Select the house tab from boosters/outfits. Close-X/Android back closes the whole shop to its original map/level; tabs switch to [boosters](shop_boosters.md) or [outfits](shop_outfits.md), not the aviary.
- Tap unowned eligible house → [purchase confirmation](shop_confirm.md); cancel/back returns here at the same offset, success marks owned without automatically equipping.
- Tap owned house requests equip; tap equipped house requests default style. There is no paid default card or second confirmation for equip. Only one global style is equipped; it applies to the game's single goal house wherever applicable, not a row of new houses. Doorway occupancy remains gameplay-supplied, never a shop-side delivered-flock state.
- No style unlock levels/world locks: Meta.Shop currently has none. Insufficient balance refuses rather than redirecting to ads/cash purchases.
- Notifications wait for shop closure. Focus loss pauses audio/motion; hide cancels visual work; re-entry uses committed collection/wallet. Dim-tap does not close a full page.

## 3. Layout
Top-left design px. Use the same page chrome and two-column cosmetic-card role as outfits, not the current build's shorter special house layout. This reuses measured dimensions; visual approval is still pending below.

| Component | Pattern / asset | Size provenance | Anchor / position | Layer |
|---|---|---|---|---|
| Forest | `page`; existing noon forest | Design canvas 1080x1920 | Full screen bleed | background |
| Currency | `coin-pill`; pill + coin | Library 190x76 | (48,150), safe-area adjusted | fixed chrome |
| Close / three tabs | Page `wood-disc`, `tabs`; close/bulb/hanger/home | Library 108 discs, selected ring 190/pill 70x28 | Close measured center (965,197); tab centers (350,330)/(540,330)/(725,330), house selected | fixed chrome |
| Four house cards | `list-grid`, `card-button`; fitted `card_wood` | Candidate 476x590, measured outfit first card, same cosmetic-card role | Measured x58/552, y456/1076; order mushroom, teapot, lantern, acorn | scroll content |
| House preview | Existing style body, aspect-preserving | Fit within borrowed measured 354x386 preview envelope | Same preview centers (295,699)/(789,699), second row +620 | card content |
| Owned/equipped cues | Check in price area / `aviary_frame` behind preview | Shared glyph normalization; ring native measured 320x320 | Price center / preview center; never cover door/style silhouette | card content |
| Unowned price | `card-button`; coin + `btn_green_pill` | Measured cosmetic price 278x100 | Measured first centers (301,952)/(795,952), next row +620 | card content |
| Confirmation | `modal-panel` | Confirmation spec | Above all chrome/cards | overlay |

Viewport uses borrowed measured cosmetic envelope (58,456,964,1218), intersects safe area and remains below tabs; right scroll indicator. Both rows must be reachable, even if safe area requires scrolling. Card is one hit target; price/owned area is a display child, not a competing primary action. Empty door is intentional product art, not a missing bird. No resident bird is added to these catalog previews; if later approved, it requires the same continuous breathing/drawn-action contract, never a static pasted bird.

[Decision needed: birdhouse visual approval] There is no `ui_shop_houses.png` among the supplied approved references. Options: approve the reused measured outfit-card composition with the four existing house bodies, or supply a separately measured house-page composition. Recommend reuse; do not generate a new layout, art or guessed size in this docs task. Resolve the shared fitted-card inner bounds before implementation; existing native square frame cannot simply be stretched into the candidate envelope.

[Decision needed: hanger tab asset] ShopView requests `icon_hanger`, but the inspected Art asset glob and `glyph_metrics.json` contain no such sprite/entry; GameArt loads that exact resource name and does not alias it. Options: approve deriving/measuring a standalone hanger from the approved shop reference, or approve another measured outfit-tab glyph. Recommend derived hanger to preserve the approved symbol; library/asset owner supplies opaque bounds before all three shop tabs can pass glyph consistency. No art is created here.

## 4. States
Fake source/state switcher must reach:
- First visit: four unowned houses, default equipped; later visit with some owned. Typical 240 coins (all unaffordable), 0, 599, exactly 600, enough for all four, 99999 and 2147483647; five-digit price stress fixture.
- Each of four houses unowned/affordable, unowned/insufficient, owned/unequipped, owned/equipped. All four owned, default equipped; maximum one alternate equipped; switch each pair and restore default. No world/species lock is invented.
- Empty catalog, one card, all four, unknown/missing style ID, missing preview/price/wallet/ownership/equipment, source error. Missing eligibility is non-actionable; missing price is not zero. Shared empty/error pattern applies. All-owned is still an equip catalog, not empty.
- Every house opens confirmation and all its cancel/busy/stale/success/failure states. Equip/default rejection (`NotOwned`, `UnknownItem`, `NotEquippable`), changed ownership/balance while confirmation is open, repeated taps, normal/pressed/disabled states.
- Both viewport ends, drag versus tap, tab switch, confirmation return at same offset; notch/home-indicator at both review endpoints. Longest/localized house and tab metadata creates no live labels.
- Normal/reduced/instant motion, mute/unmute, focus loss/hide during transition/purchase/equip. Offline behaves identically because all data/transactions are local; no network-loading state. No gameplay select/move/win/lose, timers or auto-dismiss.

## 5. Interaction map
| Input | Event / result | Feedback / sound |
|---|---|---|
| Tap/click eligible unowned house | `OfferRequested(itemId)`; logic supplies confirmation | `button-press / release`, `ui-tap`, then `panel-open` |
| Tap insufficient/unavailable card | `OfferInspectRequested(itemId)`; logic supplies refusal | Shared `card-button` refusal (red price/shake plus resolved non-color cue), `refuse` |
| Tap owned unequipped house | `EquipRequested(itemId)` | Press/release, `ui-tap`; equipped ring only on accepted update |
| Tap equipped house | `DefaultHouseRequested` | Press/release, `ui-tap`; ring removed only after accepted default update |
| Tab / close / Android back | `TabRequested(kind)` / `CloseRequested` / `BackRequested` | Pattern press/release and `ui-tap` for pointer; `panel-close` on exit; back reaches confirmation first |
| Drag/wheel | View-local clamped scroll; no card activation on release | Content/indicator movement; sound none |
| Accepted purchase / rejected result | Controller supplies ownership/wallet or reason | `counter`, `badge-pop`, `coin-spend` only after commit; failure `refuse` |

Disabled/busy is alpha 0.45, identical footprint, no event/sound. No haptics, purchase sound on equip, booster-use sound or house-specific jingle is specified.

## 6. View contract
Proposed shell boundary, not a change to persistence identifiers:
- `Show(HouseShopPresentation, origin, offset)`, `SetOffers(offers)`, `SetWallet(value)`, `SetOwnership(itemId,owned)`, `SetEquipment(styleIdOrDefault)`, `SetInputEnabled(bool)`, `ShowConfirmation(model)`, `ShowRefusal(itemId,reason)`, `Hide()`.
- Data: four stable catalog item/style IDs/order, body art keys, price, owned/equipped/eligibility/reason, wallet, origin, mute/reduced-motion. Meta.Shop provides offers; collection ledger provides ownership/equipped style; controller supplies availability. Update on show/tab switch/purchase/equip/profile changes. Missing data follows §4; unknown equipped ID never highlights an arbitrary card.
- Events are every action in §5. Controller performs affordability checks, purchase, equip/default, global-style fan-out and save. View does not call Meta.Shop or inspect GameRoot.Collection, decide a style lock, or set goal-house occupancy. Offset, highlight and animation phase may be local presentation state only.
- Purchase/equip effects render committed results; completing or cancelling an animation never owns/awards a style. Returning to a level preserves its session; no reset to make the new house visible.

## 7. Motion and sound
| Trigger | Pattern / timing | Sound / music |
|---|---|---|
| Page open/close | `panel-open` 0.18 s / `panel-close` 0.12 s; all controls present from first frame | `bgm-menu`, 1 s page crossfade, no tab restart |
| Card/tab/close | `button-press / release` | `ui-tap` |
| Purchase committed | `counter` 0.5 s, `badge-pop` 0.3 s on owned cue | `coin-spend` at counter start |
| Equip/default accepted | Replace supplied equipment ring; no unapproved house transform | Original action `ui-tap`, no second sound |
| Refusal | Shared refusal-motion decision in boosters | `refuse` |

House bodies are scenery/product art and need not breathe; never substitute their animation for bird motion. Modal transitions follow confirmation spec; mute silences all groups, focus loss pauses audio and first input retries it. Reduced/instant motion follows library fallbacks; hide cancels pending effects. Returning to board restores `bgm-game`; confirmation never restarts/ducks menu music.

## 8. Accessibility
NOT ASSESSED: no formal project contrast/accessibility/device target. Assume 88x88 design-px minimum hit targets; cards/108 discs exceed it. Owned=check, equipped=ring plus check; distinct silhouettes identify houses, not color alone. Insufficient/error uses the shared non-color refusal cue before acceptance. Spoken names identify style, cost and equipment state; no small display pill is a separate hit target. Apply shared opaque-glyph normalization/size decision and the hanger-asset prerequisite. Reduced motion does not change transactions.

## 9. Text
| Element | Limit / rendering |
|---|---|
| Wallet | Live digits 0–99999 (five); full nonnegative int maximum pending shared numeric policy |
| Price | Live 600 (three digits); five-digit stress fixture inside measured number band |
| House/style/tab names, owned/equipped/default words, descriptions | Zero visible characters; metadata/accessibility only; no long name may spill into the layout |
| Modal labels/numbers | Confirmation spec |

All future approved fixed text must be baked/re-measured; live text only numbers. Keep numeric type size fixed by role, never shrink it to conceal overflow. Coin/price inner regions come from `layout_metrics.json`, not full canvas bounds.

## 10. Acceptance criteria
1. At 720x1280 and 1440x3200 with safe-area simulation, all four styles, both rows and page close are accessible; scrolling never activates a card beneath tabs/chrome.
2. House and outfit cosmetic-card roles use the same approved measured dimensions; all page discs are 108x108 in every state. Disabled alpha 0.45 is inert, not smaller; press changes visuals, not layout/hit bounds.
3. No numeral/text leaves its measured container at 99999 coins, five-digit price or long-name fixtures. Same-role glyph opaque dimensions are uniform despite canvas padding; hanger asset/metrics must exist before acceptance.
4. Only logic-confirmed purchase changes balance/ownership, exactly once. Owned tap equips one global style, equipped tap restores default, and neither spends coins. Unowned/unknown/rejected actions cannot acquire an equipped ring.
5. Cream card panels/wood rims are clearly separated from forest; confirmation has black dim alpha 0.72 and shadow offset +14 px/alpha 0.35 above every shop object, blocking underlying input.
6. Page open completes in 0.18 s, close in 0.12 s, committed wallet count in 0.5 s and ownership cue pop in 0.3 s; no child stagger. Timed evidence belongs to later runtime acceptance, not this docs delivery.
7. Closing after equip returns to the original map/level with progress/session unchanged and the committed cosmetic applied to the single goal-house presentation; purchase alone does not auto-equip or add more houses.
8. Menu music is present from either origin, follows mute/focus loss and does not restart on tab/modal changes. Empty/error data never creates a free style or hides the close control. Reused composition and shared card/error/refusal/glyph/numeric decisions must be approved before implementation acceptance.

## Differences from the current build
- ShopView currently uses a special 455x415 house card, 290-square static preview, 1360-high viewport and 140 tabs. Target reuses the measured cosmetic-page role and library chrome; native preview aspect is preserved.
- Current ownership/equipment are read and toggled directly from GameRoot; target presents controller-supplied state and raises purchase/equip/default events. Retain four 600-coin styles, default reset, no new locks and no automatic purchase equip.
- No supplied approved image settles this page's composition; the reused layout is an explicit recommendation pending approval, not a fabricated house-page baseline.
