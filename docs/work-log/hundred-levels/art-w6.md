# Mushroom Forest artwork

Created with the built-in image generation tool, using the brief and its named local references. Only world 6 artwork is delivered; the gate uses w5 because it leads out of world 5.

## Files

- `tools/scene/board_w6_raw.png` — 1024x1536 RGB, six mossy perch branches on each side.
- `tools/scene/board_w6_empty.png` — 1024x1536 RGB, branch-removal edit of the selected raw painting.
- `tools/scene/night/board_w6_night.png` — 1024x1536 RGB, moonlit edit of the empty painting.
- `tools/map/map_world_w6_raw.png` — 1024x1536 RGB, Mushroom Forest repaint using the world 2 trail reference.
- `PerchSort/world/gate_arch_frame_w5.png` — 560x600 RGBA.
- `PerchSort/world/gate_arch_closed_w5.png` — 560x600 RGBA.
- `PerchSort/world/gate_door_left_w5.png` — 160x288 RGBA.
- `PerchSort/world/gate_door_right_w5.png` — 160x288 RGBA.
- `PerchSort/world/gate_light_w5.png` — 320x440 RGBA.
- `PerchSort/art/gate_backdrop_w6.png` — 1024x1024 RGBA.

## Checks and remaining discrepancies

All generated pictures were visually inspected. Pillow checked dimensions and modes. Transparent assets retain real alpha (minimum 0); a visible-pixel magenta check found zero pixels with R>180, G<90, B>180, alpha>16. Generated sprites were resized to the specified delivery dimensions.

Strict geometric acceptance is NOT passed. Repeated image-tool corrections produced twelve branches, but their heights still deviate from the guide. Approximate wood detections (not exact perch surfaces) are left 587,708,836,963,1095,1223 and right 655,772,897,1024,1151,1283; guide bar centres are approximately left 570,709,849,989,1128,1268 and right 640,779,919,1058,1198,1338. The map retains the switchback structure and plain grass/path end zones, but exact path width and pixel positions are not guaranteed. Gate structural proportions resemble the reference, but outline equality and perfect alignment between separately generated frame/closed/door assets are not established. Branch-removal and night edits preserve overall composition but do not guarantee unchanged pixels outside the edited regions. These are deliverables for review, not a claim of exact brief compliance.

## Prompt set

Shared instruction: cozy, soft, bright cartoon painting matching named references; mossy oaks, deep green ferns, giant red and orange white-spotted toadstools, warm daylight; no text, numbers, birds, characters or UI.

- Board: paint twelve thin horizontal mossy branches at the guide bars, six per side, small leafy tips, calm blurred centre and bright uncluttered top; subsequent prompts corrected branch count and explicit pixel heights.
- Empty: remove all twelve branches and tip leaves; inpaint forest; preserve every other object and position.
- Night: change only empty-board lighting to indigo moonlit blue with cyan highlights, using the existing night reference.
- Map: repaint world 2 scenery while preserving every cream-path switchback and width; first/last 80 pixels contain grass and path only.
- Frame: retain world 3 arch framing and opening; replace snow with moss, oak leaves and spotted mushrooms; transparent exterior/opening.
- Closed: retain mushroom frame and add curved honey-oak double doors, gold hinges/braces and central padlock, using world 3 closed layout.
- Door leaves: retain respective world 3 silhouette, curve orientation and hardware placement; honey oak, mossy edges and a tiny outer-corner mushroom; transparent exterior.
- Light: retain oval gate-light framing and soft alpha; glowing winding cream trail, warm shafts, green woodland haze and mushroom margins.
- Backdrop: retain reference inset forest-view framing and perspective; replace snow/pines with oak, ferns and mushrooms; transparent surrounding canvas.

## Repository validation

`bash scripts/run_tests.sh` passed: naming guard self-test 5/5, naming scan 60 tracked source files / 0 violations, smoke 0 failures, core 40 levels / 0 failures (87729 ms). This includes `python3 ci/check_current_game_names.py --self-test` and `python3 ci/check_current_game_names.py`. No runtime integration or visual acceptance is claimed.
