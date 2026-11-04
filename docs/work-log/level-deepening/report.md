# T42 delivery report

## Result

- Atomically installed **20 replacement boards**; all other boards are unchanged, including every T level, levels 1–6, L7 (par 13) and L32 (par 8).
- Full **DEEP validation: 40/40 PASS, exit 0** on both the frozen Core and the final live Core, with the explicitly authorized exceptions below; every other T35 gate remains active.
- Existing live-Core `LevelsDataTests`: **81 passed, 0 failed, 0 skipped** on both the staged catalog and the installed resource, using NUnitLite 3.13.3, a separate Core assembly and a Linux file-backed Resources/JSON adapter.
- No Core, shared level-tool, scene, artwork or existing test source was edited by T42; no commit or push.
- Authority: [T42 task](spec.md), [v5 design §§11–12](../../GAME_DESIGN.md#11-level-plan-40-levels), coordinator clarifications `msg_06bfe96858d7`, `msg_36c158aa8784` and Mac-budget decision `msg_56134b267b23`.

## Named gates and explicit exceptions

- P/C enforce **lower bounds only**: P ≥6 from L7 / ≥8 from L21; C ≥10 from L8 / ≥12 from L21. Par >24 is a report warning, not rejection; no installed level triggers it. R from L7 remains 3–6; no re-role was used. T and levels 1–6 retain their original gates.
- **DEADEND_WINDOW_6 (D6):** C levels with par ≥12 may use a proven losing state reachable within six legal actions. Only final L40 actually needs more than the old three-action window (six actions); the repaired L30/L36/L38 witnesses fit the original window.
- **FINALE_PAR10 (F10):** L40 may use par ≥10 instead of the original ≥16 target. Its installed par is 12; the six-action dead witness uses D6 as well.
- Existing **EARLY_WARMUP_L4_L5 (EW)** and **SAPLING_INTRO_L4 (S4)** remain scoped to their unchanged levels.
- These are disclosed acceptance changes, not claims that the unmodified original T35 three-action gate or the original L40 ≥16 target passed.

## Full final live-Core validation table

1,000 seeded uniform-legal-action random trials per level, 200-action cap, seed 350053; no undo or booster. Search limits: 60,000 ms / 20,000,000 states / 4096 MiB per search. `bound` is an interval **over explored states only**, not over the unenumerated whole graph. `O` = proved ordering decision, `D<n>` = proved dead branch in n actions, `H<n>` = maximum simultaneous homecomings on the optimal public-session replay; `—` = ordering not required. Applicable named exceptions are listed even when their relaxed limit is not needed.

| ID | K | Branches | Mechanics | Role | Par old → new | Random wins | Dead-state share | Witnesses | Core ms (1000 cap) | Exceptions | Gate |
|---:|---:|---:|---|---|---:|---:|---|---|---:|---|---|
| 1 | 2 | 4 | core | T | 2 → 2 | 1000/1000 | 0% | —, H1 | — | — | PASS |
| 2 | 2 | 3 | core | T | 3 → 3 | 1000/1000 | 0% | —, H1 | — | — | PASS |
| 3 | 3 | 5 | gifts | T | 4 → 4 | 1000/1000 | 0% | O, H1 | — | — | PASS |
| 4 | 3 | 5 | gifts | R | 8 → 8 | 1000/1000 | 0% | O, H1 | — | EW, S4 | PASS |
| 5 | 3 | 5 | gifts | R | 10 → 10 | 1000/1000 | 0% | O, H1 | — | EW | PASS |
| 6 | 3 | 5 | eggs, gifts | T | 5 → 5 | 1000/1000 | 0% | O, H1 | — | — | PASS |
| 7 | 4 | 6 | eggs, gifts | P | 13 → 13 | 602/1000 | 0.03633% | O, H1 | — | — | PASS |
| 8 | 4 | 6 | gifts | C | 11 → 11 | 289/1000 | 0.04922% | O, D1, H1 | — | — | PASS |
| 9 | 3 | 5 | gifts | R | 4 → 4 | 1000/1000 | 0% | O, H1 | — | — | PASS |
| 10 | 4 | 6 | eggs, gifts | C | 12 → 12 | 57/1000 | 0.193934% | O, D1, H1 | — | D6 | PASS |
| 11 | 4 | 6 | gifts, sleep | T | 3 → 3 | 1000/1000 | 0% | O, H3 | — | — | PASS |
| 12 | 5 | 7 | gifts, sleep | P | 5 → 7 | 742/1000 | 0.010207% | O, H2 | 132 | — | PASS |
| 13 | 4 | 6 | eggs, gifts, sleep | P | 4 → 7 | 769/1000 | 0.694215% | O, H2 | 47 | — | PASS |
| 14 | 5 | 7 | gifts, sleep | R | 3 → 3 | 1000/1000 | 0% | O, H4 | — | — | PASS |
| 15 | 4 | 6 | gifts, mama, sleep | T | 2 → 2 | 1000/1000 | 0% | O, H3 | — | — | PASS |
| 16 | 5 | 7 | eggs, gifts, mama, sleep | P | 4 → 6 | 709/1000 | 0.006103% | O, H2 | 115 | — | PASS |
| 17 | 5 | 7 | gifts, sleep | C | 4 → 10 | 86/1000 | 2.449215% | O, D1, H1 | 724 | — | PASS |
| 18 | 5 | 7 | gifts, mama, sleep | R | 2 → 6 | 1000/1000 | 0% | O, H3 | 13 | — | PASS |
| 19 | 5 | 7 | gifts, sleep | C | 4 → 12 | 192/1000 | 0.090437% | O, D2, H1 | 205 | D6 | PASS |
| 20 | 5 | 7 | gifts, sleep | C | 4 → 10 | 89/1000 | 2.211102% | O, D1, H3 | 31 | — | PASS |
| 21 | 5 | 8 | crow, gifts, sleep | T | 3 → 3 | 1000/1000 | 0% | O, H4 | — | — | PASS |
| 22 | 6 | 9 | crow, gifts, sleep | P | 7 → 8 | 547/1000 | bound [0.00123%, 66.787856%] | O, H3 | 216 | — | PASS |
| 23 | 5 | 8 | crow, eggs, gifts, sleep | P | 2 → 8 | 457/1000 | bound [0.034269%, 43.456543%] | O, H2 | 652 | — | PASS |
| 24 | 6 | 9 | gifts, sleep, swivel | T | 3 → 3 | 1000/1000 | 0% | O, H5 | — | — | PASS |
| 25 | 5 | 8 | crow, gifts, sleep | C | 4 → 12 | 71/1000 | bound [0.037491%, 14.684967%] | O, D2, H2 | 422 | D6 | PASS |
| 26 | 6 | 9 | crow, eggs, gifts, sleep, swivel | P | 2 → 8 | 603/1000 | bound [0.002748%, 54.187251%] | O, H2 | 676 | — | PASS |
| 27 | 6 | 9 | crow, gifts, sleep, swivel | C | 4 → 14 | 81/1000 | bound [0.085912%, 60.808894%] | O, D1, H2 | 239 | D6 | PASS |
| 28 | 6 | 9 | crow, gifts, sleep, swivel | R | 3 → 3 | 1000/1000 | 0% | O, H5 | — | — | PASS |
| 29 | 6 | 9 | crow, gifts, sleep | C | 4 → 16 | 76/1000 | bound [0.003223%, 57.772201%] | O, D1, H2 | 787 | D6 | PASS |
| 30 | 6 | 9 | crow, gifts, sleep, swivel | C | 4 → 19 | 63/1000 | bound [0.011763%, 54.321154%] | O, D1, H2 | 561 | D6 | PASS |
| 31 | 6 | 8 | gifts, growing, sleep | T | 3 → 3 | 1000/1000 | 0% | O, H5 | — | — | PASS |
| 32 | 6 | 8 | eggs, gifts, growing, sleep | P | 8 → 8 | 535/1000 | bound [0.004174%, 47.999256%] | O, H3 | — | — | PASS |
| 33 | 6 | 8 | daynight, gifts, growing, sleep | T | 3 → 3 | 1000/1000 | 0% | O, H5 | — | — | PASS |
| 34 | 6 | 9 | crow, eggs, gifts, sleep, swivel | P | 2 → 9 | 622/1000 | bound [0.000533%, 45.385276%] | O, H2 | 395 | — | PASS |
| 35 | 6 | 8 | daynight, gifts, growing, sleep | R | 3 → 3 | 1000/1000 | 0% | O, H5 | — | — | PASS |
| 36 | 6 | 8 | daynight, eggs, gifts, growing, sleep | C | 4 → 12 | 52/1000 | bound [0.332804%, 33.5662%] | O, D2, H3 | 477 | D6 | PASS |
| 37 | 7 | 10 | crow, eggs, gifts, sleep, swivel | P | 2 → 9 | 498/1000 | bound [0.052923%, 56.876724%] | O, H5 | 303 | — | PASS |
| 38 | 6 | 8 | daynight, eggs, gifts, growing, sleep | C | 4 → 13 | 305/1000 | bound [0.080789%, 82.200006%] | O, D2, H2 | 291 | D6 | PASS |
| 39 | 6 | 8 | daynight, gifts, mama, sleep | R | 2 → 6 | 1000/1000 | 0% | O, H2 | 263 | — | PASS |
| 40 | 7 | 9 | daynight, eggs, gifts, growing, sleep | C | 3 → 12 | 164/1000 | 2.004928% | O, D6, H3 | 62 | D6, F10 | PASS |

## One-line intent for every changed level

- **L12:** Rebuild the sleeping-flock practice around Cocoa before Berry at the proved decision; Bell still saves two real moves.
- **L13:** Uncover the eggs while prioritizing Mint over Sky; extend the wake/gift lesson to seven actions with a two-flock cascade.
- **L16:** Use Mama/chick regrouping and egg reveals before the Cocoa-first decision rather than collapsing the board in four actions.
- **L17:** Require ten-action sleeper sorting while preserving the immediate 2>4 losing temptation and the Sky-before-Berry advantage.
- **L18:** Keep the Mama lesson forgiving, but spread it over six actions before the three-flock rest payoff.
- **L19:** Stage the sleeping groups before choosing Cocoa rather than Mint; keep a two-action losing opening in a twelve-action challenge.
- **L20:** Make the lantern finale a ten-action setup into a three-flock cascade, with an immediate bad 1>4 opening.
- **L22:** Force eight-action crow parking and release before a three-flock payoff; one Scarecrow still saves three actions.
- **L23:** Combine crow obstruction, hidden eggs and sleepers in eight actions; Sky-first beats Snow-first at the proved decision.
- **L25:** Retain the two-action twig-space trap while stretching the Sky-before-Snow decision into a twelve-action challenge.
- **L26:** Use Inky in eight-action egg/wake practice with extra crow occupancy; Sky-first beats Berry-first by three actions without a large search frontier.
- **L27:** Use the swivel and constrained crow parking over fourteen actions; preserve the immediate 5>1 trap and Berry-before-Mint choice.
- **L29:** Require sixteen actions around the crowded parking branch; preserve the immediate 4>1 trap and Cocoa-before-Ember decision.
- **L30:** Turn crowded crow parking into a nineteen-action sunset challenge with an immediate 8>1 losing temptation and Cocoa-before-Ember ordering.
- **L34:** Mix older crow/egg/swivel geometry into nine-action practice; Sky-before-Mint remains the cheaper proved decision.
- **L36:** Exercise eggs, growing seats and the day/night clock over twelve actions on eight branches; retain a two-action dead branch and a three-flock payoff.
- **L37:** Build a nine-action setup for the five-flock payoff rather than a two-action auto-clear; Sky-first beats Berry-first by two actions.
- **L38:** Exercise night timing and newly grown seats across thirteen actions on eight branches; Snow-first beats Hoot-first and a two-action opening is proven dead.
- **L39:** Keep the day/night Mama rest at six actions with a two-flock payoff and a clear Sunny-before-Sky advantage.
- **L40:** Replace the three-action auto-finale with twelve-action seven-species planning on nine branches; Cocoa-first beats Hoot-first by four actions and the optimal line uses growing seats before a three-flock cascade.

## Search limits and unmet original target

- No level remains below its **final authorized** floor; no shallow T35 fallback was installed for L30/L36/L38/L40.
- The original L40 ≥16 target was **not reached**: its original 640 proposals (160 at each of 2/4/6/8 swaps) found a best solver-proven in-rate par of 10; deeper proposals were Unknown under the 200,000-state / 3,000 ms Core-solver screening budget, not proven impossible. Re-evaluating the same stream with FINALE_PAR10 initially accepted proposal 135 (par 10, 122/1000 wins); the final constrained-space search improved this to par 12, 164/1000 wins, at proposal 62.
- L30/L36/L38 each originally exhausted 640 proposals under the three-action dead-window gate; best proven in-rate pars were 15/17/12. Deterministic recovery accepted L30 six-swap proposal 74, L36 four-swap proposal 106 and L38 six-swap proposal 75 after the authorized window expansion and full proof. L38 proposal 38 hit a time-budget Unknown during recovery; proposal 75 passed instead.
- Initial search analysis used 10,000 ms / 2,000,000 states / 2048 MiB, then all installed boards received the complete 60,000 ms proof settings above. Rate-rejected proposals and Unknown searches were never installed as accepted boards.
- Initial 250-proposal L7 upper-band search was superseded by the floor-only clarification; L7 remains exactly unchanged at par 13.

## Post-install failure and correction

- The initial ten-branch par-10 finale passed both full validators and the staged 81-case fixture, but the first installed-resource fixture run failed only L40 (80/81 passed): its Core solver exceeded the unchanged 3,000 ms limit. This failure is preserved in `initial-installed-data-tests.xml`; no budget or test was loosened.
- Removed one empty normal branch from that authored finale, kept both growing branches, restored alternating row geometry, and searched token arrangements with a stricter **1,000 ms generation screen**. Proposal 62 is par 12 on nine branches and exercises all five of its listed mechanics on the optimal line. The changed fixture case took 0.058473 s in the staged full run; final installed results are linked below. This is observed Linux timing, not a Mac performance guarantee.
- The coordinator then reported actual Mac Unity BudgetExceeded failures for the earlier L30/L36 boards. These observations were accepted without re-running the Mac check. A new 1,000 ms live-Core screen over every replacement rejected L26/L27/L29/L30/L36/L37/L38 on 251 as well; all seven were replaced, while all other replacements stayed unchanged.
- The final **20/20 replacements pass 1,000 ms / 200,000 states** on 251; measured times appear in the full table (maximum 787 ms). L36 traded its earlier 17-action proposal for bounded proof rather than increasing any test limit; no final authorized floor shortfall was necessary. These Linux measurements do not claim a new Mac test pass.
- The repaired World-3 boards use four additional existing crow tokens to occupy formerly empty space; ordinary flock counts remain exactly four, the required twig and all species/mechanic debuts remain unchanged. L36/L38 remove an unused normal branch, retain both growing branches, and restore alternating geometry; L37 keeps its original branch count. These are authored-board configurations, not new mechanics or solver pruning.
- Strict-screen repair effort, exact seeds and all rejected/Unknown proposals are recorded in `fast-repair-summary.json` and the `fast-l*-spec.json` / result / log files; `repair_fast.py 30 36 26 27 29 37 38` reproduces the order. Every installed board still passes the random role band, exact par, ordering and applicable dead-window requirements.

## Booster value and gifts

| Level | Booster | Proven value |
|---:|---|---|
| 4 | Sapling | 8 → 8 real moves; saves 0 |
| 7 | OwlEye | covered-egg reveal-count proxy 2 |
| 12 | MorningBell | 7 → 5 real moves; saves 2 |
| 22 | Scarecrow | 8 → 5 real moves; saves 3 |
| 32 | SpringRain | 8 → 6 real moves; saves 2 |

- All L3–40 ordering witnesses are proven and publicly replayed; every changed board exercises the Berry gift event on its optimal replay, and all except L17/L19 include a multi-flock optimal cascade. Berry is a counted-move gift in final live Core, not the removed Song meter. Owl Eye remains the approved reveal-count proxy, not a blind-play fairness proof.

## Frozen/live comparison and verification boundaries

- The captured Core predates the completed T41 cutover: six source files differ. The frozen Core has Song state and no Encore board field; live Core removes Song and adds Encore/CountedMoves. The isolated codec preserves the live Encore bit under `LIVE_CORE`; the frozen build reserves that byte as zero. No gameplay rules are reimplemented.
- All 40 frozen/live rows agree on exact par, random wins, species count, branch count, mechanic set and role; all proof/data gates pass. Graph sizes/dead-state intervals may differ because live Core retains Encore state and bounded graph searches stop at different explored frontiers; these intervals are not claimed identical.
- Par and proof depths count `State.Moves` (real transfers/swivels), not the Encore-discounted `GameSession.MoveCount`; the updated existing data test asserts the same contract.
- Linux tests execute real live Core and the unchanged fixture source through an adapter. Unity Editor import/JsonUtility behavior, Mac compilation, rendering, input and animation were **not run** here.

## Exact commands

Run from the repository root; SDK is the existing local 8.0.419 installation.

```sh
mkdir -p tools/levels/work/core_snapshot
cp -a PerchSort/Assets/Scripts/Core/. tools/levels/work/core_snapshot/
/tmp/perch-t32-sdk/dotnet build tools/levels/work/DeepLevelTool -c Release
python3 tools/levels/work/search_world.py 2 --attempts 160 --swaps 2 4 6 8
python3 tools/levels/work/search_world.py 3 --attempts 160 --swaps 2 4 6 8
python3 tools/levels/work/search_world.py 4 --attempts 160 --swaps 2 4 6 8
/tmp/perch-t32-sdk/dotnet tools/levels/work/DeepLevelTool/bin/Release/net8.0/LevelTool.dll recover tools/levels/work/w3-l30-s6-spec.json 74 tools/levels/work/recovered-l30.json
/tmp/perch-t32-sdk/dotnet tools/levels/work/DeepLevelTool/bin/Release/net8.0/LevelTool.dll recover tools/levels/work/w4-l36-s4-spec.json 106 tools/levels/work/recovered-l36.json
/tmp/perch-t32-sdk/dotnet tools/levels/work/DeepLevelTool/bin/Release/net8.0/LevelTool.dll recover tools/levels/work/w4-l38-s6-spec.json 75 tools/levels/work/recovered-l38.json
/tmp/perch-t32-sdk/dotnet tools/levels/work/DeepLevelTool/bin/Release/net8.0/LevelTool.dll generate --spec tools/levels/work/finale-par10-spec.json --out tools/levels/work/finale-par10-result.json
/tmp/perch-t32-sdk/dotnet build tools/levels/work/LiveLevelTool -c Release -p:CoreSource=/home/developer/Projects/game/PerchSort/Assets/Scripts/Core -p:DefineConstants=LIVE_CORE
/tmp/perch-t32-sdk/dotnet tools/levels/work/LiveLevelTool/bin/Release/net8.0/LevelTool.dll generate --spec tools/levels/work/finale-fast-spec.json --out tools/levels/work/finale-fast-result.json
python3 tools/levels/work/repair_fast.py 30 36 26 27 29 37 38
python3 tools/levels/work/stage_catalog.py
/tmp/perch-t32-sdk/dotnet tools/levels/work/LiveLevelTool/bin/Release/net8.0/LevelTool.dll screen tools/levels/work/staged-levels.json 12,13,16,17,18,19,20,22,23,25,26,27,29,30,34,36,37,38,39,40 tools/levels/work/final-fast-screen.json
/tmp/perch-t32-sdk/dotnet build tools/levels/work/DataTests -c Release
PERCH_LEVELS_FILE=tools/levels/work/staged-levels.json /tmp/perch-t32-sdk/dotnet tools/levels/work/DataTests/bin/Release/net8.0/DataTests.dll --workers=0 --result=tools/levels/work/staged-data-tests.xml
/tmp/perch-t32-sdk/dotnet tools/levels/work/DeepLevelTool/bin/Release/net8.0/LevelTool.dll validate tools/levels/work/staged-levels.json --complete --json-out tools/levels/work/staged-deep-evidence.json
/tmp/perch-t32-sdk/dotnet tools/levels/work/LiveLevelTool/bin/Release/net8.0/LevelTool.dll validate tools/levels/work/staged-levels.json --complete --json-out tools/levels/work/live-deep-evidence.json
python3 tools/levels/work/install_catalog.py
/tmp/perch-t32-sdk/dotnet tools/levels/work/DataTests/bin/Release/net8.0/DataTests.dll --workers=0 --result=tools/levels/work/installed-data-tests.xml
python3 tools/levels/work/build_report.py
```

Do not repeat the snapshot-copy command now: it records the historical start-of-task action and would overwrite the original frozen evidence. Generation logs/specs/results, public replay lines, exact ordering alternatives and dead witnesses remain under `tools/levels/work/`; the final JSON evidence is authoritative for each row.

## Artifacts

- Installed resource SHA-256: `7a887dfd9066e2067e19caf77dcac5d3b0c1e74b5a4e5c47f08ffd155b03fbde`.
- [Final live evidence](../../../tools/levels/work/live-deep-evidence.json), [frozen evidence](../../../tools/levels/work/staged-deep-evidence.json), [installed data-test results](../../../tools/levels/work/installed-data-tests.xml), [atomic installation record](../../../tools/levels/work/install-result.json).
- [Strict 1000 ms screen](../../../tools/levels/work/final-fast-screen.json) and [strict-screen search effort](../../../tools/levels/work/fast-repair-summary.json).
