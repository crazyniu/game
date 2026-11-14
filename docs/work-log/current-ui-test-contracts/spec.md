# Current UI test contracts

Scope: repair nine demonstrably stale tests against main a14e30a, without modifying game code, art, economy, saved data or gameplay. Keep real pointer/callback coverage and meaningful bounds assertions. No historical baseline is invented.

- Panels reserve PurchaseDialog.TITLE_ROOM above logical content; test visible material bounds separately from logical action layout.
- Home has four independently animated birds, canonical icon hit boxes and uniform visible icon slots.
- Outfit actions use ButtonKit green plates, painted white labels and FeatherPrice quantities.
- World cards cover Levels.COUNT and Levels.WORLDS. World pages use their own backdrop, 21 local ordinal tiles and a bird beside the current tile, not the retired route.
- Reward child structure is checked after entrance sparkles finish, preserving quantity and deduplication checks.
- Current placeholder purchases credit the confirmed pack once. Opening/cancelling a confirmation never credits; duplicate confirmation signals must not double-credit. All other profile fields stay equal.
- Wait for modal destruction rather than assuming a fixed renderer-dependent timer.

Validation: rerun all nine under Godot 4.7.2 GL Compatibility at the native synthetic-input canvas; run naming, painted-word, smoke and 210-level core checks. Exercise map-node --all-levels. Missing purchase_surface historical geometry remains a documented untested boundary. No auto-merge or deployment.

Unapproved current Glow is a visual deviation, not a canonical selection contract. Report it explicitly without enforcing its presence; a geometry pass is not visual acceptance.
