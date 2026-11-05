# Board

**Status: draft.** UI-first stage: screen specification, not implementation approval. Portrait iOS/Android phones; touch, with primary mouse click for Editor review. Reference 1080 × 1920; supported aspect range 9:16–9:20. Review endpoints: 1080 × 1920 and 1080 × 2400, including notched safe areas. These are design review targets, not tested devices.

Authority: [art direction](../art-direction.md), named entries in [patterns](../patterns.md), and the decisions/gameplay/tree addenda in the [redesign brief](../../work-log/ui-redesign/brief.md). Retain current rules, catalog, economy and saves; supersede historical multi-house, square-button and mirrored-bird instructions in [GAME_DESIGN](../../GAME_DESIGN.md). Approved references: `art/approved_ui/ui_level_k3.png` and `ui_level_k7.png` for plush forest style, not superseded house/button geometry. This document owns the whole level surface, including its HUD, bubbles, tutorial and entrance; destination popup internals belong to their screen owners.

## 1. Player need

The player arrives wanting to bring every flock home, choose a useful gift order and finish within the visible star targets. They have just pressed Play on a level card, Next after a win, or Restart in Settings. The board must make exposed birds, available seats, gifts and counted-versus-free moves legible without explanatory paragraphs.

## 2. Navigation

| Entry or exit | Required behavior |
|---|---|
| Level-card Play; win Next; replay/Settings Restart | Enter the supplied unlocked level through the entrance sequence. Replay does not create another screen or reset collection progress. |
| Settings disc; Android back on an idle board | Request Settings; clear selection only when logic supplies the update. Settings owns mute, restart, return to map and close. Back with Settings open closes it, not the board. |
| Species progress badge, pending or home | Tap opens Goal info for that species; no long-press requirement. Close/back returns to the same attempt. |
| Zero-stock tool plus | Open Shop → Boosters focused on that tool; purchase confirmation is a child of Shop. Close/back unwinds the top surface and ultimately restores this attempt with updated supplied balance/stock. The coin pill itself is display-only, as in the current board. |
| All flocks home | Finish supplied delivery/cascade presentation, then open Win. Win owns Replay, Next and Home. Final-level Next returns to the map through the existing finale flow. |
| First introduction / progression notifications | After entrance, serialize New friend and Booster unlock cards before tutorial/play. After a world-ending win, serialize World complete, then map/gate/World unlock; Daily gift is map-owned, never an unsolicited board overlay. |
| Focus loss, disable or interrupted transition | Stop input, cancel transient presentation and pause music. Restore the latest logic snapshot on return; resume music without duplicating rewards, tutorial completion or notifications. No UI-owned checkpoint. |

Only the top modal receives input; back/close are equivalent for dismissible information. Reward/unlock acknowledgements remain explicit and logic-owned. Back during flight/entrance must not escape or mutate the attempt; entrance taps only request Skip. Keyboard/controller navigation is not a supported play input; Editor mouse follows touch. Android back routing is an implementation difference, not a new quit dialog.

## 3. Layout

All coordinates are top-left design pixels inside the safe-area-fitted design root. Fit the entire 1080 × 1920 composition without stretching at taller ratios; background/dim cover the physical viewport. Keep interactive content inside the safe area and at least 24 px from unrelated containers (brief quality bar); do not crop the board to fill taller phones. Decorative trunks may meet the screen edges.

Sources: **P** = named pattern; **M** = committed `PerchSort/Assets/Resources/Art/layout_metrics.json` produced by `tools/layout_metrics.py`; **G** = sibling `glyph_metrics.json`, alpha threshold 40; **R** = observed bounds in `art/approved_ui/layout.json` (±8 px measurement uncertainty); **B** = explicit brief geometry. Pattern dimensions override obsolete R dimensions. Positions retained from R are not approval of the old purple art. Unmeasured geometry is explicitly unresolved, not an implementation estimate.

