> **T30 current-engine scope (2026-11-01):** [GODOT_MIGRATION](../GODOT_MIGRATION.md) owns active implementation, cloud-first testing and gaps. Unity/C#/LitMotion/PlayerPrefs/Canvas/asmdef commands, APIs, dimensions and test results retained below are **legacy evidence/reference, not active Godot instructions or pass claims**. Engine-independent rule/content/asset-approval requirements remain; current engine mappings are in the migration matrix. Other feature expansion is stopped during migration. Native save version1 is isolated from Unity save6 and design-only economy save7.

# Current engine binding for interaction

The state machine/input-lock/event-order requirements are engine-independent and remain applicable to the migrated L01. Godot native nodes/input, Tween callbacks and scene lifecycle replace Unity component/coroutine/timeScale APIs. Session commits stable candidate/save first, then view consumes Moved→Home→Retired events. Native scene teardown/restart invalidates stale callback generations; there is no active asmdef/UnityEngine dependency. Keep English/Fredoka text and existing audio, selected/standing/flight poses; no new shop/skills behavior is developed during the framework migration. Current test/visual evidence is in GODOT_MIGRATION, not inherited from the legacy sections below.

## Retained legacy/reference details (not current engine workflow)

# Perch Sort: interaction, presentation, and input safety

2026-10-31 · Same-genre evidence revision · Documentation contract, not implemented; Unity has not been run. Gameplay is admitted only through [RULES](RULES.md) and the [evidence matrix](../MECHANIC_EVIDENCE.md). This page must not quietly restore withdrawn rules through animation, controls, or old assets. A fixed layout does not prohibit adding space during play.

## 1. Separate evidence, project baselines, and engineering protocols

- **Same-genre gameplay evidence**: Leafy Bird Sort's source→destination input, same-color exposed ends, and limited capacity; Mobirix's question-mark eggs/Undo/extra branches; Bird Sort 2's loss of a branch after a completed flock flies away; BOOPFUN's reserve bottles and repeated height increases. RULES determines adoption and exact boundaries; listing a tool in another product does not automatically authorize implementing it here.
- **PROJECT_VISUAL_BASELINE**: Static assets/dimensions are in [UI_SPEC](../UI_SPEC.md). The durations/arcs/staggering below retain earlier presentation suggestions. All are project tuning values, not measurements of those competitors. They express approved logic without changing action counts, capacity, or triggers.
- **ENGINEERING_SAFETY**: Complete transactions, atomic saves, stale callbacks, and protection against concurrent double taps are reliability requirements; see [DATA_SAVE](DATA_SAVE.md). They do not claim that competitors use the same implementation.
- **WITHDRAWN**: Dual-active target queues, sleeping/waking future flocks, exact RevealAfter/RevealSpecies crate conditions, eggs whose patterns announce species, branches that retire whenever emptied after first receiving birds, wind gates, Undo/Restart penalty stars, and 3 hints per level. Do not animate these actions; old code/assets do not establish valid new requirements.

