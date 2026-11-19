# World birds: report

## Status (2026-11-19, end of the session)
Done and merged or on `aviary-world-shell`:
- 45 skins of the nine world-1 species are whole-bird skins (`skin-redesign`). A skin is 9 painted frames plus 6 mirrored ones.
- World 2 (Bamboo): the nine birds are painted (27 frames, 12 mirrored, a face portrait each), installed as WebP, with painted names in English and Chinese.
- `meta/Worlds.ts` maps every slot species to the bird of each world; the aviary list has a world switch at the bottom (arrows beside the world name,
  the map's arrows and painted world names); a world is offered only when all its birds have pictures and names. World 2 cards only show: no detail page,
  skins or met rule yet. The owner approved this shell and the bird placement on 2026-11-19.
- `birds/feet.json` records the feet row of a world bird whose hanging tail does not fit with the feet on the plain birds' line (oriole 280, magpie 242,
  pheasant 281); `BirdView` and `Kit.liveBird` stand such a bird on its feet row. Birds without an entry use 300.
- Tools in `tools/birds/costume_parts`: `paint_world.py`, `align.py` (world set, mirrored left frames, feet row), `install_world.py`, `run_skins.py --set world`
  (sleeps through the Codex usage limit by itself), `sync_skins.py` (installs finished birds), `bird_sheet.py`; `tools/birds/world_designs`: `names.tsv`,
  `paint_names.py`, `install_names.py`, `make_skin_table.py` (writes `skins.md`: three skins per world bird, the 45 designs in a cycle per slot).

Running when this was written (processes started with nohup, they survive the session): three `run_skins.py --set world --workers 5` drivers painting worlds 3-10
(72 birds, about 1,950 paintings, status lines in `tools/birds/costume_parts/out/run_skins.status`) and `sync_skins.py`, which installs every bird as it finishes.

Not done:
- World 3-10 birds (painting, then `node tools/cocos-assets.mjs` and `python3 tools/cocos-meta.py`, then a PR).
- Painted names for worlds 3-10 (`paint_names.py <world> <en|zh>`, check the spelling letter by letter, `install_names.py <world>`).
- Game logic: detail page of a world bird (its ability label is the slot's), the skin shop for world birds (3 skins each), the met rule (a world bird is met when a
  level of its world that uses its slot starts), the display mapping in `BoardView`, `Tutorial`, the home flock and popups through `Worlds.bird(world, slot)`.
- Skins of the world birds (243 skins x 9 frames): `refs.py`, `run_skins.py --set skin <bird>_<tier>`, installed by `sync_skins.py`.
- Plain frames and old bird art are still PNG (`tools/birds/to_webp.py`; the painting tools now read PNG or WebP).

## Finding: why the painted frames are not continuous
Every frame is a separate Codex painting. The head, eyes and costume are redrawn each time, so they differ by a few pixels and details.
Registration, smoothing and a rig (tried on the owl) only reduce this; the owner still sees jitter. The whole-bird painting was preferred to a
layered costume, but it is the same limit: no two paintings are identical.

## Option (tried and abandoned 2026-11-18): build the birds as 3D models and render the frames
- Each bird becomes one model with a skeleton. The 71 animation frames are renders of poses of that model, so the body, eyes and costume are
  identical in every frame by construction. A costume is a mesh on the head bone: it turns, bows and bounces with the head, and a propeller
  can really spin. New skins and world birds then cost modelling and a render, not 71 paintings each.
- Costs and risks: about 90 bird models in the plush style (the felt and fleece look needs fur shaders and lighting tuned to match the painted
  frames); Blender exists only on the Mac (`/Applications/Blender.app`); an earlier decision (2026-11-07) ruled out 3D for the UI, not for birds
  (2026-11-10 planned a Blender pilot). It may look different from the painted birds until tuned.
- Recommendation: pilot one bird (the cocoa owl) first: model from its reference, render the existing 71 frame names, compare side by side
  with the painted frames and the propeller beanie. Decide the method for the other 80 birds and 405 skins after that.

## Open
- Owner decisions listed in `spec.md`; the Blender pilot needs the owner's go-ahead.
