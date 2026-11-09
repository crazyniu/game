# Level authoring

Levels are authored in `PerchSort/levels/levels.json` (schema described in [GAME_DESIGN](../../docs/GAME_DESIGN.md), sections 11.1 and 12). `level_lab.gd` measures and deals levels on the game's own rules, `assemble_levels.py` orders them along the difficulty curve, and `export_hints.gd` stores each level's solution. The authoritative validator is the Godot core test, which builds every level and replays its stored solution through a session:

```sh
godot --headless --path PerchSort -s res://tests/run_core.gd
```

The assembler orders puzzles globally by initial flock fragmentation, species, mechanics and measured random-play
win rate. A known short solution caps the score; long solver routes do not increase its structural component.
There are no forced teaching levels at chapter boundaries. Tutorials and mechanic-tool unlocks follow level data.
Hint export also raises par to cover the exported route, including free Encore moves.

To reorder an existing catalog without changing its puzzles or searching again:

```sh
python3 tools/levels/assemble_levels.py --reorder <levels.json> <hints.json> <curve.json> <out dir> <report dir>
```

The output includes remapped hints, pars, a difficulty curve and a renumber table. Add that table to the existing
profile and launcher migration sequence before delivering changed IDs. See the current
[difficulty report](../../docs/work-log/difficulty-continuity/report.md) for the delivered order and acceptance boundary.

`perch_levels.py` is a legacy helper and not a current validator.
