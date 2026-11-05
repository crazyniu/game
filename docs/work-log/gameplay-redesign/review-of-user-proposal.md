# Coordinator review of the user's proposal (issue 114)

The user's note: the proposal is only a suggestion; adopt what is reasonable. This is the decision record. Nothing here changes code; gameplay work starts after the current UI and bug batch is merged (user decision).

## Cross-check with the coordinator's own measurements
The proposal's findings match the measured level data (median shortest solution 8 moves; 12 of 40 levels at 3 or fewer; sleepers in 30 of 40 levels): the repeated 3-step teaching openings, the gap in mama-and-chicks review (levels 19 to 38), the late jump from level 6 (5 moves) to level 7 (13 moves), and the Inky and Hoot gifts that never get a use in the shipped levels. These are real and cheap to fix because they are LEVEL and SCORING work, not engine work.

## Decisions
| Item | Decision | Reason |
|---|---|---|
| Core direction: choose the homecoming ORDER, birds help each other, planned chains | ADOPT as the base of the redesign | It is the one idea that is actually ours, it needs no new rule, and it fits short levels because every move is part of an order decision. |
| Each challenge level compares two routes (obvious vs better) with a visible cause | ADOPT | Gives each level a question. Authoring rule for every level: write the question in one sentence. |
| Family rescue puzzles with mama and chicks | ADOPT (levels 15, 16, 18, 39 plus new review levels in 19-38) | Uses existing rules. |
| Chains as an optional, readable bonus; Chorus stays | ADOPT as optional | Never a fail condition. |
| Keep fixed levels, no timer, free undo, no new obstacles, no vanishing branches, no stamina | ADOPT | Matches the calm pillar. |
| One-way special perches, multi-layer flocks | REJECT for now | The proposal itself says not unique and needs recovery design. |
| Fix Inky and Hoot level integration (night call and moonbeam need a beneficiary and a real use in levels 34 and 37 area) | ADOPT (level edits) | A shipped gift that does nothing is a content bug. |
| Smooth level 6 to 7 difficulty, new species and mechanic one at a time; teach the ability a character brings at its debut | ADOPT | Level edits. |
| Star rules: 1 star any clear; 2 and 3 stars from per-level targets T2 and T3 (not a blanket par+3); no time scoring; keep best stars | ADOPT, with a data field per level | Needs a small LevelDef extension (targets) and the solver to compute the counted-move optimum Q (with Berry's free move). |
| Show counted moves and star targets in the HUD and results; explain the free move | ADOPT NOW (UI only) | Cheap, safe, solves the confusion without rule changes. |
| Coin top-up when a replay raises the star count | ADOPT, low priority | Small economy tweak, no schema change beyond the existing best stars. |
| Hidden-egg fairness evidence by visible information | ADOPT as a check when levels with eggs are revised | The solver sees the eggs; a visible-information check is needed. |
| Prototype five representative levels first (7, 11, 16, 34, 40) | ADOPT | Same as the coordinator's prototype-first plan. |
| Playtest with 8 to 12 unseen players | MODIFY | We have one tester (the user): they play each revised level and report; qualitative only. |
| Documentation sync (CURRENT_GAME still mentions placeholder shop, dev flags) | ADOPT now (docs only) | Cheap. |

## What the proposal does not solve (coordinator addition)
It deepens the existing game but does not by itself make the first minutes feel different from other sort games. So the plan has two phases:
- Phase 1 (adopted from the proposal): levels, scoring and feedback so that order decisions are visible and every level asks a question. Mostly data and UI, little engine work, low risk.
- Phase 2 (only if the user still finds the game flat after Phase 1): the prototype of species talents that trigger when a bird LANDS (so species matter on every move, also in 3-move levels), tried on three levels before any rebuild.
Sleepers: reduce their share (30 of 40 levels is the default texture of the whole game); keep them in a few teaching levels with an active, visible wake rule.

## Order of work
1. Now: finish the UI and bug batch (popups, glyph sizing, single nest, front-facing birds, input lock, aviary counts).
2. Then Phase 1 step A (safe): HUD and results show counted moves and star targets; docs sync.
3. Phase 1 step B: revise the five representative levels with the LevelTool, user plays them.
4. Phase 1 step C: extend to all 40 levels with per-level star targets.
5. Decide on Phase 2 from the user's feedback.
