# Dead ends and resuming play

## Dead ends

A position can keep legal moves while no sequence of moves (without boosters) can win: two part-flocks are
shuffled back and forth forever. The stuck pointer only fired when no legal move existed, and the Hint offered
the best shuffle move. Required:

- The background hint search (`Solver.hint`) already explores every reachable position before it returns
  `UNSOLVABLE`. A position with that cached result is a dead end.
- In a dead end the stuck pointer marks the tool that helps (undo first), exactly as for a position with no move.
- The Hint gives no move in a dead end (failure icon, no Firefly spent); a one-move fallback is used only when the
  search ran out of budget.
- Rules, levels and solver are unchanged.

## Resuming play

Home Play and the map focus continue from the last level the player opened (`lastLevel`, a new save field):
that level while it is unbeaten, the one after it once beaten; without a record, the first unbeaten level.
Existing save field names are unchanged; older saves without `lastLevel` keep the previous behaviour.
