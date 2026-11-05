# New friend

**Status:** draft. Stage: screen specification, not UI approval. Portrait phones; touch and Android back. Reference 1080 x 1920, aspect range 9:16–9:20; review at 1080 x 1920 and aspect-derived 1080 x 2400. No shipping keyboard/controller navigation.

Authority: [art direction](../art-direction.md), [patterns](../patterns.md), [redesign brief](../../work-log/ui-redesign/brief.md), [collection design](../../GAME_DESIGN.md#84-aviary-collection). Style: `art/approved_ui/ui_popup_new_friend.png`, superseded for old controls. Current owners: `PanelsView.NewFriend/Popup`, `PopupFlow`, `PopupMotion`, `UiBirdLife`, `GameRoot.PumpNotifications/AcknowledgeNotification`. No rules, economy or persistence redesign.

## 1. Player need

The player arrives wanting to meet the bird just encountered and recognize its trait before playing. The level entrance has finished; the board remains visible but blocked.

## 2. Navigation

Logic presents unseen species in canonical discovery order after the 3.5–4.5 s level entrance or its skip. Check requests acknowledgement once; logic commits discovery and the existing 20-coin award, updates the HUD, then selects the next new-friend, unlock-card or playable board. Already-met replay opens nothing. No nested shop, aviary or purchase popup. Dim taps do nothing; no separate close control or auto-dismiss.

Focus loss suspends presentation/music, not acknowledgement. Hide/destroy cancels motion and delayed callbacks; reopen/reload takes pending state from logic. Unacknowledged discovery must not become a view-owned receipt.

**[Decision needed: notification back]** Current notifications ignore Android back; patterns say back closes the top modal. Options: keep check-only; or map back to the same acknowledgement event. Recommend back-as-acknowledgement, never an unpaid silent dismissal. This decision is shared with unlock-card.

## 3. Layout

Design px only. `PerchSort/Assets/Resources/Art/layout_metrics.json` (from `tools/layout_metrics.py`) measures `panel_wood` canvas 800 x 540, inner `(88,119,645,317)`; `name_ribbon` canvas 320 x 100, inner `(19,12,283,72)`; `stat_pill.inner` 354 x 60. `art/approved_ui/layout.json`, `popup_new_friend.hero_bird`, measures 474 x 580 (manual uncertainty ±8 px).

Use native-border vertical nine-slicing at panel width 800. Height is measurement/pattern-derived: `119 + 24 + 580 + 24 + 100 + 24 + 84 + 24 + 120 + 28 + 104 = 1231`; 104 is measured bottom inset `540−119−317`, 24 is modal spacing, 28 is primary bottom clearance. Cream inner becomes `(88,119,645,1008)`. Center the panel in the device safe area with at least 24 px clearance. Stack children on the inner rect's center line, starting inner-top +24, with 24 px gaps. Scale the whole reference canvas uniformly only when required for fit; never resize individual controls.

| Component | Pattern | Asset | Size | Anchor/position | Layer |
| --- | --- | --- | --- | --- | --- |
| Dim/input shield | modal-panel | black overlay | full physical viewport | stretch beyond safe area | above all world/HUD/birds |
| Shadow/panel | modal-panel | `panel_wood` | derived 800 x 1231 | safe-area center | shadow then panel |
| Living front-facing hero | **new pattern needed: living portrait** | species front/rest and drawn frames | measured 474 x 580 envelope, fit every pose without stretching | first stack slot | content |
| Baked name | name-ribbon | derived species-name bake | native 320 x 100 | second slot; measured inner contains ink | content |
| Gift row | gift-coin | `ViewModel.GiftIcon(species)`, baked effect | 84 coin + 24 gap + measured 354 x 60 label; total 462 x 84 | third slot, vertically centered | content |
| Check | primary-bar | `btn_green_pill`, `icon_check` | pattern 520 x 120; visible check 64 | last slot, 28 cream bottom clearance | input topmost |

Effect is baked into cream content, not a live-text pill. No extra title, currency counter, outfit controls or collection count. Existing HUD stays beneath dim. Optional existing glow/sparkles stay inside the hero envelope, behind it, non-interactive.

**[Decision needed: living portrait pattern]** No library entry exists. Options: adopt these measured hero and goal-info portrait envelopes as shared variants; or approve new measurements. Recommend measured variants; add the shared pattern before shell work.

**[Decision needed: modal dim]** Brief says about 45% black; patterns/art direction/conventions say alpha 0.72. Options: retain 0.72; or revise the shared rule to 0.45. Recommend consistent 0.72, pending ratification.

## 4. States

Fake source/state switcher must reach all of these without writing saves:

- Empty/hidden queue; typical Minty; each of ten catalog species including Rook; first visit Sunny → Skye; later unseen species; already-met replay suppressed; locked/future species supplied as ineligible, suppressed.
- Maximum synthetic backlog: ten catalog cards followed by six booster cards, one modal at a time; normal new-friend → unlock-card → board. No nested popup.
- Longest real name Flurry (6 characters), same-length wide-glyph baked name, every effect including 18-character maximum; Rook without a gift.
- Underlying coins 0, reference-typical 240, 99999, acknowledgement ending at 99999, and 99999 → 100019. The view never caps the economy.
- Ready, held, release-outside, disabled/pending acknowledgement, rapid repeat, closing; opening, flight/landing, ten-second idle, early acknowledgement during flight; muted/unmuted, focus loss/resume, instant motion, forced hide/reopen mid-flight/close.
- Missing/unknown model, name/effect bake or required frames: development diagnostic, no fabricated species/gift. Pending data keeps the normal card hidden; offline is normal bundled-content operation, not a network error/retry popup.
- No move, selection, invalid-action, delivery, win/lose or reward simulation inside this view; those remain underlying logic states. No timer/auto-dismiss.

**[Decision needed: Rook row]** Rook is a catalog discovery but has no gift; current `GiftIcon("crow")` throws. Options: omit the row; or use a Rook portrait token and existing baked behavior `Never goes home` (15 characters). Recommend the behavior row, explicitly not a homecoming gift.

## 5. Interaction map

| Input | Event/result | Feedback / sound |
| --- | --- | --- |
| Enabled check, release inside | `AcknowledgeRequested(notificationId)` once; wait for logic | `button-press / release`, `ui-tap` on pointer down, then `panel-close` |
| Drag/release outside check | no acknowledgement | restore with `button-press / release`; initial `ui-tap` only |
| Disabled/pending/repeated exit tap | no event | inert dim/existing exit; no new sound |
| Bird, label, cream or dim tap | no event | living idle only; silent |
| Android back | §2 decision | approved acknowledgement uses `panel-close`, no pointer sound; otherwise no effect/sound |
| Focus loss/resume | presentation suspension only | music pause/resume; no replayed UI sound |

No haptics. Sole focusable control: check; future keyboard/controller bindings need approval.

## 6. View contract

| Data shown | Source/update | Missing |
| --- | --- | --- |
| Species identity, name bake, front/frame set, gift/effect bake or approved Rook row | catalog/art adapter on each show | diagnostic; no invented card |
| Notification identity and acknowledgement enabled/pending | coordinator on show/change | hidden without valid identity |
| Motion and suspension context | existing presentation/settings service on change | existing project defaults |

Shell calls: `Show(NewFriendPresentation)`, `SetAcknowledgementEnabled(bool)`, `SetPresentationSuspended(bool)`, `Close(completed)`, `Hide()`. Events: `AcknowledgeRequested(notificationId)` and lifecycle-only `Closed(notificationId)`; the latter cannot claim rewards. These specify the future boundary, not code changes now. Logic supplies resolved assets/eligibility, commits rewards/saves and advances queues. View owns no profile, coins, game rules or notification order. Fake coordinator provides responses without touching player data; wire real logic only after approval.

## 7. Motion and sound

| Trigger | Pattern/behavior | Sound/music |
| --- | --- | --- |
| Show | `panel-open`; labels/controls visible from frame one, no ribbon/text pop | no jingle; `bgm-game` continues, no restart/duck |
| Hero arrival | `bird-flight`, drawn 14 fps; retain current 0.6 s ease-out travel + 0.10 s landing; start fully above viewport at slot center-x, end at hero slot | none |
| Idle | art-direction breathing 2.4–3.2 s; drawn blink/hop/quirk every 1–3 s, hard cuts; no mirroring/still-image actions | no repeated UI sound |
| Check/exit | `button-press / release`, `panel-close`; cancel bird travel/idle on hide | `ui-tap`, no invented close sound |
| Logic's post-ack HUD update | underlying `counter`, not a popup number | `coin-gain` once at actual counter start |

All sounds use existing audio/mute controls. `GameMotion.Instant` suppresses capture motion; missing frames are an asset failure, not permission to fake an action.

## 8. Accessibility

**NOT ASSESSED:** no conformance or numeric contrast target. Assume minimum reference target 88 x 88; check is 520 x 120, to be rechecked after safe-area scaling. Non-visible accessible name “Continue”; catalog species/effect semantics are metadata. Shape/icon/name, not color alone, communicate meaning; use art-direction Fredoka treatment.

**[Decision needed: reduced motion]** No shared bird policy exists. Options: reduced travel/transforms with drawn pose changes retained; or still portrait. Recommend retained drawn changes, preserving living birds; panel reduction follows the instant pattern. Instant capture is not an accessibility setting.

## 9. Text

| Element | Limit/rendering |
| --- | --- |
| Name | baked catalog names Sunny, Skye, Berry, Minty, Cocoa, Flurry, Ember, Inky, Hoot, Rook; maximum 6 characters; ink inside ribbon `(19,12,283,72)` |
| Gift effect | baked, exactly two words, maximum 18 characters including spaces; candidate copy requires approval in [goal-info](goal-info.md#9-text) |
| Approved Rook behavior | fixed `Never goes home`, 15 characters; not gift copy |
| Check/title/reward/count | icon-only check; other visible text absent; payout belongs to HUD/logic |
| Background coin-pill | HUD-owned changing number, five-digit fit through 99999; six-digit overflow unresolved below |

**[Decision needed: localization/type bakes]** No locales or common bake type-size token are supplied. Options: approve common measured English ink sizing first; or supply measured per-locale bakes now. Recommend English-first, then locale-specific fit in the same containers; never live fixed text, per-name autosizing or truncation.

**[Decision needed: balance overflow]** Existing award applied to 99999 yields 100019, beyond the library's five-digit example. Options: measure a longer full-number HUD fit; or approve an abbreviated numeric display. Recommend measure full numbers without changing stored coins; do not invent a balance cap or overflow rectangle here.

## 10. Acceptance criteria

1. At 1080 x 1920 and 1080 x 2400 with safe-area simulations, the 800 x 1231 reference panel and all content are visible. Flurry, wide-glyph stress name and 18-character effect stay inside containers with margin, no clipping or per-item shrinking; background 99999 stays inside its pill.
2. Check layout/hit box remains 520 x 120 in normal, held, disabled and pending states; only pattern artwork press scaling changes. Visible check is 64 px, identical to unlock-card. Disabled alpha is 0.45 and raises no event.
3. Panel clearly separates from every world/HUD/bird object: full-screen black dim, soft shadow and blocked background input. Pending dim approval, draft values are alpha 0.72, shadow downward offset 14 px/composite alpha 0.35.
4. Panel entrance completes in 0.18 s, exit in 0.12 s; controls exist from frame one. Flight/landing completes in 0.70 s without delaying or duplicating acknowledgement. Device frame-time performance is NOT ASSESSED without a target device budget.
5. Ten seconds of timed frames show continuous breathing, drawn idle actions at most 3 s apart, front-facing rest, opaque frame cuts and no negative bird scale or still-image action substitute.
6. Accepted check produces one acknowledgement despite rapid taps/interruption. Logic grants the existing 20 coins once; met-species replay grants nothing. All ten cards, including approved Rook handling, and the ten-card/six-unlock synthetic queue are reachable one modal at a time; no preview changes saves.
7. Music remains present while unmuted and obeys existing focus/mute. Timestamped audio evidence places `ui-tap` on enabled pointer down and one `coin-gain` on actual HUD update, never a speculative reward callback.

**[Decision needed: brief completeness]** Assignment names three addenda; supplied brief contains two (gameplay visibility and continuous tree painting). Options: provide the third; or confirm completeness. Recommend coordinator confirmation before approval; all present decisions, including the underlying single house/continuous tree, apply.

## Differences from the current build

- Bake names/effects; change current 56 px check-canvas sizing to shared 64 px visible glyph sizing.
- Replace current 640 x 900/hard-coded layout with measured-container derivation and cream-contained bar; remove ribbon/text entrance pops, not bird motion.
- Standardize generic 0.15 s notification exit to `panel-close` 0.12 s; settle back behavior.
- Handle Rook without the throwing gift lookup; expose missing/pending/interrupted states in fake data. Preserve all existing discovery/reward/save semantics. No runtime compliance is claimed.
