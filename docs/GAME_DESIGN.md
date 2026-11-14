# Perch Sort — gameplay design record

This record preserves gameplay concepts. [CURRENT_GAME](CURRENT_GAME.md), canonical source and current accepted screen contracts govern implementation. It is not a separate version or authority to restore retired layouts. Chinese overview: [GAMEPLAY_CN](GAMEPLAY_CN.md).

Originality: theme, bird characters, names, mechanics marked "ours", palette, layout numbers and text are ours. Do not copy any existing game's characters, logos, text, art, level layouts or exact numbers.

## 0. Pitch and pillars

**Pitch**: a cozy forest where lost birds find their way home. Sort birds on branches into flocks; every flock that flies home makes the tree grow, wakes sleepy neighbours and fills the forest with song.

1. **Calm but clever**: no timers, no lives, no fail screen. Puzzles get deep through planning, not pressure. Every level is solvable without boosters.
2. **Birds are characters, not colors**: each species has a name, personality and its own idle quirk; special birds (eggs, sleepers, crows) are the level mechanics.
3. **Home is the reward**: flocks flying into their birdhouse is the main payoff moment; chaining homecomings ("Chorus") is the skill expression.
4. **Every level looks different**: ten forest worlds, each with time-of-day and weather variants, its own branch, bush and birdhouse art.
5. **Fair economy**: coins come from play only (no ads or live real-money purchase integration). Shop sells cosmetics and convenience, never required progress.

## 0.1 What is genre convention vs. ours

| Area | Genre convention (allowed, generic) | Our twist (original) |
| --- | --- | --- |
| Core move | tap end run, move to empty or same-color end, capacity, full group leaves | Groups leave into a **visible birdhouse per species**; the tree reacts (branch growth, wake-ups) |
| Hidden items | mystery egg revealed when uncovered | Eggs **wobble** when a bird of their species lands on the same branch (a fair, readable clue, not a full reveal) |
| Blockers | locked/covered containers | **Sleepy birds** woken by song, **Crows** that never go home and must be parked |
| Space | add an extra container | **Growing branches**: the tree grows a seat each time a flock goes home; short **twigs** that store but never complete |
| Combo | none / score multipliers | **Chorus**: consecutive homecomings show x2/x3 and pay bonus coins |
| Boosters | undo, hint, add tube, shuffle | Themed tools, each one the antidote to one world mechanic (Owl Eye, Morning Bell, Scarecrow, Spring Rain) |
| Completion | a full group simply disappears; order of completion rarely matters | **Homecoming gifts**: every species leaves a different gift when its flock gets home, so *which flock you finish first* is the main strategic decision (§6.5) |
| Moving | only the contiguous end run moves | **Mama bird**: moving a mama also pulls her chicks out from anywhere on that branch, leapfrogging other birds (§6.6) |
| Containers | containers are passive | **Swivel branch**: tap the knot to flip a branch end-for-end (§6.7) |
| Meta | coins, skins, backgrounds | **Aviary** collection with personalities and outfits; birdhouse styles; world map of forests |

## 1. Core rules

