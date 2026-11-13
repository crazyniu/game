# Documentation router — one current game

Start with [CURRENT_GAME](CURRENT_GAME.md), then [CODE_MAP](spec/CODE_MAP.md), the [port contract](work-log/godot-port/spec.md) and [AGENTS](../AGENTS.md). The current game is the Godot 4.7.2 project in `PerchSort/` (`core`, `meta`, `game`, assets, `levels/levels.json`).

| Need | Read | Boundary |
| --- | --- | --- |
| Project conventions | [CONVENTIONS](CONVENTIONS.md) | Binding naming, layout and verification rules |
| Current behavior and paths | [CURRENT_GAME](CURRENT_GAME.md), [CODE_MAP](spec/CODE_MAP.md) | Source facts, not a new gameplay redesign |
| Current collaboration / execution | [AGENTS](../AGENTS.md) | Shared scope, ownership and execution rules |
| Existing gameplay decisions | [design record](GAME_DESIGN.md), [Chinese gameplay record](GAMEPLAY_CN.md) | Canonical filenames; old ownership/paths/layouts are superseded by current entry and accepted later contracts |

## History

Retired implementation, old documents and superseded art were removed; Git retains their history. Version-like persistence keys remain only for user-data compatibility. Do not restore old runtime paths or create a second current specification.

## Testing status

`bash scripts/run_tests.sh` (naming guard, smoke test, 40-level core test) and `bash scripts/screenshots.sh` (silent captures of every screen state). Stills do not prove animation; timed claims need frame sequences and logs. CI runs the same tests on headless Godot and cannot establish GUI acceptance.
