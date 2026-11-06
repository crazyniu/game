# Measured mockup layouts

One JSON per approved mockup (`art/ui_design/<screen>.png` on branch `ui-design-review`), written by the conversation that painted the mockup. Canvas 1080 x 1920, origin top-left, pixels. Each element: `id`, `role`, `rect` [x, y, w, h] of visible pixels, `z`, `parent`, `text`, `sprite_hint`, `notes`. `<screen>_overlay.jpg` shows every rect on the mockup.

Rule for UI work: place every runtime element at its measured rect (Unity anchored positions converted from top-left pixels), size it to the rect, and verify with a silent capture: every element's rendered rect must be within 20 px of the measured rect. Numbers and painted words are runtime glyphs/labels sized to the measured text rects.
