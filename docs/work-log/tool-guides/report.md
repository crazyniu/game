# Tool guides report

Changes: `game/board_view.gd` (guide section beside `point_at_tool`, gates in `tap_branch`, `_tap_swivel`, `_on_undo`,
`_on_tool`, preview state `tool_guide`), `game/main.gd` (popup queue in `_open_board`), `meta/progress.gd`
(`guided_flag`, `guide_unit`), `tests/run_tool_guides.gd`.

Test `run_tool_guides`: for each of the six tools, a fresh profile at its unlock level is guided, spotlight and hand
shown, a unit is in hand, board taps/other tools/undo do nothing, pressing uses exactly one unit and ends the guide and
sets the flag, a second visit does not guide; plus a tool without an effect waits and starts when it has one.
56 checks, 0 failures.

Frame sequence (Sapling, `--preview=board_view:tool_guide`, 0.5 s steps): spotlight fades in over the Sapling slot with
the tapping hand (0.6 s to 3.6 s), the press at 3.6 s, the dimming and hand gone and the new branch grown within 1 s.
Command: `xvfb-run godot --path PerchSort --rendering-driver opengl3 --resolution 1080x1920 -- --frames=<dir>
--preview=board_view:tool_guide --wait=6 --step=0.5`. Frames are not tracked.

Boundaries: not played on a device; the other five tools are covered by the headless test, not by frame sequences.
The user accepts the UI.
