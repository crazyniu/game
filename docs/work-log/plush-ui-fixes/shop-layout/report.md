# Shop layout delivery

Implemented on `mac-shop-layout/work`, based on `origin/main` at `f1b37110`.

The owned shop layout is delivered for coordinator review. All 20 shop states were silently captured and every final side-by-side JPEG was opened and inspected. Independently mapped foreground RectTransforms are within 20 px of their corresponding measured rects. **This is not full visual acceptance:** the existing sprite family differs from the mockups, baked sprite details have no independent transforms, and the shared confirmation surface still needs its owner's adjustment.

## Changes

- Added the 317 measured entries from the four shop JSONs to only the shop keys in `layout_metrics.json` and `GameLayout.Screen`.
- Converted absolute top-left coordinates to each parent by subtracting the parent's top-left origin, then using `anchoredPosition = (rect.center.x, -rect.center.y)` and `sizeDelta = rect.size`.
- Reused leaf-free `card_wood` with alpha-padding compensation and nine-slice scaling. Booster cards are 459 x 420; outfit and house cards retain their individually measured approximately 470 x 650 proportions.
- Positioned orange/gold medallions, tool illustrations, larger red badges, separate overlapping coins, rounded cream price pills, tab discs, selected rings and underlines.
- Enlarged portraits and houses to measured slots. Existing front portraits and existing animation sequences remain in use; animation canvases retain their alignment instead of cropping each frame independently.
- Added a soft gold equipped edge using nine-slicing of the existing `glow_soft` texture, with the existing corner check.
- Placed confirmation medallion, image, price, coin, number, ribbon, close and accept at the measured foreground rects. The shared `Panel surface` call and shared `GameUI` implementation were not changed.
- Confirmation blocks page raycasts without disabling the underlying buttons, avoiding a second layer of button dimming below the scrim.
- Preserved catalog IDs, prices, ownership actions, purchase logic, saved data, levels and every `IScreenStates` name. Preview-only confirm fixtures use 240 coins; the two Names states now show the final catalog page.

## Existing sprite inventory and substitutions

The complete `Resources/Art` PNG inventory was listed before implementation. Relevant existing assets used: `card_wood`, `panel_wood`, `stat_pill`, `coin`, `btn_wood`, `btn_wood_pressed`, `btn_green`, `btn_green_pressed`, `btn_green_pill`, `badge_ring_gold`, `badge_red`, `check_green`, `glow_soft`, `icon_bulb`, `icon_aviary`, `icon_home`, `icon_close`, `icon_check`, `padlock`, all six `tool_*` illustrations, four `house_style_*_body` images, the existing `Art/labels/label_card_*` family, and brown/white painted digit glyphs. `coin_pill` was inspected but its baked coin cannot independently match the measured coin and pill sizes, so the existing cream pill and coin are placed separately.

The pre-existing canonical forest from `World` and outfit portraits/frame sequences from `Birds` are retained; there are no corresponding outfit portraits under `Art`. No art file, font file, shader, importer, shared UI helper or resource root was created or edited. Runtime sprite cropping changes geometry only and preserves the source texture.

Missing exact mockup art / coordinator follow-up:

- No painted hanger glyph exists in `Art`. Outfit tabs and equip actions use `icon_aviary` at the measured glyph rect. A painted hanger is needed to match the reference.
- The existing bulb tab icon is yellow, not the reference's cream-white outlined icon. The existing close/home/check icons also have different outline weight.
- Existing labels have narrower, taller letters and different leaf clusters. Their full image height is reduced by 18 px, inside the tolerance, to improve visual weight. Matching wide rounded painted lettering/ribbons are still needed; baked title and leaf geometry cannot be independently moved without replacing or splitting the artwork.
- Existing tool illustrations, birds, houses, coin markings and background canopy are different paintings. In particular the house kit retains wood/ceramic textures; it cannot become the reference plush houses through placement changes. Existing front-facing outfit art is used at the measured portrait bounds.
- The exact reference's bright fuzzy card rim and glow falloff are not present. The mandated leaf-free card and existing soft-glow texture are the closest available combination.
- Shared popup owner: make `GameUI.Surface` / the shared popup surface honour the visible panel bounds. The unchanged `panel_wood` canvas has alpha bounds `(4, 38, 796, 519)` on an 800 x 540 image, so its unspecialized sliced surface remains visibly inset (38 px at the top and 21 px at the bottom). The foreground content is already positioned for `[110, 550, 860, 620]`; do not move that content to compensate for the frame.
- The confirmation mockup separately repaints its backdrop. The live booster page remains stable when confirmation opens; its backdrop has up to 51 px difference from those secondary backdrop measurements. Outfit/house confirm states have intentionally different tabs and merchandise.

