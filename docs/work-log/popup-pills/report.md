# Popup pills: report

Delivered per [spec](spec.md).

| Popup | Before | After |
| --- | --- | --- |
| Win | round Replay, Play pill, round Home | Next pill; Replay and Home pills under it |
| New friend, booster unlock, world complete | round green check | Got it pill |
| How to Play steps 1-3 | round green arrow | Next pill |
| Aviary outfit purchase | round green check | Buy pill |
| Chapters | green check disc per finished row | Replay / Play pill with the stars beside it |

Code: `game/popup.gd` (`Modal.actions`, `Modal.actions_height`), `game/popups.gd` (`win`, `chapters`),
`game/aviary_view.gd`, `game/tutorial.gd` (comment only), labels in `tools/kit/locales/en.json` baked into
`art/locale/en`.

## Verification

- `bash scripts/run_tests.sh`: naming guard 0 violations, smoke 0 failures, core 0 failures.
- Captures checked by eye: `popups:win_three_stars`, `popups:chapters`, `popups:new_friend_berry`,
  `popups:unlock_sapling`, `popups:world_complete_1`, `tutorial:tutorial_1`, `tutorial:tutorial_4`,
  `aviary_view:buy_outfit`.
- UI approval is the user's.
