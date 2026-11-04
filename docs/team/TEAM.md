# Perch Sort dev team charter (set up 2026-11-03)

## Current user execution directives (2026-11-04)
- Every feature-development effort must be decomposed into parallel tasks with non-overlapping file ownership, one shared contract and coordinator integration/review. OMP is preferred for coding; Codex is the fallback. The coordinator chooses model and reasoning level by task complexity; GPT-6.1 Sol/high is a recommendation, not a requirement for simple tasks.
- The current Codex conversation replaces Claude for this stage: it assigns tasks and accepts deliveries. Reports, questions and handoffs must return to the current Codex coordinator, never the retired Claude session.
- The user requests no code test runs for this continuation and will test the integrated build. Record skipped execution honestly; historical passing results do not validate new commits.
- The user explicitly authorizes committing and merging reviewed completed work into main for testing. Preserve unfinished work on task branches. After the current T49/T50 stage is integrated, write the handoff and pause; future development runs directly on the Mac, with 251 no longer used.


The user (Orca desktop on the Mac) wants to watch every agent conversation in the desktop app. Claude (the lead) runs on 251 and directs; **all new work runs on the Mac** in Orca-managed workspaces. The old 251 workers (omp T43/T42, Codex T45) only finish what they started.

## Roles
| Role | Agent / model | Where | Owns | Does NOT |
| --- | --- | --- | --- | --- |
| `lead` | Claude (251) | 251 | design (docs/GAME_DESIGN_V5.md), task specs, acceptance, review, merging to main, talking to the user in Chinese | write feature code |
| `mac-ui` | omp, gpt-6.1-sol | Mac, existing checkout `/Users/apple/Documents/projects/game` (Unity Library is there) | everything under `PerchSort/Assets/Scripts/**`, `Assets/Editor/**`, `Assets/Tests/**` (Unity code, HUD, screens, animation) | art generation, level data |
| `mac-art` | Codex, gpt-6-astra | Mac, own worktree `art` (never opens Unity) | all new image files (`PerchSort/Assets/Resources/**/Art`, `art/**`) | code, levels |
| `mac-levels` | omp, gpt-6.1-sol | Mac, own worktree `levels` (starts only when needed; never opens Unity) | `Resources/V5/levels.json`, `tools/levels/**` | UI code, art |
| `mac-qa` | pi (or omp) | Mac, uses the same checkout as `mac-ui` only when mac-ui is idle | running `scripts/run_tests.sh`, `V5Capture`, reporting numbers and screenshots | fixing code |
Start only the roles that have work; idle roles are not launched.

## Rules (all roles)
1. **Language**: everything agents write (code comments, commits, reports, these files) is English. Only Claude talks to the user, in Chinese.
2. **Authority**: `docs/GAME_DESIGN_V5.md` (v5.4 + HUD v6 §3.0 and §10), approved mockups `art/v6/mock/` and `art/v6/mock2/`, and the task spec you were given. If something is unclear, write the question into your status file and tell `lead`; do not guess.
3. **Git**: you work on your own branch `<role>/<topic>`, commit small, push the branch; never push to `main`, never force-push, never rewrite others' files. Lead reviews and merges. Files outside your ownership are read-only.
4. **Unity lock**: only one Unity process on the Mac at a time. Before any Unity batch run create `/Users/apple/Documents/projects/.unity-lock` containing `<role> <timestamp>` (fail if it exists and is younger than 40 minutes, or if `pgrep -f 'Unity.app/Contents/MacOS/Unity'` prints anything), and delete it when the run ends. Never kill a Unity process you did not start. Batch runs never use `-nographics` for captures.
5. **Test baseline** (Mac, Unity 6000.6.7f1): EditMode and PlayMode must stay green; report the exact numbers from `build/test-results.xml` and `build/playmode-results.xml`. Do not weaken or skip tests to make them pass.
6. **Honesty**: say what was not run or not verified. Static concept images are not Unity evidence.
7. **No spend outside the plan**: never use API keys or fallbacks that bill a separate account; never open network services; never touch anything outside the repo.

## How the team talks (shared data and memory)
- **Lead -> role**: a message in the agent's terminal (Orca send) plus the same text appended to `docs/team/inbox/<role>.md` (so it survives restarts). Always include the task spec path.
- **Role -> lead**: keep `docs/team/status/<role>.md` current (what is done, what is running, what is blocked, numbers, branch + commit) and finish every task with a final status block starting `DONE:` or `BLOCKED:`. Lead reads these files (git) and the terminal output.
- **Role -> role**: never edit another role's files. Leave a note in `docs/team/inbox/<other-role>.md` under a heading `From <role> (<time>)` and tell lead; lead relays or resolves.
- **Shared memory**: `docs/team/MEMORY.md` is written only by lead (decisions, conventions, gotchas, what the user prefers). Roles read it at the start of every task and put their own discoveries into `docs/team/lessons/<role>.md` (short bullets, newest first); lead merges useful lessons into MEMORY.md.
- Each task = one spec in `tasks/` + one branch + one status block. Keep tasks small enough to finish in one sitting.
