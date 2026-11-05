# Aviary species detail

**Status:** draft. **Stage:** screen specification. Portrait iOS/Android phones; touch, Editor mouse, Android back. Reference 1080 × 1920; supported 9:16–9:20; review at 1080 × 1920 and 1080 × 2400, with safe-area insets. This is not implementation or runtime approval.

## 1. Player need

The player arrives wanting to know a met bird's personality, gift and homecoming count, and choose an owned outfit. They just selected its card in the aviary list.

## 2. Navigation

Met list card → detail. Close-x and Android back return to the same list/scroll position, not directly to the map. Background/coin taps do nothing. Locked or unknown species cannot enter; stale requests return to list without changing collection. No shop, purchase confirmation, goal-info or new-friend popup is opened by detail. Focus loss pauses input/audio; resume keeps species/look. Navigating away cancels timers and bird animation.

## 3. Layout

Reuse [patterns](../patterns.md): `page`, `modal-panel`, `name-ribbon`, `card-button`, `gift-coin`, `count-chip`, `coin-pill`, `close-x`. `R` below is `art/approved_ui/layout.json` → `screens.aviary_detail.elements` (observed visible bounds, ±8 px). `M` is `PerchSort/Assets/Resources/Art/layout_metrics.json`. Positions are top-left design px. Reference rectangles are fit boxes, never permission to distort sprites. Map measured inner rects through their actual uniform fit; maintain 24 px child margins.

| Component | Asset | Size / anchor / position | Layer |
|---|---|---|---|
| Page forest | existing page background | full safe canvas | 0 |
| Name ribbon | `labels/label_name_<species>` | `name-ribbon`, max 640 × 200; fit at R.name_ribbon center `(534,291)`; M.ribbon_green inner `(34,23,569,153)` | 3 |
| Personality line | baked species-specific label, production asset required | measured R.description box `(222,378,638,56)`, fit without stretching | 3 |
| Living bird | species drawn sequences plus outfit overlays | fit R.hero_bird `(132,446,796,734)`; feet meet panel top; front-facing rest | 2 |
| Wood/cream panel and shadow | `panel_wood` or approved derived panel after decision below | fit R.panel `(54,1148,964,708)`; current M.panel_wood native 800 × 540, inner `(88,119,645,317)` | 1 |
| Outfit strip | `card_wood`, default portrait, three outfit previews, padlock/check | `card-button`; measured R.outfit boxes 240 × 244, centers x 280/542/804, y 1368; fourth choice scrolls into strip, never becomes a smaller chip | 3 |
| Gift row | species gift coin + baked two-word effect | `gift-coin` 84 px; text fit R.effect_text 406 × 62; keep row within panel interior | 3 |
| Home count row | `house_count_chip` | `count-chip`; native measured 180 × 76; inner `(26,14,127,48)`, number in right region | 3 |
| Coin / close | `coin_pill`; `btn_wood`, `icon_close` | `coin-pill` and `close-x` pattern sizes/anchors; close safe top-right inset 24 px, not R's old purple disc | 4 |

**[Decision needed] Detail panel interior.** Uniform-fitting today's `panel_wood` into R.panel does not reproduce the reference's tall cream interior; current build's 1000 × 560 panel also places children outside its measured inner rect. Options: derive a panel from the approved detail reference and measure it, or reflow a scrolling panel inside today's measured interior. **Recommend the derived tall panel**, keeping the approved hero/slot/gift/count hierarchy. Final inner rect and row coordinates must be measured before shell implementation; do not invent them here.

**[Decision needed] Default look presentation.** Runtime offers default plus three outfits but the reference shows only three equal slots and runtime adds a tiny reset chip. Options: four equal cards in a horizontally scrolling strip, or three outfit cards with tap-equipped-to-reset and no default card. **Recommend four equal cards**, retaining explicit default choice without a new control role. Pattern owner must define this horizontal strip/focus behavior before building it.

The page blocks the underlying map/level. No dim is required behind the page's own decorative forest; any modal above it must follow `modal-panel`. Shared disc-glyph conflict and numeric-capacity decisions are recorded in [aviary list](aviary-list.md); they also govern this page.

## 4. States

Fake source must reach: each of all ten met species; Sunny first visit with zero homes and no owned outfits; typical Berry with one owned/equipped outfit; all three owned; default and every outfit equipped; owned-but-not-selected; locked/unowned; unavailable/disabled; controller rejects equip; Rook with no outfit strip, no gift and zero homes. Also: invalid/locked species (return list), missing model, missing portrait/overlay/fixed-label asset, zero/typical/99999 coins, zero/typical/99999/2147483647 homes, longest name `Flurry`, longest catalog personality, overlong localized name/personality/effect artwork, strip at both ends, repeated equip/back/reopen, focus loss, muted and instant motion. Empty ownership is a valid state, not an empty catalog. No network/loading/purchase state: local data works offline, and this page opens no popup. Missing complete model leaves navigation available and content unbound; do not synthesize a species or reward.

## 5. Interaction map

| Action | Event / result | Feedback / sound |
|---|---|---|
| Tap owned outfit | `OutfitRequested(speciesId,outfitId)`; await authoritative model | `button-press / release`, `ui-tap`; only confirmed selection gains check/ring and changes hero |
| Tap default | `DefaultLookRequested(speciesId)` | same patterns/sound; confirmed model clears equipped outfit |
| Tap already equipped outfit | `OutfitRequested(speciesId,outfitId)`; controller toggles back to default, preserving existing behavior | press/release + `ui-tap`; confirmed model changes check/hero, no reward |
| Tap unowned | `LockedOutfitTapped(id)`; refusal, no purchase | `button-press / release`, `ui-tap`, then `refuse`; padlock stays, no equip |
| Tap unavailable / hero / gift / count / coin / background | none; display-only or inert | unchanged, silent |
| Drag/wheel outfit strip | presentation scroll only | clamped movement, silent |
| Close / Android back | `BackRequested` → list | close press/release + `ui-tap`; back `panel-close`, no pointer sound |

