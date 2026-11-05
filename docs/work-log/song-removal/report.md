# T41 — Remove Song meter and implement Berry Encore

Scope: [T41_remove_song.md](spec.md), branch `t39-polish`, issue #32, plus coordinator addendum `msg_27febaaeda0c` authorizing test mode. No commit or push. No scenes, catalog data, artwork, PNGs, existing art resources, or concurrent Meta work were edited; the addendum adds only `Resources/V5/dev_options.json` outside the original source/test/tool ownership and the authorized release-checklist line.

## Implementation

- Removed `SongState`, note/payout counters, payout pool/rotation, `NoteSource`, `SongNote` and `SongPayout` from Core. No homecoming grants booster inventory now.
- Retained the Chorus chain and accumulated win bonus: each flock whose chain step exceeds one contributes five coins. Transfers/swivels with no homecoming reset the chain; boosters do not, and booster-triggered homecomings continue it. Undo restores chain and coins with the full snapshot.
- Berry emits `GiftEffect.Encore` and grants `BoardState.Encore`. At the start of the next successful transfer **or swivel**, the flag is consumed and the action is excluded from the counted total. Illegal attempts and boosters do not consume it. Granting Encore during resolution never discounts the action that granted it. Undo restores the flag/counters; Restart returns to the initial state.
- **Move contract:** `BoardState.Moves` counts all successful transfers and swivels, including Encore actions; `BoardState.CountedMoves` and `GameSession.MoveCount` are the displayed/star total. `Stars` compares the counted total against stored par. Day/night and Moonbeam continue ticking on every real action. Solver path lengths, completion-order minima, level-tool par/booster minimum proofs and their public-session replay checks use real actions, not counted moves.
- Both the Core solver key and level-tool `StateCodec` retain pending Encore. Real/counting totals, chain, coin rewards and inventory remain excluded from no-booster search keys. The level-tool codec round-trips the new flag and adjusts its header offsets.
- Removed the HUD Song pill, five note slots, overflow count, `SongNoteView.cs`, payout flights/previews and the `SongPill` layout anchor. Removed Song inputs from fixture/session construction and Song fields/keys from progress loading/saving. Old preference entries are ignored and left intact; current coins/inventory/stars remain independent.
- Kept the existing Chorus ribbon, bottom bar, birdhouses, controls and other layout values unchanged. Berry gift presentation uses the existing house-local gift fallback instead of targeting a removed meter. `gift_encore` remains a legitimate gift/Chorus icon; no `note_*` references remain in the active owned runtime/test/tool sources. Sunny's `WakeUpSong` and adjacent-row song waking are unrelated mechanics and remain intact.

## Coordinator addendum — Test mode

- Added `PerchSort/Assets/Resources/V5/dev_options.json`, currently shipped with `{"unlockAllLevels": true, "grantTestStock": true}`. `V5DevOptions` loads that resource; missing/empty/malformed JSON falls back to both flags false. Scene tests have an explicit temporary resource override and restore it during teardown without editing the asset.
- `GameProfile` keeps persisted `HighestUnlocked`, `Stars`, `Coins` and `Boosters` raw. Its effective accessors overlay highest level 40 and at least three stars for every level when unlock is enabled; stock mode overlays at least 2,000 coins and five of every booster. Larger real balances remain visible.
- The picker now renders the effective star ratings and enables all 40 controls in test mode. Level access, booster visibility/use/purchase and Firefly guards use effective highest-unlocked progression, including on a replay of L1. Actual starter grants use genuinely reached progression, not synthetic unlocks.
- Stock replenishment tracks synthetic units separately from real inventory, including synthetic adjustments mirrored into Core's initial/undo snapshots. Successful board/booster actions register the synthetic snapshot; Undo and Restart restore it alongside Core. Real use/purchases change the raw balance; purchasing after synthetic uses does not pay an invisible debt, virtual refunds cannot turn into saved stock, and replenishment never writes the artificial minimum to PlayerPrefs. Only test-stock mode allocates this extra history.
- Real coin spending is deducted from the raw balance, clamped at zero; the effective display remains at least 2,000. Genuine win rewards and best scores still persist normally. Merely loading, picking a level, saving, using synthetic stock or restarting never writes the artificial completion/minimum balances.
- Added ten EditMode cases for flags off, missing/empty/malformed input, independent flag combinations, preserving larger balances, repeated synthetic use, real purchases, all undo steps, Restart and raw-only persistence. Added a PlayMode case covering off/malformed/on scene loading, forty enabled three-star picker controls, later-unlock tools on L1, use/purchase/undo persistence and a real L39 win followed by Next to L40. Existing PlayMode cases explicitly disable the resource overlay and restore the previous override.
- Added the authorized release gate in `docs/RELEASE_CHECKLIST.md`: **dev_options.json must ship with both false**.

## Test migration

Replaced `V53SongTests.cs` with `V54ChorusEncoreTests.cs`.

Retired Song-specific tests:

1. `ConsecutiveHomesEarnBonusNotesAndCoinsButANonHomeMoveResetsOnlyTheChain` — replaced by coin/chain-only behavior coverage.
2. `BerryGiftCanPayBeforeChorusAndUndoRestoresPayoutRotation` — removed obsolete payout contract; replaced by pending Encore/undo behavior.
3. `BankedNotesCanProduceSeveralPayoutsWithRemainderAndFixedRotation` — removed obsolete bank/rotation contract.
4. `LevelOneBanksFullMeterUntilTheFirstNoteInAnUnlockedLevel` — removed obsolete cross-level meter carry contract.
5. `BoostersPreserveChainAndBoosterHomecomingsContinueItWithoutCountingAMove` — retained behavior coverage without note/payout assertions, with Encore granting and undo checks.

