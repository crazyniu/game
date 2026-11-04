# T52C delivery report — sole current entry

## Delivery

DONE for bounded C implementation on `mac-current-entry/t52`, based on `4b42159891619db23811cbef2b0de84ab742e43e`. Commit SHA is supplied in the terminal/coordinator submission so this report does not self-reference a commit hash. No A/B merge, main merge, primary-checkout edit, new worker, new artwork, gameplay change or save reset was performed.

**Worker runtime validation: NOT_RUN. Integrated stage testing: REQUIRED, current Codex coordinator-owned on the Mac after integrating all deliveries.** This follows the latest user update and supersedes the original no-tests/CI-disable request. C did not launch Unity, a compiler, dotnet, code tests, captures, playthroughs or GUI checks. Automatic PR CI tests remain enabled; no PR or workflow dispatch was opened by this worker.

## Implemented entries

- Archived old Game/Sample scenes and obsolete Editor tools, importers and legacy tests outside Unity at `archive/retired/`. Archived old contracts were not rewritten as current tests. Archived obsolete aspect-capture/front-bird tooling, including old capture assertions with no current equivalent.
- Moved the accepted scene to `PerchSort/Assets/Scenes/Game.unity`; only the root/camera display names and root assembly/class identifier changed. Exactly one build-settings entry is enabled: Game.unity.
- One `Editor/Game/GameSceneBuilder.cs`, `GameArtImporter.cs`, `GameCapture.cs`, namespace **PerchSort.Editor**. Builder imports only current art and creates only Game.unity. No old builder/importer invocation remains.
- Retained PhoneGameView, ReleaseSettings, Audio tests and their nested independent asmdef byte-identically. Shared runtime Audio/importer and all Scripts/Resources/art originals were untouched.
- Moved current tests into Tests/EditMode, Meta and PlayMode with unversioned assemblies/namespaces/types/fixtures and preserved metadata. Migrated all scene/resource/API strings; ButtonTests uses **icon_undo**, not retired ico_undo. Legacy persistence fixture keys remain unchanged.
- `scripts/run_tests.sh` invokes only `PerchSort.Editor.GameSceneBuilder.Build`, then current suites. `scripts/screenshots.sh` invokes only `PerchSort.Editor.GameCapture.Run`; writes `build/screens/game/`, validates expected current stable-state PNGs/dimensions, retains existing map/first-level/mechanic/popup assertions. It does not claim animation/persistence/aspect acceptance.
- Existing `ci/CoreTests.csproj` exists and now compiles real current Core plus 13 retained pure-Core fixture classes and TestBoards (**63 source-expanded cases**). No new tests or archived test inputs. Unity-only suites stay in the required Mac run.
- TestBoards has one **DOTNET_CI fixture-transport branch**: copied fixture JSON is read with framework System.Text.Json and public fields; Unity still uses actual LevelLoader and Application.dataPath. This is not a Game API shim, rule fork or mock. It is the sole non-identity helper change; all test assertions/attributes remain intact.
- CI sparse checkout includes ci/ (therefore the existing project file), current Scripts/Tests/Resources/Packages and scripts, not recursive archived code. Change filtering parses current JSON/asmdef/project XML/shell/Python only and selects current Core/EditMode/fixture inputs for the existing automatic .NET test job. Workflow lint remains intact; auto-merge workflow is unchanged.
- Replaced stale primary README/agent/router/code-map/team/memory entries and added CURRENT_GAME. Current Mac/Codex authority and required testing are explicit. The existing dated handoff is prefaced with the current entry/acceptance contract; its historical evidence remains intact. Older v2/v4 documents are routed as history, never today's runtime; current design filenames remain provenance. No historical research/task report was rewritten.

## Import policy correction required by acceptance

All seven authored baseline metadata files were read, without modifying resource-owner files:

| Assets/Resources/Birds image | Intended pivot | Mesh / max / compression / physics |
| --- | --- | --- |
| egg_idle, egg_wobble_l, egg_wobble_r, egg_crack, egg_shell | (0.5, 0.0625) | Tight / 1024 / Uncompressed, quality100 / no generated fallback shape |
| chick_fluff, mama_bow | (0.5, 0.5) | Tight / 1024 / Uncompressed, quality100 / no generated fallback shape |

