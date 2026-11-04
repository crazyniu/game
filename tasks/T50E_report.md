# T50E — authored family overlay integration report

## Delivery

Status: **DONE for bounded code/spec/report scope; runtime acceptance unverified**.

- Branch: `mac-motion/t50`.
- Separate follow-up after `a012e70` (T50A; current Codex reports it passed static review and D integrated it).
- Delivery SHA: supplied in the final DONE message/Orca notification after this report is committed with the code; no self-referential hash is embedded in this commit.
- Current Codex reviews; D integrates afterward. D's separate root transaction blocker is not changed here.
- No Claude contact, main merge, deployment, sub-worker or new Dispatch.

## Changed files

- `PerchSort/Assets/Scripts/V5/Game/BirdView.cs`
- `PerchSort/Assets/Scripts/V5/Game/V7BirdOverlay.cs` (new internal helper)
- `PerchSort/Assets/Scripts/V5/Game/V7BirdOverlay.cs.meta`
- `tasks/T50E_overlay_integration.md`
- `tasks/T50E_report.md`
- `docs/team/status/mac-motion.md`

No other module, art, layout/placement, Core/gameplay, authored level, shared art loader, scene or package file changed. No image generation/regeneration or saved derived QA artwork.

## Read-only source evidence

- Read `f79dc38:tasks/T49_eggs_report.md` and `art/v7/eggs_overlays/manifest.json`; asset SHA `34eddebeca3b75fa5e39019090c8b486ba99911f`.
- Read `fd14469:tasks/T49_report.md`, both supplied `art/v7/birds/qa/{sunny,sky}_frames.png` 17-frame strips, and material individual Sunny/Skye head, flight, cheer and resting frames. Viewed the supplied overlay game-size QA, not a newly generated composite.
- Final Sunny/Skye idle PNG bytes are identical to the manifest's corresponding reference snapshots; the idle anchors therefore refer to those exact delivered frames.
- Read all 40 authored levels as data. There are eight family tokens total: one mama and one chick on each of L15 Sunny, L16 Skye, L18 Sunny and L39 Sunny. No other authored species has family flags. This was a data inventory, **not a level simulation/playthrough**.

## Implemented

### Normalized source-frame geometry

Overlay attachment remains the centered source point `(64,64)` of each supplied 128×128 marker. Placement uses the body canvas's size, not hardcoded design pixels or a second density/chick multiplier:

- Bow canvas: `body.sizeDelta * (72 / 320f)`.
- Fluff canvas: `body.sizeDelta * (88 / 320f)`.
- Position: `(body.width * sourceX / 320, -body.height * sourceY / 320)` relative to the body's existing top-left child anchors.

At a 200px body these are 45px bow / 55px fluff; the existing 0.7 parent chick scale produces a 38.5px fluff canvas. Density is already in `body.sizeDelta`. Both markers stay children of the same foot-pivoted body: facing, idle squash/rotation, breathing, chick scale, flight shrink/tilt and cheer transform the whole group once. No separate facing mirror or repeated density/chick scaling was added.

Manifest idle centers retained exactly:

| Species | Bow center on 320px source | Fluff center on 320px source |
|---|---|---|
| Sunny | (104, 140) | (165, 244) |
| Skye | (215, 123) | (163, 243) |

### Material pose calibrations

The supplied strips show large opposing head rolls, Skye's tilted second quirk and downward/preening head changes; Skye's preening wing also covers part of the original chest region. Those frames receive the following **integration calibrations from static source inspection**, not extra values claimed to be in the art manifest:

| Species / frame | Bow center | Bow clockwise roll in source coordinates | Fluff center |
|---|---|---|---|
| Sunny tilt_left | (94, 164) | -20° | (165, 244) |
| Sunny tilt_right | (121, 124) | +20° | (165, 244) |
| Skye tilt_left | (193, 112) | -20° | (163, 243) |
| Skye tilt_right | (232, 142) | +20° | (163, 243) |
| Skye quirk2 | (224, 136) | +15° | (163, 243) |
| Skye look_down | (215, 140) | 0° | (163, 243) |
| Skye preen | (198, 132) | +8° | (179, 255) |

