# Perch Sort UI system

The binding visual system for every screen. Direction from the user (2026-11-07): cartoon, simple, clean, nothing
fancy. The screens are designed from scratch on this system; earlier layouts and mockups are not a reference.

## Principles

1. One source per kind of art. Scenes are one painting per world. Icons come from one icon sheet, words and numbers
   from one lettering sheet. Shapes (panels, buttons, counters) are drawn by code. Nothing is assembled from
   separately generated pieces that must visually match.
2. Few elements. Every screen shows only what the player needs now. No decorative frames, ribbons, leaves,
   stitching or textures on UI.
3. Same role, same look. A role (primary button, secondary button, counter, panel, card) has one size and one style
   on every screen and in every state. Disabled is dimmed to 45 percent and not interactive, never smaller.

## Canvas and grid

- Design canvas 1080 x 1920 (portrait). Safe margin 48 on all sides. Spacing on an 8 px grid: 8, 16, 24, 32, 48, 64.
- The top bar row is y 48 to 160; the bottom action row is centred on y 1792.

## Colour

| Token | Fill | Bottom edge | Use |
| --- | --- | --- | --- |
| cream | #FFF7E8 | #E9D6B0 | panels, cards, counters, neutral buttons |
| green | #6CC24A | #4C9631 | primary action |
| orange | #FFA53C | #DA7A14 | secondary action, tools |
| blue | #5DB9EE | #3C93C9 | selected tab, information |
| red | #EF5548 | #C23A2F | count badges, close buttons |
| ink | #5A3A1E | | words and numbers on cream |
| white | #FFFFFF | | icons, words and numbers on coloured fills |
| dim | #1E2A1A at 55 percent | | behind popups |

## Shapes (drawn by code, `UI` kit)

- Every shape is a rounded flat fill with a darker bottom edge (cartoon thickness) and one soft shadow
  (offset 0,8; blur 24; 18 percent black). No outlines, no gradients, no texture.
- Panel: radius 56, bottom edge 12. Popup panels are 864 wide (centred, x 108 to 972); their height follows content.
- Card: radius 40, bottom edge 10.
- Counter: full pill, height 96, bottom edge 8; icon 72 at the left inside, number cap height 44 in ink.
- Buttons are circles with a bottom edge of 10 and a white icon at 56 percent of the diameter:
  primary 160 (green), secondary 128 (orange, or cream with an ink icon for neutral actions such as undo, close,
  settings; red for close). Primary is 1.25 times secondary, the maximum allowed. A pressed button sinks by its bottom edge.
- Badge: red circle 56 at the button's upper right (45 degrees on the rim), white number cap height 30.

## Lettering

- One rounded, friendly typeface painted as a lettering sheet (`tools/kit/letters_white.png`), cut by
  `tools/kit/cut_kit.py` into glyphs and glyph outlines (`art/letters`).
- Words are painted label sprites built by `tools/kit/build_words.py` into `art/words/word_<slug>.png`: white
  letters with a warm brown outline (#96521C, 0.11 cap) and a soft shadow, letter gap 0.29 cap.
- Numbers are the same glyphs composed at runtime: on cream and scenes white with the same outline; on coloured
  fills (prices, badges, rewards) plain white.
- Cap heights: title 64, body 44, small 34. Numbers: large 72, medium 52, small 36.
- Close buttons are red with a white X (user choice, 2026-11-07).

## Icons

- One white icon family from `tools/kit/icons_white.png`, cut into `art/icons/icon_<name>.png`.
- Coloured objects (coin, star, booster items, birds, houses, outfits) are illustrations, shown on cream cards or
  circles, never on coloured buttons.

## Screens

- Board: the world painting fills the screen; the hanging house and goal rings at the top centre; top-left counters
  for moves (with a three-star meter) and, in day/night levels, the sun count; settings (neutral secondary) at the
  top right; tools as a row of secondary buttons with stock badges at the bottom, no bar behind them.
- Popups: dim, one panel, a title word at the top inside the panel, one content block, buttons in a row centred
  48 below the panel. Close (neutral secondary) sits on the panel's top-right corner when the popup can be dismissed.
- Map: the painted world map; level nodes are cream circles with the number, the current node larger with its bird,
  locked nodes dimmed with a lock; coins counter top left, world title top centre; shop, aviary and settings as
  secondary buttons at the bottom.
- Shop and aviary: a top bar (close, title word, coins), a row of three tab buttons where needed, and a two-column
  grid of cream cards.

## Motion

- Buttons: press sinks 6 px over 0.06 s and springs back. Popups: dim fades in 0.2 s; the panel scales from 0.85 with a
  back ease over 0.3 s. Counters bump to 1.1 and back when their number changes.
- Birds always breathe and idle; scenes keep their ambient drift. Level entrance stays 3.5 to 4.5 seconds.
