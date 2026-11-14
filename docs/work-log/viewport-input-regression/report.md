# Viewport-local inventory test verification

Base: main `eec1de2`. Official Godot 4.7.2, actual cloud X11/OpenGL Compatibility, Mesa llvmpipe. Isolated fake profiles, no real player data. Desktop tests launched through the cloud terminal. No physical-device acceptance.

## Reproduction and correction

Unmodified `run_tool_inventory_gui` failed only `zero-stock tool emits its one-more request` (exit1, 5.45s). Diagnostic output: logical viewport1080x1920, desktop window1180x812, tool rectangle(328,1668,136,136), enabled, not busy, SAPLING inventory0. Neither button_down nor pressed fired. The script passed a logical viewport point into global `Input.parse_input_event`, which applies window scaling. Replacing only the two injections with `root.push_input(event, true)` preserves actual pointer dispatch and every original assertion. The subsequent production offer/cancel checks already passed before the correction.

## Actual GUI results

| Script | Result | Duration |
| --- | --- | --- |
| run_tool_inventory_gui baseline | FAIL, one coordinate assertion | 5.45s |
| run_tool_inventory_gui repaired repeat1 | PASS, exit0 | 6.56s |
| run_tool_inventory_gui repaired repeat2 | PASS, exit0 | 5.50s |
| run_tool_inventory_gui repaired repeat3 | PASS, exit0 | 5.50s |
| run_aviary_actions unchanged | PASS, 1203 checks | 7.52s |
| run_map_nodes unchanged | PASS geometry/motion, 3606 checks; visual warning below | 5.63s |
| run_purchase_close unchanged | PASS, 2892 checks | 18.88s |
| run_reward_motion unchanged | PASS, 110 checks | 39.95s |

The unchanged level-heading test passed three GUI runs, each7861 checks/4s, plus three headless runs. The old overlap was already corrected by #282; its strict assertion remains unchanged. Reward-motion's 3600ms deadline remains unchanged. This quiet-period success does not erase the prior13.049s concurrent-rendering stall or establish its root cause.

Map-node test emits `VISUAL REVIEW REQUIRED` for an existing unapproved Glow. Geometry PASS is not visual acceptance. This patch does not alter the map. All GUI runs warn that this driver's V-Sync configuration cannot be changed. The headless inventory diagnostic reaches and passes the corrected click/cancellation checks but waits at frame_post_draw screenshots; its timeout is not counted as a full pass.

## Evidence and remaining scope

Ignored evidence under `build/heading`: `focused/results.json`, `fixed/results.json`, per-script logs, `inventory-diagnostic-gui.log`, `gui-1.log` through `gui-3.log`, and `inventory.json`. Inventory screenshots are written by the existing test under `build/board-recovery`. No evidence images are tracked.

Current inventory is56 run/check scripts, versus49 in the previous full report. Newly added: run_catalog_measure, run_chest_lid_attachment, run_draw_texture_sampling, run_level_card_action, run_reward_dialog_spacing, run_single_card_actions, run_world_celebrations. Every Node script has its scene. This focused pass does not rerun the other50 scripts; chest-specific tests are intentionally excluded. No full clean-main56/56 claim, no #254 closure. Aggregate script scope unchanged.

## Aggregate verification

`GODOT=<official4.7.2> bash scripts/run_tests.sh` with isolated XDG paths passed (exit0): naming guards, painted-word policy, launcher recovery, script/scene smoke, all210 stored level solutions, save recovery UI, bird-animation and94 goal checks. Deliberately invalid save/goal fixtures emit expected error diagnostics. Final `git diff --check` passed. Remote main remained `eec1de2` and no open PR targeted these files at submission preflight. This isolated test-only change does not touch the unresolved user-local catalog/tool work.
