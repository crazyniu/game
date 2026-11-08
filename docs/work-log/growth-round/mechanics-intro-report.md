# Mechanics intro: report

Task: [task-mechanics-intro](task-mechanics-intro.md), contract: [spec](spec.md).

## What changed

- `PerchSort/game/tutorial.gd`: eight one-step mechanic lessons built on the how-to-play live scene (world painting,
  cut branch layers, hanging house, live `BirdView` birds, pointing hand, looping by itself). Each lesson is a modal
  with the mechanic title on the ribbon, the live stage (768x720, as in the tutorial), one painted line and a green
  `Got it` pill (`UI.pill`, 360x128, the same size as every Play pill). There is no close button or other action.
  - `open_lesson(host, mechanic, world, done, instant)` opens one lesson; the stage uses the painting and branch
    layers of the level's own world.
  - `mechanics_in(level_def)` reads lesson mechanics from level data; `first_level_with(mechanic)` finds the lowest
    level id that has the mechanic in `levels.json` at runtime (it follows any reordering, with no level numbers in
    code); `due_lessons(profile, level_def)` returns the mechanics whose first level this is and whose flag
    `lesson.<mechanic>` is not set. The existing `has_tutorial` / `set_tutorial` flags are used. No save fields
    were added.
  - The branch layers are now anchored at the screen edge, as in `board_view.gd`. Short branches use the board's
    narrowed painting (twig 2 seats and thinner; growing branch 2 seats, widening with the board's 0.6 s back
    easing), and the swivel branch carries the board's `swivel_knot`. The how-to-play steps are pixel-identical to
    before; see Verification.
  - Preview states `lesson_<mechanic>` build the first level with that mechanic as a still board under the lesson.
- `PerchSort/game/main.gd`: one hook in `_open_board`, after the how-to-play entry. For each due lesson it sets the
  flag and queues `Tutorial.open_lesson`. The existing queue shows it after the entrance and after the new-friend
  and unlock popups, before the first move. The modal dim blocks board input until `Got it`.
- `tools/kit/locales/en.json`: 17 ids appended at the end of `labels` (`action.got_it`, `lesson.<m>.title`,
  `lesson.<m>.line`). They are baked into `PerchSort/art/locale/en/` (17 PNGs plus manifest entries) and imported.
- `scripts/screenshots.sh`: added `tutorial:lesson_<mechanic>` for all eight.

## Lessons (rules checked against `core/rules.gd`)

| Mechanic | First level (runtime) | Title / line | Loop |
| --- | --- | --- | --- |
| egg | 6 | Mystery Eggs / Uncover an egg to hatch it | The sky covering the egg moves to the sky branch. The exposed egg hatches (`hatch()`, crack, shell, mint), then the four skies fly home. |
| sleep | 11 | Sleepy Birds / Land a match to wake it | A berry lands on the sleeping berry (same-species landing wake). It wakes (`wake()`) and the full branch flies home. |
| mama | 15 | Mama and Chicks / Chicks follow their mama | The mama leaves `[chick, sky, sky, mama]` for an empty branch. Her chick leaps after her over the skies. |
| crow | 21 | Crows / Crows never go home | The crow on top of three sunnies is parked on a free branch. A sunny joins and the flock flies home; the crow stays. |
| twig | 21 | Twigs / A twig holds only two | Two berries fill the short, thin twig. Two skies join their pair and fly home. |
| swivel | 24 | Swivel Branch / Tap the knot to turn it | The hand taps the knot. The birds hop to the reversed seats (seat order reversed, no sprite mirror). The sky now on top joins its flock and flies home. |
| growing | 31 | Growing Branch / Each flock home adds a seat | A mint flock flies home and the 2-seat branch widens to 3 seats, with a glow on the new seat. |
| daynight | 33 | Day and Night / Owls sleep until night falls | The board's day/night pill shows a sun and 2. Two moves count it down to the switch. The night painting cross-fades in under moonlight (the board's tint), the pill shows the moon, the sleeping owls wake and the owl flock flies home. |

