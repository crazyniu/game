# Board HUD and responsive hints delivery

## Source changes

- `PerchSort/game/board_view.gd`: restored the cream move/star and phase counters after the user rejected the HUD change. The level label below Settings and the dedicated glyph outline shader were removed. Bird scale, perch layout and hint changes are retained.
- `PerchSort/core/solver.gd`: interactive hints now use a best-first heap ranked by flock groups, blockers and route depth. The existing breadth-first `solve()` remains for shortest-path analysis. Hint search uses one 1500 ms / 24000-state budget instead of potentially two 12000 ms searches. These are configured search budgets, not measured response times.
- A successful search returns a state-to-move continuation of a complete winning route. The board caches it for subsequent hints; an unchanged state can replay the last shown hint without spending another booster. No random legal move is substituted for a solution.
- Loading immediately shows a rotating arc inside the existing button. A failed search shows a red retry icon for budget exhaustion or a cross for an exhausted, unsolvable search; stock is retained. The pointing hand now repeats until a board action, restart or exit. Swivel guidance has a visible tapping motion.
- Worker completion is polled on the board. Restart/exit releases the pending task before discarding its result, preventing a previous attempt from applying its hint to a new session. Selected birds settle before committing a hint.
- Added canonical preview states `hint_searching`, `hint_timeout`, `hint_unavailable` and `hint_transfer`; they use a copied profile and fabricated presentation state without invoking search or saving progress.

## Scope and remaining acceptance

The user authorized these code edits despite the general Codex art-only project role. Existing concurrent bird-size/perch-layout and other UI changes were preserved. Rules, levels, save fields and shared UI builders were not changed.

The initial delivery was source-only. The user later authorized testing and requested the HUD rollback. Naming and all-script/40-level smoke checks passed after the rollback; native screenshots and move/phase counter updates were also checked. The [bird-spacing report](../bird-spacing/report.md) records commands and evidence. Hint solve coverage and latency remain unmeasured. A difficult state may still exhaust the hint budget; this produces explicit feedback and consumes no stock. Hint routes are winning routes, not guaranteed shortest routes.

Close the old game window and reopen `./run.sh` to load the source changes. Optional UI-only entry points are `./run.sh --preview=board_view:hint_searching` and the other named states above; these were not executed during this task.
