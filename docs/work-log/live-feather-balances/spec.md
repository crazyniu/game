# Live feather balances

Fix #263, the two mounted-page refresh gaps left outside #235's outer-shop fix.

- A daily-reward page keeps its claimed-day state but follows a successful feather purchase.
- Claiming after a purchase displays the current profile total, never the page-opening snapshot.
- An outfit detail opened inside the shop updates with its parent after a purchase.
- Cancellation never grants feathers; the display cap remains presentation-only.
- No layout, art, gameplay, level, reward, payment or save-schema changes.

Ownership: `main.gd` daily and purchase refresh calls, `popups.gd` daily_reward only, `shop_view.gd` refresh_balance, one focused regression, this work log. Do not touch parallel victory-dialog or level work.
