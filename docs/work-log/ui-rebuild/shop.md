# Shop screen rebuild: report

Task: `docs/work-log/ui-rebuild/task-shop.md`, contract `docs/work-log/ui-rebuild/spec.md`.

## What changed

- `PerchSort/game/shop_view.gd` rewritten from scratch on the cartoon kit (`UI.shape`, `UI.button`, `UI.counter`,
  `UI.icon`, `UI.number`, `UI.word`, `UI.art`, `Modal`). No wood frames, ribbons or label sprites remain.
- `PerchSort/art/words/word_shop.png` (+ `.import`): built by `tools/kit/build_words.py` from the `UI.word` literal
  "Shop". All item names were already in `tools/kit/words.txt`; it is unchanged.
- Public interface unchanged: signals `closed`, `profile_changed`; `setup(p, start_tab)`; `preview(main, state)` with
  boosters, outfits, houses, confirm_booster, confirm_outfit, locked_tools, insufficient. Buying, equipping and
  paging behave as before (single boosters only; outfits by tier then species; 4 per page).

## Layout (design pixels)

- Background: `board_w1_bg` full screen under a cream veil (#FFF7E8 at 42 percent) so cards read.
- Top bar centred on y 112 (48..176): close = cream secondary 128 at the left margin, "Shop" title cap 64 centred,
  coins counter (cream pill 96, coin illustration, ink number) right-aligned to the 48 margin. It bumps to 1.1 when
  the amount changes.
- Tabs centred on y 272 (208..336): three secondary 128 buttons (bulb, hanger, aviary), 48 apart; selected blue
  with a white icon, others cream with an ink icon. Identical sizes.
- Grid from y 384: two columns, cards 476 wide, 32 gap, 48 margins; cream card radius 40, edge 10. Booster cards
  456 high (3 rows, ends y 1816); outfit and house cards 640 high (2 rows, ends y 1696).
- Card: name word cap 40 centred at y 64 (shrinks only if wider than the card's inner width), picture area from
  y 104 to 24 above the bottom slot, bottom slot 240 x 96 whose bottom is 40 above the card bottom:
  - not owned: orange pill button, coin 64 + white price cap 44 centred;
  - owned: green pill button with a white hanger icon (same 240 x 96);
  - equipped: flat green check badge 96 (no shadow, not interactive) centred in the slot.
  - Boosters show their stock as a red 56 badge at the illustration's upper right. Locked boosters dim to
    45 percent with an ink lock 96 over the picture; their price pill does not take input.
- Pager (only when more than 4 items): cream secondary left/right buttons and a cream page pill 176 x 96 with an
  ink "n/m" number, centred on y 1792; the button at an end is dimmed and inactive, same size.
- Purchase confirm: `Modal.open` with the item name as title, close on the panel corner, picture 320
  (illustration or live bird), price line (coin 72 + number cap 52), green check primary 160 under the panel.
  Without enough coins the number is red, the check is dimmed and inactive, and the price line shakes once with
  the error sound right after the popup opens.

## Motion

- Cards fade and scale in from 0.94 (0.26 s back ease, 0.04 s stagger) when the screen opens, on tab change and on
  page turn. Pill buttons sink by their edge on press (5 px), like the kit's round buttons.
- Outfit birds are live `BirdView`s: continuous breathing and blinks from BirdView, plus a cheer and a double hop
  that alternate every few seconds.
- Private helper `_dress(bird, outfit)`: BirdView plays the species idle frames (turn, preen, flap, hop and still
  poses), which show the bird WITHOUT its outfit, and its cheer also uses the species frames. Only blink and cheer
  frames exist per outfit (`<species>_outfit_<n>_cheer_01..06`), so the shop keeps those actions (cheer swapped to
  the outfit frames) and clears the rest. The hop is a position tween of the whole bird, never a mirror.

## Verification

1. `godot --headless --path PerchSort --import`, then
   `godot --headless --path PerchSort --audio-driver Dummy -- --smoke`: `smoke checks: 0 failures`. The only script
   errors come from `board_view.gd` (`_build_scene`, `_layout_branches`, `_add_branch_art`, `_make_bird`) for the
   world 3 and 4 boards without slot files, as expected; none from the shop.
2. `python3 tools/kit/build_words.py --check`: `missing word sprites: []`.
3. `bash scripts/screenshots.sh shop_view`: 7 captures in `build/screens/shop_view__*.png` (failed=0), each opened at
   full size: no overlap or clipping, 48 margins, same-role sizes identical (tabs and close 128, all card slots
   240 x 96, pager buttons 128), text readable, consistent with the board and popups.
4. Timed evidence (xvfb, opengl3, 1080x1920):
   - `--frames=build/frames/shop_outfits --wait=8 --step=0.25 --preview=shop_view:outfits`: cards pop in, birds
     breathe, blink, cheer and hop; the hat, scarf, glasses and flower stay on in every frame.
   - `--frames=build/frames/shop_confirm_outfit --wait=6 --step=0.25 --preview=shop_view:confirm_outfit`: popup
     opens, the bird keeps its glasses while cheering and hopping.
   - `--frames=build/frames/shop_insufficient --wait=1.2 --step=0.05 --preview=shop_view:insufficient`: red price
     line left edge measured per frame 467, 463, 480, 457, 464, then steady at 464 (the shake).
5. `python3 ci/check_current_game_names.py`: 0 violations.

`build/` is ignored; captures and frames are not committed.

## Open issues

- BirdView (`bird_view.gd`, not owned here) drops the outfit during idles and cheer whenever an outfit is set. The
  shop works around it locally; the board, aviary and popups would need the same fix in BirdView (use
  `<outfit>_cheer` frames and skip actions without outfit frames), or outfit idle frames must be painted.
- The coordinator may want the shop's top bar on the board's centre line (y 104) instead of the task's 48..176
  (centre 112); it is one constant (`BAR_Y`).