The unified importer explicitly handles those seven separately from default full-art pivots/mesh/limits. It must not repeat the old broad importer's egg recentering. Full-art UI borders, house/gift/gate hinge pivots and tall map/level-background 4096 limit remain. The six compressed carried sprites (dial_sun, dial_moon, hand_pointer, night_overlay, raindrop, swivel_knot) retain their original separate policy; it does not apply uncompressed/full-art mesh/border/pivot overrides. B owns metadata delivery and coordinator reconciles the user-generated bird/overlay metadata; C does not edit it.

## Observed static evidence

| Audit | Result |
| --- | --- |
| Archived owned files | **56/56 byte-identical** to base sources, including metas |
| Current scene | Exact original accepted-scene text plus **three identity-line changes only** |
| Scene GUID | **55064aed0051b40279c77a0b9a758533**, scene .meta byte-identical |
| Root-script GUID | **07203ec62b1604449b634c729c1303a3**, serialized script reference retained |
| Font | Exact serialized `font` line retained: GUID **8e611f4ab28034e47937c98d202c1805** |
| Current editor script metas | **3/3 byte-identical** after moves |
| Current test metadata | **37/37 byte-identical**, including current folder/fixture/assembly metas |
| Fixture JSON | **5/5 byte-identical**; only filenames/lookups lose version prefixes |
| Current test sources | **25/25** preserved after required identity normalization; TestBoards has only the disclosed CI transport extension |
| Test assertion/case attributes | No differences beyond mapped identity/fixture strings; no Ignore/Explicit attributes |
| Source-expanded cases | **237 game + 53 Meta + 14 Audio = 304 EditMode; 17 PlayMode** |
| Expanded coverage | LevelsData **81**, including **40 optimal solver/replays**; Layout **41**, including **24 combinatorial** |
| Shared files sampled completely in owned scope | PhoneGameView, ReleaseSettings, their preserved existing metadata, Audio test subtree and apply_release_settings.sh unchanged (source comparison covered 8 shared files; separate metadata baseline is retained) |
| C-owned active GUID/assembly scan | No duplicate GUIDs; exactly the four intended independent test asmdefs |
| Active retired references | No old versioned namespace/type/scene/resource paths or ico_undo in owned runnable sources/tooling |
| Serialization/syntax | Parsed **9 current test JSON/asmdef files**, csproj XML, workflow YAML, CI/inline Python AST; **bash -n** passed for active top-level shell scripts; AGENTS/CLAUDE byte-identical |
| Primary links | All existing primary links resolve; Scripts/Meta and Scripts/Game links intentionally await A integration; no A/B branch was merged here |

SHA-256 values:

- Original accepted scene payload: `7abf6f4baaf18d4557a4a565f8fe29b3dd044c89b2c247b5ba74fa5e20483a0c`.
- Current renamed scene payload: `6b3c31df1f906c958fba20e595de6d2c1457523d863c6b55700c49c5b8e3f08f`.
- Retained current scene metadata: `15dbf7b74e5eb821c87ca5351d8b056ee6bfa2802556265f3ca26c7e97c5b985`.
- Archived old Game scene: `d9442f54cfe70ad31bd85bf0184dcd407445f3157345f0074acacf21ccf64b70`.

The suite counts were reproduced from source attributes/Range/Values expansion without test discovery/execution. run_tests safeguards require 304/17 overall, 237/53/14 suite breakdown and 81/41 fixture coverage, no skipped/non-passing case, consistent result totals. Added cases may raise counts. The obsolete 101/11 floors and old 163/14(+8 skipped) history do not validate or protect this migration.

## Compatibility, mapping and remaining verification risk

No shared mapping change is proposed. Editor namespace PerchSort.Editor is explicitly documented. Version-looking PlayerPrefs IDs remain compatibility identifiers only; every `v5.*`, `v6.unlockSeen.*`, `v7.notifications` fixture input is retained. C changes no migrator/DataMember/SaveData/schema/enum or runtime save logic.

