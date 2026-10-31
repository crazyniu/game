> CURRENT AUTHORITY: Use adopted B only: three branches at (-333,200),(333,-60),(-333,-320), single upper receiving nest (0,640). These are the internally selected layout baseline; historical A/user-choice notes below do not authorize five-branch production. The source old-bird diagrams remain review evidence only. New action sprites must pass internal QA before dependent View wiring.

# First-level event-to-motion handoff — DESIGN ONLY

2026-10-31 UTC. Scope: one complete first-level slice, not all40 or a global View rewrite. PROPOSED / NOT_ACCEPTED / NOT_IMPLEMENTED_UI. No production source, scene, material or asset is written by this handoff. P5 is the current documented board; P3 remains a proposal pending the user's decision. This file is not permission to choose P3 or start new UI before asset/layout acceptance.

## 1. Actual evidence and exact interfaces

Read on251 in /home/developer/Projects/game-t21 at HEAD61ca67529ea8c4846965551eb235ae62bd6d8333:
- PerchSort/Assets/Scripts/Core/FirstLevelV4.cs, namespace PerchSort.Core.FirstLevel,309 lines. This is an immutable bird-only slice; it neither loads a campaign nor migrates saves.
- State has FlockSize/SpeciesCount/Branches/HomedSpecies/MoveCount/Sequence/IsWon. Branch has stable Id, Side, Tokens and HomeSpecies; Retired derives from HomeSpecies>=0.
- MoveResult: Before, After, FromIndex, ToIndex, Count, Run, Events. MoveEvent: Kind, FromIndex, ToIndex, BranchIndex, Species, SourceStartSlot, TargetStartSlot, Tokens and TokenIds. Core emits only Moved, then optional Home, then Retired, in that order (lines237–266).
- Session.TryMove updates its in-memory State and pushes history; it does not durably save. Undo restores prior contents/move count but increments Sequence (lines294–300); Restart increments Sequence and clears history (303–306).
- Actual manifest pins one primary motion dependency, com.annulusgames.lit-motion, Git revision0b4c588ee75a07198841d92aab653e6b39445089. A pinned/imported dependency is not evidence that the new views consume it. Existing UI remains legacy; no first-level animation adapter was found by the focused read.
- Existing BoardView/BirdView are v2 and use a single fly pose plus their own timing. Do not cast this new State into List<int> or quietly route it through the old target/sleep/crate event consumer. Current BirdView.Update/SetPose also overwrites the Sprite every frame; a new wing clip cannot share Sprite ownership with that legacy loop.

The upstream scope tally (old changes/dependency/new files) is not a claim that new flight/UI is finished. This design only claims the interfaces and files actually read above; module test/runtime evidence belongs to its separate report.

## 2. Timing and asset authorities

Timing is owned only by docs/spec/INTERACTION_ANIMATION.md AN01..AN14. Use AN01/02 entrance, AN03 selection/cancel, AN04 illegal feedback, AN05 transfer/landing, AN06 reception, AN07 completion-retirement, AN10 restoration, AN11/12/13 controls/modal and AN14 victory. This handoff deliberately does not create a second seconds/amplitude table. Any proposed wing-cycle rate must be reviewed under that authority, not smuggled in as a rule.

Static layout is UI_SPEC§4.1/UI-D43 and the chosen reviewed first-level layout profile; motion library/API/license is ANIMATION_LIBRARIES plus the pinned-version evidence. The library owns clocks, easing, sequence/stagger and cancel handles. Actual wing silhouettes come from an accepted frame/rig clip, not body scaling. Do not assume LitMotion has DOTween's DOPath API. For a retained simple arc, an approved position evaluator may consume the library's normalized progress without becoming a second timing/sequence framework; a new spline requirement must follow the owning official-path/tool selection gate. Do not add a second tween library or hand-written spline/scheduler framework.

