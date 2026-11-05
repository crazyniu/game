# T52A — Current runtime delivery report

## Outcome

**DONE — bounded Scripts implementation and static audits.** Base: `4b42159891619db23811cbef2b0de84ab742e43e`. Delivery branch: `mac-current-runtime/t52`. Commit SHA and push receipt are provided in the final worker terminal delivery. No merge, additional worker, primary-checkout edit, runtime execution, compiler, Unity, automated test, screenshot, or playthrough was performed.

The latest user directive supersedes the original stage-wide no-tests instruction: **Mac compilation, EditMode, PlayMode, and GUI acceptance are REQUIRED after integration and owned/serialized by the current Codex coordinator.** Worker-local runtime checks remain **NOT_RUN**, not passed and not evidence of stage acceptance. No tests, assertions, CI settings, or test minimum-count policies were removed or disabled by T52A.

## Implementation

- Archived separate legacy Core/UI/View and their folder metas before installing current Core. Archives are outside Unity under `archive/retired/PerchSort/Assets/Scripts/`; shared Audio is untouched.
- Moved current Core/Meta/Game out of Scripts/V5 with original source/assembly/folder metadata. Namespaces and assembly identities are now `PerchSort.Core`, `PerchSort.Meta`, and `PerchSort.Game`; Core's friend assembly is `PerchSort.EditModeTests`.
- Applied all 32 requested type mappings without top-level current-namespace collisions. Preserved multi-type source organization: `LongPress` and `PopupKind` remain in `PanelsView.cs`; `GameProfile` remains in `ProgressStore.cs`; `EmptyClick`/`SafeArea` remain in `GameUI.cs`; `LevelNodeState` remains in `MapModel.cs`; `MapInput` remains in `MapView.cs`; `Notification`/`NotificationKind` remain in `PopupFlow.cs`. No new script GUIDs or shims were introduced.
- Changed art routing to `Birds/<key>`, `World/<key>`, and `Art/<key>`; changed levels/development lookup keys to `Levels/levels` and `dev_options`. Updated BirdOverlay's direct load too.
- Removed only GameArt's retired V6/V5 resource fallback and associated legacy gate cropping after static resource confirmation. Current portrait fallback and generated door mask remain unchanged; they are not versioned legacy-art lookup paths. Current optional animation-part fallbacks remain unchanged.
- Updated active-version object names, error/log text, and overlay comments. The truthful original-art provenance `art/approved_ui/layout.json` remains in GameLayout; originals were not edited.
- Archived obsolete `Scripts/V5.meta` to retired Scripts rather than dropping its bytes. This is a wrapper metadata retirement, not another runtime project.

## Compatibility and behavior preservation

Historical PlayerPrefs IDs are storage compatibility identifiers, not parallel runtimes. The comment at `ProgressStore` records why they remain. These strings are unchanged: `v5.coins`, `v5.muted`, `v5.highestUnlocked`, `v5.bestStars.`, `v5.tutorial.`, `v5.booster.`, `v5.starter.`, `v5.bestMoves.`, `v6.unlockSeen.`, and `v7.notifications`.

All ProfileMigrator reads, ProfileText DataMember names, popup SaveData fields, schema versions, enum numeric order, serialized field names, and the `GameRoot.font` field remain unchanged. The only source changes outside mapped identities, agreed resource keys, explanatory comments, and diagnostic/object text are the authorized retired-art fallback removal. No gameplay, economy, transaction, animation timing/cancellation, or save/reset redesign was made.

T50 `transactionGeneration` remains independent from presentation/hint `revision`. The declaration and every line using either token match baseline byte-for-byte (17 lines), including committed Board.Play completion ownership and cancellation/destruction invalidation.

## Static evidence

A pre-mutation SHA-256 inventory covered all 201 files under Scripts. The final mapping audit found exactly 201 files across current and retired Scripts, with no missing/extra files:

| Audited set | Files | Result |
| --- | ---: | --- |
| Archived legacy Core/UI/View, including folder metas | 85 | Baseline bytes preserved |
| Retained current source/assembly/folder metas | 52 | Baseline bytes preserved |
| Shared Audio subtree and its folder meta | 14 | Baseline bytes preserved |
| Retired V5 wrapper meta | 1 | Baseline bytes preserved |
| Current C# and assembly sources | 49 | Exact authorized whole-file transformation verified |

