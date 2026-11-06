# Aviary scroll repair

Keep the canonical aviary view and its display-only fixtures. Compare against `origin/ui-design-review:art/ui_design/aviary_list.png` at 1080 by 1920.

- The cream opening contains the viewport and scrollbar. A RectMask2D fades the first and last 24 pixels vertically; the painted arch, sides and bottom bar draw above the cards.
- Keep two columns, overlapping painted name ribbons, homecoming pills and ability badges. The mockup shows rings approximately 340 pixels across; use a 344-pixel diameter. This treats the brief's approximate 170-pixel measurement as a radius to preserve the reference scale.
- Reuse the board artwork through geometric patches; do not repaint assets or introduce font text.
- Capture Reference, Last row, Maximum coins and homecomings, and Mid-scroll through IScreenStates. AudioListener volume is zero and pause is true before initialization or state application. Do not run test suites.
- Commit JPEG captures and equal-scale comparisons, with the mockup on the left. The coordinator's extended brief permits additional silent captures for corrections.

## Detail screen

The coordinator narrowed the assignment to the aviary list and detail only. Compare the detail against `aviary_detail.png` from the same approved branch. Keep the front-facing animated bird, three visible outfit cards, a selected check, locked silhouettes, the ability row and the homecoming pill. Keep all four canonical outfit choices reachable through a horizontally clipped row. Long ability captions use a full-width painted plate below their badge, preserving the supplied lettering's aspect ratio. Capture the reference, equipped, locked, maximum, Flurry and last-outfit states.
