# Plush shop UI shell report

## Scope and dependency

- Branch: `mac-shop-plush/work`.
- Shared contract: `docs/work-log/plush-ui/spec.md`, `components.md`, and `text-inventory.md`.
- Viewed all four approved direction images from `origin/ui-design-review`: `shop_boosters`, `shop_houses`, `shop_outfits`, `shop_confirm`.
- Merged the explicitly authorized Foundation branch `origin/mac-painted-text/work` at `15d73304`. No shop edits to Foundation, board, popup, map, aviary, art, Core, Meta, save, level, scene, or settings sources.
- The branch already existed, clean at the baseline. The requested create command reported `fatal: a branch named 'mac-shop-plush/work' already exists`; work continued on that assigned branch.

## Changed files

- `PerchSort/Assets/Scripts/Game/ShopView.cs`: canonical shop shell and confirmation; local painted coin header, painted price/count/unlock numbers, painted card ribbons, fixed-size tabs/close controls, cosmetic ownership/equipment presentation, nonpersistent fixture rendering, and drawn outfit idle-frame playback.
- `PerchSort/Assets/Scripts/Game/ShopFakeData.cs`: development-only `IScreenStates` provider with public `(GameRoot root)` constructor; local fixture purchase/equipment responses and shared preview restoration lifecycle.
- Unity-generated metadata for the new shop script, if produced by the compile import.
- This report.

## Shell behavior

- Coin pill at the top left; close X is 108 at `(980, 130)`. No `HudView.CoinHeader` dependency and no player-visible TMP/Text components in the shop implementation.
- Three 108 tab discs at `(360, 270)`, `(540, 270)`, `(720, 270)`. Selected gold ring plus a separate 70 × 28 green pill. The ring uses a runtime geometric crop of the existing combined ring/marker sprite; no artwork files changed.
- Booster cards share 460 × 420 geometry; cosmetic cards share 460 × 650 geometry. Two-column layout; all 27 catalog outfits remain reachable by scrolling, with the four named reference outfits presented first.
- Prices use the existing cream coin-pill artwork, which already contains its overlapping coin, plus brown PaintedNumber glyphs. No green price pills or duplicated coins.
- Red 56 count badges use white glyphs and hide at zero. Locked booster cards retain their geometry, are dimmed and non-interactive, and show a painted numeric unlock chip.
- Equipped cosmetics have a gold outline around the card and a green check badge, with no price. Owned/not-equipped cosmetics show a 100 wood hanger action, with no price. Unowned cosmetics show a price.
- Confirmation uses an 860-wide centered panel, painted item ribbon on its top edge, a 100 close X centered 24 inside the top-right corner, and one 124 green check disc outside the panel, with its top 48 below the panel bottom. No long action bar.
- Confirmation opens over 0.18 seconds and cancellation closes over 0.12 seconds through GameMotion. Purchase remains synchronous, preventing a closing dialog from accepting repeated purchases. Page raycasts and interaction are disabled while confirmation is present.
- Outfit pictures breathe and swap through supplied outfit-specific blink/cheer frames, including in confirmation. No mirroring, pose rotation, or fabricated transform action. Existing page music/audio ownership is unchanged.
- Real purchase/equip paths still call the existing GameRoot methods. Review fixtures change only their local display state; returning to the real game uses the Foundation preview lifecycle.

## Review states

Open canonical `Assets/Scenes/Game.unity`, enter Play mode, then use these click paths:

| Click path | Coverage |
| --- | --- |
| `Tools > UI States > Shop > Boosters / Stock badges` | Reference counts 3, 2, 1, 1; zero counts hidden; 240 coins |
| `Tools > UI States > Shop > Boosters / Zero stock` | All count badges hidden |
| `Tools > UI States > Shop > Boosters / Locked tools` | Dimmed, non-interactive tools and unlock numbers |
| `Tools > UI States > Shop > Boosters / All tools unlocked` | All six tools available |
| `Tools > UI States > Shop > Numbers / Zero coins` | Zero coin boundary and insufficient-purchase interaction |
| `Tools > UI States > Shop > Numbers / Maximum coins` | Five-digit 99999 coin boundary |
| `Tools > UI States > Shop > Numbers / Maximum stock` | Five-digit badge fitting boundary |
| `Tools > UI States > Shop > Houses / Not owned` | Four houses, painted names, cream prices |
| `Tools > UI States > Shop > Houses / Owned and equipped` | Equipped, owned, and unowned house states |
| `Tools > UI States > Shop > Outfits / Mixed ownership` | Four reference outfits; equipped scarf and owned flower |
| `Tools > UI States > Shop > Outfits / All not owned` | Purchase pills and scrolling catalog |
| `Tools > UI States > Shop > Outfits / Owned not equipped` | Wear controls without prices |
| `Tools > UI States > Shop > Outfits / Equipped` | Gold outline/check state without prices |
| `Tools > UI States > Shop > Names / Longest painted ribbon` | Round Glasses and other supplied ribbons |
| `Tools > UI States > Shop > Names / Missing outfit ribbons` | Scrolls to missing-name slots; no font fallback |
| `Tools > UI States > Shop > Confirm / Booster` | Booster confirmation, purchase and cancellation |
| `Tools > UI States > Shop > Confirm / Outfit` | Animated outfit confirmation |
| `Tools > UI States > Shop > Confirm / House` | House confirmation |
| `Tools > UI States > Shop > Confirm / Disabled accept` | Dimmed/non-interactive 124 accept disc |
| `Tools > UI States > Shop > Feedback / Insufficient coins` | Price shake and temporary red coin/price feedback |