The preen fluff center stays on the exposed lower/right chest rather than the foreground wing. Other reviewed frames retain the idle attachment regions: their wing/expression changes do not justify inventing unrelated marker displacement. In particular both flight frames and cheer remain frame-mapped, not outside the attachment path. Head-roll angles become the opposite local Unity Z angle because source Y points downward; the existing parent facing scale mirrors both pose and marker together.

These are static calibration candidates shipped for review/user testing. Exact apparent attachment, overlap and transitions at actual game density are **not runtime verified**.

### Synchronous frame routing / cancellation

Every body `graphic.sprite` mutation now goes through private `BirdView.SetFrame(Sprite)`, which changes the pose and immediately applies its overlay attachment. The only direct assignment remaining is inside that setter. This includes the expression-bodied quirk/wobble callbacks, idle scheduler, wing swaps, cheer, sleep/wake, hatch/rest and cancellation paths.

The helper caches actual rendered V7 Sprite references and normalized pose data once per family bird. Repeated updates to the same frame do not reapply geometry; no frame-time strings, new motion channel, coroutine, timer or callback was added. Normal non-family birds allocate no helper. Existing public signatures, callback generation guards and committed event semantics are unchanged.

Cancel restores the resting sprite through the same setter, which restores its attachment center/roll in the same call. The body's existing scale/rotation restoration then restores the whole group. There is no asynchronous overlay work to survive teardown or race a rebuild.

### Legacy and unsupported-content safety

- V7 source-frame measurements are registered only for reviewed Sunny/Skye sprites with the expected V7 name and 320×320 canvas. Legacy `bird_<id>_*` / other-sized frames do not receive those pose measurements.
- Overlay identity is checked against the actual `Resources.Load<Sprite>("V7/Birds/<marker>")` reference and expected 128px canvas. This is identity detection only; existing V5Art still owns the resolved artwork. Width alone is insufficient: the old V5 fluff is also 128px (the old bow is 96px).
- Legacy overlay assets retain legacy display dimensions normalized from the old 200px canvas: 48/200 bow, 75/200 fluff. Legacy bird frames retain old center fractions `(105/200, 32/200)` and `(75/200, 103/200)` rather than applying measurements from a different silhouette.
- Mixed deliveries choose position by the rendered bird frame and display size by the actual marker asset. Compatibility geometry is preserved where no V7 pose is registered; mixed/legacy combinations are not claimed as newly reviewed visual placements.
- A future family marker on another species warns once and retains compatibility anchors. There is **no all-species placement claim** and no invented Berry/Minty/other-species pose map.

## Integration contract

No new public BirdView or root API. Internal helper only:

```csharp
V7BirdOverlay(RectTransform body, Image bow, Image fluff, string species, V5Art art)
void Apply(Sprite frame)
```

D should integrate this commit after current Codex review, alongside the final overlay asset delivery `f79dc38` / `34eddebeca3b75fa5e39019090c8b486ba99911f` and final birds `fd14469`. Those branches were inspected read-only, not merged into this worker branch; the current checkout still contains legacy marker fallbacks. D's root transaction fix remains independent. No shared loader/layout edits or other integration changes are requested.

## Verification and missing requirements

Performed: read-only source/manifest/level inventory, supplied image inspection, idle-byte identity comparison, source routing query (one direct body-sprite write, inside `SetFrame`), and static transform/API/lifetime review. Code uses existing UnityEngine/UI assembly references; the helper owns no Unity objects requiring disposal beyond the existing BirdView children. New `.meta` provided without import.

Explicitly **NOT_RUN** by user instruction:

- Compiler or Unity import/compile.
- Unity runtime, tests/test suites, gameplay simulation/all-level playthrough.
- Runtime attachment/clipping/facing/density/chick checks, timed animation or cancellation/teardown playback.
- New rendered overlay composites or runtime screenshots.

No intentionally missing bounded code requirement. Final art integration is D-owned; pose calibration and mixed-fallback appearance remain unverified until coordinator review and later user testing. This delivery must not be described as visually/runtime accepted.
