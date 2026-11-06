# Level authoring

Levels are authored in `PerchSort/levels/levels.json` (schema described in [GAME_DESIGN](../../docs/GAME_DESIGN.md), sections 11.1 and 12). The authoritative validator is the Godot core test, which builds every level, solves it with the game's own solver and replays the solution through a session:

```sh
godot --headless --path PerchSort -s res://tests/run_core.gd
```

`perch_levels.py` is a legacy helper and not a current validator.
