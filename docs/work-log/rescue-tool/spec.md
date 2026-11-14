# Rescue tool at the stuck moment — spec

Issue #243. When the board is stuck after the free rescue of the attempt is used or declined, the stuck pointer goes
to a tool that is proven to rescue the position, not to the first tool with stock.

## Behaviour

1. Trigger: `_watch_stuck` as today (a proven dead end, or no legal move, with undo possible). The first dead end of an
   attempt still offers the free rescue card with the pointer on undo behind it (`show_stuck_hint(false)`); every later
   dead end runs the search (`show_stuck_hint()`).
2. Candidates: the visible tool slots except the Firefly (a hint cannot change a dead end), ordered by price, cheapest
   first, ties in slot order. Undo costs 0 while free undos remain, else `Progress.UNDO_PRICE`; boosters cost
   `Progress.booster_price`.
3. Search on a worker thread, 20000 states / 600 ms per candidate:
   - a booster rescues when it has an effect (`Rules.booster_has_effect`) and `Solver.hint` on a clone with the booster
     applied returns `SOLVED` (`Solver.rescue_booster`, new);
   - undo rescues when an earlier position of the attempt is in `_winnable`, or the position one step back is proven
     `SOLVED`. The free rescue now records the position it returns to in `_winnable`.
4. The glow and pointer go to the first rescue. The slot's own action does the rest: with stock it uses the tool, without
   stock it emits `buy_requested` / `undo_more_requested` and main opens `UndoOffer.open_item` (coins or ad).
5. No rescue: the pointer stays on undo (today's choice).
6. Debug log: `RESCUE_TOOL level=<id> tool=<undo|sapling|owlEye|...|none> ms=<search> main_ms=<longest main-thread slice> tried=<n>`.
7. Any tap, move or HUD refresh (`_hide_stuck`) cancels a running search and drops its result.

## Files

- `PerchSort/game/board_view.gd`, stuck hint section only.
- `PerchSort/core/solver.gd`: new `static func rescue_booster(start, kinds, max_states, max_ms, cancel) -> int`.
- `PerchSort/tests/run_stuck_rescue.gd`: extended; `-- --frames=<dir>` records the timed evidence under xvfb.

## Not in scope

Level data, prices, new art, the Stuck card, a Restart pointer.
