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
The mint slots, their peach rims, the cyan used stars and the peach button rim clashed with the cream-and-gold card.
Derived art (originals untouched): `slot_cream_well` (warm well, gold rim; Lucky Spin and Daily Reward),
`star_draw_spent` (gold star faded to tan), `btn_plate_green_gold_rim` (gold rim, calmer green). `ButtonKit` green
now uses the gold-rim plate, so every green action across the game changes with it.

## Checks
- `bash scripts/run_tests.sh`: naming OK, smoke 0 failures, core 0 failures (new calendar, streak, draw checks).
- `run_family_dialogs.gd`: 1780 checks, 0 failures.
- `run_home.gd` hangs on the base branch too (it reads constants that no longer exist); not run as evidence.
- Captures: `evidence/pages_after_colour_fix.jpg`, `evidence/daily_reward_en_zh.jpg`; claim motion 0.25 s frames
  `evidence/claim_sequence_frames.jpg` (from `--preview=popups:daily_reward_claim --frames`).

## Open
- The level card's bird rings still use the mint `hud_tool_slot` on the cream card.
