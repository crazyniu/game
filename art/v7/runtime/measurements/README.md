# T48 layout measurement evidence

`layout_draft.json` is a measured input for the final `art/v7/layout_v7.json`, not the final runtime contract. It contains 17 approved portrait screens and 223 named element bounds. Every screen records its source. The icon atlas is excluded from portrait geometry because it is 1536 x 1536.

## Method

The approved 1080 x 1920 images were inspected at 540 x 960. Bounds were manually recorded at that inspection scale and multiplied by two. Each element supplies `center`, `size`, and `rect` (`x, y, width, height`), in source pixels from the top-left. Decorative silhouettes are included; broad diffuse light is excluded except where explicitly named. Ordinary edges have approximately 8 px uncertainty; foliage, branch extents, and glow approximately 16 px. These are art bounds, not interaction hit areas or a claim of pixel-exact segmentation.

The source inspection sheets are `group0.jpg` through `group5.jpg`. Each `*_bounds.jpg` overlays the resulting rectangles for review. `build_draft.py` reproduces the JSON and overlays using Pillow for inspection only; it generates no artwork.

## Evidence boundaries

- The approved K3 master shows one house. The approved K7 screen shows seven houses on a beam. These facts are retained explicitly. K1 through K6 multi-house arrangements in `house_count_adaptation` are inferred centered subsets of K7, not measurements from nonexistent approved screens. The larger single house in the K3 master is also available as observed geometry.
- Three bottom buttons are measured from the K3 master; four are measured from K7. K7 also has five main branch rows and a separate scarecrow perch. K3 has four branch rows.
- Bird positions describe the displayed artwork only. They do not constitute gameplay data, capacities, or level authoring.
- Map concept positions cover its six visible nodes in 1080 x 1920. The four delivered 1080 x 3000 world backgrounds now have 40 actual path-node measurements in `art/v7/runtime/map/world_paths.json`, integrated into `layout_v7.json` under `map`. Their 160 px baked top alpha fade requires a 2840 px stacking stride and a topmost viewport clamp or opaque-source backing. The draft retains its historical pending field; the final layout reads the completed map measurements directly.
- T48 corrections remain explicit: Minty's gift uses a watering can; Scarecrow uses the purple round-disc family. Measurements preserve their intended UI bounds, not the incorrect concept contents.
- No Unity, gameplay, hit testing, animation, or runtime acceptance is claimed.

The final layout integrates delivered asset slicing borders, house roof/door/rope/beam anchors and daily-gift lid/foreground-occlusion alignment. These sprite-local coordinates are separate from measured source-screen positions. Observed screen geometry and inferred K1–K6 house adaptations remain explicitly distinguished. Gate assembly metadata is integrated, including 1024 x 1024 shared-canvas alignment, door hinges and Unity pivots, layer ordering, static open projections and the existing light sprite draw rectangle. Static artwork delivery is complete; runtime integration and animation remain unverified.
