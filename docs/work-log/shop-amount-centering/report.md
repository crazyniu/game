# Shop amount centering

## Result

Review finding N1 is corrected locally. Existing visible numeral sprites are translated as a complete group into their current amount box; textures, tint, material, glyph size, inter-glyph spacing and control geometry remain unchanged.

- Shop product prices, stock badges, current-page balance rebuilds and purchase totals reuse the embedded Aviary page's existing unsoftened `_center_digits` helper.
- Aviary's outfit confirmation applies the same local centering before its existing insufficient-funds shake records its origin.
- Shared Island/UiText/ShopSkin/FeatherBar renderers, QuantityInput metrics, Buy placement, assets, prices, callbacks, saves and protected pages are unchanged.

The original maximum purchase amount 1200 had about +4.03 design-pixel horizontal residual. In the final actual OpenGL run, 332 measured amount groups have maximum residual 0.0000153 px horizontally and 0.00000191 px vertically. This includes their owning price/button centers, not only their text slots.

## Measurement isolation

Cross-page testing exposed an existing Aviary measurement side effect in headless Godot: `Texture2D.get_image()` returned the same Image instance, and the softened-alpha measurement mutated its in-memory bytes. Later ordinary Shop measurements could see that altered alpha mask. The local helper now duplicates the Image before calculating the softened mask. This does not edit or replace disk textures or change the intended material.

The final regression hashes all ten digit source images before and after repeated Shop/Aviary previews, live language changes, quantity changes and balance rebuilds. Their in-memory bytes remain identical in headless and actual OpenGL. The isolated original-function reproduction and guarded-function probe are retained as `image-probe-before.log` and `image-probe-after.log`; these are diagnostic runs, not production mutations.

## Verification

Godot 4.7.2, GL Compatibility:

- `tests/run_shop_amounts.gd`: headless 1338 checks and actual desktop OpenGL 1338 checks, zero failures. Independent nonzero-alpha union measurement, owning-face placement, fit, common translation, preserved artwork/material/size/spacing, live quantity/balance/language updates and source-image byte preservation.
- Actual viewport purchase-pointer regression: `tests/run_purchase_buttons.gd`, 128 checks, zero failures. Correct purchases, exact debit/quantity, insufficient funds and outside-frame Close/Buy remain functional.
- Actual Shop navigation/scroll regression: `tests/run_shop_cards_gui.gd`, zero failures. All three tabs, scroll to the final row, return-to-top, accidental-purchase prevention and Close passed.
- Headless purchase-dialog regression: 98 checks, zero failures, including QuantityInput focus/caret/selection and the current Purchase Preview checks.
- Headless purchase placement: 128 checks, zero failures.
- Headless Shop card assertions passed; its teardown printed one resource-in-use diagnostic. The actual GUI equivalent exited cleanly. This is not reported as an error-free headless run.
- Aggregate `scripts/run_tests.sh` passed after staging the production changes, new test and launcher: naming self-test/160-source guard, painted-word guard, smoke, and all 100-level core checks (1759 ms). After adding the two diagnostic probe scripts, the naming guard was rerun: 162 source files, zero violations.

All 36 final-source images are 1080×1920: all 16 Shop preview states in English and Chinese plus both direct Aviary purchase states in both languages. Six contact sheets were visually reviewed. No new clipping, unexpected outline, changed control size or replaced artwork was seen. Contact sheets, capture/source hashes and detailed numerical measurements are alongside the logs. Full-size component originals are preserved in the integration workspace at `build/ui-layout-review/n1-component/`; final canonical recaptures are in `build/text-centering/all-pages/screens/`. Raw PNGs are not duplicated in tracked evidence.

These stills do not revalidate motion, listening/audio or physical-device behavior. No remote upload, push or deployment occurred.

## Reproduction commands

Use the repository-required Godot executable via `GODOT`; use writable XDG data/config/cache directories. `GODOT=<executable> bash docs/work-log/shop-amount-centering/evidence/run-gui.sh` runs the final GUI suite. The headless amount test is `--headless --path PerchSort --audio-driver Dummy -s res://tests/run_shop_amounts.gd`. The two standalone image probes are `evidence/probe_original_image.gd` and `evidence/probe_image.gd`, invoked with `--headless --path PerchSort -s <absolute probe path>`.
