# Perch Sort — collaboration guide (Codex reads this; Claude reads CLAUDE.md with the same content)

Unity 6000.6.7f1 portrait bird-home sorting puzzle. Current runtime is v2; the current documentation target is evidence-based rules4/schema4/save6, not implemented. Read [README](README.md), [AI router](docs/README.md), then [mechanic evidence](docs/MECHANIC_EVIDENCE.md) before the owning specification. [RULES](docs/spec/RULES.md) alone defines effective gameplay; [CODE_MAP](docs/spec/CODE_MAP.md) records old implementation. T16 custom patterned eggs, arbitrary-empty temporary-perch retirement, old target/sleep/crate contracts and old replay claims are withdrawn or quarantined; never restore them because old code/assets exist.

User requirements: fixed authored levels, forest-only worlds and sequential world progression (MAP_PROGRESSION), icon-only controls wherever clear (accessible names are not visible labels), regular button shapes, minimal settings and mute-only sound controls with no sliders/percentages. Animations prioritize one suitable open-source tween library; never present a fixed image as completed animation. UI_SPEC owns static dimensions; INTERACTION owns timing/input; ANIMATION_LIBRARIES owns selection/license/compatibility; prompts do not grant execution authority. The user-approved forest style and branch placement are owned solely by [UI_SPEC §4.1](docs/UI_SPEC.md#approved-forest-style). Bird redesign in that style is permitted as a design direction; existing originals remain preserved, and new production assets are not accepted or implementation-authorized by that approval. Read the owning constraints before any future asset/UI work. Current single fly sprite is not proof of animated wingbeats.

## Language
- Everything written for AI agents (CLAUDE.md, AGENTS.md, new task specs and reports, code comments, commit messages) is in **English**.
- Talk to the user in Chinese. Existing Chinese docs (`docs/*.md`, older `tasks/*.md`) stay as they are unless a task says otherwise. This user-directed Chinese PRD/spec deliverable retains its requested language; new task specifications/reports, agent entry files, code comments and commits use English.

## Document ownership and verification
- The user requests task decomposition and parallel agents for suitable independent modules. Assign non-overlapping file ownership, share one rule contract and integrate with one consistency review. This does not authorize contacting external people/services or other models.
- This PR/task is documentation and document-level gameplay simulation only. Design contracts, review completion and environment readiness do not authorize game code, runtime data, scenes or new artwork. Any future implementation requires separate explicit user authorization.
- Current explicit user instructions take precedence. PRD/level cards describe product intent; RULES owns rule semantics; CODE_MAP describes v2. DESIGN is a compatibility router, not another rule source. Plans do not silently change runtime behavior.
- Every gameplay feature needs a concrete first-party same-genre source and an explicit adoption/difference record. Source-backed mechanics may use explicitly disclosed project capacity/configuration and engineering transaction boundaries; do not treat every unmeasured numeric adaptation as an invented mechanic. R07 defines free one-branch assistance; reserve growth and economy remain pending. Never attribute these project decisions to a competitor. Every gameplay feature still requires source review; unconfirmed exact rules exit effective requirements rather than being invented. Distinguish user directives and engineering safety protocols from competitor gameplay. Separate official descriptions, actual tests and disclosed adaptations. New-level optimal proof, hidden-egg visible-information fairness and runtime acceptance need separate evidence. All old T16 replay evidence is invalid for the replacement rules. Search budget exhaustion is unknown; old v2 certificates do not prove new layouts.
- New assets are derived files, never replacements for approved originals. Verify real RGBA transparency, style, pivots/bounds, 64px/grayscale recognition, then in-context screenshots and timed runtime samples. Static frames are not proof of playable animation.
- Documentation-only edits need links, quantities and contract consistency checks. Code/data changes need the Unity workflow and new-rule replay/certificates, reporting failures, explicit skips and untested boundaries honestly.

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
   - `run_tests.sh` may make SceneBuilder rewrite `Assets/Scenes/Game.unity`. Record status/diff before running and preserve user edits. Restore that file only after confirming it had no pre-existing user changes and every resulting difference is disposable output from this run; otherwise keep it for review. Never blindly checkout over user work.
   - Baseline on `main` (2026-10-31): EditMode 163/163; PlayMode 14 passed, 0 failed, 8 intentionally skipped in code (Level10/20/30/40 smoke tests and other deferred items). Skips are not failures.
   - Prefer an existing authorized artifact/file channel for screenshots. A temporary HTTP server, especially one bound to 0.0.0.0, requires actual permission covering its directory, interfaces and lifetime before starting; stop it after transfer. This guide and documentation edits grant no authority to expose network services, a whole repository or credentials.
   - If the Mac IP changes, inspect the existing environment read-only first. Updating the Orca endpoint is a configuration mutation requiring applicable authority/permission; documentation edits do not authorize it. Do not generate or disclose pairing credentials.
   - `build/` is gitignored; never commit test output.
4. Accepted → use the existing task submission authority to commit/push an isolated branch and open a draft PR for review. Rejected → return with concrete issues. **Merging into main, deployment or release each require explicit user authorization; passing acceptance is not that authorization.**
5. Compare generated `.meta`/`ProjectSettings` changes with the pre-run record, preserving user edits. Only necessary task-generated changes that have been reviewed and authorized for submission may be committed to the same branch and pulled on 251. Never auto-include unrelated changes.

## Rules
- Core (`Assets/Scripts/Core`) is pure C# with no UnityEngine references; logic and animation stay separate. Update the owning specification (rules: `docs/spec/RULES.md`) and code-difference index, and obtain implementation authorization before changing Core rules/levels. DESIGN only routes legacy links; do not add a second rule definition.
- Never modify originals under `art/`; produce derived files instead.
- Every task gets `tasks/T<n>_<name>.md` (requirements + acceptance criteria) and `tasks/T<n>_report.md` (delivery notes).
- Stay original: theme, art, names, numbers and text are our own; never copy assets or text from any existing game.
- T12 (more levels) is paused; keep 40 levels. T14 (store release) needs an account and package name from the user (`com.todo.perchsort` is a placeholder).

## Codex only
- Do only the current task dispatched by the coordinator (Claude). For anything ambiguous, use `orca orchestration ask`; do not guess or widen scope.
- 251 cannot run Unity: after writing code, self-check compile risks (namespaces, asmdef references, serialized fields) and list in the report what must be verified on the Mac.
- Do not commit to git; Claude commits after review.
