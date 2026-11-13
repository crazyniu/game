# Simple settings

The design images set the style only. Settings is rebuilt as a plain cream card with four identical rows (icon, word,
control) and one row of equal buttons, from the shared simple set in `game/simple.gd`:
- Shapes are drawn (rounded boxes with a darker lip), never stretched pictures: card, divider, switch, two-way
  language choice, green and cream buttons, close button. One green, one cream, one warm brown.
- Words are baked offline from one font per language by `tools/island/bake_labels.py` (Lilita One, Noto Sans SC Bold)
  into `art/labels/` with one line box per font; shown at one cap height per role, centred on their optical middle.
- Icons are fitted by their visible pixels into one 72 px box; the globe icon `art/pic_globe.png` was painted by local
  Codex in the style of the other row icons.
- Changing the language swaps the words in place; the popup is not closed and reopened (`main.gd`), so nothing flashes.
- Restart and Home appear only while playing; all buttons are 340 x 116.
