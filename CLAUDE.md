# Perch Sort — agent collaboration guide

Follow [CONVENTIONS](docs/CONVENTIONS.md).

## Current execution contract

Perch Sort has one game: Unity **6000.6.7f1**, portrait, 40 fixed authored levels across four sequential forest worlds. Open `PerchSort/Assets/Scenes/Game.unity`. Start with [README](README.md), [current game](docs/CURRENT_GAME.md), [documentation router](docs/README.md), and [code map](docs/spec/CODE_MAP.md).

- The current stage assigns music, entrance timing, continuous bird motion, button sizing and repository cleanup to separate owners under [the shared contract](docs/work-log/integration-contract/spec.md). Follow the assigned task; cleanup is not authority to redesign rules, art, economy or saves.
- Return questions and reports to the coordinator who assigned the task. Agent-entry filenames do not confer coordination authority. AGENTS.md and CLAUDE.md must remain identical.
- Develop on the Mac in isolated Orca workspaces. Workers must not mutate the user's primary checkout `/Users/apple/Documents/projects/game`.
- Decompose features into parallel tasks with non-overlapping file ownership and one shared contract; the coordinator integrates and reviews. OMP preferred, Codex fallback. Bounded workers must not spawn workers.
- Commit and push only the assigned task branch. Never merge peer branches, touch or push main, force-push, deploy or release as a worker. Coordinator integration authority is not release authority.
- Workers run their own Mac compilation, full EditMode/PlayMode suites and applicable screenshots under `flock /tmp/unity.lock`. Integrated acceptance remains required after coordination; historical counts and static reports do not establish new acceptance.

## Hard constraints (user, 2026-11-04)

- There is exactly one version of this game. Never name files, folders, classes, namespaces, assets, scenes, docs or branches after a version. No version-number prefixes or suffixes, numbered UI identities, or numbered "final" copies. History lives in Git, not in the tree. Never create archive folders of old versions.
- Approved reference images live in `art/approved_ui`; they are the visual acceptance target. A UI task is accepted only when actual runtime screenshots visibly match those images.
- Animation and audio acceptance requires timed evidence: frame sequences and logs, never one static frame. Level entrance must take **3.5–4.5 seconds**. Birds must never look static: continuous breathing plus frequent idle actions. Background music must always be present on the applicable page and follow the existing mute control. All same-role buttons must have identical sizes in every state; disabled means dimmed and non-interactive, not smaller.
- Never fake animation by mirroring or flipping a still image, or by showing a fixed picture. Birds in popups, houses and on branches must move continuously through breathing, hops and pose-frame changes with easing. Turning must never be a sprite mirror.
- Primary action buttons must be **at most 1.25 times** the size of sibling buttons.
- Workers may and must run applicable serialized Unity tests and screenshots. There is no coordinator-only Unity monopoly, no no-tests continuation rule, and no documentation-only exemption for an implementation task.

## Permanent naming policy and review acceptance

- Edit canonical files in place. No parallel implementation, alternate scene, replacement resource root or runnable historical copy under any name.
- Preserve every existing storage key, migrator read, ProfileText DataMember, popup SaveData field, enum numeric order and schema value. Existing persistence compatibility is not permission to reset user data.
- Review canonical paths, one enabled Game scene, canonical resource roots and each retained numbered identifier. Run `python3 ci/check_current_game_names.py --self-test` and `python3 ci/check_current_game_names.py` before Unity. The guard checks tracked paths and authored identifiers; it complements review, not compilation or runtime acceptance.
- Removed implementation and superseded art live only in Git history. Retained research documents describe historical decisions, not another runnable game. Do not recreate removed trees from historical references.

## Current source and data ownership

