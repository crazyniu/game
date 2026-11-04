# T52I — Motion acceptance delivery report

## Baseline and coordinator evidence

Task branch `mac-current-policy/t52` was clean; both HEAD and local `t52-current-game` were `d930a6f6616d50d882c4e343d50f0d32fd6529ea`. No merge or primary-checkout edit was performed. Main remains outside this worker's integration authority.

Coordinator Run 4 passed compilation and 304 EditMode cases; PlayMode passed 12/17. Assigned motion failures:

- `AnimatedTransferLocksAirborneInputAndUndoCancelsCallbacks`, `AssertSnapshot:90`: Bird 3 root-to-slot distance 2.13076782 after Undo plus 1.4 seconds, expected `< 0.1`.
- `TimedFlockArrivalsReachTheirGoalDoor:389`: closest sampled distance 45.1711273, expected `< 40`.

These are user/coordinator observations, not locally reproduced runs. Other failures are outside this report.

## Minimal runtime fix

Only `PerchSort/Assets/Scripts/Game/BirdView.cs` is modified, plus `docs/work-log/motion-acceptance/spec.md` and this report. BoardView, BranchView, BirdhouseView, GameMotion and BirdIdleGroup were inspected without editing. No test, gameplay, layout, asset, scene, root or Sapling changes.

### 1. Keep the perch anchor separate from shuffle visuals

Original `Idle` wrote `perch + idleOffset * idleEnvelope` to the root rect. Shuffle chooses 2–4 pixels and can start during the test's 1.4-second post-Undo wait. The reported 2.13076782 distance is within that deliberate visual displacement range. **Inference:** the exact action/envelope at failure was not captured; this is not evidence of an uncancelled flight callback by itself.

The root is also consumed as a logical placement coordinate: BoardView uses `Place(bird.Position)` in presentation setup, hatch/phase handling and uses it for effects. Moving the root during an idle therefore mixes placement state with purely visual motion.

The fix caches `bodyPerch` at configuration, applies the unchanged shuffle translation to the existing `Bird body` child, and resets that body position on idle stop/cancel. For a perched, unit-scale root, moving the parent by delta or its entire body subtree by the same delta produces the same visible body/overlays/Zzz position. The root now remains at its branch slot. No scheduler weights, start windows, branch leases, 2–4 pixel range, envelope, frame choices, breathing, selection, flight or callbacks were removed or slowed.

Crucially, `Position` does **not** return cached `perch`. It reads the actual root transform, so the existing `< 0.1` assertion can still detect an erroneous stale flight/selection callback moving the root after Undo. The correction does not hide that class of cancellation defect.

### 2. Keep Position in design coordinates after doorway reparenting

`BirdhouseView.MaskEntry` reparents the bird under the doorway. `FlyCore` already updates the masked bird's world position using the original full-canvas flight parent. However, the original `Position` getter kept reading immediate-parent `anchoredPosition`, switching to doorway-local coordinates at precisely that point. The flock test compared this value with the design-space `DoorPoint`.

The fix caches the original position-space RectTransform and, only while reparented, converts the **actual root pivot** back into that original top-left design space. The normal-parent path retains direct anchored-position access. This adds no heap allocation or per-frame conversion to normal perched birds and does not alter flight transforms.

Source-derived two-house aperture radius is 45.105527638 pixels. The reported 45.1711273 closest sample is 0.065599662 pixels outside it. **Inference:** the observation is consistent with the last useful design-space reading just before mask entry, followed by incomparable doorway-local samples. No Unity trace was collected by this worker.

The home-flight centering correction at the start of `FlyCore`, moving door target, tangent tilt, 0.45-second flight, 0.1-second flock staggering, mask radius, shrink, final rotation reset and arrival-before-deactivation order remain unchanged. Existing generation guards and Undo reconstruction remain unchanged.

## Position, visual center and frame sampling

The two reported assertions need **no threshold relaxation or removal**. The root contract is now stable at rest and across parents. No GameplayTests edits were made.

A root pivot and the rendered pose center are different measurements. Home flight centers the body on the root before shrinking; tangent rotation can still shift the image center during flight, and final rotation is reset before `arrived`. The existing endpoint probe already computes the transformed Image center, but calls generic `Fly(..., home: true)` rather than the actual masked `FlyHome` path.

Frame sampling has a separate limit: `TimedFlockArrivalsReachTheirGoalDoor` samples once per test-coroutine frame. The terminal motion update can reach the door and remove the bird in the same callback before that coroutine samples again. This patch deliberately does not insert a production delay/dwell or change callback order to accommodate an observer.

### Precise optional test strengthening for the coordinator/test owner

Do not apply this in the runtime worker; coordinate separately with the worker owning GameplayTests.

1. In the timed flock case, replace only the **coordinate measurement** in the existing `< 40` path with the actual pose center transformed to the same full-canvas space. A helper can be:

```csharp
private Vector2 VisualBirdPoint(BirdView bird)
{
    var space = (RectTransform)root.Hud.HomeFlightLayer;
    var pose = (RectTransform)bird.transform.Find("Bird body/Pose");
    var local = space.InverseTransformPoint(pose.TransformPoint(pose.rect.center));
    return new Vector2(local.x - space.rect.xMin, space.rect.yMax - local.y);
}
```

Use `Vector2.Distance(VisualBirdPoint(bird), door)` in the existing minimum; retain the threshold, completion and arrival assertions. This measures the visible flock rather than a tilted body's root proxy. It does not by itself eliminate low-frame-rate endpoint sampling gaps.

2. In `HomeFlightShrinksAroundTheGoalDoorNotItsFootPivot`, replace the existing `view.Fly(door, 0, true, ...)` invocation with `view.FlyHome(root.Hud.Houses["sky"], 0, ...)`, keeping its callback's transformed-pose-center capture and `< 0.1` assertion. The existing Instant-mode setup permits deterministic endpoint observation through the actual reparent/mask path before deactivation. Keep the timed end-to-end case; this endpoint observation complements it rather than pretending frame sampling guarantees the terminal frame.

If the corrected timed case still misses `< 40` under very low frame rates, inspect actual frame timestamps before deciding a further test-only sampling contract; do not invent a wider tolerance or freeze production animation. This report does not claim such a residual failure has occurred.

## Verification performed and limits

- Read the two failing tests, existing endpoint visual-center probe, owned views/scheduler, and relevant read-only root Undo, layout and UI-coordinate construction paths.
- Language-server status returned no configured servers. Used focused source references to review `BirdView.Position` consumers; no signature or caller migration was introduced.
- Ran a throwaway arithmetic check: 36 density/offset/envelope combinations preserved the exact visible body origin when moving the same translation from a unit-scale root into the body; 27 scale/rotation/point combinations checked the original-space coordinate arithmetic. Calculated the authored entry radius and observed-distance gap above. These are numerical identities, **not execution of Unity Transform, LitMotion, or the changed C# code**.
- Ran `python3 ci/check_current_game_names.py`: exit `0`, `567 tracked text files inspected; 0 violation(s)`.

**NOT_RUN locally:** Unity, C# compilation, EditMode/PlayMode suites, actual timed idle/flight/Undo interactions, screenshots or GUI verification. No passing runtime result is claimed. The coordinator is the sole Unity runner and must integrate/review, rerun the unchanged motion assertions and required suite, and verify approved visible idle/shuffle, home-door clipping/center, cancellation and input on the actual surface.

## Submission

Commit/push this bounded source fix and the two English task documents on `mac-current-policy/t52`; send final SHA to the current Codex coordinator through the verified route. No main merge, invented Task/Dispatch IDs, new workers or `worker_done`.
