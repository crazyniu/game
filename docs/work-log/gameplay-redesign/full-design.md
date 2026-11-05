# Full gameplay design: why it is not fun yet, and three complete directions

Status: PROPOSAL for user decision. Supersedes the incremental table in proposal.md. Nothing is implemented. Issue #105.

## 1. Diagnosis: why the game feels flat
1. It is a solved-on-sight solitaire. The board is fully visible and deterministic; once the player sees the structure there is one kind of decision (which run goes where) and no surprise, no risk, no tension and release.
2. The identity is hidden. The one genuinely original idea (each species leaves a different gift, so finishing ORDER matters) is invisible and automatic: the player cannot choose, cannot see it coming, and gets no moment of "I used it at the right time".
3. Mechanics are obstacles, not toys. Eggs, sleepers, crows, twigs, swivel are things to get past. Sleepers and day/night make the player WAIT, which is the opposite of agency.
4. Every level asks the same question ("finish all flocks"). Variety comes only from a harder board.
5. Skills are consumables bought with coins. They are generic helpers with no personality and no skill expression.
6. Replay and mastery are thin: three stars from a move count only.

## 2. What makes a sort puzzle fun (design rules used below)
- A decision the player is proud of (timing and order, not only placement).
- Foresight: a visible near-future the player can plan against.
- Tension and release: build, then a satisfying chain payoff.
- Different questions in different levels (goal variety).
- Skills the player earns and spends with judgement.
- Always readable: icon-only, no waiting, no hidden rules.

## 3. Direction A: "Song Flock" (evolve the engine, lowest risk)
Hook: every flock that goes home sings a song you can SPEND later.
- One Song Nest at the top replaces the 2 to 7 houses. Each finished flock fills one nest slot and turns its species gift into a glowing ability button on the nest. The player chooses when to use it (once per species per level). Abilities = the existing nine gifts: wake all sleepers, reveal eggs, grow branches, scare crows, free move, break the species rule once, hatch eggs, flip the sky, three moonlit moves.
- Bought boosters shrink to Undo, Hint, Extra Branch.
- Sleepers wake only in clear active ways (a same-species friend lands next to it, or the Wake ability). Day/night becomes an on-demand ability instead of a countdown. No waiting.
- Goal variety on the same engine: Classic (all flocks home); Order (deliver species in the shown order); Efficiency (finish in at most N moves for a bonus branch); Pure (finish without using an ability).
- Three star goals per level: finish, finish under par, finish without abilities.
Differentiator: gift abilities as an earned, timed resource. Fun gain: medium. Cost: changes when gifts apply, so the Core, solver and levels from 3 on are re-solved.

## 4. Direction B: "Forecast Forest" (recommended core idea): add foresight and tension
Hook: the forest has a visible forecast; you plan around what is about to happen.
- A forecast strip shows the next three events as icons. After every third move of yours, the next event fires, with a clear telegraphed animation (warning 1 s before). No timer; undo is free; events are deterministic.
- Events (each is one readable icon and animation):
  - Gust: the tip bird of the marked branch is blown to the nearest free seat on a neighbouring branch (can help or hurt).
  - Rain: the marked branch is wet for two of your moves (cannot be selected).
  - Sunshine: the marked branch grows one seat.
  - Night (replaces the day/night countdown): owls wake, day birds on the marked branch doze for two moves.
  - Storm (world 4 only): two gusts in one event.
- Each species gift becomes an ability that can cancel or redirect the next event (for example Sunny's song skips it, Sky's breeze moves the arrow to another branch, Ink flips night). The player is constantly asking "what is the next event, and how do I use or dodge it".
- Sleepers are now an event effect, shown clearly, and they wake on their own when the event ends (plus an active way).
Differentiator: foresight puzzle with environmental events, not found in standard sort games. Fun gain: high. Cost: a deterministic event engine in Core and the solver (events are part of the state), levels re-solved (events can be tuned per level), new UI for the forecast strip. Largest design risk: keep events legible and never unfair; mitigated by free undo, a preview of the affected seat and a hint that understands events.

## 5. Direction C: "Flock Choirs": new goals instead of colour sets (boldest)
Hook: do not collect colours, compose choirs.
- A branch completes when its birds form a pattern shown on the nest: a trio of three different species, a duet pair, a matched pair on both sides, a rainbow row.
- Pattern goals replace "N of one species". Birds are reused across patterns, so one move can serve two goals.
Differentiator: the strongest, but all 40 levels, the Core completion rule, the solver and the whole tutorial are redone. Fun gain: very high if it works, high risk. Not recommended as the next step.

## 6. Recommendation
Build A and B as one design ("Song Flock + Forecast"):
1. One Song Nest replaces the goal houses (presentation only).
2. Gift abilities earned by flocks (A).
3. Forecast events replace the day/night counter and the passive sleeper (B).
4. Goal variety and three star goals per level (A).
5. First-appearance, icon-only, 3-second animated teaching card for every mechanic and every event.
6. Meta (small, after the core): each cleared world restores a corner of a personal "Forest Home" with the birds you met, as a reason to continue (decoration only, no coins).

## 7. What stays
Fixed authored levels, forest worlds in order, the plush art, the drawn bird animations, the wood UI, Undo and Hint, the 10 species, eggs, mama and chicks, twigs, growing branches, swivel.

## 8. Delivery plan and cost
1. Nest and teaching cards (presentation, safe, visible quickly).
2. Core: gift abilities and event engine behind a rules switch, solver updated, LevelTool re-solves all 40 levels; levels that fail are redesigned with the tool.
3. UI for abilities and the forecast strip, new animations and sounds, tutorial levels per mechanic.
4. Goal variety and star goals.
Parallel tasks with separate file ownership as usual; every task is tested only where it changes things.

## 9. Decisions needed
1. Direction: A only, A+B (recommended), or C?
2. Houses: one Song Nest (recommended), none, or keep?
3. Events set: accept gust, rain, sunshine, night (and storm in world 4), or change the list?
4. Accept re-solving and redesigning levels where needed?
