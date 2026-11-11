# Daily draw refinement delivery

The check-in gift entry opens the existing lucky draw. Its presentation contract now follows the current shop: cream rounded surfaces, a compact rope-free wooden title plaque with gold lettering, fresh green action faces, teal prize counts, and consistent prize-card spacing. The draw board and draw-only result popup share this family. See [the screen design](../../design/daily-draw.md).

Implementation is in the canonical `PerchSort/game/popups.gd`; this record does not claim runtime acceptance. Existing daily rules, economy, probability tables, saves, and reward ownership remain outside the presentation scope. No new artwork or parallel scene was introduced. The homepage already received its shared ShopSkin entry in concurrent work and was not edited by this refinement.

- `lucky_draw` uses the shop's modal, plaque, cream cards, green action, fitted clean icons, and fixed cell sizes. Five remaining-draw stars sit in a compact strip.
- The travelling highlight uses a modal-owned tween. Draw and close actions remain disabled while it runs; removing the modal cancels that sequence.
- `_draw_rewards` groups identical boosters into one quantity, shows coin rewards directly, and returns to the board after either Confirm or the close tile. A mystery cell resets to its chest after the result closes.
- A nested result reuses the draw's dim backdrop, avoiding double darkening. It is removed with its owning draw modal.
- Existing fake-data previews now place the real homepage below draw popups. Added states show all available draws and direct coin/grouped-booster results.

An independent source-only review inspected the scoped draw code, result routing, close callbacks, fixed dimensions, shared surfaces, painted-word locale behavior, and referenced icon paths. No obvious parse/type blocker was identified by reading; this is not a Godot compile or runtime result. The review's stale-booster-display finding was addressed by resetting the mystery cell on return.

The user owns testing and acceptance. No tests, naming guards, lint, builds, UI automation, screenshots, timed captures, temporary validation programs, package, commit, or release were performed for this refinement. Runtime appearance, motion, audio, touch/device fit, and actual interactions have not been assessed. No runtime capture was promoted to an approved baseline.

For review, open the normal game and use the right-hand gift entry, then inspect available draws, the travelling highlight, coin and booster results, exhausted draws, and closing/reopening. The existing canonical popup previews remain available for fake-data review without modifying the player's profile.
