# Current game — source and evidence entry

## Identity and authority

There is one current Perch Sort game: the accepted T49/T50 source/art behavior, reorganized by T52. Unity 6000.6.7f1, portrait 1080×1920 design canvas, 40 fixed authored levels across four sequential forest worlds. T52 changes organization and identity, not rules, economy, approved art or progress.

Open `PerchSort/Assets/Scenes/Game.unity`. Its retained scene GUID is `55064aed0051b40279c77a0b9a758533`; GameRoot script GUID is `07203ec62b1604449b634c729c1303a3`. The root retains its serialized Fredoka font reference. There is exactly one enabled build scene. Static preservation does not prove Unity import: InputSystem scene action IDs and actual root initialization need coordinator verification.

## Permanent canonical-edit policy and review acceptance

- Maintain **ONE current game**, editing canonical files in place. Never create V-numbered parallel folders, files, classes, namespaces, scenes or resource roots, or alternate current implementation copies under any other name.
- Task branches and PRs are temporary collaboration reviewed and merged into `main`; they are not permanent runnable forks or alternative current implementations.
- Only three narrow exceptions exist: **exact existing storage IDs solely for save compatibility**; **preserved historical archives/original-art provenance**, never active implementation copies; and **legitimate dependency/schema/release metadata**. Existing storage IDs do not authorize new versioned keys. No new arbitrary exceptions or broad allowlists.
- Review acceptance must check canonical in-place edits, absence of parallel implementation identities/copies, one enabled Game scene, canonical resource roots, and an exact allowed reason for each retained version-looking identifier. `python3 ci/check_current_game_names.py` must pass before tests; the guard complements human review and required Mac acceptance.

[`art/approved_ui/layout.json`](../art/approved_ui/layout.json) is **historical-original art/layout provenance**, preserved with its original delivery metadata and paths. It is not an active runtime version, current resource root or alternate game entry. Consult canonical GameLayout/current resources for the implementation; do not reactivate the record's obsolete paths.

## Read current behavior at its owner

| Concern | Source / evidence |
| --- | --- |
| Board model, move resolution, eggs, sleep/family/crow, swivel/growth/day-night, Chorus, undo, solver | [Core](../PerchSort/Assets/Scripts/Core) |
| Profile, unlocks, world/collection/economy policies | [Meta](../PerchSort/Assets/Scripts/Meta) |
| Map, HUD, board, notifications, intro, panels and motion | [Game](../PerchSort/Assets/Scripts/Game) |
| Accepted mechanics/design decisions | [design record](GAME_DESIGN.md) (historical filename; later contracts supersede its old layouts/roadmap) |
| Latest accepted bird/overlay and presentation integration | [T49](../tasks/T49_report.md), [egg/overlay report](../tasks/T49_eggs_report.md), [T50](../tasks/T50_report.md) and linked A/B/C/D/E owner reports |
| Current organization / verification boundary | [CODE_MAP](spec/CODE_MAP.md), [T52C report](../tasks/T52C_report.md), [handoff](team/HANDOFF_2026-11-04_CODEX.md) |

Current Core retains homecoming gifts/cascades, reusable emptied ordinary branches, Mama/chick following, crows/twigs, swivels, growing branches, day/night and Chorus coins. Do not replace these with the older proposed completion-branch retirement or other withdrawn design contracts. This is a source description, not a new rule specification or acceptance claim. Actual source and authored catalog must be inspected for exact semantics before future changes.

T49/T50 supplies the approved plush/soft forest visual treatment, final ten-species frame art, egg/family overlays and LitMotion-based presentation. Current Shop/Aviary map entries remain the previously authorized placeholders, not completed full shop/collection screens. Timed animation and device calibration are not proven by offline assets or instant capture.

## Current paths

