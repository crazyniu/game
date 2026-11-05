# Win panel

**Status:** draft; pending independent review and user approval. Stage 2, documentation only. Portrait phones: touch; Editor: mouse; platform Back pending below. Reference 1080 × 1920; aspect ratios 9:16–9:20, check 1080 × 1920 and 1080 × 2400 with device safe areas. Keyboard/controller focus navigation is not specified.

Authority: [art direction](../art-direction.md), named [patterns](../patterns.md), [redesign brief and addenda](../../work-log/ui-redesign/brief.md), [conventions](../../CONVENTIONS.md). `ui_win.png` supplies plush forest style; its purple discs and round primary are superseded. This spec does not authorize rules, economy or save changes.

## 1. Player need

The player arrives wanting to see their counted moves, earned stars and coins, then continue, replay or go home. Their final flock has just gone home; logic has already committed the result and reward.

## 2. Navigation

- Open over the completed level after its final homecoming settles. Never open for an unfinished/stuck board; there is no lose panel.
- Next requests the next level, Replay the same level, Home the map. Controller chooses destinations and accepts only one exit.
- First clears of 10/20/30: Next/Home can pass through `world-complete`, then `world-unlock`; 40 has world-complete only, never a fifth world. Replay preserves pending awards. Map arrival can show `daily-gift`; the next level can show `new-friend` and `unlock-card` after entrance. These are separate controller-owned views. No shop/settings/goal-info action here.
- No auto-dismiss. Hide/destroy cancels every transient/callback. Focus loss pauses audio; resume displays the controller's committed result without re-granting coins.
- **[Decision needed] Back and final-level action:** shared Back closes the top modal, but current win has no close control. Options: Back follows Home (recommended; avoids stranding the player on a completed board), or add `close-x` and specify completed-board return. Separately choose level-40 Home glyph instead of Next arrow (recommended, matching its current destination), or retain arrow. Dim-tap behavior must follow the chosen policy or remain inert; never pass through to the board.

## 3. Layout

Top-left design px; modal group inside safe area, dim over the entire display. Reference `modal-panel` insets/shadow and `primary-bar` cream clearance; no local variants. Ribbon and birds may overlap frame, never values or controls.

Sources: [layout metrics](../../../PerchSort/Assets/Resources/Art/layout_metrics.json) from `tools/layout_metrics.py`; [glyph metrics](../../../PerchSort/Assets/Resources/Art/glyph_metrics.json); [reference measurements](../../../art/approved_ui/layout.json), `screens.win` (visible bounds ±8 px, not hit targets). Every size below is a pattern value or measurement; reference rectangles are starting evidence, not final redesigned placement.

| Component | Pattern / asset | Size and anchor basis | Layer, back to front |
|---|---|---|---|
| Completed board, balance | Underlying board; `coin-pill` / existing art | Board unchanged; pill 190 × 76 at top-left (48,150) | Scene |
| Dim/input shield | `modal-panel` / black | Full screen, alpha 0.72, above all world/bird/HUD objects | Modal base |
| Rays, shadow, panel | `modal-panel`; reward-decoration entry needed / `light_rays`, `panel_wood` | Reference panel (178,742,716,772); shadow +14 px, alpha 0.35; final panel content-fit pending | Rays behind panel, above dim |
| Ribbon, hero, side birds | `name-ribbon` shape; reward-decoration entry needed / `ribbon_green`, sunny/sky/berry frames | Ribbon source 640 × 200, inner (34,23,569,153). Reference hero (400,506,240,216), left (86,938,248,308), right (754,892,264,314); preserve aspect | Crown/frame, below controls |
| Three stars and limits | `star-row` / `star_full`, `star_empty` | Stars 140 px; each limit directly below its corresponding star | Content |
| Counted moves, credited reward, optional chain | `stat-pill` / `stat_pill`, footprints, coin, chain | Pill 440 × 72; source 400 × 96, inner (23,17,354,60). Between stars and actions; absent chain removes row | Content |
| Improvement badge, near miss | New reward-detail entry needed / coin, footprint, numeric data | Top-up on upgraded star; near-miss by dim third star; bounds pending | Content, not over limits |
| Next; Replay/Home | `primary-bar`; `wood-disc` / `btn_green_pill`, `btn_wood`/pressed, `icon_next_arrow`, `icon_replay`, `icon_home` | Next 520 × 120, glyph 64 px, its own row; secondary pair 100 × 100 below. All inside cream | Highest interactive |

