# T32 — Core v5.3 delivery report

## Scope and result

Implemented the pure-C# rule, state, event, solver and EditMode-test scope of [T32_core_v53.md](T32_core_v53.md), using [GAME_DESIGN_V5.md §1.1](../docs/GAME_DESIGN_V5.md#11-rule-clarifications-binding-from-t32-review), §6, §7, §8.1 and §12 as the binding contract.

- Local C# 7.3 compilation succeeded with warnings treated as errors.
- Local NUnitLite result: **78 passed, 0 failed, 0 skipped, 0 warnings**: 62 new cases plus all 16 unchanged `V5RulesTests` cases.
- Executable Core smoke replayed level 1's intended two moves and all five fixture shortest paths to a win.
- A seven-species × four-bird board has an independently replayed seven-move witness; bounded BFS returned **Unknown**, not an optimality certificate or an unsolvability claim.
- No Unity Editor is available on 251. Unity compilation, actual Unity JSON/resource loading, EditMode/PlayMode runs and visual level-1 acceptance remain for the coordinator on the Mac.
- No commit or push. No scene, art, Game/View, runtime level JSON, project setting or assembly-definition changes.

## State and API design

### Schema and identity

`LevelDef.cs` contains the serializable field DTOs formerly located in `BoardState.cs`. It adds `world`, `variant`, `dayLength`, branch `kind` and growing `seats`, and the documented egg/sleep/mama/chick tokens. Missing world/variant/day length/kind resolve to **1 / noon / 0 / normal**. Eggs and modifiers count toward their real species; arbitrary crow counts do not count toward the win species set. Branch positions, occupancy and real-species totals are checked when constructing a board.

Bird IDs are assigned once in branch/seat order. Birds are immutable values carrying a species and `BirdFlags`. `Bird.IsAsleep` is the current authored sleep flag; **`BoardState.IsAsleep(bird)` is the effective sleep query**, including nocturnal day sleep. Branches consult their owning board for run selection, avoiding duplicated phase caches.

The unchanged `LevelLoader` already deserializes `LevelCatalog`/`LevelDef` fields and delegates construction to Core, so no separate DTO mapping change was necessary. The production `Resources/V5/levels.json` remains unchanged.

### Session resources and history

`GameSession(level, song, inventory)` copies its inputs; the default inventory is empty. `Song` and `Inventory` expose the session outputs without global state. Undo snapshots include birds/flags/IDs, branch kinds/seats, homed order, moves, phase/counter, moonbeam, light step, Sapling allowance, chain, Chorus coins, Song notes/payout cursor and all inventory counts. Restart restores the original input snapshot.

- Existing `TryMove`, selection, undo, restart, stars, `CanMove` and `HasLegalMove` consumers remain supported.
- `Swivel(branch)` and `TryAction(Move)` support non-transfer solver actions.
- `UseBooster(kind)` returns `BoosterResult`; Firefly returns its `SolveResult` through `Hint` and consumes only when a next action exists.
- All specified boosters are board-wide, so no unused target parameter was added.
- The coordinator explicitly authorized retaining free `AddBranch()` for the current L1 HUD. It performs the Sapling effect without spending stock and shares the same once-per-level allowance with paid Sapling.

### Resolution and events

`GameRules` is the shared transition implementation for sessions and search. Each resolution wave hatches exposed eggs on all branches, removes the complete wave in branch-index order, applies the first-wave landing wake, then applies each homecoming's gift → nearby wake → growth. Waves repeat to stability. The phase counter increments at move start; an Ink gift resets it before the scheduled end-of-move flip can occur. Phase-triggered cascades finish before Chorus and the final win event. Moonbeam expires only after each subsequent protected move fully resolves.