- Runtime assemblies/namespaces: `PerchSort.Core`, `PerchSort.Meta`, `PerchSort.Game`; shared Audio remains unchanged.
- Catalog: `PerchSort/Assets/Resources/Levels/levels.json` → `Resources.Load("Levels/levels")`; 40 levels, preserved payload.
- Flags: `PerchSort/Assets/Resources/dev_options.json` → `Resources.Load("dev_options")`. Existing `unlockAllLevels=true`, `grantTestStock=true` are preserved, not release defaults. Real progress is not reset by test-mode presentation.
- Sprites: `Resources/Art`, `Resources/Birds`, `Resources/World`. T52 resource contract retains 345 current full-art PNGs plus six still-used compressed fallback PNGs. No runtime numbered fallback folders.
- Editor: `PerchSort/Assets/Editor/Game`, namespace **`PerchSort.Editor`**, containing `GameSceneBuilder.Build`, `GameArtImporter.ImportExisting`, `GameCapture.Run`. The builder creates only Game.unity and never invokes retired importers. Shared `PhoneGameView`, `ReleaseSettings` and Audio importer remain.
- Tests: `Tests/EditMode` (`PerchSort.EditModeTests`), `Tests/Meta` (`PerchSort.Meta.EditModeTests`), `Tests/PlayMode` (`PerchSort.PlayModeTests`). Shared Audio keeps its independent nested asmdef and namespace.
- Retired code/scenes/tests/tools: `archive/retired/` outside Unity Assets and runtime/test CI inputs. Historical originals/research stay preserved, not runnable alternatives.

## Import and persistence safety

`GameArtImporter` preserves existing full-art borders/door pivots and 4096 limits for tall maps/level backgrounds. The six compressed fallback sprites (`dial_sun`, `dial_moon`, `hand_pointer`, `night_overlay`, `raindrop`, `swivel_knot`) retain their separate prior import policy, without full-art compression/mesh/pivot rewrites.

Five egg frames retain custom pivot `(0.5, 0.0625)`; `chick_fluff` and `mama_bow` retain `(0.5, 0.5)`. All seven retain FullRect mesh (`spriteMeshType: 0`), 1024 limit, uncompressed quality 100 and no generated fallback physics shape. The previous broad importer must not recenter eggs or change their baseline mesh. Metadata bytes are resource-owner/integration evidence; actual post-import behavior still needs Mac acceptance.

Persistence strings `v5.coins`, `v5.muted`, `v5.highestUnlocked`, `v5.bestStars.`, `v5.tutorial.`, `v5.booster.`, `v5.starter.`, `v5.bestMoves.`, `v6.unlockSeen.` and `v7.notifications` are **storage compatibility identifiers only**. Keep them byte-identical, along with migrator reads, DataMember/SaveData names, enum order and schema versions. Do not rename serialized data merely to remove a visual prefix.

## Required validation

The user superseded the old no-tests instruction: the current Codex coordinator must integrate all task deliveries and serialize **Mac compilation, EditMode, PlayMode and GUI acceptance**. T52 workers do not launch Unity independently. No worker static report establishes runtime acceptance.

`bash scripts/run_tests.sh` builds the sole scene, then checks at least 304 EditMode cases (237 current game, 53 Meta, 14 Audio) and 17 PlayMode UnityTests, with no skipped/ignored/explicit cases. It separately guards LevelsData's 81 expanded cases (including 40 optimal solver/replays) and Layout's 41 (including 24 combinatorial). These are source-derived expectations; investigate actual discovery discrepancies.

`bash scripts/screenshots.sh` uses the current capture tool for stable-state PNGs under `build/screens/game/`. It preserves current first-level/map/popup/mechanic checks. Old saved-resume/animation-sequence/aspect assertions with no current equivalent are archived, not rewritten into false proof. Required GUI acceptance must additionally exercise real motion, input/cancellation, notifications/persistence, maps/gates and overlay alignment. Record exact results and remaining limits.

PR CI preserves automatic source/configuration checks and the existing .NET test job, retargeted to real current pure-Core fixture files. The shared TestBoards helper has a `DOTNET_CI` JSON/file transport branch; Unity keeps its actual LevelLoader path. This is not a runtime shim or substitute for Unity loader/resource testing. Source/test/fixture changes select the job; retired archives never do.

CI runs the naming guard unconditionally before the .NET test steps. The guard reads Git-tracked paths and uses `git show` for files omitted by sparse checkout; the existing `ci/` sparse entry includes its script. It does not run archived code or replace compilation, gameplay tests or GUI acceptance.

## Historical reference boundary

The v2 runtime records and v4 PRD/RULES/DATA_SAVE/MECHANIC_EVIDENCE/replay documents remain research/history, not current runtime authority. No historical certificate or passing count validates this cutover. Historical design/task filenames and originals under `art/` remain unchanged so their evidence and provenance survive. Use this entry and the current code map instead of old “future implementation”, Linux/Claude-led or numbered-scene instructions.
