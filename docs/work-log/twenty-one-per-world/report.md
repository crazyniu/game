# Twenty-one levels per world — report

User decisions (2026-11-14): keep the ten worlds, 21 levels each (210 levels), and show three level tiles per row
on a world's page.

## Changes

- `core/levels.gd`: `COUNT` 210, `PER_WORLD` 21.
- `levels/levels.json`, `levels/hints.json`: every world keeps its ten levels on its odd numbers 1 to 19 (same
  layouts, pars and stored solutions). Each is followed by a new sibling level, and level 21 is a second sibling of
  the world's last level: 110 new levels.
- A sibling (`tools/levels/deal_siblings.gd`) has its original's branches and kinds of birds, mirrored left to
  right, with the colours handed round among the level's own species and the birds dealt again; it takes another
  weather variant of its world. It is kept only when it is solvable without tools and has no losing move beside
  its first three solution steps. Its route is within a quarter of the original's where one was found; the nearly
  sorted teaching levels of worlds 1 and 2 only trade a few pairs of birds (`@light`).
- `meta/profile.gd`: schema 5 and `LEVEL_SPREAD` (old level to new level); stars, clears, best moves, level gifts,
  the highest unlocked level and the last played level move with their levels. `run.sh`, `run.ps1`: the same step.
- `game/map_view.gd`: world page as a 3 x 7 grid (tile 124, stars 38, row pitch 208); the current level's bird
  stands to the right of its tile; previous/next world moved beside the world name.
- `CLAUDE.md`, `AGENTS.md`, `README.md`, `docs/CURRENT_GAME.md`: 210 levels.

## Evidence

- `bash scripts/run_tests.sh`: naming guard, smoke, core: 0 failures; all 210 levels replay their stored solutions.
- `run_map_aviary`, `check_map_hit_regions` (101430 checks), `run_level_heading` (7849), `run_family_dialogs` (1330),
  `run_board_hud`, `run_gameplay_dialogs`, `run_visible_skills`, `run_stuck_rescue`, `run_undo_allowance`,
  `run_chest`: 0 failures.
- Stills: [world_page](evidence/world_page.png), [world_page_late](evidence/world_page_late.png),
  [level_23_original](evidence/level_23_original.png) and its sibling [level_24_new](evidence/level_24_new.png).

## Boundaries

- New levels are variations of existing ones, not new designs; no level was played by hand.
- Siblings whose route differs most from their original (new route / original): old levels 21 (17/8), 30 (17/8, both
  siblings), 31 (19/10), 48 (19/9), 53 (15/8), and world 1's siblings (4 to 10 moves beside originals of 1 to 3).
- A save from before the change keeps its progress; the new levels between cleared ones are open and unplayed, so
  world star totals show 30 of 63 at most until they are played.
- The world page layout is new and needs the user's eye; `tests/run_map_nodes.gd` (an older tree-page test outside
  the test script) was not updated.
