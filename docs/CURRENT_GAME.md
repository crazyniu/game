# Current game — source and evidence entry

## Identity and authority

There is one Perch Sort game: Godot 4.7.2 (standard build, GDScript, GL Compatibility), portrait 1080x1920 design canvas, 100 fixed levels across ten sequential forest worlds. Open `PerchSort/project.godot`; the main scene is `game/main.tscn`. The game was rebuilt from the former Unity project, which now lives only in Git history; rules were carried over unchanged; the [hundred-level round](work-log/hundred-levels/report.md) extended and reordered the levels. Current [UI work](design/ui-system.md) improves complete localized labels, icons, spacing and readability. The user rejected broad recolouring and enclosing header/navigation cards; the original palette and independent controls are restored. English is the only authored locale. [UI polish](work-log/ui-polish/report.md) records the corrected scope, uniform board bird scale and pending user acceptance. Forest paintings and branch layers remain under `tools/scene`.

## Canonical-edit policy

- Maintain **one current game**, editing canonical files in place. Never create numbered parallel folders, files, classes, scenes or resource roots, or alternate implementation copies under any other name.
- Task branches and PRs are temporary and merge into `main`; they are not permanent forks.
- `python3 ci/check_current_game_names.py` must pass; it complements human review and runtime acceptance.

[`art/approved_ui`](../art/approved_ui) holds the approved mockups (style direction). Visual acceptance is the user's approval of each clickable screen; approved runtime captures in `art/approved_runtime` are then the baseline. Layout comes from the UI system, not from measured mockup rects.

## Read current behavior at its owner

| Concern | Source |
| --- | --- |
| Board model, moves, homecoming waves and gifts, eggs, sleep, crows, twigs, swivels, growing branches, day/night, chorus coins, boosters, undo, solver | [core](../PerchSort/core) |
| Profile, unlocks, economy, map states, daily gift, shop, aviary | [meta](../PerchSort/meta) |
| Board, HUD, birds, popups, map, shop, aviary, navigation, audio | [game](../PerchSort/game) |
| Localized whole-label textures and numeric cells | [runtime renderer](../PerchSort/game/ui_text.gd), [English source catalog](../tools/kit/locales/en.json), [locale packs](../PerchSort/art/locale) |
| Design decisions | [design record](GAME_DESIGN.md) (later approved contracts supersede its old layouts) |
| Source organization | [CODE_MAP](spec/CODE_MAP.md), [port contract](work-log/godot-port/spec.md) |

## Persistence

The profile is one JSON file, `user://profile.json`, written by `meta/profile.gd` with the original Unity-era field names (`coins`, `highestUnlockedLevel`, `boosters`, `bestStars`, `clearCounts`, `bestMoves`, `ownedOutfits`, `equippedOutfits`, ...) and `schemaVersion`. Keep field names, schema values and enum numeric orders stable. Saves from the former Unity build (PlayerPrefs) are not imported; that build was never released.

## Validation

`bash scripts/run_tests.sh` runs the naming guard, the smoke test (every script loads, every level builds on the real board) and the core test (every level replayed through a session along its stored solution). `bash scripts/screenshots.sh` writes silent captures of every screen state to `build/screens/`. Stills do not prove animation; animation and audio claims need timed frame sequences (`--frames`) and logs (`--autoplay`). CI runs the same tests on headless Godot and does not replace GUI acceptance.

## Historical reference boundary

Unity-era work logs, PRD/RULES/DATA_SAVE proposals and replay documents are research and history, not current runtime authority. No historical passing count validates a new change.
