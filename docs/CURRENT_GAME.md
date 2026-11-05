# Current game — source and evidence entry

## Identity and authority

There is one Perch Sort game: Unity 6000.6.7f1, portrait 1080×1920 design canvas, 40 fixed authored levels across four sequential forest worlds. T54 changes music, entrance/idle motion, button consistency and repository naming under separate ownership; it does not redesign rules, economy or saves.

Open `PerchSort/Assets/Scenes/Game.unity`. Its retained scene GUID is `55064aed0051b40279c77a0b9a758533`; GameRoot script GUID is `07203ec62b1604449b634c729c1303a3`. The root retains its serialized Fredoka font reference. There is exactly one enabled build scene. Static preservation does not prove Unity import: InputSystem scene action IDs and actual root initialization need coordinator verification.

## Permanent canonical-edit policy and review acceptance

- Maintain **ONE current game**, editing canonical files in place. Never create V-numbered parallel folders, files, classes, namespaces, scenes or resource roots, or alternate current implementation copies under any other name.
- Task branches and PRs are temporary collaboration reviewed and merged into `main`; they are not permanent runnable forks or alternative current implementations.
- Exceptions are limited to `.git`, third-party package files, lockfiles, dated handoff/report evidence under `tasks/` and `docs/team/`, and exact storage-key literals. No historical implementation archive or numbered original-art filename is exempt. History lives in Git.
- Review acceptance must check canonical in-place edits, absence of parallel implementation identities/copies, one enabled Game scene, canonical resource roots, and an exact allowed reason for each retained version-looking identifier. `python3 ci/check_current_game_names.py` must pass before tests; the guard complements human review and required Mac acceptance.

[`art/approved_ui`](../art/approved_ui) contains the approved visual acceptance targets. Its [layout metadata](../art/approved_ui/layout.json) is offline measurement/provenance, not a second runtime. Actual UI screenshots must visibly match the reference images; canonical GameLayout and current resources implement the game.

## Read current behavior at its owner

| Concern | Source / evidence |
| --- | --- |
| Board model, move resolution, eggs, sleep/family/crow, swivel/growth/day-night, Chorus, undo, solver | [Core](../PerchSort/Assets/Scripts/Core) |
| Profile, unlocks, world/collection/economy policies | [Meta](../PerchSort/Assets/Scripts/Meta) |
| Map, HUD, board, notifications, intro, panels and motion | [Game](../PerchSort/Assets/Scripts/Game) |
| Accepted mechanics/design decisions | [design record](GAME_DESIGN.md) (later contracts supersede its old layouts/roadmap) |
| Latest accepted bird/overlay and presentation integration | [T49](work-log/forest-assets/report.md), [egg/overlay report](work-log/forest-assets/report-eggs.md), [T50](work-log/interface-runtime/report.md) and linked A/B/C/D/E owner reports |
| Current organization / verification boundary | [CODE_MAP](spec/CODE_MAP.md), [T52C report](work-log/current-entry/report.md), [handoff](team/HANDOFF_2026-11-04_CODEX.md) |

Current Core retains homecoming gifts/cascades, reusable emptied ordinary branches, Mama/chick following, crows/twigs, swivels, growing branches, day/night and Chorus coins. Do not replace these with the older proposed completion-branch retirement or other withdrawn design contracts. This is a source description, not a new rule specification or acceptance claim. Actual source and authored catalog must be inspected for exact semantics before future changes.

T49/T50 supplies the approved plush/soft forest visual treatment, final ten-species frame art, egg/family overlays and LitMotion-based presentation. Current [ShopView](../PerchSort/Assets/Scripts/Game/ShopView.cs) implements catalog tabs, purchase confirmation and cosmetic actions; [AviaryView](../PerchSort/Assets/Scripts/Game/AviaryView.cs) implements species cards/details and equipment actions. These source implementations do not establish fresh visual or device acceptance. Timed animation and device calibration are not proven by offline assets or instant capture.

## Current paths

- Runtime assemblies/namespaces: `PerchSort.Core`, `PerchSort.Meta`, `PerchSort.Game`; shared Audio remains unchanged.
- Catalog: `PerchSort/Assets/Resources/Levels/levels.json` → `Resources.Load("Levels/levels")`; 40 levels, preserved payload.
- Flags: [dev_options.json](../PerchSort/Assets/Resources/dev_options.json) contains `unlockAllLevels=false` and `grantTestStock=false`. [DevOptions.Load](../PerchSort/Assets/Scripts/Game/DevOptions.cs) reads EditorPrefs with default false in the Editor, reads the resource JSON in development builds, and returns all-false options via `ForBuild(false, null)` in release builds. Tests can override loading in the Editor and development builds. Real progress is not reset by test-mode presentation.
  In the Unity Editor, **Perch Sort → Developer Options → Reset Saved Progress** asks for confirmation and deletes only this game's saved PlayerPrefs, including notifications, coins, stars and settings. Stop Play mode first. The developer-option EditorPrefs are unchanged.
