# Home scene and aviary styling

The latest Home direction is the user's request to repair the simplified homepage, approved for
implementation on 2026-11-11 after reviewing the excessive cream cards and lost scene depth. This
supersedes the open beach/perch layout and Home's reuse of the shop surface. The Aviary section below
retains its separate current contract; this Home repair does not change that screen.

## Home

The user approved the raised, connected title-and-perch composition in
`art/approved_ui/home_title_concept.png`. The integrated Bird Sort roof/title extends into a natural
left branch and one lower open perch. Four existing animated birds occupy the middle, replacing
the empty gap; no separate large birdhouse or extra roof mascots remain. The user subsequently
approved the independent `background_home_jungle` image and requested that it replace only Home's
backdrop on 2026-11-11. The title, birds, controls, geometry and functionality stay unchanged.
This background-only change does not modify Settings or any other page.

All geometry uses the 1080 × 1920 canvas.

| Element | Placement |
| --- | --- |
| Forest backdrop | Full canvas; approved `background_home_jungle`, installed unchanged |
| Connected title and branch | Visible artwork fit in 128,208,832,832; `panel_home_perch_title` |
| Structure source bounds | 64,50,1120,1121 within the 1254 × 1254 transparent PNG |
| Macaw | Source contact 325,1084; 228 px nominal head-to-foot height; rest pose 4 |
| Yellow chick | Source contact 510,1096; 176 px nominal height; rest pose 0 |
| Blue chick | Source contact 715,1084; 178 px nominal height; rest pose 2 |
| Green parrot | Source contact 905,1082; 184 px nominal height; rest pose 4 |
| Counter / Settings | Counter at 48,48; Settings centered at 976,104, 96 px proportional icon |
| Daily challenge / draw | Centers 960,920 and 960,1140; 120 px icon boxes, 220 px vertical spacing |
| Play | 290,1360,500,200; existing complete localized action, unchanged placement |
| Bottom destinations | Centers x=174+244×column, y=1760; 132 px proportional icon boxes |
| Hit areas / badges | Seven icon hit areas 160 × 160; red availability badges 48 px |

Map, Shop, Aviary and Chest keep their current order. Icons fit measured painted bounds with ONE
uniform scale factor; never stretch width and height independently. Their 3.5 px pale contour,
6 px brown outer edge and restrained shadow remain. No cream backing cards or captions are added.

The title/branch PNG contains no birds. Each contact is transformed through the actual placed
texture's position and scale. Reuse all eight drawn idle poses of each existing species, the
per-frame parrot claw rows, continuous breathing and staggered idle scheduler. The intact transparent
structure is drawn over the birds so its branch naturally covers tails below the contact edge.
No artwork is mirrored and no reference image is sliced or pasted into the game.

Preserve all signals, availability conditions, next-level choice, coin purchase entry, menu music,
mute behavior and English/Chinese in-place Play replacement. The Home ambient layer has zero
leaves and additional sunlight; BoardView's independent level ambience is untouched. Canonical
`setup(profile)` and `preview()` interfaces remain.

### Assets and review

| Runtime asset | Provenance |
| --- | --- |
| `PerchSort/art/island/panel_home_perch_title.png` | Independent transparent production drawing of the approved connected structure |
| `PerchSort/art/island/background_home_jungle.png` | Independent bright jungle painting, explicitly approved for Home on 2026-11-11; installed byte-for-byte |
| `PerchSort/birds/home_island_{macaw,yellow_chick,blue_chick,green_parrot}_idle_*.png` | Retained original eight-frame animation sets |

Original source assets are retained. The connected foreground provenance remains in
`home-title-concept.md`. The approved bright background, checksum and latest validation are
recorded in `docs/work-log/home-forest/asset-validation.json` and
`docs/work-log/home-forest/report.md`. The background is a standalone production image, not a
flattened UI mockup. User runtime approval remains pending and no approved-runtime baseline is replaced.

## Aviary

The user approved repairing the repeated cream backdrops on 2026-11-11. Species and outfit
positions are open displays over the retained tropical sea. Warm wood supports the birds;
small labels, counters and price tags carry information. No enclosing list sheet, hero panel
or outfit card is drawn. This section supersedes the earlier cream-card treatment for Aviary.

All measurements use the 1080 x 1920 canvas.

| Element | Bounds or placement |
| --- | --- |
| Sea | 0, 0, 1080, 1920; retained `background_shop_sea` |
| Close/back | 48, 48, 112, 112 |
| List title plaque | 324, 48, 432, 112 |
| Detail title plaque | 232, 48, 432, 112; clear of the balance |
| Met counter | 768, 168, 264, 72; compact cream chip |
| List viewport | 48, 256, 984, 1568 |
| Species hit region | 480 x 432, two columns, 24 gap; transparent backing |
| List bird | Local 80, 24, 320, 288; feet at y312 |
| List perch and name | 360 px wide leafy branch at y312 contact; painted name 40, 370, 400, 54 |
| Unknown species | Existing question-bird artwork in local 60, 40, 360, 322; padlock at 240, 394; no revealed name |
| Detail glow | 232, 204, 616, 472; soft warm glow, behind the branch and live bird |
| Detail bird | 340, 232, 400, 400; feet at y632 |
| Detail perch | 592 px wide leafy branch, centre x540, contact y632 |
| Ability strip | 112, 748, 856, 80; painted caption inside 140, 762, 800, 52 |
| Outfit heading | 340, 850, 400, 48; retained standalone-list label |
| Outfit positions | 48, 936, 984, 688; six 312 x 332 transparent hit regions, three columns, two rows, 24 gaps |
| Outfit bird and perch | Local 40, 8, 232, 220; 272 px branch with contact at y228 |
| Outfit footer | Locked price at 52, 278, 208, 52; equipped small green marker at 156, 304 |
| Outfit status badge | Existing 66 px check or 56 px padlock centred at 272, 40 |

### Materials, states and interaction

Reuse the complete `island_branch`, `island_unknown_bird`, `glow_soft`, existing painted words,
status badges and dimensional controls. The leafy branch keeps its native proportions; its
painted centre contact is registered to the live bird's feet. The list name has no large backing.
The hero glow is translucent scenery, not a replacement card. The ability sentence keeps a slim
cream strip for contrast. Outfit price tags stay gold with white painted digits. An equipped
outfit has both the check badge and a small green foot marker; all six hit regions retain the
same dimensions. Locked outfit birds remain dimmed and open the existing purchase confirmation.

List taps, drag thresholds, fling behavior and saved scroll position remain unchanged. Unmet
species cannot open details. Detail bird taps still showcase drawn actions, and the existing
idle and cheer scheduling remains. Equipping replaces only the live hero inside its display
layer, keeping the perch below it. The crow keeps its ability sentence without outfit choices.
The shop continues to open this same detail view; Back closes to its existing owner. The standalone
Back restores the Aviary list. Balances, affordability, purchase/equip operations, mute/music,
locale switching and profile signals retain their current owners.

`setup(profile, species)` and the canonical `preview(main, state)` remain the only view path.
Existing list, scrolled, unknown, bird-detail, equipped, maximum-balance and purchase previews
remain. No assets are generated, edited or sliced, and no shared helper or locale manifest changes.
Runtime layout, input, animation, sound and performance remain unassessed pending user review.

## Review boundary

Keep and extend the existing `preview(main, state)` entry points with fake profiles. The user explicitly owns tests and acceptance; do not run tests, lint/builds, screenshots, UI automation or temporary validation programs. Visual and motion acceptance remain pending user review; no runtime baseline is replaced.
