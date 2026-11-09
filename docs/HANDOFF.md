# Handoff

State on 2026-11-08, after the growth round was merged to main. Read `CLAUDE.md` first; its hard constraints apply to
everything below. Chat with the user in Chinese, write code, commits and reports in English.

## Where things are

- Repo `crazyniu/game`, Godot project `PerchSort/`. Development on machine 251 (`/home/developer/Projects/game`); the
  user tests on the Mac (`/Users/apple/Documents/projects/game`, `./run.sh`), which must be synced with
  `git pull --ff-only` after every merge.
- Merged rounds: UI rebuild #164, fixes #165-#169, polish #170, home page #171, painted kit #172, matte kit #173,
  growth round (economy, level gift, chest, check-in, daily challenge, level order, feel, mechanic lessons, outfit
  turn frames). Round reports live in `docs/work-log/<topic>/report.md`; the growth round is
  [here](work-log/growth-round/report.md).
- Images are painted by local Codex on 251:
  `codex exec --skip-git-repo-check --dangerously-bypass-approvals-and-sandbox -C <repo> "<prompt>" < /dev/null`
  (without `< /dev/null` a background job hangs on stdin).
- Words are baked label sprites: add ids to `tools/kit/locales/en.json`, then
  `uv run --no-project --isolated --with pillow tools/kit/build_localized_labels.py en --labels-only <ids>` and
  `godot --headless --path PerchSort --import`. Legal and help texts: `tools/kit/locales/en_docs.json` baked by
  `tools/kit/build_documents.py`.

## In progress elsewhere

1. **UI fixes on the existing UI (owned by the session `game-d7`).** The user rejected both the glossy candy kit and
   a full relayout; the experiment lives only in `git stash` ("glossy candy kit experiment"). The agreed path is
   targeted fixes on the current UI: fewer and smaller words, labelled tiles instead of bare icons, bigger buttons,
   brighter greens, board perch spacing, auto-home of the last flock. See
   [ui-fixes](work-log/ui-fixes/report.md). game-d7 still owns `game/home_view.gd`, `game/shop_view.gd`,
   `game/popup.gd`, the settings/win/level-card parts of `game/popups.gd`, the `game/map_view.gd` footer,
   `game/ui.gd`, `game/ui_text.gd` and `art/kit/*`. Do not edit those without asking it.

## Waiting on the user

2. **Mac test of the merged game.** The user has not yet played the growth round (gift popup and tool hint,
   check-in, chest, daily challenge, mechanic lessons, haptics switch, outfit turns). UI approval is theirs; never
   declare a screen accepted on your own.
3. **Visual baseline.** After the user approves the screens, copy the captures from `bash scripts/screenshots.sh`
   into `art/approved_runtime` as the new acceptance baseline. Do this only after the restyle in item 1 settles.
4. **Release inputs.** The Privacy Policy, Terms of Service and Contact texts in `en_docs.json` are drafts; the user
   must review them and give a real support contact. Store identifiers and package names also need the user.
   Never release without explicit user authority.
5. **Chinese version.** Cancelled for now ("中文先不做"). The plan is kept in
   [task-chinese](work-log/growth-round/task-chinese.md); the branch `crazyniu/growth-chinese` holds only the contract
   commit and can be deleted.

## Known defects and follow-ups (nobody owns these yet)

6. Map top end: fixed in the UI fixes round (footer tiles are 150x132 and sit lower; level 36's stars clear them).
   Recheck on the Mac.
7. Mechanic lessons show only on the first level with that mechanic, so a save already past it never sees the lesson
   unless that level is replayed. Deliberate; could add a "lessons" entry in How to Play.
8. Done: `main.mechanics_of` now calls `tutorial.mechanics_in` (one detector); the level card shows mama/chick too.
9. Headless runs print `ERROR: 1 resources still in use at exit` and 2 leaked ObjectDB instances: the still-playing
   background music stream (`audio/bgm_menu.wav`) under the Dummy audio driver. Stopping the players in
   `Sound._exit_tree` did not clear it. An engine exit notice, not a game defect; ignore.
10. Haptics are guarded by `OS.has_feature("mobile")` and have not been felt on a device.
11. Done: after the level reorder every level's hint is ready within 0.5 s of the level start (levels 17, 30 and 34
    in about 14 ms), all served from the cache (`hint_all` preview).
12. A few 3/4 bird frames placed outfit overlays badly; the painted turn frames fix turns, but other overlay frames
    are limited to the reliable set (`bird_view._keep_reliable`). Painting more outfit frames would remove the limit.

## Verification

`bash scripts/run_tests.sh` (naming 0, smoke 0, core 0; about 5 minutes) and `bash scripts/screenshots.sh`; motion
needs frame sequences (`main.tscn -- --frames=<dir> --wait=S --step=0.1` under xvfb). Look at every capture before
reporting.
