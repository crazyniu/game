# Interface artwork delivery

Branch: `mac-ui-art/work`. Base: `f8b961ac`. Scope: art and evidence only, following `spec.md` in order.

## Tool glyph batch

Delivered `tool_hint`, `tool_branch`, `tool_owl_eye`, `tool_bell`, `tool_scarecrow`, and `tool_rain` to `PerchSort/Assets/Resources/Art/`, each 256 by 256 RGBA with transparent background. Existing booster illustrations and approved gameplay reference guided their form and material. No disc is baked into these glyphs. New centered single-sprite metadata is included; existing metadata is untouched.

Built-in imagegen produced every raw asset; macOS `sips -z 256 256` resized it without repainting. Prompts are in `art/ui_art/prompts.json`; raw files and QA are in `art/ui_art/`. Per-item before/after JPEGs in `evidence/` compare the previous monochrome glyph with the new colored glyph, alongside the approved context image. These are offline comparisons, not runtime screenshots.

Visual gates: reviewed `art/ui_art/qa/tools.jpg` at large and actual 64 px sizes against light, dark and purple backgrounds. All six retain readable subjects, true alpha, soft edges without visible matte fringe, and top-left lighting. The first egg-and-eye generation was rejected for an upper-right eye glint; the corrected generation moves it to the upper left. Rejected raw evidence is retained as `tool_owl_eye_rejected_lighting.png` and is not a runtime asset.

Checks: naming guard self-test passed 8 tests; naming scan passed with zero violations. Unity tests and screenshots are pending the complete asset delivery and shared lock. Tool glyph wiring is explicitly assigned to a later task; no runtime acceptance is claimed for unreferenced assets.

## Remaining batches

Stars, leaf sprigs, bare sleeping owl and map glyphs remain to be produced in that order.
