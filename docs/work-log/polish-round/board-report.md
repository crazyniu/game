# Board report: HUD, day and night, instant hints

Task: [task-board.md](task-board.md), contract: [spec.md](spec.md). Branch `crazyniu/polish-board`.

## What changed

### 1. Top-left HUD (`game/board_view.gd`)

The Level pill and the separate sun pill are replaced by one cream status card at (48,40), 352 px wide, rows of
56 px with 2 px rim dividers. Every row starts on the same left edge (x = 20 inside the card) and has its cap band
centred, so baselines line up within each row:

1. `Level` + level number.
2. `Moves` + moves used. The level and moves numbers share one value column.
3. Star meter: three tiers, each showing its stars followed by the most moves that still earn them. The tiers are
   three stars with par, two stars with par + 3, and one star with infinity (matching `GameSession.stars_for`). A
   tier the player can no longer reach turns hollow and dims to 40 %.
4. Day/night levels only: a sun or moon (the phase now), then `Night in` / `Day in`, then the moves left.

The labels are new painted labels in Lilita One, the same face as `Level`, at cap 32. Tier numbers use cap 24 and
painted numeral sprites. No runtime font is used. A changed value (moves, phase count) pulses briefly. The card
ends 30 px before the house and Settings is untouched. When a wide goal row (5 or more goals) would reach under
the card, the goal row moves below the card (the card is 248 px tall on day/night levels). Otherwise goal
positions are unchanged. The move count updates as soon as a move is applied. On a move that changes the phase,
the phase row updates when the cross-fade starts.

### 2. Day and night

- `board_wN_night_bg` is drawn over `board_wN_bg` and cross-fades over 0.8 s (sine ease). I removed
  `night_overlay.png`, which nothing else used.
- A moonlight multiply tint `Color(0.70, 0.76, 0.98)` goes on the branch layer and the bird layer, so it covers
  every bird on a branch. There is no dark overlay.
- Night life layer (between painting and branches): 11 twinkling `sparkle` stars in the sky band, each with its
  own period, plus 9 fireflies (`firefly` + `glow_soft`) wandering on slow loops and blinking. Their alpha follows
  the night amount, so they fade in and out with the cross-fade.
- Banner: a cream pill under the goals with a moon or sun and the painted label `Night falls` / `Day breaks` (cap
  48). It scales and fades in over 0.25–0.35 s, holds 1.0 s, then fades and drifts up over 0.4 s.
- The scene is built in the board's current phase, so a board that starts at night shows night on its first frame.
  Undo back across a phase change cross-fades without the banner. Day returns the original look exactly (night
  amount 0: night painting hidden, tint white).
- The rules are unchanged.

### 3. Instant hints (`game/board_view.gd`, `core/solver.gd`)

- `_prefetch_hint()` starts a background search on a dedicated `Thread` when the level starts and after every
  move, undo and applied booster (while the move animation plays). Results are cached by `search_key(false)`: a
  full route caches every state along it, and a fallback caches one move.
- A press on a cached state shows the hand at once. If the search has not finished, the existing spinner shows
  until it lands. A search for an older board is cancelled through a shared cancel flag and the current board is
  searched next. Leaving or restarting a level cancels the search and joins the thread. A restart of the same level
  keeps the cache.
- Background budget: 400,000 states / 8 s (it was 24,000 / 1.5 s). If no full route is found, the worker caches
  `Solver.best_move()`: the legal move with the best (lowest) `_hint_score`. A winning move comes first, and
  undoing the previous move gets the same +24 penalty as in the search. So whenever a legal move exists, a hint
  is shown.
- Solver changes: `hint()` gets an optional `cancel` argument, and `best_move()` is new. `solve()` and the search
  results are unchanged.
- The spinner, retry and cross states still exist. The cross now appears only when the board was searched and no
  legal move exists.

## Previews added (`board_view.gd` `preview()`)

`day_wN`, `night_wN` (levels 5/15/25/35 in each phase), `hud_progress` (level 36 with fake 13 moves: three stars
lost), `phase_change` / `day_break` (one real move on level 33 with the phase clock set one move before the
change), `hint_timing_N` (press at once), `hint_think_N` (press after 3 s), and `hint_all` (all 40 levels:
background ready time and first press, then quit).

## Commands and results

- `bash scripts/run_tests.sh`: naming guard OK, `smoke checks: 0 failures`, `core checks: 0 failures` (exit 0;
  run before and after the last changes).
