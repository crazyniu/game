# T49 delivery report — in progress

## Scope and method

Branch: `mac-birds/t49`. Built-in image tool only. All four approved v7 references were inspected. Individual character idle anchors reference approved boards/cards; action frames reference their species anchor and approved art. Pillow is used only for alignment, portrait cropping and QA. Original assets remain unchanged. Unity was not opened; no runtime acceptance is claimed.

The fetched T49 addendum requests 17 frames per species plus a portrait, with Sunny, Skye and Berry delivered first. Full scope is 170 bird frames, 10 portraits, 2 overlays, 5 egg frames and 32 world pieces/backgrounds.

## First checkpoint

Sunny: all 17 source frames generated, including nine additional idle actions. One fluff source with an unwanted halo was rejected and regenerated. Runtime sprites use 320x320 RGBA canvases, toe baseline y=300; face crops are 160x160. Art is still subject to lead visual review. A frame strip and an animated idle-action GIF are included under `art/v7/birds/qa/`.

Skye, Berry and Minty sources are in progress; incomplete strips mark missing actions PENDING. Later species, eggs/overlays and world pieces are pending. K3/K7 rebuilds and ten-species grayscale comparison remain pending.

## Alignment and limitations

`art/v7/birds/qa/prepare_birds.py` records source foot bounds, scale, translation and visible bounds in `alignment.json`. All species use a 195 px idle silhouette width, then per-frame toe-span registration. Feet are anchored at x=160/y=300. Body proportions and head action readability still need visual review; numeric alignment is not proof of exact reference fidelity.

GIFs are offline frame-swap previews, not Unity animation evidence. No Unity tests, game screenshots, runtime timing or integration were run. Full task acceptance is pending.

## End-of-day wrap-up

Stopped at the lead request before T49 completion. See `docs/team/status/mac-birds.md` and `art/v7/birds/qa/wrap_inventory.json` for the exact source/export/missing-file inventory. Partial QA strips and GIFs are deliberately retained as WIP, not accepted deliverables. All saved owned files are included in the final `wip:` checkpoint; no new generation, normalization or Unity work is started during wrap-up.

## 2026-11-04 Skye checkpoint

Completed 17 source frames and 17 aligned exports plus portrait, strip and idle-action GIF. Replaced wing-opening quirk2 with the sleepy head tilt, corrected right-tilt direction, and shortened the stretched wing after detecting clipping. Sunny remains unchanged. No Unity run.

## Lead review corrections — 2026-11-04

Regenerated Sunny quirk1 to retain idle body size and baseline, and both Sunny/Skye tilt pairs with clear opposing approximately 20-degree head tilts. Refreshed aligned exports, strips and GIFs. `art/v7/birds/qa/review_scale_tilts.png` provides the idle/quirk/tilt comparison at approximately 120 px body width; visual inspection found the quirk shrink corrected and both tilt directions readable. The alignment inventory has 40 frames and zero edge-clipping flags. Runtime playback remains untested, as Unity is outside this worker's scope. The same body-scale and tilt requirements apply to all remaining species.

## Berry checkpoint — 2026-11-04

Completed all 17 Berry sources/320x320 RGBA exports and portrait. Corrected crest-preening quirk2, regenerated right tilt for visible opposing head roll and rejected/regenerated a halo-bearing yawn. Full 17-frame strip and idle-actions GIF refreshed; visual strip review found distinct actions and consistent body scale. Alignment inventory: 52 frames, no clipped exports. Runtime playback and final board comparisons remain pending.

## Berry head-feature correction — 2026-11-04

Lead rejected the first set's upright crest. Regenerated every Berry source frame using `ui7_aviary_detail.png`, `ui7_level_k3_v2.png` and `ui7_aviary_list.png`: exactly two long separated plumes now arc backward, matching the large detail reference. Preserved expressions, poses and readable opposing tilts. Rebuilt all 17 exports plus portrait, strip and GIF. `berry_game_size.png` and `berry_head_reference.png` document the comparison. The prior reviewed torso scale is retained via its 98.311 px toe span, avoiding shrinkage from the wider plume silhouette. Visual game-size review and alignment found no export clipping; runtime remains untested. Minty generation was cancelled to prioritize this correction; its head features were checked against the popup before resuming.

## Minty checkpoint — 2026-11-04

Completed 17 source frames, 17 RGBA exports, portrait, strip, idle-actions GIF and game-size sheet. Head-feature comparison used the large approved popup (central green leaf with two teal side feathers and pale eye rings). Regenerated downstroke after the first result shrank the torso and lowered wings below the feet. Game-size review found consistent scale, leaf identity and distinct actions. All 68 prepared frames have no edge-clipping flags. Added documented prechecks of remaining existing idle head features. No Unity/runtime validation performed.

## Scope transfer — 2026-11-04

Lead transferred T49 item 4 (all backgrounds, branches, twigs, crowns and trunks) to mac-art. mac-birds now owns remaining bird sets, eggs and mama-bow/chick-fluff overlays. No world pieces were generated or edited by this worker. Full board composites depend on mac-art world assets and remain an integration handoff dependency.

## Cocoa checkpoint — 2026-11-04

Completed 17 frames and portrait, with strip, idle-actions GIF and game-size QA. Preserved K7/list head features: two short pointed ear tufts, cream mask and heavy brows. Checked clean transparency on the QA background, consistent body scale and baseline, readable opposing tilts. Alignment inventory: 85 frames, none clipped. No runtime acceptance claimed.

## Flurry checkpoint — 2026-11-04

Completed 17 frames, portrait, QA strip, GIF and game-size sheet. Head features checked against K7 and aviary list before generation: lilac two-feather crest, three forehead dots and long lashes. Replaced an erroneous multi-bird blink output. Reviewed body scale, baseline, opposing tilts and alpha; 102 prepared frames, no clipping. Runtime remains untested.

## Ember checkpoint — 2026-11-04

Completed 17 frames and portrait with strip, GIF and game-size QA. Flame crest and brows checked against K7 before expansion. Corrected sleep to folded wings and shortened the stretch wing to avoid edge clipping without shrinking the torso. Reviewed all poses at game size; 119 aligned frames, none clipped. Runtime not run.

## Inky checkpoint and ownership update — 2026-11-04

Completed 17 Inky frames and portrait, strip, GIF and game-size review. Preserved swept crest, brows and white chest star. 136 aligned frames, none clipped. Codex is now coordinator. Eggs and mama/chick overlays transferred to mac-art; mac-birds created none and will not edit them. Remaining owned work: Hoot, Rook and final bird QA. No tests or Unity.

## Hoot checkpoint — 2026-11-04

Completed 17 frames, portrait, strip, GIF and game-size review. Retained cream heart-shaped face and purple star cap. 153 aligned frames, no clipping. Rook base checked against K7 before action generation.

## Rook checkpoint — 2026-11-04

Completed 17 frames, portrait, strip, GIF and game-size review. Compared base to K7 before expansion; retained swept ragged crest, brows, grey toothy beak and compass. Regenerated both flight frames to remove invented straps. All 170 aligned frames have no edge clipping.
