# T30-A v5 level-1 art (Codex, image generation)

Branch `t30-v5-first-level`, shared worktree `/home/developer/Projects/game`. Read `docs/GAME_DESIGN_V5.md` §3–§4 first; it is the authority for style and sizes. Another worker writes code in parallel in the same worktree.

## Ownership
You may create/edit only:
- `art/v5/**` (raw generations, prompts, contact sheets, QA files)
- `PerchSort/Assets/Resources/V5/Art/**` (final runtime PNGs; Unity will create `.meta` on import — do not hand-write metas)
- `tasks/T30_art_report.md`

Never modify anything under `art/` outside `art/v5/`, never touch code, scenes, `ProjectSettings`, or `Packages`. Do not commit or push; the coordinator commits.

## Deliverables (exact runtime file names, all PNG with real alpha unless noted)

Under `PerchSort/Assets/Resources/V5/Art/`:

| File | Size (px) | Notes |
| --- | --- | --- |
| `bg_forest_day.png` | 1080×1920, opaque | sky gradient + soft clouds, soft distant forest hills low in the frame, leafy canopy framing the top corners. NO branches, NO birds, NO UI. Keep the band y 550–1500 calm/low-contrast so birds read clearly. |
| `branch_left.png` | 640×120 | one natural wooden branch, root at the LEFT edge (thick, cut cleanly at x=0), tapering to a twig tip at the right with 1–2 small leaves. Top surface roughly flat at y≈40 from the top so birds can stand on it. `branch_right` is made in code by mirroring — do not deliver it. |
| `bush_edge.png` | 360×300 | rounded leafy bush cluster that hides a branch root at the screen edge; drawn to sit on the left edge (code mirrors for right). |
| `birdhouse.png` | 300×340 | cute wooden birdhouse, moss-green roof (not red), round dark door hole in the center, small perch peg under the door, hanging from a short rope/vine at the top. |
| `bird_{id}_idle.png`, `bird_{id}_blink.png`, `bird_{id}_fly_up.png`, `bird_{id}_fly_down.png` for id in `sunny`, `sky` | 256×256 each | same character, same scale and position in all four frames; idle faces 3/4 toward the viewer; feet at the bottom center (feet touch y≈244). blink = idle with eyes closed. fly_up = wings raised, feet tucked; fly_down = wings lowered. |
| `btn_square_green.png`, `btn_square_wood.png` | 256×256 | glossy rounded-square button bases (corner radius ~60), thick soft outline, top highlight. No icon on them. |
| `btn_round_purple.png` | 256×256 | round glossy button base for settings. |
| `icon_undo.png`, `icon_hint.png` (light bulb), `icon_add_branch.png` (branch with a plus), `icon_settings.png` (gear), `icon_restart.png`, `icon_sound_on.png`, `icon_sound_off.png`, `icon_close.png`, `icon_next.png` (play/arrow), `icon_home.png` | 192×192 | white icons with a dark brown (#5A3A22) outline, chunky rounded style, centered. |
| `coin.png` | 128×128 | golden coin with an embossed leaf (not a star). |
| `pill_cream.png` | 260×90 | cream rounded pill with soft outline (coin counter / level pill background). |
| `panel_win.png` | 860×900 | cream panel with a warm wood frame, no text. |
| `ribbon.png` | 900×200 | leaf-green ribbon banner, no text. |
| `star_full.png`, `star_empty.png` | 192×192 | glossy gold star; empty = soft brown silhouette. |
| `sparkle.png` | 128×128 | soft 4-point sparkle for particles. |
| `hand_pointer.png` | 200×200 | cartoon tapping hand (white glove-like, brown outline), fingertip at the top-left area; record the fingertip pixel in the report. |

Also produce `art/v5/contact_sheet.png` showing every runtime file on light and dark backgrounds, and a 64 px grayscale strip of the bird idles.

## Style
Follow `docs/GAME_DESIGN_V5.md` §4: plush 3D-rendered cartoon, soft fur, big glossy eyes, warm bright palette, chunky rounded UI. The reference genre screenshots the user showed were only style direction: do not reproduce any existing game's character, logo, coin or button art. `sunny` and `sky` must be original designs matching the table in §4.

## Process and acceptance
1. Generate with your image tool; keep raw outputs and the exact prompts in `art/v5/raw/` and `art/v5/prompts.md`.
2. Post-process (Python/Pillow is fine; scripts in `art/v5/tools/`) to: exact sizes above, real transparency (no baked checkerboard, no white halo — check alpha edges), consistent bird scale/foot line across the four frames of each species.
3. Verify and record in `tasks/T30_art_report.md`: per-file size + mode (RGBA) + alpha min/max, foot-line y per bird frame, the hand fingertip pixel, grayscale 64 px distinguishability of sunny vs sky, and a list of anything you could not achieve.
4. Done = every listed file exists at the exact path and size, report written. A missing file is a failure to report honestly, not to fake (no placeholder flat shapes passed off as final art).