No haptics. Touch and Editor mouse use release-inside activation. No keyboard/controller focus navigation is supported beyond Android back.

## 6. View contract

Proposed calls: `Show(AviaryDetailModel)`, `SetCoins(value)`, `SetCollection(count,looks,equippedId)`, `ShowEquipRejected()`, `Hide()`. Model supplies stable species ID, met/availability, baked name/personality/effect keys, hero/frame/overlay keys, gift key or explicit no-gift, home count, coin number, ordered default/three looks with owned/equipped/available flags. Catalog/personality/gift mapping and committed Meta profile are controller-owned; bind on open and each confirmed profile change. Do not compute ownership, call `ToggleCosmetic`, save, or increment counts inside the view.

Missing numbers show no digits, not false zero; missing labels/portraits use supplied unknown/silhouette assets and disable affected choices. Missing outfit overlays disable that preview rather than showing a different equipped look. Explicit no-gift/no-outfits hides those rows for Rook. Raise interaction-table events; selection feedback never commits game state. Rejection leaves the previous authoritative look/check intact.

## 7. Motion and sound

| Trigger | Motion | Audio |
|---|---|---|
| Open / back | `panel-open` / `panel-close`, all controls together | retain entry context's `bgm-menu` or `bgm-game`; no restart or duck on detail/back |
| Outfit/default/close | `button-press / release`; confirmed look swaps without rebuilding entrance | `ui-tap`; locked uses `refuse`; rejected request has no success sound |
| Hero, with or without outfit | breathing 2.4–3.2 s; drawn idle actions every 1–3 s at 10–14 fps; overlays follow frames | silent idle |
| Coin update | `counter` | `coin-gain` / `coin-spend` only on supplied transaction, not opening/equipping |

Never mirror birds or use transformed stills as actions. Cancel hidden motion; reduced motion uses pattern instant fallbacks and `GameMotion.Instant`. All audio follows the single mute control and focus pause.

## 8. Accessibility

**NOT ASSESSED:** contrast/screen-reader/device targets absent. Assume ≥88 × 88 design-px targets; 100 px close and equal outfit cards meet it. Check/ring communicates selection; padlock communicates ownership independently of colour. Accessible nonvisible names identify species, default/look and equipped/locked state. Panel cream/rim/shadow must separate rows from the forest; no instruction depends on animation.

## 9. Text

| Element | Limit |
|---|---|
| Species name | baked existing catalog; 6 English characters maximum; no runtime autosize |
| Personality | baked existing `AviaryView.personalities` catalog; one line, maximum 36 English characters; longest and localization-stress artwork must fit measured box |
| Gift effect | baked, exactly two words, ≤18 characters including space; Rook has no text row |
| Coins | live integer, 1–5 digits, 0–99999; measured inner rect |
| Home count | live integer, 1–10 digits, 0–2147483647; numeric-capacity decision applies |
| Default/outfit/lock/check/close | icons only, zero visible words; no live outfit name, tooltip or paragraph |

Fixed labels are rebaked/re-measured for localization; no unapproved percentage reserve or per-string runtime shrinking. **[Decision needed] Gift copy:** approved brief requires two-word effects but runtime uses longer sentences. Options: approve new two-word labels or amend the two-word rule. **Recommend approve:** Sunny “Wakes sleepers”; Skye “Reveals eggs”; Berry “Free move”; Minty “Grows branches”; Cocoa “Clears crows”; Flurry “Any landing”; Ember “Hatches eggs”; Inky “Switches sky”; Hoot “Extends night”. These are proposals, not new rule definitions; shared goal-info copy must use the same approved table.

## 10. Acceptance criteria

1. At 1080 × 1920 and 1080 × 2400 with insets, the whole hero and all outfit/gift/count rows remain accessible; no text escapes its ribbon, label box or panel. Exercise 99999 coins, maximum count and overlong fixed-label fixtures.
2. Every default/outfit card has the same resting fit box in owned, equipped, locked and disabled states; close is 100 × 100 px. Disabled alpha is 0.45 and cannot equip.
3. Disc glyphs use one shared opaque-bound size, not equal sprite canvas sizes; close/lock/check are legible and preserve aspect.
4. Wood frame, cream interior and shadow clearly separate the information panel from the forest; all content lies inside its measured cream interior minus 24 px.
5. Enter completes in 0.18 s and back in 0.12 s after ready data; no individual control arrives later.
6. Owned-look requests do not alter the hero/check until a confirmed model arrives; rejected and unowned requests change neither coins nor equipment. Default restoration is explicit and reversible.
7. A timed capture shows breathing and drawn idle frames both bare and dressed; no negative horizontal scale or static dressed-bird exception.
8. Back returns to the same list/scroll; Rook shows no outfits or gift; reopening never grants another new-species reward. Mute silences page music and feedback.

## Differences from the current build

- Bake name, personality and short gift copy; live text is numbers only. Remove runtime auto-shrinking fixed text.
- Replace the 90 px default chip with the approved equal-choice treatment; keep three outfit tiers, Rook exclusion and controller-owned equipment semantics.
- Current dressed hero only scales a still and undressed hero has an extra transform hop; use drawn idle sequences with frame-following overlays for both.
- Rework panel interior from measured assets, normalize close/glyph sizing, and replace direct profile reads/toggle calls with model updates/events.
