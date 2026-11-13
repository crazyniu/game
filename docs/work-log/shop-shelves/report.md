# Shop shelves recovery

Reconstructed the approved shelf layout and subsequent owner corrections in the canonical shop_view.gd. No game rules, inventory, catalog entries, prices, save data or unlock/equip callbacks were changed.

## Files and assets

- game/shop_view.gd: single-column long lists; fixed icon-only tabs; flat translucent row surfaces; solid circular stock badges with explicitly requested plain numerals; shared feather prices; centered actions; measured house contours resting on shelves.
- tests/run_shop_cards.gd: replaces obsolete two-column layout expectations with current approved shelf geometry and adds vertical alignment/plain-stock checks. Economic assertions remain unchanged.
- docs/design/shop-shelves.md: recovered measured composition and asset inventory.

Existing clean shelf, sign, chevron, boosters, houses and birds were reused. No approved art was overwritten, no design images were sliced. Shared feather/button changes belong to the shared-button worker.

## Verification in this run

- Godot 4.7.2 headless `--script res://tests/run_shop_cards.gd`: 34 checks passed. Covers long/future catalogs, cache heights, purchases, nest equip, drag protection, locale and asynchronous warmup. Exit cleanup reports two retained ObjectDB instances and one resource; not hidden as a clean shutdown.
- Godot 4.7.2 OpenGL Xvfb actual 1080x1920 window `--script res://tests/run_shop_cards_gui.gd`: 9 pointer/scroll/navigation checks passed, no script errors. Screens captured in `build/shop-shelves/screens/`, English and Chinese, top/bottom list states.
- `python3 ci/check_painted_words.py`: 0 problems.
- `git diff --check` for owned code/tests: clean.

Visual inspection confirmed shelf contact for all nine houses, centered price/name rows, readable 52px solid stock badges and fixed three tabs. No device, animation-timing or audio claim is made from these still images. The coordinator owns full integration and final visual review. No commit or push performed by this worker.

## Latest correction: remove booster badges

The owner subsequently removed badges from shop game tools. Stock now appears as plain white ×quantity under each product name, with no backing or circular surface. The existing zero visibility and 99 display cap remain unchanged. Names and quantities form an information block; purchase actions remain vertically centered. Label bounds are 340 × 52 to accommodate the actual 34px font line height without overflow.

Updated real GUI test covers English/Chinese zero and 5000 inventory, asserting ×0/×99 and a text-only stock control. Final captures include boosters_empty_en/zh and boosters_maximum_en/zh. This run has 17 real input/stock assertions and 35 structural/economy assertions passing.

run_shop_amounts.gd was updated to the shared FeatherPrice/Count structure. It independently measures the visible currency-and-numeral union, inside-group and action centering (2px optical tolerance), nonoverlap, compact spacing, exact catalog/quantity totals, exact stock values, locale/balance rebuilds and original digit-image hashes. Final result: 2010 checks, 0 failures, no SCRIPT ERROR or ERROR. This supersedes old tests requiring unchanged numeral spacing and plain Digits hierarchy. The updated test requires feather groups on Aviary purchases too.
