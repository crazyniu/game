# Aviary centering audit

## Result

The bounded correction is in `PerchSort/game/aviary_view.gd`. It preserves all accepted art files, the shared `UI`, `UiText`, `Island`, `FeatherBar`, `ShopSkin` and `MenuNavigation` implementations, and all game/economy behavior.

The annotated detail sign was centered at x496 rather than x540. It is now centered at x540 without changing its original 464px width. Back, sign and visible balance artwork share y119; the balance moves 24px right to avoid its coin touching the sign leaves. Ability text moves from y764 to y766 and is centered by source alpha. Ownership and price raster textures already contained their heavy surrounding strokes; there was no duplicate button face. A local material suppresses the dark surround, and local bounds calculations center the resulting visible paint. Prices now center the coin-plus-number composite, including unequal end-glyph margins.

## Verification

- Godot 4.7.2 headless `res://tests/run_aviary_text.gd`: 370 checks, 0 failures. All 10 species in both languages plus 18 price cases spanning 1–99999. 274 measured centers, maximum analytical error 0.000016 design pixels.
- Same runner in real OpenGL Compatibility: 370 checks, 0 failures, 20 full 1080x1920 captures. `evidence/gui.log`, `evidence/visible-ink.json`.
- Existing `res://tests/run_aviary_actions.gd`: 1203 checks, 0 failures. Includes exact canonical digit identity, selected/owned/locked states, bottom-edge native-input routing, opening/closing confirmation and equipping default outfit.
- Full `bash scripts/run_tests.sh` with the 4.7.2 binary: naming self-tests 5/5, naming 0 violations, painted-word guard 0 problems, smoke 0 failures, all 100-level core replays 0 failures in 1641ms.
- A read-only broad label audit sampled 228 painted label instances across 42 states with labels. Maximum non-Aviary residual center error was 0.27px. The first rapid-lifecycle harness produced pre-existing headless Home shader and disposed-preview callback warnings; it is diagnostic coverage, not a substitute for the clean focused/aggregate tests. The final paced run sampled 230 labels and retained the same Home shader compiler error, with no disposed-preview callback errors. Initial and final diagnostic logs are preserved. Neither broad audit is claimed as error-free.
- Additional isolated locale-rebuild test: 378 checks, 0 failures, including switching the same live Aviary view between English and Chinese. It retains species, re-centers visible ownership ink, keeps the local material and leaves fake profile data unchanged. `evidence/locale-rebuild.log`; test-only patch `build/text-centering/locale-check.patch`.

## Evidence

- Bilingual contact sheets: `evidence/aviary-en-contact.jpg`, `evidence/aviary-zh-contact.jpg`.
- Full-resolution samples: `evidence/sunny_en.png`, `sunny_zh.png`, `hoot_en.png`, `hoot_zh.png`.
- All 20 full-resolution captures: `build/text-centering/screens/`.
- Bounded source patch: `build/text-centering/aviary-correction.patch`.
- Before source SHA256: 4aa8eb9ac41e831d5f52cabc8a8a7eb28d6783c5cb606daa0b0edc4223b15cbd.
- Frozen after source SHA256: 898c59e3ad314824ae188bcf8b8f6f623b071a0daa52d9c7d7839951ad665da7.

The first GUI harness set the generic capture flag before scene initialization and stopped after four screenshots. It is not the final evidence. The corrected runner completed all 20 captures and logged 370 checks; the initial auto-capture is retained under `build/text-centering/initial-auto-capture.png`.

## Boundaries

No source-art regeneration, asset upload, code upload, commit, push, merge or deploy. No gameplay modifications. No physical-device coverage or human audio listening. Motion remains the existing behavior; these stills prove only layout. Final aesthetic acceptance belongs to the user. Integration and cross-page aggregate regression are coordinated separately.
