# T50A delivery report — bird and board motion

## Delivery

- Status: **DONE — bounded implementation delivered for coordinator integration and later user testing**.
- Branch: `mac-motion/t50`; preserved WIP base: `9f84a95`.
- Implementation SHA: `39f36f5` (`Implement T50A bird idle and masked flock motion`). The subsequent report/status commit is identified in the final DONE notification.
- Coordinator: the current Codex conversation on 251, `Continue Level 1 Gameplay & UI Redesign`; no retired Claude session contacted, no Dispatch ID invented, no workers started.
- No merge to main, deployment or release.

## Changed files

Runtime, under `PerchSort/Assets/Scripts/Game/`:

1. `BirdView.cs`
2. `BoardView.cs`
3. `BranchView.cs`
4. `BirdhouseView.cs`
5. `V7BirdIdleGroup.cs` (new internal helper)
6. `V7BirdIdleGroup.cs.meta` (new stable GUID; no Unity import run)

Documents:

7. `tasks/T50A_bird_motion.md`
8. `tasks/T50A_report.md`
9. `docs/team/status/mac-motion.md`

No Core, level, economy, artwork, shared layout/art/UI utilities, root/intro, popup, map or T51 files changed. Existing script metadata unchanged. No new library.

## Implemented requirements

### Birds

- Independent weighted scheduler per perched bird. Initial delay 0–3 s; subsequent start interval 1.5–5 s. A denied branch lease retries after 0.1–0.3 s; therefore the concurrency rules can deliberately delay a start beyond the nominal interval.
- No immediate repeat. Adjacent birds cannot start the same idle action within 0.6 s. At most three idle actors per branch. Seat groups rebind on transfer, chick follow, swivel, compaction, rebuild and settlement.
- Selected, flying, sleeping, egg and committed-action actors excluded from random idle. Per-bird breathing period 1.4–2 s, scale 1.00–1.03, randomized phase; sleepers retain breathing and drifting Zzz.
- T49 blink (including occasional double blink), tilt, look, preen, fluff, stretch, yawn and two-frame signature poses. Weighted shuffle moves 2–4 design pixels and returns. Pose envelope eases in/out around a 0.3–1.2 s hold. Missing optional pose frames warn once and use procedural tilt/turn/shiver/stretch or shuffle; original V5Art fallback remains authoritative.
- Preserve attention, selection hop and hover, takeoff crouch, staggered transfer arcs, true `fly_up`/`fly_down` swaps every 0.07 s, tangent-based body tilt and landing squash. Reusable soft sparkle trail is hidden on arrival/cancellation.
- Preserve egg wobble/reveal/crack/hatch, wake startle/stretch, mama/chick leapfrog, crow two-hop transfer/flee, illegal shake and cheer. Cheer and event completion restore perched pose. Reveal opacity and Zzz offsets restore on cancellation.

### Branches and committed event presentation

- Separate landing spring channel starts at a 3-degree tip flex and settles; leaf fall remains independently cancellable.
- Branch growth/twig upgrade retain committed seat changes, sprout and seat glow. Swivel adds branch compression/knot spin alongside the existing bird reversal hops, with one completion barrier.
- Existing event order, wave beats, gift targets, Chorus increment and Core snapshot settlement remain intact. No animation awards coins, changes inventory or owns input.
- Board sequence generations suppress continuations from canceled HUD/gift/phase callbacks. The Sapling-only visual rebuild preserves its live presentation generation rather than invalidating its own continuation.
- Airborne accounting remains in BoardView/root; landing cannot underflow it. Intro entry callbacks are generation-guarded. `Board.Enter(...)` and `Branch.Entrance(float)` are unchanged.

### Birdhouses / flock home

- Open the matching house door before cheer/takeoff. Birds depart 0.1 s apart, flap/shrink toward a moving door target, and reparent into the actual circular Unity UI doorway mask only near the aperture. Coordinates continue in the original full-canvas flight space after reparenting.
- Each arrival punches the house and emits sparkles. After the final arrival, close the door, restore the approved visible species portrait and pop the green check before advancing the committed event sequence.
- The T48 padded door leaf uses its measured visible hinge/bounds through transform sizing/pivot, without making replacement art or cropped sprite allocations. Open-door body/colored roof remain separate supplied parts. Resting houses retain the approved visible portrait, not a permanently closed wooden disk.
- Missing door parts log once and retain whole-house scale / existing doorway fallbacks.