| Component | Pattern / source | Asset or requirement | Size and position | Back-to-front layer |
|---|---|---|---|---|
| World background and atmosphere | New board-scenery pattern needed; B | Existing world/variant backgrounds; continuous painted trees required | Full design canvas; extend background only beyond it | Background, then atmosphere |
| Edge trunks, crown and limbs | New board-scenery pattern needed; B | One painting per world/row arrangement; bark, vines and leaves continuous through every root, no junction patches | Visible trunk about 120 wide; full limb about 530 from its edge; feet lines below | Scenery, below birds |
| Birds, eggs, mama/chicks, crows; branch/knot hit areas | New board-piece pattern needed; R/B | Canonical species frame art and equipped overlays; front-facing at rest | First bird center ≥150 from its edge. R nominal visible bird: 122 × 152 sparse, 136 × 168 dense; these are measured reference envelopes, **not final dense packing sizes**; see Geometry decision | Birds over scenery; flights/effects over perched birds |
| Coin pill | `coin-pill`; P/M | `coin_pill`, coin overlapping left end | 190 × 76, left 48, top 150 | HUD |
| Move pill | `move-pill`; P | Cream pill, footprints, numeric count, three stars and limits; feather/green flash on an uncounted move | 190 × 64, left aligned with coin pill, 24 below it; stars 26 | HUD |
| Day/night dial | New dial pattern needed; B | `dial_sun` / `dial_moon` plus moves-until-flip number | Under move pill, left aligned, 24 gap; final envelope/inner rect unresolved | HUD |
| Single goal house and rope | `goal-house`; P | Current house/door/style art, one rope; empty dark doorway initially | 240 × 240 envelope, centered x=540, top 150; preserve art proportions within envelope. Rope ends at house suspension point, never crosses pills | Homeward flights behind doorway/body; resident within doorway |
| Species progress row | `progress-badge`; P | Front-facing face in gold ring, gift icon coin bottom-right, check when home | 44 diameter (38 only beyond seven); gift coin 22; centered below house with 24 gap. Badge pitch/touch regions unresolved | HUD above flights |
| Settings | `wood-disc`; P/R/G | `btn_wood`, pressed art, `icon_gear` | 108 × 108; measured sparse reference center (923,195), unchanged across board densities | HUD |
| Tool bar | `tool-disc`; P/R/G | Undo; `tool_hint`, `tool_branch`, contextual `tool_owl_eye` / `tool_bell` / `tool_scarecrow` / `tool_rain` | Every disc 108 × 108. Three-slot measured centers x=344/532/720, y=1802; four-slot centers x=260/440/622/808, y=1792. Hidden slots remain vacant, not rescaled | HUD |
| Tool inventory | `tool-disc`; P/R | `badge_red`; live count or baked plus | 64 diameter; same measured sparse offset (+33,−49) from each tool center; Undo has no stock badge | Above its tool |
| Light-step / chain feedback | New board-feedback pattern needed; existing gift/ribbon assets | `gift_feather`; `ribbon_green` with baked multiplier mark and live number | No final measured on-board bounds; see Geometry decision. Do not add a Song meter | HUD effects, not over counters |
| Tutorial hand | New tutorial pattern needed | `hand_pointer`, pointer decoration, not a hit target | Follows the supplied bird/knot/tool target, clamped to safe area; final measured size/pivot unresolved | Above board/HUD, below modals |
| Sleeping/stuck hints | `tooltip/bubble` | Cream icon-only bubble with tail; matching-species/song/moon cues or Undo/tool cues | Inner rect and envelope need measurement; never cover the indicated target or intercept input | Above HUD, below modals |
| Popup scrim, shadow and destination panel | `modal-panel` | Popup-owned assets and measured inner rect | Physical viewport scrim; panel geometry belongs to destination spec | Above **all** scenery, birds, flights, HUD and tutorial |

**Feet-line coordinates (B, not current GameLayout):** rows = ceil(branch count / 2), pitch = min(300,800/rows); omit absent slots, never move an odd-count final branch to another side.

| Rows | Left feet y | Right feet y |
|---|---|---|
| 2 | 925, 1225 | 1075, 1375 |
| 3 | 760, 1027, 1293 | 893, 1160, 1427 |
| 4 | 760, 960, 1160, 1360 | 860, 1060, 1260, 1460 |
| 5 | 760, 920, 1080, 1240, 1400 | 840, 1000, 1160, 1320, 1480 |

Use green forest, autumn forest, misty deep forest and snow forest treatments from the art direction; snow roots carry continuous snow caps/icicles. World identity/progression remains supplied by the catalog; this does not swap level worlds to match historical palette prose. Twigs, growing limbs and swivel knots modify the same painted slots, not separately pasted branches.

**Measured content bounds:** for `coin_pill`, M canvas is 320 × 128, inner rect `(102,27,115,74)`; multiply all coordinates by `190/320` in the pattern-sized pill. Number centered in that right-hand rect, not the full pill. M `stat_pill` canvas 400 × 96, inner `(23,17,354,60)`; `ribbon_green` 640 × 200, inner `(34,23,569,153)`; popup `panel_wood` 800 × 540, inner `(88,119,645,317)`. These are source-local rectangles, not permission to stretch existing art into an unmeasured move-pill/bubble layout. Apply `modal-panel` inner margins after scaling. No live label may extend outside its measured inner rect.