Common receiver candidate: the reviewed shared-image/mask contract, with repository-relative evidence in [T21 asset manifest](T21_asset_manifest.md). One unchanged nest mother is sampled once, with one shared-UV front-rim mask affecting only the arriving bird. Independent bilinear front/back RGBA sprites failed the300-size test and are not an import recipe. The repaired offline test passes native512 and target300, but its contour/style/Unity masking are still unaccepted. Raw static body in the diagnostic is not the required wing/landing clip. Temporary helper/contracts/GIF paths under /tmp are local diagnostic provenance only, never a prerequisite for another AI to read or implement this handoff. The final repository handoff must carry the contract text and formal relative evidence links; no implementation may depend on a temporary machine-local file.

## 2.1 Pose registration is not a standing contact line (explicit asset gate)

The two temporary diagnostics use different measurements of the new body and must not be mixed in production: the flap stability report's body/foot-color centroid is approximately(624.447,1027.075), a virtual flight registration reference; the nest diagnostic uses approximately(624.474,1078), a lower contact-foot choice. They are not interchangeable. These numbers describe temporary tests only and do not become a new hard asset specification.

An accepted pose manifest must provide source canvas/scale, a common registered actor reference and per-state/frame offsets, plus the actual standing/landing foot contact line. Tucked flight feet need not sit on a ground baseline. At the end of landing, actor-root placement plus the accepted final-pose contact offset must equal the real target slot surface; do not simply put a color centroid at the slot and claim feet contact. Entering the nest similarly maps the accepted approach/wing-fold contact pose to the accepted mouth anchor before the body crosses the shared rim. Avoid a one-frame jump when switching virtual flight registration to contact registration.

The old source(512,870)/pivot(0.5,154/1024) is only a legacy layout/old-bird reference. It must not be assigned to the new body or its flight frames. The current exact coordinate table and clearance calculations remain placeholders until the accepted per-state contact/virtual-flight manifest is used and actual rendered feet/clip envelopes are checked. No Unity foot-contact pass has been established by the two offline diagnostics.

## 3. Data binding: no implicit P3/P5 choice

Render a supplied approved initial State + one matching layout profile keyed by Branch.Id. Never infer a different board from the artwork, auto-remove empty branches, add birds to match a screenshot, or choose a profile just because it is shorter.

| Profile | Logical binding | Status |
| --- | --- | --- |
| P5 | N4,K3; S1=A3 B, S2=B3 C, S3=C3 A, S4/S5 empty. CreateDocumentedFirstLevel currently produces this with stable L01-Sn-ss IDs. | Current documented fixture, not automatic production selection |
| P3 | N4,K2; S1=A3, S2=B3 A, S3=B. A separately approved fixture must supply stable IDs before wiring. Current State constructor flexibility does not approve its product rule/content changes. | Proposal only; do not adopt silently |

Bird instances are keyed by Token.Id, never merely species or slot. Branch visual roots remain keyed by Branch.Id, including retired roots. The order in Branches/TokenIds is logical wall→exposed end; right-side geometry mirrors positions only.

Resolve a Moved event token j from Before.Branches[FromIndex] at SourceStartSlot+j to temporary target slot TargetStartSlot+j. Count=Tokens.Count, not Run; capacity-truncated leftovers are not flown. Home.TokenIds lists the complete N-token flock in slot order. Home.BranchIndex is its source branch; Home.ToIndex is -1 and must never be indexed as a receiver. The one receiver is a separately reviewed visual anchor, not a target queue field. Retired.Tokens is empty; BranchIndex identifies the segment to retire.

## 4. One playable action: exact event/visual sequence

The controller owns one stable action at a time. It validates/accepts the logical result; the presenter only consumes that result.

