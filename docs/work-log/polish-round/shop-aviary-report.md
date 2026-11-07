# Shop and aviary: report

Task: [task-shop-aviary.md](task-shop-aviary.md), contract: [spec.md](spec.md). Branch `crazyniu/polish-shop-aviary`.

## What changed

1. **Booster captions.** Every booster card shows one painted sentence under its name (cap 28, ink at 85 %), and the
   booster buy popup shows the same sentence under its title (cap 44). The sentences are the existing
   `booster.<id>.description` labels, reworded in `tools/kit/locales/en.json` (values only, no reordering) and
   re-baked with `build_localized_labels.py --labels-only`:
   Firefly "Shows a good move", Sapling "Adds one branch", Owl Eye "Peeks inside the eggs", Morning Bell "Wakes
   sleeping birds", Scarecrow "Sends the crows away", Spring Rain "Lets branches grow". "Shows what is in the eggs" was
   too wide for the card at a readable size (490 px at cap 28 against 428 px of room), so Owl Eye uses "Peeks inside the
   eggs". The same labels appear in the board's booster-unlock popup (`popups.gd`), which now uses the new wording
   too. Booster cards grew from 448 to 472 px to fit the caption without shrinking the picture; the grid still ends
   at y=1864.
2. **Outfits tab = bird kinds.** The tab is now a 3x3 grid, one card per species that wears outfits (the crow has
   none), all on one page with no pager. A met kind shows the live bird wearing its current outfit (breathing, blinks
   and a cheer every few seconds), its painted name and a hanger with owned/total such as `3/4` (the plain look
   counts as one of four). An unmet kind is the aviary's breathing silhouette with a white `?`; a tap only squashes
   the card. Tapping a met kind opens **the aviary's own detail page** (`aviary_view.gd`, not a copy) above the shop
   with a 0.18 s fade: hero bird, ability, four outfit tiles (equip, buy popup with the bird wearing the outfit, dimmed
   check and price shake when coins are short). Opened from the shop, the page adds the coin counter at the right of
   its bar and its Back button closes the page, returning to the shop (counts, worn outfits and coins refresh). The
   aviary itself is unchanged in layout: no coin counter there, Back still returns to its list.
3. **Tab pictures.** The three tabs use `Art.tex("tab_boosters" | "tab_outfits" | "tab_houses")` fitted into 96 px on
   the existing 128 px round button: blue circle when selected, cream otherwise, same size in every state, and the
   picture sinks with the button on press.
4. **Aviary loads (d).** Verified: `aviary_view:list`, `list_some_unmet`, `list_scrolled`, `detail_*`, `buy_outfit*`
   captures render without script errors, and the scripted run opens the aviary from the game, opens Berry, opens a
   buy popup, goes back and closes it.

Smaller fixes in the same files:

- Shop and aviary rebuilds now detach old cards/tiles before freeing them, so rebuilt nodes keep their names
  (`booster_firefly`, `Tile_2`, ...) instead of getting auto-renamed; the aviary bump uses `get_child(index)`.
- The `confirm_multiple` / `confirm_maximum` shop previews used the wrong id `booster.owl_eye` (the catalog id is
  `booster.owlEye`) and crashed; fixed.
- Removed the now unused outfit branch of the shop card picture and the tier/species outfit ordering.

Files: `PerchSort/game/shop_view.gd`, `PerchSort/game/aviary_view.gd`, `tools/kit/locales/en.json`, six re-baked
`PerchSort/art/locale/en/label_booster_*_description.png` plus `manifest.json`, and a one-line hook in
`scripts/screenshots.sh` (new shop preview states). `main.gd` needed no change: the shop forwards the page's
`profile_changed`, so main saves as before.

## Tap targets (both screens)

| Screen | Target | Result (checked in the scripted run unless noted) |
| --- | --- | --- |
| Shop | Close (red) | closes the shop |
| Shop | Coins counter | display only |
| Shop | Tabs Boosters / Outfits / Houses | switch tab; the selected tab stays selected |
| Shop, boosters | Card or orange price pill | opens the buy popup |
| Shop, boosters | Locked booster card | dimmed with a lock, does not take input (by design: disabled = dimmed) |
| Buy popup | `-`, `+` | quantity down/up, dimmed at 1 / at the affordable maximum |
| Buy popup | Quantity field | numeric typing (existing behaviour, not re-tested) |
| Buy popup | Buy | buys the quantity, coin counter bumps; dimmed when unaffordable |
| Buy popup | Close (red) | closes without buying |
| Shop, houses | Price pill / card | buy popup; owned: green hanger pill equips; worn: green check, a tap does nothing |
| Shop, outfits | Met kind card | opens that bird's outfit page |
| Shop, outfits | Unmet kind card | squash only, no page |
| Bird page (from shop) | Back | returns to the outfits tab |
| Bird page | Owned tile | equips it, hero cheers; the worn tile does not take input |
| Bird page | Locked tile | buy popup; check buys and equips; check dimmed and price shakes when coins are short |
| Bird page | Popup close | closes the popup |
| Aviary | Close | closes the aviary |
| Aviary | Met card / unmet card | detail page / squash only |
| Aviary | List drag / wheel | scrolls |
| Aviary detail | Back, tiles, popup | as on the bird page, Back returns to the list |

