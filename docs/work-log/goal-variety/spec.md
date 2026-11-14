# Goal variety — spec

Status: proposal for the user's decision. Nothing in this page is implemented; no code and no level data change.
Issue: #251. Owner: Claude (coordinator).

## 1. Why, and the bar every goal must clear

Every level today has one goal: send every flock home. The user wants levels that ask for different thinking. The
lessons of the Dive pilot ([gameplay depth](../gameplay-depth/spec.md), section 8) set the bar:

1. **Readable from one picture.** The user rejected Dive because he could not tell what the bird did. A goal that
   needs words is dropped.
2. **Calm pillar.** No timers, no lives, no fail screen ([design record](../../GAME_DESIGN.md), section 0).
3. **Winnable without tools.** Every level keeps a stored tool-free solution replayed by `tests/run_core.gd`.
4. **Business goal.** The game earns by building the habit of using tools (undo, hint, extra branch, the stuck
   rescue) that are paid by rewarded ads. A good goal creates moments where a tool is the natural next step, without
   making the level unfair.
5. **Fewer, readable mechanics.** The user prefers one clear idea over several clever ones.

The natural place to show a goal is the row of bird keys under the house (`BoardView._build_skill_row`): one key
per kind of bird in the level, a check once its flock is home ([skill row](../skill-row/report.md)). Every
presentation below changes only what that row and the house show; no text, no new HUD panel.

## 2. Method and measured facts

A throwaway model of the plain rules (transfer, homecoming, cascade, the last-flock auto-home of
`Rules._auto_home`; no eggs, sleepers or special branches) was written outside the repository, plus each candidate
goal. It was checked against the real `Solver.solve` on the current rules: for every example below whose plain form
the current rules can express, the model and the real solver give the same shortest length. The two recommended
goals were also run on a scratch copy of `PerchSort/core` (outside the repository) with the goal patched in, and
each example's solution was replayed through a real `GameSession` to a win. Nothing of this is committed.

Random boards, capacity 4, species chosen so that homecoming gifts have no effect (sunny, mint, cocoa, ember: no
sleepers, growing branches, twigs or crows to act on). "Pairs" = (board, named flock) pairs.

| Candidate | Boards | Optimum unchanged | Optimum longer | Becomes unsolvable |
| --- | --- | --- | --- | --- |
| (a) order, 3 kinds, 5 branches, one empty | 177 pairs | 138 | 39 (+1 or +2) | 0 |
| (a) order, 3 kinds, 4 branches, none empty | 306 pairs | 203 | 91 (+1 or +2) | 12 |
| (a) order, 4 kinds, 5 branches, none empty | 88 pairs | 49 | 18 (+1 to +3) | 21 |
| (b) cage on a root bird, 3 kinds, 5 branches, one empty | 536 placements | 536 | 0 | 0 |
| (b) cage, 3 or 4 kinds, no empty branch | 963 placements | 949 | 0 | 14 |
| (d) partial, every kind a full flock, 3–4 kinds, one or two named | several hundred | the goal route plus the rest of the sort was never more than 2 moves over the plain optimum: the goal only shortens the level | — | — |
| (d) partial, leftover birds that cannot form a flock | about 7000 random deals | solvable dig-out puzzles of 6–9 moves found readily | — | — |

What the numbers say:

- The **order goal** bites only on tight boards, but there it bites often, so a
  generator can pick the levels where it matters (a fifth to a third of the pairs on boards without a free branch). It also creates dead ends (positions that can never be won) where
  the plain board had none, which is where undo and the rescue become useful.
- The **cage** never made a level longer. It only removes the option of moving one bird away, which the shortest
  route almost never wants. It is decoration, not a goal.
- The **partial goal** with full flocks of every kind only cuts the level short. It becomes a different puzzle
  only when the other birds cannot all go home, so they must be parked instead of sorted.

## 3. Candidates

### 3.1 (a) Order goal: a named flock must go home first

**Rule.** A marked level names one kind of bird. Until that flock is home, the house door is shut for every other
kind: a branch that fills with four birds of another kind keeps them; they cheer and wait. The moment the named
flock goes home the door opens, and every waiting flock goes home in the same move (a cascade, so the Chorus chain
counts all of them). Waiting birds can still be picked up and moved like any birds. Nothing is lost and nothing
fails; finishing another flock early only costs a branch until the named flock is home.