## Rect evidence

[Element map](element-map.md) lists **every one of the 317 JSON elements**, its measured rect, runtime object and mapping kind. [Comparison script](compare_rects.py) reads the capture-time `GetWorldCorners` dump produced by `ShopFakeData`, transforms corners into the shop's 1080 x 1920 design space, and prints `max(abs(actual - expected))` across x, y, width and height for each independently positioned element. [Full output](rect-deviations.txt) and [state summary](rect-summary.json) are committed.

`BAKED` entries explicitly print `max=N/A`: a coin leaf, ribbon letters, cushion, bird accessory or tool subpart shares its image transform. This script does **not** certify their pixel bounds. `STATE` marks intentionally absent ownership/stock objects or the unused last catalog slot. Confirmation backdrop deviations are printed separately. The shared frame's RectTransform matches its slot, but its visible texture padding remains the separate issue above. A good transform result does not establish complete visual parity.

| State | Side-by-side: mockup left, capture right, height 1280 | Max mapped foreground deviation | Remaining differences / state variation |
| --- | --- | --- | --- |
| Boosters / Stock badges | [boosters-stock-badges.jpg](boosters-stock-badges.jpg) | 18 px | Reference stock and 240 coins; common art differences below. |
| Boosters / Zero stock | [boosters-zero-stock.jpg](boosters-zero-stock.jpg) | 18 px | Badges intentionally hidden; 1240 fixture coins. |
| Boosters / Locked tools | [boosters-locked-tools.jpg](boosters-locked-tools.jpg) | 18 px | All tools dimmed at unchanged card size; unlock chips remain inside cards. |
| Boosters / All tools unlocked | [boosters-all-tools-unlocked.jpg](boosters-all-tools-unlocked.jpg) | 18 px | All six cards active; 1240 fixture coins. |
| Numbers / Zero coins | [numbers-zero-coins.jpg](numbers-zero-coins.jpg) | 18 px | Zero remains centred in the measured balance slot. |
| Numbers / Maximum coins | [numbers-maximum-coins.jpg](numbers-maximum-coins.jpg) | 18 px | 99999 fits the balance slot by reducing glyph size. |
| Numbers / Maximum stock | [numbers-maximum-stock.jpg](numbers-maximum-stock.jpg) | 20 px | 99999 fits each unchanged badge; number slot grows from 38 to 58 px. Digits are necessarily small. |
| Houses / Not owned | [houses-not-owned.jpg](houses-not-owned.jpg) | 18 px | All four houses and 600-price rows fit; existing house textures are wood/ceramic rather than the reference plush art. |
| Houses / Owned and equipped | [houses-owned-and-equipped.jpg](houses-owned-and-equipped.jpg) | 18 px | Mushroom has glow/check; teapot has equip action; their prices are intentionally hidden. |
| Outfits / Mixed ownership | [outfits-mixed-ownership.jpg](outfits-mixed-ownership.jpg) | 18 px | Scarf equipped, Flower owned; existing Round Glasses catalog price is 300, while the mockup says 500. Economy unchanged. |
| Outfits / All not owned | [outfits-all-not-owned.jpg](outfits-all-not-owned.jpg) | 18 px | All four cards show their existing 300 catalog price; ownership differs intentionally from the mixed-state mockup. |
| Outfits / Owned not equipped | [outfits-owned-not-equipped.jpg](outfits-owned-not-equipped.jpg) | 18 px | Four equal-sized equip discs; missing painted hanger uses the existing aviary icon. |
| Outfits / Equipped | [outfits-equipped.jpg](outfits-equipped.jpg) | 18 px | Four glow/check states, no price/equip controls; glow uses the existing soft-glow texture. |
| Names / Longest painted ribbon | [names-longest-painted-ribbon.jpg](names-longest-painted-ribbon.jpg) | 18 px | Final catalog page shows Graduation Cap, Reading Glasses and Wizard Cape; compared against the first three measured card slots. |
| Names / Full outfit catalog | [names-full-outfit-catalog.jpg](names-full-outfit-catalog.jpg) | 18 px | Final catalog page; 27 items retained, three items on the last page. Fourth slot intentionally empty. |
| Confirm / Booster | [confirm-booster.jpg](confirm-booster.jpg) | 18 px | Foreground content aligned; shared panel inset and painted-art differences remain. Retained booster backdrop differs by up to 51 px from the separately painted confirm backdrop. |
| Confirm / Outfit | [confirm-outfit.jpg](confirm-outfit.jpg) | 18 px | Same measured popup content layout, with Straw Hat and 300. Outfit backdrop and selected tab intentionally differ from the Hint mockup (backdrop max 176 px). |
| Confirm / House | [confirm-house.jpg](confirm-house.jpg) | 18 px | Same measured popup content layout, with Mushroom and 600. House backdrop and selected tab intentionally differ from the Hint mockup (backdrop max 363 px). |
| Confirm / Disabled accept | [confirm-disabled-accept.jpg](confirm-disabled-accept.jpg) | 18 px | Accept is dimmed at the same 124 x 124 rect. Shared panel inset remains; booster backdrop max 51 px. |
| Feedback / Insufficient coins | [feedback-insufficient-coins.jpg](feedback-insufficient-coins.jpg) | 18 px | 20 coins, unchanged card/price layout. Instant still does not prove the shake/flash animation. |

