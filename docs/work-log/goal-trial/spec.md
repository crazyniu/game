# Goal trial — spec

Issue: #260. Rule contract: [goal variety](../goal-variety/spec.md), section 3.4 (partial goal). Owner: Claude
(bounded worker); the user plays the trial and decides.

## Decisions (coordinator, delegated by the user on 2026-11-14)

1. Leftover birds that can never form a flock (at most two of a kind) are allowed on partial-goal levels.
2. Only the partial goal is tried now; the order goal waits for the user's verdict on this trial.
3. At the win the leftover birds fly away.
4. The empty check ring for the bird keys is painted from the existing check art (local Codex).
5. How many goal levels per world is decided after the trial.

## Rule

- Level field `"goal": ["<species>", ...]`. Absent (or empty) means today's rule: every flock home. No catalog
  level has the field.
- `Board.goal_mask`: bit per named kind, copied by `clone`, not part of `search_key` (a constant of the level, so
  stored hints keep their keys). `Board.is_won`: every named kind in `homed`; without a goal, unchanged.
- `Levels.create_board`: a named kind must be a real kind on the board, not a crow, named once, with exactly
  `capacity` birds; other kinds have at most `capacity` birds and may be fewer. Without a goal every real kind
  still needs exactly `capacity`.
- `Rules._auto_home` does not fire once the level is won (no change for catalog levels, where the last flock is
  never home at that point).
- Solver, hints, undo, the rescue and stars go through `is_won` and work unchanged; `par` is the shortest route to
  the goal.

## Presentation

- Bird keys under the house on a goal level: only the named kinds, each with an empty ring (`art/pic_check_ring.png`,
  same 256 px canvas as `pic_check`) at the corner; the existing check fills it when that flock is home. Leftover
  kinds get no key. Catalog levels: unchanged (one key per kind, no ring).
- At a goal win the birds still on the board take off over the nearest screen edge (staggered, about 1.2 s), then
  the usual celebration and the win panel.

## Trial levels

`PerchSort/levels/trial.json`, ids 9001 to 9003 (outside the catalog), opened only with `--trial=<1..3>`
(`run.sh` / `run.ps1` / `main.tscn -- --trial=N`). Played on a copy of the profile: nothing saved, no progress,
coins or drops; the win panel leads to the next trial level. Trial 1 shows the hint hand on its first move.

| Trial | Goal | Shape | Moves |
| --- | --- | --- | --- |
| 1 | mint | the teaching example of the goal-variety spec 3.4, 5 branches | 6 |
| 2 | mint, cocoa | 6 branches, leftovers parked along the route | 10 to 14 |
| 3 | cocoa | sleeper and egg among the cocoa, 7 branches | 12 to 18 |

Each: solvable without tools, no losing move in the first three positions of the stored route, and the leftovers
matter (the same board with the leftovers removed is much shorter).

## Files

`core/board.gd`, `core/levels.gd`, `core/rules.gd`; `game/board_view.gd` (bird keys section, win hand-off in
`_celebrate`, a `goal_win` preview state for frame sequences); `game/main.gd` (`open_trial`, `--trial`);
`run.sh`, `run.ps1`; `levels/trial.json`; `art/pic_check_ring.png`; `tests/run_goal.gd` (in
`scripts/run_tests.sh`).

## Not in scope

The order goal, catalog levels, the level card, new words.