- Sprites: `Resources/Art`, `Resources/Birds`, `Resources/World`. T52 resource contract retains 345 current full-art PNGs plus six still-used compressed fallback PNGs. No runtime numbered fallback folders.
- Editor: `PerchSort/Assets/Editor/Game`, namespace **`PerchSort.Editor`**, containing `GameSceneBuilder.Build`, `GameArtImporter.ImportExisting`, `GameCapture.Run`. The builder creates only Game.unity and never invokes retired importers. Shared `PhoneGameView`, `ReleaseSettings` and Audio importer remain.
- Tests: `Tests/EditMode` (`PerchSort.EditModeTests`), `Tests/Meta` (`PerchSort.Meta.EditModeTests`), `Tests/PlayMode` (`PerchSort.PlayModeTests`). Shared Audio keeps its independent nested asmdef and namespace.
- Retired code/scenes/tests and superseded art were removed by T54E. Retrieve historical evidence from Git, never restore an alternate implementation into the tree.

## Import and persistence safety

`GameArtImporter` preserves existing full-art borders/door pivots and 4096 limits for tall maps/level backgrounds. The six compressed fallback sprites (`dial_sun`, `dial_moon`, `hand_pointer`, `night_overlay`, `raindrop`, `swivel_knot`) retain their separate prior import policy, without full-art compression/mesh/pivot rewrites.

Five egg frames retain custom pivot `(0.5, 0.0625)`; `chick_fluff` and `mama_bow` retain `(0.5, 0.5)`. All seven retain FullRect mesh (`spriteMeshType: 0`), 1024 limit, uncompressed quality 100 and no generated fallback physics shape. The previous broad importer must not recenter eggs or change their baseline mesh. Metadata bytes are resource-owner/integration evidence; actual post-import behavior still needs Mac acceptance.

Persistence strings `v5.coins`, `v5.muted`, `v5.highestUnlocked`, `v5.bestStars.`, `v5.tutorial.`, `v5.booster.`, `v5.starter.`, `v5.bestMoves.`, `v6.unlockSeen.` and `v7.notifications` are **storage compatibility identifiers only**. Keep them byte-identical, along with migrator reads, DataMember/SaveData names, enum order and schema versions. Do not rename serialized data merely to remove a visual prefix.

## Required validation

Workers must perform applicable Mac compilation, full EditMode/PlayMode testing and screenshots in their isolated worktrees, serialized with `flock /tmp/unity.lock`. The coordinator also reviews the integrated game. Never launch competing Unity processes or kill the user's Editor. Static worker reports do not establish runtime acceptance.

`flock /tmp/unity.lock bash scripts/run_tests.sh` builds the sole scene, then runs the full retained suites. T54 baseline: 304 EditMode (237 game, 53 Meta, 14 Audio), 21 PlayMode, no skipped/ignored/explicit cases. LevelsData retains 81 expanded cases (including 40 solver/replays), Layout 41 (including 24 combinatorial). These are expectations, not new results; investigate discovery discrepancies without lowering safeguards.

`flock /tmp/unity.lock bash scripts/screenshots.sh` captures stable-state PNGs under `build/screens/game/`. Instant capture does not prove animation, persistence or device behavior. Animation/audio claims require timed evidence under [the shared contract](work-log/integration-contract/spec.md): real frame sequences and logs. Applicable GUI acceptance also exercises input/cancellation, notifications/persistence, maps/gates and overlay alignment.

PR CI preserves automatic source/configuration checks and the existing .NET test job, retargeted to real current pure-Core fixture files. The shared TestBoards helper has a `DOTNET_CI` JSON/file transport branch; Unity keeps its actual LevelLoader path. This is not a runtime shim or substitute for Unity loader/resource testing. Source/test/fixture changes select the job; retired archives never do.

CI runs the naming guard unconditionally before the .NET test steps. The guard reads Git-tracked paths and uses `git show` for files omitted by sparse checkout; the existing `ci/` sparse entry includes its script. It does not run archived code or replace compilation, gameplay tests or GUI acceptance.

## Historical reference boundary

Old runtime records, PRD/RULES/DATA_SAVE proposals and replay documents remain research/history, not current runtime authority. No historical certificate or passing count validates a new change. Canonical design/art/tool filenames have been cleaned; historical references link to Git where needed. Use this entry and the current code map rather than historical ownership, numbered scenes or obsolete paths.
