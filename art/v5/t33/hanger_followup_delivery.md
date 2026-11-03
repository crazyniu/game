## Follow-up: four independent birdhouse hangers

Task `task_87cf6b4dda30`, 2026-11-03. The coordinator explicitly authorized two additional derived assets and this report append on the shared `t35-levels` worktree. **2/2 requested files delivered; missing: none.** T33 now has 110 delivered runtime PNGs across its original batch and this follow-up; the original 108-file manifest and checks above describe the earlier batch.

| New asset | Dimensions | Mode | Alpha min/max | Verification |
| --- | --- | --- | --- | --- |
| [limb_top_plain.png](../PerchSort/Assets/Resources/V5/Art/limb_top_plain.png) | 1080x320 | RGBA | 0/255 | Central rope/knot removed; repaired wood; all pixels outside the local repair rectangle unchanged |
| [rope_segment.png](../PerchSort/Assets/Resources/V5/Art/rope_segment.png) | 24x128 | RGBA | 0/255 | Top and bottom RGBA rows exactly equal; opaque rope present on every row; transparent 3px side gutters |

The plain limb starts with the original `limb_top.png` pixels. A generated local wood repair is registered to the existing limb edges and feathered only inside **x=510..569, y=95..249**. Exactly **5,894** pixels changed, within actual bounds **[511,104,569,242)**; the other **339,706** pixels match the original byte-for-byte. The old loop and its faint alpha fringe are removed, and the resulting limb has no nonzero alpha below **y=170**, comfortably satisfying the y=260 limit.

The rope matches the existing golden-tan twisted material. Processing chooses matching twist phases, normalizes the segment to 128px high, and blends six rows at each end before making the first and last RGBA rows identical. [Four-repeat light/dark tiling QA](../art/v5/t33/qa/rope_vertical_tiling.png) shows the seam at native proportions; [limb light/dark QA](../art/v5/t33/qa/limb_top_plain_light_dark.png) checks the repaired cutout.

[limb_multi_house.png](../art/v5/t33/qa/limb_multi_house.png) shows **four existing birdhouses**, each scaled to 180x204 and suspended from a separate tiled rope behind the plain limb. The ropes join the existing little roof loops. [Detail view](../art/v5/t33/qa/limb_multi_house_detail.png) and [repair close-up](../art/v5/t33/qa/limb_plain_repair_detail.png) are supplied for review. This is a static asset composition, not a Unity screenshot.

[Follow-up verification](../art/v5/t33/qa/hanger_followup_verification.json) reports **PASS** for sizes, RGBA, alpha extrema, pixel identity outside the repair, removal of the center hanger, the y=260 limit, exact rope edge equality, every-row rope continuity and transparent side gutters. All **142 pre-existing runtime PNGs** match the [follow-up baseline](../art/v5/t33/qa/hanger_followup_baseline.json), including the original limb, birdhouse and prior T33 deliveries. The first staged check detected a faint residual rope fringe; it was corrected before either runtime file was created, then all checks passed.

Built-in `image_gen.imagegen` supplied both raster generations. Exact prompts, reference paths and source provenance are retained in [limb repair JSON](../art/v5/t33/raw/limb_center_plain_followup.json) and [rope JSON](../art/v5/t33/raw/rope_segment_followup.json), alongside their raw PNGs and two derived input crops. [process_hangers.py](../art/v5/t33/tools/process_hangers.py) reproduces the local repair, periodic rope, composites and verification:

```bash
python3 art/v5/t33/tools/process_hangers.py --publish
```

Publication creates absent files exclusively and refuses a differing destination. No existing artwork, code, scene, metadata or runtime data was overwritten, and no commit or push was made. Unity import/rendering remains untested on this Linux machine; no runtime animation change is claimed.