The event sink is ordered and optional. Search allocates no view event payloads. Events cover transfer, chick followers, hatch, egg reveal/wobble, wake, homecoming, all nine gifts, growth, twig upgrade, crow departure, swivel, phase changes, notes, payout, booster consumption and win. `Transferred.BirdAt(i)` derives the individual source/destination seats from the contiguous run and preserves existing `BirdIds`/`TargetStart` consumers; non-contiguous chick movement and swivel events carry explicit `BirdTravel` entries. A wobble event does not disclose a species.

The existing view's unknown-event fallthrough was inspected: additional L1 Gift/Song events are skipped rather than blocking its animation queue. This is source-level compatibility evidence, not a Unity visual result. New-mechanic presentation and meta persistence are outside T32 ownership.

## Rule questions and coordinator decisions

Answers were received through Orca messages `msg_28b18566dd00` and `msg_b3f692e4eb92`, then added by the coordinator to design §1.1/§12 and re-read here. The later solver review was `msg_4e1a18f5f9b8`.

| Question | Binding resolution implemented |
| --- | --- |
| Old §12 omitted the new tokens/fields | T32 schema supersedes it; swivel, mama/chick and dayLength are included; default variant is noon. |
| Gifts “from level 3,” Chorus and existing L1 help | Gifts and Chorus are engine-wide including L1; “from level 3” describes level design; free legacy AddBranch remains until the HUD migration. |
| Nocturnal sleep versus authored sleep | Initial phase is day; dayLength 0 disables nocturnal sleep; ordinary wake effects clear only the authored flag; authored sleep persists at night until woken. |
| Ink, scheduled phase changes and Hoot | Counter increments at move start; Ink switches/reset immediately and suppresses a second scheduled flip; Hoot grants the next three moves, not its granting move, and changes sleep eligibility rather than the displayed phase. Ink/Hoot emit inert gifts when dayLength is zero. |
| Wave removal and gift/wake ordering | Remove all eligible flocks before gifts; first apply landing wake in step 3, then each homecoming's gift, nearby song wake and growth in index order; recheck all exposed eggs each wave. |
| Initial exposed eggs/full flocks | Core does not auto-resolve on load; these authored-start restrictions belong to level validation. Tests can construct such states to exercise resolution. |
| Snow and Mama edge cases | Light step is consumed by the next successful transfer, even an ordinarily legal one; invalid attempts, swivels and boosters preserve it; egg tips/full targets remain illegal. Only awake matching chicks on the source follow. |
| Chain, notes, payout and coins | Each flock increments chain, adds one base note and one bonus beyond chain step one; berry notes occur in gift order; overflow carries; Chorus coins accumulate five per bonus step. |
| Payout randomness/pool | No RNG: unlocked-at-current-level pool in fixed Firefly/Sapling/OwlEye/MorningBell/Scarecrow/SpringRain order; persistent payout cursor rotates through it and is undoable. |
| Empty L1 payout pool | Bank all notes, including values above five; on the first later eligible note event, pay out all full groups of five and keep the remainder. |
| Booster timing and failed use | No move/phase tick and no chain reset; booster-induced flocks continue the chain; no-effect boosters and Firefly Unknown/no solution are rejected without consumption/history. |
| Missing wobble event | Add `EggWobbled`. |
| Search equality | Coordinator requested exclusion of all economy/chain state because search never uses boosters; the required key-equivalence test passes. |

## Solver contract

BFS returns a shortest action path using the same transfer/swivel rules. The default bounds are 100,000 visited states and 3,000 ms. Existing `SolveStatus.BudgetExceeded` is retained with explicit **Unknown** semantics (`IsUnknown`); only exhausting the reachable graph proves `Unsolvable`.

Search equality includes branch position, kind and seats, bird species/flags, phase and counter, moonbeam, light step and homed species. It excludes identity, absolute move count, Song, chain, Chorus coins, payout cursor and inventory. Economic mutation is disabled during search; its read-only inventory can consequently be shared, while real undo clones remain independent. No boosters are generated.

