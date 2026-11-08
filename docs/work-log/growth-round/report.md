# Growth round: report

Scope: the coordinator's review the user accepted on 2026-11-08 (see [spec](spec.md)). The Chinese version was
cancelled by the user ("中文先不做"); `task-chinese.md` stays as the plan for later.

## Delivered

| Area | What changed | Files |
| --- | --- | --- |
| Economy | First clear pays 30/45/60 coins by stars, replay 10, improvement 15, world bonus 300 | `meta/progress.gd` |
| Level gift | First visit of each level gives one booster that fits it (mechanic-matched, else firefly/sapling alternating); after the popup the tool glows and a hand points at it | `meta/progress.gd`, `game/popups.gd`, `game/board_view.gd` (`point_at_tool`), `game/main.gd` |
| Star chest | Every 30 stars opens a chest: 150 coins and 2 boosters | `meta/progress.gd`, `game/popups.gd` |
| Check-in | Seven-day streak, rewards rise to day 7; entry on Home with a badge | `meta/progress.gd`, `game/popups.gd`, `game/home_view.gd` |
| Daily challenge | One seeded deal per day from a reached level, solver-verified (at least 3 moves), par = solution length, pays 80 coins once a day | `core/levels.gd` (`daily_challenge`), `meta/progress.gd`, `game/main.gd` |
| Level order | Each world now runs easy to hard (first level kept); schema-2 saves migrate old level numbers via `LEVEL_RENUMBER`; start hints regenerated | `levels/levels.json`, `levels/hints.json`, `meta/profile.gd` |
| Feel and layout | Quick entrance on replays, flock-home sparkle and rising pitch, branch dip on landing, haptics with a Settings switch, tools on a HUD bar, world ribbon | worker report: [feel-layout](feel-layout-report.md) |
| Mechanic lessons | An animated lesson the first time each of 8 mechanics appears | worker report: [mechanics-intro](mechanics-intro-report.md) |
| Outfit turns | 216 Codex-painted turn frames (`birds/*_outfit_N_turn_{left,right}_0N.png`) replace overlay turns, so outfits turn with the head | `game/bird_view.gd`, `tools/birds/outfit_turns` |
| Pictures | `pic_daily`, `pic_vibration` painted by Codex | `art/` |

New save fields (camelCase in JSON): `musicMuted`, `levelGifts`, `chestsOpened`, `checkinStreak`, `challengeDate`,
`vibration`. Existing field names are unchanged; the schema-1 to schema-2 migration only renumbers levels.

## Verification

- `bash scripts/run_tests.sh` on d392cd4d: naming guard 0 violations, smoke 0 failures (now requires
  `GDScript.can_instantiate()`), core 0 failures: 40 levels solved and replayed plus `_check_meta()` (migration, level
  gift once, check-in streak, chest per 30 stars, daily challenge determinism, solvability, pays once a day).
- Captures: `bash scripts/screenshots.sh` including the new states `level_gift`, `checkin_day3`, `chest_half`,
  `chest_ready`, `rewards_chest`, `document_privacy`, `document_credits`.
- Motion evidence for feel and lessons is under `feel-layout-evidence/` and `mechanics-intro-evidence/`.

## Open

See [the handoff](../../HANDOFF.md) for everything not done, including the pending kit-style decision.
