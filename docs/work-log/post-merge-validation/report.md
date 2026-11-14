# Post-merge regression evidence

Baseline: main `2bcc7969f1181d91dd368c6d6f12d6508973b9af`. The #277 and #279 changes are both present. No product code changed.

## Reproducible harness defects fixed

1. The sweep's summary regex read `AUX_DIALOGS checks=10 failures=0` as ten failures. It now distinguishes count-before-label summaries from `failures=N`, and has seven positive/negative regression examples in `tests/test_sweep_gui.py`.
2. `run_save_recovery_ui` correctly refuses non-isolated data, but the generic sweep did not provide its required marker. The runner already creates a dedicated per-test XDG_DATA_HOME; only for this test it now provides the same path as PERCH_RECOVERY_TEST and removes any inherited Exit-only mode. It does not disable the test's path guard.
3. Added explicit malformed primary without a backup, application re-instantiation with those same damaged bytes, and exact primary preservation/no invented backup assertions. Existing both-bad, bad-backup-only, Retry, valid backup, first launch and valid primary assertions remain.

4. `run_draw_purchase` used a fixed 0.2-second delay after Close. It now awaits actual modal destruction with a two-second watchdog and records elapsed time separately from the intended 120 ms tween. Three focused runs passed; the watchdog is not a performance acceptance threshold.
5. `run_family_dialogs`, `run_gameplay_dialogs`, `run_level_heading`, `run_stuck_rescue`, and `run_undo_allowance` still asserted superseded cream-panel geometry after #277: interior actions, `Panel/Skin`, and extra purchase-title padding. They now check the approved frame, exterior action spacing, and actual frame bounds. Callback, localization, price, inventory and placement checks remain. The original family null-node error reached the sweep's 300-second timeout; its corrected run passed 1,330 checks.

## Environment

Official Godot 4.7.2 standard Linux binary downloaded with GitHub CLI from `godotengine/godot-builds`, tag `4.7.2-stable`; archive SHA512 verified against that release's SHA512-SUMS.txt. Actual version: `4.7.2.stable.official.ed1daf0bf`. Xvfb/X11, OpenGL Compatibility, software Mesa, 1080x1920. This replaces the earlier 4.6.3-only acceptance boundary; it does not replace physical-device checks.

The initial older-engine partial sweep was explicitly superseded after obtaining the target engine. It is not counted as a completed run. Evidence stays in ignored `build/issue-review` and is not shipped.

## Commands

- `python3 PerchSort/tests/test_sweep_gui.py`: seven cases, passed.
- `python3 PerchSort/tests/sweep_gui.py --godot <verified-engine> --output build/issue-review/target-sweep` on a dedicated graphical display; 300-second per-test limit.
- Same runner with `--repeat 3 ... run_aviary_actions run_map_nodes run_purchase_close run_tool_inventory_gui run_save_recovery_ui`.
- `GODOT=<verified-engine> bash scripts/run_tests.sh` with separate XDG data/config paths: exit 0; 210 stored solutions and 94 goal checks passed.
- `run_save_recovery_ui.gd` with dedicated XDG data/config, matching PERCH_RECOVERY_TEST and PERCH_RECOVERY_EXIT=1: Exit dispatch terminated the process with exit 0. Expected abrupt-exit resource cleanup warnings are retained in the log; no script assertion failed.

## Repeated checks and timing

All durations below are seconds on the target engine; each row passed all three independent isolated launches.

| Test | Run 1 | Run 2 | Run 3 |
| --- | ---: | ---: | ---: |
| run_aviary_actions | 7.16 | 7.20 | 7.82 |
| run_map_nodes | 6.56 | 6.96 | 6.33 |
| run_purchase_close | 15.94 | 17.31 | 16.04 |
| run_tool_inventory_gui | 6.87 | 7.74 | 6.59 |
| run_save_recovery_ui | 3.10 | 3.20 | 2.89 |
| run_draw_purchase (corrected) | 8.66 | 8.39 | 8.10 |

