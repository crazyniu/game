# T48 approved-atlas cut-outs

27 RGBA assets are extracted directly from `art/v7/ui7_icons.png`. No artwork is synthesized or redrawn. RGB colors are retained from the source, followed only by Lanczos resizing. The nine gift badges intentionally retain their cream-and-gold discs. The extracted white glyphs intentionally retain the original subtle lavender dimensional shading; there are no purple button discs.

`provenance.json` records source hash, source crop, actual source alpha bounds, output canvas size, final alpha bounds, alpha statistics, masking method and visual review. Each output has a transparent margin and real partial-alpha edge pixels.

Masking removes the external neutral atlas background and baked external shadows. Color silhouettes use a saturation separation with filled interiors, a one-source-pixel inset, and a 0.45-source-pixel edge feather. The flag required a hand-traced cloth silhouette mask; this changes alpha only. The padlock shackle opening is cleared separately. Glyphs use white/lavender color ratios and connected-component selection to remove disc glare; no recoloring is performed.

QA includes `qa_color.jpg` and `qa_grayscale.jpg` on a dark background, plus `qa_64px.png` showing color/grayscale at 64 px on a light background. The white glyphs are intended for purple buttons and naturally have low contrast on white. Source-identical glyph silhouettes and gift symbols remain recognizable at 64 px. No external gray background rectangles remain. No Unity or runtime integration is claimed.

`extract.py` reproduces the cut-outs, resizing and initial inspection sheets. It resets provenance review fields to pending, so a subsequent rerun requires renewed inspection before marking reviewed. `flag_inspect.png` is a source crop for inspecting the low-contrast flag boundary, not a runtime sprite.