1. From Before, keep existing token views and current selected offsets. Cancel selection tweens without losing current transforms. Lock board/Undo/Restart/Hint/navigation actions before presentation; reject repeated requests, do not queue them.
2. Confirm the logical candidate according to the owner transaction policy. For a local unsaved preview, label it IN_MEMORY_PREVIEW; do not claim atomic save/SaveError/save6 is implemented. A production resume path requires the persistence owner's confirmed commit before successful motion starts. Session.TryMove alone is not that confirmation.
3. Create a small temporary **visual working board** from Before. Do not Sync(After) now: After already has an empty retired completion branch, which would prematurely destroy the very flock that must land and then fly home.
4. Consume Events in their existing order, no sorting by string/type. For Moved, move only TokenIds listed; apply accepted takeoff/wing clip plus a LitMotion-owned trajectory. Capture real selected foot positions for continuous start; derive destinations from the recorded target slots. Unmoved selected leftovers return to their original feet. Keep logical slice order: if taking off exposedmost-first, departure rank may reverse, but token j still lands at TargetStartSlot+j.
5. Finish AN05 with each actual foot reaching its recorded slot, fold wings and play accepted landing recovery. All Count birds must be landed before processing the next Home. This is expression of already calculated events, not a frame callback that recomputes completion.
6. If next event is Home, use its complete TokenIds, including birds that were already sitting on that branch before Moved. Animate all N through AN06 toward the shared receiver: real periodic wings→approach→accepted foot/wing-fold pose→cross the curved front rim→inside occlusion. No manual Dispatch, target eligibility wait, sleep, offscreen exit or instant deletion. Do not hide a wrong mask by fading a bird while it remains in front.
7. Once the whole Home group has safely left and been visually received, consume the matching Retired event: AN07 retracts/fades only that branch's playable segment. Keep its supported stump and fixed ID/location; ordinary source-emptying never retires a branch.
8. Only after the event chain: reconcile visuals against After by stable IDs, token counts, slots and branch retirement. Empty/inactive slots are not compacted. Release temporary event views and clear selection.
9. If After.IsWon, keep the board locked and begin victory after the final reception/retirement visual chain. Otherwise return to Idle or NoMoves by After; unlock only if the current lifecycle identity still matches. One final sync does not replay Home/Retire or award anything.

## 5. P5 concrete payload example (not an alternate rule)

The documented route first moves B from S1→S4. For that move, the only event is Moved with SourceStartSlot=3,TargetStartSlot=0,TokenIds=[L01-S1-04]. No receiver/retirement animation occurs.

The next action S3→S1 produces:
- Moved: FromIndex2,ToIndex0,SourceStartSlot3,TargetStartSlot3,Tokens=[L01-S3-04],Count1.
- Home: BranchIndex0,FromIndex0,ToIndex-1,SourceStartSlot0; complete token sequence [L01-S1-01,L01-S1-02,L01-S1-03,L01-S3-04].
- Retired: BranchIndex0,empty Tokens.
- After.S1 is already empty/retired. Therefore the view must preserve the three previously sitting A views and the incoming fourth view through Moved landing and Home reception. An immediate sync to After would make the group disappear before any of that presentation.

The last documented move S2→S4 carries three B tokens, appending them after L01-S1-04 already in S4; final slots are1,2,3 in their original relative ID order. Home then contains those four actual IDs. Nothing can substitute four cloned sprites of the right color.

P3, if approved, uses the same event handling. Its first S2→S1 action immediately produces Moved/Home/Retired. No presenter special-case may auto-play that move on level load or silently use P5 arrays.

## 6. Non-transaction feedback and entrance