`Return to real game` restores the untouched real attempt/profile. Instant motion can be toggled in the shared switcher.

## Verification

- `python3 ci/check_current_game_names.py --self-test`: 14 passed.
- `python3 ci/check_current_game_names.py`: 261 tracked text files inspected before Unity and 262 after staging the new shop script; 0 violations at both checkpoints.
- Exactly one serialized Unity batch invocation submitted, including a temporary construction smoke in the owned fixture file:

```sh
flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -quit -projectPath /Users/apple/orca/workspaces/game/shop-plush/PerchSort -executeMethod PerchSort.Game.ShopFakeData.CompileShopShell -logFile /Users/apple/orca/workspaces/game/shop-plush/build/shop-compile.log
```

- Unity C# compilation completed with no compiler errors. The invocation exited **1** because the temporary construction smoke threw while opening `Boosters / Locked tools`, after completing `Boosters / Stock badges` and `Boosters / Zero stock`. Those two states exercised local painted glyph loading, all four 108 controls, brown prices, and hidden zero-stock badges.
- Exact failure: `Can't add component 'CanvasGroup' to Firefly card because such a component is already added to the game object!`, followed by `NullReferenceException` at `ShopView.BuildCard`. `GameButton` has `[RequireComponent(typeof(CanvasGroup))]`; the redundant additions in locked-card and disabled-confirm setup were removed. Both now use GameButton's existing disabled appearance.
- The temporary smoke entry was removed. No second Unity launch was made. The remaining 18 states and the correction have **not** been runtime-rechecked in Unity.
- Corrected final sources were compiled directly with Unity's generated Roslyn response file, without launching Unity; exit **0**, warnings only (`UAC0009` for the project-required `DEVELOPMENT_BUILD` guards). Command, from `PerchSort/`:

```sh
/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/Resources/Scripting/DotNetSdk/dotnet /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/Resources/Scripting/DotNetSdk/sdk/8.0.318/Roslyn/bincore/csc.dll @Library/Bee/artifacts/200b0aE.dag/PerchSort.Game.rsp -out:../build/shop-validated.dll -refout:../build/shop-validated.ref.dll
```

- Preservation snapshots: `build/shop-preservation-before.json` and `build/shop-preservation-after.json`; all 33 scene/meta/settings/package files remained byte-identical.
- Unity's initial import generated changes to foreign-owned resource metadata and new metadata files. These remain local, outside the shop commit; no blind restore or broad staging was performed. Only the shop script's generated `.meta` is included.
- EditMode and PlayMode suites: NOT RUN, as instructed. Screenshot scripts: NOT RUN, as instructed.
- Runtime visual comparison, supported-resolution screenshots, timed animation/audio evidence, listening and device acceptance: NOT ASSESSED. Construction checks are not visual or timed acceptance.

## Missing label sprites

The catalog has 27 outfit IDs but only these four approved outfit-name ribbons: Straw Hat (`outfit.sunny.1`), Cozy Scarf (`outfit.sky.1`), Round Glasses (`outfit.berry.1`), Flower (`outfit.mint.1`).

The remaining 23 outfit cards and their confirmations reserve an empty same-size name-ribbon slot, as required by the shared contract. No names or substitute font labels were invented:

- `outfit.sunny.2`, `outfit.sunny.3`
- `outfit.sky.2`, `outfit.sky.3`
- `outfit.berry.2`, `outfit.berry.3`
- `outfit.mint.2`, `outfit.mint.3`
- `outfit.cocoa.1`, `outfit.cocoa.2`, `outfit.cocoa.3`
- `outfit.snow.1`, `outfit.snow.2`, `outfit.snow.3`
- `outfit.ember.1`, `outfit.ember.2`, `outfit.ember.3`
- `outfit.ink.1`, `outfit.ink.2`, `outfit.ink.3`
- `outfit.hoot.1`, `outfit.hoot.2`, `outfit.hoot.3`