**Glyph normalization:** size and center the opaque G bounds, not the texture canvas; preserve aspect. For a chosen opaque longest edge D, scale = D/max(width,height), and compensate the recorded opaque-center offset. Tools use the `tool-disc` 54% target (58.32 px on 108); the white-glyph target conflicts below. Apply one target per glyph role across every state and board density.

### Decisions required before implementation approval

- **[Decision needed] Small marks and white glyphs.** Art direction says 46% white glyphs (49.68 on a 108 disc); brief says about 55% (59.4), all same visual size and nothing below 56. P explicitly requires 26 stars, 44 badges and 22 gift coins. Options: permit explicit small informational exceptions and use 55% white glyphs; or enlarge/reflow the HUD to enforce 56 everywhere. **Recommend:** approve the explicit P exceptions and 55% white glyphs, then update shared patterns; do not silently enlarge this screen alone.
- **[Decision needed] Geometry and missing patterns.** P/M do not define move-pill subrects/number typography, dial, badge pitch/hit areas, final bird packing/seat pitch, branch/knot hits, tutorial, bubbles, or light-step/chain bounds. Current code numbers are not measurements. Options: measure an approved revised board and add these shared entries; or explicitly approve current-code geometry where it still fits. **Recommend:** measure the revised board, keep P outer envelopes, give badge taps non-overlapping targets meeting the accessibility assumption, and resolve dense bird packing against the fixed feet lines before asset production. Do not promote the R nominal bird bounds to collision-free production sizes.
- **[Decision needed] Sapling beyond five rows.** Authored level 37 has ten occupied branch slots; current `GameRules.Booster` adds the first absent slot, producing an eleventh branch in row six. B only supplies paintings/heights through five rows. Options: approve a measured six-row painting/layout; or request separate gameplay authority to change Sapling availability/placement. **Recommend:** extend presentation with an approved six-row layout, preserving rules. Do not disable Sapling or invent a sixth-row height here.
- **[Decision needed] Modal darkness.** Brief says about 45% black; art direction/P/CONVENTIONS specify black alpha 0.72 (and conventions separately describe 55–60% resulting brightness). These are not equivalent. Options: 0.45 or 0.72 scrim, reviewed over both board densities. **Recommend:** 0.72 plus P shadow as the shared-library draft, pending coordinator/user reconciliation. Never claim both alpha and brightness targets were met automatically.
- **[Decision needed] Missing third addendum.** The assigned brief currently contains two titled addenda: gameplay and continuous tree painting. Options: coordinator supplies the third; or confirms those are the complete additions. **Recommend:** request the missing authority before approval; no extra decisions inferred from an absent addendum.

## 4. States

The development-only fake source must expose every row below without solving a real level, editing saves or depending on notification history. Values/permissions are supplied, not calculated by the view. Current catalog evidence: 40 levels, 2–7 real species, up to ten authored branches/five rows, capacity four, par up to 19; Sapling can add one branch. Crow is never a progress badge.

