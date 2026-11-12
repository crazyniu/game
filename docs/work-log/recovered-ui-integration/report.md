# Recovered UI integration QA

Base 522065bf; recovery reconstructed the approved presentation rather than retrieving the lost uncommitted checkout byte-for-byte. No publish, force-push, merge or deployment.

## Review and fixes

Shared button faces are Control wrappers. Tutorial styling does not cast them to NinePatchRect and uses canonical painted Play/Next/Got It words. Reviewed actual tutorial, shop houses and boosters screenshots. Corrected remaining aviary price presentation: both outfit tiles and purchase confirmation now use standalone golden feather plus centered white exact price; original purchase, affordability and shake callbacks remain unchanged.

## Verification

Integrated Godot 4.7.2: naming/painted guard and smoke pass; 100 core levels pass; 72 gameplay dialog checks, 2306 family dialog checks, 96 feather checks, 128 purchase-button checks, 98 purchase-dialog checks, 353 draw-result checks, 180 draw-purchase checks pass. Shop structural and real pointer/scroll GUI tests pass. Current run logs and actual screenshots are under `build/integrated-qa/`.

The historical shop amount test still assumed numeral-only nodes and invariant spacing; its first run failed with missing-node errors and 8 failed assertions. A scoped update is in progress to validate the approved feather grouping and flat inventory counts while retaining exact model amounts, layout boundaries, locale rebuilding and source hash checks. This failure is not reported as a production failure or as a pass.

GUI/headless draw and some shop tests retain ObjectDB/resource-in-use warnings at shutdown. No mobile-device, audio-listening or animation-timing acceptance claim is made.

Latest board toolbar follow-up and final restore-verified backup are pending at this point.

Final amount regression update was independently rerun against the integrated checkout: 2010 checks, zero failures, including the aviary FeatherPrice paths. The obsolete node/spacing failure above is resolved. Latest shop stock display is plain white ×count below the name, with no badge/background; source references catalog/profile values unchanged.

## Final frozen-source verification

After the last toolbar/readability correction, reran the integrated naming, painted-word, smoke and all 100 core routes: pass. Tool inventory GUI pass at 1080×1920, 720×1280 and 390×844, including zero-stock lower plus opening the existing purchase action exactly once. Latest lower counts are ordinary 48px white text with dark-green outline, no badge/background; all seven full-item hit targets remain. Actual small-screen capture reviewed for count/plus visibility. Shop cards 35, real GUI 17 and strict amount regression 2010 checks pass after removing shop badges. No core/meta/level files changed.

Final evidence: `build/integrated-qa/inventory_*.png`, `shop_view__boosters.png`, `shop_view__houses.png`, `tutorial__tutorial_4.png`, `popups__lucky_draw_full.png`, and `aviary_buy_outfit.png`/`aviary_buy_outfit_short.png`. Historical pre-toolbar board captures are not final acceptance references.

## Backup contract

A complete tracked plus non-ignored untracked source snapshot is being created outside the working tree, with baseline commit, changed-path inventory, all source SHA256 hashes, and restore instructions. The backup process extracts it to a fresh directory and compares every file before returning success. Caches, credentials, local game profiles and Git internals are excluded; independent QA screenshots/logs are included. Final archive verification and persistent Library identity are reported separately to the coordinator, rather than modifying this snapshot after hashing.
