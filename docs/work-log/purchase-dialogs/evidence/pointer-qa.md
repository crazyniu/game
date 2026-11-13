# Actual desktop input checks

CUA interacted only with a temporary canonical preview window. The original running game was not changed.

1. Open Firefly confirmation with balance 1280, inventory 3, quantity 1 and price 30.
2. Click +. The visible quantity changes to 2 and price to 60; minus becomes enabled.
3. Click the painted quantity field, select all, type 5 and press Enter. Visible quantity is 5 and price is 150.
4. Click the coin-shop-style Close. Dialog disappears, same shop returns, balance remains 1280 and inventory remains 3.
5. Reopen Firefly from its card. Quantity resets to 1 and price to 30.
6. Click + then the gold price action. Dialog disappears, balance becomes 1220 and inventory becomes 5, exactly one purchase of two Fireflies.
7. Close the temporary preview through its page control. The test window exits; original game and other workers' terminals remain.

Screenshots were observed through CUA after each meaningful action. Transaction invariants and repeated-callback/insufficient-funds cases are additionally checked in `run_purchase_dialogs.gd`.

## Subsequent log review

The visible outcomes above were observed, but the initial session's `pointer.log` also recorded invalid `"white"` glyph-style lookups while editing. That error was missed in the first report. The five metric calls have since been changed to the production color-to-style mapping. `focused-tests.log` now contains 86 passing checks, including actual focus and per-frame caret/selection assertions in both locales, with no runtime diagnostics. Do not treat the original pointer session as an error-free pass.

The corrected focused suite was then rerun under the actual GUI/OpenGL renderer through CUA. It completed 86/86 checks with no SCRIPT ERROR/ERROR diagnostics, including real focus, active editing, caret position, partial/full selection and blur in both languages. The terminal visibly confirmed completion and the temporary test window exited. The original running game remained untouched.
