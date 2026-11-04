# T49 world-piece asset checks

Sixteen independent assets generated with the built-in image tool, one request per asset. Every request used the approved W1 screen or approved W4 screen as its explicit style reference. Exact prompts and source paths are stored in each world's `generation.json`; original generated files are copied into its `raw/` directory.

Final files: `PerchSort/Assets/Resources/World/{branch_left,twig_left,tree_crown,trunk_left}_w{1,2,3,4}.png`. Branch canvases are 620x90, twig canvases 400x80, crowns 600x400, trunks 160x1920. All sixteen are RGBA with genuine zero-alpha exterior pixels. Generated low-alpha background speckles below alpha 32 were removed; foreground alpha was preserved. Preparation used Pillow only for cutout, trimming, resizing, alignment and QA compositing. No artwork was drawn programmatically.

Each `metadata.json` records alpha bounds, source crop, transparent pixel counts, pivots, crown attachment and branch landing interval. Coordinates use top-left pixels; Unity pivots use bottom-left normalized coordinates. Crown attachment coordinates were visually adjusted after initial preparation. Branch top heights are W1 17, W2 9, W3 22, W4 30; twig heights are W1 18, W2 19, W3 8, W4 22. Snow/bamboo contours vary by up to 3 pixels across the conservative recorded landing intervals; these are texture contours, not perfectly planar collision geometry. W1 wider verified intervals are recorded in its metadata.

The four `qa_scene.png` images were visually inspected over the parent's matching backgrounds, with alternating branch rows and side-support trunks cropped beneath crowns. These are static composition studies without birds/UI. They establish alpha edge and environmental connection readability, not foot contact, capacity, gameplay placement or Unity acceptance.

`qa_64px_grayscale.png` shows all pieces in color and grayscale within 64px bounds. Bamboo and snowy crowns retain clear structural differences; spring oak versus maple is less reliable at 64px in grayscale. Thin branch/trunk silhouettes are intentionally similar across worlds; grayscale alone does not reliably identify their world. These decorative pieces are not species-identification controls.

No retries, game-code changes, Unity execution, commits or merges were performed by this helper. Runtime import, pivots in Unity, in-scene bird foot contact and final target-resolution acceptance remain for integration.
