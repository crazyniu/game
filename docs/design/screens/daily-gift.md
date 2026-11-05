# Daily gift

Status: draft. Stage: screen specification, not runtime approval. Portrait phones; touch, Editor mouse, Android back. Reference 1080×1920 (9:16); also review 1080×2400 (9:20) with safe areas. No additional keyboard/controller scheme is promised.

Authority: [art direction](../art-direction.md), [patterns](../patterns.md), [brief including all addenda](../../work-log/ui-redesign/brief.md), [conventions](../../CONVENTIONS.md); reward intent: [game design, Coins](../../GAME_DESIGN.md#82-coins). Starting implementation: `PanelsView.DailyGift/Popup`, `PopupFlow`, `PopupMotion`, `MapView.CollectCoins`, `GameRoot.AcknowledgeNotification`. Style reference: `art/approved_ui/ui_popup_daily_gift.png`, not authority to restore its level underlay or purple controls.

Shared source gap: [brief completeness decision](settings.md) applies here; only two addenda are present in the supplied brief.

## 1. Player need

The player arrives wanting to collect today's nest gift and continue playing. They have just reached the map; the reward is understandable through the present, coin and number, without explanatory prose.

## 2. Navigation

- Logic presents this notification on eligible map entry, after any pending world-complete/world-unlock sequence. Normal reward is 50 coins; eligibility, clock/date and once-per-day receipt remain entirely in logic.
- Check bar requests acknowledgement once. Logic commits/persists the receipt and supplies the updated balance; only then does collection feedback run on the map. No credit on open, lid animation, focus loss or dismissal.
- After collection, logic either leaves the map or resumes a deferred next level. A level's subsequent new-friend/booster-unlock popups belong to that level's queue, not this view. This popup opens no nested popup or shop.
- Scrim never acknowledges. Focus loss pauses audio; hide/destroy cancels owned motion/callbacks. On return, logic re-evaluates eligibility; an unclaimed gift remains available, an acknowledged gift is not paid again. No auto-dismiss or countdown.

[Decision needed] **Notification back/close policy** (also affects world-complete and world-unlock). `PopupFlow`/`PanelsView` currently require acknowledgement and block outside dismissal, whereas the shared pattern says back/close closes the top modal. Options: retain acknowledgement-only notifications (no X, back inert); or allow X/back to defer without claiming and let the coordinator re-present later. Recommend explicit acknowledgement-only exception to preserve existing receipt/queue behavior. Layout below assumes that exception; adding a 100 px X also requires settling the primary/sibling size rule, not silently adding a giant-bar/small-disc pair.

## 3. Layout

Use `modal-panel`, `stat-pill`, `primary-bar`. All dimensions are design px. Panel source: `PerchSort/Assets/Resources/Art/layout_metrics.json` (`tools/layout_metrics.py` output), native 800×540, cream `(88,119,645,317)`. Its existing sprite borders are left/right 150, top 110, bottom 100. Keep width 800 and nine-slice only the centre vertically; do not stretch the gift artwork.

The stack has a 578×522 gift slot measured in `art/approved_ui/layout.json.screens.popup_daily_gift`, a 440×72 `stat-pill`, a 520×120 `primary-bar`, 24 px gaps and 28 px top/bottom cream clearance. Required cream height is `522+72+120+2×24+2×28 = 818`. Mapping the measured inner rect through the actual slice gives panel height `210+818×330/317 ≈ 1061.55`, not an estimated panel size. For height H, cream top is `110+9×(H−210)/330`; cream x/width stay 88/645. Centre the panel in the safe-area design rectangle; require ≥24 px exterior clearance. Centre all rows on the cream area's horizontal centre; start at cream top +28 and advance by row height +24.

| Component | Pattern | Asset | Size / anchor | Layer |
|---|---|---|---|---|
| Dim / soft shadow | modal-panel | procedural | viewport stretch / panel silhouette | 0 / 1 |
| Frame | modal-panel | `panel_wood` | 800×1061.55, derived above; safe-area centre | 2 |
| Nest + gift | modal-panel content; reveal pattern pending below | `gift_nest`, `gift_box_base`, `gift_box_lid`, `gift_nest_front` | 578×522 slot, first row; contain the shared 768×768 canvas uniformly | 3, in listed part order |
| Reward | stat-pill | `stat_pill` + `coin` + live amount | 440×72, second row; coin in left content area, number in remaining measured inner area | 4 |
| Acknowledge | primary-bar | `btn_green_pill` + `icon_check` | 520×120, final row; 64 px glyph per pattern | 5 |

Reward coin uses the measured native `coin_pill` coin area (128 px square in its 320×128 source), uniformly fitted to the reward pill's 72 px height; no free-floating label. Use measured `stat_pill.inner = (23,17,354,60)` from its 400×96 source, transformed with the actual sprite layout, then exclude coin occupancy before placing the number. Reserve the full maximum number width; never resize per value.

Normalize the check's opaque longest edge to the pattern's 64 px using `PerchSort/Assets/Resources/Art/glyph_metrics.json` (96×96 source canvas, opaque 90×69 at offset 3,13); preserve aspect and compensate its measured centre offset.

[Decision needed] **Gift-reveal pattern.** No entry exists in the library for the multipart basket/lid. Options: add a shared `gift-reveal` entry retaining the source timings in §7; or use only generic panel/counter motion with a static gift. Recommend adding the entry before shell work; do not reclassify a static picture as animation. Geometry comes from the recorded gift assembly contract, not a new guessed asset size.

## 4. States

Fake coordinator must reach all of these, without changing the real economy:

- First eligible visit, eligible later day, same-day already claimed (no modal), and pending world sequence delaying the gift. Ineligible is the locked case, not an invented locked-gift preview.
- Typical +50; zero/empty reward model (disabled check until logic supplies a valid request); stress +99999 in the same number container (not a new payout rule).
- Map balance 0, 240, 99949→99999; 99999→100049 boundary; already-paid/stale request (no second reward). Long world/species names in fake data must not create visible text in this icon-only popup.
- Closed basket, reveal in progress, lid open, reward counting, ready, held/released, disabled acknowledgement, closing, committed map collection, and return to map/deferred level.
- Repeat taps; suspend before acknowledgement, during close and after persistence but before collection completes; reopen from a fresh authoritative snapshot. Instant/reduced motion and mute on/off.
- Offline is normal local operation. No network loading/error/retry popup exists. Missing model disables the check; missing multipart art is a development asset failure, not an accepted static-fallback state.
- Every downstream popup: world sequence before entry; new-friend and booster-unlock after deferred level entrance. No new-friend, locked-level or shop popup is opened by the gift itself; select/move/invalid/win/lose are underlay/coordinator states, not gift actions.

[Decision needed] **Currency display ceiling** (shared with world-complete). No approved balance maximum exists; 99999 +50 needs six digits. Options: measure a common numeric size supporting the real logic range; or approve a separate abbreviated display policy. Recommend measured full numbers, never cap the profile or silently truncate to five digits. Five digits are a minimum stress case, not a new economy limit.

## 5. Interaction map

| Input | Event / resulting state | Feedback / sound |
|---|---|---|
| Tap/click check, valid request | `AcknowledgeRequested(requestId)`, once; wait for coordinator close/commit/collection | `button-press / release`, `panel-close`; `ui-tap` on down; `coin-gain` only when authoritative collection begins; no haptic |
| Disabled/duplicate check, release outside | no acknowledgement | dim/inert or shared release feedback; no sound on disabled/duplicate action; no haptic |
| Tap scrim / Android back | no event under the proposed acknowledgement-only exception | popup stays visible; no sound/haptic; defer alternative requires the decision above |
| Suspend / hide | no claim event | cancel owned feedback, retain no game state; no sound/haptic |

Single actionable control; no keyboard/controller focus sequence. A tap during the reveal may acknowledge immediately; visual choreography never gates a valid claim or starts a second one.

## 6. View contract

Proposed API boundary; the UI owns no eligibility, dates, receipts or currency:

| Data shown | Source / update | Missing |
|---|---|---|
| Reward number and enabled state | current notification/economy snapshot; on show and replacement | no fabricated +50; disable check until a valid request arrives |
| Basket / reveal pose | presentation state for the supplied request | asset error if parts absent |
| Dimmed balance / map | map model; authoritative update after persistence | do not invent a profile |

Calls: `Show(DailyGiftModel { requestId, rewardCoins?, canAcknowledge })`, `Render(DailyGiftModel)`, `Close()`, `Hide()`. Outgoing events: `AcknowledgeRequested(requestId)`, `Closed`. If defer is approved, add `DismissRequested(requestId)` without treating it as acknowledgement. Map coin feedback is a separate logic-issued `PresentCollection(from, to)` command; `CollectionFinished` reports presentation completion only. Stale requests are rejected by logic. No view callback writes saves or derives the next popup.

## 7. Motion and sound

| Trigger | Motion / duration / easing | Sound / music |
|---|---|---|
| Open / close | `panel-open` 0.18 s ease-out / `panel-close` 0.12 s ease-in; all controls visible immediately | none; retain `bgm-menu`, no restart/duck |
| Press / release | `button-press / release`, 0.08 / 0.18 s | `ui-tap` on pointer down; mute respected |
| Gift reveal, pending pattern | current `PanelsView.DailyGift`: shake starts at 0.35 s for 0.5 s; lid departs at 0.9 s for 0.55 s; sparkle at 0.9 s for 0.55 s; linear timeline, existing oscillatory/sine paths | none; no gift-specific sound exists in the table |
| Reward number | `counter`, 0.5 s ease-out | preview is silent, not a payout |
| Committed map collection | coin flight from basket to map pill: retain `MapView` 0.65 s flights, 0.06 s stagger, last arrival 0.89 s; balance uses shared `counter` 0.5 s | `coin-gain` once at collection start |

Gift parts keep their common canvas/pivot from `layout.json.assemblies.daily_gift`; no arbitrary offsets. Reduced/instant motion snaps to the open gift/final values and removes decorative flight, without bypassing acknowledgement. All sound uses the existing single mute; focus loss pauses music. No automatic dismissal.

## 8. Accessibility

NOT ASSESSED: no approved contrast/device/assistive-navigation target. Assume ≥88 px design-space touch targets; the check is 520×120. Accessible name: Collect daily gift, not a visible label. Coin + numeric reward identifies the reward without color; disabled is alpha 0.45, inert, unchanged size. Honor reduced-motion settings. No haptic behavior is currently specified.

## 9. Text

- Reward only: live nonnegative number with a leading `+`; normal `+50` (3 characters), stress `+99999` (6). Use one measured Fredoka size for the full declared range, no value-dependent autosizing; missing model hides the amount rather than inventing zero.
- Underlay coin balance: live digits, at least 0–99999 (five); six-digit boundary awaits the ceiling decision. No title, date, streak, timer, player/species/world name or visible button label.
- All future fixed copy must be baked and separately measured/localized; this spec adds none. Check glyph is artwork, not a text checkmark.

## 10. Acceptance criteria

1. At 1080×1920 and 1080×2400 with safe-area insets, the complete panel and check are visible; all rows stay inside the measured cream bounds, the primary bar has ≥28 px left/right/bottom clearance, and no number escapes its pill at +99999 or a 99999 balance.
2. The check's base/hit rectangle is 520×120 in every state; disabled is alpha 0.45 and non-interactive, never smaller. Its opaque glyph matches other primary-role glyphs at 64 px; transparent sprite padding must not make it visibly smaller.
3. Black dim alpha 0.72 covers every map/HUD/bird object from the first frame; the panel has a soft shadow offset 14 px with combined alpha 0.35 and is clearly separated from its background.
4. Panel open completes in 0.18 s; close in 0.12 s. All controls appear on the first frame; optional gift reveal completes by 1.45 s and never delays acknowledgement. Device frame-time performance is NOT ASSESSED.
5. Opening, waiting, tapping the scrim or suspending never pays coins. One acknowledgement produces one authoritative receipt; a repeat tap or stale request cannot produce another payout.
6. After a committed +50 claim at 99949, the map ends at 99999. An interrupted collection resumes from authoritative data without replaying the payment; the six-digit boundary is not accepted until its decision is resolved.
7. There is no same-day popup after a paid receipt; a new eligible day can present it. Deferred next level and its new-friend/booster-unlock queue begin only when the coordinator permits them.
8. Muting silences `ui-tap`, `coin-gain` and music; opening/closing does not restart menu music. All owned motion is cancelled on hide and no late claim callback fires.

## Differences from the current build

- Reflow the current 640×930 popup into a measured nine-sliced cream stack; the existing 578×522 basket and reward overlap by their reference bounds, and a 520 px bar does not leave the required cream margins in the current narrow panel.
- Use the shared 0.12 s close and 0.5 s counter instead of `PopupMotion.Close` 0.15 s and `Count` 0.6 s. Keep collection after persisted acknowledgement; align its balance counter with the shared pattern rather than the current 0.89 s count.
- Preserve local daily eligibility, pending world priority and no nested popup. Use events instead of direct root mutations in the fake shell; unsupported gift parts must not pass acceptance via the current whole-object fallback.
- Back/close exception, gift-reveal pattern and the balance ceiling require decisions before shell approval; this draft does not alter them in code.
