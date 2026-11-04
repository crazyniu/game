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