`MinMovesByCompletionOrder` preserves homecoming order in equality, returns the exact shortest discovered length for each order, and reports whether enumeration is exhaustive. Missing orders are not evidence of impossibility after budget exhaustion. The test/smoke example proves `sunny → sky = 1` versus `sky → sunny = 3` moves, exhaustively.

## Test coverage

| Class | Cases | Consumer-visible coverage |
| --- | ---: | --- |
| V5RulesTests (unchanged) | 16 | Existing L1 legality, partial run, home, undo/restart, free branch, stars, par and budget regression |
| V53EggTests | 3 | Exposure hatch with stable ID; matching/nonmatching wobble; blocked egg tip and invalid-action atomicity |
| V53SleepTests | 4 | Sleeping-tip acceptance; wake completion; blocked runs; adjacent-row and global wakes |
| V53CrowTests | 3 | Crow runs, no crow completion on normal/twig branches, remaining crows on win, Cocoa range and undo |
| V53BranchTests | 4 | Twig capacity/noncompletion, Mint upgrade, extra growth, N cap and upgrade cascade |
| V53MamaTests | 4 | Exact documented example, original seat IDs, nearest-first limited followers, other-branch exclusion, sleeping chicks |
| V53SwivelTests | 2 | Reversal, exposed hatch and undo; invalid normal branch; empty-swivel clock move |
| V53DayNightTests | 5 | Clock counter/undo, authored versus derived sleep, Ink precedence, exactly three Hoot moves, disabled-clock behavior |
| V53GiftTests | 4 | Sky reveal, covered Ember hatch, Snow single-use/undo, egg/capacity exceptions |
| V53ResolutionTests | 3 | Ember → Sunny → Sky three-flock cascade/termination, whole-wave removal before Ink, landing wake before gifts |
| V53SongTests | 5 | Chain break/coins, Berry early payout, multiple carry payouts, L1 banking/session carry, booster-induced chain continuation |
| V53BoosterTests | 12 | All six effects and refunds, no-effect rejection, shared Sapling allowance, Firefly Unknown/unsolvable rejection |
| V53SolverTests | 10 | Five fixture pars and replays, swivel hint, completion-order minima/exhaustiveness, economy-equivalent key, phase/seats/light legality distinctions |
| V53SchemaTests | 3 | Mixed token counts/flags and stable IDs, invalid counts/twig occupancy, snapshot independence |

### Authored fixture and smoke evidence

Fixtures are test-only JSON under `Assets/Tests/V5/EditMode/Fixtures/`; no production levels were added.

| Fixture | ID | Proven par | Observed solve time (ms) | Exercised line/mechanics |
| --- | ---: | ---: | ---: | --- |
| v53_eggs_gifts | 6 | 3 | 12 | Sky reveal, covered egg exposure, Berry notes, Sunny home |
| v53_family_sleep | 16 | 2 | 0 | Mama/chick pull, landing wake, distant Sunny wake, Berry cascade |
| v53_crows_swivel | 25 | 4 | 27 | Snow token, crow parking, destination-row Cocoa removal; swivel branch is present |
| v53_growth_ember | 32 | 4 | 23 | Growing bottleneck, Mint extra growth/twig upgrade, covered Ember hatch, Snow home |
| v53_night_owls | 33 | 3 | 0 | Required opening swivel, day/night, Hoot moonbeam, Ink reset |

Times are one observed Linux run, rounded to milliseconds, not performance guarantees. The first test pass was 77/78: the crow fixture had an authored par assumption of five. BFS found four by completing Cocoa on **row 1**, which clears both relevant crow branches. The fixture par was corrected to that proved value, and the executable smoke replayed the returned four-action path. The final full harness passed 78/78 in 0.278 s.

Additional smoke observations:

- L1 action 1 events: `Transferred, FlockHome, Gift, SongNote`.
- L1 intended line ends won in two moves with three stars and three Song notes.
- Seven-species × four-bird board: explicit legal seven-move witness replayed to win; bounded BFS returned `BudgetExceeded`, `IsUnknown = true`, empty solution path in **2,659 ms**. Its par is not certified by that search.

## Local verification boundary

Used .NET SDK 8.0.419 / runtime 8.0.25, C# language level 7.3, NUnitLite 3.13.3. The temporary project compiled the real Core sources, the real unchanged `LevelLoader`, the new tests and the unchanged `V5RulesTests`. A filesystem/System.Text.Json adapter supplied only the Unity resource/JSON APIs required by that local harness. This **does not prove Unity JsonUtility behavior, resource import or a Unity assembly build**.

Executed the NUnit suite and a separate executable smoke path that applied real Core actions, inspected ordered events, replayed solver results, compared completion orders and exercised the bounded large-board case. Temporary harness source/project files are removed after recording the result; no harness or output is added to the repository.

Mac acceptance still needed:

1. Unity import/compile, including new Core files, friend-assembly access for EditMode tests and test fixture discovery via `Application.dataPath`.
2. The complete existing EditMode and PlayMode suites, not just these local Core cases.
3. Actual level-1 play/undo/restart/hint/Add Branch, animation queue and unchanged scene/UI behavior.
4. Actual Unity JSON/resource loading for the five fixture files. New-mechanic visuals and persistence are deliberately not implemented in this pure-Core task.

## Files written

Core (three existing files replaced, four new files):

- `PerchSort/Assets/Scripts/V5/Core/BoardState.cs`
- `PerchSort/Assets/Scripts/V5/Core/GameSession.cs`
- `PerchSort/Assets/Scripts/V5/Core/Solver.cs`
- `PerchSort/Assets/Scripts/V5/Core/LevelDef.cs`
- `PerchSort/Assets/Scripts/V5/Core/GameEvents.cs`
- `PerchSort/Assets/Scripts/V5/Core/GameRules.cs`
- `PerchSort/Assets/Scripts/V5/Core/AssemblyInfo.cs`

New test sources:

- `PerchSort/Assets/Tests/V5/EditMode/V53TestBoards.cs`
- `PerchSort/Assets/Tests/V5/EditMode/V53EggTests.cs`
- `PerchSort/Assets/Tests/V5/EditMode/V53SleepTests.cs`
- `PerchSort/Assets/Tests/V5/EditMode/V53CrowTests.cs`
- `PerchSort/Assets/Tests/V5/EditMode/V53BranchTests.cs`
- `PerchSort/Assets/Tests/V5/EditMode/V53MamaTests.cs`
- `PerchSort/Assets/Tests/V5/EditMode/V53SwivelTests.cs`
- `PerchSort/Assets/Tests/V5/EditMode/V53DayNightTests.cs`
- `PerchSort/Assets/Tests/V5/EditMode/V53GiftTests.cs`
- `PerchSort/Assets/Tests/V5/EditMode/V53ResolutionTests.cs`
- `PerchSort/Assets/Tests/V5/EditMode/V53SongTests.cs`
- `PerchSort/Assets/Tests/V5/EditMode/V53BoosterTests.cs`
- `PerchSort/Assets/Tests/V5/EditMode/V53SolverTests.cs`
- `PerchSort/Assets/Tests/V5/EditMode/V53SchemaTests.cs`

New test data:

- `PerchSort/Assets/Tests/V5/EditMode/Fixtures/v53_eggs_gifts.json`
- `PerchSort/Assets/Tests/V5/EditMode/Fixtures/v53_family_sleep.json`
- `PerchSort/Assets/Tests/V5/EditMode/Fixtures/v53_crows_swivel.json`
- `PerchSort/Assets/Tests/V5/EditMode/Fixtures/v53_growth_ember.json`
- `PerchSort/Assets/Tests/V5/EditMode/Fixtures/v53_night_owls.json`

Report: `tasks/T32_report.md`.
