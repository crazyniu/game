# Compact purchase dialog verification

## Problem and scope

The owner identified excessive width and empty space below the coin reward. The shared shop purchase shell reserved 960 px width and 168 px footer for every content shape. This patch targets only that purchase shell and its three callers. Jungle/result, family, settings and general Modal sizing are not globally changed; those require their own content review.

## Changes

- Currency, nest and outfit purchase cards: 840 px wide, 72 px footer.
- Booster purchase cards with description and quantity controls: 880 px wide, 88 px footer.
- Header 236 → 220 px; centered painted-title bounds 640 → 540 px to keep long English names clear of the close hit area.
- Exterior action gap 48 → 32 px only for this shell; callers read the shell's `actions_y`.
- Existing button dimensions (including the corrected 552 × 136 currency button), painted artwork, product sizes, quantity controls, balances, prices and callbacks are preserved. No payment or game logic changed.

## Verification

Godot 4.7.2:

- `run_purchase_surface.gd`: 328 checks, 0 failures, 28 states. Added content-classified card widths, card centers, action spacing/margins and footer clearance checks. Includes all currency packs, maximum/multiple quantity and insufficient balances in both locales.
- `run_purchase_close.gd`: 2892 checks, 0 failures. Real pointer dispatch at center and four edges, single-close semantics and visible title clearance. Every catalog title is additionally checked against the narrower shop shell in both locales. The first narrower-shell run exposed 16 long-English-title collisions; those were corrected by the title bounds change and the entire suite rerun.
- `run_single_card_actions.gd`: 436 checks, 0 failures. Painted-word/price centering, unchanged action heights, new-edge pointer dispatch and mock currency callback identity. No real payment attempted.
- `scripts/run_tests.sh`: exit 0, including smoke/core/goal and naming/painted-word guards. Expected negative-test diagnostic errors are present in core logs; no failed checks.
- Naming self-test: 5 passed; source scan: 176 files, 0 violations.
- `git diff --check`: clean.

Captured and inspected 18 actual cloud-desktop OpenGL screenshots, nine purchase states in each of English and Chinese, including insufficient, multiple and maximum states. Checked currency, outfit, nest and booster screenshots at full size in addition to contact sheets. Content and buttons are centered, narrower frames retain all quantities/descriptions, and bottom whitespace is reduced without cropping. Capture/log paths: ignored `build/compact-purchases/`; no evidence binaries are committed.

No merge or deployment performed by this worker. This is purchase-family coverage, not a claim that all dialogs in the game have been resized or fully reviewed.

Independent reviewer checked d8065c6 and actual English quantity/nest captures; no blocking issue found and no file conflict with the parallel result-dialog change.
