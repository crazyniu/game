# Task shop-aviary: understandable shop, one clear place for outfits

Problems the user reported:
a) In the shop the boosters have no explanation of what they do.
b) The shop's outfits tab shows four unrelated birds each with a check or a hanger: nobody knows what it means or where
   to change a bird's outfit. The user's mental model: the game shows many kinds of birds; tap a kind and on its page you
   change its outfits.
c) The three tab icons (bulb, hanger, house) do not tell what the tabs are.
d) The user could not find outfit buying in the aviary because the aviary did not load (fixed in main already; verify).
Do:
1. Boosters: every booster card and its buy popup shows what it does in one short painted sentence under the name
   (Firefly: shows a good move; Sapling: adds one branch; Owl Eye: shows what is in the eggs; Morning Bell: wakes
   sleeping birds; Scarecrow: sends the crows away; Spring Rain: lets branches grow). The strings already exist as
   captions in the catalog or can be added to `en.json`.
2. Outfits: the shop's outfits tab becomes a grid of bird kinds (one card per kind the player has met: the bird
   wearing its current outfit, its name, and how many of its outfits are owned like "2/4"). Tapping a card opens that
   bird's outfit page: reuse the aviary's detail page (equip, buy, preview). Closing returns to the shop. Unmet birds
   are silhouettes with "?" and do nothing. In the aviary, the detail page keeps working as is.
3. Tab icons: the coordinator delivers full-colour icons `PerchSort/art/tabs/tab_boosters.png`, `tab_outfits.png`,
   `tab_houses.png` (512 px, transparent). Use them as the tab pictures (selected = blue circle behind, others cream);
   if a file is missing on your branch, wire by name and continue.
4. Check every tap target in both screens does something sensible (no dead buttons) and list them in the report.
Evidence: captures of boosters, outfits grid, a bird's outfit page opened from the shop, the buy popup, the tabs; a
scripted run that taps through shop -> outfits -> bird -> buy and equips, with coin and equip results logged.
