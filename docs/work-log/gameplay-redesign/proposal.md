# Gameplay differentiation proposal (decision needed from the user)

Status: PROPOSAL ONLY. Nothing here is implemented. Core rules, the solver certificates and the 40 authored levels stay as they are until the user chooses. Issue #105.

## What the user reported
1. The game feels like other sort games; the skills/boosters are unremarkable (generic helpers, each only counters one mechanic, bought with coins).
2. Many goal houses add little. Is one house enough, or none?
3. The sleeping mechanic is unclear and weak: a bird sleeps by day and wakes at night, so the player just waits. Why can it not fly away? Tapping it gives only a sound.
4. Hint bug (two birds shuffled back and forth) and missing sounds are being fixed separately (#102).

## What is actually unique today (honest view)
- Unique: every species leaves a different gift when its flock goes home, so the ORDER of finishing flocks matters (docs/GAME_DESIGN.md 6.5). This is the real identity, but it is invisible: gifts are automatic and the player cannot see or use them as a decision.
- Generic: tapping a run to move it to an empty or same-species branch, undo, hint, extra branch.
- Weak: day/night (waiting), sleepers (opaque), six single-purpose boosters, seven goal houses.

## Area 1: goal houses
| Option | What it is | Effect |
|---|---|---|
| H1 (recommended) | ONE "Song Nest" at the top. Completed flocks fly into it; it fills with one coloured egg/feather per species as a progress row. Each species gift is shown as a small badge on the nest row. | Much less clutter, same information, one clear place for the homecoming animation. |
| H2 | No houses: a finished flock flies off screen and leaves a glowing feather on its branch; progress is a star row. | Cleanest, but loses the satisfying "go home" target. |
| H3 | Keep houses | Not recommended. |

## Area 2: skills (the main differentiator)
| Option | What it is | Effect |
|---|---|---|
| S1 (recommended) | "Gift abilities": when a species flock goes home, its gift is NOT applied automatically. It becomes a charged ability button (icon on the nest). The player taps it when it is useful (wake all sleepers, reveal eggs, grow branches, scare crows, free move, break the species rule once, hatch eggs, flip the sky, three moonlit moves). One charge per species per level, shown as glowing nest badges. Coins-bought boosters shrink to Undo, Hint and Extra Branch. | Skills become earned by play, themed per bird, and tied to the real strategic decision: which flock to finish first AND when to spend its power. Nothing comparable in standard sort games. |
| S2 | Keep the six boosters, only reskin | Not a real change. |
| S3 | Bird companions: pick 2 species as "helpers" before the level; their ability is available from the start once per level | More variety, more UI, later. |

## Area 3: sleeping and day/night
| Option | What it is | Effect |
|---|---|---|
| Z2 (recommended) | Sleepers wake in clear, active ways only: (a) bring a friend of the same species next to it, or (b) use the Wake ability. Day/night becomes an on-demand ability (Ink gift: "flip the sky" for one move) instead of a countdown: owls act only at night, so the player CHOOSES when to flip. No waiting. A first-time icon-only explainer card appears when the mechanic first shows; tapping a sleeper shows a bubble with the wake icon; Zzz loops clearly. | Removes pure waiting, adds a decision, readable. |
| Z1 | Remove sleepers and day/night entirely | Simplest, loses a mechanic. |
| Z3 | Keep as is plus clearer feedback | Does not fix "waiting is pointless". |

## Area 4: teaching
Every mechanic gets a one-time, icon-only, 3-second animated card on its first appearance (egg, sleeper, crow, twig, growing branch, swivel, night, mama and chicks, each ability). No text paragraphs.

## What changes in code and data
- S1 changes when gifts apply, so the Core rules, the solver and every level that depends on a gift order must be re-solved and re-certified with the LevelTool; some levels may need small edits. This is the largest cost (Core + levels + UI + tests).
- H1 is presentation only (BirdhouseView, HUD, entrance), one nest instead of 2 to 7 houses.
- Z2 changes day/night from a counter to a player-triggered state; Core + levels (worlds 3 and 4) need re-validation.
- Teaching cards are presentation only.
Suggested order: H1 + teaching cards first (safe, visible), then S1 and Z2 together (one Core change, one re-certification).

## Decisions needed
1. Houses: H1 / H2 / H3?
2. Skills: S1 / S2 / S3?
3. Sleep and day/night: Z2 / Z1 / Z3?
4. Do you accept re-solving and possibly editing some of the 40 levels for S1/Z2?