- A/B names/resources are the **integration contract**, not files merged into this isolated C branch. This branch alone is not a standalone integrated build; coordinator must combine A/B/C before compilation/CI/Mac acceptance.
- Required Mac tests must confirm actual discovery at or above the retained inventory and investigate discrepancies rather than weaken guards. The .NET project's 63-case subset and DOTNET_CI transport have **not** been compiled/executed here; Unity resource/parser tests and 40 full-level solver/replays remain in the retained Unity suite.
- Coordinator reported primary-checkout scene InputSystem action-ID rewrites plus 108 generated bird metas/seven changed overlay metas. C preserved all original scene InputSystem fields; it did not inspect or mutate primary checkout. Coordinator must preserve user changes, reconcile B metadata, import/rebuild, inspect the actual scene/root/font/input setup and exercise real startup before entry acceptance.
- Unity import policy/API interpretation (including Tight meshes, custom egg pivots and fallback physics setting), assembly resolution and all live visual/input/persistence/timing behavior remain **NOT_RUN locally**. Stable instant capture cannot prove real motion or device/safe-area behavior. Required GUI acceptance must cover those surfaces.
- No C# language server is configured; this was confirmed before mechanical source migration. No compiler was used as a substitute. Runtime acceptance is not claimed from text/hash checks.

## Exact changed-path inventory

The list includes removed source paths and added current/archive paths; paired meta paths are spelled out. Unlisted owned shared files are unchanged. No additional temporary files are part of delivery.

