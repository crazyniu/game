# Level card

**Status:** draft; pending independent review and user approval. Stage 2, documentation only. Portrait phones: touch; Editor: mouse; platform Back closes modal. Reference 1080 × 1920; aspect ratios 9:16–9:20, check 1080 × 1920 and 1080 × 2400 with device safe areas. Keyboard/controller focus navigation is not specified.

Authority: [art direction](../art-direction.md), named [patterns](../patterns.md), [redesign brief and addenda](../../work-log/ui-redesign/brief.md), [conventions](../../CONVENTIONS.md). [ui_level_card.png](../../../art/approved_ui/ui_level_card.png) supplies plush forest style; purple controls, round Play and missing gifts are superseded. No rules, economy, saves or level-data changes authorized.

## 1. Player need

The player arrives wanting to choose a level, see move targets and their record, and plan which species' gift to earn first. They were browsing the map or returning for a replay.

## 2. Navigation

- Selectable current/replay map nodes open this modal over the unchanged map. Controller supplies selected level and optional node origin. Locked nodes refuse entry; no locked-card preview is approved.
- Play requests selected level. Controller closes card, starts level entrance (3.5–4.5 s, tap skips), then presents due `new-friend` and `unlock-card` notifications. Card never acknowledges them.
- X, outside tap and platform Back dismiss to identical map scroll/selection. Inside blank panel does not dismiss. No auto-dismiss/timer.
- Portraits, gifts and mechanics are display-only: no hidden goal-info link. No direct shop/settings/purchase/world-unlock popup. A map-owned `daily-gift` can follow dismissal when scheduled.
- Hide/destroy cancels all motion/callbacks. Controller chooses reopen/closed state after interruption; view cannot load levels autonomously. Offline never blocks local preview.

## 3. Layout

Top-left design px. Center modal group inside device safe area; dim covers entire display. Use `modal-panel` measured cream inset (24 px) and `primary-bar` side/bottom clearance (at least 28 px), not local variants. Leaf can overlap frame; number stays inside leaf. Close never covered.

Sources: [layout metrics](../../../PerchSort/Assets/Resources/Art/layout_metrics.json) from `tools/layout_metrics.py`; [glyph metrics](../../../PerchSort/Assets/Resources/Art/glyph_metrics.json); [reference measurements](../../../art/approved_ui/layout.json), `screens.level_card` (visible bounds ±8 px, not hit targets). Every size below is a pattern value or measurement; reference anchors are starting evidence, not final redesigned placement.

| Component | Pattern / asset | Size and anchor basis | Layer |
|---|---|---|---|
| Map and balance | Underlying map; `coin-pill` / existing art | Map unchanged; pill 190 × 76 at top-left (48,150); no duplicate inside card | Below dim |
| Dim/input shield | `modal-panel` / black | Full screen, alpha 0.72, above map birds/nodes/bars | Modal base |
| Shadow and panel | `modal-panel` / `panel_wood` | Reference (230,560,612,698). Source 800 × 540, cream inner (88,119,645,317); content-fit pending. Shadow +14 px, alpha 0.35 | Above dim |
| Level leaf | `level-badge` / numbered baked `leaf_badge` | Reference visible (424,482,214,146); source 256 × 180, inner (44,49,168,82); preserve aspect, fit not stretch | Crown |
| Close | `close-x` / `btn_wood`/pressed + `icon_close` | 100 × 100, panel top-right, above badge/decoration | Highest interactive |
| Best stars and targets | `star-row` / `star_full`, `star_empty` | Three 120 px stars, reference center y=672; each target immediately under matching star | Content |
| Best moves versus target | `stat-pill` / `stat_pill`, footprints, flag | 440 × 72; source 400 × 96, inner (23,17,354,60); reference center (536,772) | Content |
| Species portraits | New level-species cluster entry needed / `aviary_frame`, front-facing drawn frames | Reference art bounds 136 × 160, centers x=346/478/612/742, y=902; final ring/face/multirow pitch pending | Content |
| Gift per portrait | `gift-coin` / `ViewModel.GiftIcon` mapping, gold frame | 84 px, portrait bottom-right; never hide face; no effect/name paragraph | Above portrait |
| Conditional mechanics | New mechanic-row entry needed / egg, sleep, crow, swivel, growth, moon art | Reference sleep 142 × 126 and egg 150 × 118 are provenance, not uniform glyph sizes. Only present mechanics; no empty row | Below portraits |
| Play | `primary-bar` / `btn_green_pill` + `icon_play` | 520 × 120, glyph 64 px; bottom content zone, wholly inside cream | Highest interactive |