| Surface | Current location |
| --- | --- |
| Pure rules and solver | `PerchSort/Assets/Scripts/Core` / `PerchSort.Core` |
| Pure profile/economy/collection policies | `PerchSort/Assets/Scripts/Meta` / `PerchSort.Meta` |
| Unity runtime and presentation | `PerchSort/Assets/Scripts/Game` / `PerchSort.Game` |
| Shared audio | `PerchSort/Assets/Scripts/Audio` |
| Sole scene and root | `Assets/Scenes/Game.unity` / `PerchSort.Game.GameRoot` |
| Current resources | `Assets/Resources/{Art,Birds,World}`, `Assets/Resources/Levels/levels.json`, `Assets/Resources/dev_options.json` |
| Editor entry | `Assets/Editor/Game`, namespace `PerchSort.Editor`: `GameSceneBuilder`, `GameArtImporter`, `GameCapture` |
| Tests | `Assets/Tests/{EditMode,Meta,PlayMode}`; shared Audio under `Tests/EditMode/Audio` |
| Level authoring | `tools/levels/LevelTool/LevelTool.csproj`, compiling canonical Core directly |
| Visual acceptance references | `art/approved_ui` |

## Specification and language

- Chinese with the user unless the assigned task requires English-only communication. Agent instructions, new task specs/reports, comments and commits are English. No emoji in task deliveries.
- [CURRENT_GAME](docs/CURRENT_GAME.md) routes current source/design evidence. [GAME_DESIGN](docs/GAME_DESIGN.md) preserves design decisions; later approved visual/animation contracts and actual source resolve superseded layout prose. Historical ownership and paths are not execution instructions.
- Old runtime descriptions and rules/schema/save proposals remain historical research. They must not silently replace the accepted game. Keep Core/Meta independent of Unity presentation; inspect actual code and obtain separate authority before changing rules or levels.
- Preserve 40 fixed levels, forest-only sequential worlds, approved forest visuals, icon-only controls where clear, regular button shapes and minimal mute-only sound settings. New assets must be derived, never overwrite retained originals. Do not generate or edit artwork unless the task authorizes it.
- Each feature gets `docs/work-log/<topic>/spec.md` and `report.md` named by subject, no version or task numbers. Stay inside assigned paths, preserve unrelated user changes and report requested contract changes explicitly.

## Serialized Mac verification workflow

1. Record scene/meta/settings/package state before Unity. Preserve user changes separately; never blindly restore generated files or update the primary checkout from a worker.
2. Inspect process and lock state. Use one Unity process at a time and `flock /tmp/unity.lock` for every batch run, test and screenshot command. If the user has the Editor open, ask them to close it; never kill their Editor.
3. Run `flock /tmp/unity.lock bash scripts/run_tests.sh`: it invokes only `PerchSort.Editor.GameSceneBuilder.Build`, then full EditMode and PlayMode suites. T54 baseline: **304 EditMode and 21 PlayMode**, all passing; these are expectations, not results. Preserve the retained LevelsData 81 expanded cases and Layout 41 cases. Never lower discovery safeguards or weaken/skip tests to hide a failure.
4. Run `flock /tmp/unity.lock bash scripts/screenshots.sh` for stable-state captures in `build/screens/game/`, then perform applicable real GUI/timed acceptance. GameCapture uses instant motion and synthetic nonpersistent profiles; stills do not prove animation, persistence or device acceptance. Animation/audio claims require the shared contract's timed frame/log evidence.
5. CI runs the naming guard before current JSON/assembly/project/script checks, workflow lint and `dotnet test ci/CoreTests.csproj`. CI is not a substitute for Unity or GUI acceptance. Sparse-checkout naming inspection uses indexed Git blobs for absent files.
6. Report exact commands, counts, skips, failures, evidence paths and remaining boundaries. Keep generated output out of commits (`build/` is ignored); commit only the task's required evidence. Historical passing counts never validate a new change.

Do not expose network services or credentials. Use authorized Orca/artifact channels. Production store identifiers and release account/package placeholders still require user input; `com.todo.perchsort` is not a production package name.
