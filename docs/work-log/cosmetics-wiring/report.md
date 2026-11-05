# Cosmetics wiring report

## Coordinator ring correction

Merged `origin/final-integration` first, fast-forwarding to `3a466e00`. The only subsequent production-code change is the outfit-card equipped ring diameter: **410 to 360 design pixels**, still centered at portrait-local `(227.5, 235)`. The house-card ring remains 320 pixels. Portraits, cards, prices, layout, equipment, persistence, and tests are unchanged by this correction.

Current verification on the merged code and corrected ring:

| Command | Result |
| --- | --- |
| `env PYTHONDONTWRITEBYTECODE=1 python3 ci/check_current_game_names.py --self-test` | 7/9 passed; 2 failures in merged naming-policy self-tests |
| `env PYTHONDONTWRITEBYTECODE=1 python3 ci/check_current_game_names.py` | 1,820 violations in merged repository content outside the ring correction |
| `flock /tmp/unity.lock bash scripts/run_tests.sh` | Scene build passed; **317/317 EditMode and 87/87 PlayMode passed**, 0 failures, 0 skips |
| Graphics-enabled PlayMode, filtered to `CosmeticsTests.OutfitSelectionIsPerSpeciesAndDetailToggleRestoresAnimatedDefault` | **1/1 passed**, 0 skips |

LevelsData retains all 81 cases and Layout retains all 41. The prior PlayMode failures described in the original delivery below no longer occur on the merged integration. Naming-policy failures were reported, not fixed or bypassed; no unowned files were edited.

Graphics command:

```bash
flock /tmp/unity.lock env \
  PERCH_COSMETICS_EVIDENCE=/Users/apple/orca/workspaces/game/cosmetics-wiring/build/ring-evidence \
  /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -projectPath /Users/apple/orca/workspaces/game/cosmetics-wiring/PerchSort \
  -runTests -testPlatform PlayMode \
  -testFilter PerchSort.PlayModeTests.CosmeticsTests.OutfitSelectionIsPerSpeciesAndDetailToggleRestoresAnimatedDefault \
  -testResults /Users/apple/orca/workspaces/game/cosmetics-wiring/build/ring-results.xml \
  -screen-width 1080 -screen-height 1920 \
  -logFile /Users/apple/orca/workspaces/game/cosmetics-wiring/build/ring-evidence.log
```

Inspected the actual runtime screenshot: the smaller gold ring hugs the equipped portrait area rather than nearly filling the card. Updated only `evidence/ui_shop_outfits.png`, its reference comparison, and `evidence/verification.json`; all other committed visual evidence is retained. Test/capture outputs remain under ignored `build/`. Generated scene and import-metadata changes were inspected and restored to the clean post-merge starting state.

The original implementation record below retains the initial delivery's context. The current results above supersede its test counts, naming results, and outstanding PlayMode failures.

## Scope

Implemented the assigned specification in `ShopView.cs`, `AviaryView.cs`, `BirdhouseView.cs`, small transaction hooks in `GameRoot.cs`, and new `Tests/PlayMode/CosmeticsTests.cs` with its Unity metadata. This report and the requested evidence are the only additional authored paths.

Meta, board bird rendering, rules, levels, resources, storage keys, schema values, and notification SaveData fields are unchanged.

## Behavior

- Shop: all three tabs at the specified centers; a scrollable two-column catalog of all 27 outfits in Meta's aviary/species order, excluding crow, with tiers in order; four birdhouse cards in catalog order. Card sizes are 455 by 575 and 455 by 415 respectively. Outfit sprite rectangles are 380 pixels.
- Prices come from Meta: outfit tiers cost 300, 500, and 800; houses cost 600. Owned cards show green tick pills; equipped cards show a gold ring.
- Unowned cards open the existing purchase confirmation layout with the cosmetic image. Meta validates purchases. The profile is saved immediately. Insufficient coins reuse the booster red-price/header shake, without ownership, equipment, or balance changes.
- Owned-card taps equip; tapping the equipped card resets to default. Outfit selection is independent per species, and only one house style is equipped globally.
- Aviary detail: all three actual tiers, owned portraits, locked silhouettes, equipped ring/tick, and a small default-look chip on the left. Equipped-slot taps also reset to default. The equipped full-bird sprite breathes without hopping, frame substitution, or mirroring; default birds retain the existing authored animation.
- Houses: replace only the body sprite and hide the colored roof while a style is equipped. Every existing house updates immediately; new houses read the saved style. Door masks, leaf/open-door parts, resident bird positioning, entry bounds, state, and delivery/undo/redelivery code are retained.
- Board birds never receive outfits.

## Owned-boundary integration

`PopupFlow` has no public ledger-update operation. The GameRoot hook uses its existing `ToText`/`Restore` persistence boundary, replacing only the JSON-encoded `rewardLedger` property. Pending notification fields remain intact; no duplicate SaveData schema, reflection, new key, or Meta edit was introduced. Purchases use the displayed development balance for validation but never mint the development subsidy into real saved coins.

There is no hanger sprite in the supplied resource catalog. The hanger tab is drawn with white UI line segments in the owned ShopView, without adding or editing artwork.

## Verification

All Unity commands use `flock /tmp/unity.lock`; shared-lock waits were allowed to complete. No other worker's Unity process or user Editor was stopped.

