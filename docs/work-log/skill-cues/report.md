# Skill cues report

Changes: `game/board_view.gd` (section "skill cues" beside the bird keys, a call in `_refresh_status`, `_finish_action`,
`_process`, `_input`, preview states `light_step`, `free_move`), `tests/run_visible_skills.gd` (extended).

Tests (`run_visible_skills`, 38 checks, 0 failures): no cues before a gift; glows exist exactly on the branches with a free
seat while `light_step` is set and Flurry's key pulses; none while a move plays; none once the step is used; the free badge
shows exactly while `encore` is set and goes with the move; the pointer waits, shows once, is remembered, stops on a tap and
does not return.

Frame sequences (`--frames=<dir> --preview=board_view:light_step|free_move --wait=9 --step=0.5`, frames not tracked): the
flock flies home at about 2 s; the glows and the pulsing key (light step) and the badge on the capsule (free move) appear
once the gift has played, at about 5 s, and stay until a move is made.

Boundaries: not played on a device. Two clearances were fixed from the first frames (the badge covered the move count, the
glow was too faint on a bright sky). The user accepts the UI.