```text
.github/workflows/ci.yml
AGENTS.md
CLAUDE.md
PerchSort/Assets/Editor/ArtImporter.cs
PerchSort/Assets/Editor/ArtImporter.cs.meta
PerchSort/Assets/Editor/AspectCaptureSequence.cs
PerchSort/Assets/Editor/AspectCaptureSequence.cs.meta
PerchSort/Assets/Editor/FirstFiveCaptureSequence.cs
PerchSort/Assets/Editor/FirstFiveCaptureSequence.cs.meta
PerchSort/Assets/Editor/Game.meta
PerchSort/Assets/Editor/Game/GameArtImporter.cs
PerchSort/Assets/Editor/Game/GameArtImporter.cs.meta
PerchSort/Assets/Editor/Game/GameCapture.cs
PerchSort/Assets/Editor/Game/GameCapture.cs.meta
PerchSort/Assets/Editor/Game/GameSceneBuilder.cs
PerchSort/Assets/Editor/Game/GameSceneBuilder.cs.meta
PerchSort/Assets/Editor/LevelBuilder.cs
PerchSort/Assets/Editor/LevelBuilder.cs.meta
PerchSort/Assets/Editor/PerchCaptureSequence.cs
PerchSort/Assets/Editor/PerchCaptureSequence.cs.meta
PerchSort/Assets/Editor/SceneBuilder.cs
PerchSort/Assets/Editor/SceneBuilder.cs.meta
PerchSort/Assets/Editor/Screenshots.cs
PerchSort/Assets/Editor/Screenshots.cs.meta
PerchSort/Assets/Editor/UIAssetBuilder.cs
PerchSort/Assets/Editor/UIAssetBuilder.cs.meta
PerchSort/Assets/Editor/UiFlowCaptureSequence.cs
PerchSort/Assets/Editor/UiFlowCaptureSequence.cs.meta
PerchSort/Assets/Editor/V5.meta
PerchSort/Assets/Editor/V5/V5ArtImporter.cs
PerchSort/Assets/Editor/V5/V5ArtImporter.cs.meta
PerchSort/Assets/Editor/V5/V5Capture.cs
PerchSort/Assets/Editor/V5/V5Capture.cs.meta
PerchSort/Assets/Editor/V5/V5SceneBuilder.cs
PerchSort/Assets/Editor/V5/V5SceneBuilder.cs.meta
PerchSort/Assets/Editor/V5/V6ArtImporter.cs
PerchSort/Assets/Editor/V5/V6ArtImporter.cs.meta
PerchSort/Assets/Editor/V5/V7ArtImporter.cs
PerchSort/Assets/Editor/V5/V7ArtImporter.cs.meta
PerchSort/Assets/Scenes/Game.unity
PerchSort/Assets/Scenes/Game.unity.meta
PerchSort/Assets/Scenes/SampleScene.unity
PerchSort/Assets/Scenes/SampleScene.unity.meta
PerchSort/Assets/Scenes/V5.unity
PerchSort/Assets/Scenes/V5.unity.meta
PerchSort/Assets/Tests/EditMode.meta
PerchSort/Assets/Tests/EditMode/ArtResourceTests.cs
PerchSort/Assets/Tests/EditMode/ArtResourceTests.cs.meta
PerchSort/Assets/Tests/EditMode/AspectTests.cs
PerchSort/Assets/Tests/EditMode/AspectTests.cs.meta
PerchSort/Assets/Tests/EditMode/BoosterTests.cs
PerchSort/Assets/Tests/EditMode/BoosterTests.cs.meta
PerchSort/Assets/Tests/EditMode/BranchTests.cs
PerchSort/Assets/Tests/EditMode/BranchTests.cs.meta
PerchSort/Assets/Tests/EditMode/ButtonTests.cs
PerchSort/Assets/Tests/EditMode/ButtonTests.cs.meta
PerchSort/Assets/Tests/EditMode/ChorusEncoreTests.cs
PerchSort/Assets/Tests/EditMode/ChorusEncoreTests.cs.meta
PerchSort/Assets/Tests/EditMode/CoreTests.cs
PerchSort/Assets/Tests/EditMode/CoreTests.cs.meta
PerchSort/Assets/Tests/EditMode/CrowTests.cs
PerchSort/Assets/Tests/EditMode/CrowTests.cs.meta
PerchSort/Assets/Tests/EditMode/DayNightTests.cs
PerchSort/Assets/Tests/EditMode/DayNightTests.cs.meta
PerchSort/Assets/Tests/EditMode/DevOptionsTests.cs
PerchSort/Assets/Tests/EditMode/DevOptionsTests.cs.meta
PerchSort/Assets/Tests/EditMode/EggTests.cs
PerchSort/Assets/Tests/EditMode/EggTests.cs.meta
PerchSort/Assets/Tests/EditMode/FeedbackTests.cs
PerchSort/Assets/Tests/EditMode/FeedbackTests.cs.meta
PerchSort/Assets/Tests/EditMode/Fixtures.meta
PerchSort/Assets/Tests/EditMode/Fixtures/crows_swivel.json
PerchSort/Assets/Tests/EditMode/Fixtures/crows_swivel.json.meta
PerchSort/Assets/Tests/EditMode/Fixtures/eggs_gifts.json
PerchSort/Assets/Tests/EditMode/Fixtures/eggs_gifts.json.meta
PerchSort/Assets/Tests/EditMode/Fixtures/family_sleep.json
PerchSort/Assets/Tests/EditMode/Fixtures/family_sleep.json.meta
PerchSort/Assets/Tests/EditMode/Fixtures/growth_ember.json
PerchSort/Assets/Tests/EditMode/Fixtures/growth_ember.json.meta
PerchSort/Assets/Tests/EditMode/Fixtures/night_owls.json
PerchSort/Assets/Tests/EditMode/Fixtures/night_owls.json.meta
PerchSort/Assets/Tests/EditMode/GiftTests.cs
PerchSort/Assets/Tests/EditMode/GiftTests.cs.meta
PerchSort/Assets/Tests/EditMode/LayoutTests.cs
PerchSort/Assets/Tests/EditMode/LayoutTests.cs.meta
PerchSort/Assets/Tests/EditMode/LevelTests.cs
PerchSort/Assets/Tests/EditMode/LevelTests.cs.meta
PerchSort/Assets/Tests/EditMode/LevelsDataTests.cs
PerchSort/Assets/Tests/EditMode/LevelsDataTests.cs.meta
PerchSort/Assets/Tests/EditMode/MamaTests.cs
PerchSort/Assets/Tests/EditMode/MamaTests.cs.meta
PerchSort/Assets/Tests/EditMode/MapTests.cs
PerchSort/Assets/Tests/EditMode/MapTests.cs.meta
PerchSort/Assets/Tests/EditMode/PerchSort.Core.EditModeTests.asmdef
PerchSort/Assets/Tests/EditMode/PerchSort.Core.EditModeTests.asmdef.meta
PerchSort/Assets/Tests/EditMode/PerchSort.EditModeTests.asmdef
PerchSort/Assets/Tests/EditMode/PerchSort.EditModeTests.asmdef.meta
PerchSort/Assets/Tests/EditMode/PersistenceTests.cs
PerchSort/Assets/Tests/EditMode/PersistenceTests.cs.meta
PerchSort/Assets/Tests/EditMode/ResolutionTests.cs
PerchSort/Assets/Tests/EditMode/ResolutionTests.cs.meta
PerchSort/Assets/Tests/EditMode/RulesTests.cs
PerchSort/Assets/Tests/EditMode/RulesTests.cs.meta
PerchSort/Assets/Tests/EditMode/SchemaTests.cs
PerchSort/Assets/Tests/EditMode/SchemaTests.cs.meta
PerchSort/Assets/Tests/EditMode/SleepTests.cs
PerchSort/Assets/Tests/EditMode/SleepTests.cs.meta
PerchSort/Assets/Tests/EditMode/SolverTests.cs
PerchSort/Assets/Tests/EditMode/SolverTests.cs.meta
PerchSort/Assets/Tests/EditMode/SwivelTests.cs
PerchSort/Assets/Tests/EditMode/SwivelTests.cs.meta
PerchSort/Assets/Tests/EditMode/TestBoards.cs
PerchSort/Assets/Tests/EditMode/TestBoards.cs.meta
PerchSort/Assets/Tests/EditMode/UiFlowTests.cs
PerchSort/Assets/Tests/EditMode/UiFlowTests.cs.meta
PerchSort/Assets/Tests/EditMode/UiKitTests.cs
PerchSort/Assets/Tests/EditMode/UiKitTests.cs.meta
PerchSort/Assets/Tests/EditMode/ViewPolicyTests.cs
PerchSort/Assets/Tests/EditMode/ViewPolicyTests.cs.meta
PerchSort/Assets/Tests/EditMode/ViewSessionTests.cs
PerchSort/Assets/Tests/EditMode/ViewSessionTests.cs.meta
PerchSort/Assets/Tests/Meta.meta
PerchSort/Assets/Tests/Meta/CollectionProgressionTests.cs
PerchSort/Assets/Tests/Meta/CollectionProgressionTests.cs.meta
PerchSort/Assets/Tests/Meta/PerchSort.Meta.EditModeTests.asmdef
PerchSort/Assets/Tests/Meta/PerchSort.Meta.EditModeTests.asmdef.meta
PerchSort/Assets/Tests/Meta/ProfileEconomyTests.cs
PerchSort/Assets/Tests/Meta/ProfileEconomyTests.cs.meta
PerchSort/Assets/Tests/PlayMode.meta
PerchSort/Assets/Tests/PlayMode/GameplaySmokeTests.cs
PerchSort/Assets/Tests/PlayMode/GameplaySmokeTests.cs.meta
PerchSort/Assets/Tests/PlayMode/GameplayTests.cs
PerchSort/Assets/Tests/PlayMode/GameplayTests.cs.meta
PerchSort/Assets/Tests/PlayMode/PerchSort.Gameplay.PlayModeTests.asmdef
PerchSort/Assets/Tests/PlayMode/PerchSort.Gameplay.PlayModeTests.asmdef.meta
PerchSort/Assets/Tests/PlayMode/PerchSort.PlayModeTests.asmdef
PerchSort/Assets/Tests/PlayMode/PerchSort.PlayModeTests.asmdef.meta
PerchSort/Assets/Tests/PlayMode/UiSpecTests.cs
PerchSort/Assets/Tests/PlayMode/UiSpecTests.cs.meta
PerchSort/Assets/Tests/V5.meta
PerchSort/Assets/Tests/V5/EditMode.meta
PerchSort/Assets/Tests/V5/EditMode/Fixtures.meta
PerchSort/Assets/Tests/V5/EditMode/Fixtures/v53_crows_swivel.json
PerchSort/Assets/Tests/V5/EditMode/Fixtures/v53_crows_swivel.json.meta
PerchSort/Assets/Tests/V5/EditMode/Fixtures/v53_eggs_gifts.json
PerchSort/Assets/Tests/V5/EditMode/Fixtures/v53_eggs_gifts.json.meta
PerchSort/Assets/Tests/V5/EditMode/Fixtures/v53_family_sleep.json
PerchSort/Assets/Tests/V5/EditMode/Fixtures/v53_family_sleep.json.meta
PerchSort/Assets/Tests/V5/EditMode/Fixtures/v53_growth_ember.json
PerchSort/Assets/Tests/V5/EditMode/Fixtures/v53_growth_ember.json.meta
PerchSort/Assets/Tests/V5/EditMode/Fixtures/v53_night_owls.json
PerchSort/Assets/Tests/V5/EditMode/Fixtures/v53_night_owls.json.meta
PerchSort/Assets/Tests/V5/EditMode/PerchSort.V5.EditModeTests.asmdef
PerchSort/Assets/Tests/V5/EditMode/PerchSort.V5.EditModeTests.asmdef.meta
PerchSort/Assets/Tests/V5/EditMode/V53BoosterTests.cs
PerchSort/Assets/Tests/V5/EditMode/V53BoosterTests.cs.meta
PerchSort/Assets/Tests/V5/EditMode/V53BranchTests.cs
PerchSort/Assets/Tests/V5/EditMode/V53BranchTests.cs.meta
PerchSort/Assets/Tests/V5/EditMode/V53CrowTests.cs
PerchSort/Assets/Tests/V5/EditMode/V53CrowTests.cs.meta
PerchSort/Assets/Tests/V5/EditMode/V53DayNightTests.cs
PerchSort/Assets/Tests/V5/EditMode/V53DayNightTests.cs.meta
PerchSort/Assets/Tests/V5/EditMode/V53EggTests.cs
PerchSort/Assets/Tests/V5/EditMode/V53EggTests.cs.meta
PerchSort/Assets/Tests/V5/EditMode/V53GiftTests.cs
PerchSort/Assets/Tests/V5/EditMode/V53GiftTests.cs.meta
PerchSort/Assets/Tests/V5/EditMode/V53MamaTests.cs
PerchSort/Assets/Tests/V5/EditMode/V53MamaTests.cs.meta
PerchSort/Assets/Tests/V5/EditMode/V53ResolutionTests.cs
PerchSort/Assets/Tests/V5/EditMode/V53ResolutionTests.cs.meta
PerchSort/Assets/Tests/V5/EditMode/V53SchemaTests.cs
PerchSort/Assets/Tests/V5/EditMode/V53SchemaTests.cs.meta
PerchSort/Assets/Tests/V5/EditMode/V53SleepTests.cs
PerchSort/Assets/Tests/V5/EditMode/V53SleepTests.cs.meta
PerchSort/Assets/Tests/V5/EditMode/V53SolverTests.cs
PerchSort/Assets/Tests/V5/EditMode/V53SolverTests.cs.meta
PerchSort/Assets/Tests/V5/EditMode/V53SwivelTests.cs
PerchSort/Assets/Tests/V5/EditMode/V53SwivelTests.cs.meta
PerchSort/Assets/Tests/V5/EditMode/V53TestBoards.cs
PerchSort/Assets/Tests/V5/EditMode/V53TestBoards.cs.meta
PerchSort/Assets/Tests/V5/EditMode/V54ChorusEncoreTests.cs
PerchSort/Assets/Tests/V5/EditMode/V54ChorusEncoreTests.cs.meta
PerchSort/Assets/Tests/V5/EditMode/V5ButtonTests.cs
PerchSort/Assets/Tests/V5/EditMode/V5ButtonTests.cs.meta
PerchSort/Assets/Tests/V5/EditMode/V5DevOptionsTests.cs
PerchSort/Assets/Tests/V5/EditMode/V5DevOptionsTests.cs.meta
PerchSort/Assets/Tests/V5/EditMode/V5LayoutTests.cs
PerchSort/Assets/Tests/V5/EditMode/V5LayoutTests.cs.meta
PerchSort/Assets/Tests/V5/EditMode/V5LevelsDataTests.cs
PerchSort/Assets/Tests/V5/EditMode/V5LevelsDataTests.cs.meta
PerchSort/Assets/Tests/V5/EditMode/V5RulesTests.cs
PerchSort/Assets/Tests/V5/EditMode/V5RulesTests.cs.meta
PerchSort/Assets/Tests/V5/EditMode/V6MapTests.cs
PerchSort/Assets/Tests/V5/EditMode/V6MapTests.cs.meta
PerchSort/Assets/Tests/V5/EditMode/ViewPolicyTests.cs
PerchSort/Assets/Tests/V5/EditMode/ViewPolicyTests.cs.meta
PerchSort/Assets/Tests/V5/EditMode/ViewSessionTests.cs
PerchSort/Assets/Tests/V5/EditMode/ViewSessionTests.cs.meta
PerchSort/Assets/Tests/V5/Meta.meta
PerchSort/Assets/Tests/V5/Meta/CollectionProgressionTests.cs
PerchSort/Assets/Tests/V5/Meta/CollectionProgressionTests.cs.meta
PerchSort/Assets/Tests/V5/Meta/PerchSort.V5.Meta.EditModeTests.asmdef
PerchSort/Assets/Tests/V5/Meta/PerchSort.V5.Meta.EditModeTests.asmdef.meta
PerchSort/Assets/Tests/V5/Meta/ProfileEconomyTests.cs
PerchSort/Assets/Tests/V5/Meta/ProfileEconomyTests.cs.meta
PerchSort/Assets/Tests/V5/PlayMode.meta
PerchSort/Assets/Tests/V5/PlayMode/PerchSort.V5.PlayModeTests.asmdef
PerchSort/Assets/Tests/V5/PlayMode/PerchSort.V5.PlayModeTests.asmdef.meta
PerchSort/Assets/Tests/V5/PlayMode/V5GameplayTests.cs
PerchSort/Assets/Tests/V5/PlayMode/V5GameplayTests.cs.meta
PerchSort/ProjectSettings/EditorBuildSettings.asset
README.md
archive/retired/PerchSort/Assets/Editor/ArtImporter.cs
archive/retired/PerchSort/Assets/Editor/ArtImporter.cs.meta
archive/retired/PerchSort/Assets/Editor/AspectCaptureSequence.cs
archive/retired/PerchSort/Assets/Editor/AspectCaptureSequence.cs.meta
archive/retired/PerchSort/Assets/Editor/FirstFiveCaptureSequence.cs
archive/retired/PerchSort/Assets/Editor/FirstFiveCaptureSequence.cs.meta
archive/retired/PerchSort/Assets/Editor/LevelBuilder.cs
archive/retired/PerchSort/Assets/Editor/LevelBuilder.cs.meta
archive/retired/PerchSort/Assets/Editor/PerchCaptureSequence.cs
archive/retired/PerchSort/Assets/Editor/PerchCaptureSequence.cs.meta
archive/retired/PerchSort/Assets/Editor/SceneBuilder.cs
archive/retired/PerchSort/Assets/Editor/SceneBuilder.cs.meta
archive/retired/PerchSort/Assets/Editor/Screenshots.cs
archive/retired/PerchSort/Assets/Editor/Screenshots.cs.meta
archive/retired/PerchSort/Assets/Editor/UIAssetBuilder.cs
archive/retired/PerchSort/Assets/Editor/UIAssetBuilder.cs.meta
archive/retired/PerchSort/Assets/Editor/UiFlowCaptureSequence.cs
archive/retired/PerchSort/Assets/Editor/UiFlowCaptureSequence.cs.meta
archive/retired/PerchSort/Assets/Editor/V5/V5ArtImporter.cs
archive/retired/PerchSort/Assets/Editor/V5/V5ArtImporter.cs.meta
archive/retired/PerchSort/Assets/Editor/V5/V6ArtImporter.cs
archive/retired/PerchSort/Assets/Editor/V5/V6ArtImporter.cs.meta
archive/retired/PerchSort/Assets/Scenes/Game.unity
archive/retired/PerchSort/Assets/Scenes/Game.unity.meta
archive/retired/PerchSort/Assets/Scenes/SampleScene.unity
archive/retired/PerchSort/Assets/Scenes/SampleScene.unity.meta
archive/retired/PerchSort/Assets/Tests/EditMode.meta
archive/retired/PerchSort/Assets/Tests/EditMode/ArtResourceTests.cs
archive/retired/PerchSort/Assets/Tests/EditMode/ArtResourceTests.cs.meta
archive/retired/PerchSort/Assets/Tests/EditMode/AspectTests.cs
archive/retired/PerchSort/Assets/Tests/EditMode/AspectTests.cs.meta
archive/retired/PerchSort/Assets/Tests/EditMode/CoreTests.cs
archive/retired/PerchSort/Assets/Tests/EditMode/CoreTests.cs.meta
archive/retired/PerchSort/Assets/Tests/EditMode/FeedbackTests.cs
archive/retired/PerchSort/Assets/Tests/EditMode/FeedbackTests.cs.meta
archive/retired/PerchSort/Assets/Tests/EditMode/LevelTests.cs
archive/retired/PerchSort/Assets/Tests/EditMode/LevelTests.cs.meta
archive/retired/PerchSort/Assets/Tests/EditMode/PerchSort.Core.EditModeTests.asmdef
archive/retired/PerchSort/Assets/Tests/EditMode/PerchSort.Core.EditModeTests.asmdef.meta
archive/retired/PerchSort/Assets/Tests/EditMode/PersistenceTests.cs
archive/retired/PerchSort/Assets/Tests/EditMode/PersistenceTests.cs.meta
archive/retired/PerchSort/Assets/Tests/EditMode/UiFlowTests.cs
archive/retired/PerchSort/Assets/Tests/EditMode/UiFlowTests.cs.meta
archive/retired/PerchSort/Assets/Tests/EditMode/UiKitTests.cs
archive/retired/PerchSort/Assets/Tests/EditMode/UiKitTests.cs.meta
archive/retired/PerchSort/Assets/Tests/PlayMode.meta
archive/retired/PerchSort/Assets/Tests/PlayMode/GameplaySmokeTests.cs
archive/retired/PerchSort/Assets/Tests/PlayMode/GameplaySmokeTests.cs.meta
archive/retired/PerchSort/Assets/Tests/PlayMode/PerchSort.Gameplay.PlayModeTests.asmdef
archive/retired/PerchSort/Assets/Tests/PlayMode/PerchSort.Gameplay.PlayModeTests.asmdef.meta
archive/retired/PerchSort/Assets/Tests/PlayMode/UiSpecTests.cs
archive/retired/PerchSort/Assets/Tests/PlayMode/UiSpecTests.cs.meta
archive/retired/PerchSort/Assets/Tests/V5.meta
archive/retired/scripts/screenshots.sh
archive/retired/scripts/screenshots_aspect.sh
archive/retired/scripts/sync_front_birds.sh
ci/CoreTests.csproj
ci/check_changes.py
docs/CURRENT_GAME.md
docs/README.md
docs/spec/CODE_MAP.md
docs/team/HANDOFF_2026-11-04_CODEX.md
docs/team/MEMORY.md
docs/team/TEAM.md
scripts/run_tests.sh
scripts/screenshots.sh
scripts/screenshots_aspect.sh
scripts/sync_front_birds.sh
tasks/T52C_current_entry.md
tasks/T52C_report.md
```
