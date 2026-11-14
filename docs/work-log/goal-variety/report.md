# Goal variety — report

Design only (issue #251): [spec](spec.md). No code, level data or art changed.

## Work

- Four candidate goals evaluated against the bar of the Dive outcome (readable from one picture, calm pillar,
  winnable without tools, tool moments for the rewarded-ad business).
- A throwaway model of the plain rules plus each goal measured random boards; it agrees with the real `Solver` on
  every example whose plain form the current rules express. The order and partial goals were also patched into a
  scratch copy of `PerchSort/core` outside the repository; their worked examples are solved by the real solver and
  replayed to a win through `GameSession`. None of this is committed.

## Result

- Recommended: (d) partial goal with leftover birds, then (a) order goal. Dropped: (b) cage (never changed a
  route in 1499 placements), (c) hard move limit (a fail state).
- Worked examples: order 8 moves (6 without the gate), partial 6 moves, cage 6 moves (same with or without the
  cage), move limit on the plain order board.

## Boundaries

- Eggs, sleepers, mama birds and special branches were not modelled.
- No runtime change, so no tests, captures or frame sequences apply; nothing was played.
