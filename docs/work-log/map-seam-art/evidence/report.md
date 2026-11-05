# Map seam art delivery

## Delivered art

Completed all assigned boundaries in top-to-bottom order:

- World 4 bottom to world 3 top: snow to autumn.
- World 3 bottom to world 2 top: autumn to bamboo.
- World 2 bottom to world 1 top: bamboo to meadow.

Four derived `map_bg_continuous_w1.png` through `map_bg_continuous_w4.png` files live in `PerchSort/Assets/Resources/Art/`, each 1080 × 3000 RGBA. Original PNGs remain unchanged. Each committed copied importer has a new unique GUID; all other metadata is identical to its source. The current importer rewrites its size limit at import time, as documented below. No game code, loader, layout, rules, levels, saves or approved reference images are changed.

The endpoint centre is **543.5 px**, width **190 px**, left edge **449 px**, right edge **638 px** on both sides of all three boundaries: zero centre or width error. Every corresponding pixel of upper rows 2840–2999 and lower rows 0–159 has identical RGB, including ground, path colour, texture, plants and lighting. Maximum per-channel RGB error is **0**. Original alpha is retained: the lower strip fades from transparent to opaque over those same 160 rows.

## Measurements before repainting

Coordinates start at the image top-left. These are medians over all 160 rows of each edge band, not a measurement of an arbitrary screenshot. Width includes both edge pixels. The sand mask is the contiguous interval containing x=540 with R > G+18, G > B+30 and R > 200; soft antialiased grass fringe is excluded. RGB is measured independently of alpha. Every row, range and source hash is recorded in [`measurements.json`](../../../../art/map_seams/measurements.json).

| World | Top centre x | Top width | Bottom centre x | Bottom width |
| --- | ---: | ---: | ---: | ---: |
| Meadow | 544.5 | 185.5 | 545.5 | 191 |
| Bamboo | 543 | 188 | 544 | 192 |
| Autumn | 541 | 186 | 542.5 | 189 |
| Snow | 541 | 186 | 542 | 189 |

## Painting and preservation

Codex built-in image generation painted one contextual panel per boundary. The unmodified generation outputs, edit inputs and exact prompts are in [`art/map_seams/raw`](../../../../art/map_seams/raw). No external image service, API image generation, or alternate model was used.

Each 1080 × 720 edit input contains upper rows 2560–2999 and lower rows 0–439 with the prescribed 160 px overlap. The generated panel is reduced from 1536 × 1024 to that size locally. Only its central 480 rows contribute to the deliverable. Local Pillow/NumPy processing aligns the generated path edges to a common 190 px profile, with a one-pixel natural contour, and feathers the outer 96 px patch margins into the original pixels. The central 160 px overlap uses the same painted pixels in both strips. This removes the repeated source corner plants at the joins and introduces low bamboo leaves/ferns, autumn leaves and melting snow appropriate to each transition. No large new props or fog bands were introduced.

The protected rows **320–2679 inclusive** are byte-identical RGBA in all four files: 2360 rows per world, with matching SHA-256 digests in [`qa/verification.json`](../../../../art/map_seams/qa/verification.json). No middle pixel is resampled. All alpha bytes are unchanged. The entire bottom 320 px of world 1 and top 320 px of world 4 are unchanged, preserving the outer continuation edges more strictly than the requested final-row match.

System Python lacked Pillow. Continued using an isolated `build/seam-tools` environment with Pillow and NumPy. Built-in generation returned a larger panel and did not preserve exact edit-mask pixels; local bounded compositing enforced the immutable rows and shared overlap instead of trusting generated geometry.

## Offline evidence

- [`stack_comparison.jpg`](stack_comparison.jpg): old and derived four-world stacks side by side at exactly 25%, world 4 above 3 above 2 above 1, 160 px overlap and 2840 px stride. Each full-resolution stack is 1080 × 11520; the standalone previews are 270 × 2880. The outer top alpha is displayed over the same green backdrop in both; continuation images are not part of this prescribed four-strip comparison.
- [`boundary_w4_w3.jpg`](boundary_w4_w3.jpg), [`boundary_w3_w2.jpg`](boundary_w3_w2.jpg), [`boundary_w2_w1.jpg`](boundary_w2_w1.jpg): 1080 × 720 crops at 100%, including 120 px of unmodified context on each end. The corresponding `_before.jpg` files show original joins. QA verifies these panels are pixel-identical to crops from the actual stacked derived strips.
- Lossless panels, full stacks and pixel checks are under [`art/map_seams/qa`](../../../../art/map_seams/qa). JPEG evidence is at most 1440 px wide, below the 1600 px limit.

The direct-overlap stack and all three full-size panels were visually inspected. The path is continuous, biome transitions use individual plants and textured ground, and the original repeated corner rosettes are removed from the joins. There is no horizontal cut in this prescribed composition. This is offline asset inspection, not runtime acceptance.

## Integration finding: the current canopy layout differs from the brief

