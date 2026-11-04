# T50C — Map motion, world ambient and leaf transition delivery

## Status and authority

**DONE — bounded module implementation; root transition/origin wiring belongs to mac-ui.** Submission SHA is recorded in the terminal/final delivery block; the companion status file records the branch and integration contract. Current coordinator is the current Codex conversation on 251, not the retired Claude session. No sub-workers or Dispatch IDs were created. Branch: `mac-map-motion/t50`, based on preserved WIP `9f84a95`.

The latest user instruction overrides historical tests-before-push requirements. Unity execution, compiler execution, EditMode/PlayMode suites, all-level playthrough, captures and timed runtime evidence are **NOT_RUN**. No historical green result is evidence for this delivery. Static source/API/asset review is not runtime acceptance.

Requirements: [T50C specification](T50C_map_motion.md). Design references: [T50 Phase 2](T50_ui_v7_code.md), [v5 animation/world contract](../docs/GAME_DESIGN_V5.md), [team memory](../docs/team/MEMORY.md), approved `art/approved_ui/ui_map.png`, `ui_level_k3_v2.png` and `ui_popup_world_unlock.png`.

## Changed files

Under `PerchSort/Assets/Scripts/V5/Game/`:

- `V6MapView.cs`: inertia tuning, clamped offset enforcement, interruptible opening focus, current-node/tap binding, gate scroll orchestration and lifecycle/reward cancellation.
- `V5SceneView.cs`: integrates shared ambient driver, preserves intro entrance API, settles canceled lighting at its committed phase and suppresses obsolete phase continuations.
- New `V7MapMotion.cs` + `.meta`: one current-node hop/shadow/Sunny idle driver and independent tap feedback channel.
- New `V7Ambient.cs` + `.meta`: bounded reusable world ambient pool and canopy driver.
- New `V7LeafTransition.cs` + `.meta`: isolated leaf curtain presentation, 0.4-second duration.

Documentation: `tasks/T50C_map_motion.md`, this report and `docs/team/status/mac-map-motion.md`.

No Core/Meta, level data, economy, shared layout/art/utilities, root, gate class, tests, scenes, settings, package files, original artwork or T51 files were edited. Existing owning gameplay specifications and tests are intentionally unchanged: presentation does not change their contracts.

## Implemented behavior

### Map

- Existing `ScrollRect` remains inertial, vertical and clamped; deceleration rate is explicitly 0.135. The existing design offset range is enforced at both ends and outward velocity is stopped at an edge.
- Opening focus uses a 0.5-second smoothstep from 140 design pixels away, choosing the available direction at the upper boundary. Drag/wheel cancels only ordinary focus, stops existing velocity and yields to direct input.
- Current-node selection uses the existing `V6MapModel.CurrentLevel(profile)`, including all-cleared/test-mode profiles. This affects only the presentation marker; locked/cleared eligibility and star records still use the unchanged model. Current node hops with its original layout amplitude/period; its separate shadow shrinks/fades with height. Sunny breathes, blinks and swaps both supplied quirk frames. The 72-second driver closes cleanly over the hop/breath/idle periods.
- Enabled-node clicks receive a separate 0.24-second holder pop, preserving the existing V5Button press/release channel and level-card callback.
- Gate presentation first snaps only the map-owned gate instance closed, scrolls to the world boundary for 0.5 seconds, then invokes existing `V7GateView.Open(Action)`. The current gate implementation supplies the 0.8-second padlock/swing animation. Scroll is temporarily disabled during that sequence and restored before the completion callback. Ordinary scroll requests cannot interrupt the gate transaction.
- Superseding/canceling a gate drops its continuation and cancels the gate-scroll channel. Because the gate API has no cancellation/snap method, explicit map Hide replaces only the interrupted instance using its stored committed open/closed state. Rebuild/destruction instead deactivates the old instance and relies on its existing OnDisable cancellation; no objects are created in teardown hooks. The next Show rebuilds from the profile.
- Focus, gate and coin callbacks have separate invalidation counters. Successful gate/coin continuations are detached before invocation. Canceled operations do not invoke stale root/navigation continuations. Canceled coin flights restore the already-committed displayed destination amount; no reward/economy mutation was added.

### World ambient

- All worlds: three subtle drifting cloud images and separate low-amplitude crown sway (0.65 degrees, 0.2% scale); trunk/branch/board geometry is unchanged.
- W1: twelve seasonal particles using delivered `leaf_fall_w1`, not legacy `petal`; rain variant retains rain alongside the seasonal particles.
- W2: twelve bamboo leaves and two diffuse mist layers; drizzle retains rain.
- W3: twelve maple leaves; windy variant uses the shorter falling period.
- W4: twelve supplied snowflakes (`leaf_fall_w4`); aurora variant adds three subtly moving/tinted glow bands over its authored background. These reuse existing `glow_soft`; no new aurora artwork or material was generated.
- Six fireflies appear for lanterns/sunset/bluehour/aurora (and the explicit dusk identifier) or the current night phase. Day-phase changes update the existing pool rather than recreate it.
- One V5Motion loop drives the ambient state. Capacity is 40 slots, maximum configured count 39 in W2 drizzle; existing Images are reused across world/level changes and are non-raycast targets. No per-frame object/collection creation; no per-particle MonoBehaviour/tween, persistent scene objects, DontDestroyOnLoad or generated textures/materials. Cancel hides the pool and restores crown poses; rebuilding resets phase offsets.
- All four seasonal particle sprites use delivered v7 `leaf_fall_w1` through `leaf_fall_w4`. Only `raindrop` retains the existing V5Art legacy lookup because no delivered replacement exists; no second art lookup convention was introduced.

