# Bird flight sequence delivery

## Current batch

Sunny: 24 newly drawn frames exported, covering takeoff (3), flap (6), and landing (3) in each direction. Right and left views were generated separately; no sprite was mirrored. Runtime images are 320 x 320 RGBA with centered pivots and importer settings copied from the approved Sunny idle metadata. Existing art and all code are unchanged.

The built-in imagegen tool produced the artwork. Cropping, uniform scaling and translation registered the drawn poses for export; no programmatic artwork, recolouring, mirroring or pose fabrication was used. Registration details and source-cell provenance are in `art/birds_sequences/qa/sunny_flight_registration.json`.

Six contact sheets under `evidence/` show the idle reference, light/dark composites and 64-pixel grayscale views. Six APNGs under `art/birds_sequences/qa/` preview the individual sequences at 10 fps. These are offline previews, not Unity runtime acceptance.

## Rejected candidates

Raw rejected candidates remain under `art/birds_sequences/raw/sunny_flight_*rejected*.png`. Reasons include background wash, failed alpha cleanup, reversed spiral crest, excessive proportions/wing length, weak anticipation crouch, low final mid-downstroke, and paw pads instead of three-toed feet. Targeted regeneration corrected those failures. An additional overly wide final flap candidate clipped at 320 pixels and was replaced by the compact drawn candidate; its source is retained in `sunny_flight_flap_upstroke.png` and is used only for the other two valid cells. The export manifest identifies the compact source used for the final flap frame.

## Validation

- Naming guard self-test: 8 passed.
- Naming guard: passed after staging (1551 tracked text files, zero violations).
- Frame dimensions, RGBA, transparent exterior, distinct file hashes, no visible bounds touching canvas edges, metadata and preview checks: passed for all 24 exports and six previews.
- Unity full EditMode/PlayMode and screenshots: not run yet. Another worktree holds the serialized Unity lock; no process was stopped.
- Code integration and actual flight playback are a later separate task. No claim of runtime visual acceptance.

## Remaining images

Sky, Berry, Mint, Cocoa, Snow, Ember, Ink, Hoot and Crow: all 24 flight frames per species remain unproduced (216 total), in the specified production order.
