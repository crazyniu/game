> **T30 current-engine scope (2026-11-01):** [GODOT_MIGRATION](GODOT_MIGRATION.md) owns active implementation, cloud-first testing and gaps. Unity/C#/LitMotion/PlayerPrefs/Canvas/asmdef commands, APIs, dimensions and test results retained below are **legacy evidence/reference, not active Godot instructions or pass claims**. Engine-independent rule/content/asset-approval requirements remain; current engine mappings are in the migration matrix. Other feature expansion is stopped during migration. Native save version1 is isolated from Unity save6 and design-only economy save7.

# Gameplay review of rules v4 (T20)

**T21 adoption update:** under delegated ordinary-design authority, this review's L01 N4/K2/P3 two-transfer tutorial is ADOPTED. RULES §2.1 and FIXED_01_10 own the effective exception. One upper receiving nest/staggered supported branches are selected, subject to internal technical and independent visual QA. Statistics below describe the pre-adoption40; all other proposals remain unadopted. Historical requests for repeated user approval are not current blockers.

2026-10-31 · Review and proposals only. Nothing here changes [RULES](spec/RULES.md), the fixed levels or code. Adopting a proposal means editing RULES and the owning documents first.

## 1. Verdict

The core loop is sound and standard for the genre: select an exposed run, place it on an empty or same-species end, a full group of N flies home and its branch retires. Partial moves, `?` eggs, free Undo/Restart/Hint and one free extra branch are clear and fit a low-pressure casual game. **No rule needs to be removed.**

The problem is level content, not rules: the 40 fixed levels in [LEVEL_SIMULATIONS](LEVEL_SIMULATIONS.md) contain almost no decisions, and the first level does not deliver a clear first experience.

## 2. Evidence: the fixed levels are not puzzles

Measured with `tools/levels/perch_levels.py` (exhaustive state search under RULES v4 plus 300 random-walk plays per level):

| Measure | Result over levels 01–40 |
| --- | --- |
| Random tapping wins | 100% in 23 levels; ≥ 85% in 37 levels; lowest is level 39 at ~50% |
| A first move that can lose | none in any level |
| Reachable states with no way to win | ≤ 3% in 39 levels (level 39: 6%) |
| Species per level | 3–6; world A is 8 identical N4/K3/P5 boards with 4–8 moves |

Random deals under the **same rules** become real puzzles once there are more kinds: with N4 and two spare branches, K8–9 gives random-win 3–50% and 5–26% dead states. The original levels are easy because they were hand-built with few segments and few kinds.

## 3. First levels (highest priority)

The first level must teach exactly one idea and pay off on the first tap. Verified with the tool:

| Level | Board (wall → exposed end) | What the player experiences |
| --- | --- | --- |
| 01 | S1 `A A A` · S2 `B B B A` · S3 `B` (N4, K2, no empty branch) | Exactly **one** legal first move: the exposed A on S2 joins S1 and the first flock flies home immediately. Second tap: the three B go to S3 (or the single B to S2) and the level ends. 2 moves, ~15 s |
| 02 | S1 `A A B B` · S2 `C C A A` · S3 `B B C C` · S4 empty (N4, K3) | Teaches the empty perch and moving a pair together. 3 equally good first moves into S4, then one forced line; three homecomings in 4 moves |
| 03 | to design | First real decision: at least one tempting move that leads to a dead end (recoverable with Undo), and one move that is truncated by space |

Level 01 replaces the current `A3·B / B3·C / C3·A + 2 empty` board, where all six first moves are equally correct, two empty branches distract, and nothing tells the player what problem to solve.

## 4. Proposals for later levels (lower priority, not adopted)

Ordered by value:

1. **More kinds, not more mechanics.** Raise K to 7–9 in later worlds (N4). This alone creates dead ends and planning.
2. **Starting layouts as content.** Besides "K full + 2 empty", allow "K full + 1 empty" (`lean`) and "no empty branch, free space scattered" (`tight`). This is level data, not a new rule.
3. **Drop N8.** Eight birds need 8 × 80 = 640 design px, wider than one 540 px column of the approved board (UI_SPEC UI-D36/UI-D38). Use N4 and N6.
4. **Allow stacked eggs.** RULES R03 already hatches only the exposed egg, so stacked eggs are deterministic; SRC-BOOP/SRC-MAGIC hide layers the same way. The adjacency ban only reduces variety.
5. **Short twigs (optional, world C+).** SRC-NUT mixes rod lengths in one level. A twig of capacity 2–3 accepts and releases birds but never completes a group.
6. **Difficulty gates.** Accept a level only if its random-walk win rate and dead-state share fall in a band for its role (teach / practice / challenge / rest), and neighbouring levels differ in layout or mechanic.

Hidden-egg fairness: with free unlimited Undo every guess is recoverable, so eggs are fair enough for casual play; confirm in playtests rather than with more documents.

## 5. Next step

Priority order agreed with the user: settle gameplay → produce UI mockups for the user to approve → hand off UI implementation. Concretely: approve levels 01–03 above, then mock the first-level screen (board, nests, tutorial hand) for approval.
