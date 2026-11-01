# Historical Unity entrypoints before T30

HISTORICAL ONLY. Superseded by the user-authorized Godot migration. None of the commands, engine priorities, machine roles or approval restrictions below overrides current AGENTS/GODOT_MIGRATION. No251 access is permitted.

## Historical AGENTS.md

```text
# Perch Sort — collaboration guide (Codex reads this; Claude reads CLAUDE.md with the same content)

Unity 6000.6.7f1 portrait bird-home sorting puzzle. The current feature branch includes the isolated `PerchSort/Assets/Scenes/FirstLevel.unity`, first-level HUD, six-frame flight/wingbeat assets, Home/nest-entry presentation and the first-level persistence integration. The legacy v2 `Game.unity` remains the default entry; later levels are not migrated. T27/T28 selection-motion and Player-acceptance work is still unsubmitted and has not been run/accepted; do not describe it as completed. Current Player visual, touch and audio acceptance remains pending. Named earlier Core/dependency/test checkpoints belong in the [T21 report](tasks/T21_report.md) and subsequent scoped reports, not a claim that the branch still contains only the old Core foundation. Full campaign migration and economy/help-skill runtime remain incomplete. Read [README](README.md), [AI router](docs/README.md), then [mechanic evidence](docs/MECHANIC_EVIDENCE.md) before the owning specification. [RULES](docs/spec/RULES.md) alone defines effective gameplay; [CODE_MAP](docs/spec/CODE_MAP.md) records old implementation. T16 custom patterned eggs, arbitrary-empty temporary-perch retirement, old target/sleep/crate contracts and old replay claims are withdrawn or quarantined; never restore them because old code/assets exist.

User requirements: fixed authored levels, forest-only worlds and sequential world progression (MAP_PROGRESSION), production icon/shape baseline subject to the scoped design-exploration exception in UI_SPEC, minimal settings and mute-only sound controls with no sliders/percentages. Animations prioritize one suitable open-source tween library; never present a fixed image as completed animation. UI_SPEC owns static dimensions; INTERACTION owns timing/input; ANIMATION_LIBRARIES owns selection/license/compatibility; prompts do not grant execution authority. The user-approved forest style and branch placement are owned solely by [UI_SPEC §4.1](docs/UI_SPEC.md#approved-forest-style). Bird redesign in that style is permitted as a design direction; existing originals remain preserved, and new production assets are not accepted or implementation-authorized by that approval. Read the owning constraints before any future asset/UI work. The existing first-level slice has six-frame wingbeat presentation; this does not establish final Player visual/touch/audio acceptance or acceptance of the new design concepts.

## Language
- Everything written for AI agents (CLAUDE.md, AGENTS.md, new task specs and reports, code comments, commit messages) is in **English**.
- All player-visible game UI is English, including titles, buttons, shop, skills, settings, tutorials and status/error text. Use the existing licensed game font (Fredoka or the verified repository font) for titles/entry labels/buttons; never substitute unreviewed system-font text and call it approved artwork. Keep the existing i18n structure and approved visual layouts.
- Continue talking to the user in Chinese unless they separately request a conversation-language change. Existing historical Chinese docs/reports are preserved; new agent-facing work is English. The request for English game UI is not permission to delete localization support.
- Preserve the current multiple bird colors/palette while the phrase about retaining extra colors is unresolved. Do not remove colors or interpret it as a confirmed multilingual feature request.

## Document ownership and verification
- The user requests task decomposition and parallel agents for suitable independent modules. Assign non-overlapping file ownership, share one rule contract and integrate with one consistency review. This does not authorize contacting external people/services or other models.
- Current user authority covers T21 first-level implementation and ordinary design decisions. The adopted L01 tutorial is N4/K2/P3, S1=AAA, S2=BBBA, S3=B, no empty branches; only this campaign/level gets the RULES tutorial-profile exception. The existing first-level slice uses one upper receiving nest and staggered, environmentally supported branches; the new side-trunk reception-area proposal is not yet visually selected; exact layout/resources remain subject to internal technical and independent visual QA before integration. Ordinary design choices are delegated to the team; do not wait for repeated user confirmation. This is not a blanket asset-QA bypass. Later-level migration, unrelated fixes, payment integration, merging and deployment are outside this scope.
- Current explicit user instructions take precedence. PRD/level cards describe product intent; RULES owns rule semantics; CODE_MAP distinguishes production v2 from the isolated T21 foundation. DESIGN is a compatibility router, not another rule source. Plans do not silently change runtime behavior.
- Every gameplay feature needs a concrete first-party same-genre source and an explicit adoption/difference record. Source-backed mechanics may use explicitly disclosed project capacity/configuration and engineering transaction boundaries; do not treat every unmeasured numeric adaptation as an invented mechanic. R07 defines free one-branch assistance; reserve growth remains pending; economy/help skills are now designed in ECONOMY_SHOP, not implemented. Never attribute these project decisions to a competitor. Every gameplay feature still requires source review; unconfirmed exact rules exit effective requirements rather than being invented. Distinguish user directives and engineering safety protocols from competitor gameplay. Separate official descriptions, actual tests and disclosed adaptations. New-level optimal proof, hidden-egg visible-information fairness and runtime acceptance need separate evidence. All old T16 replay evidence is invalid for the replacement rules. Search budget exhaustion is unknown; old v2 certificates do not prove new layouts.
- New assets are derived files, never replacements for approved originals. Verify real RGBA transparency, style, pivots/bounds, 64px/grayscale recognition, then in-context screenshots and timed runtime samples. Static frames are not proof of playable animation.
- Documentation-only edits need links, quantities and contract consistency checks. Code/data changes need the Unity workflow and new-rule replay/certificates, reporting failures, explicit skips and untested boundaries honestly.

## Current execution model: local computer + cloud (2026-11-01)

The user's latest direction supersedes the former 251-development/Mac-test-only workflow. The user's local computer may produce code and derived assets and run authorized Unity checks. Cloud agents handle documentation, design preparation, independent review and explicitly assigned code work in parallel. This changes where authorized work may occur; it does not expand the approved feature scope or override tool/runtime permissions.

**251 is retired from this workflow. Do not connect to it or perform any read, write, inspection, sync, transfer or remote command involving it.** Do not reconnect to recover files or migrate its old data. Leave its existing data untouched. Old reports preserve history, not permission to reuse that route. Do not invent a new endpoint or use a different channel to bypass this prohibition.

| Executor | Current role |
| --- | --- |
| User's local computer | Authorized code/derived-asset production and Unity compilation/tests; direct local collaboration, not a test-only machine. Inspect the actual available environment and checkout before work. |
| Cloud workspace | Parallel specs/design preparation, independent review and assigned authorized code; no claim that a local path exists remotely or that Unity is installed here. |
| 251 | No access or transfer. Historical data remains where it is; not a migration source. |

Use multiple agents for independent modules with disjoint file ownership. Each assignment states baseline commit, owned paths, shared rule/API/asset contract, acceptance boundary and integration owner. Do not let two agents edit the same scene, prefab, catalog, save schema or other shared file concurrently. One integrator reconciles changes and verifies the combined result. Uncommitted local work is not submitted work; record modified paths and actual commit/test evidence honestly. Coordinate blockers without silently pausing or reprioritizing already authorized development.

## Current Git and verification workflow (`origin = github.com/crazyniu/game`)

1. On the authorized local or cloud checkout, inspect branch/head and dirty status first. Continue the coordinator's assigned branch; for a new approved task use the project `t<number>-<short-name>` convention from its explicitly selected baseline. Preserve unrelated user changes. Do not switch a dirty worktree or overwrite newer work merely to match an old report.
2. Each agent implements only its assigned files. The integrator reviews the diff, resolves shared-contract changes and submits through the existing coordinator/role authority. This execution change does not independently authorize a commit, push, merge or deployment outside the active task's permissions. Accepted submission remains an isolated branch/draft PR; no automatic merge/release.
3. On the user's local computer, use the existing Unity workflow when authorized: `bash scripts/run_tests.sh` and, where capture is authorized, `bash scripts/screenshots.sh`. Check the current scripts and task-specific acceptance criteria first; historical test totals are not a current pass. Report pass/fail/skip/not-run separately. Cloud code changes still require appropriate integration and local Unity verification when cloud Unity is unavailable.
4. Do not forcibly close the user's Unity Editor. If it has the project open and blocks batchmode, ask the user to close it or use an already authorized compatible workflow. Record dirty state before tools that may regenerate scenes; restore generated changes only after confirming the preimage was clean and every discarded change is disposable output. Preserve user scenes, `.meta` and ProjectSettings edits.
5. Player/window access, capture and other runtime permissions remain independent. The new local-production direction does not approve a previously denied Player-window action or transfer. Use only currently authorized tools/file channels. No temporary HTTP exposure, pairing/endpoint mutation or credential disclosure is implied. Never route any artifact through 251.
6. Only include reviewed, task-owned changes in a submission. Keep `build/` output untracked. Verify remote head before reporting a push and report the exact tested commit; do not present a local modification, mocked screen or historical test report as a new runtime pass.

## Rules
- Core (`Assets/Scripts/Core`) is pure C# with no UnityEngine references; logic and animation stay separate. Update the owning specification (rules: `docs/spec/RULES.md`) and code-difference index, and obtain implementation authorization before changing Core rules/levels. DESIGN only routes legacy links; do not add a second rule definition.
- Never modify originals under `art/`; produce derived files instead.
- Every task gets `tasks/T<n>_<name>.md` (requirements + acceptance criteria) and `tasks/T<n>_report.md` (delivery notes).
- Stay original: theme, art, names, numbers and text are our own; never copy assets or text from any existing game.
- T12 (more levels) is paused; keep 40 levels. T14 (store release) needs an account and package name from the user (`com.todo.perchsort` is a placeholder).

## Codex only
- Do only the current task dispatched by the coordinator. Resolve ambiguity through the active task/coordinator channel; do not require a legacy remote orchestration route or widen scope.
- Self-check compile risks (namespaces, asmdef references, serialized fields); run authorized Unity verification on the local computer, or report it NOT_RUN when unavailable. Cloud code is not runtime-accepted without the required integration checks. Never use 251 for verification or retrieval.
- Do not commit to git; Claude commits after review.

## Current shop/help-skill design scope (2026-11-01)

The user authorizes autonomous completion of economy, shop, help-skill specifications and design exploration. [ECONOMY_SHOP](docs/spec/ECONOMY_SHOP.md) is the current design contract: coin-only skins, four help skills/three slots, purchase inventory and permanent upgrades, completion coins and voluntary ad doubling. Undo/Restart stay free and outside slots; the previous unlimited-Hint/noncommercial-design restriction is superseded by the explicit free-per-attempt/inventory policy. Initial tuning needs no repeated approval. Runtime remains DESIGN_ONLY: no live ad SDK, IAP prices/integration, merge or deployment is authorized by these documents. Current save6 code is not future save7.

For this design exploration only, the latest user permits necessary functional text and alternatives to existing UI appearance constraints. No mandatory thick-wood button base may block exploration. This is not a global cancellation of production readability, accepted-art preservation or asset QA. The skin-shop, help-skills and settings page directions/layouts are individually approved as recorded in UI_SPEC §11; their faithful English copy update is permitted without a wholesale redraw. Other visual selection and production slices remain pending; side-trunk reception is a proposal, not accepted coordinates. Do not overwrite T21’s existing first-level implementation or claim it does not exist. Shuffle is player-initiated, uses authored deterministic candidate plans, preserves authored initial layouts and needs new help-policy/candidate certificates; old witnesses do not prove the new skills.

## Reference-led component layout

Follow the user's photographed same-genre reference and established component placement rather than inventing a new navigation pattern: top utilities/coin balance, bottom help-item row, centered settings modal and full shop view. Preserve the three approved page layouts; English copy/font binding does not authorize wholesale repainting or replacing approved slices. Do not introduce a sidebar/drawer layout. Necessary functional text is permitted; approved-art production gates remain unchanged. UI_SPEC owns these current details.

```

