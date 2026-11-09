# Whole-bird skins

## Problem
Outfits are a painted overlay (`<outfit>_front_overlay`, a hood with the face cut out) laid over the plain species
frames by measured anchors. The overlay is one fixed picture: when the bird turns, blinks or hops, the face and eyes
underneath move while the overlay is only stretched, so two pairs of eyes and a misplaced hood show (most visible on
the right turn, frames 2 and 3, on the aviary detail page).

## Direction (user, 2026-11-09)
A skin is not clothing laid on a bird; buying one gives a different-looking bird. Every frame of a skin is one
painting of the whole bird with the costume, never a layer. Trial on one skin first: Ember's dragon hood
(`ember_outfit_5`); the user reviews the motion before the rest are repainted.

## Contract
- Painted frames live at `PerchSort/birds/<outfit>_<frame>.png` (320x320, same frame names as the species).
- `BirdView._load_frames`: when `<outfit>_hop_01` exists, the outfit is a whole bird: every frame, still pose and
  flight frame loads under the outfit prefix and no overlay is drawn. Other outfits keep the overlay path for now.
- Save fields and outfit ids (`outfit.<species>.<n>`) are unchanged.
- Pipeline: `tools/birds/painted_skins/paint.sh` (local Codex, plain frame + front reference per frame),
  `collect.py` (downscale and align feet/body to the plain frame).