| State family | Required reachable fixtures |
|---|---|
| Data boundaries | Empty presentation/no snapshot; empty branch; zero deliveries/empty doorway; typical 240 coins; 0, 99999 coins; 2 and 7 species; 2/3/4/5 rows; odd branch count; all authored seats occupied; added branch including eleventh-branch case; all flocks home with crow remaining. Zero species is a missing/invalid-data fixture, never a playable level. |
| Numbers and text | Counted moves 0, par−1, par, par+1, par+3, par+4 and 2147483647; coin/inventory 1→0 and gain transitions, 99999 and 2147483647; star limits through 19/22; phase countdown 1/2/3; chain 0/1/2/3/7; approved overflow notation. Longest current name Flurry (six letters), expanded localization bake in Goal info, missing baked label. No name is a live board label. |
| Access/tools | First visit before any tool unlock; later visit; hidden Undo with no history, visible after one action, hidden after final undo; each booster locked/unlocked; owned/zero-plus; no-effect disabled; Sapling already used; context absent/present; each of the six kinds in its applicable slot; hint pending, successful transfer hint, swivel hint, no-solution, timeout/unknown and failed request. Show three- and four-slot layouts even when Undo is hidden. |
| Selection/input | Idle; selected exposed run; cancellation by same branch/empty space; empty/egg tip; sleeping bird tapped; legal partial/full transfer; wrong species, full and egg-blocked target; rapid repeated taps; in-flight blocked input; one queued landing/effect tap; rejected stale queued action. |
| Resolution | Drawn takeoff/flight/landing; flock delivery to the single house; pending→home badge with gift; multiple flocks/cascade; undo last delivery with/without another species still home; each gift kind, no-target gift, light-step grant/consume/undo, Berry free transfer/free swivel; chain absent, x2, x3 and higher; no Song meter. |
| Mechanics | Mystery/revealed egg, wobble/hatch; authored sleeper, nocturnal day sleeper, both causes together, wake but still nocturnally asleep; mama plus following chicks/target capacity reached; crow parking/departure; twig, partial/full growing branch, upgraded twig; swivel; no day/night, day, night, one move until flip, flip/reset, Moonbeam active/expiry, gift-triggered phase change, undo restoring each. |
| Tutorial/entrance | Every entrance state in §7; skip in each phase, repeated skip, instant/reduced motion, cancellation/focus loss in each phase; first-play and replay; level 1 source→target prompts for 0→1 then 2→0; all current tutorial levels 2,3,6,11,15,21,24,31,33; level 2 hint prompt; level 4 owned-Sapling prompt; expected action, off-script action, undo/restart, modal suppression and completion. |
| Recovery/end | Stuck/no legal move (not a loss), hint bubble visible/dismissed, one/two/three-star clear, same-star replay, upgraded-star replay/top-up, world-ending win, final-level return. No fail panel or timer loss. |
| Opened surfaces | Settings; Goal info for every species pending/home and longest name; Shop Boosters focus for each zero-stock tool, Shop confirmation affordable/unaffordable/cancel/success; Win for each outcome; New friend and Booster unlock queued singly/multiply after entrance; World complete and downstream World unlock/map/Daily gift return flow. These are distinct switcher states, not one generic popup. |
| Environment/lifecycle | Every world/variant; normal/muted; focus loss/resume; background/foreground with modal open; entrance interrupted and restored; local loading/no snapshot; missing required asset/data; offline with local data available (normal playable board, no network spinner). A load error must not masquerade as an empty playable board. |

**[Decision needed] Numeric formatting and missing-data presentation.** Source bounds are available: `GameProfile` coins/stock and `BoardState.CountedMoves` are integers without a five-digit presentation cap; `GameSession.GrantBooster` permits stock through 2147483647. The catalog has star targets through 19/22, enabled day lengths 2/3, and at most seven homecoming chain steps. P does not settle ten-digit coin/stock/move layouts or a loading/error surface. Options: measured full-number fields through 2147483647; or capped notation with a baked overflow symbol and documented thresholds. Missing required data: approve an icon-only blocking recovery panel or a return-to-map request. **Recommend:** measure full required 99999 coins and adopt approved capped notation for larger coin/stock/move values; return to map on unrecoverable board data rather than fake zero counts. Never clamp saved values or invent error copy.

## 5. Interaction map

Each action raises a request; only logic's response changes displayed state. No haptics are specified in the shared patterns: **none** for all rows. Button gestures use P `button-press / release`; `ui-tap` on pointer down. Disabled controls are dimmed and inert, including no sound/event; locked tools are absent, not purchasable through a hidden hit area.

| Input | Event and outcome | Visual feedback / named sound |
|---|---|---|
| Tap awake branch/run | `BranchRequested(id)` → supplied selection or committed move | Raised selection using drawn hop; transfer uses `bird-flight`. Board-action sounds require the mapping decision below; illegal result uses `refuse`. |
| Tap selected branch / empty background | `BranchRequested(id)` / `SelectionCancelRequested` → supplied cleared selection | Birds settle, hints hide; no extra sound on empty background. |
| Tap illegal target / sleeping bird / egg tip | Request action; logic rejects without spending move/stock | Drawn refusal; sleeping icon-only `tooltip/bubble`, 1.6 s or next tap, no forced wake; `refuse`. |
| Tap swivel knot | `SwivelRequested(id)`; independent of branch selection hit | Drawn hops/reordered seats and knot feedback, counted/free result supplied; sound mapping below. |
| Tap Undo | `UndoRequested` → restored full snapshot | Restore birds, gift flags, badges, doorway, phase/counts and stock together; `undo` when applied. Hide Undo if no history remains. |
| Tap owned, effective tool | `BoosterRequested(kind)`; Firefly can return pending then hint | P press feedback, supplied stock update; `booster-<kind>` only when applied. Firefly draws the supplied route, never computes it. |
| Tap zero-stock plus | `ShopRequested(kind)` → focused Shop | P press and `ui-tap`; no booster effect/spend sound. |
| Tap progress badge / Settings | `GoalInfoRequested(species)` / `SettingsRequested` | P press, `ui-tap`, then `panel-open`; suppress hand/bubbles and block board. |
| Close/back/dim tap on dismissible modal | `DismissTopRequested` → destination-specific return | `panel-close`; board snapshot retained. Back on board raises `SettingsRequested`. Acknowledgement-only cards ignore dim/back. |
| Tap during entrance | `EntranceSkipRequested` only | Settle exactly once, no synthetic game move; silent. |
| Tap during flight / landing | Flight: no gameplay request. Landing/effect: at most one `DeferredActionRequested(action)` | No premature selection/press/stock change; logic later accepts or rejects the queued intent. |
| Tap sleeping/stuck bubble or elsewhere | `HintDismissRequested`; underlying target may also raise its normal request | Bubble hides without stealing the tap; no extra sound. |
| Follow/ignore tutorial hand | Normal target event; hand itself never receives input | Hand repositions only from supplied tutorial target. Off-script completion/undo/restart hides it when instructed; no hand sound. |

