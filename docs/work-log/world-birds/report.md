# World birds: report

## Done (2026-11-18)
- Roster of every world's birds and the remaining work, with numbers: `spec.md`.
- The cocoa owl's propeller beanie (`cocoa_outfit_4`) is merged to main (#319).

## Finding: why the painted frames are not continuous
Every frame is a separate Codex painting. The head, eyes and costume are redrawn each time, so they differ by a few pixels and details.
Registration, smoothing and a rig (tried on the owl) only reduce this; the owner still sees jitter. The whole-bird painting was preferred to a
layered costume, but it is the same limit: no two paintings are identical.

## Option: build the birds as 3D models and render the frames
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
