# Final bird visual review — 2026-11-04

- Inventory: 170 frames (17 per species), ten portraits, all RGBA with alpha extrema 0–255. Frame canvas 320x320; portrait canvas 160x160. See `export_inventory.json`.
- Alignment: feet anchored to y=300, idle foot span used to maintain pose scale; Berry retains reviewed torso scale despite wider replacement plumes. `alignment.json` records all 170 frames, none edge-clipped.
- Pose review: species sheets reviewed at approximately 120px body width. Opposing tilts read clearly, body size remains consistent through swaps, and flight positions differ. Per-species GIFs are offline frame sequences, not playable animation evidence.
- Recognition: `all_species_64px_grayscale.png` uses 64px visible sprite width. Sunny curl, Skye bun, Berry two swept plumes, Minty leaf/eye rings, Cocoa ear tufts/mask, Flurry light body/dots, Ember flame crest, Inky star, Hoot nightcap and Rook beak/compass remain distinguishable in grayscale. This is a visual review, not a user recognition study.
- Identity sources and prechecks are in `head_feature_review.md`. Rook follows K7's left-facing default view; record this placement exception rather than claiming uniform facing.
- Full K3/K7 rebuilt-world composites and runtime acceptance remain with integration after mac-art's world handoff. No tests or Unity were run by this worker.
- Eggs, overlays and world pieces are outside current ownership and were not edited.