**[Decision needed] Board-action sound/motion entries.** P names UI, booster and refusal sounds, but not selection, transfer, landing, homecoming, hatch, swivel, phase change, chain or tutorial motion. Options: extend shared tables with existing runtime sounds (`Sfx.Select`, `Flutter`, `Move`, `Collect`, etc.) and measured motion; or explicitly approve silence for uncovered events. **Recommend:** extend shared tables using existing assets; do not manufacture event names or attach `coin-gain` to a delivery that awarded no coins. Until resolved, listed missing sounds are unapproved, not a claim that these interactions are finished.

## 6. View contract

Proposed presentation boundary below is **not** a claim that these APIs already exist. Both fake and real adapters must provide it; `GameRoot` remains the runtime coordinator. View bookkeeping may track active animation handles/IDs, never eligibility, scoring, unlocks, delivery rules, tutorial advancement, solver decisions, purchases or saves.

| Data shown | Source and update moment | Missing behavior |
|---|---|---|
| Level/world/variant, ordered branches, capacities/kinds, stable bird IDs/species/flags/outfits/positions | Level/session adapter on Show, restore and committed resolution | Required: no interactive board until snapshot exists; recovery decision in §4. Never invent birds/seats. |
| Balance; stock per tool; slot kinds, visible/enabled/zero-plus; Undo available | Profile/economy/session adapter on entry, action, purchase and undo | Missing balance/stock is unavailable, not 0; block spending. Keep valid prior value only if explicitly marked current by adapter. |
| Counted moves, three/two-star limits, one-star-any-clear, reachable stars, free-move result | Session/scoring adapter on entry and resolved action/undo | Do not calculate par+3 or infer free moves. Missing field suppresses affected information and blocks approval pending recovery policy. |
| Ordered goal species, portrait/name/gift asset keys, delivered flags, resident species/style | Catalog plus session adapter; delivery events, equip and restore | No crow badge, no guessed gift. Missing required art is recovery, not another species portrait. |
| Day/night enabled, phase, moves left, Moonbeam status, light-step, chain | Session adapter after each corresponding event and undo | Explicit disabled hides dial/status; missing is not equivalent to disabled. |
| Tutorial target kind/ID, visible/completed; hint path/reason-icon; stuck hint; input/notification/entrance phase | Tutorial/solver/progression coordinator, on each transition | No target/hint means hidden. Never derive legal moves in the view. |
| Mute, focus, motion mode, current overlay and input permissions | Audio/navigation/lifecycle coordinator when changed | Preserve service policy; view never stores mute/progress preferences. |

Calls logic makes: `Show(BoardSnapshot)`, `UpdateHud(HudSnapshot)`, `SetSelection(SelectionPresentation)`, `PlayResolution(orderedEvents, transactionId)`, `Restore(BoardSnapshot)`, `SetInput(InputPresentation)`, `ShowHint(HintPresentation)`, `ShowBubble(BubblePresentation)`, `SetTutorial(TutorialPresentation)`, `PlayEntrance(EntrancePresentation)`, `SkipEntrance()`, `SetOverlay(OverlayPresentation)`, `SetLifecycle(focus, motionMode)`, `HideAndCancel()`.

Events raised: every named request in §5, plus `EntrancePresented(transactionId)` and `ResolutionPresented(transactionId)` exactly once, and `PresentationFailed(requiredKey)`. Overlay actions (Restart, map, purchase, Next, Replay, acknowledgement, mute) go to their owners, not board-local logic. Transaction IDs invalidate stale callbacks after skip/restore/hide. Countdown interpolation is cosmetic; authoritative values come from the adapter. No view action writes PlayerPrefs or consumes inventory.