Common remaining art differences in the inventory section apply to every row. Price/ownership variations use the real catalog and fixtures; they are not corrected by changing economy values to imitate the mockup.

## Commands and capture results

```text
git fetch origin && git switch -C mac-shop-layout/work origin/main
bash scripts/state_captures.sh -states "Shop"
uv run --with pillow python docs/work-log/plush-ui-fixes/shop-layout/compare_rects.py --pairs
python3 ci/check_current_game_names.py
```

The capture command was run four times, with layout/visual fixes between runs. The final run reported `SUMMARY captured=30 selected=30 errors=0`: **20 owned Shop states**, plus 10 existing Board preview states whose names also contain Shop. The single Shop filter covers all seven assigned filter families without repeatedly restarting Unity. Only the 20 Shop pairs are delivered. Each final capture reports four settling frames, `listenerVolume=0` and `listenerPaused=true`. Unity compiled the changed source as part of the required capture entry point.

The final standalone naming guard inspected 265 tracked text files and reported zero violations.

The capture tool automatically runs its 14 naming-guard self-tests; these passed on all runs. No EditMode suite, PlayMode suite, separate build, lint, timed animation, audio playback or device validation was run. One non-game Unity Editor QuickSearch `ArgumentOutOfRangeException` is recorded as `UNITY_ERROR` in the capture index; the tool still completed every selected state and reported zero capture/runtime errors. It is not hidden as a clean Editor log.

Generated evidence remains in `build/screens/states/Shop`, `build/screens/states/shop-rects.jsonl`, `build/screens/states/index.txt` and `build/screens/states/unity.log`. The comparison script uses the latest dump per state. The four mockups were extracted from `origin/ui-design-review:art/ui_design/<screen>.png` into `/tmp` before comparison. Every final JPEG was opened and reviewed individually, not only as a contact sheet.

Scene/settings/packages were unchanged against the pre-capture snapshot. Unity rewrote 925 tracked metadata files and created 242 untracked metadata files during import; all were restored/removed using the initially clean workspace and recorded import snapshot. Only the four owned source/data files and this task's documentation/evidence are committed. No primary checkout changes, merge, main push, PR, deployment or release.
