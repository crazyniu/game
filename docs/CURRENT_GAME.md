# Current game — source and evidence entry

## Identity and authority

There is one Perch Sort game: Godot 4.7.2 (standard build, GDScript, GL Compatibility), portrait 1080x1920 design canvas, 210 fixed levels (21 per world) across ten sequential forest worlds. Open `PerchSort/project.godot`; the main scene is `game/main.tscn`. The [hundred-level round](work-log/hundred-levels/report.md) extended the catalog; subsequent gameplay changes are recorded below. Current presentation is documented in [the design directory](design/) and implemented in `PerchSort/game`. English and Chinese painted locale packs are installed. Runtime implementation is not proof of user visual acceptance. Forest paintings and branch layers remain under `tools/scene`.

## Canonical-edit policy

The [gameplay repairs](work-log/gameplay-repairs/report.md) cover settlement, inventory, animation cancellation,
input, hints, tools and navigation. The [difficulty continuity report](work-log/difficulty-continuity/report.md)
owns the current level order, hint allowances and save migration; the hundred-level round above is historical.
The [day/night removal](work-log/no-day-night/report.md) then set `dayLength` to 0 in all levels and changed Inky to Encore and Hoot to Wake-up song. The unused phase code was removed afterwards.

Gameplay additions since then, each with its own work log:

| Feature | Record |
| --- | --- |
| Free rescue from a dead end and the one-more tool offer on the board | [stuck-rescue](work-log/stuck-rescue/report.md) |
| Pointer to the cheapest tool that rescues a dead end | [rescue-tool](work-log/rescue-tool/report.md) |
| Early losing moves repaired in 14 levels | [early-traps](work-log/early-traps/report.md) |
| Bird skills shown on the level card, by tapping a bird key and as a badge flight | [visible-skills](work-log/visible-skills/report.md), [skill-row](work-log/skill-row/report.md), [skill-cues](work-log/skill-cues/report.md) |
| 210 levels, 21 per world, three tiles per row on the world page | [twenty-one-per-world](work-log/twenty-one-per-world/report.md) |
| Level card sells the fitting tools; guided first use of every tool | [level-card-tools](work-log/level-card-tools/report.md), [tool-guides](work-log/tool-guides/report.md) |
| Partial goal (only the named flocks go home): three trial levels behind `--trial=N`, not in the catalog | [goal-variety](work-log/goal-variety/spec.md), [goal-trial](work-log/goal-trial/report.md) |

The direction and the business goal behind these are in [gameplay-depth](work-log/gameplay-depth/spec.md). None of
this has been played on a device or accepted by the user. Open work is tracked in GitHub issue 255.

- Maintain **one current game**, editing canonical files in place. Never create numbered parallel folders, files, classes, scenes or resource roots, or alternate implementation copies under any other name.
- Task branches and PRs are temporary and merge into `main`; they are not permanent forks.
- `python3 ci/check_current_game_names.py` must pass; it complements human review and runtime acceptance.

[`art/approved_ui`](../art/approved_ui) holds the approved mockups (style direction). Visual acceptance is the user's approval of each clickable screen; approved runtime captures in `art/approved_runtime` are then the baseline. Use the current screen-specific contracts and canonical layout constants; do not restore superseded layouts.

## Read current behavior at its owner

| Concern | Source |
| --- | --- |
| Board model, moves, homecoming waves and gifts, eggs, sleep, crows, twigs, swivels, growing branches, chorus coins, boosters, undo, solver | [core](../PerchSort/core) |
| Profile, unlocks, economy, map states, daily gift, shop, aviary | [meta](../PerchSort/meta) |
| Board, HUD, birds, popups, map, shop, aviary, navigation, audio | [game](../PerchSort/game) |
| Localized whole-label textures and numeric cells | [runtime renderer](../PerchSort/game/ui_text.gd), [English source catalog](../tools/kit/locales/en_docs.json), [locale packs](../PerchSort/art/locale) |
| Design decisions | [design record](GAME_DESIGN.md) (later approved contracts supersede its old layouts) |
| Source organization | [CODE_MAP](spec/CODE_MAP.md), [port contract](work-log/godot-port/spec.md) |

## Persistence

The profile is one JSON file, `user://profile.json`, written by `meta/profile.gd` with the original field names (`coins`, `highestUnlockedLevel`, `boosters`, `bestStars`, `clearCounts`, `bestMoves`, `ownedOutfits`, `equippedOutfits`, ...) and `schemaVersion`. Keep field names and enum numeric orders stable and retain all existing migrations. Schema 4 moves per-puzzle records into the continuous difficulty order through `LEVEL_DIFFICULTY`; schema 5 spreads the old 100 levels over the 210-level catalog through `LEVEL_SPREAD`. Both launchers apply the same mappings.

## Validation

`bash scripts/run_tests.sh` runs the naming guard, the smoke test (every script loads, every level builds on the real board) and the core test (every level replayed through a session along its stored solution). `bash scripts/screenshots.sh` writes silent captures of every screen state to `build/screens/`. Stills do not prove animation; animation and audio claims need timed frame sequences (`--frames`) and logs (`--autoplay`). CI runs the same tests on headless Godot and does not replace GUI acceptance.