## Historical CLAUDE.md

```text
# Perch Sort — collaboration guide (Claude reads this; Codex reads AGENTS.md with the same content)

Unity 6000.6.7f1 portrait bird-home sorting puzzle. The current feature branch includes the isolated `PerchSort/Assets/Scenes/FirstLevel.unity`, first-level HUD, six-frame flight/wingbeat assets, Home/nest-entry presentation and the first-level persistence integration. The legacy v2 `Game.unity` remains the default entry; later levels are not migrated. T27/T28 selection-motion and Player-acceptance work is still unsubmitted and has not been run/accepted; do not describe it as completed. Current Player visual, touch and audio acceptance remains pending. Named earlier Core/dependency/test checkpoints belong in the [T21 report](tasks/T21_report.md) and subsequent scoped reports, not a claim that the branch still contains only the old Core foundation. Full campaign migration and economy/help-skill runtime remain incomplete. Read [README](README.md), [AI router](docs/README.md), then [mechanic evidence](docs/MECHANIC_EVIDENCE.md) before the owning specification. [RULES](docs/spec/RULES.md) alone defines effective gameplay; [CODE_MAP](docs/spec/CODE_MAP.md) records old implementation. T16 custom patterned eggs, arbitrary-empty temporary-perch retirement, old target/sleep/crate contracts and old replay claims are withdrawn or quarantined; never restore them because old code/assets exist.

User requirements: fixed authored levels, forest-only worlds and sequential world progression (MAP_PROGRESSION), production icon/shape baseline subject to the scoped design-exploration exception in UI_SPEC, minimal settings and mute-only sound controls with no sliders/percentages. Animations prioritize one suitable open-source tween library; never present a fixed image as completed animation. UI_SPEC owns static dimensions; INTERACTION owns timing/input; ANIMATION_LIBRARIES owns selection/license/compatibility; prompts do not grant execution authority. The user-approved forest style and branch placement are owned solely by [UI_SPEC §4.1](docs/UI_SPEC.md#approved-forest-style). Bird redesign in that style is permitted as a design direction; existing originals remain preserved, and new production assets are not accepted or implementation-authorized by that approval. Read the owning constraints before any future asset/UI work. The existing first-level slice has six-frame wingbeat presentation; this does not establish final Player visual/touch/audio acceptance or acceptance of the new design concepts.

## Language
- Everything written for AI agents (CLAUDE.md, AGENTS.md, new task specs and reports, code comments, commit messages) is in **English**.
- All player-visible game UI is English, including titles, buttons, shop, skills, settings, tutorials and status/error text. Use the existing licensed game font (Fredoka or the verified repository font) for titles/entry labels/buttons; never substitute unreviewed system-font text and call it approved artwork. Keep the existing i18n structure and approved visual layouts.
- Continue talking to the user in Chinese unless they separately request a conversation-language change. Existing historical Chinese docs/reports are preserved; new agent-facing work is English. The request for English game UI is not permission to delete localization support.
- Preserve the current multiple bird colors/palette while the phrase about retaining extra colors is unresolved. Do not remove colors or interpret it as a confirmed multilingual feature request.

## Document ownership and verification
- The user requests task decomposition and parallel agents for suitable independent modules. Assign non-overlapping file ownership, share one rule contract and integrate with one consistency review. This does not authorize contacting external people/services or other models.
- Current user authority covers T21 first-level implementation and ordinary design decisions. The adopted L01 tutorial is N4/K2/P3, S1=AAA, S2=BBBA, S3=B, no empty branches; only this campaign/level gets the RULES tutorial-profile exception. The existing first-level slice uses one upper receiving nest and staggered, environmentally supported branches; the new side-trunk reception-area proposal is not yet visually selected; exact layout/resources remain subject to internal technical and independent visual QA before integration. Ordinary design choices are delegated to the team; do not wait for repeated user confirmation. This is not a blanket asset-QA bypass. Later-level migration, unrelated fixes, payment integration, merging and deployment are outside this scope.
- Current explicit user instructions take precedence. PRD/level cards describe product intent; RULES owns rule semantics; CODE_MAP distinguishes production v2 from the isolated T21 foundation. DESIGN is a compatibility router, not another rule source. Plans do not silently change runtime behavior.
- Every gameplay feature needs a concrete first-party same-genre source and an explicit adoption/difference record. Source-backed mechanics may use explicitly disclosed project capacity/configuration and engineering transaction boundaries; do not treat every unmeasured numeric adaptation as an invented mechanic. R07 defines free one-branch assistance; reserve growth remains pending; economy/help skills are now designed in ECONOMY_SHOP, not implemented. Never attribute these project decisions to a competitor. Every gameplay feature still requires source review; unconfirmed exact rules exit effective requirements rather than being invented. Distinguish user directives and engineering safety protocols from competitor gameplay. Separate official descriptions, actual tests and disclosed adaptations. New-level optimal proof, hidden-egg visible-information fairness and runtime acceptance need separate evidence. All old T16 replay evidence is invalid for the replacement rules. Search budget exhaustion is unknown; old v2 certificates do not prove new layouts.
- New assets are derived files, never replacements for approved originals. Verify real RGBA transparency, style, pivots/bounds, 64px/grayscale recognition, then in-context screenshots and timed runtime samples. Static frames are not proof of playable animation.
- Documentation-only edits need links, quantities and contract consistency checks. Code/data changes need the Unity workflow and new-rule replay/certificates, reporting failures, explicit skips and untested boundaries honestly.

## Current execution model: local computer + cloud (2026-11-01)

The user's latest direction supersedes the former 251-development/Mac-test-only workflow. The user's local computer may produce code and derived assets and run authorized Unity checks. Cloud agents handle documentation, design preparation, independent review and explicitly assigned code work in parallel. This changes where authorized work may occur; it does not expand the approved feature scope or override tool/runtime permissions.

**251 is retired from this workflow. Do not connect to it or perform any read, write, inspection, sync, transfer or remote command involving it.** Do not reconnect to recover files or migrate its old data. Leave its existing data untouched. Old reports preserve history, not permission to reuse that route. Do not invent a new endpoint or use a different channel to bypass this prohibition.

| Executor | Current role |
| --- | --- |
| User's local computer | Authorized code/derived-asset production and Unity compilation/tests; direct local collaboration, not a test-only machine. Inspect the actual available environment and checkout before work. |
| Cloud workspace | Parallel specs/design preparation, independent review and assigned authorized code; no claim that a local path exists remotely or that Unity is installed here. |
| 251 | No access or transfer. Historical data remains where it is; not a migration source. |

Use multiple agents for independent modules with disjoint file ownership. Each assignment states baseline commit, owned paths, shared rule/API/asset contract, acceptance boundary and integration owner. Do not let two agents edit the same scene, prefab, catalog, save schema or other shared file concurrently. One integrator reconciles changes and verifies the combined result. Uncommitted local work is not submitted work; record modified paths and actual commit/test evidence honestly. Coordinate blockers without silently pausing or reprioritizing already authorized development.

## Current Git and verification workflow (`origin = github.com/crazyniu/game`)

1. On the authorized local or cloud checkout, inspect branch/head and dirty status first. Continue the coordinator's assigned branch; for a new approved task use the project `t<number>-<short-name>` convention from its explicitly selected baseline. Preserve unrelated user changes. Do not switch a dirty worktree or overwrite newer work merely to match an old report.
2. Each agent implements only its assigned files. The integrator reviews the diff, resolves shared-contract changes and submits through the existing coordinator/role authority. This execution change does not independently authorize a commit, push, merge or deployment outside the active task's permissions. Accepted submission remains an isolated branch/draft PR; no automatic merge/release.
3. On the user's local computer, use the existing Unity workflow when authorized: `bash scripts/run_tests.sh` and, where capture is authorized, `bash scripts/screenshots.sh`. Check the current scripts and task-specific acceptance criteria first; historical test totals are not a current pass. Report pass/fail/skip/not-run separately. Cloud code changes still require appropriate integration and local Unity verification when cloud Unity is unavailable.
4. Do not forcibly close the user's Unity Editor. If it has the project open and blocks batchmode, ask the user to close it or use an already authorized compatible workflow. Record dirty state before tools that may regenerate scenes; restore generated changes only after confirming the preimage was clean and every discarded change is disposable output. Preserve user scenes, `.meta` and ProjectSettings edits.
5. Player/window access, capture and other runtime permissions remain independent. The new local-production direction does not approve a previously denied Player-window action or transfer. Use only currently authorized tools/file channels. No temporary HTTP exposure, pairing/endpoint mutation or credential disclosure is implied. Never route any artifact through 251.
6. Only include reviewed, task-owned changes in a submission. Keep `build/` output untracked. Verify remote head before reporting a push and report the exact tested commit; do not present a local modification, mocked screen or historical test report as a new runtime pass.

## Rules
- Core (`Assets/Scripts/Core`) is pure C# with no UnityEngine references; logic and animation stay separate. Update the owning specification (rules: `docs/spec/RULES.md`) and code-difference index, and obtain implementation authorization before changing Core rules/levels. DESIGN only routes legacy links; do not add a second rule definition.
- Never modify originals under `art/`; produce derived files instead.
- Every task gets `tasks/T<n>_<name>.md` (requirements + acceptance criteria) and `tasks/T<n>_report.md` (delivery notes).
- Stay original: theme, art, names, numbers and text are our own; never copy assets or text from any existing game.
- T12 (more levels) is paused; keep 40 levels. T14 (store release) needs an account and package name from the user (`com.todo.perchsort` is a placeholder).

## Current shop/help-skill design scope (2026-11-01)

The user authorizes autonomous completion of economy, shop, help-skill specifications and design exploration. [ECONOMY_SHOP](docs/spec/ECONOMY_SHOP.md) is the current design contract: coin-only skins, four help skills/three slots, purchase inventory and permanent upgrades, completion coins and voluntary ad doubling. Undo/Restart stay free and outside slots; the previous unlimited-Hint/noncommercial-design restriction is superseded by the explicit free-per-attempt/inventory policy. Initial tuning needs no repeated approval. Runtime remains DESIGN_ONLY: no live ad SDK, IAP prices/integration, merge or deployment is authorized by these documents. Current save6 code is not future save7.

For this design exploration only, the latest user permits necessary functional text and alternatives to existing UI appearance constraints. No mandatory thick-wood button base may block exploration. This is not a global cancellation of production readability, accepted-art preservation or asset QA. The skin-shop, help-skills and settings page directions/layouts are individually approved as recorded in UI_SPEC §11; their faithful English copy update is permitted without a wholesale redraw. Other visual selection and production slices remain pending; side-trunk reception is a proposal, not accepted coordinates. Do not overwrite T21’s existing first-level implementation or claim it does not exist. Shuffle is player-initiated, uses authored deterministic candidate plans, preserves authored initial layouts and needs new help-policy/candidate certificates; old witnesses do not prove the new skills.

## Reference-led component layout

Follow the user's photographed same-genre reference and established component placement rather than inventing a new navigation pattern: top utilities/coin balance, bottom help-item row, centered settings modal and full shop view. Preserve the three approved page layouts; English copy/font binding does not authorize wholesale repainting or replacing approved slices. Do not introduce a sidebar/drawer layout. Necessary functional text is permitted; approved-art production gates remain unchanged. UI_SPEC owns these current details.

```