### Leaf wipe

- Six mirrored/sliding leaf clusters reuse `leaf_wipe`; a green backing becomes opaque only near full closure to guarantee the covered boundary despite sprite transparency. A full-parent transparent raycast surface prevents pointer-through during the wipe.
- Total duration is 0.4 seconds. `covered` executes once at the midpoint, after setting the fully covered pose; `completed` executes once after clearing the curtain. With `V5Motion.Instant`, both boundaries execute synchronously in that order.
- Reentrant Cancel/Play from the covered callback invalidates the old run. Callback references are cleared before invocation; a covered callback exception cancels the curtain before propagation. Cancel, disable and destroy discard old callbacks, restore the hidden pose and clear IsPlaying.
- Eight child UI objects are created once per configured transition owner and reused; none survive owner destruction. This component performs no navigation, save, progression or intro actions itself.

## Exact public APIs and root integration notes

### Preserved existing signatures

`V6MapView`: `Configure(V5GameRoot, V5Art, TMP_FontAsset)`, `Show(V5Profile)`, `ScrollToLevel(int level, bool animated = true)`, `UnlockGate(int world, Action done)`, `CollectCoins(int from, int to, Action done)`, `Hide()`, `RefreshCoins(int value)`, plus existing `Nodes`, `FocusLevel`, `Scroller` properties.

`V5SceneView`: `Configure(V5Art, Action)`, `FillScreen(Transform)`, `PlaceLighting(Transform)`, `SetLevel(BoardState)`, `SetWorld(int world, string variant = "noon", DayPhase lighting = DayPhase.Day)`, **`Entrance(float alpha)`**, `ChangePhase(DayPhase value, Action complete)`, `Cancel()`.

No existing caller migration is required for map/scene changes. Existing `PumpNotifications` → `Map.UnlockGate(request.World, callback)` automatically uses the new sequencing. The gate owner need not change its public API.

### Added root-facing presentation API

Namespace: `PerchSort.V5.Game`.

```csharp
public sealed class V7LeafTransition : MonoBehaviour
public const float Duration = 0.4f;
public bool IsPlaying { get; private set; }
public void Configure(V5Art art);
public void Play(Action covered, Action completed);
public void Cancel();
```

Configure once, then reuse. Play requires the owner to be active/configured. Calling Play supersedes any previous run. Success invokes each supplied callback exactly once; cancellation deliberately invokes neither remaining callback, so obsolete loads/unlocks cannot execute.

### Approved level-card origin contract

```csharp
public bool TryGetLevelCardOrigin(int id, out Vector2 point);
```

The query transforms the actual node-face RectTransform's `rect.center` to world space, then inversely through the owning 1080×1920 design RectTransform. Subtracting that design rect's left edge and measuring down from its top edge produces top-left design coordinates. This includes content scroll, current-node bounce, holder/button scale and all intervening safe-area/canvas transforms. It never returns raw `V6MapModel.Position` coordinates and does not force a canvas rebuild or allocate.

It returns false if the map/node is inactive or unavailable, its required RectTransforms/viewport are unavailable, or the node center is outside the clipped map viewport (including a partially visible node whose center is clipped). On false the out value is always the central fallback `(540, 960)`; it never retains a prior node origin. Eligibility/locking remains the root/model's responsibility.

In the existing `V5GameRoot.OpenLevelCard(int id)`, after its navigation/modal validation and immediately before `panels.LevelCard(id)`, call `Map.TryGetLevelCardOrigin(id, out var origin)` then popup-owned `panels.SetLevelCardOrigin(origin)`. Passing the out value on every request also supplies the explicit center fallback when the bool is false. Keep the original map click listener's `root.OpenLevelCard(id)` call. The popup setter consumes this origin for the next level card; neither module edits root. Root must integrate this query in addition to the separate leaf-wipe wiring below.

**Root owner call sites (not edited here):**

