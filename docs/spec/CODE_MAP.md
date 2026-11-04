# Current implementation map

T52 identity cutover of the accepted T49/T50 game. This index describes source organization and known evidence boundaries; it does not redefine rules or claim new runtime acceptance. Start at [CURRENT_GAME](../CURRENT_GAME.md).

## Current modules

| Path under `PerchSort/Assets` | Assembly / principal symbols | Responsibility |
| --- | --- | --- |
| `Scripts/Core` | `PerchSort.Core`: BoardState, LevelDef, GameRules, GameSession, Solver | Pure C# board state, resolution/events, inventory/undo, search |
| `Scripts/Meta` | `PerchSort.Meta`: PlayerProfile, Economy, Unlocks, Shop, Aviary, WorldMap | Pure profile and progression policies; not proof that all policy screens are implemented |
| `Scripts/Game` | `PerchSort.Game`: GameRoot, GameProfile, ProgressStore, LevelLoader, ViewModel | Unity orchestration, current catalog, progress, hints, notification transactions |
| `Scripts/Game` | GameArt, GameUI, GameButton, GameLayout, SceneView, MapModel/MapView | Resource routing, layout, map and screen UI |
| `Scripts/Game` | BirdView, BoardView, BranchView, BirdhouseView, PanelsView, PopupFlow/PopupMotion, MapMotion, LeafTransition | Accepted T49/T50 presentation and cancellation lifecycle |
| `Scripts/Audio` | Existing shared Audio contract | Shared audio unchanged by T52C |
| `Scenes/Game.unity` | GameRoot, serialized font | Sole current scene; GUID `55064aed0051b40279c77a0b9a758533`, root-script GUID `07203ec62b1604449b634c729c1303a3` |
| `Editor/Game` | Namespace `PerchSort.Editor`: GameSceneBuilder, GameArtImporter, GameCapture | Sole scene builder, current art policy, stable-state capture |
| `Editor/PhoneGameView.cs`, `Editor/ReleaseSettings.cs` | Existing global classes | Shared phone view and release setup remain available |
| `Tests/EditMode`, `Tests/Meta`, `Tests/PlayMode` | PerchSort.EditModeTests / PerchSort.Meta.EditModeTests / PerchSort.PlayModeTests | Retained current contracts; nested Audio asmdef stays independent |

Full code and scene links: [Core](../../PerchSort/Assets/Scripts/Core), [Meta](../../PerchSort/Assets/Scripts/Meta), [Game](../../PerchSort/Assets/Scripts/Game), [Game scene](../../PerchSort/Assets/Scenes/Game.unity), [Editor](../../PerchSort/Assets/Editor/Game).

## Resources and compatibility

| Load key | Asset path under `Assets/Resources` |
| --- | --- |
| `Levels/levels` | `Levels/levels.json`: existing 40 authored levels, payload retained |
| `dev_options` | `dev_options.json`: existing flags retained |
| `Art/<key>` | `Art`: current UI/art plus six separately imported compressed fallback sprites |
| `Birds/<key>` | `Birds`: species frames/portraits and seven authored egg/family overlays |
| `World/<key>` | `World`: current forest pieces |

No old numbered resource fallback remains in the shared cutover contract. GameArtImporter retains full-art borders/door pivots and tall-background resolution; its separate fallback policy avoids changing compression/mesh/pivots on six carried sprites. The seven authored overlays explicitly retain their custom pivot/mesh/1024/no-physics import settings; the egg foot pivot is not the centered default.

Version-looking PlayerPrefs keys and serialized DataMember/SaveData fields are compatibility identifiers only. Preserve all the exact persistence literals listed in CURRENT_GAME, migrator inputs, enum ordinals and schema versions. Namespace cleanup must not reset progress or migrate persisted data by blind text replacement.

## Retired paths

Retired runtime/editor/test trees and superseded art have been removed. Git preserves their history; do not recreate them in the working tree. Shared Audio, PhoneGameView, ReleaseSettings and the retained current art remain canonical.

Historical implementation/research claims remain in reports and the [old design record](../archive/DESIGN_ORIGINAL.md). The older [RULES](RULES.md)/[DATA_SAVE](DATA_SAVE.md) proposal is not today's runtime contract. The canonical [design record](../GAME_DESIGN.md), later accepted contracts and actual source resolve older presentation/ownership prose.

## Tools and evidence

- `flock /tmp/unity.lock bash scripts/run_tests.sh`: builds only Game.unity, then runs full EditMode/PlayMode suites. T54 baseline: 304 EditMode = 237 game + 53 Meta + 14 Audio; 21 PlayMode; no skips. LevelsData 81 includes 40 solver/replays; Layout 41 includes 24 combinatorial. Reconcile actual discovery; never lower safeguards.
- `scripts/screenshots.sh`: only `PerchSort.Editor.GameCapture.Run`; `build/screens/game/`, stable states at 1080×1920. Archived old animation/resume/aspect assertions do not become current proof.
- `ci/CoreTests.csproj`: real current Core plus 13 current pure-Core fixture classes and TestBoards; copied current fixture JSON. `DOTNET_CI` changes fixture transport only, not production rules/assertions. Unity-dependent suites remain covered by the required Mac run.
- `.github/workflows/ci.yml` and `ci/check_changes.py`: current source/resource/test/project/script inputs only; automatic tests retained; archives excluded. Workflow dependencies' own version numbers are unrelated to game identity.

Workers run applicable serialized Unity compilation/tests/screenshots; integration acceptance remains required. Scene InputSystem action-ID reserialization, sprite imports and actual startup need observed checks. Historical counts, replay certificates and static images are not new runtime or timed-animation evidence.