**[Decision needed] Content-fit geometry/type:** added moves, thresholds and top-ups have no measured layout. Panel source 800 × 540 has cream inner (88,119,645,317), not a runtime rect after nine-slicing. Options: expand the canonical sliced panel and measure its composed inner area (recommended), or approve a rearranged measured mockup. Supply final panel/row anchors, reward-detail pattern, numeric boxes and fixed font sizes before approval. Never omit rows or shrink text to fit.

**[Decision needed] Shared control conflicts (both screens):** 520 × 120 bar versus 100 px discs meets the 1.25× cap by height (1.20), not width/area. Options: explicitly approve height comparison (recommended), or revise both pattern families. Disc glyph guidance (46% / about 55%) also falls below the brief's 56 px minimum. Recommend a shared 56 px opaque major axis for panel-disc glyphs, retaining primary 64 px; alternative: revise the minimum. Neither recommendation is approved here. Normalize opaque bounds, not canvas: replay/home major axis 92 on 96 px canvases; next-arrow 90. Preserve aspect/optical centering.

## 4. States

Fake source must reach every row without real-profile writes.

| Fixture | Required result |
|---|---|
| Empty/missing result; zero stars | Controller leaves win closed, never fabricates a clear. Separately, valid zero reward shows `+0`, no coin flight |
| Typical first clear; later visit | Moves, three thresholds, stars and supplied reward; same layout on later visits |
| 1/2/3 stars; boundaries | Three slots always present. Exercise at three-star limit, one above, at two-star limit, one above; view never scores |
| Two-star near miss | Exactly one above three-star target: footprint `+1`, third star dim; means one fewer move was needed |
| Replays | Same/lower stars; one-/two-star improvement; equal stars with better moves. Top-up only when supplied |
| Optional rewards | Chain absent/zero/positive; improvement absent/present; both together; total versus components unambiguous |
| Numbers and names | Balance 0/99999; moves, thresholds and rewards normal/maximum, 99999 and over-limit stress; longest accessibility name, no visible species label |
| Locked/disabled | Next unavailable; all actions inert during accepted exit; same resting dimensions, alpha 0.45 |
| World boundaries | First/repeat clears 10/20/30/40; Next and Home; Replay with pending award |
| Every outgoing popup | World-complete, world-unlock, daily-gift; next-level new-friend and unlock-card; acknowledge once in their own views |
| Lifecycle | Enter, stars, count-up, skip/close during each, idle after confetti, hide/reopen, focus loss/resume, muted/unmuted, instant/reduced motion |
| Offline/error | Local result usable offline; missing model never fabricates data; unavailable action disabled. No network/loading/retry popup introduced |

Board selection, transfer, refusal, homecoming and stuck actions cannot be activated through this modal. No timeout/auto-dismiss.

## 5. Interaction map

Touch/Editor mouse share down/inside-release semantics; no haptics specified. Hardware Back follows the pending policy; keyboard/controller traversal is not introduced.

| Input | Event → result | Feedback / pattern sound |
|---|---|---|
| Next / Replay / Home | `NextRequested` / `ReplayRequested` / `HomeRequested`; controller accepts once | `button-press / release`, `ui-tap`; accepted exit `panel-close` |
| Drag/release outside button | No action | `button-press / release`; `ui-tap` on down only |
| Disabled action | No event | Same-sized dim control; no press or sound |
| Noninteractive panel tap during reveal | `RevealSkipRequested`; settle supplied final values, no grant | `star-pop` tap-skip; no new sound or duplicate `coin-gain` |
| Dim / Back | Policy above; never activate board | If dismissed: `panel-close`, pointer `ui-tap`; hardware Back has no pointer sound |
| Interruption | No reward/navigation event | Cancel presentation, music pauses/resumes through audio owner |

