# Aviary list

**Status:** draft. **Stage:** screen specification, not UI approval. **Platform/input:** portrait iOS/Android phones; touch; mouse in Editor; Android back. Reference 1080 × 1920, 9:16–9:20; review at 1080 × 1920 and 1080 × 2400. Safe-area root fits the design canvas uniformly; decorative forest may extend outside it.

**[Decision needed] Brief completeness (all four specs).** The supplied brief contains two addenda (gameplay and continuous tree painting), while the assignment refers to three. Options: supply the missing addendum, or confirm that the available two are the complete contract. **Recommend supplying/reconciling the third before approval**; these drafts incorporate every decision present in the supplied file, without inventing another addendum.

## 1. Player need

The player arrives wanting to see which birds they have met, their gifts and how many flocks they have brought home. They were browsing the map; the existing programmatic in-level entry must also return to its unchanged level.

## 2. Navigation

Map aviary disc → list → met species card → [species detail](aviary-detail.md). Close-x or Android back returns to the entry screen. From detail, back returns to this list with its scroll position retained. Background taps do not close a page. Focus loss pauses audio and input; resume preserves the page and scroll. Navigation away cancels presentation callbacks. No purchase, claim, new-friend or goal-info popup opens here; first-meeting rewards remain the notification flow's responsibility.

## 3. Layout

Use [patterns](../patterns.md) by name. Coordinates below are top-left design coordinates. `R` means the measured `aviary_list.elements` in `art/approved_ui/layout.json` (manual bounds, ±8 px), not guessed hit targets. `M` means `PerchSort/Assets/Resources/Art/layout_metrics.json`; interiors map through the asset's actual uniform fit, not the containing box. Use 24 px content margins from `modal-panel`. Preserve sprite aspect ratios.

| Component | Pattern | Asset | Size / placement source | Layer |
|---|---|---|---|---|
| Forest; page backing | page | existing page forest | full safe canvas; board/backing separates collection from forest | 0 |
| Shadow, board | modal-panel (frame/interior rules), page | `aviary_board` | fit inside R.board `(20,106,1032,1814)`; M native 900 × 1640, inner `(120,164,651,1316)`; viewport strictly inside mapped inner minus 24 px each edge | 1 |
| Species grid | list-grid, card-button | `aviary_frame`, species portrait/locked silhouette | two columns; portrait fit box 318 × 316 from R.bird_frame_1; row pitch 392 from measured first/second rows; scroll all five rows, never under header/frame | 2 |
| Names | name-ribbon | `labels/label_name_<species>`, `label_name_unknown` | fit box 296 × 98 from R.name_ribbon_1; baked green-band content follows M.ribbon_green inner `(34,23,569,153)` in native 640 × 200 | 3 |
| Homecoming count | count-chip | `house_count_chip` | fit box 140 × 62 from R.house_count_chip_1; number only inside mapped M inner `(26,14,127,48)` of 180 × 76, right of house art | 3 |
| Gift per met flock species | gift-coin | species gift icon | 84 px; at portrait's lower-right, inside its card, not over name or count; Rook has no gift | 3 |
| Scroll indicator | list-grid | existing `stat_pill` track/thumb | R.scroll_track 24 × 1104 and R.scroll_thumb 34 × 118; right of viewport, within board | 3 |
| Coin header; close | coin-pill; close-x | `coin_pill`; `btn_wood`, `icon_close` | pattern sizes/anchors, not superseded R header dimensions; close at safe top-right with 24 px inset | 4 |

**[Decision needed] Collection geometry.** The measured reference grid begins above the current board's mapped cream interior, and adding gift coins needs collision checks. Options: reflow rows/viewport inside the measured current frame, or derive a larger-interior board asset from the approved reference. **Recommend reflow**, retaining measured component sizes and scrolling rather than clipping. The pattern owner must record final row offsets and indicator travel; do not reuse the reference grid rectangle as a valid cream viewport.

**[Decision needed] Disc glyph standard.** Art direction's approximately 46% of a 100 px panel disc is below its 56 px minimum; the brief instead says about 55%. Options: keep the ratio with an explicit small-glyph exception, or use a shared 56 px visible glyph extent. **Recommend 56 px**, with `glyph_metrics.json` opaque bounds determining scale/centering; no local exception pending a shared decision.

A page blocks input below it. It is not a floating modal; a modal opened above the page must use `modal-panel` dim/shadow, but this list opens none.

## 4. States

Fake data must expose each case independently and in combinations:

- Empty collection: all ten catalog entries locked, zero counts; empty supplied catalog: empty viewport with coin/close retained, no invented reward/tutorial.
- First visit after meeting Sunny/Skye; later visit with identical data, no repeat reward. Typical: six met, four locked, mixed counts. Maximum: all ten met, all five rows reachable, final row fully visible.
- Coins 0, 240, 99999; home counts 0, typical, 99999 and 2147483647 (storage boundary); longest existing name `Flurry`, every other baked name, and deliberately oversized localized-name artwork.
- Locked/grey card + padlock/unlock level; met card normal/pressed; unavailable data disabled; scroll at top/middle/bottom; return from every species detail with scroll retained.
- Missing portrait/name/gift/count model, missing entire model; muted/unmuted; focus loss/resume; safe-area extremes. Local data requires no network: offline behaves normally; no loading spinner or network-error popup. A withheld model is a fake-data boundary, not a new production loading flow.
- Detail cases include no outfits owned, every equipped look, Rook and maximum counts; list itself opens no popups.

## 5. Interaction map

