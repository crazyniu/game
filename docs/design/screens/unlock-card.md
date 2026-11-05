# Unlock card

**Status:** draft. **Stage:** screen specification, not UI approval. Portrait phones; touch and Android back, no shipping keyboard/controller navigation. Reference 1080 x 1920; aspect range 9:16–9:20; review viewports 1080 x 1920 and 1080 x 2400 (aspect-derived).

Authority: [art direction](../art-direction.md), [named patterns](../patterns.md), [redesign brief](../../work-log/ui-redesign/brief.md), [booster design](../../GAME_DESIGN.md#81-boosters-skills). Style reference: `art/approved_ui/ui_unlock_card.png`, replacing its round green confirmation with the approved bar. Current owners: `PanelsView.Unlock`, `PopupFlow`, `PopupMotion`, `GameRoot.PumpNotifications/OpenUnlock/AcknowledgeUnlock`; `UiBirdLife` only applies to birds in the underlying board. This card introduces a booster; it does not purchase, grant or use it.

## 1. Player need

The player arrives wanting to know what their newly available tool does and then get back to solving the level. They have completed the level entrance and any new-friend introductions ahead of this notification. The illustration and short effect cue must be understandable without a paragraph.

## 2. Navigation

- Logic opens each unseen, eligible booster introduction after level entrance/new-friend notifications, in the existing booster order. Existing thresholds: Firefly 2, Sapling 4, Owl Eye 7, Morning Bell 12, Scarecrow 22, Spring Rain 32. These are data from `ViewModel`, not view-owned progression rules.
- Check raises acknowledgement once; logic closes, acknowledges the queued notification and chooses the next unlock-card or playable board. No automatic booster use, shop, purchase confirmation, aviary or new-friend popup is opened by this view. A development preview may show any card without writing progress.
- Preserve current once-on-presentation bookkeeping: `PopupFlow.Next` marks `UnlockSeen` before the root saves and shows the card. Do not move this transaction to an animation or acknowledgement callback. Already-seen replay does not reopen it; interrupted app reload follows the saved seen state, not a new view-owned receipt.
- Background taps do nothing. No close-X in this draft. Focus loss suspends music/presentation without acknowledgement; hide/destroy cancels animation and callbacks. No countdown or auto-dismiss.

**[Decision needed: notification back]** Current unlock notifications ignore Android back, whereas patterns say back closes the top modal. Options: retain acknowledgement-only; or make back equivalent to check through the same logic event. Recommend back-as-acknowledgement, consistent with new-friend; settle the shared policy before implementation, without changing `UnlockSeen` persistence.

## 3. Layout

Sizes are design px from patterns and measurements. `PerchSort/Assets/Resources/Art/layout_metrics.json` records `panel_wood` 800 x 540 with inner `(88,119,645,317)`, `name_ribbon` 320 x 100 with inner `(19,12,283,72)`, and `stat_pill.inner` 354 x 60. `art/approved_ui/layout.json`, `unlock_card`, measures eye rect `(444,652,188,128)` and egg `(426,796,238,276)`: their union is `(426,652,238,420)` (individual manual bounds ±8 px). Use this measured illustration envelope, not a guessed 160 px disc or a stretched sprite.

Use native-width, native-border nine-sliced panel: width 800; height `119 + 24 + 420 + 24 + 100 + 24 + 60 + 24 + 120 + 28 + 104 = 1047`. The bottom inset 104 is `540−119−317`; 24 spacing is `modal-panel`, bottom clearance 28 is `primary-bar`. Cream inner rect is `(88,119,645,824)`. Center the panel in the safe area with at least 24 px clearance; stack illustration, name ribbon, effect and check on the inner rect's center line from inner-top +24, 24 px apart. If safe-area fit requires scaling, scale the reference canvas uniformly, never individual buttons.

| Component | Pattern | Asset | Size | Anchor / position | Layer |
| --- | --- | --- | --- | --- | --- |
| Shield/dim | modal-panel | black overlay | full physical viewport | stretch beyond safe area | above every world/HUD/bird object |
| Shadow and cream/wood panel | modal-panel | `panel_wood` | derived 800 x 1047 | safe-area center | shadow then panel |
| Booster illustration | **new pattern needed: booster illustration** | `ViewModel.Illustration(kind)`: existing `booster_*_illus` | measured envelope 238 x 420; fit, preserve proportions | first stack slot, non-interactive | content |
| Baked booster name | name-ribbon | derived name bake on `name_ribbon` | native measured 320 x 100 | second slot, text in measured inner | content |
| Baked effect | modal-panel fixed content | derived effect bake | measured `stat_pill.inner` envelope 354 x 60, not an extra live-text pill | third slot | content |
| Acknowledge | primary-bar | `btn_green_pill`, `icon_check` | pattern 520 x 120, visible check 64 | last slot, cream bottom clearance 28 | input topmost |

No prices, stock, starter-gift claim, unlock-level badge, currency counter, second action or extra title. Underlying HUD remains dimmed and non-interactive. One main action means no sibling-size exception is required.

**[Decision needed: booster illustration pattern]** The library does not define non-interactive booster hero art. Options: adopt the measured eye/egg union as the common envelope for existing booster illustrations; or approve/measure a different shared display size. Recommend the measured envelope, with one shared pattern added before shell work and no regeneration of approved originals.

**[Decision needed: modal dim]** Brief requests about 45% black; patterns/art direction/conventions require alpha 0.72. Options: retain 0.72; or revise the shared rule to 0.45. Recommend 0.72 for every modal, pending ratification; do not interpret display brightness as overlay alpha.

## 4. States

Fake source/state switcher must explicitly cover:

- Hidden/empty queue; typical Owl Eye; all six booster cards individually; first unlock at each canonical threshold; later unseen/catch-up introduction; already-seen replay (no popup); locked/future booster (no popup).
- Maximum synthetic backlog: all six unseen boosters after the new-friend queue, shown one at a time; unlock-card → next unlock-card → board. This is a development fixture, not a claim that all boosters normally unlock together. No nested popup is opened.
- Longest real name `Morning Bell` (12 characters), a same-length wide-glyph bake, all six effects including the 20-character maximum; every illustration's aspect extremes inside the same measured envelope.
- Underlying balances 0, reference-typical 240, and 99999; zero versus stocked tool behind the dim. None changes the ability to acknowledge or adds a buy/claim control.
- Normal, held, release-outside, disabled/pending acknowledgement, rapid repeated tap, closing, focus loss/resume, forced hide/reopen during open/close, muted/unmuted and instant motion. App reload after once-on-presentation persistence must not invent an unseen card.
- Missing/unknown kind, required illustration, name or effect bake: diagnostic fixture, no unrelated tool fallback; do not show a false production card. Data pending means hidden until supplied. Offline is normal bundled-content display, not a network-loading/error/retry modal.
- Underlying selected/unselected board, late-world busy board and delivered birds remain visually dimmed and logically unchanged. No move, invalid action, gift, win/lose or reward transition belongs to this card. No timer/auto-dismiss.

## 5. Interaction map

| Input | Event / result | Feedback and sound |
| --- | --- | --- |
| Check touch/release inside | `AcknowledgeRequested(notificationId, boosterKind)` once; wait for logic response | `button-press / release`, `ui-tap` on enabled pointer down, then `panel-close` |
| Drag off check/release outside | no acknowledgement | `button-press / release` restores rest; initial `ui-tap` only |
| Disabled/pending check or repeated exit input | no extra event | inert dimmed control / current exit; no new motion or sound |
| Illustration, title, effect, cream or dim tap | no event | no feedback/sound; card remains open |
| Android back | shared policy in §2 | approved back-as-check uses `panel-close`, no pointer sound; current ignore policy has no effect/sound |
| Focus loss/resume | suspension only | existing music pause/resume, no replayed UI sound |

No haptics. There is only one focusable action; shipping keyboard/controller navigation requires separate approval, not implicit new bindings.

## 6. View contract

| Data shown | Source / update | Missing behavior |
| --- | --- | --- |
| Booster identity, illustration, name bake and effect bake | catalog/art adapter at every `Show(UnlockCardPresentation)` | diagnostic state; no made-up effect or fallback booster |
| Notification identity, show eligibility and acknowledge enabled/pending | notification coordinator on show/change | hidden without valid identity/model; view does not calculate unlock thresholds |
| Motion/mute/focus context | existing settings/presentation/audio service on change | retain existing settings; no new control |

Logic calls `Show(UnlockCardPresentation)`, `SetAcknowledgementEnabled(bool)`, `SetPresentationSuspended(bool)`, `Close(completed)` and `Hide()`. View events: `AcknowledgeRequested(notificationId, boosterKind)` for an accepted check/back route; lifecycle-only `Closed(notificationId)`. These names specify the future shell boundary, not implementation work in this assignment. The view receives resolved asset IDs/display flags, never owns inventory, eligibility, coins, starter grants, `UnlockSeen`, queue order or saves. `Closed` cannot grant or mark anything. Fake coordinator responses exercise every state without writing profile data; real logic is connected only after approval.

## 7. Motion and sound

| Trigger | Pattern | Sound / music |
| --- | --- | --- |
| Show | `panel-open`, one shared panel transition; every control/label visible from first frame | no invented unlock jingle; retain underlying `bgm-game`, no restart or duck |
| Check | `button-press / release` | `ui-tap` on enabled pointer down |
| Acknowledged exit | `panel-close`; cancel all completion callbacks on hide/destroy | no coin-gain, booster-use or additional close sound: this is information, not a grant/use |
| Focus / mute / instant motion | existing audio settings; `GameMotion.Instant` and pattern reduced-motion transitions | current single mute applies to music/UI; pause on focus loss |

The illustration may be static because it is a tool, not a bird. Any visible underlying birds still follow their board motion contract; the modal must not create a frozen overlay copy. No per-control entrances, flashing labels or speculative reward animation.

## 8. Accessibility

**NOT ASSESSED:** no conformance or numeric contrast target is supplied. Assume minimum reference touch target 88 x 88; the check target is 520 x 120, to be rechecked after safe-area scaling. Accessible name “Continue”; non-visible tool/effect description comes from catalog semantics. Fixed text follows Fredoka Medium and the art direction's cream/brown treatment. Identity is carried by illustration and text, not color alone. Reduced-motion panel behavior is the pattern's instant transition; this popup introduces no bird-motion exception or new settings control.

## 9. Text

| Element | Allowed fixed text / maximum |
| --- | --- |
| Baked name | Firefly, Sapling, Owl Eye, Morning Bell, Scarecrow, Spring Rain; maximum 12 characters including space; native ribbon inner `(19,12,283,72)` |
| Baked effect | current short cues below, maximum 20 characters including spaces/punctuation; measured 354 x 60 envelope with clear ink margin |
| Check, heading, unlock-level, price, inventory, starter count | icon-only check; all other listed text absent |
| Underlying coin-pill | HUD-owned live number only, 1–5 digits through 99999; no duplicate balance in modal |

| Booster | Fixed effect (existing short copy) | Characters |
| --- | --- | --- |
| Firefly | Find a helpful move | 19 |
| Sapling | Add one branch | 14 |
| Owl Eye | See inside every egg | 20 |
| Morning Bell | Wake sleeping birds | 19 |
| Scarecrow | Send crows away | 15 |
| Spring Rain | Grow branches fully | 19 |

These effect cues describe existing behavior; no paragraph or new rule is implied. Fixed text must be baked, never a live `TextFit` name or effect. **[Decision needed: localization/type bakes]** Neither locales nor a common bake type-size token is defined. Options: approve English bakes with a common measured ink size first; or provide locale-specific measured bakes now. Recommend English-first approval within these same containers, then measure each locale; no runtime autosizing/truncation or live fixed-text fallback.

## 10. Acceptance criteria

1. At 1080 x 1920 and 1080 x 2400 with safe-area simulations, the 800 x 1047 reference panel and all children fit. `Morning Bell`, the wide-glyph stress bake and `See inside every egg` stay fully inside ribbon/label containers with margin, no clipped outlines and no per-item shrinking.
2. The check's layout and hit box remain 520 x 120 in normal, held, disabled and pending states; only pattern visual press scaling changes. Its visible glyph is uniformly 64 px, matching new-friend. Disabled alpha is 0.45; it cannot raise acknowledgement. No other same-role button uses another size.
3. The panel is clearly separated from every world, HUD and bird object by a full-screen dim and soft shadow. Pending dim decision: black alpha 0.72, shadow downward offset 14 px and composite alpha 0.35. Background taps neither close the card nor affect gameplay.
4. Opening completes in 0.18 s and closing in 0.12 s from their presentation triggers, with all controls visible from frame one. Device frame-time performance is NOT ASSESSED without a target device budget.
5. All six eligible fake cards have the correct illustration/name/effect. Locked or already-seen requests produce no card; the synthetic six-card queue drains one at a time and resumes the board. No shop/confirmation or other nested popup appears.
6. Acknowledgement raises one event despite rapid taps or interrupted open/close; no inventory, coins, move count, star target or selected branch changes. Starter inventory and once-on-presentation seen bookkeeping remain logic-owned and unchanged by view callbacks.
7. Balances 0, 240 and 99999 do not change this information card or acknowledgement availability; the underlying 99999 remains inside its coin pill. No live name, effect, price or stock text is introduced.
8. Unmuted `bgm-game` continues through open/close; existing mute/focus handling works, and timestamped audio evidence places `ui-tap` at enabled pointer down with no unlock-grant/use sound. Timed frames show any visible board birds still living, not replaced by static popup copies.

**[Decision needed: brief completeness]** The assignment refers to three addenda; the supplied brief contains two (gameplay visibility and continuous tree painting). Options: provide the third; or confirm completeness. Recommend coordinator confirmation before spec approval. Both present addenda apply, including the underlying single house and continuous painted tree.

## Differences from the current build

- Replace current 640 x 700/hard-coded content with the measured-container-derived panel and full booster illustration rather than a 160 px wood disc with a tool glyph.
- Bake the existing short name/effect copy; keep the check bar inside measured cream margins and normalize its current 56 px canvas sizing to the shared 64 px visible check rule.
- Replace the generic 0.15 s notification close path with `panel-close` (0.12 s) and settle Android-back policy; retain existing queue/seen persistence and absence of extra close control.
- Provide the complete fake-state surface before logic wiring; the existing code is source evidence, not proof that this draft already passes runtime acceptance.
