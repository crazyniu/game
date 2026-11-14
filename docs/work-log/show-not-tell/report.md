# Show, not tell: report

## Tests (Godot 4.7.2)

- `bash scripts/run_tests.sh`: naming 0 violations, smoke 0, core 0, recovery UI 0, bird animation 0 (the wake check
  now waits for the 0.8 s wake), run_goal 94/0.
- `run_visible_skills.gd` 42/0 (new: result cell inside the card, row centred), `run_tool_guides.gd` 56/0,
  `run_family_dialogs.gd` 1330/0.
- `run_stuck_rescue.gd` 40/0 after rebasing on main (the pointer checks read the guide's `Glow` node).
- `run_sleep_cue_placement.gd` removed with the sleep bubble it measured (main #282 had placed that bubble).

## Timed evidence (local, not committed)

Frame sequences via `main.tscn -- --preview=<state> --frames=<dir> --wait=S --step=0.05..0.25`:
- Bird cards for sunny, snow, sky, ember, cocoa, mint, ink: the loop runs (hop, fly in, house bounce, badge arc,
  effect, reset); the sunny result cell shows the bell over the waking sleeper.
- `wake_teach_move`: dimmed board, the firefly hovers over the matching Berry and flies to the sleeper.
- `stuck_guide`: board darkens except the Sapling slot; the firefly flies in from mid-board and circles it.

## Boundaries

Not played on a device; the wake_bell sound not judged by ear. `scripts/audio/analyze.py` was already stale (it
expects 18 files, the folder has 24) and is not changed; the new row was measured and added to `MANIFEST.md` by hand.
Not yet accepted by the owner.