Every lesson resets to its first scene after about 9 to 13 s and loops. Birds keep their own breathing and idle
motion throughout. The lessons are silent, matching the tutorial (only board birds make sounds), and the board music
continues underneath.

## Verification

All commands were run from the worktree root.

- `bash scripts/run_tests.sh`: naming guard self-test OK, `55 tracked source files inspected; 0 violation(s)`,
  `smoke checks: 0 failures` (smoke opens all 40 boards, so the lesson modals are built as well),
  `core checks: 0 failures`.
- Still captures: `bash scripts/screenshots.sh lesson_`, written to `build/screens/tutorial__lesson_<m>.png`. All eight
  were inspected: titles, lines, pill, stage, short twig and growing branches, knot and day pill are correct.
- How-to-play regression: `tutorial:tutorial_1..4` captures from this branch and from a scratch worktree at `HEAD`
  differ by 0 pixels.
- Frame sequences (motion, 16 s at 0.1 s steps, fixed 30 fps game clock, so frame time matches game time):
  `godot --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 1080x1920 --fixed-fps 30 --
  --capture=<abs>/m.png --motion --preview=tutorial:lesson_<m> --frames=<abs>/build/lesson-frames/<m> --wait=16
  --step=0.1` under `xvfb-run`. This gives 160 frames per lesson in `build/lesson-frames/<m>/`. The contact sheets
  `build/lesson-frames/sheet_<m>.jpg` (every 4th frame) were inspected; each lesson plays its full cycle and loops.
  Note: `--frames` takes an absolute path; a relative one resolves against `PerchSort/`.
- Once-per-mechanic proof: real autoplay runs with a persisted profile in an isolated user directory
  (`XDG_DATA_HOME=<scratch>/data`, so the real save is never touched). The profile was pre-seeded with every species
  met and every booster unlock flag, so the lesson is the first queued popup. The runs used
  `godot --headless --path PerchSort --audio-driver Dummy -- --level=L --autoplay` over the levels
  1 5 6 6 7 11 11 12 15 15 21 21 22 24 24 26 31 31 33 33 34 40. `open_lesson` prints `LESSON mechanic=<m>` only
  under `--autoplay`. The new `lesson.*` flags were read from the saved profile after each run. Result:

  | Level | Lesson shown | New flags |
  | --- | --- | --- |
  | 6 (first) | egg | lesson.egg |
  | 11 (first) | sleep | lesson.sleep |
  | 15 (first) | mama | lesson.mama |
  | 21 (first) | crow (twig queued behind it) | lesson.crow lesson.twig |
  | 24 (first) | swivel | lesson.swivel |
  | 31 (first) | growing | lesson.growing |
  | 33 (first) | daynight | lesson.daynight |
  | 6, 11, 15, 21, 24, 31, 33 again; 1, 5, 7, 12, 22, 26, 34, 40 | none | none |

  Every run won with the entrance at 3.43 to 3.81 s. An earlier unseeded run showed that a lesson waits behind the
  new-friend and unlock popups in the queue; all flags were still set.

## Open issues and boundaries

- On level 21 the twig lesson opens only after `Got it` on the crow lesson. The autoplay driver cannot press it,
  so this second lesson's appearance is shown by its flag and by the existing queue behaviour, not by a frame of it
  opening.
- A lesson is shown only on the first level with its mechanic. A player whose save is already past that level never
  sees it unless they replay that level. This is deliberate, so existing saves do not get a burst of lessons.
- `main.mechanics_of` (coordinator file, used by the level card) does not list mama/chick. The lessons use their
  own detector in `tutorial.gd`, which does.
- Headless autoplay runs sometimes print `ERROR: 2 resources still in use at exit`, including on levels where no
  lesson is due (7, 12, 22). This was not investigated.
- The user has not yet tried the lessons; UI approval is theirs.
