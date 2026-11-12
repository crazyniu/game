# Menu navigation

The user requested Close and Back on non-gameplay pages to follow the current coin-shop
button family: a glossy orange/red rounded face with a white mark. This is an explicit
opt-in presentation change, not a global replacement of shared Modal controls.

## Accepted materials

- Close: existing `btn_purchase_close`, exactly the coin-shop sprite.
- Back: existing `btn_forest_back`, the accepted glossy orange/white-arrow sprite.

Retain both PNGs unchanged, including their original margins and painted shadows. Do not
redraw the accepted marks, replace Back with an X, or manufacture substitute artwork.
Source dimensions and checksums are in `docs/work-log/menu-navigation/assets.json`.

## Geometry and interaction

`MenuNavigation.SIZE` is the original coin-shop control's 116 x 119 source-canvas bounds
multiplied by `DrawPurchaseArt.SCALE` (1920/1717): approximately 129.7 x 133.1 game pixels.
Every participating control uses that same actual size. Callers inside the scaled draw /
purchase canvas pass its coordinate scale, preserving the existing canvas geometry.
Full-page controls keep their established top-left position. An existing modal Close keeps
its center when opting in through `MenuNavigation.replace_close`.

Press feedback uses the same tint as the coin shop and never changes size or position.
Callbacks preserve navigation destinations, selected worlds, scroll state and one-shot
modal completion. Disabled controls retain their bounds. Words and locale choices do not
change icon identity or geometry.

## Scope

Apply to the current Shop, Aviary, Map, main Draw, Chest, purchase dialogs, Draw results and
other explicitly revised non-gameplay dialogs. Preview-only documents/chapters also opt in.
A dialog that previously exited only through an action does not gain an extra Close path.

Finalized Home and Settings remain unchanged. Keep shared Modal, BoardView and the main
controller unchanged; opt-in helpers prevent accidental propagation into gameplay. The
Help tutorial shares its implementation with first-level teaching, and mechanic lessons are
part of that protected surface, so `tutorial.gd` is intentionally unchanged in this task.

Validate real button appearance and return loops after all contributors are integrated.
Source hashes, per-page geometry tests and headless passes complement, rather than replace,
actual graphical review. No approved-runtime baseline is written automatically.
