# New forests: art brief (Codex)

Six new forest worlds follow the four existing ones (Green, Bamboo, Autumn, Snow). Every picture is painted with
your image generation tool in the SAME cozy, soft, bright cartoon style as the existing world art; match light,
saturation, brush detail and level of clutter of the references named below. No text, no numbers, no birds, no
characters, no UI in any picture.

| World | Name | Theme |
| --- | --- | --- |
| 5 | Blossom Forest | spring cherry-blossom trees in soft pink, drifting petals, fresh light-green grass, pale blue sky |
| 6 | Mushroom Forest | mossy oaks and deep green ferns with giant red and orange spotted toadstools, warm daylight |
| 7 | Lakeside Forest | willows and white birches around a calm blue lake, water lilies, reeds, gentle reflections |
| 8 | Misty Pine Forest | tall blue-green pines, soft morning mist in layers, distant mountains, cool teal palette |
| 9 | Rainforest | lush tropical jungle, huge glossy leaves, hanging vines, orchids, a small waterfall, warm humid light |
| 10 | Wisteria Forest | old trees hung with long purple and lilac wisteria cascades, soft lavender light, small lanterns of light |

Path layout: world N reuses the trail layout of an existing map so the level buttons fit: world 5 and 9 follow
`PerchSort/art/map_world_w1.png`, world 6 and 10 follow `map_world_w2.png`, world 7 follows `map_world_w3.png`,
world 8 follows `map_world_w4.png`.

## Pictures per world N (5..10)

1. `tools/scene/board_wN_raw.png`, 1024x1536 RGB. The level background with branches. Composition guide:
   `tools/scene/guide_wN.png` (the white 9:16 area is the screen; brown bars are where branches go: six on the left
   growing in from the left screen edge, six on the right growing in from the right edge, each reaching just
   short of the centre line; red boxes are UI, keep them calm and uncluttered). Style references:
   `tools/scene/board_w4_raw.png` and `tools/scene/board_w1_raw.png`. Branches are thin horizontal wooden
   branches that match the theme (blossom twigs with a few pink flowers at the tip, mossy branches, birch, pine
   with needles, vine-wrapped jungle branches, wisteria wood), a small leafy tip at the inner end, exactly at the
   guide bars' heights. Background: the theme forest, soft and slightly blurred behind, brighter at the top, the
   middle calm so birds stay readable.
2. `tools/scene/board_wN_empty.png`, 1024x1536 RGB: exactly the same painting as (1) with every branch painted
   out (edit (1); nothing else may move or change).
3. `tools/scene/night/board_wN_night.png`, 1024x1536 RGB: the empty painting (2) at night, moonlit blue, the same
   composition; compare `tools/scene/night/board_w4_night.png` vs `board_w4_empty.png` for how night looks.
4. `tools/map/map_world_wN_raw.png`, 1024x1536 RGB: the level-map painting. Keep the cream footpath EXACTLY where
   it is in the layout map named above (same switchbacks, same width, enters bottom centre, leaves top centre);
   repaint only the scenery in the new theme. Top and bottom 80 px: plain grass and the path only, so maps stack.
5. Gate into world N, transparent RGBA, same size, framing and outline as the world 3 set in `PerchSort/world/`
   (`gate_arch_frame_w3.png` 560x600, `gate_arch_closed_w3.png` 560x600, `gate_door_left_w3.png` and
   `gate_door_right_w3.png` 160x288, `gate_light_w3.png` 320x440). Save them as
   `PerchSort/world/gate_arch_frame_wM.png` etc. with M = N - 1 (the gate leading out of world M). Decorate the
   wooden arch in world N's theme (blossoms, mushrooms, reeds, pine needles, jungle vines, wisteria).
6. `PerchSort/art/gate_backdrop_wN.png`, 1024x1024 RGBA, like `PerchSort/art/gate_backdrop_w4.png`: the view
   through the open gate into world N.

Transparency: pictures 5 and 6 must have a real alpha channel with clean edges. If the image tool cannot paint
transparency, paint on flat pure magenta #FF00FF and remove it with a Python (Pillow/numpy) script, then check
there is no magenta fringe.

Check every picture yourself (size, mode, path position, branch heights against the guide) before finishing.
Do not edit any other file. Write a short `docs/work-log/new-forests/art-wN.md` listing what you made and anything
you are unsure about.
