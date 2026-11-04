# T31-A top limb art (Codex, gpt-6-astra)

Branch [t31-v5-layout](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T31_layout_art.md#L3), worktree `/home/developer/Projects/game`. Read `docs/GAME_DESIGN.md` section 3 and 4 and the existing `tasks/T30_art_report.md`. Style must match the existing v5 art (plush 3D cartoon, same leaf greens and wood tones as `bg_forest_day.png`, `branch_left.png`, `birdhouse.png`).

Why: the birdhouse currently floats in the sky. It must hang from something.

Ownership: create only `PerchSort/Assets/Resources/Art/limb_top.png`, files under [art/v5/](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T31_layout_art.md#L7) (raw, prompts, QA), and append a short section to `tasks/T30_art_report.md`. Touch nothing else. No commit.

Deliverable `limb_top.png`, 1080x320 RGBA, transparent background:
- a thick natural tree limb entering from the top-left canopy and the top-right canopy and crossing the top of the screen in a gentle sag, clearly joined to leaf clusters at both ends so it visibly grows from the same canopies as `bg_forest_day.png` (look at that image: leaf clumps in the top-left and top-right corners);
- a short rope or vine wrapped once around the limb at the horizontal center (x=540) with a hook/knot whose lowest point is at y=235, exactly where the birdhouse rope loop will attach;
- the limb's lower edge no lower than y=260 except the rope; nothing below y=260 besides the rope end;
- some leaves hanging over the limb for depth, no birds, no text.
Verify and report: exact size, RGBA, alpha range, fully transparent below y=260 except the rope, rope hook lowest opaque pixel at (540,235) +-4 px, and a composite preview [art/v5/qa/limb_top_composite.png](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T31_layout_art.md#L14) over `bg_forest_day.png` with `birdhouse.png` placed so its top rope loop meets the hook (house center y=405). Say honestly if any check fails.