The 180 purchase-close measurements have median 121.096 ms and maximum 232.153 ms. Draw-close completion measured 104.109, 56.014 and 105.891 ms. These are event-to-destruction observations on software rendering (not frame-perfect animation duration); the two-second safety timeout was never approached. Device frame rate and audible sound remain unverified.

Final unchanged aggregate suite returned 0, including 210 stored solutions and 94 goal checks. Expected deliberately-invalid level diagnostics are retained in the log.

## Issue boundaries

#254 concerns reliable automated harnesses; the full before/after report already landed with #279, and this report verifies the merged/current inventory on the target engine. #272's desktop recovery path is independently exercised, including preventing normal boot and preserving corrupted bytes; the issue also requests real target-device verification, which has not been performed here. Do not claim device acceptance or reconstructability of damaged data. #247 remains user-owned local work; its downstream catalog tasks are not altered. Real ads/payments still need production inputs, and the trial still needs the user's verdict.

## Static verification

Naming guard self-tests (5) and repository scan passed, with 174 tracked source files inspected and zero violations. `git diff --check` passed. No product script, shipped asset, CI workflow, or aggregate test scope was changed.

## Complete before/after standalone sweep

All 49 current standalone scripts ran in each sweep with a 300 s per-script timeout. Initial: 43 passed, 6 failed. Corrected: 48 passed, 1 timing failure. This table uses the full second sweep, not substituted focused results. The baseline includes the parser/isolation fixes and added recovery assertions; the subsequent six failures are preserved verbatim in ignored logs.

| Script | Initial result / seconds | Corrected result / seconds | Initial failure |
| --- | --- | --- | --- |
| `check_family_pointer.gd` | PASS / 4.69 | PASS / 4.33 | — |
| `check_map_bird_clearance.gd` | PASS / 2.54 | PASS / 2.47 | — |
| `check_map_hit_regions.gd` | PASS / 14.70 | PASS / 13.05 | — |
| `run_aux_dialogs.gd` | PASS / 2.09 | PASS / 1.68 | — |
| `run_aviary_actions.gd` | PASS / 9.14 | PASS / 6.51 | — |
| `run_aviary_text.gd` | PASS / 6.90 | PASS / 5.90 | — |
| `run_bird_animation.gd` | PASS / 5.32 | PASS / 5.42 | — |
| `run_board_hud.gd` | PASS / 4.75 | PASS / 4.18 | — |
| `run_board_shop_refresh.gd` | PASS / 10.76 | PASS / 10.14 | — |
| `run_chest.gd` | PASS / 30.58 | PASS / 29.86 | — |
| `run_core.gd` | PASS / 1.18 | PASS / 1.12 | — |
| `run_daily_reward_flow.gd` | PASS / 4.47 | PASS / 4.40 | — |
| `run_dialog_skin.gd` | PASS / 1.13 | PASS / 1.27 | — |
| `run_draw_purchase.gd` | FAIL / 7.90 | PASS / 8.46 | FAIL draw closes without leftover overlay |
| `run_draw_result.gd` | PASS / 12.11 | PASS / 11.64 | — |
| `run_family_dialogs.gd` | FAIL / 300.02 | PASS / 11.13 | FAIL the win stays a popup on the cream card |
| `run_feather_prices.gd` | PASS / 1.48 | PASS / 1.28 | — |
| `run_gameplay_dialogs.gd` | FAIL / 7.88 | PASS / 7.49 | FAIL action inside the card, as in the shop's popups |
| `run_goal.gd` | PASS / 4.33 | PASS / 4.63 | — |
| `run_guide_dialogs.gd` | PASS / 10.66 | PASS / 11.28 | — |
| `run_heading_composites.gd` | PASS / 1.52 | PASS / 1.68 | — |
| `run_home.gd` | PASS / 25.77 | PASS / 25.73 | — |
| `run_jungle_dialogs.gd` | PASS / 11.92 | PASS / 10.64 | — |
| `run_level_card_tools.gd` | PASS / 4.24 | PASS / 4.19 | — |
| `run_level_heading.gd` | FAIL / 4.87 | PASS / 4.75 | FAIL painted level card reserves title room |
| `run_live_feather_balances.gd` | PASS / 10.62 | PASS / 10.96 | — |
| `run_map_aviary.gd` | PASS / 23.47 | PASS / 23.24 | — |
| `run_map_card_clip.gd` | PASS / 2.49 | PASS / 2.43 | — |
| `run_map_nodes.gd` | PASS / 6.26 | PASS / 6.17 | — |
| `run_menu_navigation.gd` | PASS / 6.11 | PASS / 6.37 | — |
| `run_modal_motion.gd` | PASS / 45.87 | PASS / 46.02 | — |
| `run_purchase_buttons.gd` | PASS / 9.54 | PASS / 9.23 | — |
| `run_purchase_close.gd` | PASS / 14.39 | PASS / 14.74 | — |
| `run_purchase_dialogs.gd` | PASS / 7.18 | PASS / 6.62 | — |
| `run_purchase_surface.gd` | PASS / 7.18 | PASS / 7.50 | — |
| `run_reward_motion.gd` | PASS / 40.85 | FAIL / 54.98 | — |
| `run_reward_quantities.gd` | PASS / 4.40 | PASS / 5.38 | — |
| `run_save_recovery_ui.gd` | PASS / 2.53 | PASS / 3.05 | — |
| `run_shop.gd` | PASS / 21.61 | PASS / 22.38 | — |
| `run_shop_amounts.gd` | PASS / 10.41 | PASS / 10.59 | — |
| `run_shop_cards.gd` | PASS / 3.49 | PASS / 4.43 | — |
| `run_shop_cards_gui.gd` | PASS / 18.26 | PASS / 21.24 | — |
| `run_stuck_rescue.gd` | FAIL / 6.01 | PASS / 7.21 | FAIL rescue action inside the card |
| `run_tool_guides.gd` | PASS / 18.89 | PASS / 20.33 | — |
| `run_tool_inventory_gui.gd` | PASS / 6.57 | PASS / 7.21 | — |
| `run_undo_allowance.gd` | FAIL / 4.07 | PASS / 4.24 | FAIL actions inside the card |
| `run_unlimited.gd` | PASS / 0.82 | PASS / 1.13 | — |
| `run_visible_skills.gd` | PASS / 5.06 | PASS / 5.67 | — |
| `run_world_reward_dialogs.gd` | PASS / 7.78 | PASS / 9.23 | — |

