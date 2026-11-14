# Rescue dismissal report

## Changes

The owner asked to remove the rescue modal's redundant X after the forest-perch revision. The old illustration change already merged as PR #296, so this work starts from the updated main branch and changes only this dialog's dismissal.

- No visible X; accepted title, bird/perch artwork, frame, button size and bilingual painted Free Rewind / 免费回退 wording stay unchanged.
- Click/touch the surrounding shade to cancel without rescue. Inside-card clicks do nothing. The green action invokes the existing free rewind.
- The shade still stops input; it does not send clicks to the board. Existing Modal closing guard ensures repeated taps cannot dispatch twice.
- No global modal navigation, game rules, rewards, saves, tools or other screens changed. The project had no general Escape/system-back handler here, so none was removed or claimed as implemented.

## Tests

- Single card actions: 476 checks, 0 failures. Both locales; no X, unchanged button bounds and centered word image; inside-card taps do not dismiss; real mouse shade taps and repeated taps close once; underlying test button never activates; touchscreen shade input cancels without rescue; real pointer action edges and real touchscreen action input invoke rescue once; mock callback/dismissal leaves profile unchanged.
- Stuck rescue: 40 checks, 0 failures. Actual free-rewind state restoration and existing rescue search cases; repeated action dispatches once.
- `scripts/run_tests.sh`: exit 0; naming/painted-word guards, launcher recovery, smoke, core, save recovery UI, bird animation and 94 goal checks passed. Rejected goal fixtures intentionally log invalid-species errors.
- `git diff --check`: clean.
- Actual cloud OpenGL bilingual screenshots: `build/rescue-dismissal/normal-zh.png`, `normal-en.png`; inspected no X and unchanged title/forest perch/accepted button. Four-second native-motion sequence remains in `build/rescue-dismissal/motion/`.
- The UI runner retains the existing 2 ObjectDB instances / 1 resource shutdown warning; not claimed leak-free.
- Physical phone/system-back behavior is not separately device-tested. Mouse and touch cancellation/action tests route actual input events through the viewport.

## Manual acceptance steps

1. Open Stuck / 卡住了 in either language. Check no X, compact perch picture, clear title and single green action.
2. Tap the card interior: it remains open. Tap outside the card: it closes without rewinding or moving birds.
3. Reopen and tap Free Rewind: return to the latest solvable position; this is not necessarily one move and does not spend undo stock.
4. Rapidly repeat the button or outside tap: only one action occurs. Confirm the board underneath never receives the cancelling tap.

No merge or deployment performed by this worker. Captures/logs are ignored local evidence, not committed assets.
