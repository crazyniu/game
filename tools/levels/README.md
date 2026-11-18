# Level authoring

The Cocos catalog and stored solutions are in `PerchSort/assets/resources/levels/`. The pure TypeScript rules and solver are in `PerchSort/assets/scripts/core/`.

To reorder an existing catalog without changing its puzzles or searching again:

```sh
python3 tools/levels/assemble_levels.py --reorder <levels.json> <hints.json> <curve.json> <out dir> <report dir>
```

The output includes remapped hints, pars, a difficulty curve and a renumber table. Add that table to the existing
profile and launcher migration sequence before delivering changed IDs. See the current
[difficulty report](../../docs/work-log/difficulty-continuity/report.md) for the delivered order and acceptance boundary.
