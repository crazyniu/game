# Task board: HUD, day and night, instant hints

1. Top-left HUD. Today it is a "Level 33" pill next to a sun pill: different widths, no moves count or star meter,
   numbers that do not line up. Redesign it as one coordinated block on the 8 px grid: level, moves used and the
   three-star meter (what the player needs: how many moves for 3 / 2 / 1 stars) in rows with one left edge, equal
   heights, aligned baselines; on day/night levels a phase row that says what the number means (moves until night /
   until day) with a sun or moon icon. Keep clear of the house and the settings button; same-role elements the same
   size. A player must understand every number without explanation.
2. Day and night. Today night only fades a dark overlay (`night_overlay`) over the day painting, so the sun, glow
   and bright colours remain. Replace it: each world has a night painting (already in the repo:
   `PerchSort/world/scene/board_wN_night_bg.png`, N = 1..4, same framing as `board_wN_bg.png`). On a phase change cross-fade the backgrounds
   (0.8 s), tint the branch layers and the birds on them to cool moonlight (a multiply tint, no dark overlay), twinkle a few
   stars, drift fireflies at night, and show a short banner word ("Night falls" / "Day breaks", painted label) that
   fades in and out. Day returns the original look. The first frame of a night level must already be night. Remove
   the now-unused overlay art if nothing else uses it. Do not change rules.
3. Hints must feel instant. Measured on the initial boards: median 10 ms but level 17 takes 780 ms and level 30 exhausts
   the 1.5 s budget and fails. Compute the hint in the background (worker thread) as soon as a level starts and after every
   move or undo while the player is thinking, cache it by `search_key`, and when the firefly is pressed show the cached hint
   at once; only if the background search has not finished yet, show the existing spinner. Give the background search a
   larger budget (e.g. 8 s); if it still finds no full solution (level 30), fall back to a one-move suggestion: the
   legal move that gives the best `_hint_score`, so a hint is always shown when any legal move exists. Verify with a
   timing log (time from press to hand shown) on levels 1, 17, 30, 34, and run all 40 levels' first hints headless.
Report with the new captures: HUD on a normal level and on level 33, a day frame and a night frame of one level per
world, and a frame sequence across a phase change.
