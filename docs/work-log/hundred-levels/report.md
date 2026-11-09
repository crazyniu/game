# Hundred levels: report

Delivered per [spec](spec.md). Art brief: [art-brief](art-brief.md). Per-level data: `curve.json`.

## Levels

`tools/levels/level_lab.gd` (metrics, dealing, redealing on the game's own rules and solver) and
`tools/levels/assemble_levels.py` (ordering) built the catalog: 34 authored levels unchanged, 6 authored levels dealt
again over their own branches because they were far too easy for any place after their mechanic's debut (old 13, 15, 23,
33, 34, 35), 60 generated.

| Chapter | New mechanic | Teaching level | Average score | Moves (without the teaching level) |
| --- | --- | --- | --- | --- |
| 1 | eggs (level 5) | | 7.6 | 1-10 |
| 2 | sleepers | 11 | 13.8 | 6-15 |
| 3 | mama and chicks | 21 | 16.5 | 4-16 |
| 4 | crows and twigs | 31 | 19.8 | 6-20 |
| 5 | swivel | 41 | 21.8 | 6-21 |
| 6 | growing branches | 51 | 26.3 | 11-25 |
| 7 | day and night | 61 | 27.7 | 9-23 |
| 8 | all mixed | | 31.5 | 17-29 |
| 9 | all mixed | | 35.1 | 23-30 |
| 10 | all mixed | | 38.0 | 24-31 |

Limits:

- The six teaching levels (11, 21, 31, 41, 51, 61) score 6 to 10, far under their chapters, by design.
- Five other levels sit up to 1.7 points under the previous chapter's average (22, 42, 43, 62, 63).
- Most large generated levels were solved by best-first search, not proven shortest. Their par is the found route's
  counted moves plus 2, so it is an upper bound on the optimum.
- Difficulty is a formula calibrated on the authored levels; nobody has played the new levels yet.

## Code

- `Levels.COUNT`, `WORLDS`, `PER_WORLD` replace the literals. `Profile` schema 3 moves records with `LEVEL_EXPANSION`.
- A level or world holding a cleared level stays open, because a migrated save can have gaps.
- Booster unlocks follow the new mechanic order (Scarecrow at 32, Spring Rain at 52).
- The chapter browser pages four chapters at a time with worded Back and Next pills.
- `run.sh` migrates older saves the same way before opening all levels. Before this it wrote a save without a schema
  number, which the game would have read as the oldest format.
- The core test replays every level's stored solution from `levels/hints.json` instead of searching.

## Art

Six forests by local Codex: board painting with and without branches, night, map, gate set, gate backdrop. Lakeside
(pale birch branches the cutter could not detect) and Rainforest (a flat rectangle behind the branches) were painted
again. All 60 level-map nodes of the new maps sit on the painted trail (measured).

## Verification

- `bash scripts/run_tests.sh`: naming guard, smoke and core results are in the pull request.
- Captures with a full profile: levels 12, 44, 56, 66, 77, 88, 93, 100; `popups:chapters`, `map_view:top_end`.
- The launcher's migration was run on a fabricated schema 2 save: all 40 records moved correctly.
- Not verified: the 3.5-4.5 s level entrance on large boards (software rendering here reports 6 to 13 s even on small
  levels, so it needs a real GPU); playing the generated levels by hand. UI approval is the user's.