## Commands and results

- Labels: `uv run --no-project --isolated --with pillow tools/kit/build_localized_labels.py en --labels-only booster.firefly.description ...`
  then `godot --headless --path PerchSort --import`.
- Captures: `bash scripts/screenshots.sh shop_view` and `bash scripts/screenshots.sh aviary_view` -> `failed=0`, files in
  `build/screens/`: `shop_view__boosters.png`, `__outfits.png`, `__outfits_some_unmet.png`, `__bird_outfits.png`,
  `__confirm_booster.png`, `__confirm_multiple.png`, `__confirm_outfit.png` (Sparkle Tiara over Berry's page, opened
  from the shop), `__confirm_outfit_short.png`, `__houses.png`, `__locked_tools.png`, `__insufficient.png` and all
  `aviary_view__*.png`. Tabs are visible on every shop capture.
- Scripted run with real viewport mouse clicks through the game (board -> shop -> boosters -> popup -> houses ->
  outfits -> Berry -> buy -> equip -> back -> close -> aviary -> detail -> popup -> back -> close), in Godot movie mode
  at a fixed 10 fps so frame numbers are game time:
  `xvfb-run -a -s "-screen 0 1280x2000x24" godot --path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 1080x1920 --write-movie build/evidence/shop-aviary/movie/f.png --fixed-fps 10 -s build/evidence/shop-aviary/shop_flow.gd -- --level=1 --capture=/dev/null --motion --wait=600`
  (`--capture` only makes the profile sandboxed so nothing is saved). Log `build/evidence/shop-aviary/flow.log`, exit 0:

  ```
  STATE start coins=1000 equipped={} outfits=[] inventory=[0,0,0,0,0,0]
  STATE after firefly x2 coins=940 inventory=[2,0,0,0,0,0]
  TAB now=1 kinds=9 / UNMET opened page=false / PAGE opened=true species=berry
  STATE after buying berry 2 coins=440 equipped={"berry":"outfit.berry.2"} outfits=["outfit.berry.2"]   (counter 440)
  STATE after equip plain   equipped={}
  STATE after equip berry 2 equipped={"berry":"outfit.berry.2"}
  SHORT check disabled=true alpha=0.45; tapping it leaves coins=100 and outfits unchanged
  BACK page=<null> tab=1, GRID berry owned=2/4, SHOP closed=true, AVIARY ... closed=true, FLOW done
  ```

  Every TAP line names the control that was actually hit (`hit=Price`, `Increase`, `Open`, `Locked`, `Check`,
  `Equip`, `Back`, ...).
- Timed frames (311 frames, 0.1 s apart): `build/evidence/shop-aviary/movie/`. Contact sheets:
  `sheet_boosters_popup.png` (frames 17-57), `sheet_outfits_grid_idle.png` (92-103, birds breathe and blink),
  `sheet_open_bird_page.png` (112-123: card squash, 0.18 s fade to Berry's page), `sheet_buy_and_equip.png` (128-167:
  popup, purchase 940 -> 440, equip changes, hero cheers), `sheet_back_to_shop.png` (201-212).
- Tests: `bash scripts/run_tests.sh` -> exit 0: naming 0 violations (self-test ok), smoke 0 failures, core 0 failures
  (409 s). Log `build/evidence/shop-aviary/run_tests.log`. After a last one-line cleanup (an unused constant removed
  from `shop_view.gd`) the smoke test (`godot --headless --path PerchSort -- --smoke`, 0 failures), the naming guard
  (0) and the outfits capture were re-run.

## Open points

- The shop has no way back to the default house once another house is worn (no "default" house card). Not in this
  task; flagged for the coordinator.
- The coin counter appears on the bird page only when it is opened from the shop, to keep the approved aviary layout;
  adding it to the aviary detail too is a one-line change if wanted.
- Owl Eye's caption is "Peeks inside the eggs" rather than the brief's "shows what is in the eggs" (width, see above).
- The outfits grid is a new layout (3 columns, 312x456 cards) and needs the user's approval on the Mac; the frames
  above are still pictures of motion, not a substitute for trying it.
