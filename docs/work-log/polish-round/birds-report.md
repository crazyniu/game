# Birds report: continuous, natural bird motion

Branch `crazyniu/polish-birds`, rebased on `origin/polish-round` (339311dc). Files changed: `PerchSort/game/bird_view.gd`,
new `tools/birds/measure_outfit_anchors.py`, new generated data `PerchSort/birds/outfit_anchors.json`, and evidence in
`docs/work-log/polish-round/birds-evidence/`. No other file was touched; no hook in another script was needed.

## What looked wrong (before, measured on this base)

Fixed-time captures, 20 fps (one PNG per 0.05 s of game time), 8 s, `--level=12` with outfits equipped on berry
(glasses), sky (scarf), cocoa (hat) and mint (band), plus the shop berry outfit page (`shop_view:bird_outfits`).

- Outfit birds dropped every painted action except blink and cheer. Their "hop" was the outfit front repeated six
  times and moved up and down (`board_before_hop_berry_glasses.jpg`, `board_before_hop_cocoa_hat.jpg`,
  `shop_before_hero_hop.jpg`): a still picture sliding, no pose change.
- Flights used plain species frames, so an outfit vanished in flight and came back on landing.
- Every frame change was a hard cut at 8 to 14 fps; actions started from and snapped back to the rest frame with a pop.
  The hop landing squash jumped to full strength in one frame.
- Idle scheduling was per bird: in 8 s on level 12, 13 of 36 action starts landed in the same frame as another start,
  and 17 times two birds of the same branch acted at once (`before_board_action_starts.log`).

## What changed

- Outfits: every `<species>_outfit_<n>_front.png` is exactly the plain front with
  `<species>_outfit_<n>_front_overlay.png` composited on top (verified pixel-exact for front and both blink frames). So
  BirdView now always plays the plain painted frames and draws the overlay on top, following per-frame anchors measured
  by `tools/birds/measure_outfit_anchors.py`. The tool registers landmark images (dark eye marks, beak, silhouette;
  fur ignored) of the front against each frame. Most actions turn the head to three-quarter view, so the overlay is
  split at its pivot and both halves scale independently (the far lens of glasses foreshortens). BirdView draws it as a
  six-vertex textured `Polygon2D` with the same mapping, and blends anchors during crossfades. The painted outfit cheer
  is still used; the overlay fades out into it and back in after it. Turn, preen, flap, hop, all nine still poses,
  selection hover and every flight now keep the outfit. No frame was mirrored or flipped and no art was painted or
  edited.
- Implausible fits (a half torn off, overlay leaving the silhouette, tilt over 30 degrees) are refitted with tied
  half scales; if still implausible, the frame copies the nearest plausible frame of the same action, or the front.
  477 of 1674 measured frames went through this repair; each outfit's `repaired` list in the JSON names them.
- Frame playback: all texture changes go through `_show()`/`_play()`. Consecutive painted frames crossfade over the
  last 35% of each frame's time; the old frame fades out in the second half of the blend so wing-only parts do not pop
  at the swap. Every action starts and ends with a crossfade from and to the rest frame (idle actions, cheer, seat hop,
  selection lift and release, flight takeoff and landing, wake, sleeper nudge). Frame timing is eased at both ends.
- Hop: 0.82 s with a crouch (squash eased in), eased rise and fall through the painted hop frames with stretch,
  and a landing squash that ramps in over 0.04 s and relaxes over about 0.2 s. Flap has a smooth lift envelope.
- Coordination: a shared clock (advanced once per frame) with a branch key (parent node + perch y; every board branch
  has a unique perch y). One bird per branch at a time plus a 0.3 s pause after it ends; at most one start per 0.22 s
  anywhere; ready birds queue and the bird that has waited longest gets the next slot (birds earlier in the scene
  tree used to starve later ones). Hidden birds do not take slots. Neighbour glances (`look_at_x`) start 0 to 0.22 s
  apart instead of all at once.
- `--bird-log` (user argument) prints one `BIRDLOG` line per idle action or cheer start, for evidence.

## Evidence

Frame sequences (ignored, `build/polish-birds/`): `before_board/`, `after_board/`, `before_shop/`, `after_shop/` (160
frames each, `fNNNNNNNN.png` at t = N x 0.05 s, 936x1664 because the project window override sets that size; the design
canvas is 1080x1920), plus `anchor_sheets/` (one verification sheet per outfit, overlay composited on every measured
frame) and the two 6 s shop tile strips. Committed contact sheets and logs in `birds-evidence/`:

- `board_after_turn_berry_glasses.jpg`, `board_after_turn_cocoa_hat.jpg`, `board_after_preen_sky_scarf.jpg`,
  `board_after_hop_berry_glasses.jpg` against `board_before_hop_*`.
- `shop_after_hero_cheer.jpg`, `shop_after_hero_tilt.jpg`, `shop_after_hero_fluff.jpg` against `shop_before_hero_hop.jpg`.
- `outfit_anchors_berry_to_ink.jpg` / `outfit_anchors_mint_to_sunny.jpg`: all 27 outfits on eight representative frames (rows in alphabetical
  outfit order: berry 1-3, cocoa 1-3, ember 1-3, hoot 1-3, ink 1-3 / mint, sky, snow, sunny).
- `*_action_starts.log` and `coordination_summary.txt`. Level 12, 8 s: before, 36 starts, minimum gap 0.00 s, 13
  same-frame starts, 17 same-branch overlaps; after, 21 starts, minimum gap 0.25 s, 0 same-frame starts, 0 same-branch
  overlaps. 12 to 13 awake birds act within the window. The shop "overlaps" counted by the same script are separate
  cards in one row, not branches.

Commands:

```
bash scripts/run_tests.sh
python3 tools/birds/measure_outfit_anchors.py --sheet <dir>        # full measurement, about 1.5 h on four cores
python3 tools/birds/measure_outfit_anchors.py --repair --sheet <dir>
xvfb-run -a -s "-screen 0 1280x2000x24" godot --path PerchSort --audio-driver Dummy --rendering-driver opengl3 \
  --resolution 1080x1920 --fixed-fps 20 --write-movie <dir>/f.png -- --capture=<dir>/last.png --motion --wait=8 \
  --level=12 --bird-log '--profile=<json with outfits equipped, species met and unlock flags set>'
  (same with --preview=shop_view:bird_outfits)
```

The Movie Maker writer (`--fixed-fps 20 --write-movie`) gives exact 0.05 s game-time spacing; the old `--frames`
capture drifts with PNG save time. "Before" runs used the base `bird_view.gd` (plus a temporary log line), swapped in
and restored afterwards.

Test result (rebased tree, final code): `bash scripts/run_tests.sh` exit 0: naming guard 0 violations (self-test OK),
smoke checks 0 failures, core checks 0 failures (all 40 levels solved and replayed). `python3
ci/check_current_game_names.py --self-test` and `python3 ci/check_current_game_names.py` both exit 0.

## What still pops or looks wrong (honest list)

- Crossfades show a faint double image for about two display frames where consecutive paintings differ a lot
  (rest to first cheer frame, wings in and out); clearly visible on the large shop and aviary hero, barely on the board.
- A front-view overlay cannot be truly correct on a turned or profile head. Weak spots in the anchor overview:
  `turn_right_03` for several head items (mint band, sky top hat, ink hat, cocoa bow tie drift sideways);
  sky goggles drop towards the beak in `look_up`; snow earmuffs and the snowflake sit slightly off in some left-flight
  frames; eyewear on full profile frames (`turn_*_04`) is squeezed onto the one visible eye. Repaired frames that copy
  the front placement keep small items (bows, flowers, tiaras) roughly right but do not follow the head exactly. If
  this is not acceptable, the fix is painted outfit frames for the turn sequences (9 species x 3 outfits x
  turn_left 4 + turn_right 4 = 216 frames) and `look_up` for sky outfit 2; I did not request them.
- Wings in front of the face (preen) are drawn under the overlay, so glasses or a band can sit on top of a wing.
- With branch exclusivity and long still poses, a given bird can wait up to about 6 s between idle actions on a busy
  board (breathing never stops). Lower `BRANCH_REST` or `START_GAP` in `bird_view.gd` if more activity is wanted.
- The shop page leaves the species-grid birds idling underneath; they are visible-in-tree, so they share start slots
  with the page's birds (harmless, but it slows the page's birds slightly).
- Not checked: aviary hero and map bird timed sequences (same code paths), selection hover/flight with outfits in a
  timed capture (code path covered by the anchors and verified in the sheets only), sound, real-device frame rates.