The source audit reconstructed original source bytes from complete snapshots and checked their baseline hashes, then compared every current source hash with the exact bounded transformation. Core changes are namespace identity and friend-assembly identity only. This is a static source check, not executed gameplay proof.

Root script GUID remains `07203ec62b1604449b634c729c1303a3`. Every retained current `.meta` preserved its entire original content, not just its GUID. No active V5/V6/V7 type/namespace/assembly/resource prefix remains under Scripts. The remaining versioned text is the documented storage IDs and original-art provenance above.

Before fallback removal, static inspection confirmed 187 Birds + 32 World + 126 Art PNGs in current V7 resources, the six agreed V5 support PNG/meta pairs, and current `gate_open`/`gate_locked` PNG/meta pairs. A static key-routing audit covered all 351 agreed production PNG keys: every key routes to its intended unversioned directory with no duplicate key or routing mismatch. All 37 direct literal mandatory `art.Get`/`art.Icon` keys exist in that set; dynamic key construction is preserved by the exact-source audit. Actual Unity import/load behavior is **NOT_RUN**.

Read-only data inspection found 40 authored levels (IDs 1 through 40) and current flags `unlockAllLevels=true`, `grantTestStock=true`. T52A did not modify resources or payloads.

No C# language server is configured (`references` returned “No language server found for this action”). A structural bare-identifier rename proposal also reported an invalid C# pattern and made no changes. The implementation therefore used explicit bounded identity patches, with complete source/mapped-byte/reference audits instead of claiming compiler or language-server validation.

## Integration dependencies and test migration risks

1. **T52B must integrate first/before execution:** current loaders now require the agreed unversioned resource destinations, including the six retained support PNGs. This isolated branch intentionally does not move Resources.
2. **T52C must integrate before compilation/tests:** Editor/Game tools, active Tests/V5 assembly/import/type references, and Scenes/V5 currently retain old identities in this isolated checkout. They belong to C, not A. Current friend assemblies must be `PerchSort.EditModeTests`, `PerchSort.Meta.EditModeTests`, and `PerchSort.PlayModeTests` per the shared contract.
3. Retire only tests/tools whose dependencies are the separately archived legacy Core/UI/View. Preserve current behavior/transaction/animation/save/layout assertions and migrate their references. Source paths/file names alone are not grounds to remove legitimate current tests. Old error/object text assertions may need migration review, but not changes to gameplay behavior. T50 transaction-versus-presentation cancellation coverage is particularly important.
4. Old runnable scenes, root `m_EditorClassIdentifier`, build scene selection, and editor capture/build entrypoints require C's shared mapping. Root font serialization and current scene GUID must remain intact at integration; T52A preserved the script GUID and field but did not edit/verify scene migration.
5. Coordinator owns primary-guide updates and replacement of obsolete legacy-only minimum counts with justified current-coverage safeguards; no CI test disabling is authorized by the superseded instruction.
6. Required coordinator acceptance after A/B/C integration: serialized Mac compilation, EditMode, PlayMode, then actual GUI acceptance of startup, map/level navigation, current resource rendering, T49/T50 transaction/queued-input/cancellation paths, and persisted progress. These are required future stage checks, **not worker-exercised results**.

## Mapping deviations

No type/namespace/resource contract changes proposed. Existing multi-type file placement is preserved to avoid unnecessary GUID creation. The obsolete enclosing V5 folder meta is archived byte-identically outside Unity; its former empty directory is removed.

## Coordinator delivery channel

The initial local `orca orchestration send` failed with `session_caller_unknown` and sent nothing. The user then supplied the verified saved environment selector. Sending static-readiness status with `orca orchestration send --environment root@192.168.1.251 --to orca_session_id:codex_0551b7e6_4ea7_4c29_936b_a50a25d9c1ec --type status --json` succeeded (message `msg_0cfcf52f1d7a`). Final SHA/push outcome will use that verified route and this worker terminal to the current Codex coordinator, never Claude. No Dispatch/Task IDs were invented; no `worker_done` is used.

## Exact changed-path manifest

