# Perch Sort v5 — complete game design (owner: Claude)

2026-11-03 (v5.3). Chinese summary for the user: [GAMEPLAY_V5_CN.md](GAMEPLAY_V5_CN.md) (the English page wins on any conflict). The user rejected the v2 runtime and the v4 document set as not fun and visually weak, and assigned all gameplay, UI, bird and animation decisions to Claude ("你来定玩法和 UI", "小鸟这块怎么创新、玩法怎么创新，都由你来定"), with same-genre screenshots as **style direction only**, and asked for innovation rather than copying the genre. This page is the single authority for the v5 runtime and all future v5 tasks. It supersedes `docs/spec/RULES.md` (v4), `docs/UI_SPEC.md`, `docs/MAP_PROGRESSION.md` and the v4 evidence process **for v5**; v2 code and the old docs stay untouched as history.

Originality: theme, bird characters, names, mechanics marked "ours", palette, layout numbers and text are ours. Do not copy any existing game's characters, logos, text, art, level layouts or exact numbers.

## 0. Pitch and pillars

**Pitch**: a cozy forest where lost birds find their way home. Sort birds on branches into flocks; every flock that flies home makes the tree grow, wakes sleepy neighbours and fills the forest with song.

1. **Calm but clever**: no timers, no lives, no fail screen. Puzzles get deep through planning, not pressure. Every level is solvable without boosters.
2. **Birds are characters, not colors**: each species has a name, personality and its own idle quirk; special birds (eggs, sleepers, crows) are the level mechanics.
3. **Home is the reward**: flocks flying into their birdhouse is the main payoff moment; chaining homecomings ("Chorus") is the skill expression.
4. **Every level looks different**: four forest worlds, each with time-of-day and weather variants, its own branch, bush and birdhouse art.
5. **Fair economy**: coins come from play only (no ads, no real money in v5). Shop sells cosmetics and convenience, never required progress.

## 0.1 What is genre convention vs. ours

| Area | Genre convention (allowed, generic) | Our twist (original) |
| --- | --- | --- |
| Core move | tap end run, move to empty or same-color end, capacity, full group leaves | Groups leave into a **visible birdhouse per species**; the tree reacts (branch growth, wake-ups) |
| Hidden items | mystery egg revealed when uncovered | Eggs **wobble** when a bird of their species lands on the same branch (a fair, readable clue, not a full reveal) |
| Blockers | locked/covered containers | **Sleepy birds** woken by song, **Crows** that never go home and must be parked |
| Space | add an extra container | **Growing branches**: the tree grows a seat each time a flock goes home; short **twigs** that store but never complete |
| Combo | none / score multipliers | **Chorus**: consecutive homecomings fill a Song meter that pays out a free booster |
| Boosters | undo, hint, add tube, shuffle | Themed tools, each one the antidote to one world mechanic (Owl Eye, Morning Bell, Scarecrow, Spring Rain) |
| Completion | a full group simply disappears; order of completion rarely matters | **Homecoming gifts**: every species leaves a different gift when its flock gets home, so *which flock you finish first* is the main strategic decision (§6.5) |
| Moving | only the contiguous end run moves | **Mama bird**: moving a mama also pulls her chicks out from anywhere on that branch, leapfrogging other birds (§6.6) |
| Containers | containers are passive | **Swivel branch**: tap the knot to flip a branch end-for-end (§6.7) |
| Time | timers or none | **Day and night**: the sky turns every few moves; night owls only move at night (§6.8) |
| Meta | coins, skins, backgrounds | **Aviary** collection with personalities and outfits; birdhouse styles; world map of forests |

## 1. Core rules