## Historical README.md

```text
# 小鸟归巢 · Perch Sort

Current feature-branch checkpoint: `PerchSort/Assets/Scenes/FirstLevel.unity`, its HUD, six-frame flight/wingbeats, Home/nest-entry presentation and first-level persistence integration are committed. The legacy v2 `Game.unity` remains the default entry; the other 39 runtime levels are not migrated. T27/T28 selection-motion and Player-acceptance work is unsubmitted and not run/accepted. Player visual, touch and audio acceptance remains pending. [Current delivery evidence](tasks/T21_report.md).

Unity 6000.6.7f1 portrait sorting puzzle. The first-level slice exists; full campaign migration, future economy/save7 and new design integration remain incomplete. Earlier Core/dependency/test numbers are historical scoped checkpoints, not the current feature inventory. No automatic merge or deployment.

AI先读[AGENTS](AGENTS.md)→[文档路由](docs/README.md)→[玩法证据矩阵](docs/MECHANIC_EVIDENCE.md)。无同行依据的旧自拟玩法已撤回/隔离，不能从旧代码、历史报告或资源存在推定恢复。

| 内容 | 唯一入口 |
| --- | --- |
| 产品/有效规则/现实现差异 | [PRD4](docs/PRD.md) · [RULES4](docs/spec/RULES.md) · [CODE_MAP](docs/spec/CODE_MAP.md) |
| 同行原玩法文字与证据资格 | [研究手册](docs/SORTING_RESEARCH.md) · [50项矩阵](docs/MECHANIC_EVIDENCE.md) |
| 40固定关卡/新模型回放 | [教学卡](docs/LEVEL_DESIGN.md) · [固定附录与证据](docs/LEVEL_SIMULATIONS.md) |
| 世界/森林主题/顺序解锁 | [MAP_PROGRESSION](docs/MAP_PROGRESSION.md) |
| 已确认美术风格/树枝摆放/尺寸与图标约束 | [UI_SPEC §4.1](docs/UI_SPEC.md#approved-forest-style) · [尺寸记录](docs/UI_SPEC.md) |
| 动画/输入/可复制AI模板/开源库 | [INTERACTION](docs/spec/INTERACTION_ANIMATION.md) · [PROMPTS](docs/spec/ANIMATION_PROMPTS.md) · [LIBRARIES](docs/spec/ANIMATION_LIBRARIES.md) |
| schema/存档/验收/本轮记录 | [DATA_SAVE](docs/spec/DATA_SAVE.md) · [VALIDATION](docs/spec/VALIDATION.md) · [T18报告](tasks/T18_report.md) |

每关初态固定；普通加枝已补同类来源与RULES R07完整合同，免费一条本关N座空枝。备用容器逐次扩高仍待参数；固定不代表禁止扩空间。已采规则为端部同类整理、容量内部分搬移、问号蛋、完成群飞走后失枝、免费无限回退/重开、按新帮助技能合同提供提示与最佳操作记录。具体数值/组合迁移和未证边界在证据表，不声称单一竞品原套装。

世界均为不同森林，主题候选阔叶林/竹林/秋日枫林/雪松林，不加环境机关。统一美术风格与树枝摆放原则已由用户确认，唯一约束见[UI_SPEC §4.1](docs/UI_SPEC.md#approved-forest-style)；小鸟允许依此重新设计，现有原素材不覆盖。具体生产素材/地图布局仍未验收，不能将风格确认当作实现授权。生产图标/按钮基线保留；本次设计探索允许必要文字和替代样式，最终选稿/切图仍待验收。简单静音开关，无音量滑条。动画优先单一开源库，不能静图冒动画；当前首关已有六帧振翅表现；Player视觉、触控与听感验收仍待完成。

工程入口`PerchSort/Assets/Scenes/Game.unity`，Core为纯C#；原`PerchSort/Assets/Resources/Levels/levels.json`未更改。T21首关实现已授权；current execution is local-computer code/assets/Unity plus cloud parallel work, with no 251 access or transfer; 普通设计决定由团队推进，生产素材/布局须内部技术与独立视觉QA，不等待重复用户确认；新商店/帮助技能设计已获授权，实际广告SDK/IAP、后续关卡迁移不因此自动获准。旧T16规则与60路径已[失效](docs/archive/T16_WITHDRAWN.md)；当前40新独立模型回放不等于Unity/最优或20个蛋关盲玩公平通过。

Adopted L01: N4/K2/P3, S1=AAA/S2=BBBA/S3=B, two Transfers, no empty branches; RULES owns its sole campaign/level exception. The existing first-level slice uses an upper receiving nest and staggered supported branches. New side-trunk reception-area exploration is pending visual selection, not a replacement already integrated.

## Shop and help-skill design

[Economy/shop contract](docs/spec/ECONOMY_SHOP.md): coin-only same-color skins, four help skills with three equipped slots, separate charge inventory/upgrades, first-clear coins and optional double reward. Runtime is DESIGN_ONLY; current save6 is unchanged, future economy save7 is specified separately. No real-money prices or live ad/IAP integration. [UI exploration](docs/UI_SPEC.md#current-design-exploration) permits functional text/new layouts; selected production art remains pending. Existing T21 first-level scene and recorded wingbeat/Home implementation remain present.

## Current working model and UI language

Use the user's local computer for authorized production and Unity work, with parallel cloud design/documentation/review and assigned code. [AGENTS](AGENTS.md) owns disjoint agent file ownership and integration checks. **Do not connect/read/write/sync/transfer to 251; leave its old data untouched.** Player/window permissions remain separate; uncommitted changes are not a submitted or tested build.

Player-visible UI is English with the verified repository game font (Fredoka for the existing direction), retaining the current color variety and i18n structure. User conversation language is unchanged. Follow the photographed genre-standard layout: top utilities/coins, bottom items, centered settings, full shop—not a new sidebar/drawer. Existing skin/skill/settings visual approvals remain valid; translate copy faithfully without wholesale redraw. See [UI_SPEC](docs/UI_SPEC.md).

```

