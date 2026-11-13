# Purchase Close outside the frame

## Implemented change

Only `PurchaseDialog.open` changes production layout. It retains `MenuNavigation.replace_close`'s existing button, then sets its Y coordinate to `panel.top - Modal.ACTION_DROP - close.height`. The existing Close artwork, 129.715×133.069 design-pixel size, X coordinate, press effect and callback remain unchanged. The complete button and hit region move together.

All four purchase families are covered: Shop booster, Shop nest, Aviary outfit confirmation and CoinShop Preview. The Close bottom is exactly 48px above its unchanged frame, with the original 31.143px right-screen margin. New Y coordinates are 404.931, 512.931, 486.931 and 454.931 respectively. This is a 130.535px upward translation from the former corner-overlapping placement.

No shared Modal/MenuNavigation, Shop/Aviary/CoinShop page, Buy layout, numeric renderer, asset, full-screen Draw result, economy or save logic was changed. The isolated snapshot includes the coordinator's current Family dependency only for testing; that file is excluded from the delivery patch.

## Verification

Godot 4.7.2, GL Compatibility:

- `tests/run_purchase_close.gd`: 1676 checks passed headless and 1676 passed on actual desktop OpenGL. Checks exact 48px clearance, full viewport containment, retained size/art/X position/pressed geometry and visible title alpha bounds for all catalog item headings in English and Chinese. Actual center and all four edge pointer clicks, including double clicks, close exactly once with unchanged profile data and displayed Preview balance.
- All 28 standard states captured at 1080×1920: eight Shop/Aviary purchase states and six Preview packs, each in both languages. All four contact sheets were visually reviewed; Close is clearly outside each frame, does not clip or collide with the title, and retains its original art.
- `run_purchase_buttons.gd`: 128 checks passed headless and actual OpenGL, preserving Buy placement, prices, quantities, affordability and real pointer callbacks.
- `run_purchase_dialogs.gd`: 98 headless checks passed, including QuantityInput focus/caret/selection and all Preview packs.
- `run_shop_amounts.gd`: 1338 headless checks passed, preserving numeric centering and source Image bytes.
- Aggregate `scripts/run_tests.sh` passed: naming self-tests, 164 tracked-source naming guard, painted-word guard, smoke and all 100 core levels (1606 ms).
- No SCRIPT ERROR, ERROR or failed assertions in the final logs; `git diff --check` passes.

`capture-manifest.json` records hashes and dimensions. Raw PNGs are kept under ignored `build/purchase-close-placement/` to avoid duplicating large tracked evidence; contact sheets, logs, numerical results and source hashes are in this evidence directory. Reproduce the actual GUI run with `GODOT=<required executable> bash docs/work-log/purchase-close-placement/evidence/run-gui.sh`.

The temporary test window closed normally and the pre-existing game window was untouched. No physical-device, audio-listening or new animation-timing claim is made. No upload, push or deployment occurred.
