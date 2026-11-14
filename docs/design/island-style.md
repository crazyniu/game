# Island UI style

Recorded 2026-11-10 from the user's finished island homepage and their page designs (Shop, Aviary, Aviary detail,
Lucky Spin, Settings, Star Chest). The designs are a style source, not a layout to copy: each screen is designed
fresh in this style. The felt/plush UI is retired. Later forest-specific contracts supersede the original coastal setting.

## Setting and mood
Bright tropical forest cartoon styling. Use the approved page-specific jungle/forest backdrop, not the retired sea, surf, beach or pier composition. Keep backgrounds vivid and controls readable. Current popup materials are specified in [Purchase dialogs](purchase-dialogs.md) and [Outside dialog actions](dialog-layout.md).

## Palette
| Role | Colour |
| --- | --- |
| Sky and sea | saturated blue #2AA7F5 to turquoise #19C8D8, white foam |
| Leaves | fresh green #3FB93A, deep green #1E7A2A outlines |
| Hibiscus and badges | red #E5343B |
| Wood | warm orange-brown #C9691F, dark rim #6B3512 |
| Cream surfaces | #F8E8C6 with a soft #E9C88A rim |
| Gold | #FFC21A, deeper #E8920C |
| Primary action | calm single green #3FB52B with a soft white shine along the top edge and a cream rim; no multi-colour gradient |
| Counter pill | dark plum #46243C with cream rim |
| Text on cream | ocean teal #14697B; secondary symbols #288992 |
| Price digits on gold | painted cream white, using the shared homepage glyphs |

The user's latest 2026-11-11 correction selects white monetary digits again. Item names use dimensional painted gold; supporting data on cream retains ocean teal. The compact shop balance also uses white digits. Wood, bird colours, and narrow painted outlines are not text-fill colours. Existing source pack keys named `brown` remain compatibility identifiers only.

## Materials and shapes
Glossy cartoon (green surfaces stay flat), thick rounded shapes, one soft highlight along the top edge, gentle drop shadows, cream rims on
buttons and pills, no hard plastic edges, no felt or stitching. Generous corner radii everywhere. Every kind of
piece has one size and one look in every state (disabled is dimmed, never smaller).

## Lettering (all words are painted images, never a font)
- Gold: puffy glossy golden-yellow with a thick dark-brown outline. Titles on signs and item names.
- Teal: existing painted label shapes in ocean teal with their pale edges. Tabs and row labels.
- Caption: friendly rounded ocean teal, medium weight. Necessary short descriptions.
- White: puffy cream-white with a thick dark-green outline. Words on green buttons and the selected tab.
- Numbers use the homepage painted digits (cream with a soft brown drop shadow), plain, no separators.
- Names are natural game names for each language, never literal translations.

## Components
- Feather counter: gold feather coin on the left, dark plum pill with cream rim, painted digits, green plus
  opening the [coin shop](coin-shop.md); the whole counter is tappable. The coin shop's own counter is read-only. Every page uses the shop's fixed 320 x 106.97 counter, white digits and the same artwork; max 99999. Its dimensions belong to `FeatherBar`, with no per-screen width. Right-hand counters align their painted edge at x=1032 and start at y=48. Shop purchase faces show white amounts without a coin icon. Elsewhere a price is the feather coin on a gold bar with white digits; a
  reward ("+30") uses the same gold bar.
- Badges: round, one diameter. Red = reward or new, green = owned count, check = equipped or done, padlock =
  locked. Numbers never exceed 99.
- Tiles: cream rounded-square close (orange X) and back (orange arrow) tiles.
- Primary button: long green pill, one calm fill, one soft shine, white painted word. Secondary: cream pill.
- Cards and rows: cream with a soft rim; the selected one has a bright green outline; icons may overhang the
  top edge a little so they feel alive.
