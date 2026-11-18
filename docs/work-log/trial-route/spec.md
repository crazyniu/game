# Restore the Cocos partial-goal trial route

Related issue: [#260](https://github.com/crazyniu/game/issues/260). This repairs the launcher/navigation regression after the Cocos migration; it does not decide whether partial goals enter the catalog.

## Existing contract

- Keep the three authored levels in `assets/resources/levels/trial.json`, outside the 210-level catalog and map.
- Reuse the current board, named-species keys/check rings, partial-goal rules, solver, tools, undo, rescue and leftover departure animation.
- `--trial=1`, `--trial=2`, and `--trial=3` open the corresponding trial. Trial 1 shows the first hint after its entrance without spending a Firefly. Win offers the next trial, replay and home; the last trial has no next action.
- Each board receives a copy of the profile. Trial moves, stock use, tutorial flags and wins must not reach the real profile. Do not call catalog settlement, grant drops, record trial IDs, or change the normal resume level.
- Preserve the documented purchase exception from #260 / #275: explicitly confirming a tool or extra undo spends real feathers through the existing priced modal. The real profile remains under the same single-writer lock as normal play; do not bypass the lock for trials. A purchased tool is added to both the saved inventory and the current trial's copy; trial consumption stays temporary.
- Settings and purchases remain explicit real-profile actions. No new copy, artwork, rules, level data, payment provider or ad provider.

## Scope and acceptance boundary

Own `Boot.ts`, the existing shell/PowerShell launchers, and trial-route documentation. Preserve current asynchronous navigation, settlement and modal lifecycle fixes. No local tests, builds or UI automation are authorized for this follow-up. Existing PR CI may run, but it does not establish Creator/browser/device or player acceptance.

Keep #260 open: the user still needs to play the three trials and decide whether the wordless goal is clear and the puzzles require thought. Order goals and catalog placement remain unapproved.
