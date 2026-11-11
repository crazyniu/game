# Island UI style

Recorded 2026-11-10 from the user's finished island homepage and their page designs (Shop, Aviary, Aviary detail,
Lucky Spin, Settings, Star Chest). The designs are a style source, not a layout to copy: each screen is designed
fresh in this style. The felt/plush UI and the forest look are retired; the birds keep their current fleece look.

## Setting and mood
Bright tropical island, a sunny holiday game. Full-bleed painted scene behind every page (turquoise sea, white
surf, sand, palms, a waterfall cliff and a thatched pier hut far away, red hibiscus and big green leaves
framing the corners and bottom). Popups sit over a darker, blurred dusk version of the same island. Pages feel
open and airy: content floats over the scene on its own cards; there is no big backing sheet.

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
| Text on cream | chocolate brown #5A2F0E |

## Materials and shapes
Glossy cartoon (green surfaces stay flat), thick rounded shapes, one soft highlight along the top edge, gentle drop shadows, cream rims on
buttons and pills, no hard plastic edges, no felt or stitching. Generous corner radii everywhere. Every kind of
piece has one size and one look in every state (disabled is dimmed, never smaller).

## Lettering (all words are painted images, never a font)
- Gold: puffy glossy golden-yellow with a thick dark-brown outline. Titles on signs and item names.
- Brown: puffy chocolate lettering with a thin cream outline. Tabs, row labels.
- Caption: friendly rounded dark-brown, medium weight, no outline. Short descriptions.
- White: puffy cream-white with a thick dark-green outline. Words on green buttons and the selected tab.
- Numbers use the homepage painted digits (cream with a soft brown drop shadow), plain, no separators.
- Names are natural game names for each language, never literal translations.

## Components
- Feather counter: gold feather coin on the left, dark plum pill with cream rim, painted digits, green plus
  (art only). Same bar on every page; max 99999. A price is the feather coin on a gold bar with dark digits; a
  reward ("+30") uses the same gold bar.
- Badges: round, one diameter. Red = reward or new, green = owned count, check = equipped or done, padlock =
  locked. Numbers never exceed 99.
- Tiles: cream rounded-square close (orange X) and back (orange arrow) tiles.
- Primary button: long green pill, one calm fill, one soft shine, white painted word. Secondary: cream pill.
- Cards and rows: cream with a soft rim; the selected one has a bright green outline; icons may overhang the
  top edge a little so they feel alive.
- Titles hang on a carved wooden plank from two twisted ropes with vines, hibiscus and plumeria at the ends.
- Popups: orange wooden frame with a cream inner panel (960 wide), hibiscus and leaves at the corners, a shell at
  the bottom centre, a large rope sign (760 wide) with the gold title straddling the top edge and a cream close tile
  at the top right. Actions sit inside the frame at the bottom: green primary pill 480 x 150, cream secondary pills
  384 x 120 on the row under it. Hero pictures (bird, booster, chest) stand in a soft warm glow; abilities and
  descriptions sit on cream row cards.
- Settings: one cream row card per switch (picture, brown word, island toggle), then the language heading and one
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

## Never
Fonts for visible text, mirrored or flipped sprites for animation, pasted design images in the runtime, forest or
felt styling, a different feather counter look, commas in numbers. Every world keeps its own theme (bamboo,
autumn, snow ...) but is painted as an island of the same bright tropical style.
