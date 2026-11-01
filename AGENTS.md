# Bird Home — current Godot collaboration guide

Current task: migrate the current bird game from Unity to native Godot/GDScript. Migration is IN PROGRESS, not a finished mobile release. The user explicitly directed stopping other feature expansion while migrating and updating all related documentation. Read [migration status and gaps](docs/GODOT_MIGRATION.md), [document router](docs/README.md), [RULES](docs/spec/RULES.md) and the relevant owning contract. `godot/project.godot` is the active project; `PerchSort/` and Unity tools remain legacy/reference only. Do not run Unity workflows as current verification or describe this migration as merely replacing artwork.

## Execution and parallel ownership

- Cloud first: the available cloud Godot 4.6.3 headless environment is the primary implementation/test executor. Use the user's local Godot 4.7.2 only as fallback if the cloud cannot perform the needed check; record the actual blocker and version. Do not call the local computer by default. Version/environment availability is not proof the migrated game tests pass.
- Never connect/read/write/inspect/sync/transfer to251 or recover its old data. Leave it untouched. No alternative channel may bypass this prohibition. Independent Player/window/OS permissions remain independent; engine migration is not permission to reuse a denied operation.
- Multiple agents work in parallel with disjoint file ownership: core/session, persistence, scene/assets, docs and root scripts/CI have separate owners. One integrator resolves shared contracts and runs aggregate checks. Do not edit another owner's paths or overwrite dirty/user files. Record baseline/head and actual changed files; uncommitted work is not a submitted build.
- Feature expansion is stopped by the user's explicit instruction, not an autonomous reprioritization. Preserve approved shop/skin/skills/settings designs and data; do not implement new commercial systems, generate replacement art or broaden levels during framework migration.

## Submission and current workflow

The user authorizes submitting and merging the framework migration after it is actually complete and necessary review/CI pass. The coordinator/integrator owns commit/push/merge; delegated agents must not independently commit or push unless assigned. Respect branch protections; do not approve a PR on the user's behalf, bypass checks or deploy/release. If merge requirements or export prerequisites are blocked, report exactly what remains rather than calling the task complete.

Use branch `t30-godot-migration` from the assigned baseline; preserve unrelated work. Active entry and test commands are in GODOT_MIGRATION; inspect actual scripts before invoking them. Run native Godot import/parser, core/session/persistence tests and scene smoke checks cloud-first; report exit code, checks, failures/skips and tested revision. Rendering/input/audio/device acceptance are separate from headless tests. Android/iOS templates, SDKs, signing and real package/device tests must be evidenced separately. Do not claim mobile packages passed because Linux native startup passed.

Do not force-close user applications. Preserve dirty scenes/assets/configuration. Runtime outputs and import caches are not source deliverables. Use only authorized artifact channels; no temporary HTTP server, endpoint mutation or credential disclosure is implied. Test retries must preserve original failure evidence and never fabricate green CI.

## Architecture and rules

- Native GDScript core extends `RefCounted`, detached from scene tree, nodes, rendering and audio. Session manages candidate transactions, Undo/Restart and commit ordering. Presentation consumes committed events; Tween callbacks do not decide gameplay or save success. No active C#/UnityEngine/asmdef dependency is required.
- Fixed L01 stays N4/K2/P3, S1=AAA, S2=BBBA, S3=B (3/4/1 birds). The current migration scope ports existing behavior, not all40 design levels, eggs or shop skills. RULES owns semantics; CODE_MAP and GODOT_MIGRATION distinguish implemented work from intent. Retire only completed branches, preserve token order/identity and atomic Undo/Restart.
- Godot's built-in open-source Tween replaces LitMotion for current animation. Reuse approved six-frame/standing/selected bird assets, forest and nest sources; fading retired branch/root is one presentation unit. Real motion must be tested, not inferred from a poster. Preserve originals under `art/` and all approved three-page visual references; accepted originals are not redrawn or approximated.
- Native Godot save version1 is a separate binary format using object-disabled Variant serialization, not Unity save6/save7. Never overwrite/import old Unity saves by assumption. DATA_SAVE owns the isolation/recovery contract; unknown/corrupt saves are preserved.
- New gameplay still needs source/adaptation evidence, fixed content and fresh engine-specific validation. Historical Unity/paper certificates do not prove Godot behavior. Schema/rule identity and task acceptance requirements remain, but engine bindings and test commands are now Godot-native.

## Language, UI and assets

Agent-facing specs, code comments, commits and reports are English. All player-visible UI is English using existing licensed Fredoka/verified game-font assets; retain i18n structure and current color variety. Do not reinterpret ambiguous color wording as permission to remove colors or a confirmed multilingual feature. User conversation stays Chinese unless separately changed.

Follow reference-led top utilities/coin, bottom help items, centered settings and full shop layout; no invented sidebar/drawer. Three approved page directions/layouts remain valid with faithful English copy, not a wholesale redraw. Production slices, safe touch regions, RGBA/pivots/bounds,64px/grayscale readability and actual motion/audio still need their scoped acceptance. Settings' two-switch design remains distinct from current migrated audio behavior until implemented/tested; no hidden expansion.

Each task keeps its requirements/report identity and states actual commands/results/limits. Historical tasks and archive are evidence only; they cannot reactivate Unity,251 access, old temporary-perch rules or withdrawn target/egg/star policies. Never merge or deploy unrelated projects under this migration authority.
