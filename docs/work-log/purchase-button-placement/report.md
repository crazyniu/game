# Purchase buttons outside the frame

## Implemented scope

Only the Buy Y-coordinate changes in `Shop._confirm` (booster/nest) and `Aviary._confirm_outfit` (outfit). The complete existing Button node, its painted price, visual face and hit area now sit 48 px below the unchanged panel bottom, using the production `Modal.ACTION_DROP` constant.

Button sizes and horizontal alignment are unchanged. The entire frame, title, hero illustration/live bird, quantity controls and their geometry remain unchanged. Existing affordability/quantity logic, actual purchase/equip callbacks, dismissal and duplicate prevention remain. No approved texture, label, price, gameplay file, shared helper or Purchase Preview code changed.

Two-line production patch: `/workspace/scratch/2bd695cff14d/purchase-buttons-outside.patch`.

## Verified

Using Godot 4.7.2:

- `--headless --path PerchSort --audio-driver Dummy -s res://tests/run_purchase_buttons.gd`: 128 checks, zero failures. Tests all eight relevant states in English and Chinese, exact gap, non-overlapping hit rect, fixed size and center, viewport fit, insufficient-funds behavior, actual viewport pointer hits, cancellation, and exact prices/quantities.
- `--headless --path PerchSort --audio-driver Dummy -s res://tests/run_purchase_dialogs.gd`: 86 checks, zero failures, including actual input focus/caret/selection behavior.
- `GODOT=<4.7.2 binary> bash scripts/run_tests.sh`: naming self-test/guard, painted word guard, smoke and all 100 level core checks passed.
- No SCRIPT ERROR/ERROR/FAIL diagnostics in the final logs; `git diff --check` passes.
- `evidence/source-scope.json` verifies each owned production file differs from its copied input by exactly the specified one-coordinate replacement.

- Actual desktop OpenGL run of `run_purchase_buttons.gd`: 128 checks, zero failures, including pointer press/release delivered through the viewport to each enabled or disabled Buy button.
- All 16 captures are 1080×1920. English and Chinese cover single, multiple and maximum quantity, insufficient funds, nest purchase and outfit purchase. Both contact sheets were visually reviewed: every Buy is clearly outside the frame, fully visible, and retains its existing artwork and text.
- `evidence/capture-manifest.json` records capture hashes and sizes; `evidence/placement-results.json` records panel/button geometry, balances, prices and pass/fail results. Every action gap is exactly 48 px.

The temporary test window closed normally; the pre-existing game window was not changed. No upload, push or deployment occurred.
