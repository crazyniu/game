# Whole-bird skins: report

## Done
- 69 frames of `ember_outfit_5` (dragon hood) painted whole by local Codex from the plain Ember frames: turns,
  preen, flap, hop, cheer, nine still poses, fly up/down, all flight frames, and the front and blink frames. Five first attempts were rejected and
  repainted (two birds in one frame, hood missing, motion smear, ground shadow).
- `BirdView` loads such a skin as its own bird; the overlay is not drawn for it.
- Preview state `aviary_view:detail_ember_skin` (also in `scripts/screenshots.sh`).

## Evidence
- `main.tscn -- --preview=aviary_view:detail_ember_skin --frames=<dir> --wait=16 --step=0.1 --bird-log`: 160
  frames; the hero runs look_up, hop, stretch, flap, tilt, cheer and a right turn with the hood turning with the
  head and one pair of eyes in every frame.

## Open
- User review of the trial skin's motion on device.
- After approval: repaint the other skins the same way and retire the overlay path and `outfit_anchors.json`.
