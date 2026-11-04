# Bird flight sequence delivery

## Current batch

Sunny, Sky, Berry, Mint, Cocoa, Snow, Ember, Ink, Hoot and Crow: 24 newly drawn frames each exported (240 total), covering takeoff (3), flap (6), and landing (3) in each direction. Right and left views were generated separately; no sprite was mirrored. Runtime images are 320 x 320 RGBA with centered pivots and importer settings copied from the approved Sunny idle metadata. Existing art and all code are unchanged.

The built-in imagegen tool produced the artwork. Cropping, uniform scaling and translation registered the drawn poses for export; no programmatic artwork, recolouring, mirroring or pose fabrication was used. Registration details and source-cell provenance are in the per-species `art/birds_sequences/qa/*_flight_registration.json` manifests.

Sixty contact sheets under `evidence/` show the idle reference, light/dark composites and 64-pixel grayscale views. Sixty APNGs under `art/birds_sequences/qa/` preview the individual sequences at 10 fps. These are offline previews, not Unity runtime acceptance.

## Rejected candidates

The [exact rejected-source list](evidence/rejected-sources.md) contains 73 excluded strips, plus one rejected cell in a partially used Sunny strip. All requested delivery slots have replacements. `art/birds_sequences/qa/flight_inventory.json` records the exact inventory.

Raw rejected candidates remain under `art/birds_sequences/raw/sunny_flight_*rejected*.png`. Reasons include background wash, failed alpha cleanup, reversed spiral crest, excessive proportions/wing length, weak anticipation crouch, low final mid-downstroke, and paw pads instead of three-toed feet. Targeted regeneration corrected those failures. An additional overly wide final flap candidate clipped at 320 pixels and was replaced by the compact drawn candidate; its source is retained in `sunny_flight_flap_upstroke.png` and is used only for the other two valid cells. The export manifest identifies the compact source used for the final flap frame.

## Validation

- Naming guard self-test: 8 passed.
- Naming guard: passed after staging (1862 tracked text files, zero violations).
- Frame dimensions, RGBA, alpha extrema 0/255, distinct file hashes, no visible bounds (alpha > 12) touching canvas edges, metadata and preview checks: passed for all 240 exports and sixty previews.
- Final Unity compilation and full suites ran with `flock /tmp/unity.lock bash scripts/run_tests.sh`: EditMode 312/312 and PlayMode 53/53 passed, zero failures and zero skips. Exact timestamps and counts: `art/birds_sequences/qa/flight_unity_results.json`. Retained LevelsData and Layout discovery safeguards passed unchanged.
- `flock /tmp/unity.lock bash scripts/screenshots.sh`: exit 0; 34 captures at 1080 x 1920 in ignored `build/screens/game/`. Map and first-level captures were inspected. These exercise the existing game; they do not display the new, unintegrated flight sequences.
- A stricter nonzero-alpha edge audit found faint source/resampling pixels in 50 exports, at alpha 1-7 out of 255. No visible silhouette crosses the canvas boundary at the existing batch threshold of alpha > 12. Pixel-level exterior transparency is therefore not uniformly zero. Exact affected filenames and values: `art/birds_sequences/qa/flight_export_checks.json`. No alpha thresholding or painted cleanup was applied.
- Code integration and actual flight playback are a later separate task. No claim of runtime visual acceptance.

## Remaining images

None. All 240 requested images are exported in the specified production order.

## Species correction notes

Sky rejected sources record missing feet, broad alpha halos/fringe and excessive wing span. Feet, alpha and wing anatomy were regenerated before export. Full generation/correction prompts are saved per strip.

Berry initial left-facing candidates incorrectly faced right and were rejected. Regeneration used the existing left-turn frame as a direction reference, preserving the two-plume silhouette without mirroring. Alpha halos and two excessive wing spans were also regenerated.

Earlier partial-delivery run (48 flight frames) failures: `T54AMusicTests.FocusAndApplicationPauseFreezePlayheadsUntilBothResume` expected 1.13065755 +/- 0.02, observed 1.15199542; `T54CIdleTests.BreathingPersistsBetweenActionsWithoutMovingLogicalSeatsAndInstantFreezesIt` failed its scale equality assertion. No code or tests were edited. Both passed in the final complete-asset run; this does not establish a code fix for those intermittent failures. Earlier XML remains in ignored `build/flight-initial-editmode.xml` and `build/flight-initial-playmode.xml`. Final logs/XML are under ignored `build/`. The final generated scene, seven expanded overlay importers and 354 whitespace-only metadata changes were reviewed, preserved in `build/flight-final-generated-changes.patch`, then restored to the recorded pre-run tracked content. No scene, settings, package, existing art or code changes are delivered.

Mint rejected candidates had a visible backdrop, weak crouch and excessive braking wing span. Regenerated cutouts and compact braking wings passed the export gates.

Cocoa rejected sources contained a background wash or excessive mid-downstroke/braking wing spans. Seven strips were regenerated; the sleepy face and chest spots remain readable at 64 pixels.

Snow final right flap initially pointed too low; its strip was rejected and regenerated with a horizontal mid-downstroke. Both directions preserve the left-leaning crest.

Ember rejected candidates included a reversed left crest, opaque background/halo, low loop endpoints, missing forward braking feet, weak left crouch/push-off and excessive wing spans. Targeted regenerated poses passed the same export review.

Ink rejected candidates had low push-off wings, low loop endpoints or oversized wing spans. Seven regeneration passes corrected them while preserving the white star and swept crest.

Hoot rejected strips had low push-off wings, a weak left crouch and a reversed cap in left landing. Regeneration restored the left pom-pom and clarified takeoff poses.

Crow rejected candidates had reversed right-view crests, a bent-wing peak, and excessive wing span. Regeneration preserved the right-swept crest and gold compass on the viewer-left side in both travel directions. Gray-foot registration was checked separately from the orange-foot species.

## Batch pushes

Each species was committed and pushed before starting the next species. Only `mac-flight-frames/work` was pushed.

| Species | Commit | Frames |
| --- | --- | --- |
| Sunny | `236039e0` | 24 |
| Sky | `0d6269b4` | 24 |
| Berry | `cb657b3e` | 24 |
| Mint | `dddaf75e` | 24 |
| Cocoa | `454b1737` | 24 |
| Snow | `af554687` | 24 |
| Ember | `cd910aa6` | 24 |
| Ink | `147c58ae` | 24 |
| Hoot | `d74f83f0` | 24 |
| Crow | `274d0ae3` | 24 |

The final evidence commit adds this report and machine-readable delivery checks. The source diff from the starting tip contains only the new flight assets, raw/prompt/QA files and flight documentation. No merge, main push or release was performed.