| Trigger | Required behavior | Core effect |
| --- | --- | --- |
| Successful initial load/Restart | Bind supplied fixed State and profile; show environmental support before playable branches, exact initial birds at their own slots, one receiver. AN01→AN02 if entering from menu; Restart skips menu exit. No old target queue, hidden spare branch or rearranged IDs. All settle before board input. | No Transfer, no artificial Home |
| Select valid source | Highlight/lift only Rules.GetRun. If source changes or feedback is interrupted, cancel its previous feedback handle. | None |
| Same source/blank cancel | Cancel selection handle and restore the same token foot slots. | None |
| Valid target during selected feedback | Stop feedback at current render transforms; hand the accepted result to section4. | One accepted Transfer only |
| Invalid target/source | AN04, no MoveResult/no history/no move count. Invalid target cancels selection as RULES requires; no misleading fly. Empty/retired locations cannot become a move source. | None |
| Ordinary branch becomes empty | Keep its support and playable bar active, empty slots visible. | No Retired event inferred |
| Pause/settings | Only stable-state request opens normally; while presenting accept at most the one pending pause request specified by INTERACTION, not a queued move. Modal owns hit area and blocks board. | None |

The first-level tutorial should point at a genuinely legal source and target from the chosen fixture; it may illustrate selection, not execute a move. Do not introduce a new compulsory text/bubble layout; accepted UI-D34 position must stay clear of actual bodies/wing bounds.

## 7. Lifecycle, cancellation and once-only effects

Maintain a presenter generation per bind/reload/Restart/view teardown. A presentation batch identity is (generation, approved fixture identity, After.Sequence). An event identity adds its ordinal. Do not call GetStateKey a campaign contentHash; the current Core explicitly says it is a slice state key.

For every bind/start/finish callback, check current generation and batch identity. A finished old tween cannot unlock new input, create a nest glow, restore a retired branch or play victory on a new board. Keep all handles for the batch (trajectory, wing clip, landing, receiver, branch, particles). Cancel through the pinned library/native clip lifecycle; do not add a parallel stopwatch/scheduler. Releasing pooled objects must cancel their current rental, not rely only on GameObject destruction.

Track event started/completed/cancelled so click/wing/home/retirement/victory sounds and effects start at most once per event. A duplicate delivery of an already presented batch is ignored or reconciled, never replayed. Audio is subordinate to the one shared mute state. Animation completion/cancellation never changes Core MoveCount, HomedSpecies, branch state, save identity or rewards.

Normal Undo/Restart requests are accepted only in their permitted stable states; presenting requests are rejected rather than queued. At an accepted Undo/Restart, cancel old feedback/handles, increment generation, obtain the owner's new stable State, then synchronize via AN10 or AN02. Undo uses the actual restored IDs/retired flags, does not “reverse” an old animation into a new Core action, and does not re-emit Home. No-history Undo hides and leaves its slot; Won exposes Replay rather than inventing a normal Undo button. Restart uses the same chosen fixed initial fixture, clears selection and hint decoration, preserves global mute, never randomizes.

Background, destroyed scene or forced owner cancellation invalidates the generation first; cancel handles, then restore the owner's latest confirmed stable snapshot on resume. If a committed After exists, synchronize directly to After without replaying its Home/Retired sound chain. If commit was not confirmed, the owner decides old/candidate recovery per DATA_SAVE; the presenter cannot pretend “animation reached end” confirms a save. Remove all temporary birds/mask ownership and restore predictable input only once a valid current stable view exists.

## 8. Victory without invented features

AN14 follows the last received group and Retired visual. Present actual completed-level state and icon-only Replay/Home plus only a genuinely implemented legal Next route. A first-level slice must not quietly route into legacy v2 level2 or show a fake working button. No automatically lit stars, coins, cosmetics, target queue, extra branch action, advertisements or manually dispatched flock.

Controls open only when the main victory content is stable, not merely visible; particles may continue afterward. Victory entry, result sounds and navigation each occur once per current generation/Sequence. Restart/Replay cannot inherit old victory handles. Static trophy/character images alone are not the complete celebration; accepted motion/particles and runtime evidence remain required.

## 9. Layout: one reviewed diagram plus metadata owns each pending option

