# Stuck hint

**Status:** draft. **Stage:** screen specification. Nonmodal board advice, not a lose screen. Portrait iOS/Android touch, Editor mouse and Android back on host. Reference 1080 × 1920; 9:16–9:20; review at 1080 × 1920 and 1080 × 2400 with safe-area insets.

## 1. Player need

The player arrives wanting a gentle route forward when a move has left no legal transfers. They were playing; they must remain free to undo or use available tools, without a forced purchase or failure state.

## 2. Navigation

Host controller detects committed changed state, undo available and no legal move; after board motion settles and the existing 0.8 s delay, show the hint pointing toward the real Undo control. Never show during entrance, flight, busy resolution, map, modal, win or a fresh session. Dismiss on next pointer-down, state/inventory/session change, modal, navigation or owner hide; dismissing input still reaches the real board/HUD. No hint-owned buttons or popup. Android back remains host navigation and invalidates advice.

**[Decision needed] Hint lifetime.** `tooltip/bubble` specifies 1.6 s; current `StuckHint` remains until tap/state change. Options: shared 1.6 s auto-dismiss, or an explicitly documented persistent stuck-advice variant. **Recommend 1.6 s after fully shown**, preserving the 0.8 s pre-show delay. Until approved, fake data must expose both schedules; do not silently change the current timer.

## 3. Layout

Reuse [patterns](../patterns.md) `tooltip/bubble`; its icon row is advice, not interactive `tool-disc` copies. HUD tools underneath retain their own patterns. Body is cream, with rim/shadow separating it from the background; do not dim/block the puzzle. Layers: world/birds → advice/tail → real HUD → every modal dim/panel. The hint must not overlap any real tool, coin pill, move pill or bird it asks the player to act on.

| Component | Pattern / asset | Size source | Anchor and position |
|---|---|---|---|
| Cream body | tooltip/bubble; `stat_pill` | `layout_metrics.json` native 400 × 96; inner `(23,17,354,60)` | above current Undo bounds, within safe area with 24 px clearance; exact offset/tail decision below |
| Undo advice glyph | tooltip/bubble; `icon_undo` | visible extent 56 px, art-direction minimum | row start; informational only, no miniature button |
| Firefly / Sapling / contextual tool | tooltip/bubble; supplied `tool_hint`, `tool_branch`, contextual art | each visible extent 56 px; opaque-bound normalization from `glyph_metrics.json` | 24 px gaps, centered row; omit ineligible icons rather than depict usable locked tools |
| Tail | tooltip/bubble; measured asset required | not present in metrics | points to actual Undo after HUD reflow and safe clamping |
| Actual tools and badges | host `wood-disc` / `tool-disc` | 108 px discs, 64 px inventory badges | existing HUD anchors; no hint-driven resizing/pulsing |

Maximum four-icon advice row needs `4 × 56 + 3 × 24 = 296` px, inside 354 × 60 measured content. Recenter the row when contextual/ineligible icons are absent. Gift coins, counters and labels are not added to this tooltip.

**[Decision needed] Shared tail/anchor geometry.** No measured tail, gap or screen-edge rule exists in the bubble pattern. Options: complete one measured shared bubble/tail for sleep and stuck hints, or measure and approve distinct tails. **Recommend one shared measured tail**, positioned above the real HUD anchor; final tail bounds and gap must be recorded by the pattern owner. Current 420 × 170 wood panel, 42 × 24 mesh tail and 30 px gap are implementation values, not approved measurements.

## 4. States

Fake board/controller source must expose:

- Hidden fresh/first-visit state; typical legal board; no-legal-transfer after committed action with undo; awaiting settle; 0.8 s pending; shown; each lifetime option's expiry; next tap dismissal; next qualifying changed state re-arms.
- Empty/won board and no undo (hidden); maximum authored density; 3-tool/4-tool HUD; contextual tool absent and each of Owl Eye, Morning Bell, Scarecrow, Spring Rain; undo-only advice; locked/unavailable tool omitted; zero stock routes through actual tool/shop, positive stock applies normally; Sapling already used/no-effect unavailable.
- Repeated observer refresh without state change, inventory change, undo/restart, focus loss/resume, new session, phase/homecoming/flight, win, map and modal interrupt; no stale timer recreates advice.
- Typical/0/99999 host coins, maximum tool stock/count labels, long host labels and safe-area extremes; icon-only advice remains unchanged. Missing model/Undo anchor/art hides advice rather than pointing at an unrelated button.
- Actual HUD actions while advice is visible: Undo, all applicable boosters, hint success/no solution/pending result, zero-stock Shop entry, Shop close, and any Shop purchase-confirmation state reachable there; also Settings interruption and its close. These are host/Shop surfaces, not popup controls owned by the hint. Local observation works offline; there is no hint loading/error popup, reward, new-friend or lose state.

## 5. Interaction map

