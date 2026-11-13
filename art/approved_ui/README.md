# Island screen designs (the only design-image folder)

Generated 2026-11-10 with the local image generator from `docs/design/island-style.md`. These are layouts and style proposals for review, not production assets and not approved. After the user picks, each screen is built from independent assets in the canonical screen scripts (UI shell with fake data first). Nothing here is pasted into the game.

| File | Screen | Notes |
| --- | --- | --- |
| shop.png | Shop, Boosters tab | two-column card shelf |
| shop_outfits.png | Shop, Outfits tab | bird kinds, owned counts |
| shop_nests.png | Shop, Nests tab | houses, Equipped / Equip / prices, pager |
| shop_buy.png | Purchase popup | quantity selector, total price on Buy |
| aviary.png | Aviary list | collection chip, bird cards, gift badges |
| aviary_detail.png | Bird page | portrait, gift line, outfit grid |
| map_carousel.png | Island map | swipeable world cards, page dots |
| world_levels.png | World page | ten fixed levels on one screen |
| board_play.png | Gameplay (day) | HUD, goal chips, branches, tool bar |
| board_night.png | Gameplay (night) | sleeping birds, hint firefly, night banner |
| win.png | Level clear | stars, moves and par, reward, drops, Next |
| level_card.png | Level card | best result, birds in level, mechanics, Play |
| new_friend.png | New bird met | ability line, reward, Welcome |
| unlock.png | New tool unlocked | Got it |
| world_unlock.png | New island | opening gate, Play |
| world_complete.png | Island cleared | 30 of 30 stars, reward |
| rewards.png | Reward popup | check-in or chest result |
| star_chest.png | Star chest | progress bar, Open, Play |
| goal_info.png | Bird ability popup | tapped from a goal chip |
| document.png | Privacy / terms / credits | scrolling text column |
| tutorial.png | How to play step | tip sign, mascot, Next |
| settings.png | Settings popup | toggles, language switch, Help |
| lucky_spin.png | Lucky spin popup | 3x3 board, Spin |

Home is the user's finished page and is not redesigned.

## Known differences between these proposals (resolved when building)
The generator is not consistent between images. In the build, these are made identical by shared components: the feather counter (always the homepage bar, plain digits), tab pills and icons, close and back tiles, badge size, price bar, popup frame and rope sign, and the lettering colours. Bird art in the build is the game's real bird frames, not these paintings.

Rule (user, 2026-11-11): every design image of this game lives in this folder and nowhere else. New or revised designs replace the file of the same screen here; old designs live only in Git history. `style_lock.png` is the component board that fixes the style.
