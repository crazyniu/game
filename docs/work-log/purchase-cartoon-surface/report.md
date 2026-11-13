# Purchase cartoon container report

## Scope and material gate

The purchase-specific container opts into `DialogCartoonSkin.apply` after its existing title and Close setup. Production ownership is one call and the helper comment in `purchase_dialog.gd`; no purchasing logic changed. The Family owner delivers the four frozen helper/panel dependency files separately. The independent asset was inspected before implementation and its helper and PNG SHA256 values matched the supplied material gate.

The accepted forest background is reused unchanged. The panel now uses the new bright orange/gold rim and saturated green center. Shared ShopSkin/Modal, inventory screens, Sunny detail, accepted art bytes and the full-screen Draw result are untouched. Outside Close clearance, outside Buy/Confirm, title lettering/size, numeric ink alignment, quantities, prices and callbacks are preserved.

## Geometry and behavior evidence

`run_purchase_surface.gd` records the pre-material layout and checks all 28 canonical purchase states: eight Shop/Aviary states and six feather packs in both English and Chinese. It compares all 548 non-Panel Control nodes' position, size, scale, pivot and rotation, plus the panel rectangle, with the recorded original geometry. It also verifies the frozen panel texture/crop, nine-patch dimensions, original background image, transparent dim and pointer-transparent surface nodes.

The Close test now accepts an optional language filter so each native screenshot process captures only 14 states; its default behavior and assertions are unchanged. The existing Close test covers all 28 state captures, every catalog title in both languages, and center/four-edge double pointer dispatch with exactly one close callback and no profile mutation. The existing purchase and Buy suites retain quantity editing, disabled/insufficient funds, transactions, balance and geometry assertions. The number-centering suite remains unchanged.

Headless results: surface 252/0 across 28 states; Close 1676/0; Buy 128/0; purchase dialog 98/0; amount centering 1338/0. Aggregate naming, painted words, smoke and all 100 core levels passed; core replay took 1561 ms. Final headless logs contain no script errors, errors, failed assertions or warnings. Actual OpenGL results: surface 252/0, Close 838/0 per language (1676 combined), Buy 128/0, purchase 98/0. All 28 full-resolution 1080x1920 state images were captured. The 548 non-Panel Controls match the pre-material geometry exactly, both headless and on OpenGL. The native terminal explicitly reached `PURCHASE_CARTOON_GUI_COMPLETE`, all processes exited normally and the shared desktop was released. Native logs contain only the environment warning that V-Sync mode cannot be changed; there are no script errors, errors, failed assertions or resource-leak diagnostics.

All four contact sheets were visually inspected. Booster single/multiple/maximum/insufficient, nest funded/short, outfit funded/short and all six preview packs in both languages retain legible wording, prices and centered art. The bright forest backdrop and rounded orange-gold panel replace the rejected dark presentation without clipping the Close, Buy/Confirm, title or contents. The unchanged title overlaps the top rim intentionally according to its existing layout; Close and bottom actions retain their verified separate outside clearance. The new skin does not prove any new animation or sound acceptance. No physical-device test or user visual acceptance is claimed.

Concurrent environment load temporarily slowed the final two CPU suites and lightweight filesystem queries. Existing runs were retained without restarting or stacking additional tests; both completed successfully. GUI work is coordinated and runs serially after Family releases the desktop.

## Diagnostic history

Two test-harness issues were found and corrected before final acceptance. The first harness directly referenced the global skin class before autoload initialization; the resulting compile error is retained in `surface-initial-harness-error.log`. The harness now loads that script after the main scene is ready. A second probe used exact equality for floating-point nine-patch size multiplication; its 12 preview failures are retained in `surface-initial-float-equality.log`. That assertion now uses Godot's approximate vector comparison. Neither issue required any change to the frozen material helper or to production behavior. Exit status alone is not accepted: final logs are also scanned for script errors, errors and failed assertions.

## Reproduction and handoff

Use the repository-required Godot 4.7.2 binary via the `GODOT` environment variable and writable XDG directories. Run `run_purchase_surface.gd` with `--surface-baseline=<evidence>/baseline-geometry.json` and `--surface-result=<output.json>`, followed by the unchanged Close/Buy/dialog/amount suites and `scripts/run_tests.sh`. `evidence/run-gui.sh` runs the actual OpenGL surface, Close, Buy and purchase suites from the native desktop. It saves full-resolution raw frames in `build/purchase-cartoon-surface`, keeping compact contact sheets, logs, measurements and a manifest in this report folder.

No remote publication, upload, release, gameplay change or user acceptance is claimed.
