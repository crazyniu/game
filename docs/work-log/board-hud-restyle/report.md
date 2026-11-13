# Board HUD restyle: delivery

- Code: `PerchSort/game/board_view.gd` (HUD, tools, celebration), `PerchSort/game/popups.gd` (win, booster drops).
- Tests updated to the new win contract: `tests/run_family_dialogs.gd`, `tests/run_reward_quantities.gd`.
- Checks: `bash scripts/run_tests.sh` naming OK, smoke 0 failures, core 0 failures; run_family_dialogs 2072 checks 0
  failures; run_reward_quantities 144 checks 0 failures. Captures in `evidence/`.
- Open: board celebration has no frame evidence yet (autoplay on level 2 stops at the firefly intro popup); the booster
  intro, new friend and other game popups still use the old orange family frame.
