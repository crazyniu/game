# Connected-tree detail polish

The user accepted the connected-tree route direction and asked to optimize details, including
star size. This is a restrained layout refinement, not permission to redraw accepted artwork
or recomposition of the tree. No new art is required: all existing source pixels remain intact.

The current 54-pixel stars with 4-pixel gaps are visually underweight against 380-pixel branch
faces. Independent review recommends 64-pixel visible star width and 8-pixel gaps, producing
a centered 208-pixel group. Full and empty stars preserve their own aspect ratios and align
by their actual lower alpha edge to the same branch plane. This improves narrow-screen
readability while leaving 86 logical pixels of face margin on each side.

Numerals remain 64 pixels, centered by their full visible ink. Their measured alignment,
original bird registration and 176 x 154 animation limit, tree/header/root geometry and normal
modal layer order already pass; inspect them again rather than making gratuitous changes.
No glow, additional shadow, badges, extra animation or new controls are introduced.

Preserve all 100 level IDs, hit-region geometry, callbacks, save behavior and approved world
selector. Extend tests to assert actual star gaps, bottom alignment, group centering, original
aspect ratios and larger status clearance. Validate both languages, all ten worlds, current
levels 1/8/10/100, 360-pixel rendering and other established window ratios. Independent review
and real native screenshots are required before delivering the baseline-relative increment.
