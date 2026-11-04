# T54J Integrated motion evidence (QA only, no product code changes)
Base: branch `t54-integration` (all nine T54 tasks merged, including the drawn bird frame sequences). Read tasks/T54_contract.md and the "Exact action tables" in tasks/T54_animation_addendum.md.
You change NO product file. You may add a TEMPORARY PlayMode capture test and runner in your worktree (do not commit them) and you commit only evidence under `tasks/evidence/T54J/` plus `tasks/T54J_report.md`.
Capture with real Unity PlayMode, normal motion (not Instant), real resources, fixed random seed 12345, serialized with `flock /tmp/unity.lock`. Each sheet is a JPEG, max 1600 px wide, quality 80, every tile labelled with its actual elapsed time.
1. `branch_idle_20s.jpg`: level 1, the complete left branch (4 birds), 81 tiles, 0.25 s apart, 20 s, after the entrance finished. Also `branch_idle_samples.csv` (per bird per tile: current frame name, action, y scale).
2. `branch_idle_zoom_actions.jpg`: crops at 0.1 s spacing of the 4 most visible different actions that occurred (e.g. hop, look, flap, preen) of one bird, 12 tiles each, so frame-by-frame drawing change is visible.
3. `entrance_5s.jpg`: level 1 entrance, 51 tiles at 0.1 s, plus `entrance_5s.csv` with play-enabled time.
4. `win_panel_10s.jpg`: win panel of level 1, 0.0-4.0 s every 0.1 s then 4.5-10 s every 0.5 s, with `win_panel_samples.csv` of side-bird and hero-bird frame names per tile.
5. `houses_delivery_15s.jpg`: level 1 top houses from level start for 3 s (all empty), then through the first delivery for 12 s, 0.25 s spacing.
6. `settings_level_and_map.jpg`: settings opened in level 1 and on the map, 0.05 s spacing for 0.5 s plus the settled state.
Report in `tasks/T54J_report.md`: for each sheet the facts a viewer can verify (what frame names changed, whether any sprite had negative x scale in any sample: expected NEVER), and any problem you observe (stills shaking, popping, floating off the branch). Do not interpret generously; list defects plainly.
Run the full suite once (`flock /tmp/unity.lock bash scripts/run_tests.sh`) after removing your temporary files and report exact counts. Push branch `mac-t54j/t54`, final SHA in last message.
