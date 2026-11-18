# Current catalog authoring prerequisite

## Authority

The coordinator assigned a bounded current-main review of #248, #249, #250 and #239 on 2026-11-18 and
approved correction of the surviving authoring tool. This is a prerequisite of #250 item 4, not authority
to replace puzzles, select world mechanics, reorder the shipped catalog, change UI/art or migrate saves.

## Concrete defects

- `assemble_levels.py` used 100 levels and 10 positions per world, so a current 210-key hint catalog could
  fail remapping after selecting only 100 puzzles.
- A stale or partial measurement table was consumed without checking complete current-catalog coverage.
- Reorder inferred counted moves from an obsolete Encore bit in `search_key[0]`. The current key stores
  homed species there; the calculation counted raw actions and could raise authored pars incorrectly.
- The retired expansion mode could substitute candidate puzzles for current puzzles omitted from an old
  measurement list. It has no role in the fixed current catalog.
- The README recommended this heuristic as the current delivered order and implied launcher migration
  tables still existed, despite the Cocos migration.

## Work

Keep one proposal-only reorder command, set the current 210/21 dimensions, require complete unique input
IDs before any writes, retain all hints and authored pars, and distinguish cached actions from supplied
counted-move measurements. Keep measured `actions` separate from `cached_actions`; require a measured
action count for an exact claim. Preserve supplied proof flags without claiming new proof. Retire expansion mode.
Document the active Cocos paths and catalog decision/dependency boundaries.

## Ownership and verification

Own `tools/levels/assemble_levels.py`, `tools/levels/README.md` and this work-log directory only. No runtime
catalog or save files change. No local tests, lint, builds, screenshots, generation or temporary validation
programs are authorized for this task. Source review and existing GitHub CI gate the PR; report this limit.
