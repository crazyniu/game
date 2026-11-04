# Interface art wiring report

## Changes and evidence

- HUD booster discs use the six supplied coloured tool glyphs at 70% of the disc rect. Undo, stock badges and shop illustrations retain their prior handling.
- Win star rects are 160 × 160; level-card stars retain their 110 × 108 rects. The sleeping mechanic uses the bare owl sprite from the Art resource root.
- The requested separate sprig images are present on both panels. See the limitations below.
- Stable-state Unity render captures are in `evidence/tools-three.png`, `evidence/tools-four.png`, `evidence/win.png`, and `evidence/level-card-sleep.png`. The sleeping-card capture used a temporary capture-only override in the owned panel file to display the authored sleeping level through the existing capture entry point; that override was removed. These are synthetic nonpersistent captures, not timed animation evidence. Visual comparison shows the win still differs from the approved reference, including overlapping leaves and star appearance.

## Verification

- `python3 ci/check_current_game_names.py --self-test`: 8 passed. `python3 ci/check_current_game_names.py`: no violations.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`: scene build succeeded; EditMode 317/317 passed, no skips; PlayMode 69/71 passed, 2 failed, no skips. The new interface art test passed. The failures are `GameplayTests.SaplingIntroPersistsOnShowAndBranchTapDismissesWithoutSpending` (expected tutorial hand position `(720, -1802)`, actual `(720, -1764.8)`) and `IllegalMoveFeedbackTests.MissingTurnSequencesUseExistingBlinkAndInstantSettlesImmediately` (expected missing `mint_turn_left_01`, but that sprite exists). Those test and behavior files are outside ownership; no existing tests were weakened. No pre-change baseline run was made, so the failures are not proven pre-existing by this run.
- `flock /tmp/unity.lock bash scripts/screenshots.sh`: passed, including 1080 × 1920 image checks. A second serialized run with `PERCH_CAPTURE_SLEEP=1` produced the sleeping-card evidence using the temporary override described above.

## Unmet or conflicting acceptance criteria

- `panel_wood.png` already contains leaves in its upper-left and bottom-right corners. The supplied frame is outside the owned files and artwork changes were not authorized. Adding the required sprigs therefore leaves visible duplicated decoration on the win panel; the image-count test only counts separate sprig images, not baked pixels. A leaf-free frame asset is needed for no-duplication acceptance.
- The spec simultaneously requires 160 px win stars and unchanged pre-change rect sizes. The actual prior rects were 168 × 174 for the sides and 192 × 190 for the center. The explicit 160 px requirement was used; unchanged pre-change win rects cannot also be satisfied.
- The requested new PlayMode test increases discovery; unchanged test counts and adding that test are incompatible. The observed totals also differ from the historical counts in the repository guide. Full-suite green and exact approved-reference visual acceptance were not achieved.
