# T44b missing HUD icons delivery

2026-11-03. **worker_done — 8/8 requested PNGs delivered; MISSING: none.** Static asset candidates delivered for coordinator review; Unity integration and visual acceptance are NOT_RUN.

## Scope and authority

Executed [T44b_missing_icons.md](T44b_missing_icons.md) in the user-selected `/Users/apple/Documents/projects/game-icons` checkout. The current user overrides the older Linux worktree instruction and authorizes only new files under `PerchSort/Assets/Resources/Art/`, `art/v6/runtime/`, and this report. No Git commit/push, Unity launch, game code, scenes, import settings, original artwork, or existing T44 report/manifest changes were made.

Read the README, documentation router, mechanic evidence, UI_SPEC approved style, T40 measured notes, and T44 task/report. Visual references: [icon sheet](../art/v6/mock/icon_sheet.png) and the existing [sound-on icon](../PerchSort/Assets/Resources/Art/ico_sound_on.png). This task adds artwork only and establishes no gameplay contract.

## Delivered runtime files

All paths below are relative to `PerchSort/Assets/Resources/Art/`:

| File | Size |
| --- | --- |
| `ico_footsteps.png` | 128 × 128 |
| `ico_footsteps_64.png` | 64 × 64 |
| `ico_footsteps_28.png` | 28 × 28 |
| `ico_flag.png` | 128 × 128 |
| `ico_flag_64.png` | 64 × 64 |
| `ico_flag_28.png` | 28 × 28 |
| `ico_sound_off.png` | 128 × 128 |
| `ico_sound_off_64.png` | 64 × 64 |

T44b explicitly supersedes the earlier T44 report's 28 px base filenames: the unsuffixed flag/footsteps files are now 128 px; level-card consumers should use the `_28` assets. No consumer code was modified.

## Generation and processing

Used the built-in `image_gen` tool only, one call per subject. All three first attempts succeeded; no retries or fallback CLI/API calls were needed. No hand-drawn replacements were used. Exact prompts, input references, source locations, and attempt counts are recorded in [t44b_generation.json](../art/v6/runtime/t44b_generation.json).

Generated originals are preserved in [t44b_sources](../art/v6/runtime/t44b_sources/). [build_t44b.py](../art/v6/runtime/build_t44b.py) only copies, crops, resizes and composites generated images for QA. Crop bounds ignore nearly invisible alpha specks (alpha <= 8) with 3 source-pixel padding; retained alpha is not replaced or flattened. Artwork is aspect-fit to 124 × 124 inside a 128 × 128 transparent canvas, then Lanczos-downscaled to requested sizes. It does not draw pictograms. The system Python lacked Pillow; the existing Miniconda Python with Pillow 12.2.0 performed local processing, with no installation.

## Verification and limitations

- All 8 PNG dimensions were checked; all are RGBA with alpha extrema 0 and 255. [t44b_manifest.json](../art/v6/runtime/t44b_manifest.json) records actual bounds, centered intended pivot `(0.5, 0.5)`, and SHA-256 hashes. No Unity pivot/import metadata was authored.
- Visually inspected [native-size cream QA sheet](../art/v6/runtime/t44b_qa_cream.png), including 128/64/28 px color versions, 64 px grayscale, and original sound-on comparison. The paired footsteps, flag/pole, and speaker/red cross remain distinguishable at 64 px on cream and in grayscale. Flag and footprint silhouettes also remain readable at 28 px; toe/highlight details are secondary at that size. This is visual inspection, not a user recognition study.
- Colors, warm highlights and dark-brown contours follow the supplied set. Contours approximate the requested 3 px outline; generated raster artwork does not certify a constant mathematical stroke width or exact pixel identity with the reference sheet.
- Sound-off retains the same green speaker design with waves replaced by a coral-red cross. Generation slightly changes the contour/shading and proportions; it is not a pixel-identical copy of the original speaker body. Coordinator style acceptance remains pending.
- Unity import, in-context runtime screenshots, animation, and play tests: NOT_RUN, explicitly prohibited for this task. No test suite was run for this static-asset delivery.

## Supporting files delivered

- `art/v6/runtime/t44b_generation.json`
- `art/v6/runtime/t44b_manifest.json`
- `art/v6/runtime/build_t44b.py`
- `art/v6/runtime/t44b_qa_cream.png`
- `art/v6/runtime/t44b_sources/ico_footsteps.png`
- `art/v6/runtime/t44b_sources/ico_flag.png`
- `art/v6/runtime/t44b_sources/ico_sound_off.png`
- `tasks/T44b_art_report.md`

**MISSING: none.**
