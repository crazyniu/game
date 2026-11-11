# Shop material repair

Implement the approved page-materials repair directly in `PerchSort/game/shop_view.gd`.
The current working tree is the starting point; preserve existing uncommitted work.
Owned documentation: `docs/design/shop.md` and this work-log directory. Shared helpers,
locale manifests, Aviary detail, rules, prices, profile data and audio are read-only.

- Replace six tall cream booster backdrops with two continuous wooden shelves, three
  items each. Keep fixed stock badges, compact numeric price actions and all six on screen.
- Place outfit species on existing clean perches and nests on low display stands.
  Use smaller painted names and consistent 24-pixel gaps and action geometry.
- Keep three equally sized icon-only bottom tabs with a clear selected state.
- Restore a single purposeful wooden frame for purchase confirmations, with a supported
  item, necessary booster caption, quantity entry and numeric total. No nested cards.
- Preserve cached pages, one-bird-per-frame preparation, preview states, purchase/equip
  callbacks, prices and availability. Add no gameplay or save changes.

Reuse the clean bird, branch, frame, label and digit art. Generate only missing original
transparent shelf/stand art with the built-in image generator; retain existing assets.
New files: `PerchSort/art/island/prop_shop_shelf.png` and
`PerchSort/art/island/prop_shop_nest_stand.png`. Their production prompts and placement
belong to the owned design document. No shared-file lock is needed for these owned files.

The user owns testing and visual acceptance. Do not run tests, lint, builds, naming
guards, game launches, screenshots, automation or temporary validation programs.
