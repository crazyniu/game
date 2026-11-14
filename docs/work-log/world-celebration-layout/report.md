# World celebration verification

## Changes
- World completion is the reported 500-feather screen; ordinary victory and the subsequent world unlock are distinct steps in main's existing queue.
- Completion now uses the accepted 880-wide result card with three birds perched on its rim. It no longer places the center bird over unsupported empty arch space. The original reward value, three stars and single confirmation callback are unchanged.
- The same perched-bird helper now supplies ordinary victory and world completion, preserving ordinary victory's positions, sprite identities and flight-in timing exactly.
- World unlock keeps every source image and each world's backdrop. Removed the outward sliding translations. Both leaves use cosine projection around their original outer hinges, remain positive-scale (no reflection), start flush at x512 and end at 82 degrees. Original 0.5-second hold and 1.1-second opening are unchanged.
- No changes to world unlock rules, reward accounting, purchase or star-chest behavior; based on main including #294.

## Evidence
- Twelve actual desktop OpenGL static captures: completion worlds 1/4/10 and unlock worlds 2/4/10, Chinese and English.
- New world-celebration test covers all ten completion worlds in both locales, exactly-once confirmation, supported bird feet, 500 reward, and animated gates in worlds 1/4/10.
- Closed seam, fixed outer hinges, symmetric positive scales and monotonic opening are checked during motion, not just at a final screenshot.
- Core suite exit 0.
- Runtime images/logs remain under ignored `build/world-completion`.

## Validation results
- Actual desktop OpenGL: `run_world_celebrations.gd`: 246 checks, 0 failures; `run_family_dialogs.gd`: 1330/0; `run_jungle_dialogs.gd`: 531/0; `run_reward_dialog_spacing.gd`: 154/0.
- `run_reward_motion.tscn`: first desktop run 110 checks, 1 failure at the pre-existing daily-claim close boundary (last 0.12 seconds); an isolated unchanged rerun completed 110/0. Both logs retained. No test threshold or claim behavior was changed to mask this timing-sensitive result.
- `bash scripts/run_tests.sh`: exit 0. Naming self-tests: 5 pass; repository naming check: 0 violations. `git diff --check`: clean.
- Inspected first, intermediate and final world 4 gate frames: closed leaves meet, knobs remain inward, outer hinges remain fixed, and the scene is revealed continuously. Representative completed-world Chinese/English and unlock screenshots inspected at actual rendered scale.
- Desktop software OpenGL was tested, not mobile hardware. This change does not claim new audio or device-performance validation. Existing resource-at-exit warning in the spacing test remains; assertions and process exit were successful.
