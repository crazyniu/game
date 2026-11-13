# Dialog consistency verification

## Result

Seven non-gameplay dialog families now reuse the existing Home/Chest forest components: win, level card, challenge rewards, new friend, booster unlock, world complete and world unlock. This is a locally verified correction for visual review, not a replacement of any user-approved baseline.

- Replaced the old flowered title plank with transparent painted gold headings.
- Reused the exact Home/Chest button face. Primary actions are 520 × 176; secondary actions are 416 × 140.8. Disabled buttons retain their size. Play/OK use the existing accepted word sprites. Next retains its existing painted word. Replay/Home use the established white control symbols and retain their callbacks.
- Reused the Home/Chest jungle background and existing green bamboo-rim panel. No artwork files were edited, generated, or replaced.
- Rating stars now have equal 180 × 180 visible bounds, using the same filled/hollow source images, including across states.
- Numeric rewards and move counts use the Home numeral family. Feather rewards remain unboxed icon plus amount.
- Centered the new-bird ability badge and caption by visible ink. Live language changes retain the proper heading and level number.
- Reused the existing soft glow inside the world doorway, removing the previous hard rectangular clipping edge. Door motion and live birds remain.

## Checks on the final source

Engine: Godot 4.7.2, GL Compatibility. Graphics captures use the actual Mesa llvmpipe OpenGL renderer on the authorized cloud desktop.

- `godot --headless --path PerchSort -s res://tests/run_family_dialogs.gd`: **710 checks, 0 failures**. Includes both languages, every booster, empty/multiple/full challenge rewards, maximum level/full roster, disabled actions, locale switches, repeated actions, disposal, interrupted celebration and unchanged profile data.
- `bash scripts/run_tests.sh`: naming self-tests 5/5, naming guard 0 violations, painted-word check 0 problems, smoke 0 failures, and the core solution replay for all 100 levels 0 failures.
- `git diff --check`: clean.
- 36 unowned popup functions unchanged, including Settings, goal info, draw and chest. Shared Modal, gameplay, rules, profile data and all artwork bytes unchanged.

## Actual runtime evidence

- **42 raw PNGs**, each 1080 × 1920: 21 states in English and Chinese, including all six booster descriptions. Every capture was inspected, with final-state contact sheets and seven before/after actual-render comparisons.
- **48 distinct motion frames**: 16 each for win, new friend and world unlock. They show star pop/count-up, fly-in and continuing bird poses, and the opening door/soft glow. Requested timer intervals are 0.25 s, but synchronous PNG saving under the software renderer increases wall-clock intervals to about 1.2–1.3 s. This is proof of changing real rendered states, not acceptance of device performance or prescribed animation timing.
- Actual pointer clicks: disabled Play remained inert; Close did not invoke Play; Home, Replay, Next and world Play each dispatched the expected callback exactly once. Only the test window closed, with profile balance unchanged.

Raw images: `build/dialog-consistency/screens/`. Motion frames: `build/dialog-consistency/motion/`. Logs, hashes, contact sheets and before/after comparisons are under this report's `evidence/` directory.

The capture/test profiles are isolated fake data and do not save. The only graphics warning was unsupported VSync control. Physical-device touch, audio listening, performance budgets and full accessibility assessment were not performed. User visual approval remains pending. No code was uploaded, pushed, merged or deployed.
