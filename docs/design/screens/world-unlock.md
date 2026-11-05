# World unlock

Status: draft. Stage: screen specification, not runtime approval. Portrait phones; touch, Editor mouse, Android back. Reference 1080×1920 (9:16); also review 1080×2400 (9:20), with safe areas. No additional keyboard/controller scheme is promised.

Authority: [art direction](../art-direction.md), [patterns](../patterns.md), [brief and all addenda](../../work-log/ui-redesign/brief.md), [conventions](../../CONVENTIONS.md), [game design, Worlds/navigation](../../GAME_DESIGN.md#9-worlds-and-scenes-every-level-looks-different). Starting implementation: `PanelsView.WorldGate/Popup`, `PopupFlow`, `PopupMotion`, `GateView`, `MapView.UnlockGate`, `GameRoot.PumpNotifications`. Style reference: `art/approved_ui/ui_popup_world_unlock.png`; purple controls and round green play are superseded.

Shared source gap: [brief completeness decision](settings.md) applies here; only two addenda are present in the supplied brief.

## 1. Player need

The player arrives wanting to see the forest they have just unlocked and continue the journey. They have acknowledged the previous world's completion reward and watched the corresponding map gate open.

## 2. Navigation

- Logic presents this notification for newly unlocked worlds 2, 3 or 4 only, after world-complete acknowledgement and the map's gate-opening presentation. World 1 has no unlock notification; there is no world 5.
- Current queue contract: acknowledge clears this notification; then due daily-gift is shown before a deferred Next loads level 11, 21 or 31. If the player chose Home on the win panel, there is no deferred level: acknowledgement leaves them on the map after other due notifications. Subsequent level entry can show new-friend and booster-unlock; this popup never bypasses that queue.
- No coin reward, purchase, cost, lock override or eligibility calculation belongs here. The gate animation depicts a logic-authorized unlock; it does not perform it. No nested shop, level card or confirmation is opened by this view.
- Scrim does nothing. Focus loss pauses audio; hide/destroy cancels owned presentation without discarding the pending acknowledgement. Logic reconstructs the current request on resume; no auto-dismiss.

[Decision needed] **Notification back/close policy**, shared with [daily gift](daily-gift.md#2-navigation). Options: retain the current acknowledgement-only exception (no X, Android back inert); or allow X/back to defer without acknowledgement, requiring coordinator re-presentation. Recommend the explicit exception to preserve current behavior. The table assumes it; adding a 100 px close disc also needs a decision on the shared primary/sibling size rule.

[Decision needed] **Meaning of the play glyph.** The current button acknowledges; it does not always start a level. Options: retain current queue routing and use a check when there is no deferred level (play when there is); or make play always enter the new forest's first level, changing the Home route and queue contract. Recommend conditional check/play with unchanged routing. The coordinator must supply the approved action glyph/accessible name; the view must not infer navigation from world number. Pending approval, preserve current routing rather than promise unconditional play.

## 3. Layout

Use `modal-panel` and `primary-bar`. Sources: `PerchSort/Assets/Resources/Art/layout_metrics.json` (`tools/layout_metrics.py` output); `art/approved_ui/layout.json.screens.popup_world_unlock` (manual visible bounds, ±8 px) and its gate assembly. No dimensions below are guessed from the displayed reference preview.

The measured native panel is 800×540 with cream `(88,119,645,317)`, sprite borders left/right 150, top 110, bottom 100. Keep width 800; a 28 px cream inset gives a 589 px lane. Uniformly fit the measured 920×904 gate composition to that lane: 589×`904×589/920` = 589×578.76. Put a 520×120 `primary-bar` in a separate row below it, with 24 px gap and 28 px top/bottom cream padding. Required cream height C is `904×589/920+120+24+56 ≈778.76`; actual sliced panel height is `210+C×330/317 ≈1020.69`. Cream top is `110+9×(H−210)/330`; cream x/width remain 88/645. Centre panel in the safe-area rectangle with ≥24 px exterior clearance; rows are centred on the cream area, starting at its top +28. Round only at final pixel placement.

| Component | Pattern | Asset | Size / position | Layer |
|---|---|---|---|---|
| Dim / shadow | modal-panel | procedural | viewport stretch / under panel | 0 / 1 |
| Frame | modal-panel | `panel_wood` | 800×1020.69, derived above; safe-area centre | 2 |
| Gate composition | modal-panel content; world-gate pattern pending | `gate_backdrop_w2/w3/w4`, `gate_light`, `gate_door_left/right`, `gate_arch`, `gate_padlock` | 589×578.76 slot, first row | 3, ordered as named |
| Forest plaque | modal-panel decoration | `world_sign_w2/w3/w4` | measured 338×184 scaled by 589/920; relative offset `(298,76)×589/920` from gate slot top-left | 4 |
| Living Sunny | modal-panel character content | Sunny front-facing drawn sequences | measured completion-reference 230×274 footprint scaled by 589/920; centre at the measured gate aperture `(512,375)` mapped from its 1024 canvas | 4, inside opening |
| Continue / play | primary-bar | `btn_green_pill` + approved `icon_check` or `icon_play` | 520×120, second row; 64 px glyph | 5 |

Keep the gate's shared 1024×1024 canvas and measured hinges `(246,610)/(778,610)`; contain the assembly uniformly in its slot, never widen a bird or skew a door to fill a rectangle. Plaque source anchor is `(374,760)` relative to the measured gate `(76,684)`; it uses a forest symbol, not a live world name. Preserve existing plaque artwork; decorative rope sizing must follow its recorded asset, not an extra arbitrary live label. The bird footprint is borrowed from `layout.json.screens.popup_world_complete.hero_bird_yellow`, not the current unmeasured 190 px hover constant. All frame sequences share the same pivot and contain no mirrored turn.

Normalize check/play to the same 64 px opaque longest edge using `PerchSort/Assets/Resources/Art/glyph_metrics.json`: check canvas 96×96, opaque 90×69 at 3,13; play canvas 96×96, opaque 80×92 at 7,2. Preserve aspect and compensate each measured centre offset; equal canvas sizes alone do not satisfy uniformity.

[Decision needed] **World-gate pattern**, shared with [world complete](world-complete.md#3-layout). Options: add a shared measured `world-gate` pattern retaining the source timings in §7; or remove the special door/lock choreography and use generic panel motion. Recommend the shared entry before shell implementation. A whole-gate still fade is not animated opening, and cannot establish acceptance.

## 4. States

The fake coordinator must reach every case:

- First unlock of worlds 2, 3 and 4; resumed pending request; later already-acknowledged visit (no popup); world 1 and attempted world 5 (no unlock notification).
- Locked future gate (no unlock notification), map camera focus, lock release/map gate opening, popup closed-gate start, popup opening and fully open; restored model may request the final open pose without replaying progression.
- Typical Home route with no deferred level; Next routes to 11/21/31; due daily-gift before deferred entry; subsequent new-friend and booster-unlock. This popup opens no other nested popup.
- Empty/missing request, missing world/art key (action disabled, no invented forest); valid world, maximum world 4; long world/species strings (not rendered as live text). No list, stock or reward number exists here.
- Underlay balances 0, 240, 99999, unchanged by unlock. There is no unlock cost, +0 reward label or new currency cap; a six-digit balance supplied by the coordinator follows the shared currency decision in daily-gift/world-complete.
- Normal, held, released, disabled and closing action; tap during door opening; rapid repeats; suspend during map focus, lock fall, modal open, acknowledgement close and subsequent queue transition. Mute, reduced-motion and instant modes.
- Offline: same local flow. No server loading/error/retry screen. Missing asset/sequence is a development failure, not an accepted fixed-picture fallback. Select/move/invalid/win/lose are not actions inside this popup; the preceding win belongs to the other screen.

## 5. Interaction map

| Input | Event / result | Feedback / sound |
|---|---|---|
| Tap/click enabled main action | `AcknowledgeRequested(requestId)`, once; coordinator closes and routes the queue | `button-press / release`, `panel-close`; `ui-tap` on pointer down; no haptic |
| Tap while doors/bird animation runs | same acknowledgement, no separate skip or unlock event | close cancels decoration; same button feedback/sound; no double route |
| Disabled/duplicate action, release outside | no acknowledgement | dim/inert or shared release; no disabled/duplicate sound or haptic |
| Scrim / Android back | no event under proposed acknowledgement-only exception | panel remains, no sound/haptic |
| Suspend / hide | no game action event | cancel owned motion/callbacks; no sound/haptic |

One actionable control; no promised keyboard/controller focus sequence. Door/lock/bird/plaque are not buttons. The fake view must not award a reward or emit a level-start request as a side effect of an animation.

## 6. View contract

Proposed presentation boundary:

| Data shown | Source / updates | Missing |
|---|---|---|
| Forest backdrop and symbol plaque | progression notification + presentation catalog; on show/replacement | do not clamp to a different world; disable action pending valid model |
| Request/action glyph and availability | coordinator's current request and approved Home/Next presentation; on show/replacement | no request means no acknowledgement |
| Gate start/final pose | coordinator presentation command, not unlock rules | do not open a locked world based on appearance |
| Bird frames | presentation catalog, continuous while visible | development asset error, never a mirrored still |
| Underlay balance/map | retained map model, independent update | do not invent profile or reward |

Calls: `Show(WorldUnlockModel { requestId, world, gatePose, canAcknowledge, actionGlyph, accessibleActionName })`, `Render(model)`, `PresentGateOpening()` (visual only), `Close()`, `Hide()`. Events: `AcknowledgeRequested(requestId)`, `Closed`; `GatePresentationFinished` may report visual completion but never authorize progression. Add `DismissRequested` only if defer is approved. The map gate's separate `UnlockGate(world, completion)` remains a coordinator-driven presentation step; logic owns queue ordering, persistence and deferred level selection. No UI-held progression state or direct `GameRoot.LoadLevel` call.

## 7. Motion and sound

| Trigger | Motion / timing / easing | Sound / music |
|---|---|---|
| Preceding map gate, pending world-gate pattern | retain `MapView.UnlockGate`: 0.5 s smoothstep camera focus, then `GateView` map doors 0.9 s; lock release 0.2 s, light reaches full in 0.6 s | none; no gate-specific sound in the shared table; `bgm-menu` continues |
| Modal open / close | `panel-open` 0.18 s ease-out / `panel-close` 0.12 s ease-in | none; no music restart/duck |
| Popup lock/doors, pending pattern | retain start delay 0.35 s, then `GateView.Open` 0.8 s: shake first 0.25 s, lock falls 0.25–0.55 s, door swing 0.35–0.8 s with existing back-eased progress | none; do not invent unlock sound identifiers |
| Press / release | `button-press / release`, 0.08 / 0.18 s | `ui-tap`, follows mute |
| Sunny while visible | drawn flap/idle sequences, 14 fps for flight; breathing period 2.4–3.2 s and drawn idle actions 1–3 s apart, eased path/hops | none |
| Actual map → level | level entrance uses shared `level-entrance`, 3.5–4.5 s, tap skips; it belongs to the next screen | `bgm-menu` → `bgm-game`, shared 1 s crossfade |

No per-control entrance; action visible immediately and may acknowledge during decoration. Reduced/instant motion snaps map focus/gate/lock to final poses and follows panel patterns, without acknowledging automatically or altering the queue. Continuous bird motion uses drawn frames, never mirroring/shaking one still. Stop owned motion on hide; focus loss pauses music. No countdown or auto-dismiss.

## 8. Accessibility

NOT ASSESSED: contrast, assistive-input and physical-device target are not approved. Assume ≥88 px design-space hit targets; the only action is 520×120. Accessible name supplied by coordinator: Continue to map / Continue to next level, matching the approved action decision; no visible label. Door opening and forest symbol communicate the new destination without color alone; disabled is alpha 0.45, inert, same size. Honor reduced motion; no unapproved haptic behavior.

## 9. Text

Visible popup text inventory: none. The forest plaques are existing baked symbolic art, not world-name text; no title, unlock requirement, reward, price, timer, level number or species name is rendered. Long strings in fake data must not create live labels. Underlay balance is live digits, minimum stress 99999 (five); no mutation on unlock. If localized forest names are later requested, bake each into separately measured plaque art; never introduce a live name here without a revised text budget. Primary icon is a sprite, not a text character.

## 10. Acceptance criteria

1. At 1080×1920 and 1080×2400 with safe-area insets, the panel, full gate composition, plaque, bird and button are fully visible. No text, including the dimmed 99999 balance, extends beyond its own container; no live long-name label appears.
2. The action has a 520×120 base/hit rectangle in normal, disabled and closing states. Disabled means alpha 0.45 and inert, never smaller; only shared transient press feedback scales art. Check/play glyphs share the same 64 px opaque longest edge and centring, independent of canvas padding.
3. Panel cream clearance is ≥28 px left/right/bottom around the primary bar; gate decorations do not overlap it. Black dim alpha 0.72 covers all map/HUD/bird objects from the first frame; shadow offset 14 px, combined alpha 0.35, clearly separates panel/background.
4. Modal open settles within 0.18 s; close within 0.12 s, all controls visible on frame one. Popup opening finishes by 1.15 s after presentation start (0.35+0.8). Preceding map focus/open takes 0.5+0.9 s without external transition time; device frame-time performance is NOT ASSESSED.
5. Worlds 2, 3 and 4 show their respective approved backdrop/plaque; there is no world-1/world-5 notification. During a 10 s observation, Sunny breathes continuously and performs drawn actions 1–3 s apart, with no mirrored turn or static fallback.
6. Acknowledging changes no coin balance, claims no completion reward again, and emits one request even after repeated taps or interrupted door animation.
7. Home returns to the map after due notifications; Next enters 11/21/31 only after any daily gift and then follows the normal new-friend/booster-unlock queue. Do not label unconditional play as accepted until the glyph decision is settled.
8. Suspend/resume preserves the coordinator's pending request and stops stale callbacks. Muting silences effects/music, and showing this popup never restarts menu music or begins game music early.

## Differences from the current build

- Replace the current 880×1260 panel, overflowing 920×904 gate and overlapping bottom action with the measured cream-contained two-row stack; keep gate parts uniformly scaled and retain their measured hinges.
- Use the source-derived front-facing bird footprint and living frames, not the unmeasured 190 px hover size. Match primary glyphs to the shared 64 px opaque standard rather than `PopupMotion.PrimaryBar`'s 56 px canvas assignment.
- Use shared 0.12 s close instead of the generic 0.15 s close. Preserve the map gate → notification → daily/deferred-level ordering and zero reward.
- Separate view events from root navigation; settle notification dismissal, conditional check/play and shared world-gate pattern before shell approval. Missing gate parts must not pass through the current whole-gate fade/scale fallback as completed animation.
