Current T21 runtime slice: [first-level scene/evidence](../tasks/T21_report.md) now covers the two-transfer tutorial and real wingbeat/masked Home presentation. Read that current checkpoint before historical 'Core only/no UI' notes below. Full schema4/save6/40-level migration and local Player acceptance remain pending.

Active capture pipeline: `scripts/screenshots.sh` → `Screenshots.Capture` → `FirstFiveCaptureSequence`/`CaptureRoutine`; `scripts/screenshots_aspect.sh` → `Screenshots.CaptureAspect` → `AspectCaptureSequence`. The isolated first-level runtime capture remains separate. [T26](../tasks/T26_report.md) removes only the uncalled RunV2 capture cluster and 11 proven-unused imported flat UI PNGs with metadata; this does not retire v2 runtime or migrate the default scene. Current UI imports use `UIAssetBuilder` with `art/ui_kit` → `Assets/Art/ui_kit`. Preserved `art/ui_sources/generate_ui.py` and its README are historical source archives, not the normal build pipeline: their literal output is `Assets/Art/UI`, while the retired tracked imports were in lowercase `Assets/Art/ui` (distinct on case-sensitive filesystems; potentially aliases on case-insensitive ones). Do not run that archived generator as a current rebuild step.

# AI文档路由：先证据，再读唯一合同

2026-10-31. Production remains v2; T21 adds an isolated bird-only Core: 18 focused Unity tests passed at f3813da, then only 2 added tests passed at 2d85437 (not a combined 20-test rerun). T21 first-level implementation is authorized; L01 now uses the adopted N4/K2/P3 profile (RULES §2.1). Internal technical/independent visual asset QA remains mandatory. Production UI/asset binding and full v4/save6 remain incomplete; see [T21 report](../tasks/T21_report.md). 用户新要求取代旧“自行创新”：所有玩法有同类同行证据，不明边界移出有效需求，不能按名字补造。USER约束、同行事实与E工程协议分别标记。

## 1. 状态与用语

`IMPLEMENTED_V2`是现源码事实，不是本轮测试；`DESIGNED_V4`为本版有出处需求，不代表已实现；`PENDING_PARAMETERS`功能已有证据但缺精确参数，不能直接实现；`RESEARCH_ONLY`仅保研究；`WITHDRAWN/INVALIDATED`撤回规则或旧证明失效；`INDEPENDENT_REPLAY_PASS`仅独立临时模型见证；`NOT_RUN/UNKNOWN`见VALIDATION。MUST强制、SHOULD建议偏离须解释；确定规则表默认MUST。

## 2. 单一权威