**Wordless presentation (one picture: "this bird first").**

1. The named bird's key leads the row under the house, raised a little, with a gold ring and a painted `1` glyph
   (numbers are painted glyphs, `ButtonKit.number`).
2. Every other key carries the existing padlock picture (`pic_lock`) at the corner where the check appears later.
3. A flock that fills while the door is shut cheers, looks up at the house, the house's door gives a short shake
   with the padlock, and the birds settle facing the house with a slow bob (continuous breathing as always). This
   is the moment the rule is learnt, so the first trial level makes it happen on its third move.
4. When the named flock goes home: the padlocks in the row pop open one by one, the waiting flocks fly in, the
   Chorus ribbon shows the chain.

Risk to watch: a full branch that does not leave can look like a bug. The padlock on the keys and the shaking door
are the answer; if the user still reads it as a bug, the goal is dropped.

**Solver, session and stars.**

- Level field `"firstHome": "<species>"` read by `Levels.create_board` into a new `Board.first_home` (copied by
  `Board.clone`; -1 when absent).
- `Rules.can_home` refuses a branch whose kind is not `first_home` while `first_home` is not in `homed`. The
  cascade in `Rules._resolve` already loops while anything changed, so the waiting flocks leave in the same move
  with no further change. `_auto_home` needs no change (it only fires for the last flock, which is then the named
  one or the gate is already open).
- A new event (`flock_waiting`, branch and species) for the animation, emitted once when a branch fills while the
  door is shut.
- The solver, hints, undo and the rescue work unchanged through `Rules`: the gate depends only on whether the named
  kind is in `homed`, which `Board.search_key(false)` already encodes as a mask.
- `GameSession.stars_for` unchanged; `par` is the shortest route under the gate.

**Stored solutions and the 210 levels.** No level has the field, so no rule changes for them; `hints.json` keys
and routes are unchanged; `tests/run_core.gd` replays them as today. New levels get their routes from
`tools/levels/export_hints.gd`, which already builds boards through `Levels.create_board`.

**Worked example ("cocoa first", 4 branches, 3 kinds, par 8).**

```json
{ "id": 9001, "world": 1, "variant": "noon", "capacity": 4, "par": 8, "firstHome": "cocoa",
  "branches": [
    { "side": "L", "row": 0, "birds": ["cocoa", "sunny", "cocoa"] },
    { "side": "R", "row": 0, "birds": ["mint", "mint"] },
    { "side": "L", "row": 1, "birds": ["cocoa", "mint", "sunny", "mint"] },
    { "side": "R", "row": 1, "birds": ["cocoa", "sunny", "sunny"] } ] }
```

Branch indices: 0 = L0, 1 = R0, 2 = L1, 3 = R1. Shortest route under the gate (8 moves):

| Move | Action | Result |
| --- | --- | --- |
| 1 | 2→1 | mint lands on mint: R0 `mint mint mint` |
| 2 | 2→3 | sunny lands on sunny: R1 `cocoa sunny sunny sunny` |
| 3 | 2→1 | R0 holds four mint: they **wait** (cocoa is not home); L1 `cocoa` |
| 4 | 2→0 | cocoa onto cocoa: L0 `cocoa sunny cocoa cocoa`; L1 empty |
| 5 | 3→2 | three sunny to the empty branch; R1 `cocoa` |
| 6 | 0→3 | two cocoa onto cocoa: R1 `cocoa cocoa cocoa`; L0 `cocoa sunny` |
| 7 | 0→2 | L1 holds four sunny: they wait; L0 `cocoa` |
| 8 | 0→3 | four cocoa go home, then mint and sunny follow in the same move (Chorus x3) |

Checks: without the gate the same board is solved in 6 by the real solver on the current rules (moves 1–3 the same,
the mint flock leaves at once and frees R0). With the gate the patched core solves it in 8 and the session replay
wins. The first three moves of the route are the same as the plain route and none of the alternatives in those
three steps loses (the generation rule of [gameplay depth](../gameplay-depth/spec.md) 3.1); 4 of the 21 reachable
positions under the gate can never be won, against none on the plain board, so undo has a real job later in the
level.

