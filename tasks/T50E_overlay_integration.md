# T50E — authored family overlay integration

## Authority / ownership

Current Codex coordinator requested a bounded follow-up after T50A `a012e70`, already reviewed/integrated by D. Work stays on `mac-motion/t50`; D integrates this follow-up only after coordinator review. Do not contact Claude or merge main.

Own only `BirdView.cs`, isolated `V7BirdOverlay*` helpers/metadata, this spec, `T50E_report.md` and `docs/team/status/mac-motion.md`. No Core/gameplay, board placement/layout, art, other modules, compiler/Unity/tests, new artwork or regeneration. Read asset commits without checking them out or modifying them.

## Inputs and reviewed scope

- Egg/overlay delivery `mac-art/world` `f79dc38`, asset commit `34eddebeca3b75fa5e39019090c8b486ba99911f`; owning contract `tasks/T49_eggs_report.md` and `art/approved_ui/eggs_overlays/manifest.json` at that commit.
- Final bird delivery `mac-birds/t49` `fd14469`; read its supplied Sunny/Skye 17-frame strips and production frames.
- Read all 40 authored levels. Family flags occur only on Sunny (L15/L18/L39) and Skye (L16); one mama and one chick each. Do not claim all-species/all-content placement coverage.

## Requirements / acceptance

- Use source-normalized 320px anchors and 72/320 bow, 88/320 fluff display dimensions for final overlay assets. Source attachment is the overlay's centered (64,64) point on 128px canvas.
- Idle anchor centers: Sunny bow (104,140), fluff (165,244); Skye bow (215,123), fluff (163,243). These references are byte-identical to the final delivered idle frames.
- Inspect material head/pose shifts and provide explicit presentation-only offsets for the authored family species; retain stable torso placement when the supplied torso is unchanged. Account for the preening wing/chest overlap where needed. Record calibration versus manifest values separately.
- Frame changes, idle, flight, cheer and cancellation update the attachment synchronously. Body facing, breathing/tilt/flight transformations, density and chick scale 0.7 must transform the whole bird/overlay group once, without independently mirroring/scaling the markers.
- Distinguish final V7 overlay assets from legacy fallback assets (legacy fluff also has a 128px source). Never infer the new asset solely from texture size. Do not apply V7 pose measurements to legacy bird frames. Unsupported species keep disclosed compatibility geometry rather than invented attachment coverage.
- Preserve all existing public signatures and committed motion callbacks. No new asynchronous overlay work or motion channel.

## Evidence policy

Static source/manifest/level inventory and read-only supplied image inspection only. No compiler, Unity, test suite, gameplay simulation or runtime capture. No generated or edited artwork/QA art. Runtime timing, attachment under actual rendering, imports and acceptance remain **NOT_RUN**.
