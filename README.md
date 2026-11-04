# Perch Sort · 小鸟归巢

One current Unity **6000.6.7f1** portrait bird-sorting game: **40 fixed authored levels**, four sequential forest worlds. Current gameplay/art/animation is the accepted **T49/T50 implementation**, with T52 consolidating names and entry points without redesigning it.

## Open the game

Open the `PerchSort` Unity project and **`Assets/Scenes/Game.unity`**, the sole enabled build scene. `PerchSort.Game.GameRoot` creates the current map and gameplay UI. Develop on the Mac in isolated task workspaces; do not edit the user's primary checkout from a worker.

| Need | Entry |
| --- | --- |
| AI instructions and coordination | [AGENTS](AGENTS.md) · [team charter](docs/team/TEAM.md) |
| Current behavior, source and evidence | [CURRENT_GAME](docs/CURRENT_GAME.md) · [CODE_MAP](docs/spec/CODE_MAP.md) |
| Documentation status / historical research | [docs router](docs/README.md) |
| Current integration handoff | [Codex handoff](docs/team/HANDOFF_2026-11-04_CODEX.md) |

Current code lives in `PerchSort/Assets/Scripts/{Core,Meta,Game}`, shared audio in `Scripts/Audio`. Resources load from `Art/`, `Birds/`, `World/`, `Levels/levels` and `dev_options`. Old scenes, builders and old-contract tests are preserved outside Unity in `archive/retired/`; they are not alternative game entries. Approved originals under `art/` remain intact.

## Validation and tooling

The **current Codex coordinator owns required Mac compilation, EditMode, PlayMode and GUI acceptance after integration**. Workers must not launch competing Unity runs. Earlier no-tests instructions are superseded. T52C itself has static-only evidence, not new runtime acceptance.

For the coordinator's serialized Mac run:

```bash
bash scripts/run_tests.sh
bash scripts/screenshots.sh
```

The first command uses only `PerchSort.Editor.GameSceneBuilder.Build` and the retained suites; source-derived coverage safeguards are 304 EditMode (237 game + 53 Meta + 14 shared Audio) and 17 PlayMode, without skips. The second uses `PerchSort.Editor.GameCapture.Run` and writes stable 1080×1920 captures to `build/screens/game/`. Captures use instant motion, not timed-animation proof. `PhoneGameView`, `ReleaseSettings` and the shared Audio importer remain available. `GameArtImporter` separately handles full-resolution current art, seven authored overlays, and six compressed fallback sprites.

Automatic PR CI still runs current configuration/script/workflow checks and retained pure-Core fixtures through the existing `ci/CoreTests.csproj`. It never targets retired archives. Full Unity and GUI acceptance remain Mac-only and coordinator-owned.

## Compatibility and history

Existing `v5.*`, `v6.unlockSeen.*` and `v7.notifications` persistence strings remain unchanged to preserve progress; these are compatibility IDs, not runnable versions. Current development flags remain enabled in `Assets/Resources/dev_options.json`; release hardening is a separate authorized change.

Old v2 implementation records and the v4 rules/schema/save proposals are **historical**, not the current runtime. The current design evidence retains historical filenames, routed through [CURRENT_GAME](docs/CURRENT_GAME.md). Research/replay/task reports retain their original evidence and limitations; none establishes new Unity or visual acceptance.
