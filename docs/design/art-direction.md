# Art direction

Status: APPROVED by the user 2026-11-05 (with the correction below). The user approved art/ui_design board_few_branches, board_many_branches, tree_worlds and board_sleeping_bubble as the direction to implement. Written from the user-approved plush forest style (docs/UI_SPEC.md section 4.1, art/approved_ui) and the user's decisions of 2026-11-05. Items marked [Decision needed] are where the evidence does not settle the answer.

## 1. Visual identity
Rule: **a natural, softly painted storybook forest (NOT a plastic toy look) inhabited by plush, fluffy birds; everything the player touches is honey wood or green fabric.** (User correction 2026-11-05: the forest must not look like plastic toys; the birds are plush with visible soft fur.)
- Forest and trees: painterly and natural (real bark texture, moss, layered foliage, atmospheric haze), never glossy or plastic.
- Birds: plush toy material, soft visible fur, matte, no plastic shine.
- When art and readability conflict, readability of the puzzle (which bird, which branch) wins.
- Painted things (trees, backgrounds) are ONE continuous painting per scene; UI is made of a small fixed kit (wood, cream, green ribbon, gold rim). Never mix kits inside one component.

## 2. Mood per game state
| State | Emotion | Light | Adjectives | Energy | Carrier element |
|---|---|---|---|---|---|
| Map | curious, cosy | warm daylight, hazy | winding, sunny, inviting | calm | the glowing current node with the chick |
| Level (w1 green) | focused, light | bright blue sky, soft haze | airy, fresh | measured | the tree and the birds on it |
| Level (w2 autumn) | warm | golden afternoon | cosy, rustling | measured | falling leaves |
| Level (w3 misty forest) | quiet | cool green mist | still, deep | calm | light shafts |
| Level (w4 snow) | magical | violet aurora dusk | crisp, sparkling | measured | aurora and icicles |
| Win | joyful | gold rays | triumphant, bouncy | lively | rays, confetti, cheering birds |
| New friend | delight | warm spotlight | surprise, gift | lively | the bird flying in |
| Shop | tidy, rewarding | clear daylight | neat, friendly | calm | wooden cards, coins |
| Aviary | proud | soft daylight | collectible, gentle | calm | the living bird portrait |
| Popups/Settings | focused | scene dimmed (black, alpha 0.72) | clear | calm | the lifted panel |

## 3. Shape language
- Birds: round bodies, big readable eyes, one distinguishing trait per species (curl, bun, crest, leaf, ears, plume, flame, cap/hood, brow); readable at 64 px.
- Environment: organic, natural, painted; thick tapered limbs growing out of the trunk, no straight sticks, no glossy plastic surfaces.
- UI grammar: rounded rectangles (panels, cards, bars) and circles (discs); everything has a gold or dark-wood rim; generous corner radii.
- Hero shapes: birds and the goal house. Supporting: trees, UI. Receding: background haze.

## 4. Color system
| Colour | Role |
|---|---|
| Honey wood #D98A3D to #F2B35E (rim #FFD46A) | panels, discs, nodes, anything interactive |
| Cream #FBE9C6 | panel interior, pills |
| Leaf green #4DB21F to #8BD13A | primary action, ribbons, success |
| Dark brown #5A2F0E | text and glyph outlines |
| Gold #FFC83D | coins, stars, rims |
| Red #E02828 | badges only (counts), error shake |
| Sky/world palettes | per world, from the painted backgrounds |
Semantic: primary action = green bar; secondary = wood disc; reward = gold; count = red badge; disabled = same shape at 45 percent alpha; locked = grey stone card + padlock. No meaning carried by colour alone (stars have shape and number, locked has padlock, night has moon icon).
[Decision needed] none; the earlier purple discs are retired by the user's decision (clash with wood).

## 5. Character direction
- Ten plush species, no outlines, soft fur, same face construction. Told apart by silhouette trait and colour.
- Birds that face the player (portraits, popups, goal house, aviary) look straight at the viewer, head level, eyes on the camera. Birds on branches also face the viewer at rest.
- Poses are drawn frame sequences (blink, cheer, flap, hop, peek, flight); never a mirrored or shaken still. Detail must survive at 170 px on screen.

## 6. Environment direction
- Four worlds, one painting per world and layout; stylised 3D painted look with soft depth haze behind.
- Each tree is ONE continuous painting: trunk, crown and branches in one pass; branch roots grow out of the bark (continuous texture, vines and leaves across joints). Branch heights are fixed by the game (see the tree addendum in docs/work-log/ui-redesign/brief.md).
- Prop density: crowns top and corners, sparse mid-screen so birds read; snow world adds snow caps and icicles; autumn adds falling leaves.

## 7. UI visual direction
- Screen-space UI over the world. Typography: Fredoka Medium, rounded, white text with dark-brown outline on dark or green; dark-brown text without outline on cream. Fixed text (names, labels) is BAKED into its ribbon or badge image; only numbers that change are live text, placed from measured inner rects.
- Icons: white glyph, dark-brown outline, same visual size on every disc (about 46 percent of disc diameter; coloured tool illustrations 54 percent). Nothing under 56 px on screen.
- Button families: (a) primary: long green bar, inside the cream area with 28 px margins, icon 64 px; (b) secondary: honey-wood disc 108 px (100 px in panels); (c) tab: wood disc with gold selection ring and green pill; (d) close: wood disc with X at the panel corner.
- Panels: wood frame with cream interior, optional green ribbon; every modal has a drop shadow under the panel and the 0.72 black dim behind.
- Check: readability OK; target size 100+ px OK. Accessibility target NOT ASSESSED (no project target); assumption: minimum touch target 88 px.

## 8. Motion and sound direction
- Panels open in 0.18 s (scale 0.92 to 1.0, fade), close in 0.12 s; no per-control entrance animation inside a panel.
- Buttons: press scale 0.92 in 0.08 s, release 0.18 s ease-out-back. Counters count up over 0.5 s.
- Birds: continuous breathing (period 2.4 to 3.2 s), idle actions every 1 to 3 s from drawn frames; hard frame cuts at the sequence frame rate.
- Level entrance 3.5 to 4.5 s: house, branches, birds fly in from outside the screen, tap skips.
- Sound: warm acoustic, short; one distinct sound per booster and per UI action; menu music on map, game music in levels, 1 s crossfade, one mute control (no sliders).

## 9. Asset standards
- Design canvas 1080x1920 portrait; target devices phones 9:16 to 9:20; safe-area aware.
- Sprites: PNG RGBA, alpha cleaned (no pixels below alpha 13), pivots as specified, 64 px readable, no halos; derived files only, originals never overwritten.
- Names by meaning (roles prefixes in docs/CONVENTIONS.md); no version or task numbers.
- Frame animation: 10 to 14 fps for birds, numbered frames NN from 01.
- Text-bearing art (name ribbons, level badges) is generated by tools/bake_labels.py; layout areas measured by tools/layout_metrics.py.

## 10. Reference direction
- art/approved_ui (the user-approved full screens): take style, wood and ribbon language, bird look; avoid the old purple discs, multiple houses, thin stick branches.
- Plush-toy mobile casual puzzles: take clear board readability and juicy feedback; avoid busy backgrounds behind birds.
