# Daily reward: report

## Changed
- `meta/progress.gd`: seven-day calendar `DAILY_REWARDS`, `daily_reward_available`, `daily_reward_day`,
  `claim_daily_reward`; the lucky draw no longer writes the streak. Daily Challenge removed (also `Levels.daily_challenge`).
- `meta/profile.gd`: comments only; `challengeDate`, `lastDailyGiftDate` and `checkinStreak` keep their names and round-trip.
- `game/home_view.gd`: the event tile `DailyReward` (`pic_calendar`), badge while today's reward waits.
- `game/main.gd`: `show_daily_reward`; at launch it opens by itself once level 1 is cleared.
- `game/popups.gd`: `daily_reward` page, claim stamp, previews; Lucky Spin colour fix (below).
- Words (Codex, en and zh): `lettering_daily_reward_title`, `white_claim`; gold word entry `daily_reward.title`.

## Colour fix (user, 2026-11-13: Lucky Spin colours did not match)
The mint slots with peach rims and the cyan used stars clashed with the cream card. Used stars are now
`star_draw_spent` (the gold star faded to tan, derived; original untouched).

## Layout fix (user, 2026-11-13: "UI does not match the other pages; card inside card again")
The first Daily Reward page put slots on a big cream sheet, breaking `island-style.md` ("no big backing sheet").
Both Daily Reward and Lucky Spin now place each item on its own shop cream card directly on the forest. A gold-rim
green plate tried in between was withdrawn: the spec gives green actions a cream rim, so every green button is
unchanged.

## Checks
- `bash scripts/run_tests.sh`: naming OK, smoke 0 failures, core 0 failures (new calendar, streak, draw checks).
- `run_family_dialogs.gd`: 1780 checks, 0 failures.
- `run_home.gd` hangs on the base branch too (it reads constants that no longer exist); not run as evidence.
- Captures: `evidence/pages_after_colour_fix.jpg` (Daily Reward en and zh, week, claimed; Lucky Spin; Shop for comparison); claim motion 0.25 s frames
  `evidence/claim_sequence_frames.jpg` (from `--preview=popups:daily_reward_claim --frames`).

## Open
- The level card's bird rings still use the mint `hud_tool_slot` inside the cream dialog card.

## Primary buttons (user, 2026-11-13: "all pages' main buttons bigger and longer, like the home Play")
- `ButtonKit.PRIMARY` 360 x 136 -> 500 x 188, the home Play button's visible size and shape (same 2.65 ratio as the
  green plate). Same in `FamilyDialog.PRIMARY`, `UI.PLAY_SIZE`, the tutorial Got It.
- Purchase Buy `WIDE` 520 x 132 -> 560 x 188. List and shop-card actions (`SECONDARY` 288 x 108) are not page
  primaries and stay.
- The 1.25 rule: buttons beside a primary grow with it. Dialog secondaries 400 x 152, Modal secondary pills
  400 x 152, win Retry/Home keys 152, quantity keys 152 with a 240 x 150 field.
- Hit areas of the chest, draw result and daily pages enclose the larger plate.
- Checks: run_tests (naming, smoke, core), run_chest, run_family_dialogs 1780, run_purchase_buttons 128,
  run_purchase_dialogs 98, run_draw_result 353, run_gameplay_dialogs 72, run_dialog_skin 192, run_menu_navigation
  107: all 0 failures. Captures of 16 pages: `evidence/primary_buttons.jpg`.
