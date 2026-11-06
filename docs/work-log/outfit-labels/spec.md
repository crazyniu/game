# Outfit name ribbons

Deliver the 23 exact painted outfit labels listed in the [text inventory](../plush-ui/text-inventory.md#outfit-ribbon-additions) under `PerchSort/Assets/Resources/Art/labels/`. Use the built-in image generation tool to paint the words and ribbon together. No fonts or procedural lettering.

Inspect the existing Straw Hat, Cozy Scarf, Round Glasses and Flower ribbons before generation. Preserve their 1368 × 294 RGBA canvas family, fine velvet texture, cream-white puffy lettering with dark-green outlines, warm upper-left light and two three-leaf clusters. Use Straw Hat as the consistent scale reference; the existing four images have different occupied heights. Crop and scale generated pixels only, preserving generated alpha. Request at least twice the final resolution and report the actual output resolution.

Review every letter and i-dot at 300%; regenerate spelling errors. Append the supplied outfit IDs in their exact order, and provide `docs/work-log/plush-ui/outfit-labels.jpg` containing all 27 labels. Preserve the four original PNGs.

Coordinator correction during execution: do not create `.meta` files. Unity generates metadata and `GameArtImporter` applies label settings.

Run naming guards and applicable serialized Unity checks; report unavailable execution honestly. Commit and push only `outfit-labels`.

## Generation prompt set

One built-in edit call per label, with `label_card_straw_hat.png` as the sole edit reference and `transparent_background: true`. Substitute each exact inventory string for `{EXACT OUTFIT NAME}` in this prompt:

```text
Use case: text-localization. Edit the supplied Straw Hat ribbon. Change only the painted words to "{EXACT OUTFIT NAME}". Paint the lettering together with the green velvet ribbon, not typeset. Match the reference exactly: same narrow horizontal silhouette, three plush leaves on each end, same leaf sizes, creamy-white thick round puffy letters with dark-green outline, warm upper-left lighting, short fine velvet pile. Keep the original letter height, baseline, dark outline and generous spacing. No added icons, objects, flowers, stitches, knitting or extra decorations. One single line, exact spelling "{EXACT OUTFIT NAME}", clearly separated letters and every i dot. Actual transparent RGBA, no checkerboard or matte. Output a high-resolution 4096x1024 image, ribbon spanning 4000px width, about 465px tall, centered vertically; enough real rendered pixels for at least 2x downscaling to the 1368x294 target. Preserve the full leaf tips.
```

Blue Bow Tie used the same visual constraints with a requested 2736 × 588 canvas; all later labels requested 4096 × 1024. These are requested sizes, not a claim about native output dimensions.
