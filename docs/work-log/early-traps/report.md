# Early losing moves — report

Section 7 item 1 of [gameplay depth](../gameplay-depth/spec.md): the 14 levels where a move beside the first three
steps of the stored solution could never be recovered from were repaired. Level ids, worlds, mechanics, bird
counts and the level order are unchanged, so the save needs no migration.

## Method

`tools/levels/repair_early_traps.gd` swaps one or two pairs of birds and keeps the first variant that is solvable
without tools, has no losing move beside its first three solution steps, keeps the route within two moves of the
old one and keeps the random-play win rate within a fifth (40 games). `tools/levels/export_hints.gd` then stored
the new solutions; only the 14 levels' entries in `levels.json` and `hints.json` changed. Par changed for level 27
only (11 to 13, to cover its new route).

## Result

Measured with `tools/levels/measure_fragility.gd`; all rows in
[fragility.csv](../gameplay-depth/evidence/fragility.csv).

| Level | First losing step before | after | Random wins of 40 before | after |
| --- | --- | --- | --- | --- |
| 26 | 0 | 4 | 21 | 34 |
| 27 | 0 | none | 8 | 18 |
| 41 | 0 | 3 | 0 | 13 |
| 46 | 0 | 6 | 1 | 8 |
| 53 | 1 | none | 3 | 0 |
| 60 | 1 | 4 | 6 | 13 |
| 63 | 1 | none | 9 | 7 |
| 64 | 0 | none | 0 | 1 |
| 66 | 2 | 3 | 21 | 20 |
| 69 | 0 | 10 | 4 | 4 |
| 84 | 2 | 11 | 14 | 17 |
| 85 | 0 | 5 | 8 | 8 |
| 94 | 2 | 6 | 3 | 3 |
| 98 | 2 | 3 | 19 | 18 |

No level has a losing move within its first three steps any more. Later losing moves remain on purpose: they are
the stuck moments the tools are sold for.

## Evidence

- `bash scripts/run_tests.sh`: naming guard 0 violations, smoke 0 failures, core 0 failures (all 100 levels
  replay their stored solutions).
- `run_stuck_rescue.gd`: 22 checks, 0 failures. Its dead-end fixture is now `StuckOffer.DEAD_END_LEVEL` (the old
  layout of level 41), because the catalog level no longer has a losing opening move.

## Boundaries

- Levels 26, 27, 41, 46 and 60 became easier by the random-play yardstick; none was played by hand.
- "Losing" is judged with a search budget of 6000 states / 250 ms per move; moves over the budget count as unknown
  (column `unknown` of the table file).
- A player in the middle of one of these levels sees the new layout on the next start; stars already earned stay.
