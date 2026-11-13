# Gameplay depth — spec

Status: proposal for the user's confirmation. Nothing in this page is implemented. Owner: Claude (coordinator).

## 1. Why

The user judged the game as not fun and not different from other bird sorting games, and reported three concrete
problems: levels that feel impossible without boosters, bird skills nobody notices, and worlds that all feel the
same. Painting a different bird cast per world (81 designs, `tools/birds/world_designs`) was tried and dropped as
the main answer: looks alone do not change how a level plays. Decision (user, 2026-11-13): worlds differ by
**what their birds do**, and new bird art is painted only for birds whose rule was proven fun in a prototype.

## 2. Measured facts (2026-11-13, main at c4982b7b)

Method: `tools/levels/measure_fragility.gd` (headless). For every level it walks the stored solution in
`levels/hints.json`; at each step it tries every other legal move and asks `Solver.hint` (6000 states, 250 ms)
whether the resulting position can still be won. It also plays 40 random games per level (uniform legal move, never
the direct reversal, at most `3 * par + 10` moves). Raw rows: [evidence/fragility.csv](evidence/fragility.csv).
Random play is not a human; it shows how easy a dead end is to reach, not a failure rate of players.

| World | Random games won | Random games ending in a position that can never be won |
| --- | --- | --- |
| 1 | 98% | 0% |
| 2 | 85% | 12% |
| 3 | 68% | 24% |
| 4 | 73% | 12% |
| 5 | 39% | 59% |
| 6 | 46% | 49% |
| 7 | 40% | 52% |
| 8 | 59% | 36% |
| 9 | 39% | 58% |
| 10 | 28% | 69% |

- Every level has a stored booster-free solution, replayed by `tests/run_core.gd`. No level needs a booster.
- 39 of the 100 levels have at least one move beside the solution that makes the level unwinnable for good;
  in 14 of them such a move exists within the first three steps. Level 41: 8 of 63 alternatives, some on step 0,
  and 40 of 40 random games ended unwinnable.
- Most unwinnable positions still have legal moves, so the player is not told. `BoardView._dead_end` flags a
  position only after the background search has exhausted it; positions over the search budget are never flagged.
- Undo is limited to `GameSession.FREE_UNDOS` (3) per level, then coins or an ad. The stuck pointer still points
  at Undo when the allowance is spent.
- Skills: the homecoming gift of a species (`Rules.GIFT_BY_SPECIES`) is explained only in the new-friend popup and
  the aviary. The in-level goal row that carried gift badges was removed
  ([goal-row-removal](../goal-row-removal)). In a level the only cue is a coin rising after a flock has gone home.
- Worlds: all ten use the same species and the same mechanics. Sleepers are in 5–10 of the 10 levels of every
  world; world 1 already contains eggs, crows, mama birds, twigs, growing branches and day/night.

## 3. Design

### 3.1 Dead ends

1. **Generation rule** (applies when levels are rebuilt, section 5): no move within the first three steps of the
   stored solution may lead to an unwinnable position; worlds 1–4 additionally keep one free seat more than the
   minimum the solution needs. `measure_fragility.gd` is the gate.
2. **Tell the player at once.** When a position is proven unwinnable the board says so with the stuck cue, whatever
   the undo allowance.
3. **One free rewind per level.** The stuck cue offers "back to where it can still be won": the session steps back
   through its history to the latest position the solver proves winnable. The first rewind in a level is free; later
   ones cost the undo price (coins or an ad) through the existing `UndoOffer`. Restart stays free.
4. Positions over the search budget stay unflagged; that boundary is reported, not hidden.

### 3.2 Talents: skills that act on landing

A **talent** belongs to a species and changes what that bird may do or what happens when it lands. It works on
every move, not only at homecoming. Rules common to all talents:

- The move encoding stays `[from, to]` and `[b, -1]`; a talent widens or narrows `Rules.can_transfer` or adds an
  effect in `Rules._resolve`. The solver, hints, undo and the stored solutions keep working through the same calls.
- Deterministic, no randomness, fully undone by Undo.
- A level lists the talents it uses; a species without a listed talent is an ordinary bird. Existing levels list
  none, so their solutions stay valid.
- The existing homecoming gifts are unchanged during the pilot. Recommendation: the game ends up with one skill
  system, so a species that receives a talent loses its homecoming gift; confirmed after the pilot is played.
- A talent must be used often enough to be felt: a level built around a talent needs at least three talent moves
  on its stored solution, and its world may field two flocks of the star bird.

### 3.3 Making a talent visible

1. **Preview on selection** (the main cue): when the player selects a bird with a talent, everything the talent
   would touch is lit before the player commits: extra legal branches glow, birds that would wake or be passed
   pulse. No words.
2. **Talent badge**: a small painted badge floats over a selected talent bird and sits on its face in the level
   card's mechanic row.
3. **Teaching level**: the first level of a talent has only that talent, a hand shows one talent move, and the
   level cannot be won without using it.
4. **Long press** on any bird opens its card (`Popups.goal_info`), which already shows the ability line.

### 3.4 What makes a world its own

Each world owns four things; a mechanic is introduced in exactly one world and later worlds use it only as support.

1. One **star bird** with a talent.
2. One **existing mechanic** as its setting (eggs, crows, mama birds, twigs, swivel branches, growing branches,
   day and night). The seven mechanics already in the game are spread over the worlds, one each, instead of all
   appearing in world 1. No new environment rules are invented until the talents are proven.
3. About seven of its ten levels are built around its own star bird and rule.
4. Level 10 of the world is a larger **finale** level; winning it moves the star bird into the aviary.

World 1 stays the plain game (sorting, then sleepers) so the first minutes are unchanged.

### 3.5 Pilot talent: Dive (kingfisher), fully specified

