# UI redesign brief: paste-ready mockups for user approval

PROCESS (user decision, 2026-11-05): every screen is designed first as a finished 1080x1920 mockup image. The user signs off each image. Only after sign-off may anyone write UI code or put UI on the user's Mac. Text is part of the picture: names, numbers, labels are drawn INTO the image (no live text boxes in the design).

## Style (unchanged): the approved plush forest look
Reference images (style only; some details are superseded below): art/approved_ui/ui_level_k3.png, ui_level_k7.png, ui_win.png, ui_level_card.png, ui_popup_new_friend.png, ui_goal_info.png, ui_unlock_card.png, ui_settings.png, ui_shop_boosters.png, ui_shop_confirm.png, ui_shop_outfits.png, ui_aviary_list.png, ui_aviary_detail.png, ui_map.png. Stylised 3D plush birds without outlines, pale hazy forest backgrounds, honey-wood panels with green fabric ribbons, leaf sprigs, gold rims.

## Decisions that SUPERSEDE the old approved images (every mockup must follow them)
1. ONE goal house: a single rope-hung house at top centre (about 240x240 body) with ONE rope, plus a row of small round progress badges under it (one per species of the level, species front-facing face in a gold ring, dimmed until its flock is home, then lit with a green check). No row of 2 to 7 houses, no beam. A bird peeks from the doorway of the single house after a delivery.
2. Secondary round buttons are honey-WOOD discs with a gold rim (not purple). Glyphs are white with a dark-brown outline (#5A2F0E), clearly readable: glyph about 55 percent of the disc diameter, all glyphs on all discs the same visual size. Disc size 108 px for tools and settings, 100 px in panels. Coloured tool illustrations on the HUD tool discs: hint bulb, branch with plus, owl eye with egg, bell, scarecrow crow, rain cloud.
3. Primary actions are LONG GREEN BARS (rounded rectangle) that sit fully inside the panel's cream area with at least 28 px margin to the wood frame on both sides and at the bottom; icon (check, arrow, play) about 64 px high, white with dark-green outline, centred. Never a big round green disc. Maximum one primary bar per panel.
4. Birds that face the viewer (portraits, popups, houses, aviary, goal info, level card) look straight at the player, head level, both eyes looking at the camera. Birds on branches in the board mockups also face the viewer.
5. Board trees: SLIM trunks hugging the left and right screen edges (visible width about 120 px), with vines and a leafy crown at the top; every branch is ONE thick tapered limb growing out of the trunk with leaf sprigs (snow caps and icicles in the snow world); the first bird seat starts clear of the trunk; no stray sticks, no stubs, no floating birds. Compare to ui_level_k3.png and ui_level_k7.png.
6. A small pill under the coin pill: footprint icon + the counted move number + three small stars (stars dim as the 3-star and 2-star move targets are exceeded). Free-move: green flash and a feather.
7. Undo is not drawn when there is nothing to undo (level start shows only the other tools). Tool buttons are identical in size.
8. Every modal: dim background (about 45 percent black) plus a soft drop shadow under the panel so it clearly separates from the scene behind.
9. All text baked into the images and fully inside its ribbon, pill or badge with margin; names are inside ribbons; numbers inside badges.
10. Icons are LARGE and legible at phone size (nothing smaller than 56 px on screen; portraits in rings fill the ring with the bird's face).

## Screens to design (each one file, 1080x1920 PNG, in-context: popups drawn over the dimmed scene)
Board group: board_few_branches (3 tools, 2 species, level 1 look), board_many_branches (4 tools, 7 species, snow world, level 40 look), board_win_panel, board_sleeping_bubble (a sleeping bird tapped, icon bubble), board_stuck_hint.
Popup group: level_card (levels with 2 species and with 7 species), new_friend, goal_info, unlock_card, settings (in a level), daily_gift, world_complete, world_unlock, shop_confirm.
Page group: map (bottom bar of 3 wood discs, current node with chick, vine-arch gate), shop_boosters, shop_outfits, shop_houses, aviary_list, aviary_detail (living bird with outfit slots, gift row, count row), 
Names are plain (no numbers). Each worker also delivers one contact-sheet JPEG of its group and a short list "design decisions and open questions".

## Quality bar ("paste-ready")
- Exact positions: no overlapping elements, margins at least 24 px, nothing cut by a frame or the screen.
- Text legible at 1080x1920 shown on a phone; consistent type (rounded bold, same as the game font Fredoka).
- Same visual language across all screens (frames, ribbons, buttons, badges, shadows).
- After sign-off an implementer must be able to measure every rectangle from the image.
