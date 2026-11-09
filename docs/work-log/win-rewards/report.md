# Win rewards: report

Delivered per [spec](spec.md).

- `meta/progress.gd`: new coin values, `win_drops`, `fitting_boosters`, `booster_to_point_at`, `used_flag`; `on_win`
  takes the next level and returns `boosters`.
- `game/main.gd`: the board's stock is copied to the profile before `on_win`. Before this change the copy ran after
  `on_win` and silently discarded the starter stock granted at unlock.
- `game/popups.gd`: `win` shows the drops; the `level_gift` popup is removed.
- `game/board_view.gd`: idle nudge, first-use flags.
- The save field `levelGifts` stays in the JSON (no longer written with new entries). The labels `popup.gift.title`
  and `gift.line` are now unused.

## Verification

- `bash scripts/run_tests.sh`: naming guard 0 violations, smoke 0 failures, core 0 failures, with new checks for drop
  counts, next-level favouring, milestone drops and the pointer.
- Captures: `popups:win_three_stars`, `popups:win_milestone`, `popups:win_one_star`.
- Frames: `--preview=popups:win_milestone --motion --frames --wait=4 --step=0.25` (coins count up, drops pop in one by
  one); `--level=3 --frames --wait=24 --step=1` (hand appears on the hint tool after the idle delay).
- Not seen in a recording: the live win after an actual level (`--autoplay` quits when the level is won) and the
  level-start pointer on a fitting booster. UI approval is the user's.