1080×1920 safe design root, origin center,Y up. All layout candidates remain **UNACCEPTED**. The single position authority is the matching pending-review diagram plus its metadata, linked by [T21 asset manifest](T21_asset_manifest.md); the table below only transcribes that evidence for quick reading and must not become another independently edited layout contract. If the diagram/metadata changes, regenerate this transcription rather than retain competing coordinates. P3 is still not adopted.

| Pending option / evidence | Branch root centers transcribed from the review | Status |
| --- | --- | --- |
| P5 current formal fixture / matching static review and metadata | S1(-333,210); S2(333,150); S3(-333,-90); S4(333,-150); S5(-333,-390) | UNACCEPTED composition; current documented logical board |
| P3 proposed fixture / actual static review B and matching metadata | S1(-333,200); S2(333,-60); S3(-333,-320) | UNACCEPTED composition and unadopted logical board |

Common receiver in those candidate reviews: root(0,640), full frame300². Its source visible vertical bounds predict522.266..757.773; these are geometric predictions, not rendered/device acceptance. Entry/mask/pose anchors remain the reviewed asset manifest's responsibility. No queued target nests. **Do not draw, assign precise coordinates to, or implement any unimplemented spare/additional branch in this slice.** Future R07 placement requires a separate layout review; it is not defined here.

N4 branch segment320×80; four slots80 apart. Left slot centers are rootX+[-120,-40,40,120],right rootX+[120,40,-40,-120]. With the current image/renderer offset only, footY=rootY−2.8125. Use each approved new bird's real anchor/bounds, not an assumed legacy pivot or alpha-bottom. Existing136-square canvases centered horizontally predict outer edges±521,19px inside±540; non-centered/new wing anchors must be recomputed.

Each active outer segment end aroundx±493 needs a visible connector into the same-side trunk/credible rope aroundx±520. Keep root/stump behind the full segment and outside landing slots. The common nest also needs its own believable cradle/attachment, not a new playable branch. Render the support with the scene, not detached bars on the old soft forest. The P5 left/right stagger and the P3 review B's different vertical arrangement are each defined by their matching diagram/metadata; do not impose a second mandatory row grid or transplant one option's positions into the other. Do not bake birds, counts or hidden spare branches into support artwork.

Predicted P5 highest selected standing body is about340 using136 canvas/current proportions, below UI-D34 bubble bottom355; that small geometric gap is not visual acceptance for a new crest/wing clip. Nest artwork predicts about37px clear above bubble and42px below level-sign bottom800. Verify actual alpha bounds, roots, HUD, hand, safe area, true clip envelope and hit targets in the eventual accepted composition. If they fail, request layout review; do not shrink birds or alter branch count to force a pass.

## 10. Focused implementation acceptance after all gates

Before production UI: board/profile explicitly selected; species family and actual flight/landing clip accepted; common receiver contour/Unity-native/shared-UV masking accepted; supported root/stump and rounded-square button family accepted; approved layout IDs/anchors and touch/safe-area review completed. This document does not clear those gates.

Once authorized, keep checks narrow and useful:
- replay the fixture's actual MoveResult and prove exactly its IDs move/land/home/retire in order, including the P5 Move2 complete-flock example;
- no animation of Run-count leftovers, no early SyncAfter deletion, no target queue/Dispatch, no retirement on ordinary emptying;
- real wing geometry cycle and real feet/slot landing in timed recording; receive behind rim without fade-to-hide; original nest unchanged/no source seam;
- one input action at a time, duplicate delivery, current-generation lock release, pause/Restart/Undo/reload during allowed states, background cancellation and pooled-view reuse;
- snapshot alignment after each complete batch, Undo and Restart, with no duplicate sound/victory;
- true full first-level start-to-win runtime recording and actual pointer control checks. Offline mask GIFs, package import, old18-test counts, or pure Core snapshots do not prove this runtime sequence.

Unverified UI, assets, durable save, physical device and animation cases remain explicitly NOT_RUN. No broad architecture, extra mechanic, new currency/score system, other worlds or all40 implementation is proposed here.