The following mappings enumerate all affected script-tree paths. Each source/assembly row also moves its adjacent `.meta` using the same suffix; all those `.meta` bytes are unchanged. Rows with identical relative names still move from the old V5 prefix to the new unversioned prefix.

Current prefix: old `PerchSort/Assets/Scripts/V5/` → new `PerchSort/Assets/Scripts/`.

| Old relative source/assembly path | New relative source/assembly path |
| --- | --- |
| `Core/AssemblyInfo.cs` | `Core/AssemblyInfo.cs` |
| `Core/BoardState.cs` | `Core/BoardState.cs` |
| `Core/GameEvents.cs` | `Core/GameEvents.cs` |
| `Core/GameRules.cs` | `Core/GameRules.cs` |
| `Core/GameSession.cs` | `Core/GameSession.cs` |
| `Core/LevelDef.cs` | `Core/LevelDef.cs` |
| `Core/PerchSort.Core.asmdef` | `Core/PerchSort.Core.asmdef` |
| `Core/Solver.cs` | `Core/Solver.cs` |
| `Game/BirdView.cs` | `Game/BirdView.cs` |
| `Game/BirdhouseView.cs` | `Game/BirdhouseView.cs` |
| `Game/BoardView.cs` | `Game/BoardView.cs` |
| `Game/BranchView.cs` | `Game/BranchView.cs` |
| `Game/HudView.cs` | `Game/HudView.cs` |
| `Game/LevelLoader.cs` | `Game/LevelLoader.cs` |
| `Game/PerchSort.Game.asmdef` | `Game/PerchSort.Game.asmdef` |
| `Game/SettingsPanel.cs` | `Game/SettingsPanel.cs` |
| `Game/TutorialHand.cs` | `Game/TutorialHand.cs` |
| `Game/V5Art.cs` | `Game/GameArt.cs` |
| `Game/V5Button.cs` | `Game/GameButton.cs` |
| `Game/V5DevOptions.cs` | `Game/DevOptions.cs` |
| `Game/V5Fixtures.cs` | `Game/GameFixtures.cs` |
| `Game/GameRoot.cs` | `Game/GameRoot.cs` |
| `Game/V5Motion.cs` | `Game/GameMotion.cs` |
| `Game/V5Panels.cs` | `Game/PanelsView.cs` |
| `Game/V5Progress.cs` | `Game/ProgressStore.cs` |
| `Game/V5SceneView.cs` | `Game/SceneView.cs` |
| `Game/V5UI.cs` | `Game/GameUI.cs` |
| `Game/V5ViewModel.cs` | `Game/ViewModel.cs` |
| `Game/V6MapModel.cs` | `Game/MapModel.cs` |
| `Game/V6MapView.cs` | `Game/MapView.cs` |
| `Game/V7Ambient.cs` | `Game/Ambient.cs` |
| `Game/V7BirdIdleGroup.cs` | `Game/BirdIdleGroup.cs` |
| `Game/V7BirdOverlay.cs` | `Game/BirdOverlay.cs` |
| `Game/V7GateView.cs` | `Game/GateView.cs` |
| `Game/GameLayout.cs` | `Game/GameLayout.cs` |
| `Game/V7LeafTransition.cs` | `Game/LeafTransition.cs` |
| `Game/V7LevelEntrance.cs` | `Game/LevelEntrance.cs` |
| `Game/V7MapMotion.cs` | `Game/MapMotion.cs` |
| `Game/V7PopupFlow.cs` | `Game/PopupFlow.cs` |
| `Game/V7PopupMotion.cs` | `Game/PopupMotion.cs` |
| `Game/WinPanel.cs` | `Game/WinPanel.cs` |
| `Meta/Aviary.cs` | `Meta/Aviary.cs` |
| `Meta/Economy.cs` | `Meta/Economy.cs` |
| `Meta/PerchSort.Meta.asmdef` | `Meta/PerchSort.Meta.asmdef` |
| `Meta/PlayerProfile.cs` | `Meta/PlayerProfile.cs` |
| `Meta/ProfileText.cs` | `Meta/ProfileText.cs` |
| `Meta/Shop.cs` | `Meta/Shop.cs` |
| `Meta/Unlocks.cs` | `Meta/Unlocks.cs` |
| `Meta/WorldMap.cs` | `Meta/WorldMap.cs` |

