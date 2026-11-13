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
| Daily reward / draw | Centers 960,920 and 960,1140; 120 px icon boxes, 220 px vertical spacing |
| Play | 290,1360,500,200; existing complete localized action, unchanged placement |
| Bottom destinations | Centers x=174+244×column, y=1760; 132 px proportional icon boxes |
| Hit areas / badges | Seven icon hit areas 160 × 160; red availability badges 48 px |

The bottom four and right-side two icon controls also share the independent
`btn_home_icon_frame` backing. The complete texture box is 204 × 204 for bottom
destinations and 184 × 184 for events, centered on the existing control. Its
original transparent padding is retained, and the frame is drawn at 0.88 opacity
behind the accepted icon and notification badge. Its low-saturation dark-green center is translucent (approximately 40% source
alpha), allowing the background to remain visible while reducing its interference. Settings has no new backing. Press feedback only darkens the
existing icon and new frame without moving or resizing either. See
`docs/work-log/home-icon-contrast/` for the delegated asset review and checks.

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

The user approved the forest aviary list and bird detail references in
`art/approved_ui/aviary.png` and `art/approved_ui/aviary_detail.png`. This supersedes the
older sea-background treatment. Live species and outfit positions remain open displays,
with independent leafy wooden perches over turquoise forest bokeh. No enclosing list,
hero or outfit cards are added. All measurements use the 1080 x 1920 canvas.

| Element | Bounds or placement |
| --- | --- |
| Forest | 0,0,1080,1920; `background_aviary_forest`, aspect covered |
| Close/back | x48; actual visible button face centered at y119; shared `MenuNavigation.SIZE`, existing coin-shop Close and forest Back sprites |
| List title | 300,32,480,174; amber leafy sign and painted word centered by visible ink at x540, y119 |
| Detail title | 308,32,464,174; same sign, visible center x540, y119; balance ends at x1056 |
| Met counter | 784,79,248,80; existing dark painted bar, green leaf, painted count |
| List viewport | 48,232,960,1592 |
| Species region | 464 x 468, two columns, 32 gap; transparent backing |
| List bird | Local 52,8,360,356; feet at y364 |
| List perch/name | 416 px branch at feet; name in 38,400,388,80 |
| Unknown species | Teal breathing species silhouette, cream question and 72 x 80 padlock; no name |
| Scroll rail | x1036, width18; 32 x 96 gold thumb, fixed readable size |
| Detail hero | 336,212,408,432; live bird, feet at y644 |
| Detail perch | Width640, center x540, contact y644 |
| Ability | Leafy cream capsule 232,714,616,104; warm painted caption 304,740,472,52 |
| Outfit heading | 340,836,400,80; existing gold painted heading |
| Outfit positions | 24,940,1032,768; six 328 x 372 regions in three columns, 24 gaps |
| Outfit bird/perch | Local 28,0,272,280; width328 branch, feet at y280 |
| Outfit state/price | Local 44,318,240,68; shared shop green/gold face, explicit Equip/Equipped label or coin and amount inside |
| Equipped marker | Explicit localized Equipped word in the shared action face; existing check badge at the upper right |

### Materials, states and interaction

Missing background, amber title, leafy perch, orange navigation, cream question and ability
capsule are independent generated production art, not extracted reference pixels. All passed
independent visual/alpha review before wiring. Perches retain native aspect ratios and place
their source wood contact at each live bird's feet. Existing character frames and localized
painted words remain canonical. No character mirror or static substitute is used.

Unmet species cannot open details and do not mutate progress. Met birds use continuous
breathing and existing drawn idle actions; tapping the hero invokes its existing showcase.
The marked default/price strips use the same shop action-face family and equal 240 x 68 bounds.
Owned outfits say Equip or Equipped using the existing painted locale words. Locked prices
keep the coin and exact canonical amount within the gold face, without an overhanging coin.
See `docs/work-log/aviary-action-consistency/report.md` for the focused correction.

Aviary aligns title words, ability captions and action words by their actual visible alpha,
not texture padding. Header navigation, sign lettering and coin-counter artwork share
y119; price coins and tight glyph ink form one centered group inside each action face.
A view-local luminance-alpha treatment reduces the dark baked surrounds on ownership
words and price digits without editing retained source images or shared text helpers.
The cream balance digits retain their original treatment. See
`docs/work-log/text-centering/report.md` for bilingual rendered checks and limits.

The detail keeps six equal outfit hit regions. Locked purchasable outfits remain full-color
as in the approved reference, with locks and price tags; actual disabled actions dim without
resizing. Existing owned/equipped, affordability, purchase and equip callbacks are unchanged.
The crow retains its ability with no outfit choices. Back restores the saved list scroll,
or closes to the shop when the shop owns this same detail view.

`setup(profile, species)` and canonical `preview(main, state)` remain the only view paths.
Previews include list/scrolled/unknown, sunny/berry/crow/hoot/ember detail, equipped,
maximum-balance and purchase/insufficient-funds states. Balance signals, save ownership,
locale switching, menu music and mute stay with their existing owners.

Material provenance, measured GUI comparisons, focused input checks and timed animation
results are in `docs/work-log/map-aviary-approval/`. No approved-runtime baseline is replaced.

## Aviary runtime acceptance

The user explicitly retained the Sunny detail page on 2026-11-12. Its supplied original is preserved at `art/approved_runtime/aviary_detail.png` (English). Keep this page's title plaque, header alignment, background, perches, ownership/price faces, visible lettering and layout unchanged. Source/provenance hashes are recorded in `docs/work-log/ui-layout-review/aviary-acceptance.json`. The separately opened purchase dialog can receive its authorized outside-frame Close placement through the local PurchaseDialog helper; that does not authorize changes to this detail page.

## Review boundary

Use the existing canonical `preview(main, state)` entry points and isolated fake profiles.
The user's current instruction authorizes implementation, repository checks, screenshots and
GUI interaction testing without another confirmation for each detail. The earlier no-testing
restriction is superseded. Complete the integrated tests and real runtime review before the
requested publication workflow; do not infer visual acceptance from headless tests alone.
Retain accepted assets and their source hashes. No approved-runtime baseline is replaced,
and no merge or deployment is part of this work.
