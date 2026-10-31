# Sorting puzzles: a textual guide to same-genre rules

Online recheck 2026-10-31. Sort Puzzle / Sorting Puzzle means categorization puzzles: Water Sort pours liquids, Ball Sort moves balls, Bird Sort moves birds, and Nut Sort moves nuts. They chiefly share the loop “take exposed matching items→receive on empty/matching ends→plan capacity,” rather than match-three or ordering by size.

**This page explains gameplay without requiring external links.** Store/official pages are developers' first-party statements, not installed playtests; do not fill unpublished algorithms from common sense and call them facts. See the [complete evidence matrix](MECHANIC_EVIDENCE.md) for adoption/withdrawal/source keys and [RULES](spec/RULES.md) for the sole effective rules. Locally authored examples only explain mechanics; they are neither original competitor levels nor complete solution proofs. The old research conclusion allowing innovation based on plausibility is withdrawn.

## SRC-LEAF: Bird Sort / Leafy Games

**Original game's textual rules**: Tap the bird to move, then the destination branch. Take only from the outer end; connected same-color birds move together. The destination must be empty or have a matching outer end, within finite capacity. Fewer moves score higher; Undo or restart when stuck. No countdown; bird/branch/background appearances can be unlocked.

**Example (locally authored)**: Outer-end A2 birds block B; find an empty branch or A end for A2 before exposing B. The source does not say whether a destination with room for only one truncates or rejects the move; “whole flock” cannot establish that boundary.

**Project adoption/differences**: Adopt selection, end runs, and empty/matching-end reception; SRC-SIMPLE separately supports capacity truncation. Best action-count recording has evidence, but no Undo star-penalty formula does. Appearance features do not establish prices/reward quantities.

