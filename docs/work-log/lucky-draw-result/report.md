# Lucky draw: report

## Done

- Home tile repainted by local Codex from the old tile and the Daily tile (prompt `tools/home/sketch/lucky_draw.txt`),
  resized to 371x305 as `art/home_button_lucky_draw.png`; the calendar "Check-In" tile is removed.
- Labels: `draw.title` "Lucky Draw", `draw.go` "Draw" baked with `tools/kit/build_localized_labels.py`; unused
  `checkin.*` / `home.checkin` labels removed.
- After the light stops, the board steps aside (its dim stays) and the rewards popup titled "Lucky Draw" shows the
  booster or the coin with "+N". Got it brings the board back with Draw enabled while draws remain. The rewards popup
  drops its coin row when no coins were won (no more "+0").
- Launch no longer opens the lucky draw; the tile badge remains.
- Previews: `popups:lucky_draw_spin` (booster) and `popups:lucky_draw_coins` (200 coins), both auto-press Draw.

## Verification

- `python3 ci/check_current_game_names.py --self-test` and `python3 ci/check_current_game_names.py`: OK, 0 violations.
- `bash scripts/run_tests.sh`: smoke 0 failures, core 0 failures.
- Frames: `main.tscn -- --preview=popups:lucky_draw_spin --frames=<dir> --wait=7 --step=0.5` (and `_coins`):
  spin runs about 4 s, the result popup is up by 5 s.

## Open

- The user must try the screen; the tile and the result flow are not approved yet.