**Verdict: keep, second.** One picture, no new move, uses the payoff the game already has (the cascade). Weak spot:
the waiting flock must not read as a bug.

### 3.2 (b) Rescue goal: a caged bird is freed when its three flock mates sit beside it

**Rule.** A marked branch holds a caged bird in its root seat. The caged bird cannot be picked up and blocks
nothing behind it (it is the root). When the three other birds of its kind sit on that branch, the cage opens and
the flock goes home. A caged bird in any other seat could never be freed (no birds can sit "beside" it on both
sides with three of its kind), so the cage is only allowed at the root.

**Wordless presentation.** The cage picture over the bird (the shop's `house_style_cage_body` art could be derived
from), and its key in the row inside a small cage. One picture works: "bring three friends to the cage".

**Solver, session and stars.** A bird flag cannot be added: the five flag bits (egg, asleep, mama, chick, revealed)
are full, and moving the species bits would change every bird code and therefore every stored key in `hints.json`.
It would be a branch field (`"cage": true`), a new bit in the branch word of `Board.search_key` that is zero for
every existing branch (keys unchanged), `Board.run_count` stopping above the root of a caged branch, and the cage
dropping when the flock leaves. Stars unchanged.

**Stored solutions and the 210 levels.** Unchanged (no level has the field).

**Worked example.**

```json
{ "id": 9002, "world": 1, "variant": "noon", "capacity": 4, "par": 6,
  "branches": [
    { "side": "L", "row": 0, "cage": true, "birds": ["sunny", "mint", "mint"] },
    { "side": "R", "row": 0, "birds": ["mint", "cocoa", "sunny"] },
    { "side": "L", "row": 1, "birds": ["cocoa", "cocoa", "sunny", "mint"] },
    { "side": "R", "row": 1, "birds": ["cocoa", "sunny"] },
    { "side": "L", "row": 2, "birds": [] } ] }
```

Route (6): 1→3, 1→4, 0→1 (the two mint leave the cage branch), 2→1 (mint home), 2→0, 3→0 (three sunny join the
caged sunny: cage opens, sunny home; cocoa is the last flock and goes home by itself). The real solver solves the
same board without the cage in the same 6 moves, along the same route.

