# Daily draw refinement contract

The user authorized all UI reached through the homepage's right check-in gift entry to be restyled to match the current shop. Use [the screen contract](../../design/daily-draw.md) and edit the canonical presentation in place.

## Ownership and shared contract

- Coordinator: `PerchSort/game/popups.gd`, related integration, and any necessary homepage entry change.
- Documentation worker: `docs/design/daily-draw.md` and this topic's `spec.md` / `report.md`; no code or shared style document edits and no further delegation.
- Reuse `ShopSkin.card`, `ShopSkin.button`, and `ShopSkin.modal` / `plaque`. No new runtime artwork is required.
- Main panel: width 960, content groups `[108, 774]`, action height 136; remaining-draw strip 760 x 108, grid cells 246 square with 18 gaps, action 480 x 136.
- Use cream cards, a compact rope-free wooden plaque with gold painted title, green main actions, teal counts, and existing clean prize icons.
- Draw results use the same family, aggregate identical rewards, and show the actual awarded item and quantity. Other reward dialogs are outside this refinement.

Preserve existing data ownership, reward callbacks, save fields, daily rules, probabilities, navigation, locale selection, music, and shared modal behavior. Maintain fake-data previews in the canonical script; no alternate scene, demo implementation, font labels, explanatory subtitles, or added gameplay features.

## Delivery boundary

The user's current instructions assign testing and acceptance to the user. Do not run tests, naming guards, lint, builds, UI automation, screenshots, or temporary validation programs. Deliver source changes for the user to try. Record runtime appearance, interactions, motion, and audio as unassessed until the user reports them.