- **Board**: branches grow from the left and right screen edges, alternating rows. Default capacity `N` = 4 seats. Birds sit side by side; the bird nearest the branch **tip** (toward screen center) is the exposed one.
- **Species**: K species per level, exactly N birds of each (eggs included).
- **Tap a branch** with a movable tip bird → select it: the run of same-species awake birds at the tip hops up (selected). Tap the same branch or empty space → cancel.
- **Tap a target branch** → the run flies over if the target has a free seat and is empty or its tip bird is the same species (an awake bird or a sleeping bird of that species; see §6.2). Count = min(run, free seats). Illegal target → the selected birds shake their heads, selection cancels, nothing else changes.
- **Flock home**: when a branch holds N awake birds of one species (N = the level's flock size, not the branch's current seat count), they cheer, then fly one by one into **their species' birdhouse** at the top. The branch stays and becomes an ordinary empty branch.
- **After every move**, resolve in this fixed order: (1) hatch exposed eggs on the source branch, (2) flock home checks in branch index order, (3) effects of each homecoming in branch index order: the species' homecoming gift (§6.5), then wake-ups (§6.2) and branch growth (§6.4), (4) **cascade**: if steps 1–3 changed anything (a hatch, wake, growth or twig upgrade), repeat steps 1–3 over all branches until nothing changes (homecomings from a cascade belong to the same move, so one move can send several flocks home), (5) Chorus update §7, (6) win check. Light step (snow gift) is granted in step 3 and only applies to the next action.
- **Win**: every species is home. **No lose state**: if no legal move exists, show a gentle "stuck" bubble pointing at Undo and the boosters.
- **Moves**: each successful transfer = 1 move; a swivel (§6.7) = 1 move. Day/night counts moves (§6.8). Undo (free, unlimited) restores the full previous state including move count, Chorus and booster effects of that move (consumed boosters are refunded on undo of the booster action itself).
- **Stars**: moves ≤ par → 3 stars; ≤ par+3 → 2; otherwise 1. Using boosters does not reduce stars (calm pillar). Best stars per level are saved.
- **Coins**: §8.
- **Restart** and **sound mute** live in the settings panel.

## 1.1 Rule clarifications (binding, from T32 review)

