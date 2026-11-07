# Popups redesign

Owner files: `PerchSort/game/popups.gd`, `PerchSort/game/popup.gd`. Spec: `docs/design/ui-system.md` section 5.

## What changed
- All popups use one content-driven frame builder (`_frame`). Panel width is 880 (content popups) or 720 (settings). Its height is computed from the stacked content groups: 32 px between groups, content 32 px inside the felt rim, nothing empty. The whole composition (hero bird, panel, action row) is centred vertically. The measured `layout/*.json` rects are no longer used by popups. All layout constants sit at the top of `popups.gd`, named by meaning.
- Title ribbons are drawn by popups.gd at one shared scale: every `label_title_*` is fitted through one template content rect at 640 wide (540 in settings), centred on the panel top edge. `label_name_*` titles (goal info) are 110 tall.
- Close disc is 96, centred 24 px inside the panel's top-right corner, and draws above the content. Its glyph is now 50 % (popup.gd).
- Action rows sit 40 px below the panel. Primaries are green 160 and secondaries wood 128, 64 px apart and centred on one line. Disabled next/play buttons are dimmed and keep the same size.
- Win: the hero sunny BirdView stands on top of the ribbon, in front of it and no longer clipped. The sky and berry BirdViews stand fully visible on the panel's top edge beside the ribbon. Stars are 132 with threshold pills (80, numbers 30) under them, then the moves pill (80, footprints 60, number 52), then the reward pill (96, coin 104 overlapping its left end, number 72). Zero reward drops the reward row and the panel shrinks. Actions: replay 128, next 160, home 128. Motion is kept: rays, ribbon drop, stars 0.18 s apart, counters counting up, confetti, birds flying in and the coin burst to the HUD coin (now aimed at the coin position from ui-system.md section 6).
- Settings: a 720 panel with only the ribbon and the discs. Sound, restart and home are 128 in a row, with two discs centred when there is no restart. Captions are dropped because the three plates have different painted widths and the icons are clear. The quick fade+scale motion is kept.
- Level card: stars and thresholds, the best pill when set (footprints + painted word "Best" + number, all cap 52), species rings of 150 in rows of up to 4 (32 px apart), and the mechanics strip as one pill with 64 icons when non-empty. Each ring holds a live BirdView fitted to 80 % of the ring (not stretched; the old ring was squashed to 0.92 height) and an ability badge at 44 %. Green play disc 160 below.
- Goal info: name ribbon title, a 280 live BirdView, then the ability badge 96 + caption plate 62, centred together.
- Daily gift: the gift stage is scaled so its visible canvas fills a 400 row. Coins and sparkles now live in the stage canvas, so they stay attached. Reward pill 96, check 160.
- New friend: 300 live BirdView, name ribbon 110, reward pill, check 160.
- Booster unlock: 300 illustration (breathing prop), sparkles, caption 62, check 160.
- World complete: stars, a 500-wide arch with three live BirdViews (no more still pictures) cheering in turn, reward pill, check 160.
- World unlock: the gate stage is 560 wide inside a clip that crops the tall light glow but keeps the swung doors (80 px each side). Caption 62, play 160.
- Added the preview state `level_card_locked` (disabled play disc, mechanics without a best pill).

## States checked (captures at full size and as crops, two full passes)
win_three_stars, win_two_stars, win_one_star, win_zero_reward, settings_sound_on, settings_muted, level_card_first_visit, level_card_full_roster, level_card_locked, goal_info_berry, goal_info_hoot, daily_gift, new_friend_berry, unlock_owl_eye, unlock_sapling, world_complete_1, world_unlock_2, world_unlock_4 and leaf_transition (owned by main.gd; only captured).
Win sequence frame sheet: `--motion --frames --wait=3.6 --step=0.3 --preview=popups:win_three_stars` (12 frames). The sequence plays rays, ribbon drop, stars, counting, fly-ins, the coin burst and the hero cheer. Smoke: `smoke checks: 0 failures`.

## Spec conflicts and decisions
- Title ribbon 110-130 tall vs 520-640 wide: the painted ribbons are about 7.2:1, so at 640 wide the body is about 88 tall. Width won. 120 tall would need about 870 px, almost the whole panel.
- Inner padding 56: the felt rim of panel_wood is 50-70 px, so content 56 from the outer edge would touch the rim. Padding is applied as rim + 32 instead: top max(ribbon half + 32, 92) and bottom 102.
- Reward total is a hero number (cap 72), which does not fit an 80 pill. The reward pill uses the 96 height.

## Requests for other owners
- `scripts/screenshots.sh`: add `popups:level_card_locked` to the popup list.
- The HUD coin target (`HUD_COIN` = 92, 88) assumes the board coin pill from ui-system.md section 6. Adjust it if board_view places the coin elsewhere.

## Missing art / remaining issues
- No missing art required. Caption plates for sound/restart/home cannot be made uniform (different painted widths), so they are not shown.
- The goal info name ribbon (`label_name_*`) is a different ribbon style from the `label_title_*` ribbons; this is acceptable but a matching title-style name sprite would unify them.
- Not yet reviewed by the user.
