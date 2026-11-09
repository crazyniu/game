# Difficulty continuity report

## Changes

- Reordered the existing 100 puzzles globally, retaining puzzle rules and branch layouts. The six short teaching
  puzzles formerly at 11, 21, 31, 41, 51 and 61 now appear at 7, 3, 8, 11, 9 and 10. They no longer reset later chapters.
- Replaced the solver-length term with initial flock fragmentation. A known short win caps the structural score,
  so chain-heavy teaching boards remain early even when they contain many species or mechanics.
- The delivered curve rises from 3.2 to 36.5. The five-row teaching puzzle moves to the second forest at level 11,
  tied at score 6 with levels 7-10; this capacity adjustment creates no score drop.
- Moved cached hint routes with their puzzles and increased pars where those routes exceeded them. The reported
  examples are now level 12 (old 6, 9 counted moves / par 9), level 19 (old 8, 13 / 13), and level 29 (old 25, 15 / 15).
- Updated future hint export to write a par that includes the generated route's counted moves.
- Added the profile migration through the existing schema field. Per-puzzle stars, clear counts, best moves and
  legacy level gifts follow their puzzles. Previously reachable puzzles stay reachable; a migration may open
  additional intervening levels instead of locking a moved puzzle. Both launchers carry the same migration.
- Mechanic tools unlock at their first fitting puzzle. Already held tools and granted starter stocks stay usable.
  Fitting-tool detection now reads actual branch kinds and bird tokens. The coordinator updates the toolbar to
  display every applicable unlocked tool and the unlock-popup caller to use the dynamic threshold.

## Authored data

- [curve.json](curve.json): current position, source puzzle ID, formula score, cached route count, best known route
  count and par. `exact` is false for cached hint routes; no shortest-path claim is made.
- [renumber.json](renumber.json): the full pre-repair ID to current ID map, also stored as `Profile.LEVEL_DIFFICULTY`.
- The old hundred-level work log remains historical evidence; it describes the prior teaching order and score.

The final catalog was authored with `assemble_levels.py --reorder`, reading the catalog and hints from commit
`b30804cd` and the existing `docs/work-log/hundred-levels/curve.json`, writing the canonical `PerchSort/levels`
files plus the data in this work log. No puzzles were solved or played during this operation. Source and emitted
data were read for review; the launchers were edited but not run, and the user's save was not opened or changed.

## Remaining acceptance

No tests, lint, builds, Godot runs, UI automation or screenshots were performed. The random-play rates and known
short routes are retained repository evidence, not fresh measurements. Hint replay, saved-progress migration,
tool availability, forest layout and perceived difficulty need the user's acceptance. Route lengths can still
vary between adjacent puzzles; the score is an ordering heuristic, not a proof of human difficulty.
