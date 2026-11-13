# Win panel actions: report

- `game/popups.gd` `win()`: Next on its own row; `_win_pill` builds Replay and Home with `ButtonKit.word`.
- `art/locale/{en,zh}/island_words/white_action_replay.png` registered under `painted` in both manifests.
- `tests/run_family_dialogs.gd`: Next at most 1.25 times Replay in both dimensions, Replay and Home the same size on
  one row under Next, both inside the card. 1756 checks, 0 failures; `scripts/run_tests.sh` and
  `run_reward_quantities` pass.
- Evidence: `evidence/win_en_zh.jpg`, `evidence/actions_en_zh.jpg` (preview `popups:win_three_stars`).
