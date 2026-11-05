# World complete

Status: draft. Stage: screen specification, not runtime approval. Portrait phones; touch, Editor mouse, Android back. Reference 1080×1920 (9:16); also review 1080×2400 (9:20), safe-area aware. No additional keyboard/controller navigation scheme is promised.

Authority: [art direction](../art-direction.md), [patterns](../patterns.md), [brief and all addenda](../../work-log/ui-redesign/brief.md), [conventions](../../CONVENTIONS.md); [game design, Coins/Worlds](../../GAME_DESIGN.md#82-coins). Starting implementation: `PanelsView.Popup/WorldGate`, `PopupFlow`, `PopupMotion`, `GateView`, `MapView`, and `GameRoot` notification orchestration. Style reference: `art/approved_ui/ui_popup_world_complete.png`; its round green next control is superseded by `primary-bar`.

Shared source gap: [brief completeness decision](settings.md) applies here; only two addenda are present in the supplied brief.

## 1. Player need

The player arrives wanting to celebrate finishing a forest, collect the first-completion reward and understand what comes next. They have cleared level 10, 20, 30 or 40 and left the ordinary win panel; this is not another level score calculation.

## 2. Navigation

- The coordinator queues the notification after a first clear of a world's final level with an unpaid world receipt. It appears after the win panel's Next/Home action, over the retained won board or map as appropriate. Replaying a cleared finale does not create another world reward; an earlier earned unpaid receipt can still be pending.
- Arrow bar requests acknowledgement once; logic claims/persists +200 and supplies the final balance. Reward animation must not grant currency.
- Worlds 1–3 then lead to map gate opening and world-unlock for the next world. Any pending daily-gift follows the world sequence. Only after the queue permits it may a deferred Next enter the next level and its new-friend/booster-unlock notifications. Home returns to the map after the same required sequence.
- World 4 ends at the map (and daily-gift if due); no fifth world or level 41. No nested shop, level card or additional confirmation is opened by this view.
- Scrim is inert. Suspend/hide cancels motion only; logic retains earned receipts and deferred navigation. Resume uses the current authoritative request. No timeout or auto-dismiss.

[Decision needed] **Notification back/close policy**, shared with [daily gift](daily-gift.md#2-navigation). Options: keep the current acknowledgement-only exception (no X, back inert); or allow X/back to defer without claiming, requiring coordinator re-presentation. Recommend the explicit exception, preserving current queue/receipt behavior. Layout assumes that choice. Adding a 100 px X beside a 520×120 bar also needs a ruling on the shared primary/sibling ratio; do not silently exempt it.

## 3. Layout

Use `modal-panel`, `stat-pill`, `primary-bar`; celebration art is decoration, not score controls. Measurements: `PerchSort/Assets/Resources/Art/layout_metrics.json` (`tools/layout_metrics.py` output) and `art/approved_ui/layout.json.screens.popup_world_complete` (manual visible bounds, ±8 px). These are geometry sources, not a runtime acceptance certificate.

Panel width is the native measured 800 px. Its cream width is 645; the 28 px primary clearances leave a 589 px content lane. Reflow four centred rows: ribbon/stars, gate/birds, reward, arrow, separated by the modal's 24 px gap, with 28 px cream top/bottom padding. Do not retain the old overlapping reward/gate/button rectangles.

| Component | Pattern | Asset | Size / anchor | Layer |
|---|---|---|---|---|
| Dim / shadow | modal-panel | procedural | viewport stretch / panel silhouette | 0 / 1 |
| Frame | modal-panel | `panel_wood` | 800×1267.86, derived below; safe-area centre | 2 |
| Ribbon + three celebration stars | modal-panel decoration | `ribbon_green`, `star_full` | measured 730×242 group fit uniformly to width 589: 589×195.26; first row | 4 |
| Gate + birds | modal-panel content; world-gate pattern pending | gate assembly below; Sunny, Skye, Berry drawn frames | measured 856×728 slot fit uniformly to width 589: 589×500.93; second row | 3; birds above gate |
| Reward | stat-pill | `stat_pill` + `coin` + live amount | 440×72, third row | 4 |
| Continue | primary-bar | `btn_green_pill` + `icon_next_arrow` | 520×120, last row; 64 px glyph | 5 |

Exact row heights are `242×589/730` and `728×589/856`, not rounded layout constants. Cream height C is their sum `+72+120+3×24+2×28 ≈1016.18`. Existing panel borders are left/right 150, top 110, bottom 100; the measured inner rect is `(88,119,645,317)` in 800×540. Nine-slice vertically at width 800: `H=210+C×330/317 ≈1267.86`; cream top is `110+9×(H−210)/330`. Require ≥24 px exterior safe-area clearance. Start the stack at cream top +28, centred on the cream horizontal centre. Round only at final pixel placement.

Retain relative decoration positions from the measured group: stars `(338,474,128,124)`, `(478,456,130,124)`, `(618,474,130,124)` relative to ribbon `(174,448,730,242)`; birds `(414,690,230,274)`, `(268,896,230,266)`, `(526,944,264,298)` relative to gate `(102,630,856,728)`, all `(x,y,w,h)`. Apply each group's uniform scale, not the current extra 1.6× bird enlargement. Bodies face the player at rest, not a mirror or side-looking still.

Gate assets: `gate_backdrop_w<N>`, `gate_light`, `gate_door_left/right`, `gate_arch`, `gate_padlock` in recorded back-to-front order. Preserve the measured 1024×1024 assembly canvas, hinges `(246,610)/(778,610)` and aperture from `layout.json.assemblies.gates`; contain it without aspect distortion within the gate slot. Completion opens doors without a lock-fall reward cue. Reward coin is uniformly fitted from the measured 128 px coin area of `coin_pill` to the 72 px row height. Number occupies only the transformed `stat_pill.inner=(23,17,354,60)` area minus coin occupancy.

Normalize the primary arrow's opaque longest edge to the pattern's 64 px using `PerchSort/Assets/Resources/Art/glyph_metrics.json` (96×96 canvas, opaque 90×79 at offset 3,8); preserve aspect and compensate the measured centre offset.

[Decision needed] **World-gate pattern.** Library has no gate/reward-celebration entry. Options: add a shared `world-gate` pattern using the measured assembly and source timing in §7; or remove the special choreography and use generic panel motion. Recommend the shared entry before shell implementation; no static whole-gate fallback counts as door animation.

[Decision needed] **Completion backdrop/finale art.** Current `GateView` clamps world 1 to backdrop 2 and otherwise uses the supplied completed-world number; the reference does not establish all four destinations. Options: preview the next forest for completions 1–3 and retain Snow Pine for completion 4; or depict the completed forest throughout (requires a world-1 backdrop not in the recorded gate kit). Recommend next-forest previews plus Snow Pine finale, without inventing a fifth destination. The coordinator supplies the approved backdrop, not the view.

## 4. States

Fake coordinator must explicitly reach:

- First unpaid completion of each world (1–4); first-visit and restored pending receipt; already-paid finale replay (no new popup); stale request after receipt paid.
- Underlying win with 1, 2 and 3 earned stars: this popup's three ribbon stars are a fixed celebration emblem, not a replacement score or additional star award. Include a pending receipt surviving a replay.
- Typical +200; empty/missing reward or request (arrow disabled); zero preview and +99999 numeric stress without changing payout rules. Map/board balances 0, 240, 99799→99999 and 99999→100199.
- Long species/world names supplied to the fake model: no live names appear; no name label may overflow. Maximum world is 4. Locked future forest (no completion notification), next forest unlocked, final-world no-next case.
- Door opening, three living birds at rest/idle, counter running, normal/pressed/released/disabled arrow, closing, committed coin feedback, map gate transition, world-unlock, due daily-gift, deferred next level, then new-friend and booster-unlock where applicable. No other nested popup.
- Home versus Next from the win panel; rapid repeated input; suspend before claim, during close, after persistence and during gate transition. Mute, instant and reduced-motion cases.
- Offline: same local sequence. No server loading/error/retry screen. Missing model disables acknowledgement; missing assembly or bird sequences is an asset failure, not permission to animate a still. Select/move/invalid action/lose are not actions of this completed-world view.

[Decision needed] **Currency display ceiling**, shared with [daily gift](daily-gift.md#4-states). Options: full measured numbers for the real supported range; or an explicitly approved abbreviation policy. Recommend full numbers. A balance of 99999 +200 becomes 100199; never cap/reset the save or truncate to five digits to fit the UI.

## 5. Interaction map

| Input | Event / result | Feedback / sound |
|---|---|---|
| Tap/click enabled arrow | `AcknowledgeRequested(requestId)` once; logic closes, persists and advances queue | `button-press / release`, `panel-close`; `ui-tap` on down; `coin-gain` only on committed counter start; no haptic |
| Tap while celebration still running | same valid acknowledgement; no separate skip/claim action | cancel owned choreography on close; same feedback/sound, not a duplicate payout |
| Disabled/duplicate action, release outside | no acknowledgement | dim/inert or shared release feedback; no disabled/duplicate sound or haptic |
| Scrim / Android back | no event under proposed acknowledgement-only exception | unchanged panel, no sound/haptic |
| Suspend / hide | no game action event | cancel owned motion and callbacks, no sound/haptic |

One actionable control; no promised keyboard/controller focus sequence. No animation callback may choose the next forest, claim coins or start a level.

## 6. View contract

Proposed presentation API:

| Data shown | Logic source / update | Missing |
|---|---|---|
| Reward and acknowledgement availability | current notification + economy; on show/replacement | hide amount, disable arrow; never default to +200 |
| Completed world, backdrop, final-world presentation | progression/coordinator; on show | no invented destination; disable arrow pending valid model |
| Celebration species and approved front-frame assets | presentation catalog; on show, idle continuously | development asset error |
| Underlay currency / won board or map | retained underlay model; after authoritative updates | never construct a synthetic player profile in the view |

Calls: `Show(WorldCompleteModel { requestId, completedWorld, backdropWorld, rewardCoins?, canAcknowledge, celebrationSpecies })`, `Render(model)`, `Close()`, `Hide()`. Events: `AcknowledgeRequested(requestId)`, `Closed`; optionally `DismissRequested` only if defer is approved. Coordinator alone owns payout, persistence, world unlock, map/board coin feedback, queue order and deferred level; the view does not decide whether the next notification exists.

## 7. Motion and sound

| Trigger | Motion / timing / easing | Sound / music |
|---|---|---|
| Open / close | `panel-open` 0.18 s ease-out / `panel-close` 0.12 s ease-in | `win-jingle` on open; preserve underlying `bgm-game` or `bgm-menu`, no popup restart/duck |
| Press / release | `button-press / release`, 0.08 / 0.18 s | `ui-tap`, mute respected |
| Completion doors, pending world-gate pattern | retain `GateView.OpenDoors`: 0.8 s total, door progress after 0.35 s over 0.45 s, existing back-eased swing; no padlock drop | none; no gate sound in shared table |
| Bird celebration, pending pattern | retain 0.8 s flights beginning at 1.15 s, then +0.18 s per bird, ease-out path with drawn 14 fps frames; return to their measured group slots, not permanent disappearance | none; do not invent bird/chirp sound identifiers |
| Birds while panel remains | continuous 2.4–3.2 s breathing; drawn idle actions 1–3 s apart; no mirror/flip; eased hops and hard frame swaps | none |
| Reward preview / committed balance | `counter`, 0.5 s ease-out; preview is not a claim | preview silent; `coin-gain` once when logic starts committed balance feedback |

Celebration stars stay visible from the first frame; no score/star-pop sequence is implied. No pre-acknowledgement coin flight to the real wallet. On actual page change, music uses the shared 1 s crossfade. Reduced/instant motion follows patterns and snaps decorative doors/flights/counters to their final poses; never skips the receipt. Cancel all owned motion on hide. No auto-dismiss.

## 8. Accessibility

NOT ASSESSED: no approved contrast, assistive-input or physical-device target. Assume ≥88 px design-space hit targets; arrow is 520×120, accessible name Continue from completed world. Reward coin and number plus an open gate communicate completion without color alone. Disabled alpha 0.45 is inert, same size. No visible prose or unapproved haptic is added; honor reduced motion.

## 9. Text

Visible inventory: reward `+200` (4 characters), stress `+99999` (6 including plus); underlay coins at least five digits through 99999, with six-digit boundary pending ceiling decision. Numbers only are live. Use one Fredoka size measured against the maximum ink width in the transformed pill inner rect minus coin occupancy; no per-value autosizing. Missing reward hides the number. Ribbon/stars have no title; world and species names are not rendered. Any later localized fixed title would require baked measured art, not live text. No countdown, score label, star-limit labels or chain bonus is added to this separate celebration screen.

## 10. Acceptance criteria

1. At 1080×1920 and 1080×2400 with safe-area insets, all art and the arrow fit the panel; numbers including +99999 and balance 99999 remain wholly inside their containers. The bar has ≥28 px cream clearance left/right/bottom; decorative groups do not overlap reward or controls.
2. Continue has a 520×120 base/hit rectangle in every state; disabled is alpha 0.45 and inert, not smaller. Its opaque white arrow matches other primary glyphs at 64 px, regardless of transparent padding.
3. From frame one, dim alpha 0.72 covers every HUD, board/map and bird object; a separate panel shadow offset 14 px, combined alpha 0.35, visibly lifts the panel from the background.
4. Open settles in 0.18 s, close in 0.12 s; controls are visible immediately. Non-instant completion doors finish at 0.8 s; the last 0.8 s bird flight finishes by 2.31 s. Device frame-time performance remains NOT ASSESSED.
5. During a 10 s timed observation, visible birds breathe continuously and perform drawn actions 1–3 s apart, looking toward the player at rest. No mirrored turn, single-still shake or permanent empty celebration after the fly-in passes acceptance.
6. First unpaid completions award +200 only after one acknowledged receipt. Open, duplicate taps, suspend and replay do not add rewards. At 99799 the result is 99999; the six-digit boundary must be resolved before acceptance.
7. Worlds 1–3 advance through world-unlock before any daily gift/deferred level; world 4 returns to map without a fifth world or level 41. Both win-panel Home and Next routes preserve the same reward guarantees.
8. A one-star finale retains its real one-star result; this popup's three decorative stars never alter that record. Muting silences all effects/music; popup presentation never restarts the page track.

## Differences from the current build

- Reflow the current 880×1260 panel and measured full-size gate into a cream-contained stack; the current gate extends beyond the panel and overlaps the reward region. Remove the extra 1.6× bird enlargement.
- Keep the three celebration birds visibly alive after the initial flight rather than fading/receding all three out. No fallback still counts as motion.
- Replace the 0.15 s generic close / 0.6 s preview count with shared 0.12 / 0.5 s; move the current pre-acknowledgement coin flight to committed reward feedback only.
- Preserve once-only receipts and current queue order, expose view events for the fake shell, and settle notification dismissal, gate pattern, backdrop/finale art and currency ceiling before approval.
