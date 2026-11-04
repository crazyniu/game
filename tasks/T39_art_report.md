# T39 art report

Delivered **2/2 requested PNGs** for [T39_art.md](T39_art.md); missing files: **none**. No existing artwork or code changed; no commit or push.

| Asset | Size | Mode | Alpha min/max | Visible bounds (exclusive maximum) |
| --- | --- | --- | --- | --- |
| [note_full.png](../PerchSort/Assets/Resources/Art/note_full.png) | 64x64 | RGBA | 0/255 | [10,6,53,58) |
| [note_empty.png](../PerchSort/Assets/Resources/Art/note_empty.png) | 64x64 | RGBA | 0/102 | [10,6,53,58) |

The full eighth-note has glossy warm gold, a brown outline and small pale highlights. The empty state has a matte brown-grey outline/interior and exactly **40%** of the full state's alpha, rounded to 8-bit values. Its lower contrast remains readable on the existing cream pill.

Both files share the same crop, 43x52 content rectangle and placement at (10,6). Their nonzero-alpha silhouette masks are pixel-identical, and their bounding-box center is (31.5,32), within half a pixel of the canvas center. Swapping them in the same 64x64 rect preserves registration; a centered pivot is intended, but no importer metadata was changed.

## Verification

[verification.json](../art/v5/t39/qa/verification.json) reports **PASS** for dimensions, RGBA, alpha ranges, identical bounds/silhouettes, the exact 40% alpha relationship, zero RGB under fully transparent pixels, and preservation of all **144 pre-existing runtime PNGs** recorded in [baseline.json](../art/v5/t39/qa/baseline.json). SHA-256 hashes are included per final file.

Visual inspection of the native 64px icons, cream-pill composition and grayscale review passed: the states are distinguishable, the single eighth-note reads clearly, and no baked background, checkerboard or surrounding halo is visible.

- [song_pill.png](../art/v5/t39/qa/song_pill.png): existing `pill_cream.png` scaled proportionally to 384x132, with five 64px slots — **2 full, 3 empty**.
- [Light/dark contact sheet](../art/v5/t39/qa/contact.png).
- [Grayscale review](../art/v5/t39/qa/grayscale.png).
- [Matching silhouette masks](../art/v5/t39/qa/matching_silhouettes.png).

## Provenance and reproduction

Built-in `image_gen.imagegen` produced the gold icon and its dim material edit using this project's accepted music icons as references. [Exact prompts](../art/v5/t39/prompts.md), raw PNGs and per-generation JSON records are preserved under `art/v5/t39/raw/`.

[process_notes.py](../art/v5/t39/tools/process_notes.py) removes low-alpha generation residue, registers both material states against a shared alpha silhouette, fills sparse missing edit-edge color from the generated brown-grey outline palette, applies 40% alpha to the empty state, creates the composites and checks original-file hashes. Publication uses exclusive creation and refuses any differing existing destination.

```bash
python3 art/v5/t39/tools/process_notes.py --publish
```

Unity import, actual UI rendering and runtime state swapping were **not run** because the Linux development machine has no Unity Editor. These are static raster checks, not runtime acceptance.