**Verdict: drop.** In 1499 measured placements the cage never made a level longer; it only made 14 unsolvable.
The picture is readable, but the rule changes nothing the player must think about, and it repeats the sleeper ("a
bird you cannot move until its kind arrives"). The cage art could still dress the goal of 3.4 later.

### 3.3 (c) Move budget as a hard limit on marked levels only

**Rule.** A marked level has a move limit. When the counted moves reach it without a win, no further move is
accepted; the board offers Undo, the rescue or Restart.

**Wordless presentation.** Easy: the moves number already in the status pill (`_refresh_status`, the star budget)
would count down to the limit, and at zero the pill pulses and the stuck pointer shows Undo.

**Solver, session and stars.** `moveLimit` on the level and the board; `Rules.can_transfer` and `can_swivel`
refuse at the limit, so the solver respects it. The remaining moves would have to enter `Board.search_key`,
otherwise the hint search and the hint cache in `BoardView` would treat one position with 2 moves left and with 9
moves left as the same; Encore (berry, ink) makes counted moves differ from actual moves. `stars_for` unchanged.

**Stored solutions and the 210 levels.** Unchanged if the field is absent and the extra key word is only added on
marked levels.

**Worked example.** The plain board of 3.1 without `firstHome`, with `"moveLimit": 9` (par 6 plus the two-star slack
`TWO_STAR_SLACK` 3). Solvable: the real solver finds 6 moves on the current rules.

**Verdict: drop.** It is a fail state in all but name, which the calm pillar forbids, and it would be the strongest
"pay to continue" moment in the game, which is exactly the pressure the pillar excludes. The soft form of this goal
already exists: the star budget in the status pill. If the business needs more pressure, a fourth star or a
"perfect" reward on the existing budget is safer than a hard limit.

### 3.4 (d) Partial goal: only the named flocks must go home

**Rule.** A marked level names one or two kinds of bird. The level is won when those flocks are home. The other
birds on the board are **leftovers**: a few birds of other kinds (at most two of a kind, so no leftover kind ever
looks like a flock missing one bird) that can never go home. They move and land by the normal rules, so they must
be parked where they do not bury the named birds. At the win the leftovers fly off the screen edge, as crows do
when they leave (`crows_left`), so the board ends empty.

Measured: with full flocks of every kind the partial goal only shortens the level (the route to the named flock is
almost always the start of a full sort). The leftovers are what makes it a different question: not "sort
everything" but "dig this flock out and park the rest".

**Wordless presentation (one picture: "bring these home").**

1. The row under the house shows only the named kinds, each key with an empty check ring at the corner (the
   existing `pic_check`, shown as an outline until the flock is home). The row is the to-do list: when every ring
   is ticked the level is won.
2. Leftover kinds get no key, so the row stays short. Holding a leftover bird still opens its card, as for any bird.
3. The level card's species rings (`Popups.level_card`) show only the named kinds.

**Solver, session and stars.**

- Level field `"goal": ["mint"]` read into a goal mask on the board (`Board.clone` copies it).
- `Board.is_won` true when every named kind is in `homed`. The solver, hints, the rescue and `has_legal_move` all
  go through `is_won`, so they follow. `Board.search_key(false)` already encodes `homed` as a mask; the goal is a
  constant of the level, so no key change.
- `Levels.create_board` validation: today every real kind must have exactly `capacity` birds. On a level with a goal,
  a kind outside the goal may have 1 or 2 birds. Named kinds keep exactly `capacity`.
- `_auto_home` needs no change: it only fires when the last real kind is alone on the board.
- `GameSession.stars_for` unchanged; `par` is the shortest route to the goal.
- Gifts of named flocks apply as today; leftovers never go home, so they give no gift.

**Stored solutions and the 210 levels.** No level has the field: rules, validator, `hints.json` and the replay are
unchanged.

**Worked example ("dig the mint out", 5 branches, par 6).**

```json
{ "id": 9003, "world": 1, "variant": "noon", "capacity": 4, "par": 6, "goal": ["mint"],
  "branches": [
    { "side": "L", "row": 0, "birds": ["mint", "sunny"] },
    { "side": "R", "row": 0, "birds": ["mint", "cocoa"] },
    { "side": "L", "row": 1, "birds": ["cocoa"] },
    { "side": "R", "row": 1, "birds": ["ember", "sky", "mint", "sunny"] },
    { "side": "L", "row": 2, "birds": ["mint"] } ] }
```

Four mint (the goal); leftovers two sunny, two cocoa, one ember, one sky. Branch indices 0 = L0, 1 = R0, 2 = L1,
3 = R1, 4 = L2. Route (6): 1→2 (cocoa joins cocoa), 1→4 (mint joins mint), 0→1 (sunny parks on the emptied branch),
0→4 (three mint), 3→0 (sunny parks), 3→4 (four mint: home, win; ember, sky and the parked pairs fly off).

Checks: the current validator rejects this level (the leftovers are not full flocks), so it cannot be checked on the
current rules. On the patched core copy the solver finds 6 moves and the session replay wins with the six leftovers
still on the board. In the model no move in the whole route loses, and 14 of 325 reachable positions can never be
won (a leftover parked on the last free branch), so the hint and the extra branch have a natural use.

**Verdict: keep, first.** The clearest of the four: the row under the house becomes a to-do list. It makes a new
kind of level (short dig-outs between long sorts), needs the smallest engine change (`is_won` and one validator
rule) and touches no move rule, which is where Dive failed. Weak spot: it needs leftover birds, which relaxes the
"exactly four of every kind" rule; a player may try to gather two leftover sunny and wonder why nothing happens.

## 4. Recommendation

Adopt **(d) partial goal** first and **(a) order goal** second; drop (b) and (c).

- (d) is the most readable: one empty ring per named kind, filled when that flock is home. It changes what wins,
  not how birds move, and gives short dig-out levels that vary the pace of a world.
- (a) is readable through one picture (a `1` on the first key, padlocks on the others) and turns the game's best
  moment, the homecoming cascade, into the payoff of a plan. It bites only on tight boards, so levels must be picked
  by the generator.
- Both create dead ends that the plain boards do not have, so undo, the hint and the rescue (the tools paid by
  rewarded ads) get real work, while the stored route keeps every level winnable without tools and the generation
  rule keeps the first three steps safe.
- (b) changes nothing measurable; (c) breaks the calm pillar.

If the user wants only one goal, take (d).

### 4.1 Files that would change (on the user's word, in a separate task)

| File | (d) partial | (a) order |
| --- | --- | --- |
| `PerchSort/core/board.gd` | goal mask, `clone`, `is_won` | `first_home`, `clone` |
| `PerchSort/core/levels.gd` | parse `goal`; allow 1–2 leftovers of kinds outside the goal | parse `firstHome` |
| `PerchSort/core/rules.gd` | none expected | gate in `can_home`; `flock_waiting` event |
| `PerchSort/core/session.gd`, `solver.gd` | none (through `is_won`) | none (through `Rules`) |
| `PerchSort/game/board_view.gd` | key row: goal kinds only, empty rings; leftovers fly off at the win | key row: `1` and padlocks; waiting flock and door animation; padlocks open |
| `PerchSort/game/popups.gd` | level card shows the named kinds | level card marks the first kind |
| `PerchSort/game/main.gd` | trial launcher `--trial=<n>` (shared) | same |
| `PerchSort/levels/trial.json` (new) | trial levels | trial level |
| `PerchSort/tests/run_goals.gd` (new) | win with leftovers, validator, undo, solver | gate, cascade, undo, solver |
| `art/` | an empty check ring derived from `pic_check` (needs art authority) | none expected (`pic_lock` exists) |
| `docs/GAME_DESIGN.md` | §1 win rule, §12 level fields | same |

The 210 levels, `hints.json`, the save and `meta/` are not touched.

### 4.2 Three-level trial plan

Trial levels live in `PerchSort/levels/trial.json`, open only through a launcher argument (`--trial=<n>`), and are
not part of the catalog, the map or the save. Existing art only, apart from the check ring.

| Trial | Goal | Intent | Shape | Accepted by tools when |
| --- | --- | --- | --- | --- |
| 1 "Bring mint home" | (d) | teach: the row is the to-do list | the worked example of 3.4: 5 branches, one named kind, 6 moves | solvable without tools; no losing move in the first three steps |
| 2 "Dig deep" | (d) | a real dig: two named kinds, leftovers must be parked with care | 6–7 branches, two named kinds, at most two leftovers per kind, 10–14 moves | as 1, and the route parks at least three leftovers |
| 3 "Cocoa first" | (a) | the gate: an easy flock fills, waits, and the cascade pays it back | the worked example of 3.1: 4 branches, 3 kinds, 8 moves | solvable without tools; at least 2 moves longer than without the gate; no losing move in the first three steps |

Evidence for the trial task: `bash scripts/run_tests.sh` with the 210 levels replaying unchanged, the new goal test,
stills of the three key rows, and timed frame sequences (`--frames=<dir> --step=0.25`) of a leftover parking and the
win fly-off (trial 1) and of the waiting flock and the triple cascade (trial 3). Then the user plays the three
levels and judges, as for Dive: was each goal understood without words, and was there a moment that needed thought.

### 4.3 Risks

- A waiting flock (a) can look like a bug. If the padlocks and the door shake do not explain it, drop (a).
- Leftover birds (d) can look like flocks the player failed to finish. At most two per kind limits this; the trial
  will show whether it is enough.
- Both goals add dead ends. The generation rule (no losing move in the first three steps) and the existing rescue
  keep this fair, but a level built only to sell tools would break the "winnable without tools" promise in spirit.
- The order goal bites only on tight boards; worlds must not be filled with them or every board feels cramped.
- Measurements come from a model of the plain rules; eggs, sleepers and the special branches were not modelled.
  Each real trial level is checked on the real core once the goal is built.
- Not played by anyone yet.

## 5. Open questions for the user

1. Is the partial goal with leftover birds acceptable, given that it relaxes "exactly four of every kind" on marked
   levels?
2. Should the order goal be tried as well, or only the partial goal first?
3. At the win of a partial-goal level, should the leftovers fly off (proposed) or stay on the board?
4. May the empty check ring for the key row be derived from the existing check art (art authority)?
5. How often should goal levels appear once accepted: for example two or three of the 21 levels of a world?
