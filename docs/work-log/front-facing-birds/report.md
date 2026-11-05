# Front-facing bird delivery

## Delivered

Sunny, Sky, Berry, Mint, Cocoa, Snow, Ember, Ink and Hoot: 9 species, 27 PNG sprites and 27 copied importer metadata files. Each has front, half-closed blink and closed blink. All are 320x320 RGBA with centred pivot and unique asset/sprite GUIDs.

Generated using the built-in imagegen tool, one three-cell transparent sheet per species. Raw sheets, exact prompts, reference contacts and QA are under `art/front_birds/`. JPEG evidence is under `evidence/`: canonical idle on the top row, front and blink rows below, light/dark backgrounds, 64 px colour previews and 50 percent idle overlays. Offline blink APNGs are artwork previews only.

## Registration and review

Used the existing `art/birds_sequences/qa/prepare_sequences.py` feet and background functions without modifying its source. Split each raw sheet into three equal cells and crop transparent gutters to the visible bounds plus four source pixels. The initial Sky export retained a stray alpha-1 pixel at the canvas border; the gutter crop removes it without changing the drawn silhouette. The first push preceded this strict border-check correction; this follow-up records the passing rerun. One uniform scale per trio matches the canonical idle's visible silhouette height (alpha > 12); centred each cell on the idle toe midpoint and registered to its actual toe baseline, including a final integer-pixel rounding correction. No mirror, rotation, painted pixels or synthetic blink artwork. Metadata is copied from the species idle; only asset/sprite GUIDs and sprite names differ.

Reviewed direct gaze, level head, centred beak, relaxed wings, original colours/crest and proportions, light/dark alpha edges, 64 px readability and 50 percent identity overlays. The intended front-view reprojection shifts facial landmarks relative to the angled idle. Closed eyes have no catchlight; open/half-closed eyes retain catchlights.

## Verification

Asset checks passed for all 27 current PNGs: exact size/mode, alpha 0..255, transparent canvas borders, copied importer settings, pivot and unique new GUIDs. All registered toe baselines equal their idle baseline: Snow, Ember and Hoot 299 px; the other six species 300 px. Nine JPEG contacts and nine offline blink previews inspected.

Unity compilation, EditMode/PlayMode tests and runtime screenshots are pending: the user's primary Editor is open. Requested closure without terminating it. Offline artwork delivery does not establish runtime wiring, timed animation or device acceptance. No code, scene, existing art or save changes. Hoot retains the purple star cap that is already part of its canonical base idle identity; no alternate outfit was introduced.

## Remaining

Crow.