1. In `V5GameRoot.Initialize`, after the existing information/intro layers have been created, create one dedicated full-canvas RectTransform child of `Canvas.transform`, stretch it to all canvas edges, add `V7LeafTransition`, and call `Configure(art)`. Do not put it under Map, Board, Scene or a modal CanvasGroup; these are hidden/rebuilt/disabled independently. Play moves this layer to the last sibling.
2. At accepted **map → level** navigation (the `LoadLevel` path reaching `LoadDefinition` while `IsMap`), validate navigation first, then invoke `Play`. Run the existing level load/rebuild only in `covered`. Keep intro behavior and `Scene.Entrance(float)` as-is; `completed` releases only the navigation/wipe lock, not the intro's independent lock.
3. At accepted **level → map** navigation (`OpenMap` / the `ShowMapSurface` path), run existing map-surface setup in `covered`; resume notification pumping after `completed` so popups do not open under the curtain. Include the notification-driven finale `ShowMapSurface` route when it actually crosses surfaces. Do not apply this wipe to in-level Undo/Restart or popup opening.
4. Include `leafTransition.IsPlaying` in the root's navigation/input gate and reject repeat navigation until completion. The curtain blocks pointer input; programmatic/keyboard navigation still needs the root gate. Validate before starting Play rather than letting that gate reject the covered load itself.
5. Call `Cancel()` for external cancellation, a superseding navigation and root disable/destroy. **Do not unconditionally cancel the wipe from the `CancelPresentation()` executed inside its own covered `LoadDefinition` callback.** Separate external navigation cancellation from ordinary scene-presentation cancellation, or guard that call while applying the covered surface switch. The existing root `revision` changes during `LoadDefinition`; do not invalidate the matching wipe completion solely because its own covered load incremented that revision. Use the navigation owner's token/guard consistently.
6. External cancellation is not success: the root must clear its own navigation lock and restore/rebuild the committed surface in that cancel path. Do not rely on `completed` after Cancel. This matches the existing map/scene convention that canceled continuations are discarded.

### Module-internal helper APIs

These are public only to bind presentation components; root does not need to call them:

- `V7MapMotion.Configure()`, `Current(RectTransform holder, Image nodeShadow, Image sunny, V5Art art)`, `Tap(RectTransform holder)`, `Cancel()`.
- `V7Ambient.Configure(V5Art assets, RectTransform skyLayer, RectTransform particleLayer)`, `SetWorld(int world, string variant, bool isNight, RectTransform left, RectTransform right)`, `SetNight(bool value)`, `Cancel()`.

## Static review and untested limits

- Reviewed the current V5Motion/LitMotion wrapper, gate Configure/Open implementation, V5Art lookup, V5UI coordinates, V5Button scale channel, V6MapModel, V7Layout constants, root map/load/notification/Undo/Restart call sites and existing `V6MapTests` contracts. Existing model tests need no change; they are not run.
- All new helpers share the existing `PerchSort.V5.Game` namespace/assembly; the assembly already references Unity UI and LitMotion. No assembly/package/reference change is required. Language server inspection reported no C# language server configured; no LSP diagnostic or compilation success is claimed.
- Syntax-aware patch tooling reported no remaining parse warning after correcting an intermediate callback-brace insertion. Static review covers nullable delegate invocation, array bounds, right-crown mirroring, cancellation generations, callback detachment and teardown allocation boundaries. It cannot prove Unity lifecycle timing or rendering.
- All 15 required sprite paths were found. Each of the three new metadata GUIDs occurs exactly once under Assets. All nine local Markdown links in the owned specification/report/status resolve. Metadata uses the repository's existing minimal `.meta` format; Unity importer execution is NOT_RUN.
- Required sprite inventory (15): `V7/Birds/sunny_{idle,blink,quirk1,quirk2}`, `V7/Art/cloud_soft_{1,2,3}`, `V7/Art/leaf_fall_w{1,2,3,4}`, `V7/Art/{firefly,glow_soft,leaf_wipe}`, and legacy `V5/Art/raindrop` (all `.png`). The W1 inventory replaces `V5/Art/petal` with `V7/Art/leaf_fall_w1`.
- Approved reference images and the actual supplied leaf-wipe/cloud/snow sprites were opened. This is asset/style inspection, not an in-game screenshot or motion proof.
- **NOT_RUN:** Unity compile/import, EditMode, PlayMode, actual drag inertia, gate cancellation/reentry, phase cancellation, repeated world changes, memory/profiler sampling, device/safe-area coverage, transformed node-origin runtime conversion, real 0.4-second timing, runtime frame swaps and timed/visual acceptance. No test suite or all-level playthrough was started.
- Remaining integration: root must wire the isolated leaf-wipe API and the approved level-card-origin query/setter as described. These are intentionally outside this module's file ownership, not fake navigation fallbacks. No other implementation requirement is deferred within owned paths.

## Coordinator review correction

Review of `3542ced`, recorded in orchestration reply `msg_d94a9fefbf74`, required the bounded W1 resource correction. The follow-up changes only seasonal sprite selection to `"leaf_fall_w" + world` and updates this inventory/specification/status. Existing raindrop fallback, timing, object counts, cancellation, map/scene/transition APIs and root integration contract remain unchanged. Delivered W1 PNG/metadata existence was checked statically. Unity execution, compilation, tests and runtime evidence remain **NOT_RUN**. The follow-up SHA is reported through the current Codex mailbox and final terminal block.
