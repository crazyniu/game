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

1. **UI layout and kit restyle (owned by the session `game-d7`).** The user compared the glossy candy kit with the
   merged matte kit (`build/style_compare.png`) and, per game-d7, rejected both because they only reskin the current
   layout. The target is the user's reference screenshots: far fewer and smaller words, a bottom tab bar (raised
   selected tab, icons, label only on the selected tab), bundle-style shop cards, a new palette. game-d7 owns
   `game/home_view.gd`, `game/shop_view.gd`, `game/popup.gd`, the settings/win/level-card parts of `game/popups.gd`,
   the `game/map_view.gd` footer, `game/ui.gd`, `game/ui_text.gd`, `game/outline.gdshader`, `art/kit/*`,
   `tools/kit/render_kit.py` and the note at the top of `docs/design/ui-system.md`. Its uncommitted candy-kit files
   were in the 251 working tree at merge time and are not part of main. Do not edit those files without asking it.

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

6. Map top end: no scroll stop keeps every node clear of the bottom buttons; level 36's stars sit a few pixels from
   the Aviary and Help rims. A real fix needs a different footer or trail; fold it into item 1's footer redesign.
7. Mechanic lessons show only on the first level with that mechanic, so a save already past it never sees the lesson
   unless that level is replayed. Deliberate; could add a "lessons" entry in How to Play.
8. `main.mechanics_of` (used by the level card) does not list mama/chick; `tutorial.gd` has its own detector that
   does. Unify them.
9. Headless autoplay runs often print `ERROR: 1-2 resources still in use at exit`. Not investigated; check whether it
   predates the growth round.
10. Haptics are guarded by `OS.has_feature("mobile")` and have not been felt on a device.
11. Hints on levels 17, 30 and 34 could spin early in a level (noted in the polish round); recheck after the level
    reorder, since numbers changed.
12. A few 3/4 bird frames placed outfit overlays badly; the painted turn frames fix turns, but other overlay frames
    are limited to the reliable set (`bird_view._keep_reliable`). Painting more outfit frames would remove the limit.

## Housekeeping

13. On 251 `main` two stashes are obsolete ("superseded aviary scroll-window change", "Local AGENTS/CLAUDE/README
    edits before the Godot port"); drop them when the user agrees.
14. Local branches `crazyniu/polish-birds`, `polish-board`, `polish-shop-aviary`, `polish-tutorial` were integrated
    through #170 (squash), so Git does not see them as merged; delete them after a quick diff against main.
15. Remote branches of merged PRs (#164-#173) can be deleted: `ui-rebuild`, `seat-clearance`, `counter-style`,
    `aviary-outfit-buy`, `ui-and-gameplay-polish`, `polish-followup`, `painted-kit`, `matte-kit`.

## Verification

`bash scripts/run_tests.sh` (naming 0, smoke 0, core 0; about 5 minutes) and `bash scripts/screenshots.sh`; motion
needs frame sequences (`main.tscn -- --frames=<dir> --wait=S --step=0.1` under xvfb). Look at every capture before
reporting.
