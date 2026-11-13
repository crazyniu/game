# Non-gameplay navigation report

Status: locally integrated and verified. No commit, upload, push or login was started.
All scoped page/dialog owners' changes are integrated; see `../non-gameplay-ui/report.md`.

## Implementation

`MenuNavigation` is explicit and opt-in. Close uses the existing accepted coin-shop
`btn_purchase_close` pixels. Back uses the existing accepted glossy orange/white-arrow
`btn_forest_back` pixels. The source PNGs were visually inspected and hash-recorded before
wiring; none were redrawn, cleaned, overwritten or removed.

Actual game-space controls match the coin shop's original 116 x 119 bounds times its
1920/1717 canvas scale. Local coordinates account for that canvas scale, so main Draw and
ordinary full-space pages have the same displayed size. Original callbacks and accepted
press tint are retained. Modal opt-in replacement retains the original Close center.

The integration owner updated Map Back, Aviary list Close/detail Back, main Draw Close,
Chest Close and the preview-only document/chapter Close. Shop, purchase, draw-result and
other task dialog owners use the same helper in their own isolated changes. No global Modal
reskin was performed. Finalized Home/Settings, shared Modal, main controller and BoardView
are frozen by source checks. Tutorial/lesson changes remain outside this navigation edit.

## Final validation

Godot 4.7.2 headless and OpenGL graphical navigation checks pass 107 assertions across Chinese and English:
reference art identity, equal game-space size, press/release tint with unchanged bounds,
Map trail → same selector → Home, Aviary detail → collection → Home, Shop and coin-shop dismissal,
Chest/Draw/result/document/chapter closes, repeated Close and no profile mutation.

Command: `Godot_v4.7.2-stable_linux.x86_64 --headless --path PerchSort --script tests/run_menu_navigation.gd`.
The test uses fake state and writable isolated XDG directories. Engine-dispatched input
checks and native desktop pointer checks are reported separately in the combined report.
Test entry avoids pre-autoload compilation of UI classes.

`python3 docs/work-log/menu-navigation/evidence/check_sources.py` verifies all six frozen
source records and both retained navigation PNG hashes. `git diff --check` passes.
Original focused/source logs are in `evidence/`; final 107-check logs are in
`../non-gameplay-ui/evidence/`. The final graphical navigation harness has a known
2 ObjectDB / 1-resource teardown diagnostic after its zero-failure summary. It is not
represented as a clean diagnostic-free exit.

Combined aggregate checks, final helper consumers, bilingual captures and native return
loops passed. Preview chapter pagination now uses existing words, and the current Map
marker uses local sibling order below the level card. These issues and their original
failure evidence are documented in the combined report. No approved-runtime baseline,
gameplay or save schema changed.
