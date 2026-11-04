# T54 shared contract (coordinator: Claude on 251; workers: OMP on the Mac, base `origin/main` bf5e6ea)

User feedback after testing main in the Unity Editor (2026-11-04), each owned by exactly one task:
- P1 Level start animation is far too fast (whole entrance is 1.6 s, each bird flight about 0.6 s). -> T54B
- P2 No background music at all in the game. `Resources/Audio/bgm_game.wav` and `bgm_menu.wav` exist and `AudioManager.SwitchMusic` exists, but no game
  code ever starts music (GameRoot only restores a previous page). -> T54A
- P3 Birds on branches look static and fake: idle is rare (1.5-5 s gaps), tiny (rotation 8 deg), pose swaps are abrupt. Must feel alive continuously. -> T54C
- P4 The three bottom boosters are visibly different sizes: a disabled button is scaled to 0.9 and faded, so undo looks smaller. -> T54D
- P5 Version-number names (v5/v6/v7, V5LevelTool, `art/v7`, `ui7_*`, `archive/retired`) still exist; there is only ONE version of this game. -> T54E

## Hard rules for every task
1. Own only the files listed for your task. Anything else is read-only; put needed changes under "Requested contract change" in your report.
2. English only for code, comments, commits, reports. No emoji. No new V-numbered names of any kind (files, folders, classes, namespaces, assets, scenes, docs).
3. Animation or audio claims need TIMED evidence, not one static frame. For animation: capture a frame sequence at fixed intervals from real Unity
   (e.g. every 0.1 s for the first 5 s, plus 20 s of idle on level 1) and commit a contact sheet JPEG (max 1600 px wide) under `tasks/evidence/T54<letter>/`.
   For audio: log which clip plays on which page and show it in a PlayMode test with the real AudioManager. The coordinator looks at the sheets.
4. Do not weaken or skip existing tests. If an existing test fails only because you intentionally changed a timing/size it asserts, do not edit it;
   list it in the report with the old and new value and the reason. Add new tests only in NEW files named `PerchSort/Assets/Tests/{EditMode,PlayMode}/T54<letter>*.cs`.
5. Unity runs: serialize with `flock /tmp/unity.lock`; never run two Unity processes; never kill the user's Editor. Run
   `flock /tmp/unity.lock bash scripts/run_tests.sh` once before reporting and give exact EditMode/PlayMode counts (baseline on main: 304 and 21, all passing).
6. Work in your own worktree on branch `mac-t54<letter>/t54` created from `origin/main`. Commit small, push, write `tasks/T54<letter>_report.md`
   (change per problem, evidence paths, test counts, limits honestly), push, and state the final SHA in your last terminal message. Never merge, never touch main.
7. Do not generate or edit artwork. If an asset is missing, stop that item and report the asset name.

## Feel targets (the coordinator accepts only against these)
- Level entrance (T54B): total 3.5-4.5 s from tap to playable. Order: top houses drop in with a soft bounce (0.8 s), branches grow trunk-to-tip staggered
  per row (0.7 s each, 0.15 s apart), birds then fly in one after another (0.08-0.12 s stagger, each flight 1.0-1.4 s with ease-out landing and a small
  squash on touch-down). Tap skips. Input unlocks only at the end. Honour `GameMotion.Instant`.
- Music (T54A): menu/map page plays `bgm_menu`, level page plays `bgm_game`, crossfade about 1 s, loops, follows the existing mute control (mute-only,
  no sliders), pauses on focus loss, resumes after popups, never restarts on every popup, starts on first user interaction on platforms that require it.
- Idle life (T54C): every perched bird always has a subtle breathing/bob loop (about 2-3 s period, 1.5-2.5 % scale or 2-3 px) with a per-bird random phase;
  on top, random actions fire every 0.8-2.5 s per bird: blink, head tilt, look around, preen, fluff, stretch, hop-in-place, small side shuffle. Actions use
  smooth easing in AND out (no pose pops), amplitudes visible at phone size (tilt 8-14 deg, hop 8-14 px), at most 3 birds acting at once per branch,
  neighbours never sync. Selected, flying, sleeping and egg states behave as today. No motion while `GameMotion.Instant`.
- Buttons (T54D): all bottom tool buttons are identical in size in every state (rest, pressed feedback only briefly, disabled). Disabled means dimmed
  (alpha) and non-interactive, never smaller. Badge position/size identical per button. The same rule holds for every `GameButton`.
