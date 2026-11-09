# Rainforest art delivery

Generated with the built-in imagegen tool, one call per picture. Only world 9 artwork was authored; entry gate filenames use w8 as required (M = N - 1). Existing reference images were read, not changed.

## Files

| File | Size | Mode |
| --- | --- | --- |
| `tools/scene/board_w9_raw.png` | 1024x1536 | RGB |
| `tools/scene/board_w9_empty.png` | 1024x1536 | RGB |
| `tools/scene/night/board_w9_night.png` | 1024x1536 | RGB |
| `tools/map/map_world_w9_raw.png` | 1024x1536 | RGB |
| `PerchSort/world/gate_arch_frame_w8.png` | 560x600 | RGBA |
| `PerchSort/world/gate_arch_closed_w8.png` | 560x600 | RGBA |
| `PerchSort/world/gate_door_left_w8.png` | 160x288 | RGBA |
| `PerchSort/world/gate_door_right_w8.png` | 160x288 | RGBA |
| `PerchSort/world/gate_light_w8.png` | 320x440 | RGBA |
| `PerchSort/art/gate_backdrop_w9.png` | 1024x1024 | RGBA |

## Prompt set

All prompts specified cozy, soft, bright cartoon rainforest, matching reference brushwork, saturation, light and clutter; no text, numbers, birds, characters or UI.

- Board: guide_w9 composition, board_w4_raw and board_w1_raw style; glossy leaves, vines, orchids, small distant waterfall, warm humid light, calm blurred centre; exactly six left and six right vine-wrapped wooden branches at guide bar heights with small leafy inner tips.
- Empty: edit the new board, remove twelve branches and their vines/tips, inpaint only footprints, preserve every other element.
- Night: edit the empty board, preserve composition, moonlit blue lighting using board_w4_night as lighting reference.
- Map: edit map_world_w1, preserve the cream path geometry and width, repaint scenery as rainforest; plain grass/path in top and bottom 80 pixels.
- Frame: edit gate_arch_frame_w3, preserve frame geometry, replace frost with jungle leaves, vines and orchids; transparent opening/exterior.
- Closed: gate_arch_closed_w3 geometry and door positions, new rainforest frame decoration, honey wood and golden lock; transparent upper opening/exterior.
- Left/right doors: corresponding w3 outlines and hinge sides; honey wood, golden rivets, subtle vine at outer edge; transparent exterior.
- Light: gate_light_w3 arched framing; golden shafts through humid teal mist over a glowing winding path; feathered transparency.
- Backdrop: gate_backdrop_w4 framing, tropical jungle and stepping stones, small waterfall and warm humid light; transparent exterior.

## Checks and boundaries

All ten outputs were visually inspected. Pillow verified exact sizes/modes. Six transparent assets contain real alpha; low-alpha stray pixels and visible pure-magenta pixels were cleared. Backdrop alpha is confined to the reference rectangle (240,250)-(768,854). No magenta backing was used. Generated outputs were resized to required dimensions without changing retained references.

The raw board has twelve alternating branches. Sampled wood spans closely follow guide heights, with a few pixels of vertical variation from organic contours. The empty board has no perch branches. Night retains the overall composition.

**Precision limitations:** imagegen repainted pixels beyond branch removal, so raw/empty are not pixel-identical outside removed branches. The map visually follows the reference switchbacks, but exact path-edge identity is not verified and small deviations remain. The gate theme matches, but its organic outline and frame/closed decoration are not exactly identical to the w3 silhouettes or each other. These strict invariants are not accepted as passed. Runtime integration and user visual approval were not performed.

## Verification

- `python3 ci/check_current_game_names.py --self-test`: 5 tests passed.
- `python3 ci/check_current_game_names.py`: 0 violations (60 tracked source files).
- `bash scripts/run_tests.sh`: passed; smoke checks 0 failures; all 40 levels solved/replayed, core checks 0 failures in 60437 ms.

No rules, levels, screens, retained originals or other worlds were intentionally edited. Other-world files appeared concurrently in the shared workspace and were left alone.