Source descriptions are self-contained in [research](../SORTING_RESEARCH.md). Original pages: [Leafy](https://play.google.com/store/apps/details?id=com.leaf.birdsort&hl=en), [Mobirix](https://play.google.com/store/apps/details?id=com.mobirix.ldbis&hl=en), [Bird Sort 2](https://play.google.com/store/apps/details?id=com.globalplay.birdsort2.color.puzzle&hl=en-US), [BOOPFUN](https://play.google.com/store/apps/details?id=com.boopfun.watersort).

## 2. Single-source timing and path table

This page owns AN01..AN14 timing keys; seconds use s and displacement uses design px. Reference UI-D for layout. Events outside RULES' effective configuration must not be created even if this table suggests their presentation. Each level uses uniform capacity N and N birds per species, N=4/6/8; RULES R01/R02/R04 alone own configuration, maximum runs, and completion conditions. AN06 consumes only confirmed Home events; AN07 then presents that completed branch's Retired event. These cannot be substituted with retirement whenever birds leave a branch empty. Staggered flight of N birds takes one bird's flight time+(N−1)×takeoff interval, plus separate landing/reception stages; do not hardcode N as 6 or infer six birds per species from animation.

| Key/event | Presentation and stable end state | Suggested project timing/amplitude (not competitor measurements) |
| --- | --- | --- |
| AN01 Menu exit | Fade Logo/buttons/decorations to background only; finish before AN02, without overlapping board images | 0.25s |
| AN02 Level entry | Read the fixed initial state; position branches and reception/progress visuals, then show birds in their corresponding slots in sequence; omit the removed target queue | Board stage 0.55–1.05s, about 0.05s between branches; loading is additional |
| AN03 Select/cancel | Raise/outline only the exposed movable run allowed by rules; cancel returns it to its original slots; do not select inner birds of the same species | Raise 30 design px, 0.12–0.18s |
| AN04 Invalid feedback | Brief outline in place; no token movement or action count; do not imply interactivity without a source or with unknown destination capability | 0.12–0.18s; no shake with reduced motion |
| AN05 Cross-branch flight | Use accepted multiframe/skeletal clips with the existing character design for turning and real periodic wingbeats; translating a single fly image fails acceptance. Move committed Count birds in staggered arcs to actual slots, then fold wings and settle after feet contact the branch | Per bird 0.35–0.45s; stagger 0.05–0.07s; landing 0.12–0.18s |
| AN06 Completed flock goes home | Consume only completion events confirmed by rules; fly birds individually toward the existing nest, with real approach/wing-folding/front-and-back entrance occlusion/landing reception stages; do not use the old top queue to determine eligibility; no sleeping | Per bird 0.50–0.60s, stagger about 0.06s; final reception 0.20–0.35s |
| AN07 Completed branch exit | This revision adopts SRC-BIRD2-style branch loss after completion: retract the original branch after its completed flock safely leaves; retain its positional ID; ordinary emptying never triggers it | 0.20–0.35s; may connect to final reception without obscuring causality |
| AN08 Question-mark egg reveal | Use SRC-QUEST's question-mark egg only on a valid Hatch event: slight wobble/cracked shell, then the same token becomes a bird in place; do not reveal hidden species early or reroll it | 0.25–0.40s; disable the level if boundaries are insufficient; animation must not fill algorithm gaps |
| AN09 Add an extra ordinary branch | Consume only R07's committed BranchAdded; reveal reserved empty branch S(P+1); no shuffling or movement of original branches/birds. Repeated height increases remain pending evidence and must not share this event. Lock input until reveal completes; background recovery restores the committed end state directly | New branch reveal 0.20–0.35s, project presentation tuning only; R07 alone owns capacity N and usage count |
| AN10 Undo restoration | Restore the entire prior state, including completed branch availability, birds, and approved assistance state; do not repeat completion/rewards | 0.10–0.20s |
| AN11 Buttons | Press scale 1→0.92, release via 1.06 back to1; Play/Next may breathe while stable and interactive; disabled buttons do not trigger | Press 0.08s, release 0.18s; breathing 1↔1.05/period 1.6s |
| AN12 Dialogs | Open scale 0.85→1.05→1 with alpha entering first; close scale 1→0.9 with alpha exit; prevent input through modals | Open 0.25s/fade in during first 0.10s; close 0.15s |
| AN13 Overlay/HUD | Overlay alpha comes from UI_SPEC; pause/settings/no-moves panels darken only through the overlay; victory fades HUD separately | Overlay fade-in0.20s, victory HUD fade-out 0.20s |
| AN14 Victory celebration | After the final completion presentation, show the established trophy/bird/confetti and navigation; assets do not automatically authorize lighting three stars, awarding coins, or increasing progression values | Confetti about 2.2s; interactive once the main content is stable, without waiting for confetti to end |

Flight endpoints come from actual slots in committed before/after snapshots, using the character's foot pivot. Curve control points affect presentation only, never token slice order, count, or IDs. Horizontal mirroring does not reverse rule arrays. Adding/losing space during play does not compact fixed IDs. The home container is existing thematic presentation, not an automatic introduction of locked cages/target queues; unverified crate-opening logic has no dedicated effective animation.

## 3. Input and transaction contract (engineering reliability)

`Loading→Entering→Idle↔Selected→Resolving→Presenting→Idle/Won/NoMoves`. Save failures enter `SaveError`; DATA_SAVE alone specifies candidate retention/retry/readback, without a separate policy here.

| State | Accepted | Rejection/exit conditions |
| --- | --- | --- |
| Loading/Entering | System background/exit | Reject board/assistance/Next; enter Idle only after the fixed initial state loads and everything settles |
| Idle | Valid source, approved and available Undo/Restart/Hint/expansion actions, pause | Do not activate buttons without admitted rules or parameters; invalid requests do not change the board |
| Selected | Valid destination, same-source/blank cancel, available assistance/pause | Valid assistance cancels selection first; invalid destination gives brief feedback and returns to Idle; invalid assistance preserves selection |
| Resolving/Presenting | System background; at most one pending pause request | Do not queue moves/Undo/Restart/Hint/expansion/Next; unlock after stable commit and all event playback/view synchronization |
| SaveError | Verification/retry defined by DATA_SAVE, return to menu/exit | Do not play unconfirmed success, create a second candidate, or remain permanently stuck in Resolving |
| Paused/Settings | Modal controls | No board/HUD input through modal; close to stable state |
| Won | Valid Next for this level (none on final level), Replay, Home | No further bird movement, duplicate rewards, or skipping two levels |
| NoMoves | Available Undo/Restart and explicitly approved assistance | No legal moves does not imply global unsolvability; assistance without rule admission cannot rescue play through an advertised extra branch |

Hide Undo without history, retaining its original HUD slot; invalid requests preserve selection. Undo remains free/unlimited. Current Hint policy is ECONOMY_SHOP helpPolicyVersion2: one free use per attempt then inventory, replacing the old unlimited-Hint project policy; the source’s original policy remains historical evidence. Run only one hint query at a time; answers must match the current level hash/stable state/generation, and stale results must neither execute nor overwrite highlighting. Do not restore product quotas such as 3 per level/6000 nodes/1.5 seconds from old badges; search budgets belong only to data/validation engineering contracts, not competitor gameplay facts.

Stabilize the full logical candidate and confirm atomic commit before playing events; animation callbacks cannot advance completion/rewards. Increment viewGeneration on every load; background recovery synchronizes from the stable snapshot, and old coroutines cannot unlock a new level. Selection/spring effects/particles are transient visuals, not save authority.

## 4. Information, scoring, sound, and accessibility

Leafy supports higher scores for fewer moves without publishing score/star formulas. This revision records historical best action counts under RULES, without separate Undo/Restart penalties; RULES and DATA_SAVE own counting/restoration. Do not retain penalties such as “Undo+Restart=0 gives three stars, 1–3 gives two,” or invent replacement formulas. Existing star slots/progress signs are dimension baselines only; do not drive them without effective calculation rules. Artwork cannot imply star-based unlocking or invented star rewards. Coin rewards/help inventory now follow the adopted ECONOMY_SHOP design; do not substitute old quotas or invent other item costs. Runtime implementation remains pending.

Do not leak question-mark eggs' hidden identity through species patterns/silhouettes, hint highlights, timbre, accessibility descriptions, or previews. Restart/Replay do not randomize the same initial state. RULES decides whether eggs are adopted; otherwise do not use AN08. Hidden-information fairness requires separate strategy acceptance using only player-visible information, beyond an omniscient solution.

Reduced motion removes large arcs/spring effects/dense particles while preserving actual bird counts, branch loss after completion, assistance-space changes, and navigation order. System preference reading is not integrated; do not claim support or add a settings entry. Muting/grayscale must not obscure visible species, capacity, or unavailable branches. Retain existing short action sounds; play completion/victory once per event and limit overlapping sound from staggered flocks. New sounds/haptics are presentation suggestions only: no changed actions/rewards or additional system permissions/settings.

Provide six timed frames and a recording from actual runtime for every acceptance item; label static storyboards “illustrative.” Input locks/actual logical touch dimensions/safe areas/background behavior/sound require interaction or device tests, never images as substitutes. New derived assets require acceptance before future integration; this task creates no assets.

## 5. World map and icon conventions (explicit user requirements)

[MAP_PROGRESSION](../MAP_PROGRESSION.md) alone owns four-world allocation, sequential unlocking, and Replay boundaries; do not repeat divisions/thresholds here. Current theme concepts are not accepted final assets and must not overwrite original UI. Mark undecided map-node positions/page layout PENDING_LAYOUT. Unlocking/entry are navigation only and do not change fixed initial states.

Buttons use regular circles/rounded rectangles. Use icons alone where clear, without duplicate visible labels beside them. Accessibility names serve assistive technology and are not visible button text; level numbers and necessary error messages are exempt. Settings retain the established single sound button; music/game effects/click sounds only mute/unmute and may share global mute. Do not add volume sliders/percentages or independently split them into three controls. Map locked/completed/current states use status icons, not irregular hit regions in place of regular buttons.

## 6. Real animation and open-source library priority (USER_REQUIREMENT)

Every animation location must visibly change over time; a fixed image cannot stand in for completed takeoff/landing/nest entry/results. Flight combines visible wingbeats and a trajectory; nest entry includes approach/entry/reception; results include a staged sequence of panels, characters, or particles. Backgrounds/raw pose frames may be static assets, but must combine into actions over time; camera screenshots or text cannot prove motion.

Prefer a suitable open-source animation library for existing tween needs, with one primary tween library and no duplicate hand-written tween infrastructure. [ANIMATION_LIBRARIES](ANIMATION_LIBRARIES.md) alone owns candidates, licenses, Unity compatibility, and selection status; do not describe candidates as selected/installed or DOTween as MIT. Documentation only now; no package installation. Libraries own presentation, not R01..R07 logic; background cancellation/generation/atomic commit constraints remain.

Acceptance requires actual runtime recordings, beyond static images, and checks that every claimed animation location has a dynamic sequence. User-specified icons, regular buttons, simple settings, and mute controls are also USER_REQUIREMENT, not attributed to competitor rules.

## 7. Complete R07 AddBranch input flow

Callable in Idle/Selected/NoMoves with extraBranchAdded=false; a valid call cancels selection and the current Hint, locks input, and constructs one AddBranch candidate. Play AN09 only after atomic save succeeds. Double taps are not queued and cannot create a second branch; failure retains the same-sequence candidate under SaveError. Addition is unavailable after use or in Won, Paused/Settings, or Resolving/Presenting.

The NoMoves modal owns its add-branch control at the same location as the underlying HUD, which receives no raycasts. After success, close or retain NoMoves according to the new legal Transfer set, without returning from ads/store. Undo restores the final branch's existence and extra-branch state according to the latest transaction; AN10 synchronizes snapshots only, without resolving Home again. DATA_SAVE owns Restart/exit recovery. A full completed flock on the new branch still plays AN06→AN07; ordinary emptying does not retire it. UI_SPEC UI-D41/42 alone own entry layout; implementation is prohibited until new sliced assets pass acceptance.

## 12. Shop/help-skill interaction extension — DESIGN_ONLY

[ECONOMY_SHOP](ECONOMY_SHOP.md) owns exact transaction/state policy. Open shop/loadout only outside an active unpaused board; new equipment is frozen at next attempt. Preview/confirm purchase, upgrade, replacement-slot and Rewind/Shuffle candidates; Cancel before commit is free. Disable duplicate submit while pending; never auto-buy when stock is0. Show Free allowance, owned inventory and skill level distinctly. Undo/Restart stay independent free controls.

At stable input, a confirmed help transaction atomically commits its allowance/inventory and any board/history result BEFORE animation. Only one hint/candidate query; bind exact state/generation and discard stale results without consumption. Unknown/no eligible Shuffle, canceled candidate or failed save never spends. Rewind previews the exact last3/6 actions; Shuffle previews the actual verified candidate. View animations consume committed events, not determine wallet credit. Foreground/resume restores complete committed board/wallet, not partial motion.

Result presentation follows committed base50, then optional explicit ad+50. Cancel/fail/no-fill retains50 and Continue. Verified duplicate/late completion is handled idempotently even if the result view has closed; never credit from ad-close or animation callbacks. ECONOMY_SHOP defines readback/uncertain state and recovery. Functional text is permitted in current design exploration under UI_SPEC §10; production cutout/style selection remains pending.
