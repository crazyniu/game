# Sleeping bird bubble

**Status:** draft. **Stage:** screen specification. Nonmodal board tooltip; portrait iOS/Android touch and Editor mouse. Reference 1080 × 1920; 9:16–9:20; review at 1080 × 1920 and 1080 × 2400 with safe-area insets. No runtime approval claimed.

## 1. Player need

The player arrives wanting to understand why a tapped sleeping bird will not move and what can wake it. They were selecting a branch or bird, not opening a tutorial or changing rules.

## 2. Navigation

A refused selection of an effectively sleeping bird opens one bubble attached to that bird; another sleeper replaces it. A same-species landing remains legal and does not trigger refusal merely because the destination bird sleeps. Auto-dismiss after the `tooltip/bubble` 1.6 s lifetime; dismiss on next pointer-down, wake/removal, board change, new session, busy/airborne transition, modal opening, map entry or owner hide. The dismissing tap continues to the board/HUD; this bubble is not a second tap gate. Android back remains the host board's action and dismisses the tooltip as part of interruption. No close button, navigation, shop, purchase, goal-info or other popup originates here.

## 3. Layout

Reuse [patterns](../patterns.md) `tooltip/bubble`; all icons are informational, not `wood-disc` buttons. Layer: world/branches → birds → bubble and tail → HUD/modal stack; no dim/input blocker. Use cream fill with visible rim/shadow so the bubble is separated from forest/birds without dimming gameplay. Modals must cover it, never the reverse.

| Component | Pattern / asset | Size source | Anchor/position |
|---|---|---|---|
| Cream body | tooltip/bubble; `stat_pill` | measured native 400 × 96; `layout_metrics.json` inner `(23,17,354,60)` | above tapped bird, safe-clamped; content entirely in measured inner |
| Matching landing group | tooltip/bubble; two species portraits + `icon_next_arrow` | each visible glyph 56 px minimum from art direction; group width 3 × 56 = 168 | first group for authored sleep; portraits then directional arrow, no visible words |
| Homecoming song | tooltip/bubble; `icon_home` | same 56 px visible extent | after landing group with 24 px pattern margin |
| Night wake | tooltip/bubble; `dial_moon` | same 56 px visible extent | sole centred icon for nocturnal-day-only; with both causes, add after song with 24 px gap |
| Tail | tooltip/bubble; measured production asset required | unresolved below | points to actual tapped bird after clamping, never another seat |
| Sleeping pose / Zzz | host bird presentation; species sleep frames, `zzz` | host board's approved bird geometry, not rescaled by tooltip | same seat; colours/identity retained |

For both causes the row occupies `168 + 24 + 56 + 24 + 56 = 328` px, inside the measured 354 px content width; vertically center 56 px glyphs inside the 60 px inner height. Use `glyph_metrics.json` opaque bounds for measured arrows/home glyphs, not their square canvases. Missing portrait/moon metrics must be measured, not visually guessed.

**[Decision needed] Bubble geometry/pattern completion.** Library specifies a cream tailed bubble and lifetime, but no tail dimensions, edge clearance or bird offset; current code's 300 × 120 body, 30 px rotated-square tail and scale-dependent offset are not measured assets. Options: measure/approve a complete reusable bubble at the 400 × 96 measured body size, or retain current geometry after measurement and explicit approval. **Recommend the reusable measured body/tail**, anchored above the bird and clamped inside safe area with 24 px clearance. Record tail and offset measurements in the shared pattern before implementation; do not stretch a pill to fake an approved tail.

## 4. States

Every case must be reachable from the fake board presenter, without the bubble computing sleep rules:

- Hidden/no target; ordinary awake bird (no bubble); egg (not a sleeper); authored sleeper with landing/song icons; nocturnal daytime sleep with moon only; both causes with all groups; same nocturnal bird awake at night or in a level without day/night (hidden unless authored sleep remains).
- First refused tap and repeat taps, another sleeper tapped, timer just opened/just before expiry/expired; one bubble only. Include smallest and largest board densities, left/right edge, highest/lowest seat, mama/chick sleeper and all affected species.
- Authored wake, phase wake, transfer/homecoming, undo restoring sleep, restart/new session, entrance, flight, win and map/modal interruption; selection cancelled by controller after refusal, with no successful move/count change.
- Missing model/target/art → hide, not misleading empty advice; instant/reduced motion, focus pause/resume and mute. No asynchronous load/error/offline UI; assets/state are local.
- Host stress fixtures: empty board (hidden), typical board, maximum authored level density, locked/unavailable and zero-stock tools, long host labels and 99999 coins. They must not overlap the bubble or become tooltip-owned controls. This screen has no names, coins, owned inventory, locked card, reward, lose state or popup of its own.

## 5. Interaction map

| Input | Event/result | Feedback / sound |
|---|---|---|
| Tap/release sleeping source | board emits `SleepingBirdTapped(birdId)`; controller cancels selection and calls show | sleeping pose remains; drawn refusal response, Zzz emphasis; `refuse` |
| Tap a second sleeper | same event; controller replaces model and restarts lifetime | same refusal feedback; one `refuse`, no stacked bubble |
| Pointer down elsewhere/on bubble | `DismissRequested(nextInput)`; host input continues | bubble disappears; silent itself; any real HUD action uses `ui-tap` and its own effect sound |
| Timer expiry | `DismissRequested(timeout)` | hide; silent |
| Wake/board invalidation/modal/back | controller calls hide; no duplicate player event | hide; silent; host owns any back/modal sound |

