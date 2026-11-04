# Bird flight sequence delivery

## Current batch

Sunny, Sky, Berry, Mint, Cocoa, Snow, Ember, Ink and Hoot: 24 newly drawn frames each exported (216 total), covering takeoff (3), flap (6), and landing (3) in each direction. Right and left views were generated separately; no sprite was mirrored. Runtime images are 320 x 320 RGBA with centered pivots and importer settings copied from the approved Sunny idle metadata. Existing art and all code are unchanged.

The built-in imagegen tool produced the artwork. Cropping, uniform scaling and translation registered the drawn poses for export; no programmatic artwork, recolouring, mirroring or pose fabrication was used. Registration details and source-cell provenance are in `art/birds_sequences/qa/sunny_flight_registration.json`.

Fifty-four contact sheets under `evidence/` show the idle reference, light/dark composites and 64-pixel grayscale views. Fifty-four APNGs under `art/birds_sequences/qa/` preview the individual sequences at 10 fps. These are offline previews, not Unity runtime acceptance.

## Rejected candidates

Raw rejected candidates remain under `art/birds_sequences/raw/sunny_flight_*rejected*.png`. Reasons include background wash, failed alpha cleanup, reversed spiral crest, excessive proportions/wing length, weak anticipation crouch, low final mid-downstroke, and paw pads instead of three-toed feet. Targeted regeneration corrected those failures. An additional overly wide final flap candidate clipped at 320 pixels and was replaced by the compact drawn candidate; its source is retained in `sunny_flight_flap_upstroke.png` and is used only for the other two valid cells. The export manifest identifies the compact source used for the final flap frame.

## Validation

- Naming guard self-test: 8 passed.
- Naming guard: passed after staging (1551 tracked text files, zero violations).
- Frame dimensions, RGBA, transparent exterior, distinct file hashes, no visible bounds touching canvas edges, metadata and preview checks: passed for all 216 exports and fifty-four previews.
- Unity compilation and full suites ran with `flock /tmp/unity.lock bash scripts/run_tests.sh`. EditMode: 312/312 passed, zero skipped. PlayMode: 51/53 passed, two failed, zero skipped. Screenshots still pending.
- Code integration and actual flight playback are a later separate task. No claim of runtime visual acceptance.

## Remaining images

Crow: all 24 flight frames per species remain unproduced (24 total), in the specified production order.

Sky rejected sources record missing feet, broad alpha halos/fringe and excessive wing span. Feet, alpha and wing anatomy were regenerated before export. Full generation/correction prompts are saved per strip.

Berry initial left-facing candidates incorrectly faced right and were rejected. Regeneration used the existing left-turn frame as a direction reference, preserving the two-plume silhouette without mirroring. Alpha halos and two excessive wing spans were also regenerated.

Unity failures: `T54AMusicTests.FocusAndApplicationPauseFreezePlayheadsUntilBothResume` expected 1.13065755 +/- 0.02, observed 1.15199542; `T54CIdleTests.BreathingPersistsBetweenActionsWithoutMovingLogicalSeatsAndInstantFreezesIt` failed its scale equality assertion. No code or tests were edited. Logs/XML are under ignored `build/`. Generated scene, seven expanded overlay importers, metadata whitespace and tracked Python cache changes were reviewed, preserved in `build/flight-generated-changes.patch`, then restored to the pre-run tracked content.

Mint rejected candidates had a visible backdrop, weak crouch and excessive braking wing span. Regenerated cutouts and compact braking wings passed the export gates.

Cocoa rejected sources contained a background wash or excessive mid-downstroke/braking wing spans. Seven strips were regenerated; the sleepy face and chest spots remain readable at 64 pixels.

Snow final right flap initially pointed too low; its strip was rejected and regenerated with a horizontal mid-downstroke. Both directions preserve the left-leaning crest.

Ember rejected candidates included a reversed left crest, opaque background/halo, low loop endpoints, missing forward braking feet, weak left crouch/push-off and excessive wing spans. Targeted regenerated poses passed the same export review.

Ink rejected candidates had low push-off wings, low loop endpoints or oversized wing spans. Seven regeneration passes corrected them while preserving the white star and swept crest.

Hoot rejected strips had low push-off wings, a weak left crouch and a reversed cap in left landing. Regeneration restored the left pom-pom and clarified takeoff poses.