The source measurements do not support a large endpoint path jump: source edge centres already differ by only a few pixels. The current `MapView`/`GameLayout` use **4000 px stride**, **3000 px strips**, and a **1320 px canopy crop** from the lower world's top. That canopy begins 1160 px before the lower strip, so it overlaps the upper strip by 160 px, then replays lower-strip rows 1160–1319 beneath the lower strip's rows 0–159. It is not a direct 160 px overlap between adjacent full strips.

[`canopy_layout_before.jpg`](canopy_layout_before.jpg), [`canopy_layout_derived.jpg`](canopy_layout_derived.jpg) and the [`derived internal join crop`](canopy_inner_join_derived.jpg) reconstruct that existing background mapping locally, without nodes or UI. They are explicitly **offline reconstructions**, not Unity screenshots. The derived images repair the external boundary match but cannot remove the internal canopy replay jump while their middle rows remain immutable. This evidence explains the original rectangular step more accurately than attributing it to the outer path endpoints.

The coordinator must switch the loader and reconcile this canopy/stride mapping before accepting a seamless runtime map. Changing the stride alone could affect authored node and gate layout; no such change is made or implied here. Existing top/bottom continuation resources are preserved by keeping the two outer bands untouched.

## Importer integration finding

The serialized Unity run exposed another concrete integration dependency: `GameArtImporter.cs` selects the 4096 px limit only for `map_bg_w` and `bg_level_` names. It assigns 2048 to the required `map_bg_continuous_w` names and rewrites their copied default-platform limit from 4096 to 2048. That would downsample the tall derived textures on import despite the full-resolution PNGs.

The delivery preserves the exact requested source metadata except for new GUIDs. The coordinator must include `map_bg_continuous_w` in the existing tall-map importer policy when switching the loader, then confirm full-resolution imported textures. Altering the importer here would violate the explicit no-code-change scope. The limitation is documented from observed Unity metadata, not merely inferred from a filename.

## Verification commands and status

- `uv venv build/seam-tools` and `uv pip install --python build/seam-tools/bin/python pillow numpy`: local image environment created successfully.
- `build/seam-tools/bin/python art/map_seams/qa/assemble.py 1` (then `2`, then `3`): each boundary assembled, inspected, committed and pushed before the next delivery.
- `build/seam-tools/bin/python art/map_seams/qa/verify.py`: all bounded pixel, alpha, overlap, endpoint, importer and composite comparisons passed.
- `python3 ci/check_current_game_names.py --self-test`: 9 passed.
- `python3 ci/check_current_game_names.py`: 184 tracked text files inspected, zero violations before Unity; 185 after staging the final QA script, also zero violations.
- Saved hashes and copies of 1591 tracked scene/meta/settings/package files under ignored `build/` before Unity. Initial worktree was clean.
- `flock /tmp/unity.lock bash scripts/run_tests.sh`: passed after waiting for the shared lock. Scene build succeeded; **317/317 EditMode** and **95/95 PlayMode** passed, zero failures, skipped or inconclusive cases. EditMode includes 250 game, 53 Meta and 14 Audio cases; LevelsData retains 81 and Layout 41. These counts come from this run, not the historical baseline.
- `flock /tmp/unity.lock bash scripts/screenshots.sh`: passed; **34/34** PNGs at 1080 × 1920 under `build/screens/game/`. The four-world [`unity_map_baseline.jpg`](unity_map_baseline.jpg) contact sheet was visually inspected. The unchanged loader selects original strips; these are baseline captures, not new-art runtime acceptance.

No device, timed animation, audio, persistence or integrated new-art runtime acceptance is claimed; none is changed by this image task. Exact suite timestamps, result hashes, screenshot hashes and cleanup evidence are recorded in [`qa/unity_verification.json`](../../../../art/map_seams/qa/unity_verification.json). The PlayMode shutdown log reports three persistent allocation leaks despite a passing suite and exit code; no failure or skip was hidden.

After Unity exited, inspected and restored **786** generated files to their recorded pre-run bytes: 768 whitespace-only metadata changes, 17 importer/serialization changes and the rebuilt scene. The 17 include four new-map size-limit rewrites, seven expanded bird-overlay metadata files and six unrelated gate-door pivot rewrites. Observed generated copies remain in ignored `build/unity-after/`; the pre-run copies remain in `build/unity-before/`. No primary checkout was touched. Final offline pixel/importer comparisons passed again after cleanup. The suite results describe the generated scene/import state during the run; they do not establish final integrated new-art acceptance.

## Commits and push recovery

- `5e0492c`: bamboo to meadow.
- `902d85c`: autumn to bamboo.
- `26a806b`: snow to autumn.

All three boundary commits are pushed to `origin/mac-map-seam-art/work`. The first ordinary push failed on GitHub's remote temporary pack-file write/rename step. Retrying with `git -c http.version=HTTP/1.1 push --no-thin -u origin mac-map-seam-art/work` succeeded; later boundary pushes used the same transport successfully. No force-push or peer-branch merge was used.
