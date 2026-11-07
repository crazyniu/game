# Aviary list polish

## Authorized scope

The user approved improving one page for review, with multilingual support but English content only for now. This overrides the older Codex art-only role for this bounded UI task. The chosen page is the canonical aviary list in `game/aviary_view.gd`; bird detail, shop, map and gameplay remain visually unchanged. No tests, lint, build or screenshot acceptance run is requested. Open the page for the user after implementation.

## Visual contract

- Keep the existing forest painting and animated BirdView characters.
- A calm cream header contains a 128 px close control, a complete English title sprite, and a green collection-count pill with a clear feather icon.
- Header title space is separate from the close and counter spaces. Title uses a dark forest green; names use dark warm ink without a thick outline.
- Two columns, 476 x 480 cards, 32 px gaps and 48 px side margins. A 1536 px-high clipped grid at y=264 shows three complete rows at rest. Scrollbar and hit regions follow that viewport.
- Each card reserves independent bird, ability-badge and name regions. Keep animated birds; remove the redundant Unknown label under unknown silhouettes.
- Draw close and feather icons as clean vector silhouettes; no new decorative icons or instructional copy.

## Localized asset contract

- Runtime words remain complete texture sprites, not live font rendering or letter assembly. English-only content is the first locale pack.
- Source catalog `tools/kit/locales/en.json` uses stable IDs: `aviary.title`, `bird.<species>.name`. Retain all species/storage identifiers.
- `tools/kit/build_localized_labels.py` renders complete phrases from an OFL Nunito source font kept under `tools/kit/fonts/`, including its license. Font use is offline only.
- Assets: `PerchSort/art/locale/en/label_<id>.png`, `number_<code>.png`, and `manifest.json`. The manifest contains `locale`, `labels` (ID -> `text`, `file`, `cap`, `line_height`, `baseline`), and `numbers` (`cap`, `line_height`, `baseline`, `advance`, `glyphs` mapping character to `file`). Use uniformly sized digit cells; include 0-9, slash and question mark for this page. Images have transparent backgrounds and white ink for runtime tinting.
- `game/ui_text.gd` resolves full locale, then base language, then the declared English default from `art/locale/catalog.json`. It loads whole label textures by ID and uses the numeral cells only for changing numeric values. It has no dependencies on core or meta.
- Future locales add catalog entries and authored assets; source catalogs can contain explicit line breaks and a font path for their script. This page refreshes on Godot's translation-change notification. Do not add a language switch UI while only English is available.

## Ownership and boundaries

- Asset worker: only `tools/kit/build_localized_labels.py`, `tools/kit/fonts/`, `tools/kit/locales/`, `PerchSort/art/locale/en/`.
- Coordinator: `game/ui_text.gd`, `game/aviary_view.gd`, locale catalog, two scoped SVG icons, this work log.
- Preserve previous launcher/window changes and unrelated bird-model work. No core/meta, rules, levels, economy, save or animation-frame edits. No commits or publication in this task.