Locator: About→HOW TO PLAY/FEATURES. [First-party source](https://play.google.com/store/apps/details?id=com.leaf.birdsort&hl=en).

## SRC-SIMPLE: Simple Games / Water Sort

**Original game's textual rules**: Each tube holds 4 units, each color totals 4, with K colors plus two empty tubes. Select source then destination. A nonempty source pours as much of its continuous same-color top run as fits into an empty or matching-top destination; excess remains at source. Invalid taps do nothing and incur no penalty. One pour counts as one move. At completion, every tube is empty or full of one color. Each level reproduces from the same seed and is offered only after finding a complete solution.

**Assistance/end state**: Free unlimited Undo reverses one pour and its move count. Hint gives the first step of a currently found solution without automatically pouring. Failed search does not prove a dead end. No purchase of extra empty tubes.

**Project adoption/differences**: Transfer min(run,free), one-action counting, free assistance, fixed puzzles, and solvability validation. This project authors fixed arrays rather than generating at runtime. Capacity 4 is directly supported here;6/8 come from SRC-BOOP. Branch loss on completion comes from SRC-BIRD2, without claiming this water game loses tubes.

Locator: Pouring/The tubes/Moves/Questions. [Developer rules](https://pixapps.ai/simple-games/games/water-sort/en/).

## SRC-QUEST: Bird Sort Quest / mobirix

**Original game's textual rules**: Touch birds to move them to another branch; only branch-end birds move, the destination needs space, and empty branches accept them. Gather matching birds on branches. Question-mark eggs hatch after birds in front move away. Restart, Undo/extra-branch tools, bird/branch/background appearances, daily-task coins, and rankings are available.

**Example (locally authored)**: Movable B blocks a ? egg; moving B reveals the next bird. Before reveal, do not assume it is the desired A.

**Project adoption/differences**: Adopt revealing eggs after front birds move away. The user's fixed-level requirement fixes hidden egg species in level data, without rerolling on restart. The source does not describe shell patterns announcing species; old known-color eggs are withdrawn. Extra branches are explicit, but seats/uses/prices are unpublished and cannot be supplied by guesswork. Consecutive eggs and combinations with completion-based branch loss require separate validation; no claim of successful installed playtests.

Locator: How to Play/Features. [First-party source](https://play.google.com/store/apps/details?id=com.mobirix.ldbis&hl=en).

## SRC-BIRD2: Bird Sort 2 / FALCON

**Original game's textual rules**: Tap birds then a branch to gather matching birds. Once a completed flock flies away, its branch is lost and cannot serve the next sorting operation. Official text also lists extra branches, stepwise return, shuffling, breaking same-color restrictions, and timer-freeze tools, with time/life-limited modes. It claims levels are solvable without items.

**Example (locally authored)**: Completing flock A also removes one usable branch; the next flock must use remaining space. The emptied completed branch is not a fresh buffer.

**Project adoption/differences**: Adopt only “complete flock→fly away→branch unavailable.” Reject the old invented rule “after receiving birds, any emptying retracts the branch”; ordinary emptying through transfer is not completion. The page fixes neither capacity nor flock size; our 4/6/8 have separate sources. Time/lives/shuffling do not automatically enter the main campaign with branch loss.

Locator: HOW TO PLAY item 2. [First-party source](https://play.google.com/store/apps/details?id=com.globalplay.birdsort2.color.puzzle&hl=en-US).

## SRC-BOOP: Water Sort / BOOPFUN

**Original game's textual rules**: Tap a bottle then another; pour only into an empty or matching-top bottle with space, completing single-color fills. Includes 6/8-layer long bottles, question-mark hidden colors, and collection bottles that accept input but cannot pour out. Supports Undo/restart/hints; repeatedly tapping reserve bottles makes them taller. Offline and untimed; official text also describes ads and ads replenishing Undo.

**Example (locally authored)**: A collection bottle receiving A cannot pour it back, unlike an ordinary buffer. A taller reserve container changes usable space during play.

**Project adoption/differences**: Use 6/8 as capacity sources. Added space/repeated height growth are sourced features, while exact initial height, increment, maximum, uses, and costs remain pending evidence. Fixed initial boards do not prohibit in-level expansion. Collection containers and unspecified expansion parameters stay outside formal 40-level numeric configurations; do not invent ad/coin rescue systems.

Locator: HOW TO PLAY/WHAT'S INSIDE. [First-party source](https://play.google.com/store/apps/details?id=com.boopfun.watersort).

## SRC-CUTIE: Cutie Sort / Easetouch

**Independent code reference:** [Cutie Sort gameplay reference (version 1.111, research only)](references/CUTIE_SORT_GAMEPLAY_REFERENCE.md) separately records cat selection, movement, matching, orders/waiting, completion, special mechanisms, items, and unknown boundaries. Static evidence supports ordinary end-group selection, configured capacity per layer, and immediate group-based progress after a full match. Execution-layer count clipping does not prove that every input entry permits partial moves, and completion checks do not additionally wait for order-delivery animation. Original input hashes and method locators remain in the reference.

This reference does not rely on phone observations this round or generalize historical six-cat clips to all levels. It changes neither the adoption/quarantine decisions below nor project RULES; readmission requires separate review.

**Official text explicitly states**: Tap cats onto another open shelf, group by color, and finish sorting all cats. Extra layers, crates, Undo, swap, and expansion exist. Text does not specify dual-active target counts, sleep freezing, six per flock, cumulative/specified-species crate formulas, or expansion numbers.

**Old video records and this round's limits**: Earlier video notes recorded six cats, top-target refreshes, sleeping poses, crates, and baskets. This round did not recheck locatable timestamps or complete boundaries. The notes have value but cannot supply a complete precise formula claimed as verified online. In particular, whether sleep freezes movement and which completion opens which crate require corresponding footage and counterexamples.

**Project decision**: Retain research facts, quarantining dual targets/sleep/exact conditional crates/single-bird baskets from effective rules this round. Swap's name does not identify swap objects; expand establishes existence, not slot counts. Preserving 40 old levels cannot justify retaining unsourced boundaries.

Locator: How to Play/Key Features. [Official](https://play.google.com/store/apps/details?id=sort.casual.color.cat.puzzle.match&hl=en); old reference [video 1](https://www.youtube.com/watch?v=fZHsuXpTwEE)/[video 2](https://www.youtube.com/watch?v=onptn300img).

## SRC-NUT: Nut Sort Mania / Gameware

**Original game's textual rules**: Move same-color top nuts onto an empty or matching-top rod; short/medium/long rods change space. Undo/hints/add rod are available. Official claims include no hidden randomness requiring luck, all levels solvable without items, diagnostics, and daily challenges; short timed challenges also exist.

**Project adoption/differences**: Reference capacity planning and validation without items; the page does not supply specific 4/5 short-branch numbers. Current 40 levels use uniform capacity within a level; evidence of differing lengths does not authorize inventing five-seat branches. Smart-diagnostic algorithms/prices/budgets are unpublished, so do not guarantee instant correctness in every state.

Locator: Features/How to play/Fair Play/Smart Assist. [Developer page](https://www.gamewareeurope.com/Public/games?id=nutsort).

## Other verified systems (not all enter the first version)

| Source | Confirmed in original game's text | Project handling/unknowns |
| --- | --- | --- |
| SRC-GURU: [Guru Ball Sort](https://play.google.com/store/apps/details?id=ball.sort.puzzle.color.sorting.bubble.games&hl=en) | Matching-top reception with space,4-ball bottles, Undo/add bottle/restart, offline and untimed | Capacity/assistance reference; single-ball wording does not prove every mode always moves one ball at a time; costs unverified |
| SRC-MAGIC: [Magic Water](https://play.google.com/store/apps/details?id=com.magic.relaxing.water.sort.colorgame.puzzle&hl=en) | Covers/ice/locked cabinets/shutters/question-mark water; map progress, wallpapers, action sounds; shuffle/add tube/events | Map supports the user's requested world map; exact mechanism triggers/prices unknown and must not be invented; no stamina/spinner adoption |
| SRC-AZURA: [Azura Bird Sort](https://play.google.com/store/apps/details?id=com.birdsort.color.sortpuzzle&hl=en) | Undo/restart/add branch, fewer-move sorting, completion coins unlocking items/backgrounds, increasing colors/bird counts | Progression feature is sourced; coin quantities/prices/unlock tables await evidence; no invented economy curve |
| SRC-TVC: [TVC Bird Sort](https://play.google.com/store/apps/details?id=com.birdsort.colorpuzzle&hl=en) | Species and branch counts increase across levels | Supports cross-level difficulty dimensions only, not proof that simply adding birds makes play enjoyable |

## Evidence ownership for world maps, progression, and animation requirements

The user's world-map/sequential-world-unlock request is USER_REQUIREMENT. SRC-SIMPLE also has sequential level unlocking, SRC-MAGIC an adventure map, and [Bluebook's Magic Sort](https://apps.apple.com/gb/app/magic-sort-water-sorting/id6481261917) explicitly describes sorting while building/exploring differently themed towns. Do not confuse Bluebook's product with GrandGames' identically named game. Four regions 01–08/09–18/19–28/29–40 are this project's provisional configuration, not competitor values; do not add star thresholds, reward quantities, or coin-building formulas.

[Splashscreen Bird Sort](https://splashscreen.games/game/bird-sort) is a themed-route lead verified by the parent task; direct retrieval in this subtask was restricted and it is not the sole basis for any key rule. The user also requires open-source animation priority and forbids static images standing in for runtime animation. These are delivery/technical preferences, with library licenses/compatibility checked separately, not new gameplay.

## Rules for moving research into requirements

1. Every mechanic needs an evidence key and adoption differences; existence of a mechanism does not establish all formulas. Unverifiable details leave effective requirements.
2. Fixed 40 levels are a user constraint; layouts are project-authored content using the sourced combination above. Playability still requires new-rule replays and human acceptance, without citing old 60 paths as proof.
3. User maps, icons without duplicate text, regular button shapes, and mute-only music/sound/click controls are USER. Stable IDs, atomic saves, input locks/paths/pivots/timings are engineering/presentation contracts, not claimed competitor gameplay parameters.
4. Do not redraw/copy competitor art or transplant original levels. This round only organizes documents, without implementing code, runtime data, or assets.

## SRC-WSWEB: WaterSort.in browser supplement (ordinary branch addition)

On 2026-10-31, directly read Objective, Controls, and browser/Android differences on the [browser gameplay page](https://www.watersort.in/play). It explicitly states four units per bottle, one extra empty bottle per level, and reconstruction of the same puzzle. This is a first-party statement about that website's own product, not verification of Galaxy Android implementation; no browser interaction playtest occurred.

Transfer only “add one empty ordinary-capacity container”: bird-branch capacity uses defined N rather than claiming a sourced bird product also has six/eight seats. Free use follows the project's noncommercial scope. RULES R07 explicitly identifies Undo, stable IDs, saves, action counts, and assistance records as project/E integration, not unpublished source-site boundaries. This source does not prove repeated height growth or coin economies; they remain pending evidence.
