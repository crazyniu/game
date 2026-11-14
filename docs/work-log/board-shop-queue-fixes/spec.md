# Board, shop and queue fixes, daily check-in timing

Issues 233, 234, 235 and 236, plus the user's report that the daily check-in rewards card opens while coins are
still flying.

- 233: `BoardView.restart` carries `session.undos_left` over `start()` (which builds a new session), so a restart
  neither refills spent undos nor lowers a purchased allowance.
- 234: a level's popup queue (new birds, unlocks, how-to-play, lessons) belongs to its board: it is dropped when that
  board is no longer the screen, and each lesson or unlock is marked seen when it is shown, not when it is queued.
- 235: `Main._refresh_screen` also refreshes a mounted shop or aviary (`refresh_balance`), so a feather purchase made
  from the open shop updates that shop's balance and cards (scroll kept).
- 236: `BoardView.refresh_hud` syncs appearance: a newly equipped house is rebuilt in place (behind the status and
  tools, resident re-seated) and birds whose outfit changed are replaced at the same seat and index. Moves, undos and
  seats are untouched. Outfits stay on the original species only (`_look_of`, as at creation).
- Daily check-in: `Popups.coin_flight` returns the time until its last coin lands; the daily reward page waits for it
  (plus 0.3 s for the counter bump) before opening the rewards card.
