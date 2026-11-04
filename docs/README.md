# Documentation router — one current game

Start with [CURRENT_GAME](CURRENT_GAME.md), then [CODE_MAP](spec/CODE_MAP.md), the assigned task and [AGENTS](../AGENTS.md). The current game is the accepted T49/T50 implementation consolidated by T52: **Game.unity**, `Scripts/{Core,Meta,Game}`, current `Resources/{Art,Birds,World,Levels}` and `Resources/dev_options.json`. No numbered runnable forks remain.

| Need | Read | Boundary |
| --- | --- | --- |
| Current behavior and paths | [CURRENT_GAME](CURRENT_GAME.md), [CODE_MAP](spec/CODE_MAP.md) | Source facts, not a new gameplay redesign |
| Current collaboration / execution | [AGENTS](../AGENTS.md), [TEAM](team/TEAM.md), [handoff](team/HANDOFF_2026-11-04_CODEX.md) | Mac development; current Codex coordination; coordinator-owned required testing |
| Existing gameplay decisions | [design record](GAME_DESIGN.md), [Chinese gameplay record](GAMEPLAY_CN.md) | Historical filenames retained; old ownership/paths/layouts are superseded by current entry and accepted later contracts |
| Latest visual/animation delivery | [T49 report](../tasks/T49_report.md), [T50 report](../tasks/T50_report.md), linked owner reports | Source/offline approval is not timed runtime acceptance |
| Current identity cleanup | [T52C spec](../tasks/T52C_current_entry.md), [report](../tasks/T52C_report.md) | No gameplay, economy or data reset |
| Release preparation | [release checklist](RELEASE_CHECKLIST.md) | Inspect current paths/flags first; historical instructions do not grant release authority |

## Preserved research and history

The following documents remain accessible for provenance, **not as the current runtime contract**:

- [PRD](PRD.md), [RULES](spec/RULES.md), [DATA_SAVE](spec/DATA_SAVE.md), [VALIDATION](spec/VALIDATION.md): the older v4 proposal, not an assertion that today's game uses its schema, retirement rules or save protocol.
- [MECHANIC_EVIDENCE](MECHANIC_EVIDENCE.md), [SORTING_RESEARCH](SORTING_RESEARCH.md), [gameplay review](GAMEPLAY_REVIEW.md), [Cutie reference](references/CUTIE_SORT_GAMEPLAY_REFERENCE.md): historical research/adoption records; do not rewrite evidence or attribute our choices to competitors.
- [LEVEL_DESIGN](LEVEL_DESIGN.md), [LEVEL_SIMULATIONS](LEVEL_SIMULATIONS.md) and appendices: document-model layouts/replays, not the shipped current JSON or new Unity/optimality/fairness acceptance.
- [UI_SPEC](UI_SPEC.md), [MAP_PROGRESSION](MAP_PROGRESSION.md), [interaction](spec/INTERACTION_ANIMATION.md), [prompts](spec/ANIMATION_PROMPTS.md), [libraries](spec/ANIMATION_LIBRARIES.md): earlier design evidence; later accepted forest art, source and task contracts define the present implementation. A prompt is not execution authority.
- [old design archive](archive/DESIGN_ORIGINAL.md), [T16 withdrawal](archive/T16_WITHDRAWN.md), [UI history](archive/UI_SPEC_HISTORY.md), [DESIGN router](DESIGN.md) and historical task reports preserve their original context. `archive/retired/` stores non-runnable retired code/assets outside Unity.

Old “v2 current / v4 not implemented”, Claude-led development and numbered scene paths describe previous stages, not today. Version-like persistence keys remain only for user-data compatibility. Do not restore old runtime paths or create a second current specification from archived evidence.

## Testing status

The coordinator must perform serialized Mac compilation, EditMode, PlayMode and GUI acceptance **after integration**. Workers report their own NOT_RUN boundaries honestly and do not start competing Unity processes. Automatic PR CI still runs current pure-Core fixtures and configuration/workflow checks; its passing result cannot establish Unity or visual acceptance. Current source-derived expectations: 304 EditMode, 17 PlayMode, no Ignore/Explicit; see CURRENT_GAME for suite breakdown and tooling.
