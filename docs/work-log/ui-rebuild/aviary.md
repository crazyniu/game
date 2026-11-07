# Aviary screen rebuild: report

Branch `crazyniu/rebuild-aviary`. Screen file `PerchSort/game/aviary_view.gd`, rebuilt from scratch on the cartoon
system (docs/design/ui-system.md). Public interface unchanged: signals `closed` and `profile_changed`,
`setup(p: Profile)`, `preview(main, state)` with states list, list_some_unmet, list_scrolled, detail_berry, detail_crow,
detail_hoot; `_set_scroll` and `_show_detail` keep their names for the previews.

## Design

- Background: `board_w1_bg` stretched to the screen under a cream wash (42 percent) so the cards read first.
- Top bar (row y 48 to 160, same positions as the board HUD): neutral cream secondary close at (112, 104), the title
  word "Aviary" (cap 64) centred, and a cream counter "met/total" with the ink feather icon right-aligned to x 1032.
- List: two columns of cream cards 476 x 456 (radius 40, edge 10), gaps 32, margins 48. Met birds are live BirdViews
  (breathing plus idle actions) with their name word (cap 44) and their ability badge `gift_coin_<effect>` (72) in the
  upper right; the crow has no badge. Unmet birds are a dark breathing silhouette with a white painted "?" glyph and
  the word "Unknown" in faded ink; they are not tappable (tapping only squashes the card).
- Scrolling: the grid is clipped under the top bar (clip from y 176). Drag scrolls with a 12 px threshold; release
  flings with exponential decay; the mouse wheel adds velocity. A slim ink track and thumb (8 px wide) sit in the
  right margin at x 1052. The list keeps its scroll position when returning from a detail page.
- Detail page: back button (cream secondary, `left` icon) top left and the bird's name as the title. One cream panel
  (radius 56, edge 12) holds a big live bird (680 box), the personality line (cap 44, ink 80 percent) and the ability
  line (badge 72 plus caption cap 44, same captions as the popups). Below it, 48 apart, a row of four cream outfit
  tiles 228 x 312: the plain bird, then outfits 1 to 3, every bird at the same size. Owned tiles equip on tap
  (`Shop.equip`, then `profile_changed`, the hero re-dresses and cheers, the tile bumps); locked tiles show the bird
  at 45 percent, an ink lock in the top-right corner and a coin plus painted price below; tapping a locked tile shakes
  it. The equipped tile wears a green check badge (56) in the same corner. The panel and row are centred in the free
  area on the 8 px grid.
- Crow detail: name, a bigger live bird (840 box), personality line and a home icon with "Never goes home"; no
  outfit row; the panel is centred so the page stays balanced.
- Names and ability captions come from `popups.gd` (`NAME`, `ABILITY`, `EFFECT`, preloaded) so the aviary matches the
  goal-info and new-friend popups. All words already exist in `tools/kit/words.txt`; no new words were needed.

## Private helper (kit gap)

`BirdView` plays the plain species frames for turns, preens, flaps, hops and still poses, so a bird wearing an
outfit loses it during those actions (seen in timed frames: the glasses vanished mid-turn). Every outfit has front,
blink and six cheer frames painted with the outfit. `_dress()` in aviary_view.gd keeps only those actions for a
dressed bird (blink and outfit cheer) and drives them with a timer (a blink every 1.5 to 3 s, a cheer every third or
fourth beat), on top of the continuous breathing. It reaches into BirdView private fields (`_seqs`, `_pose_names`,
`_last_action`, `_start_idle`). Recommended follow-up for the owner of bird_view.gd: prefer `<outfit>_<action>`
frames when they exist and skip actions without outfit frames; then `_dress()` can be removed. The same issue affects
dressed birds on the board and in the shop.

## Verification

1. `godot --headless --path PerchSort --import`, then `godot --headless --path PerchSort --audio-driver Dummy -- --smoke`:
   `smoke checks: 0 failures`. The only script errors are the expected ones from board_view.gd for worlds 3 and 4
   (missing `board_w3_slots.json` / `board_w4_slots.json`).
2. `python3 tools/kit/build_words.py --check`: `missing word sprites: []`.
3. `bash scripts/screenshots.sh aviary_view`: six captures, failed=0, in `build/screens/aviary_view__*.png`. Each
   was viewed at full size: no overlap or clipping, 48 margins, cards and tiles of one size per role, the close and
   back buttons at the board's settings size, readable words.
4. Timed evidence (`--frames --wait=6 --step=0.25`, detail_berry `--wait=8`) under xvfb in
   `build/evidence/aviary/{list,list_some_unmet,detail_berry}/f000.png ...`, contact sheets
   `build/evidence/aviary/detail_berry_sheet.png` and `list_some_unmet_sheet.png`. Every consecutive frame pair
   differs in all three sequences; the sheets show breathing, blinks, turns and cheers in the list, the unmet
   silhouettes breathing, and the dressed hero and tiles keeping their outfits through blinks and cheers.
5. `python3 ci/check_current_game_names.py --self-test` OK; `python3 ci/check_current_game_names.py`: 0 violations.
6. Interaction check (headless script driving the real `_on_scroll_input`): a 600 px drag scrolls 600 and does not
   open a detail; the fling carries to the end and stops; tapping an unmet card opens nothing; tapping Berry opens its
   detail; equipping outfit 2 sets `equipped.berry = outfit.berry.2`, emits `profile_changed` and re-dresses the hero;
   equipping the plain tile clears the entry; back returns to the list; close emits `closed`.

## Open points

- The `_dress()` workaround above (BirdView outfit frames).
- The species display names in `meta/aviary.gd` (Skye, Minty, Flurry, Rook, Inky) differ from the painted names used
  by the popups (Sky, Mint, Snow, Crow, Ink); the aviary follows the popups.
- The flocks-home count is no longer shown (the task's layout does not list it); the save field is untouched.