**[Decision needed] Content-fit geometry/type:** added thresholds and 84 px gifts invalidate the old four-portrait row. Four measured portrait widths alone total 544 px, before gaps/gifts; actual sliced cream area needs measurement. Options: taller/wider canonical panel with multirow species clusters (recommended), or an approved scrollable species zone with fixed Play. Pattern owner must supply measured ring/face/cluster sizes, row gaps, mechanic glyph target, numeric boxes/fixed font sizes, and final panel/row anchors for 2 and 7 species. Asset-space inner rectangles must map through actual nine-slicing; never omit species or shrink text/art ad hoc.

**[Decision needed] Shared controls:** see [Win panel — Layout](win-panel.md#3-layout). Bar 520 × 120 and panel disc 100 px meet the 1.25× sibling cap by height (1.20), not width/area: explicitly approve height comparison (recommended), or revise both families. Disc glyph 46%/about 55% conflicts with 56 px minimum: recommend shared 56 px opaque major axis, primary 64 px; alternative is revising the minimum. `icon_close` measures 92 × 91 and `icon_play` 80 × 92 on 96 px canvases. Normalize opaque bounds, not canvas; preserve aspect/optical centering. Recommendations are not approval.

## 4. States

Fake source must reach every row without real-profile writes.

| Fixture | Required result |
|---|---|
| Empty/missing level | No fabricated preview; controller leaves card closed. Missing record on valid level means unplayed, not missing level |
| First visit / unplayed unlocked | Baked level, three dim star slots and thresholds, no best record, all species/gifts, enabled Play; no invented reward |
| Typical replay | Best 1/2/3 stars; best counted moves versus target; reopen preserves map position/selected level |
| Species counts | Minimum 2, maximum 7, every intermediate 3–6; short final row centered; gift visible on every portrait |
| Mechanics | None (row absent); egg/sleep/crow/swivel/growing/night each alone; all six as stress fixture, even if not co-authored. Crow never duplicated as homecoming species |
| Numbers/names | Levels 1/40; best missing/0/normal/99999/over-limit; min/max catalog targets; map balance 0/99999; longest and overlong accessibility names, none rendered as visible labels |
| Locked | Locked node refuses opening (`refuse` on map). Also revoke availability while open: Play dim/inert, Close active |
| Disabled/busy | Play unavailable/not ready; accepted Play/dismiss prevents duplicate requests; normal/pressed/disabled dimensions consistent |
| Every resulting popup | Play → new-friend → unlock-card when due; dismissal → map-owned daily-gift when due. No card-owned nested modal |
| Lifecycle | Node-origin/centered open, idle, Play, X/outside/Back, drag-out, mixed repeated taps, hide/reopen, interruption/resume, mute/unmute, instant/reduced motion |
| Offline/error | Valid local preview usable offline. Missing catalog/assets are defects, not fabricated levels, network spinner or retry/purchase popup |

Selection, transfer, refusal, homecoming, win and stuck moments belong to the board after Play, not this card. No lose state, timeout or auto-dismiss.

## 5. Interaction map

Touch/Editor mouse use identical pointer semantics; no haptics specified. Back follows Close. No keyboard/controller traversal added.

| Input | Event → result | Feedback / pattern sound |
|---|---|---|
| Available Play | `PlayRequested(levelId)`; accepted once, selected level enters | `button-press / release`, `ui-tap`; `panel-close`, then `level-entrance` |
| X | `DismissRequested`; unchanged map | `button-press / release`, `ui-tap`, `panel-close` |
| Outside / Back | Same dismissal event | `panel-close`; pointer `ui-tap`, hardware Back no pointer sound |
| Drag/release outside button | No action | `button-press / release`; `ui-tap` on down only |
| Disabled Play | No event | Alpha 0.45, unchanged resting size, no press or sound |
| Blank cream / portrait / gift / mechanic | No event or hidden popup | Display-only, no press or sound |
| Locked node (entry boundary) | Map declines; no card event | Map `refuse`; card stays closed |
| Interruption/hide | No implicit Play/dismiss/reward | Cancel callbacks; audio owner pauses/resumes music |

## 6. View contract

Proposed API, not existing members. Controller provides a snapshot; fake and real sources share it. View owns rendering, feedback and cancelable animation, never saves, scoring, rules, level loading or availability policy.

| Shown data | Source / update | Missing |
|---|---|---|
| Level ID and baked badge | Catalog presenter; accepted selection/`Show` | Invalid model: closed. Missing badge is asset defect, not live number fallback |
| Best stars, nullable best counted moves | Profile; show/reopen or record update | Missing record: unearned slots and baked unavailable-record glyph, not live `-` |
| Three threshold presentations, par | Catalog/rules presenter; show | Supplied baked unavailable glyph; never calculate `par + 3` in view |
| Ordered homecoming species, front frames, gifts, accessible names | Level/catalog presenter and canonical gift mapping; show | Empty list invalidates preview; missing art is defect, never remove species |
| Ordered present mechanics | Level presenter; show | Empty means no row; absent data is not silently mechanic-free |
| Play availability, selected ID, optional origin | Controller; show/change | Unknown availability disables Play; no origin means centered opening |
| Underlying balance/mute | Economy/audio owner; change | No duplicated/invented balance; no mute control here |

Calls in: `Show(LevelCardPresentation)`, `SetRecord(bestStars, bestMoves)`, `SetPlayAvailable(bool)`, `Hide()`. Events out: `PlayRequested(levelId)`, `DismissRequested` (X/outside/Back). Controller accepts requests and owns the board/popup flow. Card never constructs a rules board to discover species/mechanics or reads saved records itself.

## 7. Motion and sound

Reference shared rows, not per-screen substitutes.

| Trigger | Motion authority | Sound / music |
|---|---|---|
| Open | `panel-open`, 0.18 s ease-out; controls present first frame; optional supplied node origin | No defined popup-open SFX; retain `bgm-menu`, never restart |
| Dismiss / accepted Play | `panel-close`, 0.12 s ease-in; cancel callbacks on hide | Pointer `ui-tap`, no invented close sound |
| Buttons | `button-press / release`, shared duration/easing | `ui-tap` on pointer down, silent when muted |
| Portraits | Breathing 2.4–3.2 s; drawn blink/hop/pose actions every 1–3 s; front-facing rest | No added chirp loop; menu music continues |
| Level entry | Controller's `level-entrance`, 3.5–4.5 s, tap skips | Page change crossfades to `bgm-game` over 1 s; subsequent popups do not restart it |

Pattern reduced motion makes panel transitions instant; `GameMotion.Instant` stops all motion. **[Decision needed] Portrait/transition coverage:** shared library lacks portrait-idle reduced behavior and node-origin travel-distance rule. Options: extend shared patterns from existing drawn clips/controller origin, with reduced mode holding front pose (recommended), or remove origin travel and retain frame-only reduced idle. Record centrally. Normal-mode actions never use mirrored stills or procedural shake.

## 8. Accessibility

**NOT ASSESSED:** no approved contrast/assistive-technology target. Assumed minimum touch target 88 design px; Close 100 px/Play height 120 px exceed it. Accessible names identify Close/Play level and announce record, targets and gifts without visible labels. Stars use filled/empty shape, locking a map padlock, not color alone. Disabled remains inert, never smaller. Input stays modal; rest portraits face the player, actions return there. Reduced behavior follows section 7.

## 9. Text

Fredoka Medium; constant size per numeric role, autosize off. Fixed text/signs baked; live text only changing numbers. No visible live name, effect, Play/Close label, mechanic label or title.

| Element | Limit / missing / overflow |
|---|---|
| Level leaf | Baked 1–40, at most 2 digits; number within measured inner (44,49,168,82) mapped with art, never live overlay |
| Best moves | Stress 5 digits (99999); missing baked no-record glyph; larger values pending below |
| Par / three-star limit | Current catalog 2–19, maximum 2 digits; presenter supplies |
| Two-star limit | Current maximum 22, maximum 2 digits; presenter supplies |
| One-star limit | Any clear, no finite move ceiling; representation pending |
| Underlying coin balance | 5 digits including 99999 in measured coin-pill number area; no card duplicate |
| Names/fixed prose | None visible; long-name fixture accessibility-only. Later localized text requires measured baked assets, never live fallback |

**[Decision needed] Unlimited/missing/overflow displays:** number-under-every-star request conflicts with unlimited one-star clears. Options: baked infinity/unlimited glyph (recommended), or another truthful non-cap convention. Also approve baked no-record glyph (dash recommended, preserving meaning), and shared >5-digit formatting: presenter-supplied compact notation (recommended), or wider measured boxes. Never clamp saves, ellipsize digits or auto-shrink. Final font sizes/text boxes await content-fit measurements; shared with [Win panel — Text](win-panel.md#9-text).

## 10. Acceptance criteria

Future shell checks, not results from this docs-only task:

1. At 1080 × 1920 and 1080 × 2400 with safe-area insets, badge/X/Play remain wholly visible. Play has at least 28 px cream clearance at sides/bottom; other content 24 px inset.
2. Two-/seven-species fixtures retain every portrait and its bottom-right 84 px gift; no face/gift/threshold/mechanic/control is clipped or obscured. Zero mechanics leaves no empty row.
3. At 99999 best moves/coins, max targets and longest names, no text escapes its container; numeric font size is constant and fixed labels are never live text.
4. Close stays 100 × 100 and Play 520 × 120 at rest in every state unless shared-ratio decision revises patterns. Same-role press feedback is identical; disabled retains size, alpha 0.45, and emits no Play.
5. Close opaque glyph matches approved shared panel-disc target; Play glyph is 64 px. Equal transparent canvas size alone fails; resolve shared glyph conflict first.
6. Black alpha-0.72 dim covers every map object/bar; +14 px, alpha-0.35 shadow visibly lifts panel from forest. No underlying input passes through.
7. Open/close settles in 0.18/0.12 s; controls visible first frame. Device latency/frame-rate performance remains NOT ASSESSED without hardware/budget; timing alone is not performance proof.
8. X/outside/Back return identical map scroll/selection; blank cream/portrait/gift/mechanic taps do nothing. Locked entry never opens; availability revoked while open blocks Play, not Close.
9. Repeated/mixed Play/Close accepts at most one route. Play reaches 3.5–4.5 s entrance and due new-friend/unlock-card sequence; no callback changes coins or acknowledges awards.
10. Timed frames show breathing and drawn actions every 1–3 s in normal motion, no mirrors/frozen portraits. Mute silences sounds/music; card never restarts menu music.
11. Geometry/type, shared controls, portrait/transition coverage and unlimited/missing/overflow displays are resolved before approval. Affected checks are blocked until then, not passed.

## Differences from the current build

Source inspection: `PanelsView.cs`, `PopupMotion.cs`, `UiBirdLife.cs`, `PopupFlow.cs`, `ViewModel.cs`, `GameRoot.cs`; references inspected, no runtime capture.

- Add each species' gift coin and each star's threshold; current card has neither. Keep mechanics conditional and portraits front-facing.
- Replace live leaf number with baked level-badge and live missing-record `-` with baked glyph. Current best/par icons 40 px and primary glyph 56 px need shared icon targets.
- Current variable-height card is 620 px wide, with 118 px rings, four per row at 132 px pitch. Remeasure for gifts/thresholds, never guessed offsets.
- Current `LevelCard(int)` loads a level, constructs a rules board and queries profile itself; target receives presentation data and raises events, preserving gameplay/save policy behind controller.
- Current open/information close already 0.18/0.12 s; Play uses generic close 0.15 s. Align all exits. Existing dim/shadow/drawn portrait life still require future timed shell acceptance.
- Current portrait scheduler uses blink intervals 2–4 s, peek 4–8 s and turn 6–10 s; the requested 1–3 s drawn-action cadence needs shell verification and implementation where unmet.
