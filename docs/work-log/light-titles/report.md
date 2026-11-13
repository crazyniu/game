# Light titles: report

- `game/button_kit.gd`: `sign()` draws the banner in the lower part of the title box (`BANNER_TOP`), lettering in
  `SIGN_FACE`; `banner()` is the plate alone. Used by purchase dialogs, game dialogs (`FamilyDialog.sign_title`),
  win, lucky spin, star chest, map world page and coin shop.
- `game/shop_view.gd`, `game/aviary_view.gd` (banner centred on the header line), `game/map_view.gd` (card name),
  `game/coin_shop_view.gd`, `game/board_view.gd` (`_hud_pill` uses the capsule), `game/popups.gd` (`WIN_PERCH`: birds
  stand on the banner).
- Tests: `scripts/run_tests.sh` 0 failures; run_family_dialogs (perched frames keep their feet above the lettering;
  flight frames excluded as they only show in the air), run_aviary_text, run_level_heading, run_dialog_skin,
  run_undo_allowance, run_shop, run_chest, run_gameplay_dialogs, run_purchase_dialogs: 0 failures.
- Pre-existing failures, identical on the unmodified head: run_draw_purchase (48, coin purchase simulation checks),
  run_draw_result (1, animated roll), run_purchase_close (double pointer).
- Evidence: `evidence/runtime_all_pages.jpg` (12 pages, English and Chinese).

## Board HUD follow-up

- User feedback: the status capsule was too long (it touched the house) and its content sat off-centre; the undo slot
  did not match the booster slots.
- The capsule now hugs its content (`_status_width`: three stars, footprint, room for two digits) and ends at least
  `STATUS_HOUSE_CLEAR` (48 px) left of the house; content centres on the capsule body below the leaf
  (`CAPSULE_BODY`, `_capsule_mid`), not on the image box.
- Undo uses a painted icon in the booster style (`island_icon_undo`), so all four slots share size, frame and style.
- `tests/run_board_hud.gd` checks all of this in every world (198 checks, 0 failures).
- Evidence: `evidence/board_hud.jpg`.
