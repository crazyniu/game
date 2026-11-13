# Home hanging title — delivery

- Art: generated with local Codex image generation from the previous title and the jungle background as style
  references (prompts beside this report), cropped to their visible area and scaled offline (sign 0.72, branch 0.8).
- `PerchSort/game/home_view.gd`: sign at (94,-30), branch at (-74,760), perch line y=950, bird x 330/500/665/830,
  event column from y=1230. `panel_home_perch_title.png` is no longer referenced.
- Checks: `bash scripts/run_tests.sh` (naming guard OK, smoke 0 failures, core 0 failures);
  `scripts/capture.sh --preview=home_view:home` in en and zh; 3 s motion frames at 0.25 s step: feet stay on the
  branch in every pose. Evidence in `evidence/`.
- Not done: user approval of the new look; old title file and its docs references left in place until approved.
