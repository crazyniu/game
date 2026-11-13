# Board HUD restyle

The user found the game page HUD mismatched with the pages styled today (home, shop, purchase popups): stars and
moves floating on leaves, a gear lost in the canopy, goal faces scattered in the sky, booster counts in a system font,
and no celebration on the board before the win panel. Not too many cream cards; new art, not patched old pieces;
no corner badges on the tool counts.

- New Codex paintings: `hud_goal_shelf` (wooden plank, leafy ends; only its plain middle extends), `hud_tool_slot`,
  `hud_gear_button`; win title lettering `lettering_win_title` (en, zh) in the shop lettering style.
- Status and day/night plates: the shelf plank in a darker wood; stars `island_star_*`; ButtonKit numerals.
- Goal faces stand on the shelf, which hangs from the house on two ropes; a delivered face dims and gets a check.
- Tools: painted slots, stock in ButtonKit numerals under each slot (no badge, never a font).
- Win: board celebration (house bounce and glow, goal faces hop, confetti from the door, resident cheers, 1.3 s),
  then the shop's purchase card with the hanging sign, three live birds cheering on its branch, green Replay and Home
  keys either side of the green Next (136 / 112 = 1.21).