## 7. Motion and sound

Reference P animation/sound names below rather than inventing per-control alternatives. Loop `bgm-game` on this page (existing service asset `bgm_game`); 1 s crossfade from `bgm-menu`, never restart for popups. Mute silences UI/SFX/music together; focus loss pauses, first authorized user interaction retries playback. No new volume controls.

### Entrance state sequence

Clock is from initiating Play/Next/Restart to the settled board, **including** any map-to-level wipe; unskipped total 3.5–4.5 s. Gameplay unlocks at that boundary only if no introduction card is queued; the entrance/notification timing distinction requires approval below. Schedule rows concurrently, not serially across every bird. Existing timing constants provide the draft schedule; final measurements must satisfy the total rather than append delays.

| State | Presentation and transition | Timing / easing / input |
|---|---|---|
| Prepared / covered | Snapshot bound, doorway empty, no Undo, hand hidden; awake birds completely beyond their branch's own screen edge or above screen | Existing leaf wipe 0.4 s on surface change; smoothstep; blocks play. No mirror-generated poses. |
| House | Single rope/house drops from completely above viewport to its §3 anchor; background reveals | 0.8 s soft out-back, from integration entrance contract; silent unless shared sound mapping approved. |
| Branches | Reveal continuous painting from trunk to tip, not separately moving cutout joints | Each row 0.7 s out-cubic, rows start 0.15 s apart; starts after house. |
| Birds | Start after their own row reveal; drawn directional takeoff/flap/land, source fully off-screen, target exact supplied seat | 0.08 s row-local stagger; 1.0 s ease-out flight; 0.10 s landing squash. Eggs/sleepers stay at seats and reveal over current 0.25 s seated entrance rather than flying while asleep. |
| Settle / controls | Final positions exact; uniform tools/HUD fade in, no miniature first-level tools | Current final-controls reveal 0.45 s overlaps ending; wait for all arrivals and minimum total. No gameplay input until settled. |
| Ready / notifications / tutorial | Emit completion once; coordinator opens queued introduction cards or supplies playable/tutorial state | Unskipped settled-board boundary by 4.5 s; first hand/play only after top cards close. Skip settles immediately to this same snapshot and cannot activate the tapped tool beneath. |

Tap skips from any visible entrance phase. Instant/reduced-motion skips motion with the same final state; no 3.5 s artificial wait. Hide/restore cancels all entrance callbacks. **[Decision needed] Entrance exceptions and timing boundary.** Brief says birds fly from outside the screen, whereas current eggs/sleepers reveal at seats. Options: retain that exception or approve a carrying/arrival treatment. Also, “tap to playable” conflicts with post-entrance acknowledgement cards of user-controlled duration: measure tap-to-settled-board, or move those cards before the measured entrance. **Recommend:** approve seated reveal for eggs/sleepers and measure tap-to-settled-board, logging modal-blocked and actual playable times separately. Never claim a board blocked by a card is playable. Measured board-piece geometry from §3 must also settle off-screen bounds.

| Trigger | Animation / duration / easing | Sound and music |
|---|---|---|
| Any enabled button gesture | P `button-press / release` | P `ui-tap`; game music unchanged |
| Modal open/close | P `panel-open` / `panel-close`; all child controls visible together | Input `ui-tap` where applicable; no new popup cue; keep game music |
| Coins/moves change; delivery badge | P `counter`; `badge-pop` | P `coin-spend` / `coin-gain` only for actual currency change; gift/delivery mapping pending |
| Select/cancel/refusal | Current select 0.15 s out-back, drawn hop; drawn refusal, not shaking/mirroring a still | Refusal P `refuse`; other board cues pending shared-table decision |
| Transfer/homecoming/cascade | P `bird-flight` (14 fps); each flock enters the **same** masked doorway; badge lights/checks only for supplied completed species | Mapping pending; Win owns P `win-jingle` / `star-pop`; no premature win panel |
| Phase flip | Current 0.6 s lighting interpolation after supplied resolution; sun/moon/count update from supplied event | No music restart; sound mapping pending |
| Idle birds, including resident | Art-direction continuous breathing 2.4–3.2 s plus real drawn actions 1–3 s apart; eased action travel, hard frame swaps at authored rate; at most three per branch and no neighbour same-action starts within 0.6 s | No repetitive UI sound; ambient continues. Never flip a still to turn |
| Tutorial / Firefly hint | Current hand pulse loop 0.9 s; supplied source/target guidance. Current hint route loop 1.5 s until next action/hide; no live explanatory paragraph | No hand sound; P `booster-Firefly` on applied hint only; missing pattern entry noted above |
| Sleep / stuck bubble | P `tooltip/bubble`: 1.6 s then hide or next tap. Stuck retains current 0.8 s settled delay; reset on action/modal/surface change | Sleep refusal P `refuse`; automatic stuck suggestion silent. Do not pulse-scale the actual Undo disc |
| Exit / restore / focus loss | Existing leaf wipe 0.4 s for map exit; cancel timers/tweens on hide; restore committed snapshot, not intermediate flight | Crossfade to `bgm-menu` for map; focus loss pauses audio |

