# Level authoring tool

Authority: [GAME_DESIGN](../../docs/GAME_DESIGN.md), especially sections 11.1 and 12. The legacy `perch_levels.py` is not a current validator. `LevelTool` targets .NET 8 / C# 9 and compiles `PerchSort/Assets/Scripts/Core/**/*.cs` directly. It contains no replacement gameplay rules; legal moves, transfers, swivels, resolution and booster effects all execute Core.

## Run

From the repository root, with .NET SDK 8 on `PATH`:

```sh
dotnet build tools/levels/LevelTool -c Release
dotnet tools/levels/LevelTool/bin/Release/net8.0/LevelTool.dll validate PerchSort/Assets/Resources/Levels/levels.json --complete --json-out /tmp/level-validation.json
dotnet tools/levels/LevelTool/bin/Release/net8.0/LevelTool.dll show PerchSort/Assets/Resources/Levels/levels.json 1
dotnet tools/levels/LevelTool/bin/Release/net8.0/LevelTool.dll generate --spec /tmp/level-spec.json --out /tmp/level-candidates.json
```

Use a local .NET SDK installation; no host-specific SDK path is required. No SDK, `bin/` or `obj/` content belongs in a commit. The tool runs single-threaded.

`validate` exits 0 only when all selected levels and catalog checks pass, 2 for validation failures, and 1 for malformed input or command errors. Without `--complete`, partial catalogs are allowed; a catalog of 40 or more entries is always checked for exactly IDs 1–40. `--id 7` analyzes just that level while retaining catalog-wide structural checks. Additional options:

- `--ms 60000`: search time budget, separately for reachable-graph closure, ordering queries and optional one-booster search.
- `--states 20000000`: state budget for each search.
- `--memory 4096`: process-memory guard in MiB, never above 4096; graph expansion reserves space for reverse reachability arrays.
- `--seed 350053`: random-play seed. Each of 1,000 trials uses `seed + levelId*1009 + trialIndex*104729` with unchecked 32-bit arithmetic and .NET 8 seeded `Random`; each legal transfer or swivel is equiprobable, stopping at a win, no legal move, or 200 moves. No undo or booster is sampled.
- `--json-out /tmp/report.json`: detailed reproducible evidence, rewritten after each completed level.

## Evidence semantics

Breadth-first search proves par at the first winning state, independently of whether the entire reachable graph closes. State keys retain branch geometry, capacity/kind, ordered species/flags, homed species, Light Step, phase/counter, Moonbeam and Sapling usage. Stable bird IDs and economy do not affect no-booster reachability and are omitted; transitions use Core's existing economy-disabled search mode. Every reported optimal line is also replayed through public `GameSession`, with normal events/economy enabled, and must actually win in the stated number of moves.

Reverse graph traversal marks states with a witnessed winning continuation. A state is proven dead only if it cannot reach either a win or an unexpanded frontier. On exact closure the dead share is exact. Otherwise the printed interval is **over explored states only**: `[knownDead / explored, (knownDead + unknown) / explored]`; it is not a bound on an unenumerated whole-graph proportion. Budget exhaustion is Unknown, never evidence of unsolvability.

The ordering witness starts at the initial state or a state proven to lie on an optimal line. It compares two reachable **next homecoming species** choices; conditional breadth-first search proves a strictly worse minimum or no winning continuation for one. The report includes the common prefix, both choices and paths. If several flocks home in one action, the first Core-recorded homecoming determines the next choice. A challenge additionally needs a proven dead state reachable within at most three moves of the start.

Booster introductions use a zero/one-cost shortest-path search: exactly one use of that booster, costing zero moves, at any legal moment, and normal actions costing one. This invokes Core effects without an inventory prerequisite; it models one supplied starter booster, not earning unlimited boosters. Owl Eye instead reports the explicitly approved **covered-egg reveal-count proxy**, maximized at the initial state because eggs are never created. This is not an information-state or blind-play fairness proof.

### Named early-level exceptions

The coordinator authorized these two scoped exceptions for T35; the global role and booster gates are unchanged:

- `EARLY_WARMUP_L4_L5`: L4 and L5 use role **R**, requiring at least 700 random wins and par at most 10, without an upper win-rate cap.
- `SAPLING_INTRO_L4`: L4 remains the optional Sapling safety-net introduction. Its one-use minimum must still be proven and reported, but the usual saving of at least two moves is waived for this level only; measured saving zero is accepted.

Validation prints applicable exception IDs to stderr and includes them in each JSON evidence row's `AppliedExceptions`. Other practice levels still require 400–800 wins; Bell, Scarecrow and Rain still require savings of at least two moves, and Owl Eye still requires its reveal-count proxy of at least two.

## Candidate spec

```json
{
  "Id": 2,
  "K": 2,
  "Spare": 1,
  "Role": "T",
  "Species": ["sunny", "sky"],
  "Mechanics": [],
  "NearSolved": true,
  "StartRight": false,
  "Seed": 3502,
  "Attempts": 20,
  "Keep": 1,
  "Limits": { "MaxStates": 200000, "Milliseconds": 5000, "MemoryMiB": 4096 }
}
```

`Mechanics` accepts `gifts`, `eggs`, `sleep`, `mama`, `crow`, `swivel`, `growing`, `daynight`; normal debut/schema checks still apply. Optional count/configuration fields are `Eggs` (2), `Sleepers` (2), `Crows` (2), `GrowingSeats` (2), `DayLength` (3), `SwivelIndex` (0), and `GrowingIndices` (the last two branches). Species default to available species with any required debut included. World/variant/role follow the fixed plan. `NearSolved` makes a clustered random deal; otherwise all birds are randomly dealt. Both are proposals filtered by the same validator, not gameplay changes.

Alternatively, `BaseLevel` may contain a complete level DTO; `Swaps` (default 2) exchanges that many random token pairs while preserving its geometry, counts and flags. Generation first rejects invalid schemas and out-of-gate random rates, then fully analyzes survivors, printing progress to stderr and returning the best candidates with their evidence. It exits 0 when at least one candidate passes, 2 otherwise; failed/Unknown candidates are explicitly labelled and must not be installed as accepted levels. Candidate JSON is not itself the catalog: copy only an accepted candidate's `Level` into the `levels` array, then validate the complete catalog.
