# Shop purchase quantity delivery

## Delivered

- Recorded the user-approved arrangement and current style in `spec.md`: 864-pixel cream modal, smaller item picture, editable quantity between matching minus/plus controls, and an orange Buy bar with the total inside the panel.
- Added `shop.buy: Buy` to `tools/kit/locales/en.json`.
- Produced `PerchSort/art/locale/en/label_shop_buy.png` using the existing offline lettering pipeline and registered it in the existing locale manifest. Existing images were not regenerated.
- Updated the canonical `PerchSort/game/shop_view.gd` purchase popup in place. Booster illustrations now use a 240-pixel slot; the quantity row and orange Buy bar sit inside the existing cream modal. Single-ownership goods retain their 320-pixel picture slot and omit the quantity row. Equipped check badges remain status indicators.
- Added integer input, matching 128-pixel minus/plus controls, immediate total-price updates, and disabled states at quantity/affordability limits. The input uses LineEdit for editing and the existing painted numeral sprites for all visible digits, with matching selection and caret overlays. Enter finishes editing without purchasing. Empty input disables Buy until editing is completed; focus loss restores quantity 1.
- Extended `PerchSort/meta/shop.gd` purchase with an optional quantity argument. It checks the full price before deducting coins and adding all purchased units in one call. Existing callers still buy one catalog entry; package prices, discounts, unlocks, outfit/house ownership and saved field names are unchanged.
- Kept existing modal transitions, press feedback, sound and save notifications. The existing modal action guard dispatches the purchase once during closing. Added `confirm_multiple` and `confirm_maximum` to the existing fake-profile preview interface.

Asset production used `DYLD_LIBRARY_PATH=/opt/homebrew/opt/fribidi/lib uv run --no-project --isolated --with pillow python` with an inline script importing `tools/kit/build_localized_labels.py`, loading the catalog's weight-800 font, calling its `render` function for `shop.buy` only, and appending that label's existing-format metadata to the manifest. The command completed successfully. This was asset production, not a game build or validation run.

## Authority and acceptance

After the AGENTS.md role boundary was explained, the user explicitly authorized direct shop-code changes. This is a task-scoped exception; project agent instructions were not changed. Source inspection was used to preserve the existing style and call paths. LineEdit input properties were referenced from the [official Godot documentation](https://docs.godotengine.org/en/stable/classes/class_lineedit.html).

Tests, lint, builds, screenshots, UI automation and temporary validation programs were not run. Runtime layout, keyboard behavior, visual alignment and purchase behavior await user acceptance. Existing uncommitted project work was preserved. No commit, push, package or release was made.
