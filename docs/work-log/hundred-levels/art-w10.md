# Wisteria Forest art

Produced with built-in imagegen from the art brief, using the named existing paintings as references. No runtime wiring, rules or level changes.

| Output | Size | Mode |
| --- | --- | --- |
| `tools/scene/board_w10_raw.png` | 1024x1536 | RGB |
| `tools/scene/board_w10_empty.png` | 1024x1536 | RGB |
| `tools/scene/night/board_w10_night.png` | 1024x1536 | RGB |
| `tools/map/map_world_w10_raw.png` | 1024x1536 | RGB |
| `PerchSort/world/gate_arch_frame_w9.png` | 560x600 | RGBA |
| `PerchSort/world/gate_arch_closed_w9.png` | 560x600 | RGBA |
| `PerchSort/world/gate_door_left_w9.png` | 160x288 | RGBA |
| `PerchSort/world/gate_door_right_w9.png` | 160x288 | RGBA |
| `PerchSort/world/gate_light_w9.png` | 320x440 | RGBA |
| `PerchSort/art/gate_backdrop_w10.png` | 1024x1024 | RGBA |

Prompt set: match the existing cozy, soft, bright painted cartoon style; old trees with long purple/lilac wisteria cascades, lavender light and small warm lantern lights; no text, numbers, birds, characters or UI. Board prompts supplied the guide and explicit branch coordinates; the empty edit requested removal of branches only; the night edit requested blue moonlight with unchanged composition. Map edit requested unchanged w2 cream-path geometry and grass/path-only 80px edge bands. Gate edits requested unchanged w3 framing and silhouettes with wisteria decoration and real transparency. Backdrop requested the same forest-window framing as w4.

## Checks and boundaries

Personally inspected all ten paintings. Pillow confirmed every listed size/mode, and all six RGBA assets contain genuine transparent pixels. All have the requested theme and no visible forbidden content. Day/night retain the composition. The following strict requirements are **not fully met** and must not be treated as approved:

- Branches: six per side are present, but repeated image edits did not achieve exact guide heights. At x256 the measured brown branch centers are approximately 614, 752, 884, 1012, 1143, 1271; target guide centers are 570, 709, 849, 988, 1128, 1268. At x768 they are 684, 815, 947, 1075, 1203, 1332; targets 639, 779, 919, 1058, 1198, 1338. The largest deviation is about 45px.
- Empty edit: branches are removed, but the generator also changed pixels outside the branches. Mean absolute RGB change in the top 500 rows is 3.916/255. Exact unchanged-background acceptance fails.
- Map: the switchbacks and width visually match w2; 17 sampled rows show cream-path boundary differences of 0–6px against the scaled reference. Exact path identity is not established. Top/bottom 80px contain grass and path only.
- Backdrop: genuine alpha, no visible pure-magenta pixels. Stray alpha outside the forest window was cleared. Visible bounds are (240,251)–(767,853), close to but not identical to the reference window.
- Gates: wooden framing visually follows w3, but floral decoration changes the silhouette; exact outline identity is not established. The left door needed transparent-margin trimming before sizing. Transparency-edge cleanup is recorded below.

Gate transparency cleanup removed low-alpha contamination, saturated edge spill and hidden RGB beneath fully transparent pixels. Reopened and personally checked all five final images: conspicuous red/yellow/magenta fringes are gone; all five contain zero visible pure-magenta pixels. Door margins were trimmed before final sizing. Tiny decorative highlights and nonidentical decorative silhouettes remain; these are painted candidates, not an exact-geometry acceptance pass.

Commands: `python3 ci/check_current_game_names.py --self-test` passed (5 tests); `python3 ci/check_current_game_names.py` passed (0 violations). Pillow/numpy checked dimensions, modes, alpha and sampled geometry. `bash scripts/run_tests.sh` first exited 143 during import without smoke/core output; retry passed: smoke 0 failures, all 40 levels solved/replayed, core 0 failures in 86132 ms. No gameplay or animation acceptance is claimed.