All six booster names and all four house names have supplied painted ribbons.

## Boundaries and integration requests

- Exact 40% modal desaturation needs an owner-authorized change to `Assets/Shaders/UIDesaturated.shader`: its existing fragment code hardcodes 60%. Shop uses that shared shader unchanged, with an overlay calculated for 35% remaining brightness in the project's color space. Thus desaturation is stronger than the requested 40%; this is not claimed as exact visual acceptance.
- Supplied `card_wood` and `panel_wood` sprites contain corner leaves. They are retained untouched; the task does not authorize repainting or editing shared art. The requested two-leaf-cluster-only modal decoration needs corrected source art from its owner.
- Five-digit stock inside a fixed 56 badge uses PaintedNumber's fitting behavior, reducing cap height below the nominal 28 minimum. Coins at 99999 have a separate wide pill and do not need that reduction. The fixed badge size and long-stock readability require a design choice; no inventory rule or value was changed to hide this boundary.
- Outfit prices remain the real catalog prices, not invented prices copied from the reference picture. Featured reference items happen to be tier-one entries at 300; no economy changes were made.
- No new GameRoot hook is required: the merged Foundation `PreviewMap(GameProfile)` and preview restoration methods suffice.

## Outfit ribbon wiring follow-up

- Branch: `mac-shop-outfit-names/work`, based on `origin/main` at `0241c629`.
- The earlier missing-outfit-label limitation above is resolved for all 27 catalog outfits. `ShopView.CosmeticLabel` now uses an ordinal outfit-ID-to-painted-sprite lookup matching `docs/work-log/plush-ui/text-inventory.md`, including all 23 additions. Both the card and confirmation already call this shared resolver; neither uses an empty slot for a known catalog outfit. Truly unknown outfit IDs still return null and retain the existing same-size empty slot, never a font fallback.
- `PaintedLabels.cs`: added only the 23 new exact painted-text inverse lookup entries. The existing four outfit entries are unchanged.
- `ShopFakeData.cs`: renamed the obsolete `Names / Missing outfit ribbons` state to `Names / Full outfit catalog`; it still scrolls to the bottom for reviewing the later outfits. No catalog, price, ownership, equipment, save, layout, or artwork changes.
- Changed files: `ShopView.cs`, `PaintedLabels.cs`, `ShopFakeData.cs`, and this report.
- Review path: `Tools > UI States > Shop > Names / Full outfit catalog`. Scroll through all 27 cards; click any unowned card to inspect its corresponding confirmation ribbon, then cancel. `Outfits / All not owned` and `Confirm / Outfit` remain available.

### Follow-up verification

- Exactly one **plain** Unity batch compile, with no execute method or test runner:

```sh
flock /tmp/unity.lock /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -quit -projectPath /Users/apple/orca/workspaces/game/shop-plush/PerchSort -logFile /Users/apple/orca/workspaces/game/shop-plush/build/shop-outfit-names-compile.log
```

- Result: **exit 0**, `Exiting batchmode successfully now!`; no compiler errors. Existing `UAC0009` warnings remain for the project's development-build guards.
- A temporary standalone console smoke invoked the freshly compiled `ShopView.CosmeticLabel` and `PaintedLabels.KeyForText` through reflection, without launching Unity. Passed: all **27** outfit mappings exactly match the inventory, all **27** inverse lookups match, all **27** PNG paths exist, all **4** house mappings remain unchanged, and unknown outfit/text fallbacks return null. Its first attempt had an incorrect Unity assembly resolver directory; correcting it to the compiler response file's `Resources/Scripting/Managed` path produced exit 0. Observed output: `build/outfit-name-smoke.log`. Temporary harness sources and outputs were removed; no permanent tests were added.
- `python3 ci/check_current_game_names.py --self-test`: **14 passed**. Naming guard: **260 tracked text files, 0 violations**.
- All **33** snapshotted scene/meta/settings/package files remained byte-identical: `build/outfit-names-before.json`, `build/outfit-names-after.json`.
- EditMode/PlayMode suites and screenshot scripts: **NOT RUN**, as instructed. Rendered card/modal appearance was not visually reverified; the smoke covers compiled label resolution, not rendering.
- Missing label sprites for catalog outfits: **none**. Unknown-ID placeholders are intentional.
- Before branch creation, the prior compile's local import state was preserved in the stash named `Preserve Unity import metadata before outfit ribbon follow-up`, because untracked Foundation metadata blocked checkout. Newly generated import metadata stays outside this follow-up commit; no shared art or metadata is staged.

