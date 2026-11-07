# Number lettering report

## Delivery

- Replaced the 0–9 numeral source with bundled Lilita One and retained its OFL license. Regenerated 17 canonical numeral/symbol PNGs; complete labels and outlined headings were preserved.
- Digit cells now use a 128 px canvas height, baseline 126 and advance 126. Mathematical symbols retain the Nunito source with individual widths, centred inside the same vertical band.
- Replaced per-digit child TextureRects with drawing in NumberSprite's own bounds after layout. Numeric updates queue a redraw. A single scale fits the whole string to available width and height, with the existing alignment and tint API.
- The user's next runtime screenshot showed solid rectangles replacing numerals. The initial drawing code held textures only in local variables, allowing the resources to be released before queued drawing. The corrected locale loader retains all numeral Texture2D resources and drawing reads those shared references. No artwork regeneration or layout changes were made for this correction.
- The subsequent user log reported a missing `.` key. The board HUD converted the raw JSON level ID directly to text, introducing a decimal suffix into an integer-only display. It now reads the already typed `session.state.level_id`. Other numeric call sites use integer values, integer formatting or the existing supported symbols. No decimal glyph or missing-character fallback was added; this correction changes the source value, with no generation or runtime checks.
- Widened the level-number box to 70 px and reduced the adjacent label box. The level pill retains its size, position and 16 px right inset. The phase pill remains beside it.
- Updated the shared UI contract and font production documentation. Unrelated ongoing edits were preserved.

## Production command

```sh
DYLD_LIBRARY_PATH=/opt/homebrew/opt/fribidi/lib uv run --no-project --isolated --with pillow tools/kit/build_localized_labels.py en --numbers-only
```

Completed successfully: 0 labels, 0 headings and 17 numeral cells rendered into `PerchSort/art/locale/en`.

## Acceptance boundary

No tests, lint, game build, game launch, UI automation or screenshots were run. The change has source and generated assets only; runtime clipping, visual fit and readability remain for user acceptance after relaunching.
