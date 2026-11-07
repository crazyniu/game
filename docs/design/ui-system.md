# UI system

One visual system for every screen. Mockups in `art/approved_ui` set the mood (soft plush felt, warm forest colours); they are **not** a pixel target. When a mockup and this file disagree, this file wins. All values are in the 1080x1920 design space.

## 1. Grid and spacing
- 8 px grid. Spacing steps: 8, 16, 24, 32, 48, 64.
- Screen side margin 40; top margin 40; bottom margin 48.
- Two elements never touch: at least 16 px between separate controls, 24 px between a control and a panel edge, 32 px between content groups in a popup.
- Nothing may overlap another element unless this file says so (coin overlapping its pill, close disc overlapping a panel corner, badge on a portrait ring, title ribbon across a panel's top edge).

## 2. Painted numbers (UI.number) and words
Cap heights (the `rect` height passed to UI.number):
| Role | Cap height |
| --- | --- |
| Hero number (reward total, level number on the map current node) | 72 |
| HUD and pill main numbers (coins, moves) | 52 |
| Secondary numbers in pills and cards (prices, counts) | 40 |
| Small labels (star thresholds, badge counts) | 30 |
Colour: `brown` on cream/light surfaces (pills, panel interiors, cards). `cream` on orange/green/grey surfaces (map nodes, buttons). `white` only on red badges. Never mix colours within one group.
Words are painted label sprites: title ribbons (`label_title_*`) 110-130 px tall; caption plates (`label_caption_*`) 60-64 px tall. All captions in one group share one height (and one width when they sit side by side). If a caption cannot be made uniform, drop it where the icon is clear.

## 3. Buttons
- Round disc (`btn_wood`): standard 128. Primary disc (`btn_green`): 160 (= 1.25x). Close disc: 96, centred on the panel's top-right corner, inset 24 px.
- Glyph icon inside a disc: 50 % of the disc diameter, centred.
- Same-role buttons have identical sizes in every state; disabled = 45 % alpha, never smaller.
- Hit area is at least the disc; touch targets never smaller than 96.

## 4. Pills and badges
- `stat_pill` height 96 (HUD) or 80 (inside popups and cards). Icon in a pill 60-64 high; number per section 2.
- Coin pill: coin icon 104 overlapping the pill's left end by half its width, number left-aligned 24 px after the coin.
- Red stock badge 52 at a disc's top-right, white number 30.
- Portrait ring (`badge_ring_gold`) 104 on the board, 150 in popups; the bird face/front frame fits inside at 80 % of the ring and is never stretched; ability badge 44 % of the ring at its bottom-right.

## 5. Popups
- Dim 0.72, soft shadow, `panel_wood` frame.
- Width 880 (x 100-980) for content popups, 720 for small ones (settings, confirmations). Inner padding 56. Height follows the content: no empty areas larger than one content row.
- Title ribbon centred on the panel top edge (half above, half below), width 520-640.
- Content stacked top to bottom with 32 px between groups, centred.
- Actions sit BELOW the panel, 40 px under its bottom edge: one primary disc 160, or a row with secondaries 128 and one primary 160 spaced 64 apart.
- Birds in popups are live BirdViews (breathing, blinking, cheering), sized so the whole bird is visible; a hero bird stands on the top edge of the ribbon, never hidden behind it.

## 6. Board
- Top-left column (x 40-380): coin pill (y 40, h 96), level pill (y 152, h 112: footprints + moves, divider, three stars 44 with thresholds 30 below them), day/night pill (y 280, h 88) only when the level has day/night.
- Gear disc 112 at the top-right (x 928, y 40).
- Goal house centred at x 540, at most 260 wide, hanging from its rope; goal badges in one centred row 24 px under the house, rings 104, 24 px apart.
- Bottom toolbar: cream bar centred, tool discs 128 with stock badges, 48 px from the bottom.
- Side trees: complete trees standing at both screen edges (never cropped strips or straight poles); every branch grows out of its tree through a bark collar.

## 7. Map
- A soft top shade (dark green to transparent, y 0-280) behind the HUD so path nodes passing under it never fight the HUD. Coin pill top-left, world title ribbon centred at the top (480 wide), both on the shade.
- Nodes: number centred inside the node at cap 52 (`cream`); locked nodes show the number and a small padlock badge (56) at the node's bottom-right, both inside the node; cleared nodes show three small stars along the node's lower rim.
- Current node: larger node, live bird standing on it at 70 % of the node width.
- Bottom nav bar 160 tall, 48 from the bottom, three discs 128 evenly spaced.

## 8. Shop and aviary
Same pills, buttons, numbers and ribbons as above; cards keep 24 px gutters; item pictures fit inside their card's picture area without stretching.

## 9. Window
Design space 1080x1920, stretch `canvas_items`, aspect `keep` (letterbox in the dark forest-green clear colour), desktop window 468x832 so it fits a laptop screen without being resized.