| Input/action | Event / result | Feedback and sound |
|---|---|---|
| No legal move after settle/delay | controller calls `Show` | advice appears together; silent (no illegal-move accusation) |
| Pointer-down anywhere, including advice | `DismissRequested(nextInput)`; input continues to host | advice exit; no hint sound/haptic |
| Tap actual Undo | host `UndoRequested`; controller restores state and hides advice | `button-press / release`, `ui-tap`, then `undo` when effect applies |
| Tap actual available stocked tool | host `BoosterRequested(kind)`; controller decides effect | same button feedback + `ui-tap`; `booster-<kind>` only on effect |
| Tap actual zero-stock tool | same host request; controller opens Shop focused on tool | `button-press / release`, `ui-tap`; Shop uses `panel-open`; no effect sound or forced spend |
| Unavailable tool | no action; inert; locked tools absent | disabled alpha 0.45, unchanged size; silent |
| Back / modal / state change | controller hides; host handles its own action | `panel-close` exit; no extra hint sound |

No hint focus stops/keyboard/controller actions; Android back is forwarded to host. Displayed advice icons never raise booster requests. Haptics: none. Underlying Settings/Shop/confirm controls retain their own pattern feedback; this hint never implements them.

## 6. View contract

Proposed calls: `Show(StuckHintModel)`, `SetAnchor(undoBounds,safeBounds)`, `Hide(reason)`. Model carries session/state revision, ready/show eligibility, real Undo bounds, ordered eligible advice icon keys, lifetime policy and reduced-motion flag. Controller sources eligibility from committed `GameSession`/Core legal-move query, undo history, inventory/unlock/effect availability and board busy/modal flags; re-evaluates after committed change and motion completion, not every view frame. UI never calls solver, reads inventory or decides whether a level is stuck.

View raises `DismissRequested(reason)` for input/expiry; it owns only visual timer/anchor. Host raises Undo/booster/settings events. Missing eligibility/model/anchor hides hint, missing optional tool art omits that icon, missing mandatory Undo/body art suppresses the hint. Changing revision cancels pending animation/timer; hide is idempotent. Presentation must not save, award coins, consume stock or alter moves.

## 7. Motion and sound

| Trigger | Motion / duration | Audio/music |
|---|---|---|
| Ready eligible state | controller waits existing 0.8 s after settle; view uses `panel-open` (0.18 s), all icons together | silent; continuous `bgm-game` |
| Dismiss / interruption | `panel-close` (0.12 s); cancel callbacks | silent; no music restart |
| Underlying tool input | `button-press / release`; no independent Undo scale pulse | `ui-tap`; `undo` / `booster-<kind>` at committed effect only |
| Shop/Settings interrupt | hide advice before modal presentation; modal follows its own patterns | preserve level music under popup; host owns UI sound |

Pattern easing/reduced-motion fallbacks apply; `GameMotion.Instant` removes presentation motion, not the eligibility delay or approved lifetime. No perpetual bob or resize animation. Single mute control and focus pause apply to music and all host feedback.

## 8. Accessibility

**NOT ASSESSED:** no project contrast/screen-reader/device target. Assume host hit targets ≥88 × 88 design px; HUD discs are 108 px. Advice is informational and pass-through. Accessible nonvisible description identifies available recovery actions; disabled/locked tools use badge/lock and inert state, not colour alone. No urgent red warning, fail wording, countdown label or motion-only instruction.

## 9. Text

Tooltip visible text limit: zero characters; icon-only. No “stuck” paragraph, live label, stock number or coin number inside advice. Host tool stock is live digits inside its 64 px badge; plus-at-zero is fixed icon art. Host coin capacity is five live digits through 99999 in its own measured pill. Hint must neither duplicate nor relayout those host text elements. Localized accessible descriptions remain nonvisible metadata.

## 10. Acceptance criteria

1. At 1080 × 1920 and 1080 × 2400 with insets, advice/tail stays inside the safe area, points to the real Undo control and never covers HUD/birds; no host text or hint glyph escapes its container, including 99999 host coins.
2. All advice glyphs have uniform 56 px visible extents using opaque bounds; the four-icon row fits inside 354 × 60 content bounds.
3. Actual same-role tools remain 108 × 108 design px in normal, pressed-at-rest, disabled and hint-visible states; unavailable is alpha 0.45 and non-interactive, never smaller. No hint-driven Undo growth.
4. Cream body, rim and shadow clearly separate advice from the forest. The board stays undimmed and interactive; all modal dim layers/panels render above advice.
5. With ready qualifying data, show begins only after 0.8 s settled delay, completes its entrance in 0.18 s and exits in 0.12 s; the approved lifetime is exercised with timestamped evidence.
6. Fresh/empty/won/busy/modal/no-undo fixtures never show advice. Repeated unchanged refreshes do not re-arm it; changed sessions cannot fire old timers.
7. A tap dismisses advice and still activates a legal underlying action once. Undo restores the prior state; zero-stock tool opens Shop without spending; advice art itself has no buttons.
8. Eligibility/refusal/success remain controller-owned; showing/dismissing advice changes no moves, stock, coins or save state. Level music continues and mute silences UI/effects/music.

## Differences from the current build

- Use measured cream bubble art rather than a 420 × 170 wood panel; standardize tail/anchor after shared measurement, retaining automatic no-legal-move detection in the controller.
- Remove current Undo scale pulse (up to 1.10), tooltip bob and custom 0.2 s open; use named pattern transitions without changing tool size.
- Current observer requires changed state plus undo, waits 0.8 s and persists until dismissal; keep that eligibility/delay and resolve the lifetime conflict explicitly.
- Current bubble always depicts Firefly/Sapling and gates only context by unlock; supplied advice eligibility must avoid implying unavailable tools work. Move session/legal-move reads out of the view; keep input pass-through and no lose state.
