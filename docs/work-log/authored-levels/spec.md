# T35 40 levels and the level tool (omp, gpt-6-astra)

Worktree `/home/developer/Projects/game`, branch `t35-levels` (a Codex worker is still writing art under `PerchSort/Assets/Resources/Art/` and [art/v5/t33/](https://github.com/crazyniu/game/blob/cb438e87e67e6d66c97bdf00b77ef356ca161290/tasks/T35_levels.md#L3); never touch those). Authority: `docs/GAME_DESIGN.md` v5.3, especially §1, §1.1, §4.1 (species debut levels), §6, §8.1 (booster unlock levels), §9 (worlds/variants), §10 (board density), §11 (level plan and difficulty gates), §12 (schema). Read them fully. Ask the coordinator with `orca orchestration ask` on any design ambiguity rather than guessing.

## Part A: the level tool (single source of rules)

Do NOT reimplement the rules in Python. Build `tools/levels/LevelTool/` as a .NET console project (net8.0, C# 9 language level to match Unity) that compiles the Unity Core sources directly via `<Compile Include="../../../PerchSort/Assets/Scripts/Core/**/*.cs" />`, so the tool and the game share one rule implementation. Add `tools/levels/README.md` with the exact commands. Install the .NET SDK only under `/tmp` or `~/.dotnet` as you did for T32; never commit SDK files or `bin/obj` (add a `.gitignore` in the tool folder).

Commands:
- `validate <levels.json>`: per level check schema and §12 validator rules; exhaustive no-booster search (generous budget, e.g. 60 s and 20M states per level; report Unknown honestly) giving par = shortest solution; 1000 seeded random plays (uniform over legal transfers and swivels, cap 200 moves) giving win rate; dead-state share over the explored states (states with no path to a win); the §6.5 ordering rule from level 3 on (the minimum moves or solvability must differ between at least two completion orders); the §11 role gate. Print one table row per level and exit non-zero if any level fails.
- `generate --spec <json>`: propose candidate boards for a given K, spare branches, mechanics mix and role, by scrambling a solved board with legal reverse moves or random deals filtered by the validator; output the best candidates with stats.
- `show <levels.json> <id>`: ASCII board.

## Part B: author levels 1–40

Write all 40 levels into `PerchSort/Assets/Resources/Levels/levels.json` (level 1 must stay byte-identical in meaning: same branches, birds and par 2). Follow §11 exactly: K per range, spare branches, mechanic debut levels (gifts L3, Sapling teach L4, eggs L6, Owl Eye L7, sleepy L11, Morning Bell L12, mama L15, crow+twig L21, Scarecrow L22, swivel L24, growing L31, Spring Rain L32, day/night L33), species debut levels from §4.1 (a species may appear only from its debut level on), roles and the difficulty gates. Also:
- World and variant per §9: no two consecutive levels share a variant; level 10/20/30/40 use the world's special variant (rain, lanterns, sunset, aurora).
- Board fits §10: at most 10 branches (5 rows), each branch has side and row; rows filled left/right alternately; twigs and swivel branches placed where their art reads well (twigs are shorter, so put them on the side opposite a long branch in the same row).
- Teach levels (T) are small: they must teach exactly one new thing and be winnable in ≤ 6 moves; the new element must be on the obvious path.
- Every level is solvable without boosters with a proven par (no Unknown allowed in the final file; shrink or redesign a level whose search does not finish).
- Neighbouring levels must differ in layout or mechanic.
- Level 40 is the finale: K=7, every mechanic of World 4 plus at least two older ones, a satisfying cascade available on the optimal line.

## Part C: tests and report

- Add an EditMode test `LevelsDataTests` that loads `levels.json` and, for every level, checks schema, species counts, debut rules and that the stored par equals the solver result when the solver finishes within a test budget (skip with an explicit message only for levels whose search is known to exceed the budget, and list them; the tool remains the authority).
- `tasks/T35_report.md`: the full validation table (id, world, variant, K, branches, mechanics, role, par, random win rate, dead-state share, ordering-rule result, gate pass), the design intent of every teach level in one line, tool usage, and anything not met.

Ownership: `tools/levels/LevelTool/**`, `tools/levels/README.md`, `PerchSort/Assets/Resources/Levels/levels.json`, `PerchSort/Assets/Tests/EditMode/LevelsDataTests.cs`, `tasks/T35_report.md`. Nothing else. No commit. Send a status after the tool works, after each world of levels, and `worker_done` at the end.