## 6. View contract

Proposed API, not existing members. Snapshot data comes from controller/presenter; UI owns no game state and never loads levels, scores, compares saves, computes rewards or claims notifications.

| Shown data | Source / update | Missing |
|---|---|---|
| Level, earned stars, moves, three threshold presentations, near-miss delta | Rules/catalog presenter; `Show` or authoritative replacement | Missing result: closed; absent threshold: supplied baked unavailable glyph, no guessed target |
| Previous/best record, upgraded-star indices | Profile before/after commit; `Show` | Hide absent improvement; do not infer from already-updated save |
| Total credited coins, chain component, per-star top-up | Committed economy receipt; `Show` | Zero stays zero; absent optional row hidden; no invented breakdown |
| Balance before/after | Economy/controller; balance change | Never invent/grant balance; flight callback is synchronization only |
| Allowed actions/destination/notification route | Controller; show/change | Unknown action disabled; no inferred level 41 |
| Bird art and accessible names | Asset/catalog identifiers; show | Missing frame is an asset defect, not permission for mirrored still motion |

Calls in: `Show(WinPresentation)`, `SetActions(ActionAvailability)`, `SetBalance(before, after)`, `SettleReveal()`, `Hide()`. Events out: the four action/skip events above, `BackRequested` (pending policy), optional `CoinFlightStarted` for already-committed HUD synchronization. Re-show/skip/hide/resume never grant coins. Other popups own their acknowledgements.

## 7. Motion and sound

Use shared table rows by name; do not create alternate button/modal behavior.

| Trigger | Motion authority | Sound / music |
|---|---|---|
| Open / close | `panel-open` 0.18 s / `panel-close` 0.12 s; actions visible from first frame | `win-jingle` once on initial result; no invented close SFX; retain `bgm-game`, no restart |
| Earned stars | `star-pop`: 0.35 s, 0.18 s apart, tap skips; dim slots remain | `star-pop` per earned star, no queued chime burst after skip |
| Reward / balance | `counter`, 0.5 s; committed endpoints only | `coin-gain` once at counter start, not per digit |
| Buttons | `button-press / release` | `ui-tap`; mute silences UI/SFX |
| Birds | `bird-flight`, drawn frames and landing squash; breathing 2.4–3.2 s, drawn idle actions every 1–3 s | No added chirp loop; underlying music continues |
| Destination | Controller's transition and `level-entrance` 3.5–4.5 s, tap skips | Actual page change only: `bgm-menu` / `bgm-game`, 1 s crossfade |

**[Decision needed] Win motion exception:** conventions require overshoot, ribbon drop, rotating rays and about 3 s confetti; generic art direction says one 0.18 s entrance, no per-control entrances. Options: win-only celebration exception with actions visible immediately (recommended), or generic-modal-only entrance. Pattern owner must record reward-decoration timings/reduced behavior, not accidentally inherit current delayed buttons. Shared reduced mode makes panel/counters/stars instant; `GameMotion.Instant` stops all motion. Normal birds never mirror or fake an action with a still. Hide cancels everything.

## 8. Accessibility

**NOT ASSESSED:** no formal contrast, assistive-technology or device budget. Assumed minimum target 88 design px; 100 px discs/120 px bar height exceed it. Stars use filled/empty shapes, not color alone; unavailable buttons dim and inert, not smaller. Accessible names identify actions and announce stars/moves/reward, without visible labels. Input is modal; outlines and panel remain distinct from forest. Reduced behavior follows section 7; decoration-specific behavior awaits its decision.

## 9. Text

Fredoka Medium; constant size per numeric role, autosize off. Fixed text/signs baked; live text only changing numbers. No live victory title, species names or explanatory sentence.

