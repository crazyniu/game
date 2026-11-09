# Blossom Forest art

Painted with the built-in image generation tool using the brief's local references. All ten requested images are saved. No runtime, rules, levels, or existing reference artwork was changed.

| Delivered path | Size | Mode |
| --- | --- | --- |
| `tools/scene/board_w5_raw.png` | 1024x1536 | RGB |
| `tools/scene/board_w5_empty.png` | 1024x1536 | RGB |
| `tools/scene/night/board_w5_night.png` | 1024x1536 | RGB |
| `tools/map/map_world_w5_raw.png` | 1024x1536 | RGB |
| `PerchSort/world/gate_arch_frame_w4.png` | 560x600 | RGBA |
| `PerchSort/world/gate_arch_closed_w4.png` | 560x600 | RGBA |
| `PerchSort/world/gate_door_left_w4.png` | 160x288 | RGBA |
| `PerchSort/world/gate_door_right_w4.png` | 160x288 | RGBA |
| `PerchSort/world/gate_light_w4.png` | 320x440 | RGBA |
| `PerchSort/art/gate_backdrop_w5.png` | 1024x1024 | RGBA |

## Review and remaining deviations

Inspected every image with `view_image`; checked dimensions and modes with Pillow. Reviewed transparent assets on light and dark backgrounds. All six have nonconstant alpha including fully transparent pixels. No visible magenta pixels were found (R > 200, G < 70, B > 200, alpha > 32). Export sizing and mode conversion used Pillow; artwork was painted by the image tool.

These are painted deliveries requiring correction before strict acceptance:

- The board has six branches on each side, but repeated image-tool position corrections did not achieve exact guide heights. Shaft tops sampled at x=200 are 568, 686, 821, 952, 1083, 1226; at x=800 they are 633, 756, 888, 1021, 1147, 1295. Guide bar tops are approximately 560, 699, 839, 978, 1118, 1258 on the left and 629, 769, 909, 1048, 1188, 1328 on the right. These are known layout deviations, not a pass.
- Empty-board removal preserves the composition visually but does not guarantee unchanged pixels outside the removed branches. Night preserves the scene composition, with the requested lighting change.
- Map switchbacks, width, and stacking margins closely follow the reference, but are not pixel-exact: sampled path edges differ by roughly 1–4 pixels at several rows. Top and bottom 80 pixels contain grass and path only.
- Gate framing closely matches the reference, but identical source silhouettes were not established. The backdrop also has small edge/padding differences. Left and right door orientations were checked after correcting an initially reversed left leaf.

## Prompt set

Shared prompt: same cozy, soft, bright cartoon painting as the supplied world references; spring pink cherry blossoms, drifting petals, fresh light-green grass, pale blue sky; no text, numbers, birds, characters, or UI.

Asset instructions: board—twelve thin alternating edge-grown wooden perches aligned to `guide_w5.png`, small blossom/leaf tips, quiet blurred middle; empty—edit the board to remove only the twelve perches and their tip decorations; night—edit the empty scene to moonlit blue using the snow day/night pair; map—edit world one's scenery only, preserving cream path and plain 80-pixel stacking margins; frame—replace snow with blossoms while preserving arch geometry; closed gate—use the blossom frame with reference wooden doors and gold padlock; door leaves—preserve each reference silhouette and hinge side with warm spring wood; light—preserve the feathered arched glow with spring blossom haze; backdrop—replace snow forest with blossom forest while retaining the transparent square canvas and central painted view. Correction prompts specified individual branch coordinates and left-door orientation.

## Validation

- `python3 ci/check_current_game_names.py --self-test`: five tests passed.
- `python3 ci/check_current_game_names.py`: 60 tracked source files, zero violations. This guard does not validate the new untracked art geometry.
- `bash scripts/run_tests.sh`: exit 0; smoke and all 40 core levels passed; core reported zero failures in 122164 ms. Log: `/tmp/perch-art-tests.log`.
- No runtime integration, screen approval, animation, or audio acceptance is claimed by this art-only delivery.