| 文件 | 拥有内容 | 不得据此推定 |
| --- | --- | --- |
| [MECHANIC_EVIDENCE](MECHANIC_EVIDENCE.md) | 全量玩法系统来源/资格/撤回与缺口，USER/E/P/O区分 | 官方文案不是安装实测，旧代码不是同行证据 |
| [SORTING_RESEARCH](SORTING_RESEARCH.md)及[Cutie Sort参考](references/CUTIE_SORT_GAMEPLAY_REFERENCE.md) | 原游戏可离线阅读的玩法/操作/例子与迁移差异；版本化静态分析仅供研究 | 链接只是出处；竞品参考不修改有效规则 |
| [ECONOMY_SHOP](spec/ECONOMY_SHOP.md) | Current coin-only shop/help-skill/reward design, initial tuning, migration/transactions | DESIGN_ONLY; no SDK/IAP/real-money quotes; future save7 is not current save6 |
| [PRD](PRD.md) | 产品范围/取舍/目标 | 不另定义算法 |
| [RULES](spec/RULES.md) | R01..R07有效玩法、动作/完成/回退/未定功能边界 | 普通加枝遵R07明确合同；逐次扩高待证；经济采用ECONOMY_SHOP的明示项目初始调参，不冒称竞品值 |
| [MAP_PROGRESSION](MAP_PROGRESSION.md) | 用户指定世界进度、唯一章节/森林主题分配与门槛 | 未定概念不是已验收资源；环境不影响规则 |
| [LEVEL_DESIGN](LEVEL_DESIGN.md) | 每关教学/决策/节奏 | 题目多不等于策略丰富 |
| [LEVEL_SIMULATIONS](LEVEL_SIMULATIONS.md)与四份固定附录 | 全40精确初态/逐步回放、可见视图/证据限制 | 非运行JSON、非最优/Unity/公平证书 |
| [DATA_SAVE](spec/DATA_SAVE.md) | schema4/rules4/save6身份、hash、存档/事务/迁移 | 工程协议不冒充竞品实现 |
| [INTERACTION](spec/INTERACTION_ANIMATION.md) | AN时间键、输入/前后台/表现 | 时长是项目调参，非同行测量 |
| [UI_SPEC](UI_SPEC.md) | [已确认美术风格与树枝摆放](UI_SPEC.md#approved-forest-style)、静态尺寸/安全区/命中目标、用户图标/按钮/静音硬约束 | 风格确认不是生产素材/坐标/运行验收；旧星槽/双巢等资源尺寸不是有效玩法 |
| [ANIMATION_PROMPTS](spec/ANIMATION_PROMPTS.md) | 分镜/未来获授权实现/验收模板 | 模板不是执行许可 |
| [ANIMATION_LIBRARIES](spec/ANIMATION_LIBRARIES.md) | 开源优先、单主库选型/许可/兼容、动态验收 | LitMotion 2.0.2 pin 0b4c588ee75a07198841d92aab653e6b39445089 resolved on Mac; desktop compile and 1 smoke witness passed at a740b85. Runtime motion/mobile acceptance remains pending; static images are not animation proof. |
| [VALIDATION](spec/VALIDATION.md) | V用例/证书/公平/设备/运行证据 | Unknown不是无解 |
| [CODE_MAP](spec/CODE_MAP.md) | 现实现差异与未来唯一依赖顺序 | 代码阶段仍需独立用户授权 |

## 3. 按任务最少阅读集合

| 任务 | 顺序 |
| --- | --- |
| 产品评审 | PRD→证据矩阵→相关同行文字→RULES |
| 规则/数据 | 证据矩阵→RULES全篇→ECONOMY_SHOP→DATA_SAVE→CODE_MAP→VALIDATION |
| 关卡策划 | RULES→MAP_PROGRESSION→对应卡→对应10关固定附录→VALIDATION |
| UI/动画 | UI_SPEC→INTERACTION→ANIMATION_LIBRARIES→所需PROMPTS→RULES事件 |
| 地图/成长 | MAP_PROGRESSION→证据矩阵成长项→UI_SPEC→DATA_SAVE；经济/帮助技能→ECONOMY_SHOP；当前设计不是runtime实现 |
| 验收/存档 | VALIDATION→DATA_SAVE→相关规则/内容hash→AGENTS |

Gameplay review and proposals (T20): [GAMEPLAY_REVIEW](GAMEPLAY_REVIEW.md).

Small-scope next-step proposal (first level first): [SMALL_SCOPE_NEXT](proposals/SMALL_SCOPE_NEXT.md).

本轮交付与未测边界见[T18报告](../tasks/T18_report.md)。旧[T16撤回记录](archive/T16_WITHDRAWN.md)、[v2档案](archive/DESIGN_V2.md)、[UI历史](archive/UI_SPEC_HISTORY.md)、旧任务报告仅历史；[DESIGN](DESIGN.md)只兼容导航。旧“唯一DESIGN”、原创新临时枝、纹样蛋/队列/箱/星级条款不覆盖本路由。

First-level preparation only: [asset/layout review and decisions](../tasks/T21_asset_manifest.md) → [event-to-motion handoff](../tasks/T21_event_motion_map.md) → [exact review metadata](../tasks/T21_layout_review.json). These are unaccepted design inputs, not production UI or changed formal levels.

Current design update (2026-11-01): the user-directed shop and help-skill policy replaces old research-only economy and unlimited-Hint design statements, without rewriting historical competitor evidence. RULES incorporates ECONOMY_SHOP §4; DATA_SAVE separates future save7 from implemented save6. UI_SPEC owns the limited new design-exploration exception and pending side-trunk reception proposal. Old first-level implementation evidence and T26 entry cleanup remain preserved.

Current execution/language update: [AGENTS](../AGENTS.md) now owns local-computer production/Unity plus cloud parallel work, disjoint agent ownership and an absolute no-access/no-transfer rule for 251. Historical machine reports remain historical, not instructions to reconnect. [UI_SPEC](UI_SPEC.md) owns English player-visible copy/game-font binding and reference-led top utilities/bottom items/centered settings/full-shop layout. Keep existing i18n/color variety and individually approved pages; user chat language and independent Player permissions are unchanged.
