# Shop and aviary implementation report

## Delivery status

Implemented the owned runtime and test files. Functional acceptance passes in the six new PlayMode cases. Full-suite and exact visual acceptance have the explicit integration boundaries below; this report does not claim that the supplied concept images are pixel-matched.

## Implementation

- `PerchSort/Assets/Scripts/Game/ShopView.cs`: full-screen noon-forest shop, reserved three-tab layout with only boosters visible, six single-unit catalog cards, owned counts, runtime greyscale locked cards, unlock chips, shake feedback, affordability feedback, confirmation panel, purchase countdown and badge pop. Prices and transactions use `Meta.Shop.Catalog` and `Meta.Shop.Purchase`; no bundle UI.
- `PerchSort/Assets/Scripts/Game/AviaryView.cs`: ten species in Meta order, met/unmet portraits, runtime solid silhouettes, stored homecoming counts, scroll-preserving list/detail navigation, exact requested personality/gift sentences, default/locked outfit slots, authored cheer sequences through `UiBirdLife`, continuous breathing and a hop every 2.5 seconds. No sprite mirroring.
- `GameRoot.cs`: small integration blocks for page ownership, modal input, zero-stock routing, Meta purchase validation and the existing immediate save path. Closing the shop does not recreate the session or board. Core and Meta behavior and persistence keys are unchanged.
- `HudView.cs`: white geometric plus icon in the existing red badge at zero stock; positive stock retains the numeric badge. Undo remains unchanged.
- `PanelsView.cs`: removes the old placeholder drawings and routes their existing callers to the new pages; the old one-booster drawing is replaced by shop navigation.
- `PerchSort/Assets/Tests/PlayMode/ShopTests.cs` and `AviaryTests.cs`: six behavior cases, plus opt-in real-camera evidence capture controlled by `PERCH_COLLECTION_EVIDENCE`. The retained test assembly does not reference TMPro, so visible text assertions use its UI `Graphic` base and the public text property without changing the read-only assembly definition.
- Unity-generated metadata companions for the four new C# files are included. No Core, Meta, existing tests, art, resource art, scene, settings, packages or shared audio changes are part of this delivery.

## Verification

Executed on the assigned Mac worktree, with every Unity command serialized by `flock /tmp/unity.lock`; queued behind other workers rather than bypassing the lock. No user Editor was killed.

```sh
python3 ci/check_current_game_names.py --self-test
python3 ci/check_current_game_names.py
flock /tmp/unity.lock bash scripts/run_tests.sh
flock /tmp/unity.lock bash scripts/screenshots.sh
```

Observed full run: **312/312 EditMode passed; 58/59 PlayMode passed; zero skipped**. The six added tests all pass. The existing discovery count is preserved: 53 existing PlayMode cases plus six new cases. Retained LevelsData and Layout discovery safeguards were not changed.

The sole full-suite failure is `PerchSort.PlayModeTests.T54HModalTests.EveryInformationModalCoversBoardAndShowsControlsWithoutStagger`, at `T54HModalTests.cs:157`: it requires `PanelsView.IsOpen` after `panels.Buy(...)`, and later requires dimmed placeholder modals for the two map entries. Those assumptions are incompatible with the required undimmed full-screen pages. Existing tests are explicitly read-only and remain untouched. The full script therefore exits nonzero; it is not reported as green. See [full PlayMode XML](evidence/playmode-results.xml) and [EditMode XML](evidence/editmode-results.xml).

Graphical smoke command (1080×1920 render targets, actual Game scene and runtime UI):

```sh
flock /tmp/unity.lock env PERCH_COLLECTION_EVIDENCE=/Users/apple/orca/workspaces/game/shop-and-aviary/docs/work-log/shop-and-aviary/evidence /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -screen-width 1080 -screen-height 1920 -projectPath /Users/apple/orca/workspaces/game/shop-and-aviary/PerchSort -runTests -testPlatform PlayMode -testFilter 'PerchSort.PlayModeTests.ShopTests;PerchSort.PlayModeTests.AviaryTests' -testResults /Users/apple/orca/workspaces/game/shop-and-aviary/build/collection-visual-results.xml -logFile /Users/apple/orca/workspaces/game/shop-and-aviary/build/collection-visual.log
```

Graphical smoke: **6/6 passed, zero skipped**. It exercises the actual map button callbacks, purchase confirmation, durable reload, insufficient funds, locking, zero-stock HUD navigation, unchanged board/session on close, ten collection cards, met detail navigation, retained scroll position, real bird frame changes, breathing, hops, and nonnegative UI x scales.

After the last visual correction, the insufficient/locked-card graphical case passed **1/1, zero skipped** and re-captured the genuinely red price pill. The green source pill is desaturated at runtime before its red tint, avoiding a nearly black green×red multiply. [Focused graphical XML](evidence/price-graphical-results.xml) records that final run. Exact focused command:

```sh
flock /tmp/unity.lock env PERCH_COLLECTION_EVIDENCE=/Users/apple/orca/workspaces/game/shop-and-aviary/docs/work-log/shop-and-aviary/evidence /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -screen-width 1080 -screen-height 1920 -projectPath /Users/apple/orca/workspaces/game/shop-and-aviary/PerchSort -runTests -testPlatform PlayMode -testFilter PerchSort.PlayModeTests.ShopTests.InsufficientAndLockedCardsNeverOpenConfirmationOrMutateProfile -testResults /Users/apple/orca/workspaces/game/shop-and-aviary/build/price-visual-results.xml -logFile /Users/apple/orca/workspaces/game/shop-and-aviary/build/price-visual.log
```

