# Level card and goal information close

## Change

`GameRoot.ClosePanel()` now starts the information-card exit instead of hiding these two cards immediately. `PanelsView` cancels their active presentation tweens, keeps the modal raycast barrier in place and its controls non-interactable, and eases the panel scale from its current scale to 0.92 of that scale while fading the entire modal, including the dim layer, to zero over 0.12 seconds. Completion hides the panel and refreshes input. `GameMotion.Instant` completes synchronously. Opening and other popup dismissal paths remain unchanged. Hiding or disabling the view cancels the motion.

The level-card close button and backdrop route through the same close hook. The goal-info backdrop routes through it as well; the existing doorway return pose runs during the exit.

## Evidence

- Before: [level card](../remaining-evidence/evidence/level_card_close.jpg), [goal information](../remaining-evidence/evidence/goal_info_close.jpg). The supplied sequences show the underlying surface on the first captured frame.
- After: [level card](evidence/level_card_close.jpg), [goal information](evidence/goal_info_close.jpg). These are Unity PlayMode camera renders captured on successive frames with `Time.captureDeltaTime = 0.02` seconds, from 0.02 through 0.20 seconds after the close call. They show the fading modal and dim, followed by the undimmed surface. The capture used nonpersistent fixtures; level card is level 1, goal information uses the level 5 fixture. Frame 0 was omitted because the first render target initialization produced a different viewport.
- Approved visual references: [level card](../../../art/approved_ui/ui_level_card.png), [goal information](../../../art/approved_ui/ui_goal_info.png). The fixtures and artwork/layout do not exactly match those reference compositions; this change does not alter the opening layout or art and does not claim full visual acceptance.

## Verification

- `python3 ci/check_current_game_names.py --self-test`: 9 passed.
- `python3 ci/check_current_game_names.py`: 179 tracked text files inspected, zero violations.
- Serialized targeted PlayMode run with a graphics device and `POPUP_EXIT_CAPTURE` enabled: 2 passed, zero failed or skipped. It captured the contact-sheet frames and asserted intermediate scale/opacity, the modal input barrier, deactivation and instant close. The temporary capture helper was removed after collecting evidence.
- `flock /tmp/unity.lock bash scripts/screenshots.sh`: passed; stable 1080 × 1920 captures in `build/screens/game/` (instant motion, synthetic profiles).
- `flock /tmp/unity.lock bash scripts/run_tests.sh`: EditMode 317/317, PlayMode 89/89 passed; zero failed or skipped. Results: `build/test-results.xml`, `build/playmode-results.xml`.
- Unity regenerated resource metadata and the scene during verification; their pre-restoration changes were saved separately in ignored `build/unity-generated-changes.patch` and were not included in the branch.

No physical-device run or interactive GUI input acceptance was performed. Coordinator integration and approved-reference visual acceptance remain required. No contract changes requested.
