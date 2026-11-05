# Naming policy audit report

## Baseline and boundaries

- Audited baseline: `38fca7094230cc628380a8e86ec32af92576b456` (`main`).
- Cloud-only isolated branch: `naming-policy`. No changes to the user's primary checkout or the old cloud Godot migration checkout.
- The complete GitHub tree contained 7,306 entries and was not truncated. No tracked path matched the repository's forbidden version/task identity pattern. Current Art/Birds/World resource filenames were lowercase with no spaces.
- The baseline guard passed: 200 source files inspected, zero violations; all nine existing guard self-tests passed. This was not proof of every convention.

## Confirmed guard defects and fixes

1. Lowercase and mixed-case version-like identifiers, including prefixes followed directly by letters, escaped the content pattern, even though corresponding filenames were rejected. The pattern now rejects these source identifiers while retaining ordinary words and the third-party `cv2` module.
2. Serialized Unity scene/prefab/asset/template object and type identities were excluded by `is_source`. The guard now reads `m_Name` and `m_EditorClassIdentifier` fields in authored Assets documents. Other serialized values, historical descriptions and storage payloads remain untouched.
3. CI did not run the guard's regression suite. The existing naming step now runs the self-tests first.

The changes modify only validation tooling and this report. No asset bytes, .meta GUIDs, C# types, save fields, schema values or runtime references changed.

## Further naming findings requiring engine-backed refactoring

The following files contain multiple namespace-level types, contrary to the one-top-level-type-per-file rule. This inventory is based on declaration inspection and does not rename serialized or persisted types. These are not alternate game versions. Splitting them needs compile/import checks and file-specific ownership coordination.

- `PerchSort/Assets/Tests/Meta/ProfileEconomyTests.cs`: Profiles, FakeClock, ProfileEconomyTests.
- `PerchSort/Assets/Tests/PlayMode/ShopTests.cs`: CollectionScreenTests, ShopTests.
- `PerchSort/Assets/Scripts/Meta/ProfileText.cs`: ProfileText, ProfileMigrator.
- `PerchSort/Assets/Scripts/Meta/Economy.cs`: IClock, SystemClock, WinPayout, PayoutResult, Economy.
- `PerchSort/Assets/Scripts/Meta/WorldMap.cs`: MapNodeState, MapPosition, LevelNode, MapWorld, WorldMap.
- `PerchSort/Assets/Scripts/Meta/Shop.cs`: ShopItemKind, ShopError, ShopItem, ShopResult, Shop.
- `PerchSort/Assets/Scripts/Meta/Unlocks.cs`: BoosterInfo, Unlocks.
- `PerchSort/Assets/Scripts/Meta/Aviary.cs`: SpeciesCard, SpeciesMetResult, Aviary.
- `PerchSort/Assets/Scripts/Meta/PlayerProfile.cs`: BoosterKind, PlayerProfile.
- `PerchSort/Assets/Scripts/Game/MapView.cs`: MapView, MapInput.
- `PerchSort/Assets/Scripts/Game/PanelsView.cs`: ModalLayer, PopupKind, LongPress, PanelsView.
- `PerchSort/Assets/Scripts/Game/ProgressStore.cs`: GameProfile, ProgressStore.
- `PerchSort/Assets/Scripts/Game/PopupFlow.cs`: NotificationKind, Notification, PopupFlow.
- `PerchSort/Assets/Scripts/Game/GameUI.cs`: GameUI, EmptyClick, SafeArea.
- `PerchSort/Assets/Scripts/Game/MapModel.cs`: LevelNodeState, MapModel.
- `PerchSort/Assets/Scripts/Game/BranchView.cs`: BranchView, PerchLimbImage.
- `PerchSort/Assets/Scripts/Game/StuckHint.cs`: StuckHint, StuckHintTail.
- `PerchSort/Assets/Scripts/Core/Solver.cs`: MoveKind, Move, SolveStatus, SolveResult, CompletionOrderMinimum, CompletionOrderResult, Solver.
- `PerchSort/Assets/Scripts/Core/BoardState.cs`: Species, BirdFlags, BranchKind, DayPhase, BoosterKind, Bird, BoosterInventory, BranchState, BoardState.
- `PerchSort/Assets/Scripts/Core/GameRules.cs`: ResolutionBuffer, GameRules.
- `PerchSort/Assets/Scripts/Core/LevelDef.cs`: LevelCatalog, LevelDef, BranchDef.
- `PerchSort/Assets/Scripts/Core/GameEvents.cs`: GameEvent, ResolutionWave, BirdTravel, Transferred, ChicksFollowed, BirdEvent, Hatched, EggRevealed, EggWobbled, Woke, FlockHome, GiftEffect, Gift, BranchGrew, TwigUpgraded, CrowsLeft, Swivelled, PhaseChangeCause, PhaseChanged, BoosterUsed, Won.
- `PerchSort/Assets/Scripts/Core/GameSession.cs`: BoosterResult, GameSession.
- `PerchSort/Assets/Scripts/Audio/AudioManager.cs`: Sfx, MusicPage, AudioManager.

Count: 24 files.

