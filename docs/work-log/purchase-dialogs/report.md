# Purchase dialogs

## Result

The canonical booster, nest, outfit and feather-purchase-preview dialogs now use the existing green and gold forest materials. The old cream quantity card, wooden item stand and repeated title plaque are gone from these dialogs. Existing independent pictures, labels and numeric glyphs are reused without editing their pixels. Close uses `MenuNavigation` and the accepted coin-shop artwork at the shared dimensions.

Booster quantity editing, clamp/affordability rules, actual prices, inventory, nest equipping and outfit purchase/equip remain on their existing production callbacks. Repeated opens and repeated confirmations cannot create duplicate dialogs or charge twice. The feather preview remains explicitly labelled as a simulation and cannot charge or grant currency.

## Scope

- `shop_view.gd`: purchase constants, nested painted quantity input, `_confirm`, `_purchase_modal`, `_quantity_row`, and nest preview states only.
- `aviary_view.gd`: `_confirm_outfit` only.
- `coin_shop_view.gd`: `show_preview_purchase` only.
- New purchase-only helper, focused tests and documentation.
- No edits to gameplay, pure rules, economy, saved data, source bitmaps, shared modal/style helpers, or Settings.
- Work is local. No push, PR, deployment or upload.

## Checks completed

Environment: Godot 4.7.2, `/workspace/scratch/2bd695cff14d/godot472/Godot_v4.7.2-stable_linux.x86_64`.

- `GODOT=<path> bash scripts/run_tests.sh`: passed naming self-test, naming guard, painted-word guard, script/level smoke, and all 100 core level replays. Logs: `evidence/full-tests.log`.
- `GODOT --headless --path PerchSort --audio-driver Dummy -s res://tests/run_purchase_dialogs.gd`: 86 checks, 0 failures, no runtime diagnostics in the final headless run. Covers quantity editing and limits, empty input, insufficient funds, exact-once purchases, cancel, both languages, existing live BirdView, nest equip, outfit purchase/equip and all six simulated feather packs. Logs: `evidence/focused-tests.log`.
- `git diff --check`: clean.
- Existing asset hashes recorded before implementation in `evidence/asset-validation.json`.

## Runtime visual review

All 18 requested preview captures (nine states in each language) were reviewed at 1080 × 1920. The dark green surface, compact gold title, hero art, quantity controls and gold numeric price are clear; no clipped captions, amount overflow, overlap with Close, or unintended cream card stacks were found. Disabled Buy and +/- retain their geometry. The feather simulation title has a local display-scale correction for its existing source halo; its original pixels remain untouched.

- `evidence/purchase-states-en.jpg` and `purchase-states-zh.jpg`: complete state contact sheets.
- `evidence/purchase-reference-comparison.jpg`: previous purchase composition beside the canonical updated booster dialog. The centered item/caption/quantity/price hierarchy remains; the compact forest material follows the latest direction instead of copying the older cream/plaque treatment.
- `evidence/outfit-motion-contact.jpg`: 16 real frames at nominal 0.25 s intervals over four seconds, showing entrance, breathing, sparkle progression and an actual blink frame. All 15 adjacent frame pairs changed in the hero region; raw frames and per-pair metrics are retained.
- `evidence/capture-manifest.json`: exact dimensions and hashes.
- `evidence/gui-focused.log`: 86 checks, 0 failures under the actual OpenGL renderer, including active quantity input focus/caret/selection in both languages.
- `evidence/pointer-qa.md`: real CUA quantity/keyboard/cancel/reopen/purchase verification.

The installed `xvfb-run` wrapper was unavailable, so captures used the existing cloud desktop through CUA. A test-only `capture_purchase_dialogs.gd` sets the root viewport to 1080 × 1920 before loading the same canonical main scene. This prevents the smaller desktop window from reducing render resolution; no desktop setting or production project setting changed. The normal comparison image was assembled locally from the resulting capture.

The cloud renderer reports only its known unsupported V-Sync warning in screenshots. The initial interactive pointer log contained a glyph-style error described below; the earlier blanket no-script-errors statement was incorrect. Audio listening, mobile device behavior and final user approval are not claimed. Catalog screens behind the dim layer are the isolated pre-integration context; the coordinator must review the final combined pages. No approved runtime baseline has been replaced.


## Focus review correction

Integration review caught five quantity caret metric calls using `"white"`, which is a display color, not a locale glyph-style key. `UiText.number_style(Color.WHITE)` resolves to `"cream"`. Visible purchase behavior still worked, but the initial pointer session emitted dictionary-access errors while the input was focused. The initial focused tests never entered the active editing path, so their passing result did not cover this case.

All five calls now share the production color-to-style resolver. Twelve regression assertions genuinely focus and edit the field, execute `_process`, check its caret and partial/full selection geometry against the visible glyph style, and verify blur stops processing, in both English and Chinese. The revised 86-check headless run passes without script/runtime diagnostics. The complete repository suite was rerun after the fix and passed, with no script/runtime diagnostics. A fresh GUI focused run on the actual OpenGL renderer also passed all 86 checks, with no SCRIPT ERROR/ERROR diagnostics. `evidence/check-focus.sh` fails if such diagnostics are present. The original pointer log remains as truthful diagnostic history.

Lesson recorded for this work: painted number colors must always resolve through the production style mapping; a visible typing result alone does not establish an error-free focus path. Read runtime logs after actual input checks and include focused per-frame execution in the regression test.
