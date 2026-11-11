# Shop material repair delivery

Implemented in the current local working tree. Runtime acceptance remains with the user.

## Changed

- `PerchSort/game/shop_view.gd`: removed full-height cream item backdrops. Six boosters
  share two wooden shelves; species use leafy perches; nests use low display stands.
  Item art is fitted by visible bounds and aligned to a support baseline.
- Painted item names now use44-pixel-high boxes. Main booster/nest action faces share
 224x80 dimensions; stock badges remain60x60 and show zero. Outfit counts have a small
 144x64 face. Three equal icon-only tabs retain their sizes and add a selected check.
- Owned/equipped nests retain the existing callbacks and now identify Equipped inside
  their compact action face, with a check badge on the item.
- Purchase confirmations use one existing painted wood frame, a smaller item-name
  plaque, an item on a stand, the necessary booster caption, existing quantity controls
  and a numeric total. No inner item card is added.
- `docs/design/shop.md`: current material roles, exact element boxes, asset provenance,
  production prompts and the user-owned acceptance boundary.
- New original transparent art: `PerchSort/art/island/prop_shop_shelf.png` (1760x894)
  and `PerchSort/art/island/prop_shop_nest_stand.png` (2172x724), generated with the
  built-in image tool and copied into the project. Existing source art is retained.

## Preserved

The canonical view, its signals, `preview()`, cached tab pages, one-bird-per-frame
preparation, hidden-page processing rules, catalog prices, unlock eligibility,
quantity entry and purchase/equip callbacks remain. Aviary detail and all shared
helpers, locale manifests, gameplay, save data and audio were not edited by this task.
No shared-file lock was needed: all edits were in the assigned files or new unique assets.
No commit, branch operation, package or release was made.

## Inspection and boundaries

Read-only source inspection and direct viewing of clean/generated asset images were
performed. `sips -g pixelWidth -g pixelHeight` reported the source dimensions above.
An optional Pillow metadata read could not run because Pillow is not installed; no
dependency was installed and no temporary inspection program was created.

No tests, naming guards, lint, builds, exports, game launches, screenshots, UI automation
or timed captures were run. Source delivery does not establish runtime rendering,
word readability, support contact, purchase behavior, animation, audio, performance or
device acceptance. No runtime baseline was produced or approved. Godot asset imports
are left to the user's normal project opening/launch.