| Input | Event/result | Feedback and sound |
|---|---|---|
| Tap/release met card | `SpeciesRequested(id)`; controller opens detail | `button-press / release`, `ui-tap`; card stays same resting size |
| Tap locked card | `LockedSpeciesTapped(id)`; controller refuses, no unlock/purchase | `button-press / release`, `ui-tap`, then `refuse`; lock remains, no mutation |
| Drag or mouse wheel | presentation scroll only; no game-state event | clamped list/indicator movement; no sound/haptic |
| Tap close / Android back | `CloseRequested` | close uses `button-press / release`, `ui-tap`; back uses `panel-close`, no pointer sound |
| Tap coin/background; press unavailable card | no action | unchanged; no sound/haptic |

Touch and mouse have the same release-inside behavior. No keyboard/controller navigation is promised; Android back is the only non-pointer navigation command. Haptics: none.

## 6. View contract

Proposed presentation contract, not existing API: `Show(AviaryListModel)`, `SetCoins(value)`, `SetSpecies(card)`, `RestoreScroll(position)`, `Hide()`. Models contain ordered stable species IDs, baked name/unknown asset keys, portrait/locked art, met/availability flags, unlock-level badge key, committed home count, gift asset key and coin number. Data comes from Meta.Aviary catalog, profile collection/reward ledger and species gift mapping; updates arrive on opening or committed profile change, never from idle animation.

Missing card metadata disables that card; missing art uses a supplied silhouette/unknown ribbon; a missing number has no digits (never fabricated zero). Missing whole model leaves a closable empty page. Rook's absent gift is intentional. Raise only the interaction-table events; controller owns eligibility, rewards, navigation and persistence. Scroll position is ephemeral presentation state. UI never calls `OnLevelStarted` or increments home counts.

## 7. Motion and sound

| Trigger | Motion/timing | Sound/music |
|---|---|---|
| Enter / leave / detail return | `panel-open` / `panel-close`; no staggered controls; restore scroll before reveal | `bgm-menu` for map entry; retain `bgm-game` for existing level entry; no restart on detail/back; page music changes use 1 s crossfade |
| Card, close, refusal, scroll | interaction-map pattern rows | `ui-tap` / `refuse` / silent as above |
| Coin change | `counter` | `coin-gain` or `coin-spend` only for a controller-supplied transaction, never initial binding |
| Visible met portraits | continuous breathing 2.4–3.2 s; drawn idle actions every 1–3 s, 10–14 fps; front-facing rest, never mirror/transform-only actions | silent idle; one mute control covers all audio |

Cancel motion/timers on hide. Reduced motion uses pattern fallbacks and `GameMotion.Instant`; it does not change data or target sizes.

## 8. Accessibility

**NOT ASSESSED:** no project contrast, screen-reader or physical-device accessibility target. Working assumption: at least 88 × 88 design-px hit targets; cards and close exceed it. Locked is indicated by padlock, not grey alone; accessible names identify species, lock requirement, count and close without adding visible live labels. Keep pattern wood/cream separation and shadow; no motion-only instruction.

## 9. Text

| Element | Limit and rendering |
|---|---|
| Species name / unknown | fixed baked catalog, at most 6 English characters (`Flurry`); `?` one character; no live name, wrapping or runtime autoshrink |
| Locked level | fixed baked level badge, 1–40 (two digits); no live explanatory sentence |
| Coins | live integer, 1–5 digits through 99999, inside measured coin inner rect; no separators or currency text |
| Home count | live nonnegative integer, 1–10 digits through 2147483647; requires the decision below |
| Gift, close, empty collection | icon-only, zero visible characters; no hidden paragraph or toast |

Localized fixed text is rebaked and remeasured inside its band; oversized-art fake state must fail acceptance, never overflow or silently shrink a single name. **[Decision needed] Numeric capacity:** current measured count/coin interiors are not proven readable at these limits. Options: measure and widen number-bearing art to preserve full stored values, or approve compact/capped display. **Recommend full values with measured wider art where required**; do not truncate stored counts or clamp the profile. This decision also applies to detail.

## 10. Acceptance criteria

1. At 1080 × 1920 and 1080 × 2400 with safe-area insets, the last catalog row can scroll completely into the cream viewport; no card is hidden by bars/frame.
2. All names, `?`, locked-level badges and 99999 coins stay wholly inside their measured containers; maximum home-count and long-name fixtures expose any unresolved fit, not clipping.
3. Every same-role card has identical resting bounds when normal, locked, disabled or selected; close remains 100 × 100 design px; disabled is alpha 0.45 and cannot open detail.
4. Disc glyphs have uniform measured opaque extent under the resolved shared standard, independent of transparent canvas padding; none is stretched.
5. Wood rim, cream backing and shadow visibly separate the panel from the forest; no world bird or HUD control renders above it or accepts input through it.
6. Page enter completes in 0.18 s and exit in 0.12 s after the controller presents ready data; no control has a delayed entrance.
7. Locked taps never meet a species, award coins or open a shop; detail/back restores scroll; repeated visits leave coins/counts unchanged.
8. A real-motion capture shows continuous breathing and drawn idle changes for visible met portraits; mute silences UI and music without resetting the page.

## Differences from the current build

- Add each met species' gift coin; use baked names/unknown labels instead of live text.
- Replace silhouette-only locks with the shared grey/lock requirement treatment; separate data and events from `GameRoot` reads and direct detail navigation.
- Reflow the grid to measured cream bounds; standardize coin/close geometry and glyph normalization; current per-row ribbon/count offsets are not a new standard.
- Current list portraits are still images; apply continuous breathing and drawn idle sequences. Keep the existing ten-species order and committed count semantics.
