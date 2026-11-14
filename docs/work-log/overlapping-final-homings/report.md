# Overlapping final homings: report

- `bash scripts/run_tests.sh`: exit 0 (naming 0 violations, smoke 0, core 0, recovery UI 0, bird animation 0,
  run_goal 94/0); `tests/run_visible_skills.gd` 41/0.
- Frames at 0.1 s (`main.tscn -- --preview=board_view:final_homings --frames=<dir> --wait=5 --step=0.1`): the Sky
  flock takes off about 0.6 s after the Sunny flock, while the Sunny flock is still reaching the doorway; before the
  change it took off after Sunny was inside and the wake badge had landed (about 1.8 s). The door stays open until
  the last flock is in; no badge flies to an empty branch. Evidence not committed.
- Not played on a device; not yet accepted by the owner.
