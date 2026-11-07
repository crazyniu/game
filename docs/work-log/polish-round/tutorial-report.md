# Task tutorial: report

## What was built

- `PerchSort/game/tutorial.gd` (new): a four-step how-to-play inside the shared `Modal` (title "How to Play", red
  close to skip, green round Next on steps 1-3, the green Play pill on step 4). Each step has a live scene in a
  768x720 rounded card, built in board pixels and scaled by 768/1080:
  the world 1 painting (`board_w1_bg`, rows 380..1392), three cut branch layers (`board_w1_branch_L0/R0/L1`) at the
  board's seat geometry (bird scale 1.10, `SEAT_FIRST`, `END_CLEAR`, outward shift), the default hanging house on its
  rope with a round doorway mask, live `BirdView` birds and the `hand_pointer` hand (tap = dip, squeeze and a soft
  `glow_soft` ring). Below the card: the painted sentence (locale labels, lines of one step share one cap, at most 36)
  and four step dots.
  - Step 1 "Tap a branch to pick up its birds": the hand taps the upper branch, the top run of two Sunny lifts with
    the flight-flap frames (`set_selected`), settles, repeats.
  - Step 2 "Tap another branch with room / to move them: same bird on top, / or an empty branch": the run is lifted
    and flies to the branch with Sunny on top, then on the next loop to the empty branch; birds fade back each loop.
  - Step 3 "Fill a branch with one kind of bird / and they fly home to the house": one Sunny joins three, the four
    fly into the doorway (`fly_to` home with the doorway mask), the house bounces and Sunny looks out.
  - Step 4 "Send every kind home to win. / Fewer moves, more stars": two finishing moves send Sunny and then Skye
    home, the resident cheers, three outlined rating stars pop in one by one over a warm glow.
  - Loops run on tweens bound to the step's layer (no scene-tree timers), so Next, skip or Play stop them cleanly.
    Birds keep their own breathing and idle actions throughout; no sprite is mirrored or flipped.
  - Silent captures (`instant`) pose one telling moment per step with frozen birds.
- `PerchSort/game/map_view.gd`: a fourth bottom-row button `Help` (cream secondary, icon `info`, 128 px like the
  others) emitting `help_requested`. The four buttons are centred at x 240/440/640/840 (same 200 px gap as before).
- `PerchSort/game/main.gd` (hooks): `Tutorial` script getter; the map's help button opens the tutorial over the map;
  `_open_board` appends the tutorial to the post-entrance popup queue on level 1 when the profile lacks the flag
  `tutorial.how_to_play`, setting it at once (`has_tutorial` / `set_tutorial`, saved in the existing
  `tutorialFlags` field). It runs after any New Friend / unlock popups, so the player's last popup ends with Play.
  No new save fields; `meta/profile.gd` is unchanged.
- Text: appended `tutorial.title`, `tutorial.pick`, `tutorial.move.a/b/c`, `tutorial.home.a/b`, `tutorial.win.a/b`
  to `tools/kit/locales/en.json`, baked with
  `uv run --no-project --isolated --with pillow tools/kit/build_localized_labels.py en --labels-only <ids>` into
  `PerchSort/art/locale/en/` (9 new label PNGs and manifest entries) and re-imported. Lines are split per id because
  the renderer left-aligns multi-line phrases; every line is centred at runtime.
- `scripts/screenshots.sh`: states `tutorial:tutorial_1` .. `tutorial:tutorial_4`.
- `docs/work-log/polish-round/tutorial_first_run.gd`: driver for the first-run evidence (below).

## Commands and results

- `bash scripts/screenshots.sh tutorial` and `bash scripts/screenshots.sh map_view`: failed=0, no script errors.
- Frame sequences, one per step, 13 s at 0.1 s with a fixed 20 fps clock so slow PNG saving cannot skip game time:
  `xvfb-run -a -s "-screen 0 1280x2000x24" godot --path PerchSort --audio-driver Dummy --rendering-driver opengl3
  --resolution 1080x1920 --fixed-fps 20 -- --frames=$PWD/build/frames/tutorial_N --wait=13 --step=0.1
  --preview=tutorial:tutorial_N` -> 130 frames each, no script errors. The only stderr line, "1 resources still in use
  at exit", also appears for `--preview=map_view:forest_trail` alone on this branch base, so it is not from this task.
- First run: `xvfb-run -a -s "-screen 0 1280x2000x24" godot --path PerchSort --audio-driver Dummy --rendering-driver
  opengl3 --resolution 1080x1920 --fixed-fps 20 -s $PWD/docs/work-log/polish-round/tutorial_first_run.gd --
  --capture=$PWD/build/first_run/unused.png --motion --wait=120 --out=$PWD/build/first_run
  --profile='{"speciesMet":["sunny","sky"],"lastDailyGiftDate":99999999}'` (sandboxed profile, never written; it opens
  level 1 twice through `main._start_level`, wipe and entrance included):
  ```
  FIRST_RUN flag_before=false
  FIRST_RUN first_open tutorial_shown=true step=1 flag_after=true
  FIRST_RUN second_open tutorial_shown=false popups=0
  ```
- `python3 ci/check_current_game_names.py --self-test` OK; `python3 ci/check_current_game_names.py` 0 violations.
- `bash scripts/run_tests.sh`: exit 0; naming 0 violations, smoke checks 0 failures, core checks 0 failures
  (40 levels, 413812 ms).

## Evidence (committed, `docs/work-log/polish-round/tutorial-evidence/`)

- `step_1.png` .. `step_4.png`: silent captures of the four steps (half size; full size in `build/screens/`).
- `step_1_sequence.png` .. `step_4_sequence.png`: the card of every fourth frame (0.4 s apart, from 0.2 s) of each
  13 s sequence. Full frames: `build/frames/tutorial_1..4/f000..f129.png` (not committed).
  Loop lengths observed: step 1 about 4 s, step 2 about 5 s per move (alternating targets), step 3 about 7 s,
  step 4 about 12.5 s.
- `first_open.png` (tutorial over level 1 after the entrance) and `second_open.png` (level 1 again, no tutorial).
- `map_help_button.png`: the map with the four bottom buttons.

## Open points

- The task asked for a cream help button; the other three map buttons stay orange per the design doc, so the row
  mixes colours on purpose (cream marks the help role). Switch to orange if the user prefers a uniform row.
- The Play pill on step 4 is the approved 360x128 Play shape, wider than the 160 px Next; this follows the Play
  button contract rather than the 1.25 ratio.
- Sentences were split into lines per locale id for centred layout; a translator sees line ids, not one sentence.
- Not tested by the user yet; no approved runtime baseline exists for this new popup.
