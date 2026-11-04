# T37a delivery report — pure v5 Meta logic

## Scope and authority

Implemented the scope in [T37a_meta_logic.md](T37a_meta_logic.md), using [GAME_DESIGN.md §4.1–4.3](../docs/GAME_DESIGN.md#41-species-the-aviary), [§8](../docs/GAME_DESIGN.md#8-boosters-coins-and-shop), [§9](../docs/GAME_DESIGN.md#9-worlds-and-scenes-every-level-looks-different) and the task's explicit replay-improvement rule.

All repository changes are new files under `PerchSort/Assets/Scripts/Meta/`, `PerchSort/Assets/Tests/Meta/`, and this report. No Core/Game/existing tests, level data, scenes, artwork, settings, or other workers' files were edited. No commit was made.

## API overview

Namespace: `PerchSort.Meta`. Assembly: `PerchSort.Meta`, `noEngineReferences: true`, no project assembly references. Its `BoosterKind` is independent of Core; integration must explicitly map names rather than cast enums. Seven kinds include unlimited Undo; only the six consumables have stored quantities.

| API | Contract |
| --- | --- |
| `PlayerProfile` | Externally immutable, copy-on-write profile; coins, inventory, highest reached level, best stars, per-level/total clears, cosmetic ownership/equipment, species discovery/home counts, daily date, tutorial/starter/world-reward flags and legacy mute preference. |
| `WithCoins`, `WithBoosterCount`, `WithTutorialFlag`, `WithMuted` | Return updated profiles without changing the input; Undo cannot be assigned inventory. Read-only dictionary views cannot mutate profile state. |
| `ToText` / `PlayerProfile.FromText` | Schema 1 JSON using framework `DataContractJsonSerializer`; no Unity JsonUtility or third-party package. Missing/unknown fields are tolerated, including unknown nested fields and future schema numbers; recognized data is normalized to current invariants. Invalid JSON fails explicitly rather than silently resetting progress. |
| `ProfileMigrator.FromLegacy(IDictionary<string,string>)` | Reads actual `V5Progress.cs` keys: `v5.coins`, `v5.muted`, `v5.highestUnlocked`, `v5.bestStars.<level>`, `v5.tutorial.<level>`, `v5.booster.<0..5>`, `v5.starter.<0..5>`. Obsolete Song keys are ignored. |
| `Economy.StarsForMoves(countedMoves, par)` | Three stars through par, two through par + 3, otherwise one. Input already includes any Berry move adjustment. Overflow-safe threshold comparison. |
| `Economy.OnWin(profile, level, countedMoves, par, chorusStepsBeyondFirst)` | Returns `PayoutResult` containing the new profile, achieved stars and `WinPayout` lines. First clear pays 10 + 5 × stars; replay pays 5 plus 5 per newly improved star. Saves best stars, increments clears, advances reach and grants newly reached starter gifts. Rejects a locked level. |
| `WinPayout` | Separate `FirstClear`, `Replay`, `StarImprovement`, `Chorus`, `WorldComplete`, `DailyGift`, `NewSpecies`, and checked `Total`. |
| `Economy.ChorusBonus(stepsBeyondFirst)` | Five coins per qualifying chain step, summed across every chain in the attempt, not the longest chain. `OnWin` takes this step count, **not already-calculated coin value**. |
| `Economy.ClaimDailyGift(profile, IClock)` | Fifty coins on a later local calendar date; exact `yyyy-MM-dd` date stored. Injected clock is read once; repeat claims and backwards dates pay zero. |
| `Unlocks.Boosters`, `BoosterUnlockLevel`, `IsBoosterUnlocked` | Six immutable catalog rows with unlock levels, prices and starter counts; Undo unlocks at L1 and has no shop entry. |
| `Unlocks.IsLevelUnlocked`, `IsWorldUnlocked`, `IsWorldComplete` | L1 is initially available; each next level needs its predecessor cleared. The previous world's final level opens the next world. World completion requires all ten clears, not just its final node. |
| `Unlocks.GrantReachedStarterGifts` | Idempotent catch-up for all reached unlock levels, including migrated saves; does not replenish spent starters. |
| `Unlocks.WorldBoosterFor(level)` | World default OwlEye / MorningBell / Scarecrow / SpringRain. Does not inspect level mechanics; the §11.1 HUD fallback remains an integration concern. |
| `Shop.Catalog`, `TryGetItem`, `Purchase`, `Equip` | 43 purchasable items: 12 booster singles/bundles, 27 outfits and four houses. Typed `ShopResult` keeps the original input on failure. Cosmetics are not auto-equipped on purchase. |
| `Aviary.Species`, `TryGetSpecies` | Ten immutable species cards in debut order, including Rook between Ember and Inky; canonical ids/names/debut levels plus personality/quirk localization keys. |
| `Aviary.OnLevelStarted` | Deduplicates supplied canonical species ids, marks newly met species, credits 20 each once, and returns cards in canonical debut order plus payout. No separate economy call is needed to award discovery coins. |
| `Aviary.IncrementFlocksHome` | Explicit committed home-count increment; rejects crow and unknown species. Does not award coins or infer first discovery. |
| `WorldMap.Build`, `Node`, `NextLevelId`, `NodePosition` | Four ten-node worlds with gates, stars and locked/current/cleared states. No next node after L40. Whole-map normalized coordinates place L1 at the bottom and L40 at the top, following a continuous gentle sinusoidal S-path without boundary resets. |

Stable item ids: `booster.firefly`, `booster.firefly.x3`, `booster.sapling`, `booster.owlEye`, `booster.morningBell`, `booster.scarecrow`, `booster.springRain`; bundles append `.x3`. Outfits use `outfit.<species>.1` / `.2` / `.3`; houses use `house.mushroom`, `.teapot`, `.lantern`, `.acorn`. Equip-only reset ids are `outfit.<species>.default` and `house.default`.

## Decisions and boundaries

- **Crow cosmetics:** §4.2 explicitly forbids outfits for crows, overriding the broad “per species” wording. Nine eligible species × three outfits = 27. Rook still has an Aviary card and the once-only discovery reward, but never a flock-home count.
- **Reach versus clear:** clearing L1 immediately reaches L2 and grants Firefly starters; opening the next screen is unnecessary. Booster purchases and catch-up gifts use saved highest reach. The map independently requires predecessor clears and the stated world gates, so an inconsistent migrated high-reach value alone cannot bypass a map gate.
- **Cosmetic availability:** no new discovery/debut gate was invented for outfits or houses. Booster purchases alone have the task-specified reach gate. Single/bundle prices are 30/75, 60/150, 80/200, 80/200, 100/250 and 100/250; outfit tiers are 300/500/800; houses are 600.
- **Legacy inference:** old saves have no clear counts, so each cleared level starts at one known clear. Legacy numeric tutorial flags become `level.<n>` string keys. Already-complete legacy worlds are marked rewarded to avoid speculative retroactive payouts; incomplete worlds can earn the bonus when their last missing clear is recorded. No coins are fabricated during migration.
- **Serialization:** arrays for best stars/clear counts use level ids as indices 1–40 with unused index 0. Owned houses use bare style ids, while owned/equipped outfits use catalog item ids. Unknown fields are ignored, not preserved on rewrite; malformed typed values are not silently rescued. Future schema fields should not be edited through this writer if lossless forward round-tripping is required.
- **Daily clock:** local dates follow the injected `IClock.Today`, with `SystemClock` using `DateTime.Today`. A backwards device date is not eligible; this prevents duplicate grants when switching back to a previously claimed date. This is not server-authoritative anti-cheat.
- **Transactional input:** home counters must be applied for committed winning-attempt homecomings, not visual callbacks, previews or attempts that will be undone/restarted. `OnWin` is a domain transaction, not a persistence/idempotency service: integration must commit its returned profile once per won attempt. Recalling it is intentionally a replay reward.
- **Numbers and failures:** runtime negative inputs are rejected and arithmetic overflow fails without mutating the input profile. Typed shop failures are `UnknownItem`, `NotEnoughCoins`, `AlreadyOwned`, `Locked`, `NotOwned` and `NotEquippable`; success is `None`.

These are disclosed integration decisions, not requests to expand task scope.

## Executed verification

No Unity Editor is available on this machine. The new EditMode test sources were compiled and executed directly in a temporary **C# 9 / .NET 8.0.419 / NUnitLite 3.13.3** harness, using the production source files rather than copied implementations.

```text
Test Count: 53, Passed: 53, Failed: 0, Warnings: 0, Inconclusive: 0, Skipped: 0
```

Coverage includes serialization of the complete profile, escaped/Unicode flags, missing/unknown/future-schema fields, invalid JSON, invalid persisted values, actual legacy keys, ignored obsolete keys, legacy completion inference, move-star boundaries, first/replay/improved-star/Chorus payout lines, all four once-only world bonuses across reload, daily same-day/next-day/rollback determinism, all six booster unlock and gift boundaries, purchased inventory and bundles, spent starter non-replenishment, every outfit tier across all eligible species, all four houses, typed shop errors, equip/reset and per-species isolation, discovery ordering/deduplication/once-only rewards, home-count restrictions, all forty sequential map transitions/gates, campaign completion and normalized continuous map positions.

The separate executable smoke exercised **40 level wins, four world awards, all ten species discoveries at debut, forty save/load cycles, home-count persistence, a booster bundle, outfit/house purchase and equip, and repeated daily-gift suppression**. Observed output:

```text
SMOKE PASS: 40 sequential levels, 4 world grants, 10 species, 40 save/load cycles; campaign coins=2090; purchases=1150; remaining=940; SpringRain=5; cosmetics and daily idempotency preserved.
```

The 2,090 campaign coins comprise 1,000 first-clear coins, 800 world coins, 200 discovery coins, 50 daily coins and 40 Chorus coins. Purchases cost 250 + 300 + 600 = 1,150.

A separate production-only **C# 9 / .NET Standard 2.1** build also succeeded with **0 warnings and 0 errors**, checking the API subset used by Unity without NUnit or game assembly dependencies.

Executed harness commands used `/tmp/perch-t32-sdk/dotnet`, a temporary project linking `Scripts/Meta/*.cs` and `Tests/Meta/*.cs`, and a separate production-only library target. NUnit XML remains at `/tmp/perch-t37a-meta-results.xml`; temporary harness source/build scaffolding was removed after execution. No new test frameworks or runtime packages were installed.

## Remaining Unity integration verification

- Wire persistence/load/save and migration atomically; map Core booster kinds explicitly. Replace the caller's profile reference only after a successful transaction.
- Feed counted moves and the sum of qualifying Chorus steps exactly once on win; apply final inventory and committed home counts without double-counting restart/undo events. This task does not alter Game or Core callers.
- Add UI/localization bindings for map nodes/gates, payout lines, catalog, Aviary, equipped overlays, daily claim and starter presentation. Card personality/quirk keys are plain data, not new visible copy or completed animation.
- Run Unity compilation and the new `PerchSort.Meta.EditModeTests` assembly on the Mac; validate generated Unity metadata and assembly discovery. No Unity EditMode/PlayMode run was claimed here.
- Exercise serialization on the actual target player, including managed stripping/IL2CPP reflection behavior of the framework serializer, real saved-data upgrade and application lifecycle persistence.
- Screenshots, runtime visuals and input behavior remain untested: this delivery is pure logic and has no UI or asset changes.
