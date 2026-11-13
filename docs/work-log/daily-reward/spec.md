# Daily reward

User request (2026-11-13): the home trophy tile (Daily Challenge) becomes a daily reward that pays coins and boosters;
logging in on consecutive days pays more. The design is left to Claude.

## Rules (`meta/progress.gd`)

- Seven-day login calendar, `DAILY_REWARDS` = [coins, random boosters] per day:
  1: 50 | 2: 80 + 1 | 3: 120 | 4: 150 + 1 | 5: 200 | 6: 250 + 2 | 7: 500 + 3 (one week: 1350 coins, 7 boosters).
- One claim per calendar day. A claim the day after the previous one moves one day on; a missed day, or the day after
  day 7, starts again at day 1. Boosters are random unlocked ones (`unlocked_boosters`), so a locked tool is never given.
- `daily_reward_available`, `daily_reward_day` (the day the page shows as today's) and `claim_daily_reward`
  (`{"day", "coins", "boosters"}` or `{}`).
- The claim owns the existing save fields `lastDailyGiftDate` and `checkinStreak`; no field is added or renamed.
- Lucky draw: still `min(streak day, DRAW_MAX_SPINS)` draws a day, but opening it no longer writes the streak; the
  daily reward is the only check-in.
- Daily Challenge is removed (`Levels.daily_challenge`, `Progress.challenge_*`, `main.start_challenge`). Its save
  field `challengeDate` stays in `Profile` and round-trips unchanged.

## Home (`game/home_view.gd`, `game/main.gd`)

- The first event tile becomes `DailyReward` with the painted calendar picture `pic_calendar` (existing art), same
  frame, size and place; a red "1" badge while today's reward is waiting. Signal `daily_reward_requested`.
- At launch the page opens by itself 0.9 s after Home settles when today's reward is waiting and level 1 has been
  cleared (a new player goes straight to play). Never in captures.

## Page (`game/popups.gd` `daily_reward`)

- Shop and Lucky Spin page family: forest backdrop, hanging sign "Daily Reward / 每日奖励" at `PAGE_SIGN`, Close
  top-left, feather counter top-right.
- No backing sheet and no card inside a card (`docs/design/island-style.md`): each day is its own cream card
  (`ButtonKit.panel`, the shop's card) floating on the forest. Days 1-6: two columns of 482 x 290 cards with 20 px
  gaps in 48,352,984,1220; day 7: one full-width 984 x 290 card under them with glow and sparkles.
- Each card: the day number on the shop's green disc at the top-left corner, the coin pile with its amount, and a
  mystery chest with "×n" when boosters are included.
- Claimed days are dimmed like disabled items with the shop's tick top-right; today's card has the shop's green
  selected outline, bounces and its pictures breathe.
- One green Primary action "Claim / 领取", dimmed and inactive once claimed.
- Claim: the slot pops, the tick stamps in, coins burst, the counter bumps; then the rewards dialog (sign title "Daily
  Reward", calendar hero, coins and the actual boosters). Its OK closes the page.
- Claim sits at 280,1600,520,176 (360 x 136 Primary plate).
- New painted words (Codex, en and zh): `lettering_daily_reward_title`, `white_claim`. Manifests only gain entries.

## Preview states

`popups:daily_reward`, `daily_reward_first`, `daily_reward_week`, `daily_reward_claimed`, `daily_reward_claim`
(presses Claim after 0.5 s), `rewards_daily`, `rewards_daily_coins`, `rewards_daily_all`.
