# Map screen rebuild: report

Task: docs/work-log/ui-rebuild/task-map.md, contract docs/work-log/ui-rebuild/spec.md.
File changed: `PerchSort/game/map_view.gd` (rewritten on the cartoon kit). No other source, art or word list changed
(every word the map uses, the four world names, is already in `tools/kit/words.txt`).

## What the screen is now

- Background: the four painted world maps (`map_bg_continuous_w1..w4`), scrolling, flicking and wheel exactly as before.
  The dark overscroll fills are gone (without a bottom bar they read as a bar): the room below world 1 is the bottom
  strip of its painting reflected at the edge, and the room above world 4 is that painting's top strip made opaque
  (its top 160 rows fade in over the previous world and nothing is above world 4) and reflected upwards.
- Level nodes (kit shapes, `UI.shape` circles with the 10 px bottom edge and soft shadow):
  - cleared: cream circle 132, ink number cap 52, three stars 46 (gap 2) centred 88 below the node, gold earned,
    faded empty;
  - current: green circle 168, white number cap 64, the level's first species (from the level's board) standing on
    top as a live `BirdView` (breathing and idle poses), the node and bird hop 10 px every 2.2 s;
  - locked: cream circle 132 with the lock icon in the faded tone, the node dimmed.
  Same kind, same size everywhere. Tap areas cover node, stars and bird.
- Top bar (fixed): world title word (cap 40, as in the approved `map_screen()`) in a cream pill 96 high at
  x 48, y 56; coins counter (`UI.counter` with the coin art) right-aligned to x 1032. The title follows the world
  in view. A subtle shade (32 percent at the top, clear at 240) sits under them.
- Bottom: shop (orange, icon shop), aviary (orange, icon aviary), settings (cream, icon gear), `UI.button`
  secondaries 128, centred on y 1792 at x 340 / 540 / 740. No bar.
- World gates: the gate art and behaviour are unchanged (open arch with light and swung doors, closed arch, tap
  emits `gate_tapped`); the gate box grew to 320 x 280 and the open light now sits inside the arch. A locked gate
  shows a cream circle 112 with the ink lock icon over its painted padlock.
- Signals, `setup`, `show_gate`, `scroll_to`, `current_level` and the preview states are unchanged. Previews give
  cleared levels mixed star counts (3 3 2 3 1) so empty stars are visible.
- Captures (`main.instant()`): the bird is quiet and the node does not hop, so stills are stable.

Private helpers in the file (kit lacks them): `_circle` (round kit shape), `_fitted` (where `UI.art` places a
sprite's visible pixels), `_strip` / `_overscroll_top` / `_overscroll_bottom`, `_instant`.

## Verification

- `godot --headless --path PerchSort --import`, then `godot --headless --path PerchSort --audio-driver Dummy -- --smoke`:
  `smoke checks: 0 failures`. Script errors all come from `BoardView` (worlds 3 and 4 slots missing, expected); the
  exit leak warnings are identical with the previous map_view.gd.
- `python3 tools/kit/build_words.py --check`: `missing word sprites: []`.
- `python3 ci/check_current_game_names.py`: 0 violations.
- `bash scripts/screenshots.sh map_view`: failed=0. Captures `build/screens/map_view__{forest_trail,first_level,
  autumn_world,all_cleared,gate_locked}.png`, each viewed at full size and against `build/style/map.png`.
- Timed: `xvfb-run -a -s "-screen 0 1280x2000x24" godot --path PerchSort --audio-driver Dummy --rendering-driver
  opengl3 --resolution 1080x1920 -- --frames=build/frames/map --wait=5 --step=0.25 --preview=map_view:forest_trail`.
  20 frames over 5 s: the bird changes pose (fluff, tilt, look down, blink) and breathes in every frame; node top
  y per frame (`build/frames/map/bounce_log.txt`): 1047 1042 1051 1052 ... 1043 ... 1044 1044 1050, one hop
  per 2.2 s.

## Open points

- Scrolling nodes pass under the fixed top bar and the bottom buttons at some positions (e.g. level 35 behind the
  shop button in `all_cleared`); that is inherent to a fixed bar over a scrolling map. The old bottom shade was
  dropped per "no bar behind them".
- `gate_tapped` is not connected in `main.gd` (unchanged; out of this task's ownership).
- `all_cleared` has no current node, so no bird on the map in that state (as before).
- Not yet reviewed or approved by the user.
