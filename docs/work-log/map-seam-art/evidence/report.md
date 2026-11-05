# Map seam art delivery

## Boundary progress

- Finished: world 2 bottom to world 1 top (bamboo to meadow).
- Pending: world 3 bottom to world 2 top; world 4 bottom to world 3 top.
- The first boundary shares byte-identical RGB across all 160 overlap rows. Both protected middles remain pixel-identical. Alpha is copied from each original.
- Built-in Codex image generation supplies the repaint; local Pillow/NumPy resize only the generated panel, align its sand edges, feather the outer patch margins, and copy the shared overlap. No source middle is resampled. Prompts and unmodified generation outputs are under `art/map_seams/raw/`.

## Source findings and method changes

Source edge path centres already differ by only a few pixels. `MapView` uses a 4000 px stride with 3000 px strips and a 1320 px canopy crop, not a direct 160 px strip overlap. The requested overlap composite is an asset contract, not proof of the current runtime composition. Loader and layout changes remain with the coordinator; no runtime code is changed here.

System Python lacked Pillow. Continued using an isolated `build/seam-tools` environment with Pillow and NumPy. Only built-in image generation and local image processing are used; no external image service or API fallback.

## Verification status

Measurements recorded before painting in `art/map_seams/measurements.json`, including every measured source row and the colour threshold used. First-boundary byte comparisons pass. Full Unity suites and final composites will follow completion of all boundaries.
