# Perch Sort — agent collaboration guide

## Current execution contract

Perch Sort has **one current game**: Unity **6000.6.7f1**, portrait, 40 fixed authored levels in four forest worlds. Open `PerchSort/Assets/Scenes/Game.unity`. Start with [README](README.md), [current game](docs/CURRENT_GAME.md), [documentation router](docs/README.md), and [code map](docs/spec/CODE_MAP.md).

- T52 is an authorized identity/organization cleanup of the accepted T49/T50 implementation, not a gameplay, art, economy or save redesign. Change the current game directly; do not create numbered implementation forks.
- Current coordinator is Codex, `orca_session_id:codex_0551b7e6_4ea7_4c29_936b_a50a25d9c1ec`. Reports/questions return there, never to the retired Claude session. This file and CLAUDE.md carry the same guide; the filename does not assign coordination authority.
- Development runs on the **Mac**, in isolated Orca workspaces. Do not mutate the user's primary checkout `/Users/apple/Documents/projects/game` from a worker. Linux 251 is no longer the development host.
- Decompose feature efforts into parallel tasks with non-overlapping file ownership and one shared contract; the coordinator integrates and reviews. OMP preferred, Codex fallback; coordinator chooses model/effort by complexity. A bounded worker must not spawn more workers.
- Workers may commit and push only their assigned task branch. Do not merge peer branches, push main, force-push, deploy, or release. The user authorized coordinator review and integration of completed work into main for testing; that is not release authority.
- **Updated testing directive:** after integration, the current Codex coordinator must perform Mac compilation, EditMode, PlayMode and GUI acceptance. This supersedes the earlier no-tests continuation instruction. Workers do not independently launch Unity; coordinator serializes it. Worker reports may state NOT_RUN locally, never that stage testing is optional.

## Permanent single-current-game naming policy

- Maintain **ONE current game**. Edit canonical files in place; never create V-numbered parallel folders, files, classes, namespaces, scenes or resource roots, or alternate current implementation copies under any other name.
- Normal task branches and PRs are temporary collaboration vehicles reviewed and merged into `main`, not permanent runnable forks or alternative current implementations.
- The only narrow exceptions are: **exact existing storage identifiers**, solely to preserve save compatibility; **preserved historical archives and original-art provenance**, never active implementation copies; and **legitimate dependency, schema or release metadata**. Existing storage compatibility is not permission to introduce new versioned keys. No new arbitrary exceptions or catch-all allowlists.
- **Review acceptance:** verify changes update canonical files, introduce no prohibited parallel identity/copy, keep one enabled Game scene and the canonical resource roots, and fit any retained version-looking identifier into exactly one exception above. Require `python3 ci/check_current_game_names.py` to pass before tests; its static guard complements review and does not authorize exceptions or replace required Mac acceptance.

## Current source and data ownership

| Surface | Current location |
| --- | --- |
| Pure game rules and solver | `PerchSort/Assets/Scripts/Core` / `PerchSort.Core` |
| Pure profile/economy/collection policies | `PerchSort/Assets/Scripts/Meta` / `PerchSort.Meta` |
| Unity runtime and presentation | `PerchSort/Assets/Scripts/Game` / `PerchSort.Game` |
| Shared audio | `PerchSort/Assets/Scripts/Audio` (retain existing contract) |
| Sole scene and root | `Assets/Scenes/Game.unity` / `PerchSort.Game.GameRoot` |
| Current art and catalog | `Assets/Resources/{Art,Birds,World}`, `Assets/Resources/Levels/levels.json`, `Assets/Resources/dev_options.json` |
| Editor entry | `Assets/Editor/Game`, namespace `PerchSort.Editor`: `GameSceneBuilder`, `GameArtImporter`, `GameCapture` |
| Retained tests | `Assets/Tests/{EditMode,Meta,PlayMode}`; shared Audio stays under `Tests/EditMode/Audio` |

`archive/retired/` is outside Unity Assets and compiled/test CI inputs. It preserves retired code/data/scenes/tests, not a second runnable project. Never restore it because a historical document or asset names it. Originals under `art/` are preserved, including historical versioned filenames.

Exact existing storage identifiers listed in [CURRENT_GAME](docs/CURRENT_GAME.md) are compatibility identifiers, **not implementation versions**. Preserve every existing key, ProfileMigrator read, ProfileText DataMember, popup SaveData field, enum numeric order and schema version. Do not reset user data or rename persisted fields during cleanup.

## Specification and language

- Talk to the user in Chinese. Agent instructions, new task specs/reports, comments and commits are English. Preserve historical research/task evidence rather than rewriting its claims.
- [CURRENT_GAME](docs/CURRENT_GAME.md) routes current source/design evidence. [GAME_DESIGN_V5](docs/GAME_DESIGN.md) retains its historical filename and design decisions; later approved T49/T50 visual/animation contracts and actual current source resolve superseded layout prose. Do not interpret stale Claude ownership or numbered paths there as current execution instructions.
- The old v2 runtime description and v4 rules/schema/save proposal are historical reference, not today's runtime. `docs/spec/RULES.md`, PRD/evidence/replay documents and older UI specifications must not silently replace the accepted game. T52 does not change any rule.
- Keep logic independent of presentation. Core/Meta have no UnityEngine references. Inspect current code before changing rules/levels and obtain separate feature authority; do not infer implementation scope from a historical roadmap.
- Preserve 40 fixed levels, forest-only sequential worlds, approved forest visuals, icon-only controls where clear, regular button shapes and minimal mute-only sound settings. Do not replace animation with a static-image claim. New assets must be derived, never overwrite originals.
- Every task has `tasks/T<n>_<name>.md` and `tasks/T<n>_report.md`. Stay within assigned paths and preserve unrelated user changes.

## Coordinator verification workflow

1. Integrate reviewed task branches in an isolated Mac integration workspace. Record scene/meta/settings/package state before Unity. Preserve the user-checkout changes separately before any update; never blindly restore generated files.
2. Use one Unity process at a time. If the user has the project open, ask them to close it; never kill their Editor. Inspect existing process/lock state before starting a serialized run.
3. Run `bash scripts/run_tests.sh`: it invokes only `PerchSort.Editor.GameSceneBuilder.Build`, then the retained EditMode and PlayMode suites. Source-derived safeguards are **304 EditMode** (237 game + 53 Meta + 14 Audio) and **17 PlayMode**, no Ignore/Explicit. LevelsData retains 81 expanded cases (40 solver/replay), Layout 41 (24 combinatorial). These are expectations, not passing results; investigate actual discovery differences rather than lower counts or skip failures.
4. Run `bash scripts/screenshots.sh` for stable-state captures in `build/screens/game/`, then perform actual GUI/timed acceptance. Static GameCapture uses instant motion and synthetic nonpersistent profiles; it does not prove animated playback, persistence or device acceptance.
5. Automatic PR CI runs `python3 ci/check_current_game_names.py` unconditionally before tests, then retains JSON/assembly/project/script checks, workflow lint and the current pure-Core fixture subset via `dotnet test ci/CoreTests.csproj`. The naming guard uses Git-tracked paths and `git show` for sparse files; it validates naming policy, never compiles or runs archived tests. CI is not a substitute for Unity or GUI acceptance.
6. Report exact commands, results, skips, failures and boundaries. All T52 worker runtime validation remains NOT_RUN until the coordinator executes it. Historical passing counts do not validate new changes. Keep generated output out of commits (`build/` is ignored).

Do not expose network services or credentials. Existing authorized Orca/artifact channels suffice for reports and captures. Store identifiers and release account/package placeholders still require user input; `com.todo.perchsort` is not a production package name.