## Public API contract / minimal integration hooks

All pre-existing public signatures remain unchanged. New presentation-only signatures:

```csharp
BirdView.FlyHome(BirdhouseView house, float delay,
    Action airborne, Action landed, Action arrived)
BranchView.Swivel(Action done)
BirdhouseView.OpenDoor(Action done)
BirdhouseView.CloseDoor(Action done)
BirdhouseView.Peek(bool visible, Action done)
BirdhouseView.CompleteFlock(Action done)
```

`BoardView` already integrates `FlyHome`, `Swivel`, `OpenDoor` and `CompleteFlock`; root requires no new call to obtain flock choreography. `V7BirdIdleGroup` and `BirdhouseView.MaskEntry/EntryRadius` are internal, not root contracts.

For the root/popup owner only: optional goal-info choreography is `OpenDoor` → `Peek(true, ...)`; closing is `Peek(false, ...)` → `CloseDoor(...)`, then restore normal house presentation with `Cancel()`/the existing HUD state sync. Keep the existing `OpenGoalInfo` `IsBusy` gate; do not overlap a goal-info visit with a flock sequence. Cancel the house on interrupted dismissal/rebuild. This module does not open/close the info card or own its modal/input state.

Art integration remains mac-ui-owned. Existing `V5Art` and world branch sprite selection were not changed; no assumed future asset path was added. Intro/root and popup integrations outside these owned files were intentionally not edited.

## Cancellation / exactly-once contract

- Live successful BirdView callbacks are guarded one-shots tied to a presentation generation. Cancellation terminates the operation without invoking its stale success continuation; root Undo/Restart continues to own reconstruction and transaction lifetime.
- Disable/destroy/rebuild cancel all owned motion channels and random scheduling. Bird pose, scale, rotation, resting position, reveal alpha and sleep bubble restore. Branch flex/scale/knot/glow restore, transient leaves/sprouts are removed. House door, portrait, scale/check restore, transient sparkles are removed.
- Board cancellation invalidates its sequence first, cancels the active house and reparents in-flight/masked birds back to the board layer before restoring them. Rebuild disposes them from there; no detached masked actors remain owned only by a house.
- Final home/flee callback executes before deactivation: deactivation itself cancels the bird generation and must not suppress the final successful arrival.

## Static evidence and limits

Performed, without executing game logic:

- Read T50, shared memory, v5 design §5, approved K3/K7 level references, existing V5Motion/V5Art/V5UI patterns, owning views, root gates, HUD/intro consumers and game assembly references.
- Language-server status: **no language servers configured**. No compiler/Unity compilation claim is made.
- Static resource inventory: required base/flight frame resources exist through V7 or existing V6/V5 fallback. V7 currently lacks 48 required frames across cocoa/crow/ember/hoot/ink/snow, and 54 optional extra idle poses; preserved/procedural fallbacks cover those pending art integration.
- Compared decoded RGBA pixels of each available species' resolved `fly_up`/`fly_down`: no identical pair across all 10 species. This proves distinct source frames, **not animated playback**.
- Confirmed house body/open/leaf/check/sparkle resources and leaf-fall resources for worlds 1–4 exist. Measured supplied leaf alpha bounds `(145, 198, 361, 414)` in its 512×512 source. No art file modified.
- Static lifecycle/API review covered intro skip, canceled flight/door/gift continuation, hatch/wake resting state, group lease release, masked-parent restoration, Sapling rebuild, crow compaction, swivel completion and final-arrival callback/deactivation order.

Explicitly **NOT_RUN**, as required by the latest user instruction:

- Unity Editor/batch import/compile or runtime launch.
- EditMode/PlayMode/test suites or all-level playthrough.
- Flight/idle/flock/intro timed capture sequences, runtime screenshots and visual/input/leak checks.

## Missing / deferred

No intentionally missing production implementation inside the bounded T50A ownership. Final V7 art for fallback species remains pending external art integration. Runtime correctness, actual clipping/hinge appearance at K1/K3/K7 densities, timing/tuning, no-leak/input behavior and Unity import/compile remain **unverified**. They must not be presented as accepted animation evidence. Coordinator reviews/integrates, then pauses this stage for user testing; later development moves fully to Mac.

## Coordinator communication

Sent status from this module's existing Orca shell terminal to the observed current Codex session mailbox, using no Dispatch ID. Receipt: `msg_ee5e13a083b4`. Final SHA and report notification follows after the task-branch push.
