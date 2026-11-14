# Goal trial — report

Issue: #260. Spec: [spec](spec.md). Rule contract: [goal variety](../goal-variety/spec.md) 3.4.

## Changes

- `core/board.gd`: `goal_mask` (copied by `clone`, not in `search_key`), `is_won` checks only the named kinds when a
  goal is set, `in_goal(species)`.
- `core/levels.gd`: parses `"goal"`; validator: named kinds real, not crow, named once, exactly `capacity` birds;
  other kinds at most `capacity` (fewer allowed) on goal levels only. `Levels.trial(n)` / `trial_count()` read
  `levels/trial.json`.
- `core/rules.gd`: `_auto_home` does nothing once the level is won (no effect on catalog levels).
- `core/session.gd`, `core/solver.gd`: unchanged; solver, hints, undo, rescue and stars follow `is_won`.
- `game/board_view.gd`: bird keys section: on a goal level only the named kinds get keys, each with the new empty
  ring (`art/pic_check_ring.png`, painted with local Codex from `pic_check`, 256 px like the check) under the check.
  `_celebrate`: leftover birds take off over the nearest screen edge (staggered, about 1.2 s) before the fanfare.
  Preview state `goal_win` (trial 1, last move) for frame sequences.
- `game/main.gd`: `open_trial(n)` and `--trial=N`: real board, copy of the profile, win panel to the next trial
  level, trial 1 shows the hint hand on its first move. `run.sh` / `run.ps1` document `--trial=1`.
- `levels/trial.json`: three levels (ids 9001-9003, world 1, noon).
- `tests/run_goal.gd`, added to `scripts/run_tests.sh`.

## Trial levels

Routes are the real `Solver.solve` shortest routes. "Without the leftovers" is the same board with every leftover
bird removed (goal kinds only); without the goal at all the boards cannot be won (leftovers can never go home).

| Trial | Goal | Branches | Leftovers | Par (with goal) | Without the leftovers | Leftover parks in route | Losing moves in first 3 positions |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | mint | 5 | sunny 2, cocoa 2, ember 1, sky 1 | 6 | 1 | 3 | 0 |
| 2 | mint, cocoa | 6 | sunny 2, sky 2, snow 1 | 11 | 4 | 5 | 0 |
| 3 | cocoa (one asleep, one egg) | 7 | berry 2, ink 2, ember 2, snow 2, sunny 2, mint 1, sky 1 | 12 | 1 | 9 | 0 |

Routes (branch indices in file order):
- 1: 1-2, 1-4, 0-1, 0-4, 3-0, 3-4.
- 2: 1-5, 3-0, 3-1, 3-0, 2-3, 2-5, 2-1, 2-3, 4-2, 4-5, 3-4.
- 3: 0-4, 1-6, 5-1, 2-5, 2-6, 0-2, 3-0, 3-4, 2-3, 5-2, 5-0, 3-5.

Levels 2 and 3 were found by a scratch generator (random deals and local search, outside the repository) and
checked on the real core.

## Commands and results

- `bash scripts/run_tests.sh`: exit 0. Naming guard OK; launcher recovery: five scenarios passed; smoke checks: 0
  failures; core checks: 0 failures (catalog solutions replay unchanged); bird animation failures=0;
  run_goal: 94 checks, 0 failures. The goal test prints expected `ERROR:` lines from its reject cases.
- `godot --headless --path PerchSort --audio-driver Dummy -s res://tests/run_goal.gd`: 94 checks, 0 failures
  (validator accept/reject, win with leftovers, undo across the win, restart, no win without the goal, the three
  trial levels solved and replayed at par, key row content, ring state through a whole route and after undo,
  catalog row unchanged).
- Stills: `bash scripts/capture.sh <abs>/trial_N.png --trial=N --lang=zh --wait=7` (N = 1, 2, 3).
- Frames: `xvfb-run -a godot --path PerchSort --audio-driver Dummy -- --preview=board_view:goal_win --frames=<dir> --wait=6 --step=0.25`.

Evidence (not tracked): `/home/developer/Projects/game-birds/docs/work-log/goal-trial/evidence/` — `trial_1.png`,
`trial_2.png`, `trial_3.png`, `goal_win_frames/f000-f023.png`, `goal_win_sheet.png` (frames 4-21).

## Boundaries

- Not played by a person or on a device; the user plays `./run.sh --trial=1` and judges.
- In the frame sequence the leftovers sit about 1.5 s while the winning flock flies home and its gift plays, then
  fly off; the board is empty for about 0.5 s before the confetti. Timing not tuned by eye.
- The ring is small (42 percent of the key) and pale; whether it reads as a to-do mark is for the user.
- Buying a tool inside a trial level spends coins from the real profile (same as the removed Dive pilot); the
  board itself plays on a copy and nothing else is saved.
- The status pill's star budget, the level card and the hint score heuristic are unchanged (the heuristic still
  counts leftover kinds; it only steers hint search order).
- Trial 3's egg sits at the root of a branch; trial 3's ink birds use world 1 art for that species.
