# T49 addendum: six colored booster illustrations

Scope: six new 256×256 RGBA illustrations in `PerchSort/Assets/Resources/Art/booster_*_illus.png`. No originals, code, scenes, import settings or existing assets were changed. Unity was not opened. No git commit was made.

## Generation and provenance

- Built-in `image_gen.imagegen`, transparent background enabled. No CLI/API generation fallback.
- Every call supplied both approved screenshots as style references: `art/approved_ui/ui_shop_boosters.png` and `art/approved_ui/ui_unlock_card.png`.
- Six initial calls and one targeted owl-eye retry. Exact prompts are in `prompts/`. All seven original generated outputs are preserved in `raw/`; original source paths and final hashes are in `qa/metrics.json` and `prepare.py`.
- The first owl-eye image had a connecting translucent glow plane and was rejected. The retry separates the amber eye and rays with genuinely transparent gaps.
- Subjects: glowing bulb; branch with green plus; egg in nest with amber eye; ringing brass bell; grey crow with red cross; rain cloud, droplets and sprout.
- The green plus and colored amber eye follow the dispatched addendum, while the reference shop uses white symbols. No UI discs, cards, text or numbers are baked into the six production assets.

## Preparation and static review

Run `uv run --with pillow python art/approved_ui/world/boosters/prepare.py` from the repository root to reproduce preparation and QA. Pillow only crops the generated alpha cutout, resizes it, centers it and creates QA composites. It does not draw or synthesize production artwork.

The raw tool outputs contain extremely faint distant alpha noise. Crop bounds use alpha >16 to exclude it; alpha values within the crop remain unmodified. Each crop is fit proportionally inside 224×224, centered on a transparent 256×256 canvas, using Lanczos resizing. Recommended normalized pivot is (0.5, 0.5); no Unity import settings were changed. The generated opaque interiors are mostly alpha 252–254, preserved from the tool rather than forced opaque.

All six files were checked for RGBA mode, required dimensions, nonempty alpha, clear canvas margins and antialiased edges. Final alpha bounds are recorded in `qa/metrics.json`. Visual review of `qa/contact_light_dark_64_grayscale.png` found clean light/dark silhouettes and distinct subjects at 64px in color and grayscale. The crow's red cross remains a distinct X in grayscale; the owl eye remains distinct above the egg; rain cloud, drops and sprout remain separate readable forms.

`qa/approved_shop_comparison.png` puts approved reference crops above the new illustrations composited on the existing approved `btn_purple.png` asset. The shop and unlock static overlays show approximate contextual size and color against the corresponding approved screens. Their explicit cream QA masking rectangles hide the original illustrations; these masks and the purple button are not part of any delivered illustration. These images are compositing studies, not seamless screen replacements or screenshots of Unity. The shop QA mask partially covers adjacent count badges and price edges, so it does not establish final layout clearance.

Result: static asset preparation and visual QA complete, pending coordinator/user acceptance and future runtime integration. Runtime scale, actual code-generated disc layering, device screenshots and animation were not tested and are not claimed.
