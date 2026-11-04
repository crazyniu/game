# Interface artwork delivery

Branch: `mac-ui-art/work`. Base: `f8b961ac`. Scope: art and evidence only, following `spec.md` in order.

## Tool glyph batch

Delivered `tool_hint`, `tool_branch`, `tool_owl_eye`, `tool_bell`, `tool_scarecrow`, and `tool_rain` to `PerchSort/Assets/Resources/Art/`, each 256 by 256 RGBA with transparent background. Existing booster illustrations and approved gameplay reference guided their form and material. No disc is baked into these glyphs. New centered single-sprite metadata is included; existing metadata is untouched.

Built-in imagegen produced every raw asset; macOS `sips -z 256 256` resized it without repainting. Prompts are in `art/ui_art/prompts.json`; raw files and QA are in `art/ui_art/`. Per-item before/after JPEGs in `evidence/` compare the previous monochrome glyph with the new colored glyph, alongside the approved context image. These are offline comparisons, not runtime screenshots.

Visual gates: reviewed `art/ui_art/qa/tools.jpg` at large and actual 64 px sizes against light, dark and purple backgrounds. All six retain readable subjects, true alpha, soft edges without visible matte fringe, and top-left lighting. The first egg-and-eye generation was rejected for an upper-right eye glint; the corrected generation moves it to the upper left. Rejected raw evidence is retained as `tool_owl_eye_rejected_lighting.png` and is not a runtime asset.

Checks: naming guard self-test passed 8 tests; naming scan passed with zero violations. Unity tests and screenshots are pending the complete asset delivery and shared lock. Tool glyph wiring is explicitly assigned to a later task; no runtime acceptance is claimed for unreferenced assets.

## Star batch

Replaced `star_full` and `star_empty` at 256 by 256, plus `star_full_small` and `star_empty_small` at 128 by 128. These explicit dimensions in the item specification take precedence over its generic canvas-preservation sentence: the previous files were 160 and 48 px. All four existing `.meta` files remain byte-identical. Small sprites are direct downsamples of the corresponding raw generation, preserving the same look.

Reviewed per-item comparisons on light, dark and purple backgrounds at 64 px. Smooth rounded gold and dim cream-beige states have visually matching silhouettes and top-left highlights; no visible matte fringe. The approved level card guided the smooth treatment, with the win panel retained alongside the evidence for context. No star generation was rejected. Runtime verification remains pending.

## Leaf sprig batch

Added `leaf_sprig_a`, `leaf_sprig_b`, and `leaf_sprig_c`, each 180 by 140 RGBA. The first two are distinct three-leaf corner arrangements derived from the approved level card; the third is a newly drawn two-leaf arrangement in the same style. No mirrored variants. Per-item evidence identifies that no prior standalone sprite existed. Reviewed all three on light, dark and purple at 64 px: clean transparency, readable leaf count and fresh green material with top-left light. No leaf generation was rejected. Runtime placement is not wired by this art-only task.

## Remaining batches

Bare sleeping owl and map glyphs remain to be produced in that order.