| Element | Limit / rule |
|---|---|
| Balance | 5 digits, including 99999, inside measured coin-pill number area with margin; larger values pending below |
| Total, chain, per-star improvement | Stress 5 digits; plus sign baked, at most 6 visible characters including sign; absent components hidden |
| Counted moves / best record | Exercise 0, normal, 99999 and larger; no gameplay cap established; overflow pending |
| Star limits | Current catalog par 2–19; two-star maximum 22; 2 digits for finite targets. One-star has no finite ceiling |
| Near miss | Baked footprint/plus, live single digit `1`; no sentence |
| Ribbon / names | Decorative ribbon blank; zero visible name text. Long names accessibility-only |

**[Decision needed] Unlimited/overflow displays:** request for a number under every star conflicts with unlimited one-star clears. Options: baked infinity/unlimited glyph (recommended), or approve a truthful alternative without a scoring cap. Also approve shared overflow presentation: presenter-supplied compact notation/glyph (recommended) or wider measured boxes; never clamp saves, ellipsize digits or auto-shrink. Final numeric boxes/font sizes belong to the layout decision.

**[Decision needed] Reward receipt:** current `GameProfile.Reward` pays flat replay coins plus Chorus, with no per-star top-up; `Meta.Economy.OnWin` includes improvement payouts, and design prose disagrees about replay pay. Options: coordinator authorizes one canonical receipt supporting the requested upgraded-star badge (recommended), or explicitly revises requested badge/credit behavior. View displays a receipt, never resolves economy policy. Keep total and chain component distinct; do not imply a second credit.

## 10. Acceptance criteria

Future shell checks, not results from this docs-only task:

1. At 1080 × 1920 and 1080 × 2400 with safe-area insets, all actions remain fully visible; birds, stars, numbers and confetti never obscure controls.
2. At 99999 balance and numeric/name extremes, no text escapes its measured container; numeric font size stays constant and fixed labels are not live text.
3. Replay/Home are each 100 × 100 at rest in every state; Next 520 × 120 unless shared-ratio decision revises patterns. Same-role press feedback is identical; disabled returns to full size, alpha 0.45, and emits no event.
4. Secondary glyph opaque bounds match the approved shared target, primary glyph is 64 px; transparent canvas equality alone fails. Shared glyph decision must be resolved first.
5. Panel visibly separates from forest with +14 px, alpha-0.35 shadow and full-screen black alpha-0.72 dim. Every board bird/HUD/house is below dim; underlying input is blocked.
6. Generic open/close settles in 0.18/0.12 s; any win exception is explicitly approved. Controls never wait 1.16 s to appear. Stars take 0.35 s at 0.18 s start intervals; counters 0.5 s.
7. Every award state retains three threshold presentations. Two-star near miss shows footprint `+1` and dim third star; improvement/chain match supplied receipt; zero reward has no coin flight.
8. Rapid mixed action taps, skip, hide and resume cause at most one accepted route and no extra grant. Finale flow shows world-complete then world-unlock when due; no world 5.
9. Timed frames show continuous breathing and drawn idle actions every 1–3 s after arrival, no mirrors/still stand-ins. Bird life continues after confetti until close. Mute silences all sound/music; popup never restarts music.
10. Back/final action, geometry/type, shared controls, motion exception, unlimited/overflow and receipt decisions must be resolved before approval. Device latency/frame-rate performance remains NOT ASSESSED without hardware/budget; animation timing alone is not a device benchmark.

## Differences from the current build

Source inspection: `WinPanel.cs`, `PopupMotion.cs`, `UiBirdLife.cs`, `PopupFlow.cs`, `ViewModel.cs`, `GameRoot.cs`, `ProgressStore.cs`; no runtime capture.

- Add moves, per-star limits, near miss and per-upgraded-star top-up; current `Show` has only stars/reward/Chorus and optional presentation callback.
- Current total includes Chorus while second row repeats its component; target must avoid implying double credit. Receipt policy remains open above.
- Current hard-coded panel (220,650,640,850) is not measured content-fit geometry; Replay/Home 90 px must become pattern 100 px. Primary helper glyph is 56 px, target 64 px.
- Current open 0.35 s from scale 0.6, buttons delayed 1.16 s, generic close 0.15 s, counters 0.6 s differ from shared timings. Current dim/shadow and drawn bird life are starting points, not fresh acceptance evidence.
