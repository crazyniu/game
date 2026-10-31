# 小鸟归巢 · Perch Sort

Unity **6000.6.7f1** 竖屏离线排序益智游戏：搬移外露同种鸟群，凑齐并按鸟巢目标回家。

**当前代码v2已有40关；新版纹样蛋、临时短枝与40关设计是评审稿，尚未实现/落库/获得新证书。** 本次任务只整理与评审文档、桌面玩法推演及文档提交，不授权游戏代码、运行关卡、场景或素材实施；未来实施须另行明确授权。

## AI从这里读

1. [AGENTS](AGENTS.md)：执行地点、Git/审核和资源门槛。
2. [docs导航](docs/README.md)：唯一权威归属、状态标签、按任务的最少阅读集合。
3. [主PRD](docs/PRD.md)：目标与范围；[40关玩法](docs/LEVEL_DESIGN.md)：每关参数/步骤/差异/验收。

| 需要查什么 | 文档 |
| --- | --- |
| 唯一规则/事务 | [RULES](docs/spec/RULES.md) |
| 40关固定初盘/完整纸面回放 | [LEVEL_SIMULATIONS](docs/LEVEL_SIMULATIONS.md)（40主线+20对照，不是Unity/最优证明） |
| 动画、交互、输入锁 | [INTERACTION_ANIMATION](docs/spec/INTERACTION_ANIMATION.md) |
| 动画AI提示词/按钮尺寸 | [ANIMATION_PROMPTS](docs/spec/ANIMATION_PROMPTS.md) · [UI_SPEC](docs/UI_SPEC.md) |
| schema、版本、存档提交/迁移 | [DATA_SAVE](docs/spec/DATA_SAVE.md) |
| 求解、公平性、精确用例、阶段验收 | [VALIDATION](docs/spec/VALIDATION.md) |
| 现行代码与新版差异 | [CODE_MAP](docs/spec/CODE_MAP.md) |
| 同类来源与采纳理由 | [SORTING_RESEARCH](docs/SORTING_RESEARCH.md) |
| 既有视觉/上架/本轮证据 | [UI_SPEC](docs/UI_SPEC.md) · [发布清单](docs/RELEASE_CHECKLIST.md) · [T16报告](tasks/T16_report.md) |

每关内容/玩法固定，重开、重玩和重新进入不随机换盘；差异来自跨关设计。新版保留自动归巢与等待连锁，轮换经典腾位、长短枝/篮周转、探蛋、条件救援、临时空间、目标顺序六类决策。每关最多两类特殊机制，每五关舒缓。无广告/内购/体力；手动放飞风闸仅后续候选。

## 工程与验证

用固定版本Unity Hub打开`PerchSort/`，入口`PerchSort/Assets/Scenes/Game.unity`。`Assets/Scripts/Core`（工程内）为纯C#；View/UI/Audio负责表现。现行数据`PerchSort/Assets/Resources/Levels/levels.json`仍是40关v2，不调用旧生成器覆盖新版设计。

开发在251，Mac仅测试/截图；运行前确认工程未被Editor占用，不强杀用户编辑器。实际命令/测试基线与失败/跳过要求见AGENTS和VALIDATION。本轮未重跑Unity，新关必须重新证明和实机核验。原始美术不覆盖，新资源先验收；不自动合并、部署或发布。
