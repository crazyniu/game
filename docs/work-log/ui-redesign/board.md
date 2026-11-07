# Board HUD redesign

Owner file: `PerchSort/game/board_view.gd`. Spec: `docs/design/ui-system.md` section 6.

## What changed
- The HUD no longer reads `layout/board_base.json`; all rects are named constants at the top of the HUD section.
- Every pill uses `stat_pill` nine-sliced at its own art height and scaled (`_pill`), so coin, level, day/night pills and the tool bar all have fully round ends and identical borders at any height.
- Coin pill: pill y 40 h 96; coin 104 centred on the pill's left end; number cap 52 brown, left-aligned 24 px after the coin (99999 fits in 212 px).
- Level pill: y 152 h 112; footprints, moves (cap 52) in an 84 px slot, divider, three stars 44 with thresholds cap 30 below them in 52 px columns 6 px apart (two-digit thresholds no longer touch).
- Day/night pill: y 280 h 88, dial icon 60, number cap 52; shown only with day/night.
- Gear disc 112 at (928, 40); glyph at 50 % fitted by visible pixels.
- Goal house: centred at x 540, 240 wide, top y 140; rope 16 wide ends at the hanger loop (clipped, starts above the screen so the entrance drop/overshoot shows no end). Door rect is derived from the house sprite instead of the old measured rect; resident and gift use it.
- Goal badges: one row 24 px under the house, rings 104, 24 apart (falls back to 16 px gaps and smaller rings only if a row would exceed 1000 px; max in the 40 levels is 7 = 872 px). Face fitted at 80 % of the ring inside a cream backing disc that also clips it to the ring hole (no squashing, no corners outside the ring). Ability/check badge 44 % of the ring at its bottom-right.
- Tool bar: cream bar 160 tall ending 48 px above the bottom, discs 128 spaced 64 apart, 40 px end padding; glyphs at 50 % (world tool illustrations 62 %) fitted by visible pixels; red stock badge 52 centred on the disc rim at 45 degrees, white number cap 30. Disabled tools are dimmed, same size.
- Sleep bubble: nine-sliced `sleep_bubble` at a fixed 35 % art scale (tail and ends never stretch), icons 64 with 48 px padding and 16 px gaps.
- Stuck hint: glow 1.375x the disc, pointer 64 placed 16 px above the disc and bobbing upwards so it never touches the disc or badge.
- Delivery gift coin pops to ring size (104) instead of 144.
- Stuck-hint slot flag for undo no longer indexes `inventory[-1]`.

## Deviation from the spec
- Spec column x 40-380 cannot hold footprints + moves (two digits at cap 52) + three star columns with two-digit thresholds at cap 30 without touching. Coin and level pills end at x 404 and the house is 240 wide (spec: at most 260), keeping a 16 px gap to the house.
- Tool bar is 160 tall like the map nav bar (section 7), giving 16 px between disc and bar edge instead of the 24 px of section 1. A 176 bar would satisfy section 1 but differ from the map.

## States looked at (two full passes, plus crops)
Levels 1, 2, 12, 25, 35, 37 with the met-all profile (plus tutorial flags and booster stock to skip unlock popups), previews `board_view:sleeping_bubble` and `board_view:stuck_hint`, and an autoplay frame sequence of level 2 (house drop, delivery, resident in doorway, check badge, gift pop).

## Verification
- `godot --headless --path PerchSort --import; godot --headless --path PerchSort --audio-driver Dummy -- --smoke` -> `smoke checks: 0 failures`, no SCRIPT ERROR.

## Remaining issues / missing art
- Non-default house styles (`house_style_*_body`) reuse the default house's doorway position; their doorway may not line up (pre-existing). Needs measured door rects per style.
- The bubble's moon (`dial_moon`, bare glyph) and sun note (`gift_sun_note`, gold coin) differ in visual weight; a matching pair of bubble icons would read better.
- `layout/board_base.json` is now unused by the board; whoever owns it may delete it.
