# AI文档路由：先证据，再读唯一合同

2026-10-31。当前实现v2，新设计v4；本轮只文档，不代码/关卡入库/素材制作/安装动画库。用户新要求取代旧“自行创新”：所有玩法有同类同行证据，不明边界移出有效需求，不能按名字补造。USER约束、同行事实与E工程协议分别标记。

## 1. 状态与用语

`IMPLEMENTED_V2`是现源码事实，不是本轮测试；`DESIGNED_V4`为本版有出处需求，不代表已实现；`PENDING_PARAMETERS`功能已有证据但缺精确参数，不能直接实现；`RESEARCH_ONLY`仅保研究；`WITHDRAWN/INVALIDATED`撤回规则或旧证明失效；`INDEPENDENT_REPLAY_PASS`仅独立临时模型见证；`NOT_RUN/UNKNOWN`见VALIDATION。MUST强制、SHOULD建议偏离须解释；确定规则表默认MUST。

## 2. 单一权威

| 文件 | 拥有内容 | 不得据此推定 |
| --- | --- | --- |
| [MECHANIC_EVIDENCE](MECHANIC_EVIDENCE.md) | 全量玩法系统来源/资格/撤回与缺口，USER/E/P/O区分 | 官方文案不是安装实测，旧代码不是同行证据 |
| [SORTING_RESEARCH](SORTING_RESEARCH.md)及[Cutie Sort参考](references/CUTIE_SORT_GAMEPLAY_REFERENCE.md) | 原游戏可离线阅读的玩法/操作/例子与迁移差异；版本化静态分析仅供研究 | 链接只是出处；竞品参考不修改有效规则 |
| [PRD](PRD.md) | 产品范围/取舍/目标 | 不另定义算法 |
| [RULES](spec/RULES.md) | R01..R07有效玩法、动作/完成/回退/未定功能边界 | 普通加枝遵R07明确合同；逐次扩高/经济待证不可自由补值 |
| [MAP_PROGRESSION](MAP_PROGRESSION.md) | 用户指定世界进度、唯一章节/森林主题分配与门槛 | 未定概念不是已验收资源；环境不影响规则 |
| [LEVEL_DESIGN](LEVEL_DESIGN.md) | 每关教学/决策/节奏 | 题目多不等于策略丰富 |
| [LEVEL_SIMULATIONS](LEVEL_SIMULATIONS.md)与四份固定附录 | 全40精确初态/逐步回放、可见视图/证据限制 | 非运行JSON、非最优/Unity/公平证书 |
| [DATA_SAVE](spec/DATA_SAVE.md) | schema4/rules4/save6身份、hash、存档/事务/迁移 | 工程协议不冒充竞品实现 |
| [INTERACTION](spec/INTERACTION_ANIMATION.md) | AN时间键、输入/前后台/表现 | 时长是项目调参，非同行测量 |
| [UI_SPEC](UI_SPEC.md) | [已确认美术风格与树枝摆放](UI_SPEC.md#approved-forest-style)、静态尺寸/安全区/命中目标、用户图标/按钮/静音硬约束 | 风格确认不是生产素材/坐标/运行验收；旧星槽/双巢等资源尺寸不是有效玩法 |
| [ANIMATION_PROMPTS](spec/ANIMATION_PROMPTS.md) | 分镜/未来获授权实现/验收模板 | 模板不是执行许可 |
| [ANIMATION_LIBRARIES](spec/ANIMATION_LIBRARIES.md) | 开源优先、单主库选型/许可/兼容、动态验收 | 未安装/实测，不可静图冒动画 |
| [VALIDATION](spec/VALIDATION.md) | V用例/证书/公平/设备/运行证据 | Unknown不是无解 |
| [CODE_MAP](spec/CODE_MAP.md) | 现实现差异与未来唯一依赖顺序 | 代码阶段仍需独立用户授权 |

## 3. 按任务最少阅读集合

| 任务 | 顺序 |
| --- | --- |
| 产品评审 | PRD→证据矩阵→相关同行文字→RULES |
| 规则/数据 | 证据矩阵→RULES全篇→DATA_SAVE→CODE_MAP→VALIDATION |
| 关卡策划 | RULES→MAP_PROGRESSION→对应卡→对应10关固定附录→VALIDATION |
| UI/动画 | UI_SPEC→INTERACTION→ANIMATION_LIBRARIES→所需PROMPTS→RULES事件 |
| 地图/成长 | MAP_PROGRESSION→证据矩阵成长项→UI_SPEC→DATA_SAVE；未知经济不补猜 |
| 验收/存档 | VALIDATION→DATA_SAVE→相关规则/内容hash→AGENTS |

本轮交付与未测边界见[T18报告](../tasks/T18_report.md)。旧[T16撤回记录](archive/T16_WITHDRAWN.md)、[v2档案](archive/DESIGN_V2.md)、[UI历史](archive/UI_SPEC_HISTORY.md)、旧任务报告仅历史；[DESIGN](DESIGN.md)只兼容导航。旧“唯一DESIGN”、原创新临时枝、纹样蛋/队列/箱/星级条款不覆盖本路由。