Bubble has no hit targets/focus stops, drag action or haptics. Mouse and touch share behavior. No keyboard/controller selection is added; Android back stays with the board.

## 6. View contract

Proposed calls: `Show(SleepHintModel)`, `SetAnchor(birdId,screenBounds)`, `Hide(reason)`. Model contains bird/session revision, species portrait key, effective sleep, authored/night reason flags, eligible explanation icon keys, screen anchor bounds and visible lifetime. Controller sources effective sleep/eligibility from Core (`BoardState.IsAsleep`, phase and authored state), never from displayed pose or tint. Update on refused tap, movement, phase/wake resolution and screen change. Missing/unavailable target, reason or required asset hides the hint; never imply a reason the controller did not supply.

Events: `DismissRequested(reason)` only from user dismissal/expiry; `SleepingBirdTapped` belongs to host board input, not an invisible bubble button. View may own elapsed presentation time, not sleep, selection, moves, booster stock or wake logic. It cannot wake a bird, spend a booster or mutate a session.

## 7. Motion and sound

| Trigger | Animation / timing | Sound / music |
|---|---|---|
| Open/replace | body and all icons visible together; `tooltip/bubble` 1.6 s lifetime, no stagger | `refuse` on refused tap; keep `bgm-game` continuously |
| Refusal response | current response window 0.35 s; draw a sleep-safe reaction sequence, not body translation/shake of a still | same single `refuse`, not a second sound |
| Sleeping idle | continuous breathing 2.4–3.2 s, sleep-safe drawn idle actions every 1–3 s; Zzz may drift/fade over current 1.6 s cycle | silent |
| Expiry/input/interruption | immediate hide, cancel pending callback/reaction | none; no music change |

**[Decision needed] Refusal animation.** Current code bobs a sleeping still; no shared drawn refusal sequence is identified. Options: produce a drawn sleepy reaction or retain only breathing plus Zzz emphasis until such art exists. **Recommend a drawn sleepy reaction** within the existing 0.35 s window; exact frame cadence follows delivered 10–14 fps sequence, never a mirrored/translated still. This is an asset gate, not permission to fake animation.

Reduced motion/`GameMotion.Instant`: no breathing, drift or reaction, stable icons for 1.6 s; dismissal unchanged. One mute control silences refusal/music; focus loss pauses audio, invalidation prevents stale callbacks on resume.

## 8. Accessibility

**NOT ASSESSED:** no project contrast, screen-reader or physical-device target. Host interactions assume ≥88 × 88 design-px targets; tooltip itself is not interactive. Sleep is visible via closed eyes/Zzz and reason icons, never colour alone. Accessible nonvisible description exposes the supplied sleep reasons. No instruction relies solely on the refusal motion or sound.

## 9. Text

No live or baked explanatory sentences, species names, numbers or timer label: zero text characters. Zzz is fixed baked art (at most three Z glyphs), not a live label. Any accessible reason string is metadata, not visible text. New localized explanatory copy would require a separate decision and remeasured asset, not a runtime text box.

## 10. Acceptance criteria

1. At 1080 × 1920 and 1080 × 2400, edge/high/low-seat fixtures keep the complete bubble in the safe area and its tail pointing to the tapped bird; no icon/text crosses its container.
2. Landing group, song and moon have uniform 56 px visible glyph extents after opaque-bound normalization; combined reasons fit the measured 354 × 60 inner region without collision.
3. A refused tap leaves moves, coins, sleep and inventory unchanged; the same-species destination landing still works. Awake birds and eggs do not show sleeper advice.
4. Exactly one bubble shows for 1.6 s from the latest refused tap; expiry/next input hides it without swallowing a legal move or HUD action. No delayed icon entrance.
5. No tooltip control shrinks or enlarges host buttons: every same-role HUD tool remains 108 × 108 design px in all states; disabled is dimmed/inert, not smaller.
6. Cream body, rim and shadow clearly separate advice from forest/birds; the bubble does not darken or block the board, and any modal renders above it.
7. Authored, night-only and combined fixtures show their respective reasons; wake/session replacement immediately removes stale advice.
8. A timed capture proves sleep breathing/drawn response, no mirrored or shaken still, and uninterrupted level music; mute silences both refusal and music.

## Differences from the current build

- Replace the unmeasured 300 × 120 stretched pill/tail with the approved measured bubble geometry; current 52 px portraits and 24 px arrow are below the glyph minimum.
- Current refusal translates the bird body by up to 6 px; use an approved drawn response. Sleeping `BirdView` currently uses faster 1.4–2 s, 3% uniform breathing and skips awake idle actions; supply sleep-safe motion instead.
- Replace `Sfx.Sleep` refusal routing with the pattern's `refuse` mapping, once owned by the audio/controller integration; UI never dispatches both.
- Retain icon-only reason distinctions and 1.6 s lifetime; make wake/target invalidation explicit and move rule reads from `SleepBubble` into its supplied model.