**[Decision needed] First tutorial target progression.** GAME_DESIGN asks for source then target hand; current `TutorialHand.Refresh` hides it once a branch is selected. Options: supply source→target prompts for transfer lessons; or retain source-only prompts. **Recommend:** source→target with coordinator-owned progression, including level 1's two moves; no view-side legal-move checks.

## 8. Accessibility

**NOT ASSESSED:** no project contrast ratio, physical touch-size/device target or accessibility certification is defined. Assumption from art direction: at least 88 × 88 design-px touch targets; all HUD discs are 108. Small visual progress badges require separate non-overlapping targets (Geometry decision), not 44 px taps. Branch and knot hits must not overlap adjacent actions after dense packing.

Species use silhouette and color; delivery uses a check, sleep uses pose/icon, phase uses sun/moon, star loss uses lit/dim shapes, disabled controls are inert at unchanged size. Provide nonvisible accessible names for icons and numeric values; do not add visible text labels. Reduced motion uses instant entrance/counters/panels, no decorative pulses or bird transformations; static final poses here are an explicit accessibility/instant-mode exception, not normal animation acceptance. Retain immediate selection/check/phase cues. Localization changes baked assets, not live names or squeezed type.

## 9. Text

Fixed text is baked; only changing numbers are live, including numeric parts of multipliers. No autosize-to-fit solution that makes equivalent values illegibly small. Exact font sizes require measured number-layout approval in §4, not copied arbitrary runtime font constants.

| Element | Limit / rendering / overflow |
|---|---|
| Coins | 0–99999, at most five live digits for required fixture; centered in M coin right-hand inner rect. Larger values need the §4 decision, not silent truncation. |
| Counted moves; star limits; phase countdown | Moves: source domain 0–2147483647 (ten digits), approved display overflow needed. Catalog three/two-star limits: ≤19/22 (two digits); supplied logic values, not view calculations. Phase countdown: 1–3 (one digit) for current enabled levels, reset atomically on flip. Fixed one-star-any-clear mark baked as an icon. Measured numeric subrects remain unresolved. |
| Inventory badge | Source domain 0–2147483647; positive count live, maximum ten digits before approved overflow formatting. Zero uses baked plus, not live `+` text. Undo has no numeric stock. |
| Chain multiplier | Baked multiplication mark plus one live digit, 2–7 for current catalog; 0/1 hides the ribbon. No live `x` label. Measured on-board bounds remain unresolved. |
| Goal species names and effects | Board shows faces/gift icons only: zero visible name/effect characters. Goal info receives baked name/effect assets; exercise Flurry (six letters) and expanded localization there. No runtime truncation or live fallback. |
| Tutorial, sleeping/stuck/hint feedback, controls | Zero live words; icon-only. Zzz, if retained, is sprite art. Remove current dynamic hint explanation strings. |
| Destination popup numbers/names/rewards | Owned by each destination spec; fake board routes their boundary fixtures, including 99999 balance, longest name and star-upgrade top-up. No extra board text behind them. |

## 10. Acceptance criteria

These are future shell review checks, **not results of this documentation task**. Open decisions block approval of their affected checks; do not substitute code constants.

