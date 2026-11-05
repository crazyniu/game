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


## Resumed Mac acceptance and follow-up

The user subsequently authorized their Mac again, superseding the cloud-only restriction. Historical cloud-stage results above remain cloud results. Work resumed in the existing isolated Orca worktree `/Users/apple/orca/workspaces/game/naming-audit`, on the existing PR branch `naming-policy`; no duplicate PR or tooling implementation was created. The abandoned local `naming-audit` branch remains at the original baseline with no authored edits.

- Verified fetched PR head: `8c74313dfe7007e7c1ea2239b06e142a362fc800`.
- Verified tree: `38199b25d71da0e59717a6248838e4e48f146678`.
- PR: https://github.com/crazyniu/game/pull/115 (draft; no merge/deploy performed).
- Exact-head cloud CI: https://github.com/crazyniu/game/actions/runs/41997403533/job/127143853114 (success). The separate Auto-merge enable job was skipped for the draft.
- Initial isolated worktree status was clean. Process inspection found no active Unity or test process; `flock -n /tmp/unity.lock true` succeeded. No user process was stopped.
- `python3 ci/check_current_game_names.py --self-test`: 13/13 passed, no errors or skips.
- `python3 ci/check_current_game_names.py`: 212 tracked text files, zero violations.
- `EVENT_NAME=workflow_dispatch GITHUB_OUTPUT=/tmp/game-naming-mac-ci-output.txt python3 ci/check_changes.py`: 28 current JSON/assembly/project/script files validated; Core and workflow checks selected.
- Before Unity, SHA-256 state was recorded for 1,764 tracked scene/meta/settings/package files in ignored `build/naming-audit/pre-unity-state.json`.
- Unity command: `flock /tmp/unity.lock bash scripts/run_tests.sh`, Unity 6000.6.7f1. Full results and generated-file reconciliation are recorded below after completion.

### Concrete follow-up proposals requiring coordination

No runtime refactoring was mixed into this small guard PR. In particular, GameRoot.cs, PopupFlow.cs and the separate homecoming-count test task remain untouched.

A bounded first type/file cleanup can move `ProfileMigrator` from `Scripts/Meta/ProfileText.cs` to `Scripts/Meta/ProfileMigrator.cs`, and the `Profiles` / `FakeClock` test helpers from `Tests/Meta/ProfileEconomyTests.cs` to `Profiles.cs` / `FakeClock.cs` in that same test assembly. Preserve the existing owning file GUIDs, namespaces, declaration text and exact migrator key literals; generate distinct new file GUIDs and extend the exact owner-scoped storage-key guard allowance if the migrator moves. Require the full profile compatibility tests and engine suites. The 24-file inventory above is the full affected-file planning boundary, not authority to split unrelated owners' files during their work.

For resource cleanup, actual callers establish these candidate roles: `aviary_board` -> `panel_aviary_board` (AviaryView), `aviary_frame` -> `panel_portrait_frame` (AviaryView/ShopView/PanelsView), `name_ribbon` -> `ribbon_name` (AviaryView/PanelsView and GameArtImporter border switch), `stat_pill` -> `panel_stat_pill` (SleepBubble/WinPanel/ShopView/AviaryView/PanelsView and GameArtImporter border switch), `coin_pill` -> `panel_coin_pill` (HudView/ShopView), `check_green` -> `icon_check_green` (AviaryView), `padlock` -> `icon_padlock` (AviaryView/ShopView), `tab_selected_ring` -> `badge_selected_tab` (ShopView). These are proposed resource identities only: layout measurement keys of the same spelling in GameLayout and approved metadata are separate identities and must not be renamed by global replacement. Each move must carry its exact .meta and asset bytes and update all authored resource/import/test/tool references; engine import and relevant screen validation remain prerequisites. No artwork or resource was renamed in this PR.

Local branch inventory exposed 52 legacy task/version-labelled refs, including `t30-v5-first-level`, `mac-art/ui7`, `mac-current-runtime/t52` and `mac-t54a/t54`. These were not deleted or renamed because branch ownership/history belongs to the coordinator. In a bounded sample of the latest 250 commits across local refs, six subjects contained historical task labels: a7679d97, c081b789, 98265152, ecdab196, 47d5e7e2, da74b638. Their subjects still describe their changes; no published history was rewritten. Current remote main remained `38fca7094230cc628380a8e86ec32af92576b456`, and the only open PR at recheck was this naming PR.


### Completed Mac engine results

The serialized `scripts/run_tests.sh` command exited 0. Unity scene build/compilation succeeded. EditMode: **318/318 passed**, zero failed/inconclusive/skipped (2026-11-05 08:54:00–08:54:14 UTC). Breakdown: game 251, Meta 53, shared Audio 14; retained LevelsData 81 and Layout 41 cases remain present. PlayMode: **119/119 passed**, zero failed/inconclusive/skipped (08:54:20–09:00:05 UTC). No tests were changed, filtered, weakened or rerun.

Evidence in the isolated worktree:

- `build/scene-build.log`: compile/import and scene build, exit 0.
- `build/test-results.xml` and `build/editmode-tests.log`: complete EditMode result.
- `build/playmode-results.xml` and `build/playmode-tests.log`: complete PlayMode result.
- `build/naming-audit/unity-acceptance.log`: combined script result and exact counts.
- `build/naming-audit/pre-unity-state.json`: original 1,764-file hash snapshot.
- `build/naming-audit/post-unity-changes.json` and `generated-unity.patch`: classified generated import/scene differences, preserved before restoring the reviewed files.
- `build/naming-audit/naming-self-tests.log` and `naming-check.log`: post-engine naming rerun, 13 tests and 212 files with zero violations.

Unity generated changes to 908 tracked files: 894 whitespace-only meta changes, seven egg/family meta serialization completions, six world gate-door pivot resets, and scene object-ID regeneration. All meta GUIDs were preserved; package and project settings stayed unchanged. The seven egg/family overlays retained their key pivot, mesh, 1024 size, compression and physics-shape settings. However, the six `Resources/World/gate_door_{left,right}_w{1,2,3}.png.meta` pivots became centered `(0.5, 0.5)` instead of their recorded hinges (left x=0.025 / right x=0.975, y=0.5): the existing GameArtImporter special cases match only un-suffixed gate-door names. This is an independent importer correctness finding, not a naming-tool regression; it was not fixed in another owner's runtime scope. The generated patch is preserved for coordinator review.

After the engine exited, only these reviewed generated files were restored to their exact pre-run hashes, after checking the HEAD blobs matched the recorded baseline. The isolated worktree had been clean initially, so no unrelated user edits were restored or removed. No generated asset/meta/scene changes are included in this PR. These results validate the normal build/import/test workflow at the fetched source revision; they do not certify the unimported serialized scene bytes or unchanged gate hinge visuals.

No UI/art/resource changes were made, so `scripts/screenshots.sh`, timed visual/audio recording, device testing and listening acceptance were not run. Full suites passing does not claim that broader 24-file/39-resource naming refactoring is complete. Local .NET CLI was unavailable; pure-Core acceptance remains the exact-head GitHub CI's 63/63 passing tests, zero skips. The final report-only commit has identical runtime/tooling content to the Mac-tested head; its own CI must also pass before coordinator merge review. The PR remains draft and no merge or deployment was performed.