| Check | Observed result |
| --- | --- |
| `python3 ci/check_current_game_names.py --self-test` | 8 passed |
| `python3 ci/check_current_game_names.py` | 0 violations |
| `flock /tmp/unity.lock bash scripts/run_tests.sh` | Scene build passed; EditMode 317/317 passed; PlayMode 72/74 passed, 2 retained failures, 0 skips |
| Graphics-enabled serialized PlayMode run filtered to `PerchSort.PlayModeTests.CosmeticsTests` | 4/4 passed, 0 skips |
| `flock /tmp/unity.lock bash scripts/screenshots.sh` | Passed; all 34 required 1080 by 1920 stable captures written |

Graphics-enabled command, executed from the workspace root:

```bash
flock /tmp/unity.lock env \
  PERCH_COSMETICS_EVIDENCE=/Users/apple/orca/workspaces/game/cosmetics-wiring/docs/work-log/cosmetics-wiring/evidence \
  /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -projectPath /Users/apple/orca/workspaces/game/cosmetics-wiring/PerchSort \
  -runTests -testPlatform PlayMode -testFilter PerchSort.PlayModeTests.CosmeticsTests \
  -testResults /Users/apple/orca/workspaces/game/cosmetics-wiring/build/cosmetics-results.xml \
  -screen-width 1080 -screen-height 1920 \
  -logFile /Users/apple/orca/workspaces/game/cosmetics-wiring/build/cosmetics-evidence.log
```


EditMode discovery is 250 game, 53 Meta, and 14 Audio cases. The retained LevelsData 81 cases and Layout 41 cases all passed. All 70 existing PlayMode cases remain discovered; four new cosmetic behavior tests bring the total to 74. No safeguard, existing test, assertion, skip, or fixture was removed or weakened.

The new tests exercise exact-price purchase and immediate persistence; house-style equip/reset persistence across reload; per-species outfit replacement and toggle/reset; default-chip reset; insufficient-funds and unowned-equipment rejection; every style on every goal house; delivery, undo and redelivery; unchanged board sprites; and a three-second real-time outfit breathing sequence with fixed position and sprite.

Initial implementation verification found and corrected an ambiguous SafeArea type in the capture helper, a funded-fixture reload being overwritten by the outgoing scene's save, and an exact floating-point door-point comparison affected by camera/canvas rescaling. The geometric assertion now requires less than 0.01 design-pixel displacement. No production behavior was special-cased for tests.

Full local logs and XML remain under ignored `build/`: `scene-build.log`, `test-results.xml`, `playmode-results.xml`, `cosmetics-results.xml`, `cosmetics-evidence.log`, and `screenshots.log`. The evidence summary retains exact test outcomes and failure details.

The pre-Unity workspace was clean. After verification, inspected and restored generated-only scene IDs/InputSystem references, import-metadata serialization, and naming-guard bytecode caches to their recorded starting state. No generated scene, resource metadata, settings, package, or cache changes are included in delivery.


## Evidence

Under [evidence/](evidence/):

- `ui_shop_outfits.png`, `ui_aviary_detail.png`: actual Unity runtime captures; comparison images place each beside its approved reference.
- `shop-outfits-bottom.png`: terminal catalog row, including the final Hoot tier.
- `shop-birdhouses.png`, `shop-outfit-confirm.png`, `shop-house-confirm.png`: house catalog and actual purchase dialogs.
- `aviary-default.png`: default animated look restored after toggling the equipped slot.
- `outfit-breath/` and `outfit-breath.tsv`: twelve timed frames and measured scale/position/sprite observations; equipped outfits breathe without hops or mirroring.
  The final measured scale range is 1.00008 to 1.01971; every sample retains wrapper position `(540, -850)` and sprite `berry_outfit_2`.
- `houses-default.png`, `houses-<style>.png`, `houses-<style>-delivered.png`: all four styles on the same runtime goals and readable delivered species.
- `house-aperture-overlays.png`: default/style authored images and 50 percent overlays for all four styles.
- `house-runtime-overlays.png`: runtime default/style delivery crops and 50 percent overlays, including the peeking bird.

## Requirements impossible within the assigned ownership

1. **Full-suite green:** two retained tests outside the owned files fail:
   - `GameplayTests.SaplingIntroPersistsOnShowAndBranchTapDismissesWithoutSpending`, `GameplayTests.cs:324`: expects `(720, -1802)` but runtime returns `(720, -1764.80)`. The unmodified `TutorialHand.PlaceHand` clamps the hand's lower edge to `-1920 + 0.97 * 160`, which is `-1764.8`.
   - `IllegalMoveFeedbackTests.MissingTurnSequencesUseExistingBlinkAndInstantSettlesImmediately`, `IllegalMoveFeedbackTests.cs:158`: expects no `mint_turn_left_01`, but the supplied resource exists. Both left/right frame assets are already present in the branch's starting commit, confirmed with `git cat-file -e`.
   Fixing or deleting these retained expectations would require unowned files and was not attempted. These failures are reported, not suppressed. No separate baseline Unity run was performed.
2. **Literally unchanged total test counts:** the specification also requires a new PlayMode test file. Adding four behavioral cases necessarily changes PlayMode discovery from 70 to 74. Existing discovery is unchanged. Historical documentation's 304/21 figures do not describe the supplied source inventory.

## Visual and verification boundaries

The specification's prices, species/tier ordering, actual supplied outfit sprites, all three tier slots, equipped ring, and default chip differ from the illustrative reference pictures. Runtime comparisons preserve those differences rather than changing approved art, Meta, or catalog semantics. No device/touch-input, audio, or native-window manual acceptance claim is made; the evidence is real graphics-enabled Unity runtime output and timed observations.
