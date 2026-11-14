# Board, shop and queue fixes report

Tests (new, each fails without its fix): `run_undo_allowance` (view restart keeps 0 and 7 undos),
`run_board_shop_refresh` (shop balance, house and outfit refresh, queue dropped after leaving during the entrance,
how-to-play stays due), `run_daily_reward_flow` (timed).

Daily check-in timing, before and after (claim pressed at 0 ms, motion mode):

| | last coin gone | rewards card opens | coins still flying at opening |
| --- | --- | --- | --- |
| before | 1162 ms | 1162 ms | 4 |
| after | 1248 ms | 1618 ms | 0 |

Other checks: `scripts/run_tests.sh`, `run_stuck_rescue`, `run_board_hud`, `run_family_dialogs`,
`run_gameplay_dialogs`, `run_map_aviary`, `run_shop_cards`, `run_purchase_dialogs`, `run_reward_quantities`,
`run_menu_navigation`: 0 failures. `run_draw_purchase` and `run_chest` fail identically on main.

Boundaries: not played through the graphical client; the house swap and outfit swap were checked by node state, not by
a visual alignment capture of the new house on a live board.
