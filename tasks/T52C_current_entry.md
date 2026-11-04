# T52C — sole current scene/editor/tool/documentation entry

## Task and authority

User-authorized T52 identity cleanup, base `4b42159891619db23811cbef2b0de84ab742e43e`, branch `mac-current-entry/t52`. Implement only C ownership in the isolated Mac workspace. Do not merge A/B, start workers, modify Scripts/Resources/art originals or touch the primary checkout. Current Codex coordinator integrates all deliveries and reviews/tests on the Mac.

The latest user instruction supersedes the initial no-tests/CI-disable request: **stage compile, EditMode, PlayMode and GUI acceptance are REQUIRED and coordinator-owned after integration**. C must not independently run Unity, compiler, code tests, captures or playthroughs. Static source/hash/GUID/JSON/XML/shell review is permitted. Preserve legitimate assertions; do not disable checks to hide failures.

## Shared contract

- A migrates current Scripts into Core/Meta/Game, unversioned namespaces/types, preserving metadata and storage compatibility. B migrates current Resources to Art/Birds/World/Levels and dev_options, preserves payload/meta bytes, archives obsolete resources/original-derived legacy Assets/Art. C consumes those mappings without cross-editing or merging peer branches.
- Scene `V5.unity` becomes the sole `Game.unity` after archiving old Game/Sample scenes. Retain scene GUID `55064aed0051b40279c77a0b9a758533`, root-script GUID `07203ec62b1604449b634c729c1303a3`, serialized font, all unrelated serialized fields. Root identifier becomes `PerchSort.Game::PerchSort.Game.GameRoot`; exactly one enabled build scene.
- Current resources load as Art/key, Birds/key, World/key, Levels/levels, dev_options. Six carried PNGs retain compressed import policy separately. Seven authored egg/chick/mama metas require explicit policy preserving custom pivots/mesh/texture settings; eggs must not be recentered by a broad importer.
- Existing PlayerPrefs keys, migrator reads, DataMember/SaveData fields, enum ordinals and schema versions remain compatibility IDs, never renamed/reset. Retain 40 levels, T49/T50 behavior, forest visuals/animation and approved originals.

## Owned scope

`PerchSort/Assets/{Editor,Tests,Scenes}/**` and their folder metas; `PerchSort/ProjectSettings/EditorBuildSettings.asset`; `scripts/**` except shared Audio/audio generation; `ci/**`; `.github/workflows/**`; README.md, AGENTS.md, CLAUDE.md, docs/README.md, docs/spec/CODE_MAP.md, docs/team/{MEMORY.md,TEAM.md,HANDOFF_2026-11-04_CODEX.md}, docs/CURRENT_GAME.md; this spec/report. Retired counterparts only under `archive/retired/PerchSort/Assets/{Editor,Tests,Scenes}/**` and `archive/retired/{scripts,ci}/**`. Shared Audio code/importer/tests remain unchanged.

## Required changes

1. Archive obsolete scene/editor/test/tool sources with metadata outside Unity; do not rewrite old-contract tests into current tests. Preserve archival bytes and historical reports/research.
2. One current `GameSceneBuilder`, `GameArtImporter`, `GameCapture` under Editor/Game; document namespace PerchSort.Editor. Builder invokes no legacy importer or scene. Retain shared PhoneGameView, ReleaseSettings, Audio.
3. Move current tests/asmdefs to Tests/{EditMode,Meta,PlayMode}; preserve GUIDs and assertions; remove test/fixture prefixes including V53/V54. Update namespaces, APIs, resource/scene/fixture strings; icon_undo replaces obsolete ico_undo. Preserve nested Audio asmdef without path/assembly conflicts.
4. Retarget tests/captures to the sole builder/scene/capture. Retire old capture assertions without a current equivalent. Replace weak legacy minimums with source-justified retained-suite guards: 304 EditMode (237 current game, 53 Meta, 14 Audio), 17 PlayMode, no Ignore/Explicit. Preserve LevelsData 81 expanded cases (40 optimal solver/replays) and Layout 41 (24 combinatorial).
5. Inspect the existing ci/CoreTests.csproj and retarget current pure-Core fixtures; no archived test/source compile inputs. Keep automatic CI tests enabled under updated user direction. Ensure sparse checkout includes the project and current source/JSON/reference/script filters exclude archives. No invented replacement tests or fake runtime API shims.
6. Replace stale primary entry docs with one current game, unversioned paths, Mac development, current Codex coordination, current T49/T50 implementation facts, storage compatibility notes, and required coordinator verification. Older v2/v4 documents remain clearly historical; later design documents may retain historical filenames as evidence. Do not claim new rule or runtime acceptance.

## Acceptance and delivery

Static evidence: scene exact expected three identity-line changes only; scene/root/font retention; one build scene; moved current metadata and fixture JSON hashes; byte-identical archives/shared tools/Audio; no active old namespace/resource/tool references; retained test assertion/attribute inventory; valid current JSON/asmdef/csproj, shell syntax and Python source syntax; current CI inputs exclude archives. Report test migration/Unity import risks and integration-only references because A/B are not merged locally.

Commit and push only the task branch, then report DONE/BLOCKED, SHA, exact changed paths, evidence, mapping changes and untested limitations to current Codex via supplied Orca route/terminal. No Dispatch IDs exist. Coordinator performs integrated Mac testing/GUI acceptance, then reports actual results; C must not call static checks runtime proof.
