# Lucky draw: one name, a visible result, no automatic popup

User report (2026-11-09): the daily lucky draw has naming that does not match, a draw ends without saying what was
won, and the popup opens by itself when the game starts.

## Contract

- One name everywhere: "Lucky Draw". The home tile is repainted (gift box, "Lucky Draw") as
  `art/home_button_lucky_draw.png`, replacing the calendar "Check-In" tile. The popup title is `draw.title`
  ("Lucky Draw") and its button is `draw.go` ("Draw"). Unused `checkin.*` and `home.checkin` labels are removed.
  Code identifiers follow: `lucky_draw_requested`, `show_lucky_draw`, tile node `LuckyDraw`.
- Save fields (`checkinStreak`, `lastDailyGiftDate`, `drawDate`, `drawsLeft`) and `Progress` rules are unchanged.
- When the light stops, the prize cell pops, then the shared rewards popup (title "Lucky Draw") shows the booster(s)
  or a coin with the amount. Dismissing it clears the light and re-enables Draw if draws are left.
- The game no longer opens the lucky draw on launch. The tile badge is the only reminder.

## Files

`game/popups.gd`, `game/main.gd`, `game/home_view.gd`, `art/home_button_lucky_draw.png`,
`art/locale/en/*`, `tools/kit/locales/en.json`, `tools/home/sketch/lucky_draw.txt` (Codex prompt).