- **Board**: branches grow from the left and right screen edges, alternating rows. Default capacity `N` = 4 seats. Birds sit side by side; the bird nearest the branch **tip** (toward screen center) is the exposed one.
- **Species**: K species per level, exactly N birds of each (eggs included).
- **Tap a branch** with a movable tip bird → select it: the run of same-species awake birds at the tip hops up (selected). Tap the same branch or empty space → cancel.
- **Tap a target branch** → the run flies over if the target has a free seat and is empty or its tip bird is the same species (an awake bird or a sleeping bird of that species; see §6.2). Count = min(run, free seats). Illegal target → the selected birds shake their heads, selection cancels, nothing else changes.
- **Flock home**: when a branch holds N awake birds of one species (N = the level's flock size, not the branch's current seat count), they cheer, then fly one by one into **their species' birdhouse** at the top. The branch stays and becomes an ordinary empty branch.
- **After every move**, resolve in this fixed order: (1) hatch exposed eggs on the source branch, (2) flock home checks in branch index order, (3) effects of each homecoming in branch index order: the species' homecoming gift (§6.5), then wake-ups (§6.2) and branch growth (§6.4), (4) **cascade**: if steps 1–3 changed anything (a hatch, wake, growth or twig upgrade), repeat steps 1–3 over all branches until nothing changes (homecomings from a cascade belong to the same move, so one move can send several flocks home), (5) Chorus update §7, (6) win check.
- **Win**: every species is home. **No lose state**: if no legal move exists, show a gentle "stuck" bubble pointing at Undo and the boosters.
- **Moves**: each successful transfer = 1 move; a swivel (§6.7) = 1 move. Undo (with the allowance defined by `GameSession.FREE_UNDOS`) restores the full previous state including move count, Chorus and booster effects of that move (consumed boosters are refunded on undo of the booster action itself).
- **Stars**: moves ≤ par → 3 stars; ≤ par+3 → 2; otherwise 1. Using boosters does not reduce stars (calm pillar). Best stars per level are saved.
- **Coins**: §8.
- **Restart** and **sound mute** live in the settings panel.

## 1.1 Rule clarifications

