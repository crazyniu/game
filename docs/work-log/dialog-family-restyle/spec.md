# Dialog family restyle

Self-review of every screen after the home, shop and board restyle (user: find and fix what does not match).
The game dialogs (new bird, goal info, level card, booster unlock, world complete and unlock, lessons, guide) still
wore the old orange frame, green field, gold-and-brown titles and the old pill buttons; Lucky Spin and Star Chest had
old titles, old pills and an orange-and-blue board; several numbers used older glyph sets.

Contract (the shop's style, ButtonKit):
- Home jungle backdrop; the shop's cream card (`panel_card_cream`); one card per dialog.
- Title: the hanging wooden sign with lettering painted in the shop style (yellow, dark-green outline), 640 x 223 on
  the card's top edge; Close inside the card's top-right corner, never over the sign.
- Actions inside the card: green plate, painted white word; Primary 360 x 136, Secondary = Primary / 1.25.
- Captions in warm brown ink (painted captions) or the shop's deep green captions; numerals `digit_ui_*` everywhere.
- Lucky Spin, Star Chest and their results: page title sign at the map's position, cream card with painted slots for
  the prizes, every action the same Primary plate.
- New lettering (Codex, en and zh): Guide, Level, New Bird, Cleared!, Unlocked, eight lesson titles, Lucky Spin,
  Star Chest.
