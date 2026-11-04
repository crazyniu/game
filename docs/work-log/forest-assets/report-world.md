# T49 world assets and booster addendum

Status: DONE for static art delivery. Branch `mac-art/world`, based on accepted T48 `825ca1d`. Delivered 32 world PNGs and six booster illustrations. No approved originals, birds/eggs, runtime code, scenes or Unity settings changed.

## Deliverables and integration

World files are in `PerchSort/Assets/Resources/World/`. Each world has four `bg_level_<world>_<variant>.png` backgrounds, plus `branch_left_<world>.png`, `twig_left_<world>.png`, `tree_crown_<world>.png`, and `trunk_left_<world>.png`.

| World | Background variants | Branch top y / observed range | Twig top y / observed range | Crown attachment |
|---|---|---|---|---|
| W1 spring | morning, noon, golden, rain | 17 / 17 | 18 / 18 | (138,319) |
| W4 snow pine | morning, noon, bluehour, aurora | 30 / 28–30 | 22 / 20–22 | (105,355) |
| W2 bamboo | mist, noon, lanterns, drizzle | 9 / 6–9 | 19 / 18–19 | (96,332) |
| W3 maple | morning, noon, sunset, windy | 22 / 22–23 | 8 / 8 | (111,358) |

Backgrounds are opaque RGBA, 1080x1920. Branches 620x90, twigs 400x80, crowns 600x400 and trunks 160x1920 have genuine transparent exteriors. Branch roots are at the left edge; mirror for the right side. Coordinates are top-left sprite pixels, while recorded Unity pivots use bottom-left normalized coordinates. Preserve transparent canvas padding. Crop/clip trunks above the crown attachment, and overlap their top under foliage. Snow/bamboo contours have up to 3 px local variation; nominal top y is an alignment reference, not collision geometry.

[`art/approved_ui/world/world_assets.json`](../art/approved_ui/world/world_assets.json) consolidates variants, actual sizes/hashes and all piece contracts. Per-world `pieces/w<n>/metadata.json` records usable perch intervals, bounds, pivots and attachments. W1 branch usable x60..580 and twig x60..370; other conservative intervals are in their metadata.

## Six coloured booster illustrations

In `PerchSort/Assets/Resources/Art/`, all 256x256 RGBA:

- `booster_hint_illus.png`: glowing bulb.
- `booster_sapling_illus.png`: branch with green plus.
- `booster_owl_eye_illus.png`: egg in nest with amber eye above.
- `booster_bell_illus.png`: brass bell and rays.
- `booster_scarecrow_illus.png`: grey crow with red cross.
- `booster_rain_illus.png`: rain cloud above sprout.

No discs, cards, numbers or text are baked in. The green plus and amber eye follow the explicit addendum. Recommended pivot is center. Exact prompts, raw generations, bounds and QA are in `art/approved_ui/world/boosters/`. One owl-eye retry removed a connecting glow; the other first attempts were retained.

## Provenance and static verification

All 39 generation calls used only the built-in image tool with approved screenshots as references: 16 backgrounds, 16 world pieces, 7 booster calls. No CLI/API-key fallback. Exact prompts and raw outputs are preserved under `art/approved_ui/world/`. Pillow operations only prepare cutouts/alpha, trim, resize, align and make QA composites; no production artwork was drawn. Backgrounds use proportional center-fit resizing with no colour alterations.

W1 references `ui_level_k3.png`; W4 references `ui_level_k7.png`. Bamboo and maple are disclosed seasonal adaptations of W1's pale hazy rendering. All backgrounds retain an open center and top-corner foliage without perches/birds/UI. Lanterns stay distant; the aurora variant alone carries aurora lights. These generated assets follow the approved palette/material direction, not pixel-identical segmentation.

Checks passed: exact 32 world filenames and requested dimensions; all 38 exports RGBA; 16 backgrounds opaque; 16 pieces and 6 boosters genuinely transparent. All four background contact sheets and all four assembled world scenes reviewed. `qa/w1_comparison.png` and `qa/w4_comparison.png` compare approved screens with world-only compositions using measured branch-row heights. Birds/HUD are intentionally outside this takeover; flat branches use mean observed row heights. These are static composition studies, not runtime level data.

Booster light/dark and 64px colour/grayscale recognition reviewed. World pieces' 64 px grayscale sheet is in `pieces/qa_64px_grayscale.png`: bamboo/snow remain structurally distinct; oak versus maple and thin branch/trunk silhouettes are not reliably identifiable by grayscale alone. They are decorative scenery, not species controls. Individual reports: `pieces/QA.md`, `boosters/REPORT.md`.

Unity import, bird foot contact, runtime placement, clipping, target-device screenshots and animation were NOT_RUN. No Unity process or lock was created. Coordinator/code/QA integration remains next; no unfinished owned files or external blockers remain.

## Push milestones

- `ba83f02`: W1 alone, for early mac-ui integration.
- `2e3d391`: W4 and approved-board comparisons.
- `6d15659`: W2.
- `c66c3f4`: six booster illustrations.
- W3 and consolidated final contracts: this report's delivery commit (branch HEAD).
