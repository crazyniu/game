# Play buttons report

The level card, next-level win action and world-unlock action now share a green 360x128 rounded Play button through `UI.button`. The existing green palette, dark lower edge, shadow, sound and 6-pixel press feedback are retained. The triangle is replaced by one centered white Play label.

`Modal.actions` lays out actual widths and heights. The win row keeps its orange replay/home discs, 56-pixel gaps and a total width of 728 pixels. The three popup callers reserve the new 128-pixel action height. Their callbacks, disabled states and close handling are unchanged.

Added `action.play` to the English source catalog and generated only `PerchSort/art/locale/en/label_action_play.png` using the existing offline lettering pipeline and bundled Nunito font. Updated the locale manifest without regenerating other textures. Runtime still renders whole-phrase textures, not fonts.

Production asset generation used `DYLD_LIBRARY_PATH=/opt/homebrew/opt/fribidi/lib uv run --no-project --isolated --with pillow python` to call the existing pipeline's `load_font` and `render` functions for this label. It completed successfully.

No tests, lint, game builds, game launch, UI automation or screenshots were run. Visual acceptance remains with the user.
