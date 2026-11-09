# Level authoring

Levels are authored in `PerchSort/levels/levels.json` (schema described in [GAME_DESIGN](../../docs/GAME_DESIGN.md), sections 11.1 and 12). `level_lab.gd` measures and deals levels on the game's own rules, `assemble_levels.py` orders them along the difficulty curve, and `export_hints.gd` stores each level's solution. The authoritative validator is the Godot core test, which builds every level and replays its stored solution through a session:

```sh
godot --headless --path PerchSort -s res://tests/run_core.gd
```

`perch_levels.py` is a legacy helper and not a current validator.
