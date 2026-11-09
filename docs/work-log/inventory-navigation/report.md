# Inventory and navigation repairs report

## Changed

- `game/main.gd` delegates restart to the board's coordinated restart method instead of restoring a session and immediately replacing it from stale profile inventory.
- `game/shop_view.gd` emits `booster_purchased(kind, count)` after a successful booster transaction and before `profile_changed`. The count includes both item pack size and selected purchase quantity.
- `game/main.gd` applies that exact purchase count to the board through `granted()`. It no longer derives a purchase quantity by subtracting session inventory from profile inventory. `GameSession.grant_booster()` already applies purchases to the live board, initial snapshot, and undo snapshots, so no core change was needed.
- `set_screen()` now disposes overlays without refreshing the screen it is replacing. Opening another overlay also skips an underlying refresh. Closing no overlay returns immediately. A normal shop or aviary close refreshes the revealed screen once.
- Shop profile changes save immediately; hidden map/home pages refresh when the shop closes instead of reconstructing on every purchase.

## Integration

The coordinator supplies `BoardView.sync_inventory()` and `BoardView.restart()`. Undo and restart must publish the restored full session inventory before any subsequent purchase or new session. Booster use and Firefly use also publish the updated inventory. The shop transaction already modifies the profile; `granted()` updates only the session.

## Review and remaining acceptance

Reviewed the source changes and signal ordering. No tests, lint, builds, game runs, UI automation, captures, or temporary verification programs were executed. Inventory behavior and navigation timing remain for user verification.