- Stills: `bash scripts/capture.sh <abs.png> --preview=board_view:<state>` for `level_5`, `level_33`,
  `hud_progress`, `day_w1..4` and `night_w1..4`. Output is in `build/screens/polish-board/board_*.png` (ignored).
- Frame sequences, fixed 30 fps so frames are evenly spaced in game time (software GL saves are slow):
  `xvfb-run -a -s "-screen 0 1280x2000x24" godot --path PerchSort --audio-driver Dummy --rendering-driver opengl3
  --resolution 1080x1920 --fixed-fps 30 -- --capture=<x> --motion --frames=<dir> --wait=6 --step=0.1
  --preview=board_view:phase_change` (60 frames), and the same with `day_break`, `--wait=3.5` (35 frames).
  Output is in `build/screens/polish-board/phase_change_frames/` and `day_break_frames/`.
- Hint timing (xvfb, rendering on): `... -- --capture=<x> --motion --wait=140 --preview=board_view:hint_timing_N` and
  `hint_think_N` for N = 1, 17, 30, 34.
- All 40 first hints, headless: `godot --headless --path PerchSort --audio-driver Dummy -- --capture=<x> --motion
  --wait=600 --preview=board_view:hint_all`.

Committed evidence in [board-evidence/](board-evidence/):

- `hud.jpg`: HUD on level 5 (normal), level 33 (day/night) and level 36 with three stars lost.
- `day_night_worlds.jpg`: a day frame (top) and a night frame (bottom) for each world, levels 5/15/25/35.
- `night_falls_frames.jpg`: t = 0.4–2.7 s at 0.1 s steps. The move lands, then at t ≈ 1.0 s the banner, HUD moon row
  and cross-fade start together. Night is complete about 0.8 s later, and the banner is gone by about t = 2.2 s.
  Fireflies and stars are visible.
- `day_breaks_frames.jpg`: the same for night to day. The original day look returns.
- `hint_timing.log`, `hint_all.log`: raw logs.

### Hint timing (machine load average 10–12 on 4 cores during the runs; other workers were capturing)

| Level | Background search | Press at once: press to hand | Press after 3 s: press to hand | Next presses (following the hint) |
| --- | --- | --- | --- | --- |
| 1 | 130–233 ms | 0.0 ms (cached) | 0.0 ms | 0.0 ms |
| 17 | 3.0–4.1 s | 3761 ms (spinner) | 0.0 ms | 0.0 ms |
| 30 | 5.2–5.5 s | 5229 ms (spinner) | 0.0 ms | 0.0 ms |
| 34 | 1.3–1.5 s | 1166 ms (spinner) | 0.0 ms | 0.0 ms |

Headless, all 40 levels: every first search found a full route (no fallback, nothing cancelled). The slowest were
level 30 at 4668 ms, level 17 at 2561 ms and level 20 at 521 ms. The first press after the search showed the hand
in 0.0 ms on all 40 levels.

## Open issues and boundaries

- On this loaded machine, the level 30 search sometimes exceeded 8 s in an earlier run that used
  `WorkerThreadPool`. The fallback then gave a one-move hint (logged `source=fallback`, as designed). After every
  fallback move the next state needs a fresh search, so on a slow phone, presses on level 30 mid-game may show the
  spinner for a few seconds while the player has not waited. Making the solver itself faster (a cheaper clone or
  key in `core/board.gd`) was out of scope.
- Pressing within the first seconds of levels 17/30/34 still shows the spinner until the background search
  lands. During the normal 3.5–4.5 s entrance the search usually finishes before input is allowed on 17 and 34.
- Shared files touched outside the board ownership:
  - `tools/kit/locales/en.json`: five ids inserted directly after `hud.level` (`hud.moves`, `hud.night_in`,
    `hud.day_in`, `board.night_falls`, `board.day_breaks`) and added to `display_labels`.
  - Baked labels in `PerchSort/art/locale/en/` and `manifest.json`. `manifest.json` will likely conflict with
    other workers' bakes; resolve it by re-running the label tool for the union of ids.
  - `docs/design/ui-system.md`: the Board HUD bullet now describes this HUD.
- I did not check other house styles (acorn, teapot) with a 6-goal level visually. The rule that moves the goal row
  below the card covers them by construction.
- None of this is user-approved. The user still needs to try the HUD, the night look and the banner on the Mac
  before the `approved_runtime` baseline is refreshed.
