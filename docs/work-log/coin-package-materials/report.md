# Coin package materials delivery

Implemented the authorized presentation repair in the existing canonical coin-shop UI shell.
User runtime acceptance is pending.

## Changes

- Removed all six 480 × 448 cream package backgrounds. The existing two-column, three-row groups are transparent.
- Reused the six independent feather-coin illustrations, increasing their display heights from the former 208 pixels to 232–288 pixels. Piles, pouch, basket and treasure chests retain their distinct shapes and natural aspect ratios.
- Added a small 344 × 101 warm-wood support behind each painted amount using the existing `panel_shop_sign`. Cream-white digits sit on the wood; the sea remains visible around every treasure.
- Kept all six green yuan-price actions at 336 × 112. Their final row ends at design y 1784. Existing pressed dimming, tap sound, page fade and simulation modal remain in place.
- Preserved the six package dictionaries, `setup(balance)`, `closed`, `purchase_requested`, `show_preview_purchase` and all `preview()` states. There are no payment calls, coin grants or profile writes.

## Owned files

- `PerchSort/game/coin_shop_view.gd`
- `docs/design/coin-shop.md`
- `docs/work-log/coin-package-materials/spec.md`
- `docs/work-log/coin-package-materials/report.md`

Shared helpers, locale manifests, source artwork, navigation, other page edits and existing uncommitted work were not modified. No shared-file edit or lock was required. No shared-helper follow-up is requested.

## Inspection and acceptance boundary

Read the project instructions, shared dispatch, owning script, current helper signatures and screen documents using `cat`, `sed` and `rg`; inspected initial working-tree state with `git status --short`. Viewed existing production PNGs and read their dimensions/alpha bounds with Pillow through `uv run --offline --no-project --with pillow python`. These were read-only asset inspections; no image files were written. The initial direct `python3` inspection could not import Pillow and was repeated using the existing offline environment.

No tests, naming guards, lint, builds, exports, game launches, screenshots, UI automation, motion captures or temporary validation programs were run. No Git commit, branch operation, release or deployment was performed.

Runtime appearance, contrast, overlap, input, localization, animation timing, music/mute behavior and device scaling are **NOT ASSESSED**. The user can review the changed page through its existing coin-counter entry and the existing `packs`, `empty_balance`, `max_balance` and `purchase_preview` states. Source geometry and asset inspection do not constitute runtime approval.

## User screenshot follow-up

The supplied screenshot showed tiny yuan symbols and two detached green fragments to the right of the small chest. Read-only asset inspection located the yuan's painted bounds at `(277, 47, 229, 253)` within a 768 × 342 canvas, and confirmed that the chest fragments occupy x 505–511 while the chest itself ends at x 410.

The price layout now sizes the yuan from its explicit painted bounds. A page-local package-picture helper excludes the small chest's rightmost 32 source pixels with an `AtlasTexture`, preserving the chest's original scale and position. Both the package list and simulation modal use this helper. Source PNGs and shared helpers remain unchanged.

Only the user's supplied screenshot and existing source assets were inspected. No game launch, capture, test, lint or build was performed; the revised result awaits the user's runtime retry.
