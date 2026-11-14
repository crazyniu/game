# Star chest opening: report

- `scripts/run_tests.sh`: exit 0. `tests/run_chest.gd` (xvfb, OpenGL): 0 failures; two waits now follow the new
  timing (the shake comes before the pop sound; the mid-swing lid check is taken early in the swing, before the 4
  degree overshoot).
- Frames at 0.1 s (`main.tscn -- --preview=popups:chest_opening --frames=<dir> --wait=5 --step=0.1`): the chest has
  the same size and feet position in every frame, before and after the page change; shake, squash, pop with flash and
  star fountain; the three prizes arc above the lid and land in the row in turn; amounts count up to +300, +2, +1.
- Not played on a device; sounds unchanged (crate_open at the pop, star_1 as the lid settles, reward per prize).

## Follow-up

Lifted 120 px: `scripts/run_tests.sh` exit 0, `run_chest.gd` 0 failures; stills of `chest_half` and the settled
`chest_opening` reward page checked (count clear of the title sign, Play ends at y 1760, OK at 1400..1608).
