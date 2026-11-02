# T35 delivery report

## Result and scope

- Installed all **40 fixed authored levels** with one atomic same-filesystem replacement; the original L1 object, branch contents and par 2 were preserved.
- Complete installed-catalog validation: **40/40 PASS, exit 0**; all no-booster pars are proven and all L3–40 completion-order witnesses are proven. All catalog-wide debut, recurrence, geometry, variant and count constraints pass.
- Installed-resource data tests: **81 passed, 0 failed, 0 skipped**, using real Core in a separate assembly with a Linux file-backed Resources/JSON adapter.
- Authority: [GAME_DESIGN_V5 §§11–12](../docs/GAME_DESIGN_V5.md#11-level-plan-40-levels), [T35 task](T35_levels.md), and the coordinator’s explicit scoped decision `msg_acbf7dc8af71` for L4/L5.
- Changes stay within the owned tool directory, tool README, V5 level resource, V5 data test and this report. No Core, UI, scenes, art or unrelated files changed; no commit/push.

## Named exceptions

- **EARLY_WARMUP_L4_L5:** L4 and L5 are R warm-ups, not P. The unchanged R gate requires at least 70% random wins and par ≤10, without an upper win-rate ceiling.
- **SAPLING_INTRO_L4:** L4 remains the optional Sapling introduction: “Sapling is your safety net”. Its no-booster par is 8 and its proven best one-use minimum is also 8, so the measured saving is **0**, not two moves. The coordinator waived only L4’s saving threshold; the tool still requires and reports the exact one-use minimum.
- Exception IDs appear in validator stderr and JSON `AppliedExceptions`. Every other role, ordering, booster, schema and proof gate is unchanged.
- The coordinator’s one-time tutorial-hand cue at Sapling is a UI presentation requirement outside T35 ownership; no hand animation or tutorial UI is claimed here. This ownership boundary was sent back to the coordinator.

## Complete validation table

Command: `/tmp/perch-t32-sdk/dotnet run --project tools/levels/V5LevelTool -c Release -- validate PerchSort/Assets/Resources/V5/levels.json --complete --json-out /tmp/t35-installed-evidence.json`.

Settings: 60,000 ms / 20,000,000 states / 4096 MiB per search; seed 350053; 1,000 uniform random plays per level, capped at 200 transfers/swivels. No undo or booster is sampled. The final complete run exited 0.

Dead-share percentages are exact on closed reachable graphs. `bound` means an interval **over explored states only**, not the unenumerated full graph. L22/L32/L37 exhausted the graph time budget; their pars and ordering witnesses are nevertheless proven, as permitted by the approved contract. `PASS*` identifies the named exceptions above.

| ID | World | Variant | K | Branches | Mechanics | Role | Par | Random wins / 1000 | Dead share | Ordering | Gate |
|---:|---:|---|---:|---:|---|---|---:|---:|---|---|---|
| 1 | 1 | noon | 2 | 4 | core | T | 2 | 1000 | 0% | Not required | PASS |
| 2 | 1 | golden | 2 | 3 | core | T | 3 | 1000 | 0% | Not required | PASS |
| 3 | 1 | morning | 3 | 5 | gifts | T | 4 | 1000 | 0% | Proven | PASS |
| 4 | 1 | noon | 3 | 5 | gifts | R | 8 | 1000 | 0% | Proven | PASS* |
| 5 | 1 | golden | 3 | 5 | gifts | R | 10 | 1000 | 0% | Proven | PASS* |
| 6 | 1 | morning | 3 | 5 | eggs, gifts | T | 5 | 1000 | 0% | Proven | PASS |
| 7 | 1 | noon | 4 | 6 | eggs, gifts | P | 13 | 602 | 0.039026% | Proven | PASS |
| 8 | 1 | golden | 4 | 6 | gifts | C | 11 | 289 | 0.052932% | Proven | PASS |
| 9 | 1 | morning | 3 | 5 | gifts | R | 4 | 1000 | 0% | Proven | PASS |
| 10 | 1 | rain | 4 | 6 | eggs, gifts | C | 12 | 57 | 0.211977% | Proven | PASS |
| 11 | 2 | drizzle | 4 | 6 | gifts, sleep | T | 3 | 1000 | 0% | Proven | PASS |
| 12 | 2 | mist | 5 | 7 | gifts, sleep | P | 5 | 593 | 0.007666% | Proven | PASS |
| 13 | 2 | noon | 4 | 6 | eggs, gifts, sleep | P | 4 | 753 | 0.606061% | Proven | PASS |
| 14 | 2 | drizzle | 5 | 7 | gifts, sleep | R | 3 | 1000 | 0% | Proven | PASS |
| 15 | 2 | mist | 4 | 6 | gifts, mama, sleep | T | 2 | 1000 | 0% | Proven | PASS |
| 16 | 2 | noon | 5 | 7 | eggs, gifts, mama, sleep | P | 4 | 719 | 0.03667% | Proven | PASS |
| 17 | 2 | drizzle | 5 | 7 | gifts, sleep | C | 4 | 176 | 1.098901% | Proven | PASS |
| 18 | 2 | mist | 5 | 7 | gifts, mama, sleep | R | 2 | 1000 | 0% | Proven | PASS |
| 19 | 2 | noon | 5 | 7 | gifts, sleep | C | 4 | 207 | 1.098901% | Proven | PASS |
| 20 | 2 | lanterns | 5 | 7 | gifts, sleep | C | 4 | 192 | 1.098901% | Proven | PASS |
| 21 | 3 | morning | 5 | 8 | crow, gifts, sleep | T | 3 | 1000 | 0% | Proven | PASS |
| 22 | 3 | noon | 6 | 9 | crow, gifts, sleep | P | 7 | 547 | bound [0.002447%, 44.439007%] | Proven | PASS |
| 23 | 3 | windy | 5 | 8 | crow, eggs, gifts, sleep | P | 2 | 522 | 0.001966% | Proven | PASS |
| 24 | 3 | morning | 6 | 9 | gifts, sleep, swivel | T | 3 | 1000 | 0% | Proven | PASS |
| 25 | 3 | noon | 5 | 8 | crow, gifts, sleep | C | 4 | 243 | 0.29513% | Proven | PASS |
| 26 | 3 | windy | 6 | 9 | crow, eggs, gifts, sleep, swivel | P | 2 | 442 | 0.008971% | Proven | PASS |
| 27 | 3 | morning | 6 | 9 | crow, gifts, sleep, swivel | C | 4 | 177 | 0.582524% | Proven | PASS |
| 28 | 3 | noon | 6 | 9 | crow, gifts, sleep, swivel | R | 3 | 1000 | 0% | Proven | PASS |
| 29 | 3 | windy | 6 | 9 | crow, gifts, sleep | C | 4 | 216 | 0.305707% | Proven | PASS |
| 30 | 3 | sunset | 6 | 9 | crow, gifts, sleep, swivel | C | 4 | 197 | 0.582524% | Proven | PASS |
| 31 | 4 | noon | 6 | 8 | gifts, growing, sleep | T | 3 | 1000 | 0% | Proven | PASS |
| 32 | 4 | bluehour | 6 | 8 | eggs, gifts, growing, sleep | P | 8 | 535 | bound [0.00218%, 48.807289%] | Proven | PASS |
| 33 | 4 | morning | 6 | 8 | daynight, gifts, growing, sleep | T | 3 | 1000 | 0% | Proven | PASS |
| 34 | 4 | noon | 6 | 9 | crow, eggs, gifts, sleep, swivel | P | 2 | 442 | 0.008971% | Proven | PASS |
| 35 | 4 | bluehour | 6 | 8 | daynight, gifts, growing, sleep | R | 3 | 1000 | 0% | Proven | PASS |
| 36 | 4 | morning | 6 | 9 | daynight, eggs, gifts, growing, sleep | C | 4 | 236 | 0.500501% | Proven | PASS |
| 37 | 4 | noon | 7 | 10 | crow, eggs, gifts, sleep, swivel | P | 2 | 422 | bound [0.00379%, 9.474705%] | Proven | PASS |
| 38 | 4 | bluehour | 6 | 9 | daynight, eggs, gifts, growing, sleep | C | 4 | 216 | 0.500501% | Proven | PASS |
| 39 | 4 | morning | 6 | 8 | daynight, gifts, mama, sleep | R | 2 | 1000 | 0% | Proven | PASS |
| 40 | 4 | aurora | 7 | 10 | daynight, eggs, gifts, growing, sleep | C | 3 | 307 | 0.36062% | Proven | PASS |

## Booster opportunities

| Level | Booster | No-booster par | Best one-use minimum / proxy | Result |
|---:|---|---:|---|---|
| 4 | Sapling | 8 | 8 moves; saves 0 | PASS under SAPLING_INTRO_L4 |
| 7 | Owl Eye | 13 | Reveals 2 covered egg identities | PASS under the approved reveal-count proxy |
| 12 | Morning Bell | 5 | 3 moves; saves 2 | PASS |
| 22 | Scarecrow | 7 | 4 moves; saves 3 | PASS |
| 32 | Spring Rain | 8 | 6 moves; saves 2 | PASS |

One-booster search supplies exactly one introductory item, invokes the real Core effect at the best legal moment, counts that use as zero moves, and disallows further uses. It does not assume unlimited earned inventory. Owl Eye uses the explicitly approved covered-identity reveal-count proxy, not an information-state or blind-play fairness proof.

## Teach-level intent

- L1 — preserve the original two-transfer same-species homecoming lesson exactly.
- L2 — introduce the Firefly opportunity at the required temporary empty-branch parking move; solve in three transfers.
- L3 — complete Berry to exercise Encore, then finish Sunny and Sky; homecoming order has a proven cost difference.
- L6 — move the three Mint birds to uncover the hidden Mint egg, hatch it and reunite its flock.
- L11 — finish Sunny to wake sleeping complete flocks and show the first wake cascade.
- L15 — move Sunny Mama so her separated chick follows and the family can return together.
- L21 — park the blocking crow on the opposite short twig before resolving the ordinary flocks.
- L24 — swivel the blocked branch to expose the movable Sunny group, then complete the cleared flocks.
- L31 — Sunny wakes Mint, Mint grows the two-seat branches, and the final transfer occupies newly opened seats.
- L33 — the second move reaches night, wakes Hoot and resolves its flock; day/night is the only new mechanic.

All T rows have proven par ≤6 and 100% random wins. These are data/design intentions with exercised Core events, not claims that tutorial overlays or animation were implemented.

L40 uses seven real species, ten branches, aurora, growing branches and day/night plus older egg/sleep mechanics. Its proven three-move optimal line returns seven flocks in one action without a booster.

## Tool and exercised verification

- [Tool instructions](../tools/levels/README.md) contain exact build, validate, generate and show commands, the candidate schema, deterministic sampling and proof semantics.
- The net8.0 / C# 9 project compiles the Unity Core sources directly. There is no Python gameplay-rule implementation; every legal move, transfer, swivel, resolution and booster effect executes Core.
- Breadth-first search proves shortest no-booster solutions. Reverse reachability distinguishes witnessed wins, proven dead states and unknown frontier-dependent states. Conditional searches prove differing next-homecoming minima or a dead alternative at an optimal-line decision.
- Every reported optimal line, successful one-booster line and ordering witness was replayed through public `GameSession`, with normal events/economy enabled.
- Actual `generate` runs produced accepted candidates, including L10 (par 12, 57/1000) and the authorized R-role L4 (par 8, 1000/1000, measured Sapling saving 0). `show` displayed the installed ten-branch, seven-species L40 board.
- Negative CLI smoke: a non-exempt L12 with 1000/1000 wins and Bell saving 0 exited 2 for both the P ceiling and the booster-value gate; budget-limited L4 exited 2 with Unknown despite its named exceptions. The exceptions do not suppress unrelated failures.
- `V5LevelsDataTests` compiled against a separate Core assembly and passed **81/81** cases on the installed resource through NUnitLite 3.13.3. The tests cover exact L1 preservation, all forty schemas/counts/debuts, stored-par equality and winning public-session replays.
- The test solver budget is 200,000 states / 3,000 ms per level. All forty completed within it in this Linux run; there are no known over-budget skips or ignored levels.
- Temporary probe and NUnit adapter source/projects were removed after smoke execution. SDK files, build outputs and verification artifacts are not submitted.

## Untested boundaries and handoff

- No Unity Editor is installed on 251. Mac Unity compilation/EditMode execution, actual Resources/JsonUtility importing, rendering, input and animation remain the coordinator’s verification step; Linux NUnit used a temporary file-backed resource/JSON adapter.
- Full reachable-graph closure is not claimed for L22/L32/L37; their labelled bounds and proven pars/witnesses satisfy the authorized acceptance rule.
- Owl Eye fairness is a reveal-count proxy only. The L4 Sapling opportunity is intentionally a safety-net introduction with measured zero minimum-move saving, not a claimed optimization benefit.
- The one-time Sapling tutorial hand and any design-authority-document edits belong to their respective owners, not this data/tool change. No remaining in-scope data/tool acceptance failures are known.
