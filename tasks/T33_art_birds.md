# T33 v5.2 art: species, special birds, boosters, World 1 variants (Codex, gpt-6-astra)

Worktree `/home/developer/Projects/game` (branch `t31-v5-layout` is checked out; another Codex worker is finishing `limb_top.png` and an omp worker is editing code there). Authority: `docs/GAME_DESIGN.md` v5.2, sections 4, 5, 6, 8.1, 9. Match the accepted v5 art exactly in style, lighting and scale (see `tasks/T30_art_report.md`, `art/v5/contact_sheet.png`, and the existing `bird_sunny_*` / `bird_sky_*` files).

## Ownership
Create only new files under `PerchSort/Assets/Resources/V5/Art/` (names below; do not overwrite existing files), `art/v5/t33/**` (raw, prompts, tools, QA), and `tasks/T33_art_report.md`. Touch nothing else. No commit.

## Deliverables (all RGBA, real transparency, exact sizes)

### Birds, 256x256 each, same canvas conventions as the accepted birds (standing feet at y=244, same body scale for all species, character facing 3/4 toward the viewer, facing right)
For each id in `berry, mint, cocoa, snow, ember, ink, crow`: `bird_{id}_idle, _blink, _quirk1, _quirk2, _fly_up, _fly_down, _cheer, _sleep`.
For `sunny` and `sky` add only the missing frames: `_quirk1, _quirk2, _cheer, _sleep`.
- Looks and quirks: the §4.1 table. quirk1/quirk2 are two key poses of the quirk animation. cheer = wings up, mouth open, joyful. sleep = eyes closed, slightly slumped, NO Zzz drawn (the Zzz is separate).
- `crow` is "Rook": slate-grey crow, crooked smile, holding a small shiny trinket; must look mischievous but cute, clearly not one of the sortable species.
- Every species must stay distinguishable from every other at 64 px in grayscale (silhouette features: tuft, bun, plumes, leaf crest + eye rings, ear tufts, three-dot crown, flame crest, star chest spot). Produce `art/v5/t33/qa/grayscale_64_all.png` showing all 9 idles.

### Special
- `zzz.png` 128x128: soft lilac "Z z" bubble.
- `egg_idle.png`, `egg_wobble_l.png`, `egg_wobble_r.png`, `egg_crack.png`, `egg_shell.png` 256x256: speckled cream egg sitting in a small twig nest, a soft "?" formed by speckles; the nest bottom at y=244 like bird feet; wobble frames tilt ±10 degrees around the nest; crack shows a zigzag crack; shell = two shell halves with the nest, for the hatch moment.
- `twig_left.png` 420x90: a thin short branch (2 seats), root at the left edge, same wood as `branch_left.png`, top surface at y≈32.
- `roof_band.png` 300x60: a white (tintable) cloth bunting strip that sits on the birdhouse roof edge; code tints it per species.

### Boosters and badges (same style as the existing icons: white with dark brown #5A3A22 outline, 192x192)
`icon_firefly.png` (glowing firefly), `icon_sapling.png` (sprout in soil), `icon_owl_eye.png` (owl face with big eyes), `icon_bell.png` (little brass morning bell is allowed to have a gold tint), `icon_scarecrow.png`, `icon_rain.png` (cloud with drops and a tiny sprout).
`badge_count.png` 64x64 red-orange circle with white outline (number drawn by code); `badge_plus.png` 64x64 green circle with a white plus.

### World 1 scene variants, 1080x1920 opaque, same composition rules as `bg_forest_day.png` (calm middle band y 550–1500, canopies in the top corners, low hills at the bottom)
`bg_w1_morning.png` (soft pink-gold dawn), `bg_w1_golden.png` (warm golden hour), `bg_w1_rain.png` (light rain, soft grey-blue, puddle glints low in the frame). `bg_forest_day.png` remains the noon variant.
Also `petal.png` 64x64 (pink blossom petal) and `raindrop.png` 32x64 for ambient particles.

## Process and acceptance
1. Keep raw generations and exact prompts in `art/v5/t33/raw/` and `art/v5/t33/prompts.md`; post-process with scripts in `art/v5/t33/tools/`.
2. Verify per file: size, RGBA, alpha min/max, no halos/checkerboard; foot/nest line y=244 for every bird and egg frame; same body scale across species (report body bbox heights); fly frames have tucked feet and clearly different wing positions; grayscale 64 px sheet; a contact sheet `art/v5/t33/qa/contact.png` on light and dark backgrounds; a composite `art/v5/t33/qa/w1_board_composite.png` showing 6 species + egg + sleeping bird + crow on branches over `bg_w1_golden.png`.
3. Report in `tasks/T33_art_report.md`: per-file checks, anything not achieved, honestly. Outfits are NOT in this task (moved to T36).
4. `worker_done` listing delivered/missing files.
