# Documentation router — one current game

Start with [CURRENT_GAME](CURRENT_GAME.md), then [CODE_MAP](spec/CODE_MAP.md), the [port contract](work-log/godot-port/spec.md) and [AGENTS](../AGENTS.md). The current game is the Godot 4.7.2 project in `PerchSort/` (`core`, `meta`, `game`, assets, `levels/levels.json`). The former Unity project lives only in Git history.

| Need | Read | Boundary |
| --- | --- | --- |
| Project conventions | [CONVENTIONS](CONVENTIONS.md) | Binding naming, layout and verification rules |
| Current behavior and paths | [CURRENT_GAME](CURRENT_GAME.md), [CODE_MAP](spec/CODE_MAP.md) | Source facts, not a new gameplay redesign |
| Current collaboration / execution | [AGENTS](../AGENTS.md) | Claude develops; Codex paints art and may help test |
| Existing gameplay decisions | [design record](GAME_DESIGN.md), [Chinese gameplay record](GAMEPLAY_CN.md) | Canonical filenames; old ownership/paths/layouts are superseded by current entry and accepted later contracts |
| Unity-era visual/animation records | [forest assets](work-log/forest-assets/report.md), [interface runtime](work-log/interface-runtime/report.md) | History; describes the former Unity implementation |
| Release preparation | [release checklist](RELEASE_CHECKLIST.md) | Inspect current paths/flags first; historical instructions do not grant release authority |

## Preserved research and history

The following documents remain accessible for provenance, **not as the current runtime contract**:

- [PRD](PRD.md), [RULES](spec/RULES.md), [DATA_SAVE](spec/DATA_SAVE.md), [VALIDATION](spec/VALIDATION.md): the older v4 proposal, not an assertion that today's game uses its schema, retirement rules or save protocol.
- [MECHANIC_EVIDENCE](MECHANIC_EVIDENCE.md), [SORTING_RESEARCH](SORTING_RESEARCH.md), [gameplay review](GAMEPLAY_REVIEW.md), [Cutie reference](references/CUTIE_SORT_GAMEPLAY_REFERENCE.md): historical research/adoption records; do not rewrite evidence or attribute our choices to competitors.
- [LEVEL_DESIGN](LEVEL_DESIGN.md), [LEVEL_SIMULATIONS](LEVEL_SIMULATIONS.md) and appendices: document-model layouts/replays, not the shipped current JSON or new Unity/optimality/fairness acceptance.
- [UI_SPEC](UI_SPEC.md), [MAP_PROGRESSION](MAP_PROGRESSION.md), [interaction](spec/INTERACTION_ANIMATION.md), [prompts](spec/ANIMATION_PROMPTS.md), [libraries](spec/ANIMATION_LIBRARIES.md): earlier design evidence; later accepted forest art, source and task contracts define the present implementation. A prompt is not execution authority.
- [old design archive](archive/DESIGN_ORIGINAL.md), [T16 withdrawal](archive/BIRD_HOME_WITHDRAWN.md), [UI history](archive/UI_SPEC_HISTORY.md), [DESIGN router](DESIGN.md) and historical task reports preserve their original context. Retired implementation and superseded art trees were removed; Git retains their history.

Old “v2 current / v4 not implemented”, Claude-led development and numbered scene paths describe previous stages, not today. Version-like persistence keys remain only for user-data compatibility. Do not restore old runtime paths or create a second current specification from archived evidence.

## Testing status

`bash scripts/run_tests.sh` (naming guard, smoke test, 40-level core test) and `bash scripts/screenshots.sh` (silent captures of every screen state). Stills do not prove animation; timed claims need frame sequences and logs. CI runs the same tests on headless Godot and cannot establish GUI acceptance.