- Page and popup titles use painted lettering without a visible backing after PR #231 (`5d62178`). The older rope-hung wooden plank and later light-green banner requirements are historical; do not restore them from this document. Preserve independently scoped Home and Settings designs.
- Popups: orange wooden frame with a cream inner panel (960 wide), hibiscus and leaves at the corners, a shell at
  the bottom centre, a large rope sign (760 wide) with the gold title straddling the top edge and a cream close tile
  at the top right. Actions sit inside the frame at the bottom: green primary pill 430 x 162 (the home Play shape, one step smaller; user 2026-11-13), cream
  secondary pills 346 x 130 on the row under it. Hero pictures (bird, booster, chest) stand in a soft warm glow; abilities and
  descriptions sit on cream row cards.
- Settings: one cream row card per switch (picture, teal word, island toggle), then the language heading and one
  chip per installed language, each named in its own language; the current one is green. Any number of languages
  fits (two per row).
- Carousels (map worlds, shop outfits): the centred card is large with the green outline, neighbours peek in at the
  sides, chevron arrows between them, page dots under them. The map card carries the world number on a green disc
  and the world name on the leafy plaque; its stars and Play sit under it.
- Level nodes: cleared = green disc with white digits and stars, current = larger green disc in a glow with the
  bird on top, locked = cream disc with the padlock.
- Toggles: green with a tick and a cream knob when on, grey-beige with a dash when off.
- Progress: green or gold bars with a big gold star at the left end.

## Layout principles

8 px grid, margins 32 to 44 px on a 1080 x 1920 canvas, one clear focal block per page, large readable lettering,
tap targets at least 112 px, important content kept in the calm middle of the scene, decorations at the edges.

### Forest selection refinement

Requested on 2026-11-11: match all balance counters to the shop and improve the current forest selection
page. This is a layout refinement of `map_view.gd`, using its existing paintings, painted words and preview
interface. User runtime approval is pending. No additional assets or visible instructions are needed.

| Element | Box on the 1080 x 1920 canvas |
| --- | --- |
| Back tile | 48,48,112,112 |
| Balance | Shared 320 x 106.97 artwork; visible right edge 1032, top 48 |
| Selected forest card | 227,428,626,844; centre 540,850 |
| Neighbour cards | Existing 540 px stride and 0.72 scale |
| World badge | Existing 140 px disc, centre 12 px below card top |
| Star progress | 340,1312,400,104 |
| Play | 260,1440,560,213; same size when locked |
| Page indicators | Centres at y=1712; existing 54 px spacing and 32 px base size |

The card, star progress and Play form one centred vertical group. Preserve swipe snapping, neighbour
selection, card/Play navigation, locked state, back navigation, localized lettering, and shared music/mute
behavior. The canonical previews `cards`, `cards_late`, `cards_all`, `world_page` and `world_page_late`
remain available through `preview()` with fake profiles. The shared counter retains zero-to-99999 display
and the coin shop keeps its read-only state. No rules, saves, economy or level data change.

User review covers the first, middle and last forest, locked Play, long localized names, zero/maximum
balances, opening a forest and returning, swiping, and the purchase entry. Resolution, runtime appearance,
input, motion, audio, accessibility and performance are not assessed by this source-only delivery.

### Homepage forest composition

The user approved one raised composition: the Bird Sort wordmark and compact teal birdhouse roof
connect through a curved natural branch to an open perch with four live birds below. This fills the
middle without a separate large house, dense flowers, extra signs or empty cards. The backdrop is the approved `background_home_jungle` forest painting. Root Home particles are removed; level ambience remains independent.

Seven Home entries use proportional standalone color icons with alpha contours and no cream
backings. Bottom icons fit 132 px boxes, right activities 120 px, and Settings 96 px. Hit areas
remain 160 × 160, badges 48 px, and activity centers are 220 px apart. Current functionality,
localized Play and the compact balance counter remain. Settings itself and other pages are frozen.

See [Home and aviary](home-and-aviary.md) for geometry and
[the approved title direction](home-title-concept.md) for asset provenance. Independent production
art and existing frame animation are used; the flattened design is never a runtime background.
User runtime acceptance is pending.

## Never
Fonts for visible text, mirrored or flipped sprites for animation, pasted design images in the runtime, felt styling, a different feather counter look, commas in numbers. Every world keeps its own theme (bamboo,
autumn, snow ...) with the same bright cartoon styling.
