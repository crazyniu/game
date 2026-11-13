# Dive pilot — report

## Changes

- `core/board.gd`: `dive_mask`, `dive_target(from, to)`. `core/rules.gd`: `can_transfer` accepts a dive,
  `transfer` inserts the run under the tip bird and names it in the event (`passed`); chicks do not follow a dive.
  `core/levels.gd`: the `talents` level field, `Levels.lab(number)`.
- `game/board_view.gd`: talent badge on Dive birds, glow on dive-only targets while a Dive run is selected, the
  passed bird hops outwards before the divers land; preview state `dive`.
- `game/main.gd`: `open_lab` and the `--lab` argument. `run.sh`, `run.ps1`: documented.
- `levels/lab.json`: three levels. `art/talent_dive.png`: the badge (painted by Codex from `gift_coin_wake`).
- `tools/levels/dive_lab.gd`: deals and accepts lab levels. `tests/run_dive.gd`, added to `scripts/run_tests.sh`.

## Lab levels

| Lab | Species | Branches | Shortest solution | Dives on it | Without the talent |
| --- | --- | --- | --- | --- | --- |
| 1 | 3 | 4 | 5 moves | 1 | cannot be won |
| 2 | 4 | 5 | 11 moves | 2 | cannot be won |
| 3 | 5 (two sleepers, one egg) | 6 | 14 moves | 2 | cannot be won |

No lab level has a losing move beside its first three solution steps.

## Deviations from the gameplay-depth spec

- Lab 2 and 3 have two dives on their shortest solution, not three, and one branch fewer than planned. Random deals
  with three dives on a shortest route and a clearly longer talent-free route were not found in about an hour of
  search; tighter boards where the talent is required were found at once. Lab 3 is 14 moves (planned 16–22).
- The dead-end notice and free rewind were delivered separately ([stuck rescue](../stuck-rescue/report.md)) and
  also work on lab levels.

## Evidence

- `bash scripts/run_tests.sh`: naming guard 0 violations, smoke 0 failures, core 0 failures, dive 0 failures.
- Stills after the entrance: [lab_1](evidence/lab_1.png) (hand on the opening move), [lab_2](evidence/lab_2.png),
  [lab_3](evidence/lab_3.png).
- Timed sequence, 0.25 s per frame: selection with the glow on the dive target, the passed bird hopping outwards,
  the two divers landing under it: [dive_flow](evidence/dive_flow.png)
  (`--preview=board_view:dive --frames=<dir> --wait=5 --step=0.25`).

## Boundaries

- The Dive bird is the existing blue bird with a badge; no kingfisher art.
- The glow is the only preview; there is no painted arrow or word. Long-press bird cards are not part of the pilot.
- Hints on lab levels come from the live search only (no stored solution).
- Not played by a person; whether the talent is fun is the user's call after playing the three levels.