The canonical screenshot script also completed successfully with **34 PNGs** in `build/screens/game/`. Its read-only capture code still uses the old placeholder output filenames, but those images render the real new pages. [Graphical XML](evidence/graphical-results.xml) is retained beside the full-suite XML. Batch output included a `Can not play a disabled audio source` warning in the fixture/reload case; this is not an audio recording or proof of audible spend feedback.

Intermediate compilation attempts exposed the read-only test assembly's missing TMPro reference and an ambiguous `SafeArea` type name. Both were fixed inside the owned tests before the passing runs. Initial capture margins were corrected by applying the canonical safe-area/layout rebuild after selecting the render target. No failed cases were removed or weakened.

## Runtime evidence

Approved references are copied byte-for-byte beside the corresponding runtime captures under [evidence](evidence/); `art/approved_ui` is untouched.

| Surface | Actual runtime capture |
| --- | --- |
| Booster shop | [ui_shop_boosters.png](evidence/ui_shop_boosters.png) |
| Purchase confirmation | [ui_shop_confirm.png](evidence/ui_shop_confirm.png) |
| Locked card | [shop-locked.png](evidence/shop-locked.png) |
| Insufficient coins | [shop-insufficient.png](evidence/shop-insufficient.png) |
| Aviary list | [ui_aviary_list.png](evidence/ui_aviary_list.png) |
| Aviary detail | [ui_aviary_detail.png](evidence/ui_aviary_detail.png) |
| HUD plus badges | [hud-plus.png](evidence/hud-plus.png) |

Purchase samples after the tick are nominally 0.1 seconds apart (observed 0.1000–0.1094 s); the initial open dialog is held for 0.2518 s before the tick. Detail samples are nominally 0.25 seconds apart (observed 0.2500–0.2651 s) and span 10.0202 s. Contact sheets: [purchase-contact.png](evidence/purchase-contact.png), [aviary-motion-contact.png](evidence/aviary-motion-contact.png). Raw frames remain in `evidence/purchase/` and `evidence/aviary-motion/`. [capture-times.tsv](evidence/capture-times.tsv) records actual simulation timestamps, authoritative/displayed coin balances, owned-badge scale, bird frame identity and breathing scale. Observed countdown: 240 → 233 → 227 → 221 → 215 → 210; owned-badge scale: 1.3000 → 1.0018 → 0.9758 → 1.0000; all six authored cheer frames appear and bird breathing scale spans 1.0000–1.0200. These are rendered runtime frames, not illustrations or instant-motion animation claims.

Side-by-side comparisons (approved left, runtime right): [shop](evidence/compare_shop_boosters.png), [confirmation](evidence/compare_shop_confirm.png), [list](evidence/compare_aviary_list.png), [detail](evidence/compare_aviary_detail.png).

## Required integration changes and impossible combinations

1. **Removing placeholder enum values cannot compile within the owned-file boundary.** `MapView.cs`, `Editor/Game/GameCapture.cs` and the retained modal test still reference `PopupKind.ShopPlaceholder` and `AviaryPlaceholder`, and all are read-only. Their enum identities remain solely as the existing entry points; no placeholder screen remains. Integration must migrate those callers and remove the two enum members together. `PanelsView.Buy` likewise remains as the read-only test's existing entry point and now navigates to the shop.
2. **All existing tests green and all existing tests untouched cannot both hold.** The failing modal test asserts the replaced presentation, not the new contract. Integration needs permission to remove the retired shop/aviary rows from that modal-specific test and rely on the new page behavior tests. Do not fake `PanelsView.IsOpen`, add a dim to the new pages, or lower discovery to satisfy it.
3. **Exact reference-image equality is impossible with the frozen art and conflicting dimensions.** The supplied resources differ from the concept art: `btn_purple` has a gold rim rather than a rimless product disc; `booster_sapling_illus` has a green rather than white plus; `name_ribbon`, gift badges, wood frames and the 320-pixel bird frames have visibly different authored shapes/poses. No replacement art was generated and no resource was overwritten. A rimless purple product-disc sprite has no supplied resource key. The reference's aviary detail panel is measured at 964×708 with roughly 240-pixel outfit slots, whereas the assigned text requires a 1000×560 panel with 140-pixel slots; this implementation follows the text for the detail panel/slots. The list uses the measured 1032×1814 reference board rather than stretching the content into the prose's approximate 905×1640 board. Detail bird display compensates for transparent sprite padding so the feet reach the panel edge. These differences remain visible in the side-by-side evidence; visual sign-off is not claimed.
4. **Outfit and birdhouse art is not supplied.** Their tabs stay hidden; both future outfit slots are noninteractive silhouettes with padlocks. No outfit ownership, equipment, purchases or invented art was introduced.
5. **There is no dedicated coin-spend entry in the existing `Sfx` enum.** Purchase uses the existing `Sfx.Collect` effect. A distinct spend effect would require shared-audio ownership and an asset. No audio recording or device acceptance is claimed.
6. **Homecoming counts are displayed from the existing profile ledger.** The UI does not invent historical counts or alter reward/progression policies to backfill older saves. Fixtures prove nonzero stored counts are read correctly.

## Repository preservation

The worktree was clean before implementation. Before Unity, protected hashes were recorded for the sole scene/meta, build settings and package files. The scene builder rewrote scene-local object IDs. Unity also changed 114 bird-frame importer files only by trailing whitespace and expanded seven overlay importer schemas with default fields/platform entries. A recursive comparison confirmed every previously authored overlay field was retained, including pivots, mesh policy, compression and texture limits. Only these reviewed generated changes were restored to the clean starting blobs after verification. The scene/meta, build settings and package files match their original recorded hashes exactly. The owned source and requested evidence remain; existing test files and art bytes are unchanged.

Branch: `mac-shop-and-aviary/work`. The pushed commit SHA is supplied in the final worker response.
