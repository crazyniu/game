# Interface artwork delivery

Branch: `mac-ui-art/work`. Base: `f8b961ac`. Scope: art and evidence only, following `spec.md` in order.

## Tool glyph batch

Delivered `tool_hint`, `tool_branch`, `tool_owl_eye`, `tool_bell`, `tool_scarecrow`, and `tool_rain` to `PerchSort/Assets/Resources/Art/`, each 256 by 256 RGBA with transparent background. Existing booster illustrations and approved gameplay reference guided their form and material. No disc is baked into these glyphs. New centered single-sprite metadata is included; existing metadata is untouched.

Built-in imagegen produced every raw asset; macOS `sips -z 256 256` resized it without repainting. Prompts are in `art/ui_art/prompts.json`; raw files and QA are in `art/ui_art/`. Per-item before/after JPEGs in `evidence/` compare the previous monochrome glyph with the new colored glyph, alongside the approved context image. These are offline comparisons, not runtime screenshots.

Visual gates: reviewed `art/ui_art/qa/tools.jpg` at large and actual 64 px sizes against light, dark and purple backgrounds. All six retain readable subjects, true alpha, soft edges without visible matte fringe, and top-left lighting. The first egg-and-eye generation was rejected for an upper-right eye glint; the corrected generation moves it to the upper left. Rejected raw evidence is retained as `tool_owl_eye_rejected_lighting.png` and is not a runtime asset.

Checks: naming guard self-test passed 8 tests; naming scan passed with zero violations. Final Unity results are recorded below. Tool glyph wiring is explicitly assigned to a later task; no runtime acceptance is claimed for unreferenced assets.

## Star batch

Replaced `star_full` and `star_empty` at 256 by 256, plus `star_full_small` and `star_empty_small` at 128 by 128. These explicit dimensions in the item specification take precedence over its generic canvas-preservation sentence: the previous files were 160 and 48 px. All four existing `.meta` files remain byte-identical. Small sprites are direct downsamples of the corresponding raw generation, preserving the same look.

Reviewed per-item comparisons on light, dark and purple backgrounds at 64 px. Smooth rounded gold and dim cream-beige states have visually matching silhouettes and top-left highlights; no visible matte fringe. The approved level card guided the smooth treatment, with the win panel retained alongside the evidence for context. No star generation was rejected. The full gold star is also visible in the final Unity win capture; the dim and small variants retain offline evidence only because the standard capture fixtures do not expose all those states.

## Leaf sprig batch

Added `leaf_sprig_a`, `leaf_sprig_b`, and `leaf_sprig_c`, each 180 by 140 RGBA. The first two are distinct three-leaf corner arrangements derived from the approved level card; the third is a newly drawn two-leaf arrangement in the same style. No mirrored variants. Per-item evidence identifies that no prior standalone sprite existed. Reviewed all three on light, dark and purple at 64 px: clean transparency, readable leaf count and fresh green material with top-left light. No leaf generation was rejected. Runtime placement is not wired by this art-only task.

## Bare sleeping owl batch

Added `Art/hoot_sleep_bare`, 320 by 320 RGBA, derived by removing the nightcap from `Birds/hoot_sleep`. The original bird asset remains unchanged. The new sprite retains the original centered `(0.5, 0.5)` pivot, pixels-per-unit and square canvas. The closed-eye expression, tan layered feathers, cream face, orange feet and lower-canvas placement remain recognizable; the visible lower alpha bound is y=301. Reviewed per-item before/after and 64 px transparency on all three backgrounds. No owl generation was rejected. This is a static asset delivery, not animation evidence or mechanic-row runtime acceptance.

## Map glyph batch

Replaced `icon_basket` with a white woven basket and arched handle, and `icon_aviary` with a simple house outline and small bird silhouette. Both use faint lilac shading based on `icon_gear`, and both are 256 by 256 RGBA as explicitly requested (previous canvas: 96 by 96). Existing filenames and `.meta` bytes are preserved. Reviewed transparent openings and outer edges against light, dark and purple backgrounds, with actual 64 px previews. No purple disc is baked in.

The initial basket generation was rejected for broad white glow outside the silhouette and inside the handle. `icon_basket_rejected_glow.png` retains that raw rejection; it is not a runtime asset. Regeneration removed the glow and restored clean transparent openings. The aviary candidate passed offline visual review without regeneration.

## Delivery and verification status

All 16 requested runtime images are present. No requested image is missing. Rejected generations: the initial egg-eye lighting and initial basket glow, both replaced by corrected assets. New tool glyphs, sprigs and bare owl are not wired by this task, so their runtime placement remains an integration boundary. No code was edited.

Each specified batch was committed and pushed in order: `ffeb30b7` tools, `74e53d46` stars, `b6b50207` sprigs, `8f953e4f` owl, `01517f3c` map glyphs. This final evidence update follows those five pushes.

## Final Mac verification

- `PYTHONDONTWRITEBYTECODE=1 python3 ci/check_current_game_names.py --self-test`: 8 passed.
- `PYTHONDONTWRITEBYTECODE=1 python3 ci/check_current_game_names.py`: zero violations.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`: scene build and compilation succeeded; EditMode 312/312 and PlayMode 53/53 passed, zero failed, skipped or inconclusive. Retained LevelsData 81 and Layout 41 cases were discovered. No test or discovery safeguard was edited.
- `flock /tmp/unity.lock bash scripts/screenshots.sh`: exit 0; all 34 expected captures validated at 1080 by 1920 under `build/screens/game/`.

`evidence/verification.json` records commands, counts and XML hashes. Local XML and logs remain under `build/`. Reviewed map, level-card and win captures; committed JPEGs and reference comparisons are `evidence/runtime_map*`, `evidence/runtime_level_card*` and `evidence/runtime_win*`. The new map glyphs render with clean openings on purple discs; win stars render with smooth rounded points and top-left highlights. Whole-screen layouts and other existing artwork still differ from the approved references, so this is not whole-UI visual acceptance. The level-card fixture uses a fresh profile, and it does not prove completed-node small stars or the bare-owl mechanic row. No timed animation, GUI interaction or device acceptance is claimed.

`tools_context.jpg` and `panel_context.jpg` are explicitly labeled offline material assemblies for the unwired assets. `rejections.jpg` shows rejected versus corrected generations. `art/ui_art/qa/delivery_manifest.json` records all runtime dimensions, alpha-capable modes and image hashes. Every item has a committed before/after JPEG and quantitative alpha QA.

Before Unity, the worktree was clean outside assigned artwork/evidence and an initial scene/meta/settings/package hash snapshot was recorded. Unity rewrote 114 unrelated bird metadata files with whitespace, expanded defaults in seven existing overlay metadata files, and rebuilt scene object/InputSystem action IDs. Those 122 files were reviewed, their patch saved at `build/ui-art/unrelated-unity-import.patch`, and restored to the recorded original bytes. Every original scene/meta/settings/package hash matches the initial snapshot, including all six replaced sprites' metadata. Only the ten new sprites retain their Unity-added empty metadata fields, with trailing whitespace normalized. The test results describe the standard builder-generated scene; integrated acceptance of the retained scene remains with the coordinator.
