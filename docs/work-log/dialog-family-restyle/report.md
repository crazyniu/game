# Dialog family restyle: delivery

## Changed
- `family_dialog.gd`: sign title (`sign_title`, `lettering_key`), Close inside the card corner, ButtonKit actions inside
  the card, ButtonKit numerals (`digits`), brown painted ability captions, steps icon on the move line.
- `dialog_cartoon_skin.gd`: home jungle backdrop, the shop's cream card (`cream_card`, no ButtonKit dependency so it
  loads before the Sound autoload).
- `tutorial.gd`: cream stage frame with gold rim, brown captions, green plate actions.
- `popups.gd`: booster unlock caption (`lettering_cap_*`), Lucky Spin and Star Chest sign titles (`PAGE_SIGN`), every
  page action re-dressed as a ButtonKit Primary plate (`_kit_action`), draw board as a cream card with painted slots,
  all prize and reward numerals ButtonKit, level card bird slots `hud_tool_slot`.
- `board_view.gd`: preview state `celebrate`; fuller confetti burst.
- Lettering (Codex, en and zh, 34 images): tutorial, level, new friend, world complete, world unlock, eight lessons,
  lucky spin, star chest. Manifests only gained entries (existing order untouched).
- Tests updated to the new contract: run_family_dialogs, run_dialog_skin, run_level_heading, run_gameplay_dialogs,
  run_chest, run_draw_result, run_draw_purchase.

## Checks
- `bash scripts/run_tests.sh`: naming OK, smoke 0 failures, core 0 failures.
- 0 failures: run_family_dialogs (1780), run_level_heading (3229), run_dialog_skin (192), run_draw_result (353),
  run_reward_quantities (144), run_gameplay_dialogs (72), run_chest, run_purchase_dialogs (98), run_menu_navigation (107),
  run_map_aviary.
- `scripts/screenshots.sh`: all 82 states captured; reviewed as contact sheets (evidence/).
- Board celebration: `--preview=board_view:celebrate --frames --step=0.15` (evidence/board_celebration_frames.jpg).

## Not changed / open
- Pre-existing failures, identical on the committed HEAD 951519b4: run_home (`HomeView.NAV_SIZE` missing) and
  run_draw_purchase coin preview title (`coins.preview`, 24 checks), both in code owned by the shop session.
- Settings (user-approved simple card), credits document, aviary pages (shop session) left as they are.
