# Jungle purchase verification

## Scope and source

Based on main 054d04a. Implemented the approved purchase design via an explicit PurchaseDialog.open_shop entry point. Only shop and aviary callers opt in; recharge, generic dialogs, gameplay and economy are unchanged. The panel source is identical to the coordinated level/result panel; the mockup is a reference only.

## Verification

- Godot 4.7.2 GL Compatibility, 1080 x 1920, Xvfb TCP display with temporary user-data paths.
- Full scripts/run_tests.sh: naming checks, painted word guard, launcher tests, smoke, all 210 stored-solution core checks and bird animation checks passed. Deliberate corrupt-save fixtures log expected errors; core summary reports zero failures.
- run_purchase_dialogs.gd: 106 checks, zero failures; quantity input/caret/selection, limits, buying, closing and unchanged recharge purchase family.
- run_purchase_buttons.gd: 128 checks, zero failures; real pointer hits, disabled price actions, complete on-screen bounds and exterior placement.
- English and Chinese real runtime captures cover single/multiple/maximum quantities, bird outfit and house, sufficient and insufficient funds. All 14 visually reviewed in build/purchase/contact.jpg and individual captures. Evidence remains local and ignored, not pushed.
- run_purchase_close.gd: 1,676 checks, zero failures, including real pointer center/four-edge double clicks and every localized item title clearance.
- The approved small leaf refinement uses the same decor_jungle_leaves source as the coordinated level/result shell; two proportionate sprites, no input interception. All 14 final captures were repeated and reviewed after integration. design_comparison.jpg shows the approved purchase composition beside the real English capture; content grouping, wooden heading and exterior price action match, while original red close artwork is intentionally retained.
- Runtime logs and screenshots live under build/purchase; no generated evidence is committed.

## Boundaries

User runtime visual acceptance is pending. No device test, release, merge, or deployment. Existing price-button art and dimmed disabled behavior remain unchanged. No purchase logic, save format or balance policy was changed.
