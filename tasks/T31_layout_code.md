# T31-C v5 layout rework (code)

Branch `t31-v5-layout`, worktree `/home/developer/Projects/game`. No Unity here; Mac verifies. Authority: `docs/GAME_DESIGN_V5.md` section 3 (v5.1 layout, rewritten for this task), read it in full. Screenshot problems being fixed: buttons of mixed size/style, numbers and icons crammed under the birdhouse, floating birdhouse, board crammed in the upper half.

Ownership: `PerchSort/Assets/Scripts/V5/**`, `PerchSort/Assets/Editor/V5/**`, `PerchSort/Assets/Tests/V5/**`, `tasks/T30_code_report.md` (append a T31 section). No other files. No commit.

Do exactly:
1. Implement every number in section 3 as named constants in one `V5Layout` static class (single source) and have HUD, board, win panel and settings panel read from it.
2. One button family: `btn_square_wood` 150 for tools/settings/close/replay/home, `btn_square_green` 200 only for Next. Remove the round purple settings base from use. Disabled = same button, alpha 0.45, scale 0.9, not clickable (applies to Undo with no history, Add Branch after use).
3. Birdhouse hangs from `limb_top` (load `Resources/V5/Art/limb_top`; if missing, draw a placeholder brown bar with a hook so layout still works). Rope loop top at (540,235). Add the +-2 degree sway with LitMotion pivoting at the hook; cancel it on destroy.
4. Replace the old level pill and loose goal dots with the single flock tracker pill (level badge + one icon per species, dimmed until home, small check when home) exactly as specified; icon sprites reuse `bird_{id}_idle`.
5. Board rows centered in the band (row pitch rule), bird slots unchanged.
6. Win panel and settings panel placements as specified; Next is the only green button.
7. Tests: add EditMode tests for V5Layout invariants (min 40 px gap between every pair of HUD rects at 1080x1920 for the listed elements, bottom bar inside the safe area, 2-row board centered at y=1160, panel button gaps). Keep all existing V5 tests passing (adapt only tests that asserted old positions, and say which).
Report: what changed per element, tests added, anything not verifiable without Unity. `worker_done` when finished.
