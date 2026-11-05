# Front-facing bird delivery

## Delivered

All ten species are complete as artwork: Sunny, Sky, Berry, Mint, Cocoa, Snow, Ember, Ink, Hoot and Crow. Each has `<species>_front.png`, `<species>_front_blink_01.png` (half closed) and `<species>_front_blink_02.png` (closed) in `PerchSort/Assets/Resources/Birds/`.

| Deliverable | Count |
| --- | ---: |
| Completed species | 10 |
| 320x320 RGBA resource PNGs | 30 |
| Copied importer metadata files | 30 |
| Built-in generated raw sheets | 10 |
| Exact generation prompts | 10 |
| Idle/cheer/face reference contacts | 10 |
| Reviewed JPEG evidence contacts | 10 |
| Offline blink APNG previews | 10 |
| Per-species registration records | 10 |
| Missing required sprites | 0 |
| Rejected generated sheets | 0 |

Generated with the built-in imagegen tool only, one three-cell transparent sheet per species. Raw sheets, exact prompts, reference contacts, registration records, `checks.json` and `summary.json` are under `art/front_birds/`. JPEG contacts under `evidence/` show canonical idle on the top row, front and blink rows below, light/dark backgrounds, 64 px colour previews and 50 percent idle overlays. APNGs show open/half/closed/half/open at 800/90/120/90/800 ms; they are offline artwork evidence only.

## Registration and visual review

Used the existing `art/birds_sequences/qa/prepare_sequences.py` feet and background functions without modifying its source. Split each raw sheet into three equal cells and crop transparent gutters to visible bounds plus four source pixels. Use one uniform scale per trio to match the idle's visible silhouette height (alpha > 12), align each cell to the idle toe midpoint and baseline, then correct integer-pixel rounding. Crow uses the existing remaining-species grey-toe selection rule. No mirroring, rotation, painted pixels or procedural blink artwork.

Every exported trio matches its idle toe baseline exactly: Snow, Ember, Hoot and Crow at 299 px; Sunny, Sky, Berry, Mint, Cocoa and Ink at 300 px. Rasterized front silhouette height differs from idle by at most two pixels. All use the same 320x320 canvas and `(0.5, 0.5)` pivot. Metadata is copied from each species idle; only asset/sprite GUIDs and sprite names differ. Existing originals and slicing tools are unchanged.

Reviewed all ten contacts for direct gaze, level head, centred smiling beak, resting wings, species colours/crest/proportions, light/dark edges, 64 px readability and 50 percent identity overlays. Front-view reprojection intentionally moves facial landmarks relative to angled idle art. Sky and Cocoa retain their relaxed eyelids. Hoot retains its canonical purple star cap, and Crow its canonical compass; neither receives an alternate outfit. Closed eyes have no catchlight.

The initial Sky export retained a stray alpha-1 pixel at the outer canvas border. The first push preceded that strict border-check correction. Cropping the empty sheet gutters removed it without changing the drawn silhouette; all nine first-batch assets then passed the rerun and the correction was pushed before starting the next batch. No raw sheet required regeneration.

## Verification and boundaries

- `/tmp/perch-remaining-species-tools/bin/python /tmp/prepare_front_birds.py <species...>`: temporary adapter around the existing slicer helpers; produced the registered sprites, copied metadata and contact/APNG evidence. It does not modify project code or the existing tools.
- `/tmp/perch-remaining-species-tools/bin/python /tmp/check_front_birds.py`: **30/30 asset checks passed** for size/mode, real alpha 0..255, fully transparent canvas borders, exact importer equivalence after identity substitution, centred pivot and **165 unique fresh asset/subsprite GUIDs**.
- Registration review: **10/10 species**, three distinct PNGs each, exact idle toe baseline and silhouette-height tolerance within two raster pixels. Records include translations, scale and SHA-256 hashes.
- `python3 ci/check_current_game_names.py --self-test`: **9 passed**.
- `python3 ci/check_current_game_names.py`: **0 violations**.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`: **NOT_RUN**. The user's primary Unity Editor was open at initial inspection and remained open at final inspection. Closure was requested; no Editor was terminated and no competing batch process was launched. New compilation/EditMode/PlayMode counts are unavailable.
- `flock /tmp/unity.lock bash scripts/screenshots.sh`: **NOT_RUN** for the same open-Editor constraint. These new resource names are not wired by this art-only assignment. Timed runtime, integrated visual and device acceptance remain pending with the coordinator.
- No game code, existing tools, scene, settings, original art, persistence keys or save data changes. Work is confined to this assigned branch and asset/evidence paths.

## Pushes

| Batch | Commit |
| --- | --- |
| Sunny, Sky, Berry | `e63c5849` |
| Transparent gutter correction and passing QA | `9ced155e` |
| Mint, Cocoa, Snow | `712d1f93` |
| Ember, Ink, Hoot | `a63e996b` |
| Crow and final inventory | This report's commit |

Each three-species batch was pushed before starting the next batch. Crow is the final single-species push. All required artwork is delivered; runtime verification remains explicitly unclaimed.
