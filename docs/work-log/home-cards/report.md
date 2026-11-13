# Home card refinement delivery

The canonical homepage now uses the current shop's painted cream surface for Settings, Daily Challenge, Lucky Draw, Map, Shop, Aviary, and Chest. Runtime acceptance remains with the user.

## Implementation

- `PerchSort/game/home_view.gd` builds the seven controls through `_card_button`, reusing `ShopSkin.card` without changing the shop implementation. Cream faces and their separate icons dim together on press, with no resizing.
- Existing hit areas and the centered 190 x 174 navigation faces remain unchanged in the homepage's reference coordinates. Icons fit their visible bounds into centered square boxes at 72% of each face's shorter side.
- Settings, challenge, map, shop, aviary, and chest reuse the existing clean `pic_*` artwork. Lucky Draw uses the new transparent `PerchSort/art/icon_home_draw.png`, produced with built-in image generation from the existing `home_icon_lucky_draw` artwork. The original asset is retained; the cream background and detached sparkles are absent from the displayed gift.
- Badge placement now accepts a generic `Control` face and follows the final visible card bounds. Existing reward conditions and navigation callbacks remain intact.
- The complete localized Play textures, coin HUD, background, wordmark, six animated birds, fake-data previews, rules, economy, and persistence are unchanged by this refinement. No visible instructions or labels were added.
- The visual contract is recorded in `docs/design/island-style.md`; execution scope and ownership are in `spec.md`.

## Independent source review

A separate reviewer inspected the final source diff, shared skin implementation, existing homepage integration points, and generated gift icon. No actionable source issues were identified. The seven card faces use the existing shop nine-patch with fixed corner scale; navigation insets are established before icon fitting; badges remain above their faces. `Play/Face` remains a `TextureRect` for in-place locale switching, while cream faces are generic controls.

The homepage depends on the existing uncommitted shop work: `PerchSort/game/shop_skin.gd` and `PerchSort/art/island/btn_shop_tab.png` are already present in the working tree. This delivery preserves that work and is not a standalone committed package. No commit, merge, export, or release was performed.

## Acceptance boundary

Only read-only source/diff inspection and visual inspection of the standalone gift image were performed. In accordance with the user's instructions, no tests, naming guards, lint, builds, validation programs, UI automation, runtime captures, motion evidence, or audio checks were run or added. No runtime baseline was created. Engine import, actual card appearance, navigation interactions, and user approval remain unassessed.

The user can inspect the normal homepage after reopening the existing Godot project or relaunching `./run.sh`; the launcher's normal asset-import step has not been executed during this change. Existing `home_view:home`, `home_view:home_new`, and `home_view:home_max` preview states remain available for review. The gift generation prompt and production path are recorded in `docs/design/island-style.md`.
