# Catalog measurement and sibling repair

## Scope
Measure the current 210-level catalog, and repair only sibling levels outside a 0.67–1.5 cached-route ratio to their original. No level order, ids, worlds, save schema, new mechanics or UI change. Leave the user's separate local catalog worktree untouched.

The original 10 puzzles of each world occupy odd local slots 1–19. Even slots are their siblings; slot 21 is another sibling of slot 19. Compare those pairs, not arbitrary neighbours. Global monotonic difficulty remains issue 239/250 and is not claimed here.

## Acceptance
- 210 measurements with route length, alternate-move traps/unknowns and 40 seeded random plays each.
- Every replacement has a stored booster-free winning route, retains its original world/branch geometry and mechanics, and has no unresolved or losing alternative beside its first three stored-route steps.
- Each sibling route is within 0.67–1.5 of its original; teaching levels with a one-action original therefore require a one-action sibling.
- Re-export only changed solutions. Untouched levels and hint entries stay identical.
- Replay all 210 solutions, verify data invariants and run the repository aggregate tests. Keep device/engine-version limits explicit.

## Generator corrections
The old generator rejected full single-species sleeping flocks as though already solved, although sleeping birds cannot go home. This needlessly destroyed teaching layouts. Preserve these valid flags; reject only an awake ready flock. Never emit an out-of-band fallback, and do not treat a search-budget-exceeded alternative as proven safe.
