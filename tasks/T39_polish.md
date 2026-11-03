# T39 polish pass (omp + Codex)

Branch `t39-polish`, worktree `/home/developer/Projects/game`. No Unity here; the coordinator tests on the Mac (it already ran the merged build: EditMode 358/358, PlayMode 23/0 failed). Authority `docs/GAME_DESIGN_V5.md` v5.3 (§3 layout rules still apply: one wood button family, 40 px gaps). Issue #31. Items found in the Mac screenshots:

1. **Song pill reads as an empty pill.** Always show 5 note slots: empty = dim outlined note, filled = gold note; a filled note pops (scale 1.3 -> 1, 0.2 s). Add a small count label only when the meter holds more than 5 notes. Needs art `note_empty.png` and `note_full.png` (see Codex task below); until they exist draw them in code (fallback).
2. **Gift icons on the birdhouse signs are too small.** Raise the sign to 72x72 with the icon filling it; keep 40 px clearance to neighbouring elements (re-check the K=7 case: houses may need a smaller width, document it) and update the layout tests.
3. **Win panel shows a stray Undo button under the dim.** Hide the whole bottom bar and the song pill while the win panel is up; the dim overlay must sort above every HUD element. Add a PlayMode assertion that no bottom-bar button is raycast-reachable while the panel is open.
4. **Lower half of the board looks empty on few-row levels.** Row pitch = min(380, band / rowCount) (was 300) and bird scale cap 1.12 when rowCount <= 3 (keep clamp 0.72..1 for rowCount >= 4); rows stay centered at y=1160. Re-verify the spacing tests and that 5-row levels still fit (levels 22, 26, 37, 40).
5. **L4 Sapling tutorial hand** (hand-only, no text): the first time level 4 is shown and the player owns a Sapling, the tutorial hand points at the Sapling button once; tapping any board branch or the button dismisses it; stored under the tutorial flags.

Ownership: `PerchSort/Assets/Scripts/V5/Game/**`, `PerchSort/Assets/Tests/V5/**` (adapt tests that assert changed geometry, list which), `tasks/T39_report.md`. Do not touch `levels.json`, `tools/`, Core rules or art files. No commit. Status after items 1-3 and at the end; `worker_done` with what is untested without Unity.