- 0. Yes, the T32 schema supersedes the old §12.
- 1. Gifts and Chorus are always active engine-wide in every level, including L1 ("from level 3" is a level-design statement). In L1 sunny/sky gifts simply have no target. Keep the existing free AddBranch() as a compatibility wrapper: it performs the Sapling effect WITHOUT consuming inventory (the L1 HUD keeps using it until T34). UseBooster(Sapling) consumes inventory. Both share the one-per-level flag. Booster inventory is a session input (default empty) and output; L1 behaviour/tests unchanged.
- 2. Initial phase = day. dayLength=0 means no day/night at all: nocturnal species behave as normal birds. With dayLength>0: a counter counts successful transfers and swivels; after the move has fully resolved (including the cascade) and the counter reaches dayLength, the phase toggles, the counter resets to 0, then run the cascade again (newly awake nocturnals may complete flocks; those homecomings belong to the same move). Nocturnal sleep is derived, not a flag: a nocturnal bird is asleep iff (phase==day && moonbeamMovesLeft==0) || it has the authored :sleep flag. Authored :sleep persists at night until woken. Wakes (sunny gift, Morning Bell, same-species landing, adjacent-row song) clear only the authored flag; they cannot make a nocturnal awake during day. Ink gift: toggles phase immediately and resets the counter to 0 (inside step 3; cascade continues). Hoot moonbeam: sets moonbeamMovesLeft=3, counting the next 3 moves, excluding the granting move; decremented after each later move fully resolves; it only overrides nocturnal day-sleep, the displayed phase is unchanged.
- 3. Each wave: step 2 scans branches in index order, removes ALL currently complete flocks, then step 3 applies their gifts in the same order, then wake/growth effects. Same-species landing wakes are applied in step 3 of the transfer's first wave even when no flock goes home. Every wave rechecks exposed eggs on all branches. Initial exposed eggs / complete flocks are forbidden by the level validator; Core does not auto-resolve on load (they resolve after the first action).
- 4. Light step: consumed only by the next successful transfer (even if that target was legal anyway); survives swivel, boosters and illegal attempts; still requires a free seat and never lands on an egg tip. Mama pulls only awake chicks; sleeping chicks stay.
- 5. Chorus: within a homecoming move each homed flock increments chain by 1. A transfer or swivel that homes nothing resets chain to 0 after resolution. Notes per flock: 1 base + 1 bonus if its chain step > 1. Meter overflow carries; every time it reaches 5 it pays out immediately (several payouts possible) and subtracts 5. Berry encore notes are added in step 3 in gift order and can trigger payout immediately. Win coin bonus = 5 x (number of flocks whose chain step > 1), accumulated in the session. Payout is deterministic, no RNG: pool = boosters unlocked at or before the current level id (unlock levels from §8.1, excluding Undo), in fixed order Firefly, Sapling, OwlEye, MorningBell, Scarecrow, SpringRain; pick pool[payoutCount mod pool.Count], payoutCount is part of the meter state (session input/output), increments per payout; undo reverts everything.
- 6. Boosters cost no move and no phase tick and do not reset the chain. Homecomings caused by a booster (e.g. Bell wakes a sleeper that completes a flock) resolve with the full cascade and continue the current chain (add chain steps and notes). A booster with no effect is rejected: not consumed, no history entry (UI will disable it). Firefly consumes only when it returns a move; Unknown/no move -> not consumed. Sapling once per level.
- EggWobbled: yes, add it.
- (a) Empty payout pool: no payout and nothing subtracted; the meter keeps its notes (it may exceed 5 and is displayed as full). At the first note event in a level whose pool is non-empty, pay out floor(notes/5) times in the usual rotation and keep the remainder.
- (b) Per homecoming, in branch index order: [that flock's gift, then its adjacent-row song wake, then its growth +1]. Same-species landing wakes of the transfer are applied first in step 3 of the first wave, before any homecoming effects.
- (c) Approved exactly as you propose: counter increments at move start; resolve all waves; toggle only if the surviving counter reaches dayLength; an ink gift that already flipped and reset to 0 prevents a second scheduled flip in that move.
- (d) Yes: with dayLength=0 the ink and hoot gifts are no-ops that still emit the Gift event; phase stays Day, counter 0, moonbeam 0.
- Default variant noon is correct.

## 2. Level 1 ("First Flight") — implemented in T30

Purpose: teach tap-select, tap-move and the flock-home payoff in under 20 seconds.

| Branch | Side, row | Birds, root → tip |
| --- | --- | --- |
| B0 | left, row 0 | sunny, sunny, sunny, sky |
| B1 | right, row 0 | sky, sky, sky |
| B2 | left, row 1 | sunny |
| B3 | right, row 1 | (empty) |

N=4, K=2 (`sunny`, `sky`), par=2. Intended line: B0→B1 (sky flock home), then B2→B0 (sunny flock home). Tutorial hand: pulse on B0, then on B1; after the first homecoming, pulse B2 then B0. Tutorial shows only on the first play of level 1 and only hands, no text.

Level data lives in `PerchSort/Assets/Resources/V5/levels.json`:

```json
{ "levels": [ { "id": 1, "capacity": 4, "par": 2,
  "branches": [
    { "side": "L", "row": 0, "birds": ["sunny","sunny","sunny","sky"] },
    { "side": "R", "row": 0, "birds": ["sky","sky","sky"] },
    { "side": "L", "row": 1, "birds": ["sunny"] },
    { "side": "R", "row": 1, "birds": [] } ] } ] }
```

## 3. Screen layout (v5.1, portrait, design canvas 1080x1920, origin top-left, y downward)

Rules that apply to every screen: (1) **one button family**: every button is the rounded square `btn_square_wood` at one of two sizes, 150 (tools, settings, close, replay, home) or 200 (the single primary action, which alone uses `btn_square_green`); no round buttons, no purple. (2) **Disabled = the same button at 45% alpha with a 0.9 scale**, never a different color or style. (3) **Minimum 40 px clear gap between any two HUD elements**; text never overlaps art. (4) **Nothing floats**: the birdhouse hangs from a limb, branches grow from bushes at the edges. (5) Icons only on buttons; numbers only where needed.

| Element | Placement |
| --- | --- |
| Background | full screen `bg_forest_day` |
| Top limb | `limb_top` (1080x320, from the two top canopies, a thick tree limb crossing the top with a rope hook at x=540, y=235), drawn above the background, below HUD. Fills the gap between the canopies. |
| Coin pill | top-left, 260x90, left edge x=48, center y=130 (below the safe-area top inset). Coin icon 64 + number, 24 px padding, number left-aligned after the icon. |
| Settings | top-right, wood square 150x150, right edge x=1032 (center x=957), center y=130 |
| Birdhouse | 300x340, hanging from the limb hook: rope loop top at (540, 235), so house center y=405, bottom y=575. Gentle sway animation +-2 degrees (4 s loop) pivoting on the hook. |
| Flock tracker | one cream pill 380x96 centered at (540, 663) (40 px below the house bottom 575, so it spans y 615 to 711): [level badge] [species icon 64] [species icon 64]. Level badge is a 72x72 wood square with the level number (font 44) at the pill's left, then 28 px gap, icons 40 px apart. Species not yet home show their bird icon dimmed to 40% with a small lock-free outline; home shows full color plus a small green check at the corner. This replaces the old level pill and the loose dots. |
| Board area | vertical band y 760 to 1560 (800 px). Rows are centered in the band: row pitch = min(300, 800 / rowCount); the group of rows is centered at y=1160. Level 1 (2 rows): y=1010 and y=1310. Left branches span x 0 to 600, right branches 480 to 1080, right rows offset +40 px in y as before. |
| Bird slots | unchanged: bird 150x150, slot pitch 120, first slot 110 px from the screen edge, feet on the branch top line |
| Bottom bar | tool buttons: wood squares 150x150 at x=300/540/780, center y=1760 (gap between buttons 90). Undo, Hint, Add Branch. Under each, nothing. Bar bottom edge 1835 stays inside the safe area. |

**Win panel** (centered, scale from 0.6): `panel_win` 860x900 at (540, 1010); ribbon overlaps the top edge but its bottom must sit 40 px above the first star row; stars 3 x 150 in a row at y=870, 30 px apart; coin reward row at y=1060 (coin 64 + "+25" font 56, 20 px gap); buttons at y=1330: replay (wood 150) at x=305, **next (green 200) at x=540**, home (wood 150) at x=775, clear gaps 60. Dim overlay behind: black alpha 0.6.

**Settings panel**: `panel_win` scaled to 700x520 centered at (540, 1010); sound toggle (wood 150) at x=400 and restart (wood 150) at x=680, both y=1040; close (wood 150) at the panel top-right corner, inset 24 px. No text.

## 4. Visual style and birds

Bright, warm, plush "3D-rendered cartoon" look: soft fur texture, big glossy eyes, round chubby bodies, orange feet, gentle rim light; soft gradient skies; saturated foliage; chunky rounded UI with thick soft outlines, glossy highlights, warm wood and cream panels. Text font: Fredoka. All buttons are icons.

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
| hoot | Hoot | round tawny owl chick with big amber eyes and feather ear tufts (nocturnal) | sleepy by day; quirk: rotates head left-right | 33 |
| crow | Rook (special) | slate-grey crow with a crooked smile and a shiny trinket | trickster; quirk: tosses its trinket | 21 |

### 4.2 Special bird looks

- **Mystery egg**: speckled cream egg in a small twig nest, with a soft "?" pattern in the speckles; wobble frames (3).
- **Sleeping bird**: the species' sleep pose with closed eyes and a drifting "Zzz" bubble; it keeps its species colors (sleep is a state, not a hidden identity).
- **Crow**: see table; crows never wear outfits.

### 4.3 Outfits (cosmetic)

Each species has 3 outfits (hat, scarf, glasses-or-flower), drawn as separate overlay sprites anchored to a per-species head/neck point so one overlay set works on every frame. Outfits never change silhouettes enough to hurt species recognition.

## 5. Animation and feel (LitMotion, MIT, pinned commit 0b4c588)

All motion uses LitMotion plus real frame swaps; no fixed image stands in for animation. Game state is committed first; animation only plays committed events. Input is locked only while birds are in the air; a tap during landing is queued for at most one action.

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
- Chorus: Song meter fills with a note sliding in; at x2/x3 a ribbon text "x2" pops near the birdhouse.
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
| berry | Encore | +2 notes on the Song meter (§7) |
| mint | Gardener | every growing branch +1 seat (on top of the normal +1), and the lowest-index twig becomes a full branch of capacity N (its birds stay) |
| cocoa | Grumpy stare | every crow on the same row or an adjacent row of the branch the flock left flies away |
| snow | Light step | the next transfer may land on **any** tip with a free seat (species rule ignored once); shown as a feather on the bottom bar until used; undo restores it |
| ember | Warm glow | every egg on the board hatches immediately (even if covered; the hatched bird keeps its seat) |
| ink | Night call | day and night switch immediately (§6.8) |
| hoot | Moonbeam | the next 3 moves count as night for nocturnal birds |

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

### 6.8 Day and night (World 4, from level 33)

- The level has `dayLength` (e.g. 5). A sun/moon dial on the HUD shows moves left until the sky turns. After every `dayLength` moves, day and night swap (sky, lighting and ambient change).
- **Nocturnal species** (`ink` and the owl `hoot`) are asleep during the day (cannot be selected, block birds behind them, still accept same-species landings) and awake at night. Day species stay awake at night.
- Undo restores the phase and the counter. Hint and solver include the phase in the state.

## 7. Chorus (ours)

- A **homecoming move** is a move that sends at least one flock home.
- Chorus chain = number of consecutive homecoming moves (a non-homecoming move resets it to 0). Two flocks home in the same move count as +2.
- The **Song meter** has 5 notes. Each flock home adds 1 note; each chain step beyond the first adds 1 bonus note. When full, it pays out one random owned-world booster (§8.1) and resets. The meter persists across levels.
- Coin bonus: +5 coins per chain step beyond the first, paid on win.
- Undo restores chain and meter to the pre-move state.

## 8. Boosters, coins and shop

### 8.1 Boosters (skills)

| Booster | Effect | Unlocks | Price (coins) | Starter gift |
| --- | --- | --- | --- | --- |
| Undo | revert one move (or one booster) | level 1 | free, unlimited | — |
| Firefly (hint) | a firefly flies from the best source to the best target (solver path) | level 2 | 30 | 3 |
| Sapling | adds one empty branch of capacity N (max 1 per level) | level 4 | 60 | 2 |
| Owl Eye | all mystery eggs on the board show their species as a translucent bird inside (permanent for the level) | level 7 | 80 | 2 |
| Morning Bell | wakes every sleeping bird | level 12 | 80 | 2 |
| Scarecrow | every crow on the board flies away for good | level 22 | 100 | 2 |
| Spring Rain | every growing branch grows to full capacity N | level 32 | 100 | 2 |

Rules: boosters are used from the bottom bar; using one is an undoable action (refunded on undo). The bottom bar shows Undo, Firefly, Sapling and the current world's booster (4 buttons, see §10). Each booster button shows its owned count in a small badge; at 0 it shows a "+" badge that opens the shop on that item. Every level is validated solvable without boosters (§12).

### 8.2 Coins

| Source | Amount |
| --- | --- |
| Level win | 10 + 5 × stars (first clear); replays pay 5 |
| Chorus | +5 per chain step beyond the first (on win) |
| World complete (first time) | +200 |
| Daily nest gift (first open each day) | 50 |
| New species met (Aviary card) | 20 |

No ads and no real-money purchases in v5 (store account pending, T14). The design leaves room for a future coin pack but does not depend on it.

### 8.3 Shop (what it is for)

The shop gives coins a purpose and gives long-term goals. Three tabs:
1. **Boosters**: buy single boosters at the prices above, or a bundle of 3 at 2.5× price.
2. **Outfits**: per-species cosmetic outfits (§4.3), 300 / 500 / 800 coins each; equipped outfits appear on that species everywhere (board, birdhouse arrivals, Aviary). Purely cosmetic.
3. **Birdhouses**: alternative birdhouse styles (mushroom house, teapot, lantern, acorn), 600 coins each; cosmetic, applied to all birdhouses.
Nothing in the shop is required to finish any level.

### 8.4 Aviary (collection)

A page listing every species card: portrait, name, personality line (short, icon-first), the levels-home count, owned outfits with equip buttons. A species card unlocks with a short "new friend" pop-up (portrait + quirk animation) the first time it appears in a level.

## 9. Worlds and scenes (every level looks different)

Four forest worlds × 10 levels = 40 levels for v5 release. All worlds are forests (user requirement); worlds unlock sequentially on the world map.

| World | Levels | Scene kit (branch / bush / limb / birdhouse) | Variants (rotated per level) | Ambient | New mechanic | Booster |
| --- | --- | --- | --- | --- | --- | --- |
| W1 Spring Woods | 1–10 | oak branch, round leafy bush, oak limb, wooden house with moss roof | morning, noon, golden hour, light rain | petals, butterflies | homecoming gifts (L3), mystery egg (L6) | Owl Eye |
| W2 Bamboo Grove | 11–20 | bamboo cane perches from bamboo clumps, bamboo limb, bamboo house with leaf roof | mist morning, noon, dusk lanterns, drizzle | mist, drifting bamboo leaves | sleepy bird (L11), mama and chicks (L15) | Morning Bell |
| W3 Maple Autumn | 21–30 | maple branch with red leaves, maple bush, limb, red-roof cabin house | crisp morning, noon, sunset, windy | falling maple leaves | crow + twig (L21), swivel branch (L24) | Scarecrow |
| W4 Snow Pine | 31–40 | snowy pine branch, pine bush, limb with snow, log house with snow cap | snowy morning, noon, blue hour, aurora night (L40) | snowfall, breath puffs | growing branch (L31), day and night (L33) | Spring Rain |

Scene rule: each level JSON names `world` and `variant`; two consecutive levels never share a variant; every 10th level uses the world's special variant (light rain, dusk lanterns, sunset, aurora). Branch layout also changes per level (rows, which sides, twigs).

Each world kit (art): 4 background variants (1080×1920), branch, twig, bush, top limb, birdhouse (per species tint band on the roof edge so the player sees which house is whose), ambient particle sprites.

## 10. Screens and navigation

1. **World map** (home screen): vertical scrolling forest path with level nodes (wood discs with the number; stars under cleared nodes), world gates between worlds, the coin pill, shop and Aviary buttons (wood squares 150) bottom-left/right, settings top-right. The current level node bounces; tapping it starts the level.
2. **Level**: layout §3, with changes for v5.2: the birdhouse area shows **one birdhouse per species** in a row hanging from the limb (house width = min(300, 900 / K) with 20 px gaps; tracker icons are replaced by the houses themselves: a house glows and shows a check when its flock is home; the level badge moves to a small wood sign hanging at the left end of the limb). The bottom bar has 4 wood buttons of 150: centers x = 225 / 435 / 645 / 855, y = 1760, each booster with a count badge (64 px circle, top-right corner).
3. **Win**: §3 win panel plus Chorus bonus line.
4. **Shop**, **Aviary**: full-screen panels over a blurred map, tabs as icon wood squares.
5. **Settings**: sound toggle, music toggle, restart (in level only), close. No sliders.

Board density for bigger levels: rows = ceil(branches / 2); row pitch = min(300, 800 / rows); bird scale = clamp(pitch / 220, 0.72, 1); seat pitch scales with bird scale.

## 11. Level plan (40 levels)

Roles: T = teach (one new thing, short), P = practice, C = challenge, R = rest (easy, satisfying). Flock size N = 4 everywhere. Branches = K + spare (spare = empty or partly empty branches/twigs).

| Levels | K | Spare branches | Mechanics | Roles |
| --- | --- | --- | --- | --- |
| 1–2 | 2 | 1–2 | core | T, T (Firefly at 2) |
| 3–5 | 3 | 2 | homecoming gifts (L3 T, using berry Encore); L4 teaches Sapling | T, P, P |
| 6–10 | 3–4 | 2 | eggs (L6 T); L7 Owl Eye | T, P, C, R, C |
| 11–15 | 4–5 | 2 | sleepy (L11 T); L12 Morning Bell; eggs mixed | T, P, P, R, T |
| 16–20 | 5 | 2 | mama and chicks (L15 T), sleepy + eggs | P, C, R, C, C |
| 21–25 | 5–6 | 2 + 1 twig | crow + twig (L21 T); L22 Scarecrow; swivel (L24 T) | T, P, P, T, C |
| 26–30 | 6 | 2 + twig | swivel, crow, eggs, sleepy mixed | P, C, R, C, C |
| 31–32 | 6 | 2 growing | growing branch (L31 T); L32 Spring Rain | T, P |
| 33–40 | 6–7 | mixed | day and night (L33 T), then everything | T, P, R, C, P, C, R, C (L40 finale) |

Difficulty gates (validated by tool, §12): T levels: random-play win rate ≥ 80%; P: 40–80%; C: 5–40% with at least one tempting dead end; R: ≥ 70% and ≤ 10 moves. Neighbouring levels must differ in layout or mechanic.

## 12. Data and validation

`levels.json` level fields: `id, world, variant, capacity (N), par, dayLength (0 = no day/night)`; branch fields `side, row, kind (normal|twig|growing|swivel), seats (current capacity; growing only)`; bird tokens `"species"`, `"species:sleep"`, `"species:mama"`, `"species:chick"`, `"egg:species"`, `"crow"`. Level 1 stays as in §2 (missing fields default: world 1, variant noon, dayLength 0, kind normal). Validator rules: species counts exactly N each (eggs included), no exposed egg and no complete flock at start, crows only with at least one twig.

A Python tool `tools/levels/v5_levels.py` (run on 251) validates every level: counts, solvable without boosters by exhaustive search, par = shortest solution, random-play win rate (1000 runs), dead-state share, the role gate (§11) and the ordering rule (§6.5). Core C# has a matching solver used by the Firefly hint and by EditMode tests that re-verify par for every level.

## 13. Development roadmap

| Task | Owner | Content | Depends on |
| --- | --- | --- | --- |
| T31 | Codex + omp | v5.1 layout rework (in progress) | — |
| T32 | omp | Core v5.2: eggs, sleepy, crow, twig, growing branch, rule order, Chorus, boosters as undoable actions, multi-birdhouse per species, schema; EditMode tests for every rule in §1, §6, §7, §8.1 | T31 |
| T33 | Codex | Art: species 3–8 + crow full frame sets, egg/nest frames, sleep frames, outfits (3 per species), booster icons, count badge, W1 kit variants, per-species birdhouse roof bands | T31 |
| T34 | omp | Animation polish per §5 (attention, branch flex, egg/sleep/crow/growth, Chorus, ambient particles, transitions) | T32, T33 |
| T35 | Claude (design) + Python worker | 40 levels authored + `v5_levels.py` validation; Claude reviews every level's role | T32 |
| T36 | Codex | W2–W4 scene kits and variants, map art, shop/Aviary UI art, alt birdhouses | T33 |
| T37 | omp | Meta: world map, shop, Aviary, coins/daily gift, saves | T32, T36 |
| T38 | Claude + Mac | Full playthrough on Mac, screenshots, tuning | all |

Every task: spec in `tasks/`, GitHub issue, Mac test run (EditMode + PlayMode green, screenshots), Claude review before merge.
