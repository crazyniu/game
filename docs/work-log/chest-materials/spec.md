# Chest materials repair

## Authority and ownership

Implement the approved page-materials repair in the current working tree. Own only chest constants,
`chest`, `_chest_page`, `_chest_hero`, `_chest_action`, `_chest_rewards` and generic `rewards`
in `PerchSort/game/popups.gd`, plus `docs/design/star-chest.md`. Follow the exclusive
`build/ui-page-locks/popups.lock` protocol before source edits. Preserve all adjacent edits.

## Presentation

Remove the large cream hero and progress panels. Keep the existing sea, painted wooden heading,
balance and action family. Place the chest on a fresh transparent sandy islet with a honey-wood
platform; use warm rays and sparse sparkles behind it. Show progress directly over the sea.
Arrange actual rewards in a compact shallow arc with painted amounts and no individual cards.
Generic rewards keep the shared modal frame but use the same open object composition.
Detailed layout and asset direction belong in `docs/design/star-chest.md`.

## Preserved behavior

Keep thresholds, readiness, ready-count badges, reward aggregation, balance, preview interfaces,
callback signatures, modal dispatch, sounds and all 560 x 132 chest actions. Do not change rules,
awards, saves, locales or shared helpers. Honor instant mode for presentation motion.

## Delivery boundary

No tests, naming checks, lint, build, export, game launch, automation, capture or temporary
validation program. Source and raw asset inspection only; the user owns runtime acceptance.
