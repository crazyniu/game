# Costume parts

## Problem
Skins were small overlays laid over the plain bird frames and fixed pictures of effects floated around the body. When the head
turns or tilts a front-view overlay cannot follow, the skins of one species look alike, and the effects belonged to nothing.

## Direction (user, 2026-11-18)
A skin is the whole bird painted wearing its costume in every animation frame (the user compared a layered cap against the
whole painted bird and chose the whole bird). Motion belongs to the costume: a moving piece hangs on the painted costume,
never in the air around the bird. Pilot: the cocoa owl wearing a propeller beanie, slot `cocoa_outfit_4`.

## Contract
- Per frame, local Codex paints the plain frame wearing the costume (input: the plain frame and a reference picture of the costumed
  front). `tools/birds/costume_parts/paint_frames.py`. The paintings are registered to the plain frame on the silhouette outside
  the costume (`whole.py`) and published as `<species>_outfit_<slot>_<frame>.png` (`publish.py`), the same frame names as the species.
- `BirdView` draws a skin as its own bird when `<outfit>_hop_01` exists; whole-bird skins have no aura or floating particles.
- A moving piece is a painted picture listed in `birds/outfit_parts.json` (`image`, `width`, `squash`, `turns`, `hubs`: the button
  position per frame). `BirdView.buildPart` turns it in a plane squashed to the view, with two fading blur copies, and hangs it on the
  hub of the current frame (blended with the next frame while frames cross-fade). Asleep: it stops turning.
- Save fields and outfit ids are unchanged.
