# Daily draw UI

The user approved this material repair on 2026-11-11: the check-in entry keeps the existing lucky draw,
while its presentation stops repeating large cream cards. This supersedes the earlier all-cream
shop-card treatment. Runtime approval is pending.

## Material and composition

Keep the bright tropical homepage visible through the existing modal dim layer. The draw is one
crafted honey-wood board with gold bevels, brass pins, integrated dividers and nine turquoise timber
recesses. Eight outer recesses hold prizes; the centre holds the open chest. The original production
sprite `PerchSort/art/island/panel_draw_board.png` was generated with Codex's built-in image tool;
it contains no words, prizes or characters and is not extracted from a design image.

Use `Modal.open` for layout, dim, close behavior and animation, remove this page's generic panel
instance, and keep the compact `ShopSkin.plaque` title. No outer cream wrapper and no strip behind
the remaining-draw stars. Existing painted title, stars, item pictures, quantities and green action
retain the dimensional island family. No extra instructions or promotional copy.

All dimensions use the existing 1080 x 1920 design canvas. Vertical content origins come from the
modal's `ys`; action position comes from `actions_y`.

| Element | Box or rule |
| --- | --- |
| Layout footprint | Modal's 960-wide bounds; no visible generic panel |
| Title | Existing wood plaque, 520 x 132, x 280, top at modal top minus 56 |
| Remaining draws | 92-high open row; 72-square stars with 24 gaps, centred |
| Complete board | 840 x 840, x 120, second content origin |
| Recess placement | Board-local origin (84, 90); column/row stride (232, 222) |
| Prize wells | 208 x 200; fixed equal size for every prize |
| Prize art | 120 x 120 at local (44, 12), fitted by visible ink |
| Prize quantity | 180 x 44 at local (14, 144); white painted glyphs |
| Centre chest | 176 x 166 at local (16, 17), with warm glow |
| Gold cursor | Inset luminous outline, transparent fill, travels around the eight wells |
| Draw action | Existing 480 x 136 green action, centred below the board |

The recess positions follow the generated board's visible openings rather than its original prompt's
nominal proportions. Render the entire square production sprite without cropping or stretching its
aspect ratio. Keep prize art and counts above the highlight so both remain readable.

## Result popup

The draw's result displays each distinct award once, without any per-item card or outer cream sheet.
A fitted coin or booster floats over a warm rotating ray sprite, with its full white painted
`+quantity` immediately below. Coins are represented by the actual coin picture. Existing aggregation
combines repeated boosters before presentation.

One reward uses a 440 x 480 open composition; multiple rewards use equal 260 x 360 slots, up to three
per centred row, with 24 gaps. The picture fills the upper area; a 72-high quantity sits near its foot.
The result uses the same wood title and 480 x 136 green OK action. The close tile and OK share the
existing exactly-once dismissal behavior. No new player-visible words or locale entries.

After dismissal, restore the draw board, clear its cursor, and enable the draw action only when draws
remain. A revealed random booster returns to the mystery chest. The result never grants another
reward or restarts music. Nested over a draw, it reuses the existing dim backdrop.

## Interaction and review states

Preserve `lucky_draw(host, spins, slots, draw)`, current prize order/probabilities, remaining-draw
count, reward callback, sound, modal layering and close behavior. Disable draw and close while the
cursor travels. Bind the sequence to the modal's tween lifetime. Economy, streak rules, eligibility,
saves, audio ownership and all unrelated popup pages remain unchanged.

Keep `preview(main, state)` with the homepage underneath and current fake states: `lucky_draw`,
`lucky_draw_spin`, `lucky_draw_coins`, `lucky_draw_none`, `lucky_draw_full`,
`lucky_draw_reward_coins`, and `lucky_draw_reward_boosters` (the grouped duplicate-booster case).

The user owns testing and acceptance. No tests, lint, naming checks, builds, application launches,
screenshots, timed captures or UI automation are performed for this change. Runtime appearance,
rolling/reveal timing, input, audio, both locales and device fit are not assessed.

## Production asset prompt

Built-in image generation, transparent background, one 1254 x 1254 PNG. The original generated
source remains in Codex's generated-images directory; the runtime asset is copied into this project.

> Use case: stylized-concept. Asset type: production PNG UI sprite for a polished portrait tropical-island casual mobile game, not a mockup or screenshot. Generate ONE square carved wooden lucky-draw board with a complete 3 by 3 grid of nine empty, equal square recessed compartments, all part of a single physical board. Straight-on orthographic front view, perfectly square, no perspective. The board fills the square image with only a narrow transparent safety margin. Rounded outer honey-orange wooden frame with rich painted grain, small brass corner rivets, beveled golden edges, a subtle bottom thickness and ambient occlusion. Use two connected vertical and two connected horizontal narrow wooden dividers. Geometry relative to visible board bounds: outer border 5% each side, each compartment 28.57% of width and height, divider width 2.14%; all nine compartments precisely equal and centered on a regular 3x3 grid. Recesses have warm muted turquoise painted timber floors, shaded slightly at the top edges; these are shallow carved wells with integrated wood rims, NOT floating cream cards. All compartments completely EMPTY including the center: no chest, no icons, no prizes, no numbers, no stars, no lettering. Vivid warm tropical palette, charming dimensional hand-painted game UI, soft upper-left light and clean readable details, not photorealistic, not flat vector, no surrounding scene. Four tiny engraved leaf motifs can fit on corner wood only. Transparent exterior background with clean alpha, no checkerboard, no white background, no extra pieces or separate assets. Keep openings regular and unoccupied to receive game icons and live highlight overlays.
