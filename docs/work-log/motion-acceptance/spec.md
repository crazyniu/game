# T52I — Motion acceptance coordinate triage

## Scope

Authorized runtime owners are BirdView, BoardView, BranchView, BirdhouseView, GameMotion and BirdIdleGroup under `PerchSort/Assets/Scripts/Game`, only as necessary, plus this specification and `docs/work-log/motion-acceptance/report.md`. The minimal fix changes **BirdView.cs only**. Other listed views were inspected read-only. Do not edit GameplayTests: another worker owns its Sapling methods and the coordinator integrates test changes separately. Do not merge main or alter gameplay, authored layout, original art, thresholds or assertions.

The current Codex coordinator alone runs Unity and performs runtime acceptance. This worker may inspect source, run the static naming guard and perform disclosed arithmetic checks, not launch Unity or claim runtime success.

## Coordinator failure evidence

Run 4: compilation and 304 EditMode cases passed; PlayMode passed 12/17. Two motion failures assigned here:

1. `AnimatedTransferLocksAirborneInputAndUndoCancelsCallbacks`: after Undo and 1.4 seconds, `AssertSnapshot` line 90 found Bird 3's `Position` 2.13076782 pixels from its branch slot; expected distance remains `< 0.1`.
2. `TimedFlockArrivalsReachTheirGoalDoor`: closest sampled distance was 45.1711273; expected remains `< 40` at line 389.

These are coordinator observations, not a worker reproduction. Do not rerun them to confirm. Other failed cases are outside this task's diagnosis.

## Diagnosed contracts and correction

### Perch anchor versus idle visuals

`Position` is consumed both for branch-slot snapshot checks and as a placement/effect coordinate by BoardView. The original idle shuffle wrote its 2–4 design-pixel translation directly to the root rect. That animated a logical placement anchor and let presentation code observe/reuse transient idle displacement as an anchor.

Cache the configured body rest position. Apply the **same** shuffle offset/envelope to `Bird body`, leaving the root rect anchored at the perch. Restore the cached body rest position on idle cancellation and general cancellation. Preserve the weighted scheduler, delays, branch leases, frame selection, 2–4 pixel range, envelope, breathing, attention, selection, flight and squash. Do not freeze idles or return cached `perch` instead of the actual root position: a stale flight callback must remain observable by the existing assertion.

### Masked flight coordinate space

`BirdhouseView.MaskEntry` reparents the bird into the doorway mask. Flight updates already continue in the original full-canvas coordinate space, but `Position` originally returned immediate-parent `anchoredPosition`, so its meaning changed at mask entry.

Cache the original position-space RectTransform during configuration. Preserve the existing direct anchored-position fast path while still under that parent. Only when reparented, transform the actual root pivot back into the original top-left design coordinates. Do not alter the flying body's visual-center correction, moving door target, mask entry radius, path, timing, callback order, shrink or retirement.

For a two-house layout, authored size calculation yields entry radius `max(150*512/448, 176*512/398)*204/512/2 = 45.105527638` pixels. The reported 45.1711273 minimum lies just outside that boundary. This is consistent with correct samples stopping at mask entry and later samples using doorway-local coordinates; it is source/arithmetic evidence, not a captured Unity trace.

## Test interpretation and ownership

Both reported thresholds remain unchanged and meaningful after the coordinate corrections. `Position` remains the actual root anchor, not the center of a tilted/flapping body. The home-flight body is centered on the root for shrinking; final rotation is reset before the arrival callback, so its final visual center and root coincide. Mid-flight tilt may separate them.

A frame-only minimum cannot prove an exact endpoint: the final motion update and removal can happen before the next test-coroutine sample. Do not add an artificial production dwell to satisfy sampling. The report supplies a precise separate-owner test strengthening: measure the transformed `Bird body/Pose` center for the visual-distance claim, retain `< 40`, and route the existing endpoint geometry probe through `FlyHome` so it observes the masked visual center inside the arrival callback before deactivation. No test edit is part of this delivery.

## Acceptance

- Source change limited to BirdView and the two task documents; no idle suppression, thresholds, assertions, callback generation changes or test edits.
- Static naming guard passes.
- Disclose arithmetic-only checks and all runtime limits.
- Coordinator reviews/integrates and reruns the two original motion cases, the remaining suite, and actual idle/flight/mask/Undo/cancel GUI/timed acceptance on Mac. Existing source inspection and numeric invariants are not runtime passing results.
