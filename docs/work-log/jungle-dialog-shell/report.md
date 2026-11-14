# Jungle dialog shell report

## Changes

- New isolated branch `jungle-dialog-shell`, based on main `a14e30a04a25aee2d51cdb1c6df26a08d56f7053`.
- Level-entry and win-result now opt into the approved opaque lime / emerald / honey-wood frame. No global cream-card replacement.
- Existing painted lettering, bird animations, stars, reward values, inventory and callbacks retained. Title/ordinal ink centered; footer clearance corrected after runtime review.
- Play/Next moved outside the panel. Replay/Home use wooden faces with a dark lower rim, restrained contact shadow and coordinated pressed state, following the user's follow-up feedback. No glow added.
- Two independent AI-generated blank assets; mockups are references only. No design-image pixels were extracted into runtime artwork.
- No feature edits to core, meta, level data, save schema, purchase logic or other screens.
- Integrated current main `dd4887065fb5169342ec6b55398211881c329158` without conflicts, preserving its save recovery, live balance, hint-key and bird-animation repairs; all checks were rerun on this integrated tree.

## Verification

- `bash scripts/run_tests.sh`: naming self-tests, naming guard, painted-word guard, smoke and all 210 stored level routes pass. The current main suite also passes save-recovery, launcher-recovery and bird-animation checks; its intentional corrupted-save/read-only test diagnostics are expected.
- `godot --headless --path PerchSort -s res://tests/run_level_card_tools.gd`: 26 checks, zero failures; fitting tools, purchase callback, stock refresh and preserved Play size verified.
- `godot --headless --path PerchSort -s res://tests/run_jungle_dialogs.gd`: 148 checks, zero failures: both languages and ten states, all action placements / callback guards / disabled states / secondary depth tested.
- Actual Godot OpenGL/Xvfb captures: ten dialog states in English and Chinese. Reviewed titles, frame corners, row clearance, button hierarchy and bounds. `scripts/compare.sh` ran against both approved mockups. Files stay local in `build/` and ignored evidence folders, not Git.
- Timed win capture: 20 frames at 0.25 second steps over 5 seconds; stars, counters, reward appearances, flying/perched/idle birds remain animated. No sound-device listening or physical-device testing claimed.

## Boundaries

The user accepted the revised wooden secondary-button runtime screenshot. This PR stays draft for their branch testing and must not be merged automatically. Close button and characters intentionally retain game artwork rather than copying the mockup. Primary/secondary row width follows the latest user-approved composition. Other popup families and board HUD are outside this branch.

The initial temporary disk filled during import; this worktree was moved to the larger workspace, its generated cache was rebuilt completely, and tracked import settings restored. Final captures had no parse or missing-resource errors.

## Approved leaf-border follow-up

The owner approved restrained leaf accents. Added an independently generated transparent
leaf pair (`decor_jungle_leaves.png`), not pixels cut from the approval mockup.
Two proportional overlays attach to the upper-left and lower-right rim; the existing
panel, title, stars, content positions and buttons remain unchanged. The sprites ignore
mouse input and do not participate in nine-patch stretching. Transparent compositing
was inspected against a solid background: no visible bloom outside the leaf silhouette.

Validation: 268 dialog checks pass across 10 states and both English/Chinese, including
leaf size, screen bounds and input pass-through; actual Godot captures of both dialogs
in both languages were inspected. Runtime captures remain ignored build evidence.