1. At both 1080 × 1920 and 1080 × 2400 with notches, the full board, tool targets and counters remain inside the safe area; only scenery/background extends to edges. No text leaves its measured container, including 99999 coins and all approved maximum numeric/localized fixtures.
2. Every board tool and Settings disc has an unchanged 108 × 108 layout/hit rectangle in normal, disabled, busy and selected states; transient press visual uses 0.92 then returns exactly. Disabled alpha is 0.45 and raises no event. First level never substitutes 64 px tools; Undo is absent, including its hit area, with no history.
3. Compare opaque glyph bounds, not texture sizes: every tool illustration has longest edge 58.32 px at rest and is optically centered from recorded bounds; all white disc glyphs share the single approved target after the Small marks decision. No tool changes size because its icon or stock changes.
4. Exactly one 240 × 240 goal-house envelope and one rope exist at every species count. Start shows an empty doorway; delivery lights/checks only the matching badge and shows an animated resident. Undo restores the supplied badge/resident state, including another flock still home. Tapping each badge opens its own Goal info, never a neighbour's.
5. Two-row feet lines are left 925/1225 and right 1075/1375; five-row lines are left 760/920/1080/1240/1400 and right 840/1000/1160/1320/1480. First centers clear the edge by at least 150; no floating feet, root seams or independently pasted bark. Sapling's eleventh branch requires approved six-row evidence before acceptance.
6. At par and immediately above it, reachable stars/limits match the supplied model; a free move flashes the footprint/feather without incrementing the counted number, while the independently supplied phase counter may change. Undo restores both. With day/night disabled the dial is absent; otherwise one-move-left and phase reset are legible.
7. With real motion enabled and no queued introduction card, timestamped frames show house→row reveal→bird arrival and input readiness 3.5–4.5 s after entry tap, including wipe. With cards queued, log settled-board and actual playable times separately under the Entrance decision. Every phase can be skipped once without a move, purchase, duplicate callback or leftover off-screen bird. This bounded entrance is the current performance criterion; device frame-time budget is NOT ASSESSED without a named device/budget.
8. A timed normal-play capture shows continuous breathing and actual drawn idle/turn/hop frames, with 1–3 s idle opportunities, no mirrored bird, ghost frame or normal-mode frozen resident. Muted/unmuted audio logs show looping game music, no popup restart and correct first-input/focus recovery.
9. Over the busiest snow board, each opened panel is clearly separated by a visible shadow and full-viewport black scrim at the approved alpha (draft recommendation 0.72; shadow offset 14, alpha 0.35). No bird, flight, hand or HUD appears above it and no tap reaches the board. Panel children stay inside measured cream bounds with 24 px margin.
10. Every §4 fixture is selectable without modifying real saves. Sleep/stuck bubbles dismiss at 1.6 s or next tap; stuck starts only after the supplied settled delay and is not a fail state. No extra text paragraph or actual Undo scale-pulse appears.
11. Requests alone never change coins, stock, moves, stars, home flags or branch capacity. A rejected request, offline local play, interrupted flight, cancelled hint and repeated popup acknowledgement cannot produce a duplicate effect or reward. Returning from Goal info/Shop/Settings preserves the supplied attempt.

## Differences from the current build

Source inspection only; no runtime, Unity or tests were run for this draft. Inspected canonical `GameRoot`, `HudView`, `BoardView`, `BranchView`, `BirdView`, `BirdhouseView`, `SceneView`, `LevelEntrance`, `TutorialHand`, `GameLayout`, plus hint/transition/glyph owners.

- `HudView.SetLevel` builds a house per species and a beam above three species; `BirdhouseView` uses long press. Replace with one shared house and tappable gift-bearing progress badges, retaining delivery/undo semantics.
- `HudView` has no counted-move/star-target pill; dial is currently a separate fixed rect, not beneath it. Add supplied move/free-move data and relocate the dial after measurement.
- `SceneView`/`BranchView` assemble crowns, trunks, limb and junction art. Replace that composition with continuous per-layout paintings/reveals; `GameLayout.RowPitch` currently uses 350 or 880/(rows−0.5), and first slot 100, contrary to the brief's fixed heights/150 clearance.
- Tools currently use sparse 108 or dense 128–132 measured rectangles, first-tutorial bounds of 64, badges 46/50, and context-dependent gear rectangles. Use P's uniform 108 discs/64 badges; keep progression-driven hiding rather than exposing locked actions.
- `GlyphMetrics` currently selects 46% white/54% tools; white percentage remains an explicit shared decision. Coin layout uses a custom 232-wide sprite canvas and autosizing, rather than P/M's 190 × 76 measured-inner layout.
- `StuckHint` currently has no 1.6 s timeout and pulse-scales Undo. `BoardView.Hint` can show a live explanation; the target uses icon-only bubbles and no real-button size pulse. Tutorial currently hides on selection rather than pointing to the target.
- Current entrance already has timing/skip/settled callbacks and seated sleeper/egg reveal, but starts after a separate 0.4 s wipe; verify the total entry-tap budget rather than assuming its 3.5 s minimum proves it. Modal dim constant currently reads 0.55; shared darkness conflict remains unresolved.
- Current views call `GameRoot`/session directly for eligibility, tutorial checks and stuck detection. Stage-four fake-data adapters must drive the same canonical views through the presentation-only contract; no second scene or parallel board implementation.
