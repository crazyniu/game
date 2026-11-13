# Island screens

Status: design contract written 2026-11-10 from the user's design images and rules. No code or art produced yet. Work stays on `island-ui-redesign`; the island homepage is the user's finished reference and is not changed.

## Authority and method

- The user supplied island-style designs for Shop, Aviary list, Aviary detail, Lucky Spin, Settings and Star Chest, and a rule for currency and badges. The designs are a style direction, 1:1 in spirit, never pasted: every screen is rebuilt from independent assets drawn with the local image generator (Codex) in the same style, assembled by the canonical screen scripts.
- Words are painted label images (English and Chinese), numbers use the painted digit set. No font text in the game.
- Names are not literal translations; each language uses the natural game name (table below).
- UI first: each screen is a clickable shell with fake data through its `preview()`, approved by the user, then wired to logic. Layout and art do not change after approval.
- The felt/plush UI direction is retired. Birds keep their current fleece look; all UI is the glossy cartoon island style.

## Global rules

### Currency is "feather"
- Player-visible name: Feather (English), 羽毛 (Chinese). The gold leaf coin art stays.
- Internal field names (`coins` in `meta/profile.gd`, save JSON) are not renamed; only text, assets and documentation say feather.
- Every feather display is the home counter: `panel_home_island_coin` (gold feather coin, dark brown pill, green plus) with painted digits `icon_home_island_digit_*`.
- Digits are plain with no thousands separators. The display is capped at `99999` (display only, never written back). The plus is art only, no purchase action.
- One component builds it (extend the existing home implementation into a shared builder); Shop, Lucky Spin, popups and the board HUD all call it. Different position, same look and proportions.
- Prices on shop items use a gold pill (gold feather + painted digits), same digit set.

### Badges
- Four badge roles, one size each in every state (disabled is dimmed, never smaller):
  - Notification (red circle, white painted number): reward or new content available; max `99`.
  - Count (green circle, white painted number): owned quantity of a booster; max `99`. Hidden at 0.
  - Lock (gold padlock): locked item.
  - Check (green circle with tick): equipped or done.
- Numbers on badges use the same painted digits with a dark outline; values above 99 display 99.
- Collection progress such as `4/10` on the Aviary is a feather-leaf chip, not a feather counter.

### Kit (generated once, reused)
Wood-and-cream popup frame with hibiscus corners and shell, hanging rope sign (word-less base plus painted word layer), close X and back arrow tiles, green primary pill, cream secondary pill, gold price pill, toggle (on/off), language switch, cream card with selected green outline, scroll bar, page dots, badges, padlock, check, stars (filled/empty), progress bar. Same base for English and Chinese; only word layers change.

## Names (draft, English / Chinese)

| Item | English | Chinese |
| --- | --- | --- |
| Currency | Feather | 羽毛 |
| Shop tabs | Boosters / Outfits / Nests | 道具 / 装扮 / 鸟窝 |
| firefly | Firefly, lights a good move | 萤火灯，照亮好走法 |
| sapling | Sprout, grows a new perch | 新枝芽，长出一根新树枝 |
| owl_eye | Owl Eye, see inside an egg | 猫头鹰眼，看清蛋里的鸟 |
| morning_bell | Wake Bell, wakes sleeping birds | 唤醒铃，叫醒睡着的鸟 |
| scarecrow | Scarecrow, shoos crows away | 稻草人，赶走乌鸦 |
| spring_rain | Spring Rain, grows your branches | 春雨，让树枝长大 |
| Aviary | Aviary, friends you have met | 鸟舍，已结识的小伙伴 |
| Draw | Lucky Spin | 幸运转盘 |
| Chest | Star Chest | 星星宝箱 |
| Settings | Settings | 设置 |

The full word list with both languages is produced as one label manifest before generation.

## Screens

### Shop
Close tile top left, feather bar top right, hanging "Shop" sign. Three tabs (Boosters, Outfits, Nests) as pills with a painted picture; the selected tab is green. Boosters: one row per item (picture with count badge, name in gold lettering, short brown caption, gold price pill); locked boosters show a padlock and a dimmed pill. Outfits: bird kinds as cards, a met kind opens that bird's outfit page. Nests: house cards in a grid. Purchase confirm is a popup in the same frame. Island ocean backdrop, sand strip at the bottom.

### Aviary list
Close tile, hanging sign "Aviary" with the subtitle ribbon, collection chip `n/10` (leaf, not feather). A cream panel with a two-column grid of bird cards (bird on a small branch, name painted, a small gift badge top right); unmet birds are brown silhouettes with a question mark; scroll bar and a down chevron. Tapping a bird opens its detail page.

### Aviary detail
Back arrow, sign with the bird's name, large portrait card with the bird on a branch and its gift line, sign "Outfits", a three-column grid of outfit cards: Owned, Equipped (green outline and tick), or a price pill with a padlock. Buying uses the shop confirm popup.

### Settings
Frame popup with sign "Settings", close tile. Rows: Music, Sound, Vibration (toggles), Language (two-state switch), then a green Help button. The design adds music and vibration beyond the earlier mute-only rule; sound controls follow the design, vibration is interface only until the device feature exists.

### Lucky Spin
Frame popup with sign, two small birds on the sign, close tile. Five stars for the day's streak, a "spins left today" ribbon, a 3x3 prize board (feather piles of different sizes, gift boxes, the chest in the centre as the jackpot) in a rope frame, and a large green Spin button. Prize amounts are painted digits.

### Star Chest
Frame popup with sign, chest picture with glow, a one-line caption, a star progress bar (`n/30`), a green Open button and a green Play button.

### Map: world cards (replaces the scrolling trail)
- Ten worlds, one card each, swiped left and right with snap; the centred card is large and its neighbours peek in. A page-dot row shows the ten positions; the island backdrop parallaxes as you swipe.
- Each card shows the zone painting, a hanging sign with the zone name, and stars `n/30`. Locked cards are dimmed with a padlock and the requirement (half the previous world's stars); completed cards carry a gold ribbon.
- Opening a card shows that world's page: the zone painting on one screen, no vertical scrolling, ten level nodes numbered 1 to 10 along a winding path. The level number restarts at 1 in every world; the global level id and saves are unchanged.
- Nodes: cleared (green, one to three stars), current (larger, pulsing, a bird standing beside it), locked (padlock). Node 10 is the chest or gate. Tapping a node opens the existing level card popup. The back arrow returns to the carousel on the same world.
- Clearing level 10 plays the gate opening and slides the carousel to the next world.
- The old scrolling map, world ribbon and in-map gate states are removed from the map view.

### Later batches (same kit)
Win panel, level card, new friend, goal info, unlock cards, daily gift, tutorial popups, board HUD and tool bar: restyled with the kit after the screens above are approved. They also use the feather bar and badge rules.

## Delivery order
1. Shared kit and the feather/badge components.
2. Shop. 3. Aviary list and detail. 4. Settings. 5. Lucky Spin. 6. Star Chest. 7. Map carousel and world page. 8. Later batches.
Each screen: assets generated and checked (alpha, no fringe, English and Chinese), fake-data `preview()` states, silent 1080x1920 captures, then the user's approval before logic is touched.

## Acceptance
- Captures compared with the supplied design for style, spacing and hierarchy; every feather display identical to the home counter (digits, no separators, cap 99999); badge sizes identical per role; no font-rendered text; no pasted design image in the runtime.
- Motion (swipe snap, pop-ins, button press) verified with timed frame sequences, not stills.
- No change to rules, levels, economy values, save field names or enum orders.
