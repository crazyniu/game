# UI component unification (direction mockups)

User review 2026-11-05: the direction mockups in `art/ui_design/` use a different primary button, close button, icon style, toolbar and HUD on almost every screen, and primary buttons sit inside the panels. The user wants every action button OUTSIDE the panel and one consistent component set. This spec is the single rule set for redrawing the 18 unapproved mockups and for the component sheet. Approved mockups stay untouched: `board_few_branches`, `board_many_branches`, `board_sleeping_bubble`, `tree_worlds`.

All numbers are pixels on the 1080 x 1920 portrait canvas. Style stays the approved art direction: natural painted forest, plush furry birds, honey-wood and green fabric UI, not plastic toy. Reference for style: the four approved mockups above.

## 0. Lettering is painted art, never typeset (user, 2026-11-06, overrides any earlier wording)
- It is FORBIDDEN to render any visible word or number with a font: no PIL ImageDraw.text, no ImageFont, no SVG or HTML text, no tools/bake_labels.py, no font file of any kind, not even for a single digit. (An earlier instruction of the coordinator allowed PIL fonts; it is withdrawn. Typeset lettering looks flat and web-like and the user rejected it.)
- All lettering is painted artwork produced by the image generation tool, in the style of the approved mockup art/ui_design/board_few_branches.png (the "Level 1" ribbon is the reference): thick rounded hand-painted letters, cream-white fill with dark-green outline on green ribbons, soft emboss and drop shadow, slightly organic edges.
  - Titles, name labels and captions (Level 40, World Complete, Daily Gift, Autumn Forest, card names, Best 28 and so on) are generated TOGETHER with their ribbon or plate as one painted image with the exact text, then composited into the screen.
  - Numbers and symbols inside pills and cards (digits 0-9, the infinity symbol, plus, times) come from one shared painted glyph set in art/ui_design/glyphs/ (see ui-glyph-set): dark-brown painted numerals for cream pills, cream-white outlined numerals for wood discs and ribbons. Compose a number by placing the glyph PNGs side by side (this is allowed: it is placing art, not typesetting). A pill with a coin or footprint uses a painted coin or footprint image.
  - The third star threshold is the infinity glyph, never letters.
- PIL stays allowed for geometry only: cutting, scaling, positioning, masking, dimming, compositing, measuring.
- Verification: when you finish, zoom to 300 % on at least three text areas per image; the letters must show brush-like variation and emboss, not a uniform vector outline. Report any text you could not paint.

## 0b. No collisions between a modal and the page behind it
- The modal panel and its ribbon must never overlap or touch another ribbon, title or pill of the page behind. If the page behind has a title ribbon at the top (map: world title at y 130), place the panel so its own title ribbon is at least 60 px below the bottom of that ribbon, or move the page title out of the dimmed layer's visible area by keeping the page layout as is and shifting the panel down. Level card: panel vertical centre about y 960.

## 1. Modal skeleton (every popup)
- Dim layer (user, 2026-11-05: "when a popup shows, put a mask over everything else"): EVERYTHING that is not the modal is under the dim layer: background, branches, birds, the whole board HUD (coin pill, move pill, settings gear, goal house, goal portraits) and the bottom tool bar or navigation bar. Only the modal panel, its title ribbon, its close X and its action row are above the dim. Behind-the-modal buttons must read clearly as inactive: their brightness drops to at most 35 % of normal and their colours desaturate by 40 %.
- Build every modal image in layers, never in one generation: (1) the full unmodified background screen, (2) multiply that whole layer by 0.35 and desaturate it 40 % with PIL, (3) composite the modal panel, ribbon, close X and action row on top at full brightness. The same dim factor on every modal.
- Panel: cream card with a honey-wood frame 24 thick, corner radius 56, width 860 (x 110 to 970), horizontally centred. Height follows the content. Vertical centre about y 860.
- Decoration: exactly two small leaf clusters, one at each end of the title ribbon. No other random leaves on panel corners.
- Title ribbon: green fabric ribbon 640 x 120, horizontally centred, its vertical middle sitting on the panel's top edge (half above, half over the frame). Text white, bold rounded, dark-green outline, cap height about 52. Text is part of the image (baked).
- Close X: honey-wood disc 100 with white X (dark-brown outline). Its centre is 24 inside the panel's top-right corner on both axes (x = panel right - 24, y = panel top + 24); it overlaps the frame corner, never the ribbon. Only on modals that can be dismissed without a decision: goal_info, level_card, settings, shop_confirm. Reward and result modals (daily_gift, new_friend, unlock_card, board_win_panel, board_win_panel_two_stars, world_complete, world_unlock) have NO X; their action row is the only way out.