The following 39 UI resource filenames lack a role prefix from the explicit list in CONVENTIONS section 1.5. Some are scenery/effect/fallback resources, so the appropriate role must be established from their actual callers before renaming. Renaming requires complete Resources-key and importer/test updates plus byte-identical .meta movement and Unity import verification. No speculative replacement art or mass string replacement was performed.

- `PerchSort/Assets/Resources/Art/aviary_board.png`
- `PerchSort/Assets/Resources/Art/aviary_frame.png`
- `PerchSort/Assets/Resources/Art/card_wood.png`
- `PerchSort/Assets/Resources/Art/chain_bonus.png`
- `PerchSort/Assets/Resources/Art/check_green.png`
- `PerchSort/Assets/Resources/Art/cloud_soft_1.png`
- `PerchSort/Assets/Resources/Art/cloud_soft_2.png`
- `PerchSort/Assets/Resources/Art/cloud_soft_3.png`
- `PerchSort/Assets/Resources/Art/coin.png`
- `PerchSort/Assets/Resources/Art/coin_pill.png`
- `PerchSort/Assets/Resources/Art/dial_moon.png`
- `PerchSort/Assets/Resources/Art/dial_sun.png`
- `PerchSort/Assets/Resources/Art/firefly.png`
- `PerchSort/Assets/Resources/Art/flag.png`
- `PerchSort/Assets/Resources/Art/footprints.png`
- `PerchSort/Assets/Resources/Art/glow_soft.png`
- `PerchSort/Assets/Resources/Art/hand_pointer.png`
- `PerchSort/Assets/Resources/Art/hoot_sleep_bare.png`
- `PerchSort/Assets/Resources/Art/light_rays.png`
- `PerchSort/Assets/Resources/Art/name_ribbon.png`
- `PerchSort/Assets/Resources/Art/night_overlay.png`
- `PerchSort/Assets/Resources/Art/padlock.png`
- `PerchSort/Assets/Resources/Art/raindrop.png`
- `PerchSort/Assets/Resources/Art/silhouette_frame.png`
- `PerchSort/Assets/Resources/Art/sparkle.png`
- `PerchSort/Assets/Resources/Art/stat_pill.png`
- `PerchSort/Assets/Resources/Art/swivel_knot.png`
- `PerchSort/Assets/Resources/Art/tab_selected_ring.png`
- `PerchSort/Assets/Resources/Art/tool_bell.png`
- `PerchSort/Assets/Resources/Art/tool_branch.png`
- `PerchSort/Assets/Resources/Art/tool_hint.png`
- `PerchSort/Assets/Resources/Art/tool_owl_eye.png`
- `PerchSort/Assets/Resources/Art/tool_rain.png`
- `PerchSort/Assets/Resources/Art/tool_scarecrow.png`
- `PerchSort/Assets/Resources/Art/world_sign_w1.png`
- `PerchSort/Assets/Resources/Art/world_sign_w2.png`
- `PerchSort/Assets/Resources/Art/world_sign_w3.png`
- `PerchSort/Assets/Resources/Art/world_sign_w4.png`
- `PerchSort/Assets/Resources/Art/zzz.png`

Current bird pose resources also use semantic ordinal names such as `sunny_quirk1` and `sunny_quirk2`. They are pose identities, not parallel game versions. Their exact visual roles need inspection before any subject-based rename; numbering must not be removed by guessing.

## Commit and branch review

The live remote initially exposed `main` and `mac-glyph-sizing/work`, with no open PRs. Recent commit subjects described the actual changes. Published history was not rewritten. All new branch, task-document and PR names in this change are subject-based. The hourly task must inspect current refs again rather than assuming these findings remain current.

## Validation

Unity is not installed in the cloud executor; full Unity compilation, EditMode/PlayMode and GUI acceptance were not run. The user's computer must not be used as a fallback. The local cloud executor also has no .NET SDK; exact-commit GitHub CI supplies the available pure-Core test coverage. No full-game acceptance or merge-readiness claim is made.

### Completed cloud checks

- `python3 ci/check_current_game_names.py --self-test`: 13 tests passed, no skips or errors.
- `python3 ci/check_current_game_names.py`: 212 tracked text files inspected; zero violations. The pre-change guard inspected only 200.
- `EVENT_NAME=workflow_dispatch GITHUB_OUTPUT=/tmp/game-naming-ci-output.txt python3 ci/check_changes.py`: 26 materialized JSON/assembly/project/script files validated; Core and workflow jobs selected. Sparse omissions were not counted as parsed files.
- `git diff --check`: passed.
- Three new regression probes were run against the unmodified baseline guard: all three test methods failed as expected (nine failing subtest/test assertions, zero execution errors). They pass against the fix.

An intermediate broadened prefix pattern flagged the existing Unity shader vertex-to-fragment type. An exact shader-only technical-identifier exception and a regression test resolved that false positive; arbitrary version-labelled shader identifiers remain rejected. The intermediate failures were not runtime failures and no shader was changed.

Logs are generated under ignored `build/naming-audit/`; they are not committed artifacts. GitHub CI and any later cloud Unity verification must be reported against the submitted commit rather than inferred from these static checks.