- 1. Gifts and Chorus are always active engine-wide in every level, including L1 ("from level 3" is a level-design statement). In L1 sunny/sky gifts simply have no target. Booster inventory is a session input (default empty) and output; L1 behaviour/tests unchanged.
- 3. Each wave: step 2 scans branches in index order, removes ALL currently complete flocks, then step 3 applies their gifts in the same order, then wake/growth effects. Same-species landing wakes are applied in step 3 of the transfer's first wave even when no flock goes home. Every wave rechecks exposed eggs on all branches. Initial exposed eggs / complete flocks are forbidden by the level validator; Core does not auto-resolve on load (they resolve after the first action).
- 4. Mama pulls only awake chicks; sleeping chicks stay.
- 5. Chorus tracks consecutive homecoming flocks and contributes the existing win bonus. There is no Song meter or free-booster payout; see §7.
- 6. Boosters cost no move and do not reset the chain. Homecomings caused by a booster (e.g. Bell wakes a sleeper that completes a flock) resolve with the full cascade and continue the current chain (add chain steps). A booster with no effect is rejected: not consumed, no history entry (UI will disable it). Firefly consumes only when it returns a move; Unknown/no move -> not consumed. Sapling once per level.
- EggWobbled: yes, add it.
- Restart refunds boosters used in that attempt; purchased boosters are never undone or refunded; the next level starts with the inventory of the winning attempt.
- (b) Per homecoming, in branch index order: [that flock's gift, then its adjacent-row song wake, then its growth +1]. Same-species landing wakes of the transfer are applied first in step 3 of the first wave, before any homecoming effects.
- Default variant noon is correct.

## 2. Introductory puzzle design

Purpose: teach tap-select, tap-move and the flock-home payoff in under 20 seconds.

| Branch | Side, row | Birds, root → tip |
| --- | --- | --- |
| B0 | left, row 0 | sunny, sunny, sunny, sky |
| B1 | right, row 0 | sky, sky, sky |
| B2 | left, row 1 | sunny |
| B3 | right, row 1 | (empty) |

N=4, K=2 (`sunny`, `sky`), par=2. Intended line: B0→B1 (sky flock home), then B2→B0 (sunny flock home). Tutorial hand: pulse on B0, then on B1; after the first homecoming, pulse B2 then B0. Tutorial shows only on the first play of level 1 and only hands, no text.

Level data lives in `PerchSort/levels/levels.json`:

```json
{ "levels": [ { "id": 1, "capacity": 4, "par": 2,
  "branches": [
    { "side": "L", "row": 0, "birds": ["sunny","sunny","sunny","sky"] },
    { "side": "R", "row": 0, "birds": ["sky","sky","sky"] },
    { "side": "L", "row": 1, "birds": ["sunny"] },
    { "side": "R", "row": 1, "birds": [] } ] } ] }
```

## 3. Screen layout

Use the current screen-specific [design contracts](design/) and canonical `PerchSort/game` views. Retired cream-glass HUD coordinates, four-tool layouts and disabled-state scaling are not current constraints.

## 4. Visual style and birds

Use the accepted bright forest cartoon assets and current painted lettering. Existing bird identities and animations remain intact. Presentation details belong to `docs/design/`, not this gameplay record.

### 4.1 Species (the Aviary)

Each species is an original character, distinct by silhouette AND color (must read in grayscale at 64 px), with a personality expressed by one **idle quirk** animation (§5) and a voice (short chirp SFX). Frames per species: idle, blink, quirk (2 frames), fly_up, fly_down, cheer, sleep (Zzz pose).

| id | Name | Look | Personality / idle quirk | First level |
| --- | --- | --- | --- | --- |
| sunny | Sunny | lemon-yellow round chick, one curly head tuft, rosy cheeks | bubbly; quirk: a little double hop | 1 |
| sky | Skye | sky-blue egg-shaped bird, white belly, round top-knot bun | dreamy; quirk: slow sleepy blink and head tilt | 1 |
| berry | Berry | raspberry-magenta, two long swept-back head plumes | show-off; quirk: preens plumes with a wink | 3 |
| mint | Minty | mint-teal, leaf-shaped crest, round eye rings like glasses | bookish; quirk: pushes "glasses" up with a wing | 6 |
| cocoa | Cocoa | chocolate-brown, cream face mask, short ear tufts | grumpy-cute; quirk: big yawn | 9 |
| snow | Flurry | white, lilac wing tips, crown of three lilac dots | shy; quirk: shivers and fluffs up | 14 |
| ember | Ember | warm orange, flame-shaped three-feather crest | energetic; quirk: flaps wings in place | 19 |
| ink | Inky | deep navy with a white star spot on the chest | calm; quirk: looks up at the sky, star spot twinkles | 26 |
| hoot | Hoot | round pale-tawny owl chick, heart-shaped white facial disc, very large amber eyes, tiny star-patterned nightcap, NO ear tufts | quirk: rotates head left-right | 33 |
| crow | Rook (special) | slate-grey crow with a crooked smile and a shiny trinket | trickster; quirk: tosses its trinket | 21 |

### 4.2 Special bird looks

- **Mystery egg**: speckled cream egg in a small twig nest, with a soft "?" pattern in the speckles; wobble frames (3).
- **Sleeping bird**: the species' sleep pose with closed eyes and a drifting "Zzz" bubble; it keeps its species colors (sleep is a state, not a hidden identity).
- **Crow**: see table; crows never wear outfits.

### 4.3 Outfits (cosmetic)

Each species has 3 outfits (hat, scarf, glasses-or-flower), drawn as separate overlay sprites anchored to a per-species head/neck point so one overlay set works on every frame. Outfits never change silhouettes enough to hurt species recognition.

## 5. Animation and feel

Motion uses Godot Tweens plus real frame swaps; no fixed image stands in for animation. Game state is committed first; animation only plays committed events. Input is locked only while birds are in the air; a tap during landing is queued for at most one action.

**Birds**
- Idle: breathing scale 1→1.03 (1.6 s loop, random phase); blink every 2–5 s (0.12 s); species quirk every 6–12 s (random bird, never two neighbours at once).
- Attention: when a run is selected, the other birds on the board turn their heads toward it (flip-x look, 0.15 s); they look back on cancel.
- Select: run hops up 40 px (0.15 s OutBack), stays raised with a tiny hover bob.
- Take-off: anticipation crouch (scale 1.08×0.92, 0.06 s) then launch; each bird departs 0.06 s after the previous.
- Flight: bezier arc 0.45 s, peak 160 px above the midpoint, wing frames alternate every 0.07 s, body tilts toward travel direction (±12°).
- Landing: feet-first, squash 1.15×0.85 → 1 (0.12 s); the **branch flexes** under the landing (rotate tip −3° and back, 0.25 s OutElastic) and a couple of leaves fall.
- Illegal move: head shake ±10° twice (0.25 s) with a short "huh" chirp.
- Flock home: the flock cheers (cheer frame + jump, 0.2 s), the matching birdhouse door swings open, birds fly in one by one (0.1 s apart) shrinking to 0.4, the house punches scale 1.1 per arrival, door closes, music-note particles rise from the chimney.
- Egg: wobble when a same-species bird lands on its branch; hatch = crack frame → shell pops → bird pops up with a squash (0.35 s).
- Sleep/wake: Zzz bubble drifts; on wake the bird startles (jump 20 px, eyes open), stretches wings.
- Crow: hops instead of flying straight (two short hops), cackles.
- Branch growth: the branch tip extends with a leaf sprout and a new seat glow (0.4 s OutBack).

**HUD and screens**
- Buttons: press scale 0.92, release OutBack; disabled = 45% alpha, 0.9 scale.
- Chorus: at x2/x3 a ribbon text "x2" pops near the goal tray.
- Win: birds still visible dance (cheer frames), panel scales in from 0.6 (0.35 s OutBack), stars pop one by one 0.2 s apart with a chime, coins fly to the coin pill and the number counts up.
- Scene ambience (per world, §9): clouds drift, canopy sway, petals/leaves/snow particles, fireflies at dusk.
- Screen transitions: leaf wipe (0.4 s) between map and level.

## 6. Special birds and branches (level mechanics, introduced one per world)

Each mechanic is taught by one tutorial level with hands only (no text) and then mixed in. Exact rules:

### 6.1 Mystery egg (World 1, from level 6)
- An egg occupies a seat and has a hidden species fixed in level data (not random; Restart keeps it).
- Eggs cannot be selected and are never part of a run. A branch whose tip is an egg accepts no birds.
- When the egg becomes the tip (the birds in front of it moved away), it hatches into its species at the end of that move (rule order step 1).
- Clue (ours): when a bird of the egg's species lands on the egg's branch, the egg wobbles once. It never reveals the species directly.

### 6.2 Sleepy bird (World 2, from level 11)
- A sleeping bird keeps its species, cannot be selected and is never part of a run; birds behind it are blocked.
- A sleeping tip bird still **accepts** landing birds of its own species (it is a valid same-species tip).
- It wakes when (a) a bird of its own species lands directly next to it, or (b) a flock flies home from a branch on the same row or an adjacent row (the song wakes it). Wake-ups are resolved in rule order step 3.
- Sleeping birds never complete a flock until awake: a branch with N same-species birds where one is asleep completes the moment it wakes.

### 6.3 Crow "Rook" (World 3, from level 21)
- Crows are birds of the special species `crow`. They move like normal birds (runs of crows move together, land on empty or crow tips).
- Crows never go home and never complete a flock. A branch containing a crow cannot complete.
- Levels with crows include at least one **twig** (§6.4) so crows can be parked. Win needs all real species home; crows may remain.

### 6.4 Twigs and growing branches (twigs World 3, growth World 4 from level 31)
- **Twig**: a short branch with capacity 2 (thinner art). It accepts and releases birds normally but can never complete a flock.
- **Growing branch**: a branch whose current capacity starts below N (2 or 3). Each time any flock flies home, every growing branch gains +1 seat up to N (rule order step 3). Birds never fall off; seats only grow. A growing branch that has reached N behaves like a normal branch.

### 6.5 Homecoming gifts (ours, core identity, from level 3)

Every species leaves one fixed gift the moment its flock gets home. The gift icon is painted on that species' birdhouse sign, so the player can always see what finishing each flock will do. Gifts are deterministic and the solver models them. Planning which flock to complete first is the heart of the game from World 1 on.

| Species | Gift | Exact effect |
| --- | --- | --- |
| sunny | Wake-up song | every sleeping bird on the board wakes |
| sky | Gentle breeze | every egg on the board turns see-through: its species is shown (it still hatches only when exposed) |
| berry | Move refund | at once: the move that sent the flock home is taken off the counted moves (stars only); the badge flies to the move counter |
| mint | Gardener | every growing branch +1 seat (on top of the normal +1), and the lowest-index twig becomes a full branch of capacity N (its birds stay) |
| cocoa | Grumpy stare | every crow on the same row or an adjacent row of the branch the flock left flies away |
| snow | Spare undo | at once: one more undo for this attempt; the badge flies to the undo slot. Paid once per flock per level: going home again after an undo, rescue or restart pays nothing |
| ember | Warm glow | every egg on the board hatches immediately (even if covered; the hatched bird keeps its seat) |
| ink | Move refund | as berry |
| hoot | Wake-up song | every sleeping bird on the board wakes |

Level design rule: from level 3 on every level must have at least one meaningful ordering decision (the solver must find that a different completion order changes the minimum moves or solvability).

### 6.6 Mama bird and chicks (World 2, from level 15)

- A **mama** is a normal bird of her species wearing a little bow; **chicks** are smaller birds of the same species (they count as full birds for flocks and seats).
- When a moving run contains a mama, after the run lands, the chicks of her species on the **source branch** follow her one by one, from the tip side to the root side, even if other birds sit between them, as long as the target has free seats.
- Chicks without their mama move like normal birds. A mama never pulls chicks from other branches.
- Example: source `[A-chick, B, B, A-mama]` → empty target: mama flies, then the chick leapfrogs out: source `[B, B]`, target `[A-mama, A-chick]`. One move.

### 6.7 Swivel branch (World 3, from level 24)

- A swivel branch has a wooden knot at its root. Tapping the knot (not the birds) flips the order of its birds end-for-end: the old root bird becomes the tip. It costs 1 move and is undoable.
- Eggs that become the tip hatch (rule order step 1 applies to the swivelled branch). A swivel never completes a flock by itself unless its N birds are already all the same species.
- The knot is a separate 120×120 hit area at the screen edge; tapping the birds still selects them normally.

### 6.8 Current level timing

The day/night switch was removed in [PR #224](https://github.com/crazyniu/game/pull/224). Every current level has `dayLength: 0`; Inky and Hoot have no time-of-day movement restriction. Authored sleeping birds still use the sleep mechanic in §6.2. Inky now grants Move refund and Hoot grants Wake-up song, as defined by `Rules.GIFT_BY_SPECIES`.

Retained phase fields and engine branches are compatibility/cleanup concerns, not authority to reintroduce the removed feature. See the [removal report](work-log/no-day-night/report.md) for changed puzzles, hints, reported checks and remaining limits.

## 7. Chorus

- A **homecoming move** is a move that sends at least one flock home. Chorus chain = number of consecutive flocks sent home across consecutive homecoming moves; a transfer or swivel that homes nothing resets it to 0. Two flocks home in the same move count as +2.
- Feedback: when the chain reaches x2 / x3 / x4+ a ribbon ("x2", "x3") pops near the goal tray.
- Coin bonus: +5 coins per chain step beyond the first, paid on win.
- Undo restores the chain to the pre-move state.

## 8. Boosters, coins and shop

### 8.1 Boosters (skills)

| Booster | Effect | Unlocks | Price (coins) | Starter gift |
| --- | --- | --- | --- | --- |
| Undo | revert one move (or one booster) | level 1 | allowance from `GameSession.FREE_UNDOS`; extra-use price from `Progress.UNDO_PRICE` | — |
| Firefly (hint) | a firefly flies from the best source to the best target (solver path) | level 2 | 30 | 3 |
| Sapling | adds one empty branch of capacity N (max 1 per level) | level 4 | 60 | 2 |
| Owl Eye | all mystery eggs on the board show their species as a translucent bird inside (permanent for the level) | level 7 | 80 | 2 |
| Morning Bell | wakes every sleeping bird | level 12 | 80 | 2 |
| Scarecrow | every crow on the board flies away for good | level 22 | 100 | 2 |
| Spring Rain | every growing branch grows to full capacity N | level 32 | 100 | 2 |

Rules: boosters are used from the bottom bar; using one is an undoable action. The current toolbar composition and quantities belong to `game/board_view.gd`, not the retired four-slot layout. Unlocks come from `Progress.booster_unlock_level`; undo limits and refunds come from `GameSession`.

### 8.2 Coins

Current rewards, prices, draw limits and daily rewards are defined by `PerchSort/meta/progress.gd`; the older fixed payout table is removed to avoid contradicting those production constants. Keep persisted `coins` fields unchanged even where the UI uses feather artwork. The coin shop is a preview flow, not a live payment integration.

### 8.3 Shop (what it is for)

The shop gives coins a purpose and gives long-term goals. Three tabs:
1. **Boosters**: buy single boosters at the prices above, or a bundle of 3 at 2.5× price.
2. **Outfits**: per-species cosmetic outfits (§4.3), with prices from `Shop.OUTFIT_PRICES`; equipped outfits appear on that species everywhere (board, birdhouse arrivals, Aviary). Purely cosmetic.
3. **Birdhouses**: alternative birdhouse styles (mushroom house, teapot, lantern, acorn), 600 coins each; cosmetic, applied to all birdhouses.
Nothing in the shop is required to finish any level.

### 8.4 Aviary (collection)

A page listing every species card: portrait, name, personality line (short, icon-first), the levels-home count, owned outfits with equip buttons. A species card unlocks with a short "new friend" pop-up (portrait + quirk animation) the first time it appears in a level.

## 9. Worlds and progression

The canonical catalog has 100 fixed levels in ten sequential forest worlds (`Levels.COUNT`, `WORLDS`, `PER_WORLD`). World names come from `Progress.WORLD_NAMES`; scene variants and puzzle contents come from `PerchSort/levels/levels.json`.

The [difficulty continuity contract](work-log/difficulty-continuity/spec.md) owns the reordered progression and save migration. Old four-world/40-level ranges, fixed debut levels and former task-numbered authoring gates are not current constraints. Read actual mechanic introductions from the current catalog and production helpers.

## 10. Screens and navigation

Home, world/level selection, gameplay, Shop, Aviary, Settings and reward/purchase overlays are separate canonical views coordinated by `PerchSort/game/main.gd`. Use [CODE_MAP](spec/CODE_MAP.md) and the screen contracts in [design](design/); do not restore the retired map-as-home S-curve layout or four-button HUD.

## 11. Level authoring

Preserve the current catalog, hint routes and migrations unless gameplay/data changes are explicitly authorized. Validate through `scripts/run_tests.sh` and report the actual limits. A passing automated replay does not establish human playability or visual acceptance.

## 12. Data and validation

`levels.json` level fields: `id, world, variant, capacity (N), par, dayLength (0 in every current level)`; branch fields `side, row, kind (normal|twig|growing|swivel), seats (current capacity; growing only)`; bird tokens `"species"`, `"species:sleep"`, `"species:mama"`, `"species:chick"`, `"egg:species"`, `"crow"`. The current level data and `Levels.create_board` define accepted fields and defaults; the introductory example above is not a requirement to overwrite the catalog. Validator rules: species counts exactly N each (eggs included), no exposed egg and no complete flock at start, crows only with at least one twig.