Logs and JSON: `build/issue-review/target-sweep/` and `build/issue-review/target-after/`; independent focused fixes in `draw-fixed`, `family-fixed`, `gameplay-fixed`, `heading-fixed`, `rescue-fixed`, and `undo-fixed`. Required triplicate runs in `target-repeat/`. The six initial failures were test expectations/waiting against the approved dialog migration. The corrected sweep separately observed an intermittent reward-motion failure under concurrent cloud GUI rendering; see the timing investigation below.

## Reward-animation timing investigation

The second full sweep had one new failure: `run_reward_motion` target 7 reported 13,049 ms before a result, exceeding its unchanged 3,600 ms limit and missing the visible-target observation. The first complete sweep passed this script in 40.85 s. Other agents were rendering batch screenshots on the same cloud host during the second sweep, confirmed by the coordinator; scheduling/render contention is a plausible contributor, not a proven product root cause. The original failure is retained. No timing gate or visual assertion was relaxed, and no product animation was changed. Focused unchanged-script repeats were started, followed by an explicitly coordinated GUI quiet period. These are not a clean full-sweep pass, nor mobile performance certification. #254 should remain open until acceptance/review of this remaining environment-sensitive observation and the follow-up PR; #272 remains open for device checks.

Unmodified reward-motion repetitions subsequently passed **110 checks each**, in **42.03 s, 41.53 s, and 41.23 s**. During the later repetitions the other GUI worker paused by coordination, and the full sweep had ended before the final repetition completed. This establishes successful repeatability but does not erase the single shared-host timing failure or prove its root cause. The original strict timing threshold remains active.
