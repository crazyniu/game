# Current shared team memory

## Entry and coordination

- One current game: `PerchSort/Assets/Scenes/Game.unity`, Unity 6000.6.7f1, Mac development, current Codex coordinator. Read [CURRENT_GAME](../CURRENT_GAME.md), [AGENTS](../../AGENTS.md), [TEAM](TEAM.md) and [handoff](HANDOFF_2026-11-04_CODEX.md).
- T52 preserves accepted T49/T50 behavior/art, 40 authored levels and user data while removing active numbered naming. Archive is not another runnable project.
- Current code: Scripts/Core, Meta, Game; shared Audio unchanged. Resources: Art, Birds, World, Levels/levels and dev_options. Historical design/report/art filenames remain provenance.
- World map is home; four sequential forests. Current dev flags remain ON (`unlockAllLevels`, `grantTestStock`) until a separately authorized release change. Existing Shop/Aviary entries are authorized placeholders, not complete policy screens.

## Validation and preservation

- The latest user directive requires coordinator-owned Mac compile, EditMode, PlayMode and GUI acceptance after integration. It supersedes earlier no-tests language. Workers do not launch Unity independently. Historical green counts and static assets never establish a new pass.
- Retained source expectation: 304 EditMode (237 game, 53 Meta, 14 Audio), 17 PlayMode, no Ignore/Explicit. LevelsData expands to 81 including 40 solver/replays; Layout to 41 including 24 combinatorial. Investigate actual discovery differences. Never lower safeguards to hide failures.
- `run_tests.sh` builds only Game.unity through `PerchSort.Editor.GameSceneBuilder.Build`; capture tool is `PerchSort.Editor.GameCapture.Run`. Capture output `build/screens/game/` is stable-state/instant, not animation proof.
- Record and preserve scene/meta/settings/package changes before Unity. Do not blindly discard generated files. The coordinator reported user-primary-checkout changes including 108 generated bird metas, seven overlay-meta changes and scene InputSystem ID rewrites; only coordinator handles that preservation/update.
- Seven egg/family imports need explicit policy: egg pivots `(0.5,0.0625)`, chick/mama `(0.5,0.5)`, FullRect mesh (`spriteMeshType: 0`), max1024, uncompressed quality100, no fallback physics generation. Unity's enum is FullRect=0, Tight=1; do not change the baseline mesh or allow generic art import to recenter eggs. Six retained compressed fallback sprites must not inherit full-art policy.
- Scene GUID `55064aed0051b40279c77a0b9a758533`, root script GUID `07203ec62b1604449b634c729c1303a3`, serialized font stay intact. Static GUID retention is not runtime startup proof.
- Existing `v5.*`, `v6.unlockSeen.*`, `v7.notifications` keys, ProfileMigrator reads, DataMember/SaveData fields, enum order and schema versions are storage compatibility only; never rename/reset them.

## Practical constraints

- Chinese chat with the user, English agent artifacts/comments/commits. OMP preferred, Codex fallback; no extra sub-workers from bounded workers.
- Core/Meta remain pure C#. NUnit `Range` can conflict with UnityEngine; use `NUnit.Framework.Range` where needed.
- Capture needs a graphics device; `-nographics` cannot validate visuals. Only one coordinator-serialized Unity process. Never kill the user's Editor.
- Preserve originals under `art/` and historical reports/research. Generated `build/` is ignored; no unauthorized network services, credentials or paid fallbacks.