- **Rule.** A selected run of Dive birds may also land on a branch that has a free seat and whose tip is exactly
  one bird of another species sitting directly on a Dive bird of the mover's species. The run slips in **under**
  that tip bird and joins its own kind; the tip bird stays the tip. Count = min(run, free seats), as for any move.
- The tip bird may be awake or asleep, a crow, a mama or a chick. It may not be an egg (nothing lands by an egg).
- Ordinary landings (empty branch, same species at the tip) are unchanged. Light step is unchanged.
- No homecoming follows directly from a dive, because the branch tip is another species; `Rules.can_home` already
  requires a full branch of one awake species. The same-species landing wake applies to a sleeping Dive bird under
  the tip, as for any same-species landing.
- A dive counts as one move, ticks day/night, and resets the Chorus chain like any move that homes nothing.
- Preview: on selecting a Dive run, branches reachable only by diving glow in the talent colour and the bird that
  would be passed bobs once.
- Pilot stand-in: the pilot uses the existing `sky` bird with a talent badge. No new bird art in the pilot.

Why this one first: it needs no new input, it is visible on the move itself, and it attacks the commonest
frustration of the genre (one wrong bird sitting on a nearly complete flock).

### 3.6 Candidates for later worlds (one line each; rules are fixed only after their own prototype)

| World | Star bird | Talent candidate | Existing mechanic as the setting |
| --- | --- | --- | --- |
| 1 Green | the round flock | none | sleepers |
| 2 Bamboo | kingfisher | Dive (3.5) | eggs |
| 3 Autumn | woodpecker | Knock: on landing wakes every sleeper on that branch | twigs |
| 4 Snow | long-tailed tit | Huddle: lands on a full branch of its own kind as a fifth bird | growing branches |
| 5 Blossom | parrot | Mimic: may land on any tip; keeps its own species for homecoming | mama birds |
| 6 Mushroom | cuckoo | Lays its egg on the branch it leaves | crows |
| 7 Lakeside | pelican | Carries the bird behind it along | swivel branches |
| 8 Pine | magpie | Swap: trades places with the tip bird of the target | to be decided |
| 9 Rainforest | owl | nocturnal (existing) | day and night |
| 10 Wisteria | macaw | Call: all exposed birds of its kind fly to its branch | mixes earlier ones |

Only worlds 2–4 are planned after the pilot; worlds 5–10 are a direction and are specified when reached.

The species list in `tools/birds/world_designs/birds.tsv` stays as a reference for which real birds fit each
world; its 81 paintings are not runtime art.

## 4. Work in two steps

### 4.1 Step one: dead ends on the real levels

Small, certain and useful today, so it comes first and is not mixed into the talent trial.

- Items 2 and 3 of 3.1 on all 100 levels: the notice as soon as a position is proven unwinnable, and the free
  rewind (once per level, then the undo price). When the solver cannot prove a winnable earlier position within its
  budget, the offer is a free restart instead.
- The 14 levels with a losing move in the first three steps (list in the evidence table, `first_trap_step` 0–2)
  are reported to the user with two options: regenerate just those levels now, or leave them until the worlds are
  rebuilt. Changing levels needs the user's word.

### 4.2 Step two: Dive pilot

Three lab levels, played by the user, existing art only.

| Lab level | Intent | Shape |
| --- | --- | --- |
| A "First dive" | teach: one dive wins | 3 species, 4 branches, target 4–6 moves, impossible without a dive |
| B "Which first" | a choice: several dives are possible, the order matters | 4 species, 6 branches, target 10–14 moves, at least 3 dives |
| C "Deep water" | combine with sleepers and one egg | 5 species, 7 branches, target 16–22 moves, at least 3 dives |

- Lab levels are stored in `PerchSort/levels/lab.json`, opened only by a launcher argument (`--lab=<id>`), and are
  not part of the 100 levels, the map or the save.
- Each lab level is accepted by tools: solvable without boosters; unsolvable when the talent is switched off (A)
  or at least 30% longer (B, C); passes the generation rule of 3.1.
- Included: `Rules.can_transfer`/`transfer` for Dive, solver support through the same calls, selection preview and
  badge, the hand in level A.
- Excluded: new bird art, changes to the 100 levels or their order, map/aviary/shop changes, other talents,
  retuning gifts.
- Known limit: the stand-in bird with a badge will look plainer than a painted kingfisher; the pilot judges the
  rule, not the look.

## 5. After the pilot (not authorised by this spec)

1. User plays A–C and judges: was there a moment that needed thought, and was the talent understood without words.
2. If yes: specify world 2 fully (ten levels), paint the kingfisher with the shared-rig approach, rebuild world 2,
   and apply the generation rule of 3.1 to all rebuilt levels. If no: try the next candidate with the same three
   level shapes.
3. Rebuilding levels changes `levels.json`, `hints.json` and possibly the save mapping in `meta/profile.gd`; that
   needs its own spec and the user's authority.

## 6. Acceptance of the pilot

- `bash scripts/run_tests.sh` passes; the 100 levels replay unchanged.
- A core test covers Dive: legal and illegal targets, egg tip refused, undo restores the position, the solver
  solves A–C and proves A unsolvable without the talent.
- Timed frame sequence of one dive (selection preview, flight, landing under the tip bird).
- Step one has its own evidence: a timed frame sequence of the dead-end notice and the free rewind on a real level,
  and a core test of the rewind.
- The user plays the three lab levels on the Mac.

## 7. Open points for the user

1. Is Dive acceptable as the first talent?
2. Is one free rewind per level, then coins or an ad, the right price?
3. Are the world themes in 3.6 acceptable as a direction (only worlds 2–4 are planned now)?
4. The 14 levels with an early losing move: regenerate them now, or wait for the world rebuild?