Folder metadata mappings (exact paths):

- `PerchSort/Assets/Scripts/Core.meta` → `PerchSort/Assets/Scripts/Core.meta`
- `PerchSort/Assets/Scripts/Meta.meta` → `PerchSort/Assets/Scripts/Meta.meta`
- `PerchSort/Assets/Scripts/Game.meta` → `PerchSort/Assets/Scripts/Game.meta`
- `PerchSort/Assets/Scripts/V5.meta` → `archive/retired/PerchSort/Assets/Scripts/V5.meta`

Legacy archive prefix: old `PerchSort/Assets/Scripts/` → new `archive/retired/PerchSort/Assets/Scripts/`. Every exact relative file below moves byte-identically between those prefixes (including the explicitly listed metadata):

```text
Core.meta
Core/BoardState.cs
Core/BoardState.cs.meta
Core/GameSession.cs
Core/GameSession.cs.meta
Core/Generator.meta
Core/Generator/LevelGenerator.cs
Core/Generator/LevelGenerator.cs.meta
Core/LevelData.cs
Core/LevelData.cs.meta
Core/Move.cs
Core/Move.cs.meta
Core/PerchSort.Core.asmdef
Core/PerchSort.Core.asmdef.meta
Core/Rules.cs
Core/Rules.cs.meta
Core/Solver.meta
Core/Solver/Solver.cs
Core/Solver/Solver.cs.meta
UI.meta
UI/ButtonFeedback.cs
UI/ButtonFeedback.cs.meta
UI/CaptureRoutine.cs
UI/CaptureRoutine.cs.meta
UI/CrateLabels.cs
UI/CrateLabels.cs.meta
UI/GameController.cs
UI/GameController.cs.meta
UI/GameHud.cs
UI/GameHud.cs.meta
UI/GameHudPages.cs
UI/GameHudPages.cs.meta
UI/HintService.cs
UI/HintService.cs.meta
UI/LevelGrid.cs
UI/LevelGrid.cs.meta
UI/LocalizedText.cs
UI/LocalizedText.cs.meta
UI/MenuAnimation.cs
UI/MenuAnimation.cs.meta
UI/PanelMotion.cs
UI/PanelMotion.cs.meta
UI/PerchSort.UI.asmdef
UI/PerchSort.UI.asmdef.meta
UI/ProgressStore.cs
UI/ProgressStore.cs.meta
UI/ResultAnimation.cs
UI/ResultAnimation.cs.meta
UI/SafeAreaLayout.cs
UI/SafeAreaLayout.cs.meta
UI/ScreenTransition.cs
UI/ScreenTransition.cs.meta
UI/SettingsPanel.cs
UI/SettingsPanel.cs.meta
UI/SettingsStore.cs
UI/SettingsStore.cs.meta
UI/SoftFocusMask.cs
UI/SoftFocusMask.cs.meta
UI/Strings.cs
UI/Strings.cs.meta
UI/TutorialOverlay.cs
UI/TutorialOverlay.cs.meta
UI/TutorialState.cs
UI/TutorialState.cs.meta
UI/UiTheme.cs
UI/UiTheme.cs.meta
View.meta
View/BirdView.cs
View/BirdView.cs.meta
View/BlankBoardInput.cs
View/BlankBoardInput.cs.meta
View/BoardView.cs
View/BoardView.cs.meta
View/CelebrationBurst.cs
View/CelebrationBurst.cs.meta
View/FallingFeathers.cs
View/FallingFeathers.cs.meta
View/PerchSort.View.asmdef
View/PerchSort.View.asmdef.meta
View/PerchView.cs
View/PerchView.cs.meta
View/TargetQueueView.cs
View/TargetQueueView.cs.meta
View/ViewArt.cs
View/ViewArt.cs.meta
```

Additional owned documentation paths:

- `docs/work-log/current-runtime/spec.md` — new task specification and updated validation ownership.
- `docs/work-log/current-runtime/report.md` — this delivery report and complete path manifest.

No other owned/non-owned files were edited; Audio paths are intentionally unchanged.
