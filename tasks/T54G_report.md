# T54G bird frame sequences report

Branch: `mac-t54g/t54`, created from the supplied T54 contract tip `47d5e7e2e4059e9cfd1946734f30f1b078204679`.

## Scope and method

Pilot art only: Sunny, Sky and Berry, eight drawn frame sequences and 38 runtime PNGs per species. Approved stills and popup art were inspected read-only. No scripts, scene, settings, packages, saves or approved originals are modified. The built-in imagegen tool draws the poses. Exact prompts are in `art/birds_sequences/prompts/`; sheets are in `art/birds_sequences/raw/`.

`art/birds_sequences/qa/prepare_sequences.py` slices sheets, performs uniform scale registration against each species' approved toe span, anchors grounded toes at y=300, and writes 320x320 RGBA sprites. Hop airborne poses retain their drawn displacement above that ground plane. Registration does not manufacture action frames, mirror artwork or create transformations as actions. Metadata copies the Sunny idle importer settings, replacing only asset/subsprite identities.

## Delivery status

Sunny complete and pushed at `53f8bb0`: 38 runtime frames, 38 metadata files, eight source sheets, eight contact sheets and eight 10 fps APNG previews.

Sky complete: the same 38-frame/eight-sequence set, with metadata, source sheets, contact sheets and APNGs. Berry has not been delivered.

Required per-species sequence counts: flap 6, hop 6, turn_left 4, turn_right 4, cheer 6, preen 5, peek 4, blink 3.

## QA and rejected candidates

Every completed sequence has a JPEG contact sheet on light/dark backgrounds, includes the approved idle and 64px grayscale thumbnails, and has a looping APNG preview at 10 fps. Contact sheets are duplicated under `tasks/evidence/T54G/` (maximum width 1120px). Inventories record source cells, scale, placement, alpha, bounds and export hashes.

Rejected: the initial six Sunny flap candidates had an off-white translucent backdrop. A built-in background-extraction edit produced a clean alpha cutout; only the corrected sheet is used by runtime exports. The rejected source is retained as `art/birds_sequences/raw/sunny_flap_rejected_backdrop.png`.

An intermediate Sunny hop registration was rejected because the first toe detector included low yellow wing tips and shrank the exports. The detector was corrected to sample opaque orange toes, then the exports and evidence were rebuilt. This did not change the drawn source poses.

An initial Sky flap 05 extraction included three low-alpha pixels from the preceding sheet cell's wing tip in its empty left gutter. The five-pixel empty gutter was excluded during slicing, with the bird's own pose untouched. The corrected export has no clipped bounds.

## Validation boundary

- `python3 ci/check_current_game_names.py`: PASS, 0 violations before delivery staging; repeated at each completed delivery.
- Sunny offline PNG/import metadata and APNG inventory: PASS (38 RGBA exports at 320x320, 38 centered-pivot metadata files with unique asset GUIDs, all eight looping previews encoded at 100ms per frame, no clipped export bounds). Recorded in `art/birds_sequences/qa/sunny_checks.json`.
- Sky: the same offline gates PASS for all 38 frames and eight looping previews. Recorded in `art/birds_sequences/qa/sky_checks.json`.
- Mac compilation, EditMode, PlayMode, GUI/timed Unity acceptance: NOT_RUN by this worker. The current user-provided AGENTS.md requires the coordinator to serialize Unity after integration and prohibits workers from independently launching it; this supersedes the older T54 contract's worker test-run paragraph.
- Contact sheets and APNGs are offline art evidence, not real Unity playback or final visual acceptance.

## Remaining work

Push Sky, then complete Berry. No Sunny or Sky frames are missing or remain rejected. Berry is missing all 38 pilot frames. The seven non-pilot species are outside this assigned delivery: mint, cocoa, snow, ember, ink, hoot and crow.
