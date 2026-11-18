# Level authoring

The current fixed catalog contains **210 levels, 21 per world**. Catalog and stored solutions live in
`PerchSort/assets/resources/levels/`; the Cocos-independent TypeScript rules and solver live in
`PerchSort/assets/scripts/core/`.

## Current tool

`assemble_levels.py` can produce a complete **reorder proposal**. Its historical structural heuristic is not
the agreed difficulty measure requested by [#250](https://github.com/crazyniu/game/issues/250). It does not
establish monotonic difficulty, enforce world signatures or reserve finales. Do not ship its output as the
accepted catalog order.

```sh
python3 tools/levels/assemble_levels.py --reorder \
  PerchSort/assets/resources/levels/levels.json \
  PerchSort/assets/resources/levels/hints.json \
  <current-210-level-curve.json> <proposal-dir> <report-dir>
```

The catalog, hints and measurement table must each cover IDs 1 through 210 exactly once. The measurement
input is a JSON array with `level`, `species`, `win`, `mechanics` and `counted`; `known_counted` and `exact`
are optional. Measurements must describe the supplied puzzles under the current rules. The historical
100-level `difficulty-continuity/curve.json` is not a current input. `catalog-measure/fragility.csv` is a
historical measurement record, not this command's JSON input format.

The proposal retains puzzle bodies, authored pars and every cached route. It assigns 21 positions per world,
preserves destination variant metadata and writes `levels.json`, `hints.json`, `curve.json` and `renumber.json`.
`actions` is the cached route length; `counted` comes from the supplied measurements because homecoming gifts
can refund scored moves. Neither a cached route nor the old heuristic is a shortest-action proof. The tool
preserves the input `exact` flag but performs no new proof or replay.

The former expansion/candidate assembly mode is retired: the game already has its fixed 210 puzzles. A
partial old measurement list must not silently replace unmeasured current puzzles with candidate entries.
Godot authoring commands (`level_lab.gd`, `deal_siblings.gd`, `measure_fragility.gd`, `export_hints.gd`) were
removed with the engine migration. Historical issue instructions for those files are not runnable Cocos
workflows; port the required authoring operation against the current pure rules in its own scoped task.

## Catalog decisions still required

The dependency chain is [#247](https://github.com/crazyniu/game/issues/247) (completed measurement and sibling
repair), [#248](https://github.com/crazyniu/game/issues/248) (world mechanics),
[#249](https://github.com/crazyniu/game/issues/249) (finales), then
[#250](https://github.com/crazyniu/game/issues/250) (difficulty order and save migration).

- Confirm #248's full ten-world table, including exact combinations for worlds 8–10 and allowed supporting
  mechanics, before replacing puzzles. Resolve whether replaced puzzles inherit old scores; this conflicts
  with the puzzle-identity rule recorded in [#239](https://github.com/crazyniu/game/issues/239).
- #249's latest amendment sets its finale route at **1.0–1.15 times level 20**, superseding the issue body's
  1.2–1.5 times median proposal. Finales still require the prior world policy and approval of any changed map UI/art.
- Agree #250's difficulty measure and tolerance before reordering. Audit all **209 actual adjacent pairs**,
  including the nine world boundaries from local **21 to 1**, not only local 20 to the next world's 1.
- Before delivering changed IDs, implement `renumber.json` as the next explicit migration in
  `PerchSort/assets/scripts/meta/Profile.ts`. Preserve all per-puzzle records, unlock progress, last level and
  earned tools. The current launchers import through `Profile.ts`; they no longer duplicate migration tables.

The tool correction does not close #239, #248, #249 or #250. Their design, catalog, migration and human-play
acceptance remains outstanding. Follow the repository's user-owned test policy; do not run catalog generation
or verification merely because a historical work log lists a command.
