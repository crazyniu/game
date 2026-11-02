# Perch Sort v5 — gameplay and UI design (owner: Claude)

2026-11-02. The user rejected the v2 runtime and the v4 document set as not fun and visually weak, and assigned gameplay and UI decisions to Claude ("你来定玩法和 UI", "先把第一关跑通"), with a set of same-genre screenshots as **style direction only**. This page is the single authority for the v5 runtime. It supersedes `docs/spec/RULES.md` (v4), `docs/UI_SPEC.md` and the v4 evidence process **for v5 code only**; v2 code and the old docs stay untouched as history.

Originality: theme, bird characters, names, palette, layout numbers and text are ours. The reference screenshots show a genre convention (plush 3D cartoon birds on branches growing from the screen edges, a birdhouse goal at the top). Do not copy any of their characters, logos, text, coin/settings art or exact layout.

## 1. Core loop (v5 rules)

- **Board**: branches grow from the left and right screen edges, alternating rows. Every branch in a level has the same capacity `N` (level 1: N=4). Birds sit side by side; the bird nearest the branch **tip** (toward screen center) is the exposed one.
- **Species**: K species per level, exactly N birds of each.
- **Tap a branch** with birds → select it: the run of same-species birds at the tip hops up and looks at the player (selected). Tap the same branch or empty space → cancel.
- **Tap a target branch** → the run flies over if the target is empty or its tip bird is the same species. Count = min(run, free slots); the rest stay selected-cancelled on the source. Illegal target → the selected birds shake their heads, selection cancels, nothing else changes.
- **Flock home**: when a branch holds N birds of one species they cheer, then fly one by one into the **birdhouse** at the top of the screen. The branch stays and becomes an ordinary empty branch (no branch breaking in v5).
- **Win**: every species is home. **No lose state**: if no legal move exists, show a gentle "stuck" bubble that points at Undo / Add Branch.
- **Moves**: each successful transfer = 1 move. Undo restores the previous state including move count.
- **Stars**: moves ≤ par → 3 stars; ≤ par+3 → 2; otherwise 1. Stars are shown on the win panel and stored as best per level.
- **Coins**: win reward = 10 + 5 × stars. Shown top-left. Coins are display/progress only in this slice (no shop yet).
- **Tools (bottom bar)**: Undo (free, unlimited), Hint (free; highlights a source then a target from a solver path), Add Branch (once per level, free; adds an empty capacity-N branch at the next free slot).
- **Restart** and **sound mute** live in the settings/pause panel.

Later-level mechanics (designed, NOT in this slice; do not implement yet): mystery eggs that hatch when exposed; multiple birdhouses that each request a specific species in order (a finished flock of an un-requested species falls asleep on its branch until called); worlds of 20 levels each.

## 2. Level 1 ("First Flight")

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

## 3. Screen layout (portrait, design canvas 1080×1920, origin top-left for this table)

| Element | Placement |
| --- | --- |
| Background | full screen `bg_forest_day` (sky, soft distant forest, leafy canopy in the top corners) |
| Coin pill | top-left, 260×90, center (170, 150) in safe area; coin icon + number |
| Settings button | top-right, Ø120 round, center (980, 150) |
| Birdhouse | top-center, 300×340, center (540, 230); flock count shown as small bird icons under it |
| Branch rows | row y centers start at 720, spacing 300 (row 0 = 720, row 1 = 1020, row 2 = 1320). Left branches span x 0→600, right branches 480→1080 (rows alternate overlap via y offset of ±40: left rows at y, right rows at y+40). A leafy bush sits on the screen edge at each branch root. |
| Bird slots | bird display 150×150, slot pitch 120 along the branch, first slot 110 px from the screen edge; feet on the branch top line |
| Bottom bar | y center 1760; three rounded-square buttons 170×170 at x 300 / 540 / 780: Undo, Hint, Add Branch. Icon only. |
| Level pill | below birdhouse: "1" in a small rounded pill, (540, 430) |

Overlay panels (win, settings) are centered, dim the board with black 60% alpha.

**Win panel**: wooden-frame cream panel 860×900, a ribbon at the top, 3 star slots, the coin reward with a coin-fly-to-pill animation, buttons: Next (big green), Replay and Home (small). In this slice Next reloads level 1 (only level), Home also reloads level 1.

**Settings panel**: sound on/off toggle (icon), Restart, Close. No sliders, no text labels.

## 4. Visual style (for art generation)

Bright, warm, plush "3D-rendered cartoon" look: soft fur texture, big glossy eyes, round chubby bodies, orange feet, gentle rim light; sky-blue to cream gradient sky with soft clouds; saturated leaf greens; chunky rounded UI with thick soft outlines, glossy highlights, warm wood and cream panels. Text font: rounded heavy (Fredoka already in project). All button labels are icons.

v5 species (original characters, each distinct by silhouette AND color so they read in grayscale at 64 px):

| id | Look |
| --- | --- |
| sunny | lemon-yellow round chick, one curly head tuft, rosy cheeks, cheerful open smile |
| sky | sky-blue egg-shaped bird with a white belly and a little round top-knot bun, calm half-closed eyes |
| berry | raspberry-magenta bird with two long swept-back head plumes, sly wink pose |
| mint | mint-teal bird with a leaf-shaped crest, round glasses-like eye rings |
| cocoa | chocolate-brown bird with a cream face mask and short ear tufts, sleepy eyes |
| snow | white bird with lilac wing tips and a tiny lilac crown of three dots |

Only `sunny` and `sky` are required for level 1.

## 5. Animation (LitMotion, MIT, pinned commit 0b4c588)

All motion uses LitMotion; no fixed image stands in for animation.

- Idle: subtle breathing scale (1→1.03, 1.6 s loop, random phase) and random blink (swap to blink frame 0.12 s every 2–5 s).
- Select: run hops up 40 px (0.15 s, OutBack) and stays raised; un-select drops back.
- Transfer: each bird in the run departs 0.06 s after the previous; arc flight (0.45 s, bezier with peak 160 px above the midpoint), wing frames alternate up/down every 0.07 s during flight, land with a squash (scale 1.15×0.85 → 1, 0.12 s).
- Illegal: selected birds shake ±10° twice (0.25 s).
- Flock home: cheer jump (0.2 s), then each bird flies to the birdhouse door 0.1 s apart, shrinking to 0.4 while entering; birdhouse punches scale 1.1 on each arrival; sparkle burst.
- Win: panel scales in from 0.6 (0.35 s OutBack), stars pop in one by one 0.2 s apart, coins fly to the pill.
- Buttons: press scale 0.92, release OutBack.

Input is locked while a transfer or flock-home animation is running; game state changes are committed first, animation only plays the committed result.

## 6. Out of scope for this slice

World map, level select, more levels, eggs, multiple birdhouses, shop, ads, IAP, music. Audio: reuse existing SFX in `Assets/Audio` if trivially available, else silent; mute toggle must still persist.
