# mac-motion — T50E family overlay integration

Status: **DONE for bounded code/spec/report scope; awaiting current Codex review, then D integration**.
Branch: `mac-motion/t50`; separate follow-up after T50A `a012e70`. Final delivery SHA is supplied in the post-commit DONE notification.
Coordinator: current Codex on 251. Do not contact the retired Claude session or merge main.

## T50E delivery

- Only BirdView runtime edits plus new internal [V7BirdOverlay.cs](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/docs/team/status/mac-motion.md#L9) / `.meta`.
- Consumes the final overlay contract read-only from `mac-art/world` `f79dc38` (asset `34eddebeca3b75fa5e39019090c8b486ba99911f`) and reviewed Sunny/Skye frame strips/PNGs from `mac-birds/t49` `fd14469`.
- All 40 authored levels read as data: Sunny family on L15/L18/L39; Skye on L16; eight mama/chick tokens total. No all-species attachment claim.
- Manifest idle centers and normalized bow72/320, fluff88/320 size. Seven explicit material pose overrides for head rolls/Skye head shift/preen; other reviewed family poses retain stable attachment regions.
- Every body-frame change synchronously updates marker geometry, including idle/flight/cheer/cancellation. Children inherit body facing, density, breathing/tilt/shrink and chick0.7 once.
- Actual V7 marker identity distinguishes legacy fluff with the same source dimensions. Legacy frames/assets preserve disclosed compatibility geometry. Unsupported family species warn once rather than inventing placements.
- No new public/root API or asynchronous work. Gameplay, bird placement/layout, art, other modules and D's separate root transaction fix untouched.

Spec: `docs/work-log/overlay-integration/spec.md`.
Report: `docs/work-log/overlay-integration/report.md` (exact calibration table, fallback rules and integration contract).

## Evidence / untested limits

Static source/manifest/data inventory and read-only supplied image inspection only. Final Sunny/Skye idle bytes match the overlay reference snapshots. No new/generated or saved derived artwork.

Compiler, Unity import/compile/runtime, tests, simulation/playthrough, runtime overlay/timing/cancellation/facing/density verification and new composites/screenshots: **NOT_RUN**. Pose calibrations are unverified integration values, not visual/runtime acceptance. D integrates final assets plus this commit only after coordinator review; no asset branch merged here.

## T50A history

Per current Codex instruction, T50A `a012e70` passed static review and D integrated it. Implementation `39f36f5`; prior delivery report `docs/work-log/bird-motion/report.md`. The old report's partial-V7 inventory describes that historical checkpoint, not final `fd14469` art availability. Existing prior notification receipt: `msg_da01c12e6434`.
