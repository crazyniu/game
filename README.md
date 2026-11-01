# Bird Home · Godot migration

The active project is **`godot/project.godot`**, native GDScript. The user requested switching the current bird game from Unity to Godot and stopping other feature expansion while the framework is migrated. **Migration is IN PROGRESS.** See [current status/gap matrix](docs/GODOT_MIGRATION.md); do not infer completed/mobile-tested status from the new directory.

Cloud Godot4.6.3 is the primary test environment; local Godot4.7.2 is fallback only if cloud testing cannot proceed. No251 access or transfer. Unity `PerchSort/`, scripts and historical reports remain reference material, not the active entry or current test workflow.

## Scope

Port the fixed two-transfer L01 (N4/K2/P3, AAA/BBBA/B), native rules/session/Undo/Restart, isolated persistence, existing six-frame birds/selection/flight/Home/retired branch-root fade, English/Fredoka UI and audio. Godot native Tween replaces LitMotion. Native save version1 does not overwrite Unity save6. The other39 levels and economy/help-skill designs are not thereby implemented. Approved shop/skill/settings images are preserved, not redrawn or newly expanded.

## Read and run

- [AGENTS](AGENTS.md): cloud-first execution, ownership, migration-only merge authority and boundaries.
- [Docs router](docs/README.md) → [Migration](docs/GODOT_MIGRATION.md) → [Rules](docs/spec/RULES.md).
- [Godot code map](docs/spec/CODE_MAP.md), [save contract](docs/spec/DATA_SAVE.md), [animation](docs/spec/ANIMATION_LIBRARIES.md), [validation](docs/spec/VALIDATION.md).
- [Product](docs/PRD.md), [fixed levels](docs/LEVEL_SIMULATIONS.md), [UI approvals](docs/UI_SPEC.md), [design-only economy](docs/spec/ECONOMY_SHOP.md).

From the repository root, use `godot --headless --path godot --editor --quit` to import/parse, then `GODOT_BIN=godot bash scripts/run_tests.sh` as documented in GODOT_MIGRATION; `godot --path godot` runs the main scene when display access is authorized. Commands are workflow instructions, not pass claims. Exact aggregate/CI commands and outcomes are recorded by the integrator. No Unity Editor/TestRunner is needed for active migration verification.

The user authorized submitting/merging the completed migration after necessary review/CI. No deployment, bypassed protections or automatic approval on the user's behalf. Native/headless checks do not certify signed Android/iOS packages, actual mobile input or final visual/audio acceptance.
