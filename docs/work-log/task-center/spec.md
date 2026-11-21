# Task center, combo and skin gifts

Goal (owner, 2026-11-21): connect the daily systems so a player has a reason to return each day, make the combo visible, give common skins at milestones, and keep the no-fail, no-lives design. Skins may be added when the gifts run short.

## Done in the shell round

- **Combo on the board.** When flocks come home on consecutive moves (`Rules.finish`: every flock after the first pays 5 feathers), the board shows `xN` and a feather `+5` beside the house; the tone rises with the chain (`star_1..3`).
- **Win and world-complete birds.** The three birds cheering above the popup are the played world's sunny, sky and berry slots (`Worlds.look`).
- **Task center shell** (`game/TaskView.ts`, fake data through `TaskView.preview`): home entry (painted clipboard icon, left of the play button), today's and this week's points bars with chests, six daily tasks, claim buttons, a skin as the week's last prize, and the skin-gift popup (`Popups.outfit_gift`). Words: `tools/island/ui_words.tsv`, painted with `tools/island/ui_words.py`.

## Rules to connect after the screen is approved

Daily tasks (reset at local midnight, 20 points each):

| Task | Target | Counted when |
| --- | --- | --- |
| Clear levels | 3 | a level is won (replays count) |
| Earn stars | 6 | stars shown on a win popup |
| Send flocks home | 20 | a flock flies home |
| Make combos | 5 | a combo of x2 or more |
| Use tools | 2 | a tool is used on the board |
| Spin the draw | 1 | a lucky-draw spin |

Today's chests: 20 points 60 feathers; 50 points 60 feathers and 1 tool; 80 points 120 feathers; 100 points 100 feathers and 2 tools.
Week (Monday to Sunday, the sum of claimed daily points): 150 points 200 feathers; 350 points 3 tools; 550 points one common skin.

Skin gifts (common skin = the cheapest skin of a bird the player has met and does not own; 500 feathers when every such skin is owned):
- the week's 550-point chest;
- the first clear of each world: a skin of a bird of that world, shown after the world-complete popup;
- check-in day 7, in place of its tools.

The more expensive skins stay for feathers. No task needs an ad, so no task is stuck when ads are unavailable. Saves gain task and week fields with a schema migration; older saves start with empty progress.

## Not taken from fishsort

Lives, failing and revive: Perch Sort has no fail state and no timers. Sharing (owner, 2026-11-21).
