# Plush UI art fixes

The assigned art scope follows the binding `CLAUDE.md` lettering rules and the user's explicit asset request. No code or metadata changes are owned by this task.

| Asset | Required output | Visual contract |
| --- | --- | --- |
| `Art/labels/label_title_level_<N>.png`, N = 1..40 | Forty RGBA PNGs, 2040 × 360 | Exact text `Level N`, generated together with the green velvet ribbon; match `label_title_daily_gift.png` lettering size, outline, lighting and two leaf clusters. |
| `Art/labels/label_word_best.png` | RGBA PNG, height 150 | Painted `Best` alone, dark brown, no plate; cap height 100, baseline 126, matching brown numeral art. |
| `Art/panel_wood.png` | RGBA PNG, 800 × 540 | Remove corner leaves while preserving honey-orange plush frame, cream area, thickness and 9-slice geometry. |
| `Art/card_wood.png` | RGBA PNG, 640 × 640 | Same constrained removal as the panel. |

Existing measurements: Daily Gift alpha bounds `(18,39)-(2022,320)`; panel bounds including leaves `(4,21)-(796,519)`; card bounds including leaves `(15,4)-(625,636)`. Brown 2 and 8 have alpha height 100, y 26..125. Existing slice borders (left, bottom, right, top) are panel `(150,100,150,110)` and card `(120,115,120,120)`; metadata remains untouched.

Use the built-in image generation tool with the existing assets as references, generating multiple complete ribbons per image. No font rendering. Geometry-only processing may crop, size and place painted assets. Inspect every level's text at 300%. Record actual source resolution and any limits honestly.

Required evidence: `art/sheet_levels.jpg` contains all forty levels. `art/sheet_best_panels.jpg` compares old/new panels on light/dark backgrounds and places Best beside brown digits 28. Preserve originals only in Git and temporary ignored build output. Commit and push only `plush-fix-art`.
## Production prompt set

All calls used the built-in image generation tool with `transparent_background: true`. The reference images were viewed before editing. The first four-title trial was discarded in favor of two-title sheets.

Twenty two-title calls used the Daily Gift image (first pair) or a geometry-only sheet containing two unchanged copies of it (remaining pairs). Substitute consecutive pairs `(1,2)` through `(39,40)` below:

> Use case: text-localization. The supplied image is the exact two-row EDIT TARGET. Preserve both ribbon silhouettes, green color, fine plush velvet texture, two leaf clusters, lighting and relative proportions exactly. Change ONLY the painted wording. Top ribbon text exactly "Level N"; Bottom ribbon text exactly "Level N+1". Cream-white thick rounded puffy painted letters with dark-green outline and soft shadow. Match the Daily Gift cap height and letter thickness exactly: about 195 pixels high on a 2040x360 tile. Text stays centered; shorter phrases leave more empty ribbon at the sides. Paint the whole lettering together with its ribbon, never fonts. Real alpha transparency outside both ribbons, clean cutout edges, no particles or halos. Highest resolution: requested 4080x1440 (two 4080x720 tiles); do not add captions or any other text.

The common ribbon surface used for final size registration was generated with:

> Use case: precise-object-edit. Exact EDIT TARGET supplied: this green velvet Daily Gift ribbon. Remove ONLY all lettering and its dark green outlines/shadows, seamlessly filling the former letters with the SAME green velvet ribbon surface. Keep the entire remaining ribbon exactly unchanged: identical wide aspect ratio, identical rounded padded green bar, identical two end leaf clusters with the exact leaf shapes and positions, same green hue, fine plush velvet texture, top-left warm light, transparent outer silhouette and original canvas padding. The output is one perfectly BLANK ribbon, no words, no letters, no symbols, no decoration added. Do not change size, proportions, shorten the bar, move leaves, or crop any part. Actual transparent alpha outside the ribbon. Highest available resolution for downscaling; target 4080x720.

The panel and card were separately generated with:

> Use case: precise-object-edit. EDIT TARGET is this existing UI panel sprite. Remove ONLY the green leaf clusters at its upper-left and lower-right corners, filling their places with the continuous honey-orange velvet frame and transparent exterior as appropriate. Preserve everything else exactly: same 800:540 canvas proportions, same scale and location, same rounded-rectangle silhouette and corner curves, same 50-60 pixel frame thickness (at 800 width), same cream interior with the SAME bounds, fine plush velvet texture, warm top-left light, same soft shadow. The panel body spans x=4..796, y=38..519 on the 800x540 reference. Cream interior spans approximately x=55..741 and y=96..461. Maintain these proportions and positions. No leaves anywhere, no foliage, no extra decoration, no text, no stitches, no fabric weave, no wood grain, no plastic. RGBA with real transparency outside the panel. Requested resolution 3200x2160, as large as available for clean downscaling. This is a constrained leaf-removal edit, not a redesign.

> Use case: precise-object-edit. EDIT TARGET is the supplied existing square UI card. Remove ONLY the green leaf cluster at the upper-left corner. Fill its place with a continuous rounded honey-orange velvet frame and true transparent exterior where the leaf extended beyond the frame. Change nothing else. Preserve exact square canvas proportions, original panel scale/position, frame width about 57 pixels, rounded corner geometry, warm cream center area and its bounds, fine short plush velvet texture, soft top-left warm lighting and shadows. At the original 640x640 size the body bounds are x=16..625, y=49..636; center cream bounds approximately x=74..565, y=108..580. Preserve those geometry ratios, no change in frame thickness. No leaves anywhere, no text, no decorations, no seams, no stitching, no wood grain, no plastic. RGBA transparency outside the card. Request 2560x2560 high resolution for downscaling. This is a constrained leaf removal, not a redesign.

The standalone word used the existing brown 2 and 8 as material references:

> Use case: stylized-concept. Paint the exact word "Best" by hand as one transparent game UI word sprite, capital B then lowercase e s t, with clear complete letters. The two supplied brown numeral PNGs are the binding material, hue and stroke style references. Match their dark chocolate-brown rounded puffy painted strokes, subtle warm top-left highlight and softly shaded edges. The word sits on a shared straight baseline; a full round capital B with two counters, round lowercase e, soft curved s, lowercase t with a clear crossbar. Thick smooth soft embossed lettering, gentle velvet finish, no shiny plastic. Word alone, entirely freestanding on real transparent alpha. NO plate, NO ribbon, NO panel, NO underline, NO decorative elements, NO numerals or punctuation, NO external shadow plane. Do not reproduce the reference digits. Need a very large clean source at least 2048x768 for downscaling to a cap height of 100 pixels on a 150-pixel-high canvas. Preserve natural word proportions and spacing. Exact text: "Best".

Whole painted phrases, including their outlines and surrounding ribbon material, may be geometrically registered to the common ribbon to match the measured reference lettering height. Individual characters are never constructed or assembled. Final numerical measurements and source-resolution limitations are recorded in the report.