Six replacement cases cover consecutive homecoming coin rewards, non-home transfer/swivel chain reset, booster preservation and booster homecomings, Encore discount/clock/star boundaries, illegal-attempt/booster survival, successful-swivel consumption versus Light Step, undo/restart restoration, and exact real-action solver minima/key distinctions. The existing multi-wave resolution test now asserts the ten-coin three-flock bonus rather than note ordering.

Updated shared snapshots to retain `CountedMoves`/Encore instead of retired economy fields. Updated fixture/catalog solver replays to compare real `State.Moves` with par. Removed obsolete Song expectations from layout and PlayMode tests; retained win HUD/raycast, next-level starter inventory and profile round-trip coverage. Updated the solver's economy-independence case without its removed Song input.

## Local verification actually exercised

SDK: existing `/tmp/perch-t32-sdk/dotnet` (.NET 8.0.419), C# **9.0**, cached NUnit/NUnitLite 3.13.3. Temporary harnesses directly compiled production Core, level-tool codec, LevelLoader, V5ViewModel, V5Progress, V5DevOptions, V5Fixtures and V5Layout; a separate headless smoke also compiled and executed the actual GameRoot and V5Panels. Unity resource/JSON/preferences/vector/rectangle/component/view boundaries used temporary adapters, not replacement gameplay rules.

- Before implementation, the actual two-transfer Berry smoke printed `real=2, displayed=2, phaseCounter=2, stars=2, chain=2, coins=5` and failed the Encore expectation (exit 134).
- After implementation, the same smoke printed `real=2, displayed=1, phaseCounter=2, stars=3, chain=2, coins=5` and exited 0.
- `dotnet build tools/levels/LevelTool -c Release`: passed, zero warnings/errors.
- Before the addendum: **180/180** Core/catalog/View-policy cases and **30/30** layout geometry cases passed, with zero failures/warnings/skips; `/tmp/t41-nunit.xml` and `/tmp/t41-layout-nunit.xml`.
- Final combined Core/catalog/View-policy/layout/developer-options harness: **220/220 passed**, zero failures/warnings/skips; `/tmp/t41-dev-nunit.xml`. Command: `/tmp/perch-t32-sdk/dotnet run --project /tmp/t41-devtests/Tests.csproj -c Release -- --workers=1 --result=/tmp/t41-dev-nunit.xml`.
- Additional executable smokes passed: pending Encore survives codec round-trip, produces a distinct key, and remains correctly consumed after decode with real-action/day-counter advancement; legacy Song preference values are harmlessly ignored/preserved without granting boosters or changing coins.
- Final Roslyn C#9 syntax parsing: **58 owned-source files, zero syntax errors**, covering Core, Game, V5 EditMode/PlayMode tests and production level-tool files, excluding concurrent Meta work; this is not Unity type checking.
- Compared the installed catalog to `/tmp/t35-installed-evidence.json`: 40 levels in each, **zero changed pars**. No catalog edits were made.
- Complete installed-catalog validation exited **0**, **40/40 PASS**, no catalog failures: `/tmp/perch-t32-sdk/dotnet tools/levels/LevelTool/bin/Release/net8.0/LevelTool.dll validate PerchSort/Assets/Resources/Levels/levels.json --complete --json-out /tmp/t41-validation.json` (398.85 seconds). All pars match the installed T35 baseline. L22/L26/L32/L37 retain bounded explored-state dead-share evidence rather than exhaustive graph closure; these are not claims of full reachable-graph enumeration.
- Developer-mode executable smoke loaded the actual shipped-on resource, exercised eight uses without exhausting stock, purchased/undid a booster, and observed `accessible=40`, `stars40=3`, `displayedCoins=2000`, `rawCoins=70`, `rawFirefly=1`, with synthetic progress absent from saved values.
- Actual-controller/picker smoke passed: flags off/malformed/on, forty enabled controls each with three fully visible star data objects, later-unlock Sapling use on L1, use/purchase/Undo/Restart raw persistence, arbitrary L39 selection, public-controller replay to a real Core win, and Next to L40. Command: `/tmp/perch-t32-sdk/dotnet run --project /tmp/t41-controller/Controller.csproj -c Release`. These are headless scene-graph/data checks through adapters, not rendered UI proof.

Temporary harness sources were removed after verification; result XML/JSON remain outside the repository as evidence. Product rule docs were left to the coordinator because v5.4 already explicitly voids the old Song clauses; only the addendum-authorized release-checklist line was added.

## Untested / integration boundaries

No Unity Editor exists on this machine. Actual Unity compilation/import, native EditMode/PlayMode execution (including `V5ButtonTests` and the new test-mode scene case), rendered HUD removal/picker stars, real animation/tween timing, native raycasts, scene lifecycle and platform PlayerPrefs persistence require the coordinator's Mac verification. No screenshots or visual acceptance are claimed. The modified PlayMode source received syntax checking, not Unity compilation or execution; the actual-controller/picker headless smoke does not replace native PlayMode.

Concurrent `Scripts/Meta` files appeared during this task and were left untouched; the coordinator was notified that their owner should remove an obsolete comment assigning Song notes/payouts to session state. Historical `tools/levels/work` snapshots were left untouched. No LSP server was available for C# references, so active consumers were checked through scoped searches, direct compilation of reachable pure code and syntax verification.
