# Costume parts: report

## Done (pilot: cocoa propeller beanie)
- Cap painted on all 69 plain cocoa frames by local Codex; cap layers cut out; hub and tilt measured per frame.
- Propeller painted top-down and rotated in a squashed plane with blur ghosts; follows each frame's hub.
- Evidence (local, ignored): `docs/work-log/costume-parts/evidence/cocoa_beanie_reel.gif` (345 ticks at 25 per second:
  blink, turns, tilts, looks, hops, flaps, cheers, preen, take-off, landing) and `cocoa_beanie_sheet.png`.

## Jitter fix (2026-11-18)
- Cause: every frame's cap is painted separately, so its size and place on the head wobbled by several pixels.
- `tools/birds/costume_parts/stabilize.py`: blink frames reuse the front layer; inside each sequence the cap width and its
  lower-rim offset from the plain beak are smoothed over neighbouring frames, clamped to 5 percent and 4 px so a real head
  move is never undone; hub follows. Width jitter (mean second difference): flap 12.3 -> 3.8 px, hop 6.8 -> 3.5, cheer 6.8 -> 2.8.
  Position jitter: hop 4.4 -> 1.4, flap 10.9 -> 5.3, cheer 5.3 -> 2.2. Turns, preen and flights keep the real head motion.

## Rig attempt and whole-bird frames (2026-11-18)
- Rig (`rig.py`): fixed view pictures plus a smoothed transform per frame. Anchoring the smoothing on the plain bird's beak was
  wrong (the beak moves 35 px between flap frames) and pushed some caps 6 px off the head; abandoned.
- Whole-bird frames (`whole.py`): the Codex paintings of the whole costumed bird (69 frames) registered to the plain frames on the
  silhouette outside the cap, used directly as the animation frames. Hat and face are one painting in every frame.
- Evidence (local): `cocoa_beanie_compare.gif` (left: plain bird + cap layer, right: whole painted bird), `cocoa_beanie_reel_whole.gif`.
- Cost measured: one Codex frame job takes about 1.5 minutes and 12 run in parallel, so a skin of 69 frames takes about 7 minutes.

## In the game (not yet built or run)
- `cocoa_outfit_4` now is the propeller beanie: 70 whole-bird frames, `cocoa_outfit_4_prop.png`, `birds/outfit_parts.json`; the old teddy
  hood overlay and its anchors of that slot are removed (history stays in Git). Manifest and `.meta` regenerated.
- `BirdView`: `buildPart`/`updatePart` for the propeller; whole-bird skins (also ember_outfit_5) no longer draw the aura and particles.
- Typecheck needs the Creator declarations, so it has to run on the Mac. Nothing was built or run in the game.

## Open
- User review of the pilot. Not integrated into `BirdView`; no game art or code changed.
- Not checked in the game or on the Mac.