## 2. Action row (buttons outside the panel)
- All action buttons of a modal sit in ONE horizontal row BELOW the panel, on the dim layer. Nothing actionable remains inside the panel except the close X and content cards (shop items, outfits).
- Row position: the top of the tallest button is 48 below the panel's bottom outer edge. Buttons share one horizontal centre line. The row is centred on x 540.
- Primary button: round green disc, diameter 124, gold rim 8, face leaf green #4DB21F to #8BD13A with a soft top highlight. Glyph white with dark-green outline, visual size 58, optically centred (a play triangle is shifted right by 4 so it looks centred).
- Secondary button: honey-wood disc, diameter 100, gold rim, white glyph with dark-brown (#5A2F0E) outline, visual size 46, centred.
- 124 / 100 = 1.24, which keeps the primary at most 1.25 times its siblings (hard project rule).
- Gap between neighbouring disc edges: 48. Order: secondary, primary in the middle, secondary. A modal with only a primary shows one green disc centred on x 540.
- Primary glyph has exactly two meanings:
  - Play triangle: the action enters a level (level_card, world_unlock, board_win_panel and board_win_panel_two_stars "next level").
  - Check mark: accept, claim or confirm (daily_gift, new_friend, unlock_card, shop_confirm, world_complete).
  - No arrows anywhere. No long green bars anywhere.
- Per modal:
  - board_win_panel, board_win_panel_two_stars: [retry] [play] [home].
  - level_card: [play] alone.
  - world_complete: [check] alone. world_unlock: [play] alone.
  - daily_gift, new_friend, unlock_card: [check] alone.
  - shop_confirm: [check] alone (close X cancels).
  - goal_info: no action row (close X only).
  - settings: the three settings controls (sound, restart, home) stay inside the panel as one row of 100 wood discs because they are the panel's content; no primary.

## 3. Glyphs and centring
- Control glyphs (retry, home, settings gear, close X, play, check, sound, shop tabs, undo) are all one style: white fill, dark-brown outline 6, slight soft bevel, 46 % of the disc diameter, centred within +-2 of the disc centre.
- Tool and booster pictures (bulb, branch with plus, egg with eye, bell, crow, rain cloud) are coloured illustrations at 54 % of the disc diameter, fully inside the disc's inner face, never touching or crossing the gold rim, centred within +-2.
- Pills that carry a coin or icon: the coin disc overlaps the left end; the number is centred in the remaining cream area. Every pill and number row is centred on its panel or card.
- Minimum text cap height 28 anywhere; numbers under stars at least 28.

## 4. Board HUD (every screen that shows the board behind)
- Same as the approved board mockups: coin pill top-left; move pill under it (footprint + moves, divider, three stars with thresholds below in the order 12 16 inf, i.e. three-star threshold first, digits at least 28 cap height); settings gear disc 108 top-right; hanging goal house top centre; three goal portraits 96 under it.
- Goal portraits carry at most ONE badge, 40, bottom-right: before delivery the species' gift or ability badge; after delivery a green check replaces it. Never two badges.
- Bottom tool bar: exactly three tool discs 108 at x 300, 540, 780, centre y 1800: undo (white glyph), hint bulb, extra branch with plus. Same set and size on every board screen. No egg-eye or owl tool in the bar.

## 5. Map pages
- Coin pill top-left; world title ribbon top centre (x 540, y 130); no gear at the top.
- Bottom navigation: one honey-wood bar 960 x 150 at the bottom with three 108 discs at x 300, 540, 780: shop basket, aviary house, settings gear. The level_card background shows this same bar.

## 6. Shop pages
- Top row: coin pill top-left at the same place as on the map. Close X disc 108 top-right (centre x 980, y 130), separated from the tabs.
- Tabs: a second row of three wood discs 108 centred at x 360, 540, 720, centre y 270: boosters (bulb), outfits (hanger), houses (house). Selected tab: gold glow ring plus a green pill 70 x 28 under it; unselected tabs are the same size.
- Item cards: cream cards with wood frame, two columns, identical card size within a page, every card with a name ribbon (houses and outfits alike).
- Price: a cream pill with a gold coin overlapping its left end and a dark-brown number. Green is NOT used for prices (green is reserved for primary actions).
- Owned count: red disc 56 with a white number at the card's top-right; hidden when the count is 0.
- Outfits: equipped = gold glow ring on the card plus a green check badge 56 at the card's top-right, no price pill; owned but not equipped = no price pill, small wood "wear" disc with a hanger glyph; not owned = price pill.

## 7. Board stuck hint
- No speech bubble that repeats the tool icons. The recommended tool disc in the bottom bar gets a pulsing gold glow ring and a small cream pointer arrow bouncing above it. Nothing else changes.

## 8. Win panel content
- Hero bird above the title ribbon, three stars, the threshold row (12 16 inf), the move count pill, one reward row: coin +N only. No extra corner bubbles (+10, +1) and no chain icon.
