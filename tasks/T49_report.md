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
