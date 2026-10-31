# Perch Sort — collaboration guide (Codex reads this; Claude reads CLAUDE.md with the same content)

Unity 6000.6.7f1 portrait bird-sorting puzzle game (original theme, art and text). Authoritative design: `docs/DESIGN.md`; UI spec: `docs/UI_SPEC.md`; release checklist: `docs/RELEASE_CHECKLIST.md`; past task specs and acceptance reports: `tasks/T*.md` (latest: T15b).

## Language
- Everything written for AI agents (CLAUDE.md, AGENTS.md, new task specs and reports, code comments, commit messages) is in **English**.
- Talk to the user in Chinese. Existing Chinese docs (`docs/*.md`, older `tasks/*.md`) stay as they are unless a task says otherwise.

## Machines and roles (since 2026-10-31)
| Machine | Role |
|---|---|
| **251** (Linux, 192.168.1.251, `~/Projects/game`) | Development. Claude splits tasks, writes acceptance criteria, reviews, accepts, decides; Codex writes code and produces images. **No Unity Editor** — cannot compile or run tests. |
| **Mac** (192.168.1.232, Orca environment `Mac-Image-Worker`, repo `/Users/apple/Documents/projects/game`) | Testing only: Unity compile, EditMode/PlayMode tests, screenshots. Do not edit code on the Mac. |

The Mac-side Claude from the old workflow is retired.

## Git workflow (one repo on both machines, `origin = github.com/crazyniu/game`)
1. On 251, branch from `main` as `t<number>-<short-name>`. Codex implements; Claude reviews, commits and pushes.
2. On the Mac: `git fetch && git checkout <branch> && git pull --ff-only`, then
   - `bash scripts/run_tests.sh` (runs SceneBuilder first; requires EditMode ≥101 and PlayMode ≥11 passing, zero failures)
   - `bash scripts/screenshots.sh` (output in `build/screens/flow/`)
3. 251 drives the Mac remotely; ask the user to run things by hand only when the Mac is unreachable:
   ```bash
   orca status --environment Mac-Image-Worker --json
   orca terminal create --environment Mac-Image-Worker --worktree path:/Users/apple/Documents/projects/game \
     --title Mac-Test --command "git fetch && git checkout <branch> && git pull --ff-only && bash scripts/run_tests.sh" --json
   orca terminal send --environment Mac-Image-Worker --terminal <handle> --text "<cmd>" --enter --json
   orca terminal read --environment Mac-Image-Worker --terminal <handle> --json
   ```
   - The Unity Editor must **not** have this project open on the Mac, or batchmode exits immediately with code 1. Ask the user to quit the editor; never kill it yourself.
   - `run_tests.sh` makes SceneBuilder rewrite `Assets/Scenes/Game.unity` with renumbered fileIDs only. If the branch did not intentionally change the scene, discard it afterwards: `git checkout -- PerchSort/Assets/Scenes/Game.unity`.
   - Baseline on `main` (2026-10-31): EditMode 163/163; PlayMode 14 passed, 0 failed, 8 intentionally skipped in code (Level10/20/30/40 smoke tests and other deferred items). Skips are not failures.
   - Fetching screenshots to 251 (the Mac has no passwordless SSH to 251): on the Mac start `python3 -m http.server 8765 --bind 0.0.0.0 -d build/screens/flow &`, `curl` the files into `/tmp/perch-screens/flow/` on 251, then kill the server immediately.
   - If the Mac's IP changes, edit `endpoint` in `~/.orca/orca-environments.json` on 251; the pairing credentials stay valid.
   - `build/` is gitignored; never commit test output.
4. Accepted → merge into `main` and push. Rejected → send Codex back with a concrete list of issues.
5. If Unity on the Mac rewrites `.meta` files or `ProjectSettings` (e.g. SceneBuilder, ReleaseSettings) in a way the task needs, commit those from the Mac to the same branch and push, then pull on 251, so the two sides never diverge.

## Rules
- Core (`Assets/Scripts/Core`) is pure C# with no UnityEngine references; logic and animation stay separate. Update `docs/DESIGN.md` and get the user's approval before changing Core rules or levels.
- Never modify originals under `art/`; produce derived files instead.
- Every task gets `tasks/T<n>_<name>.md` (requirements + acceptance criteria) and `tasks/T<n>_report.md` (delivery notes).
- Stay original: theme, art, names, numbers and text are our own; never copy assets or text from any existing game.
- T12 (more levels) is paused; keep 40 levels. T14 (store release) needs an account and package name from the user (`com.todo.perchsort` is a placeholder).

## Codex only
- Do only the current task dispatched by the coordinator (Claude). For anything ambiguous, use `orca orchestration ask`; do not guess or widen scope.
- 251 cannot run Unity: after writing code, self-check compile risks (namespaces, asmdef references, serialized fields) and list in the report what must be verified on the Mac.
- Do not commit to git; Claude commits after review.
