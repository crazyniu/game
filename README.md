# 小鸟归巢 · Perch Sort

Unity6000.6.7f1竖屏排序益智项目。**当前运行代码仍v2/40关；这里新增的是同行依据版v4文档，尚未实施。** 本任务只整理文档和临时独立玩法分析，不改源码/运行关卡/场景/素材，不安装库，不自动合并/部署。

AI先读[AGENTS](AGENTS.md)→[文档路由](docs/README.md)→[玩法证据矩阵](docs/MECHANIC_EVIDENCE.md)。无同行依据的旧自拟玩法已撤回/隔离，不能从旧代码、历史报告或资源存在推定恢复。

| 内容 | 唯一入口 |
| --- | --- |
| 产品/有效规则/现实现差异 | [PRD4](docs/PRD.md) · [RULES4](docs/spec/RULES.md) · [CODE_MAP](docs/spec/CODE_MAP.md) |
| 同行原玩法文字与证据资格 | [研究手册](docs/SORTING_RESEARCH.md) · [50项矩阵](docs/MECHANIC_EVIDENCE.md) |
| 40固定关卡/新模型回放 | [教学卡](docs/LEVEL_DESIGN.md) · [固定附录与证据](docs/LEVEL_SIMULATIONS.md) |
| 世界/森林主题/顺序解锁 | [MAP_PROGRESSION](docs/MAP_PROGRESSION.md) |
| UI尺寸/图标硬约束 | [UI_SPEC](docs/UI_SPEC.md) |
| 动画/输入/可复制AI模板/开源库 | [INTERACTION](docs/spec/INTERACTION_ANIMATION.md) · [PROMPTS](docs/spec/ANIMATION_PROMPTS.md) · [LIBRARIES](docs/spec/ANIMATION_LIBRARIES.md) |
| schema/存档/验收/本轮记录 | [DATA_SAVE](docs/spec/DATA_SAVE.md) · [VALIDATION](docs/spec/VALIDATION.md) · [T18报告](tasks/T18_report.md) |

每关初态固定，局内加枝/备用容器扩高有同行依据但精确参数待证；固定不代表禁止扩空间。已采规则为端部同类整理、容量内部分搬移、问号蛋、完成群飞走后失枝、免费无限回退/提示与最佳操作记录。具体数值/组合迁移和未证边界在证据表，不声称单一竞品原套装。

世界均为不同森林，主题候选阔叶林/竹林/秋日枫林/雪松林；沿现小鸟和统一图标/按钮语义，不加环境机关。世界章节与进度是用户要求，概念未定版。清晰图标不重复可见文字，规则形按钮、简单静音开关，无音量滑条。动画优先单一开源库，不能静图冒动画；现单张fly资源尚不等于真实振翅。

工程入口`PerchSort/Assets/Scenes/Game.unity`，Core为纯C#；原`PerchSort/Assets/Resources/Levels/levels.json`未更改。未来代码/素材实施须另有授权，251开发、Mac测试。旧T16规则与60路径已[失效](docs/archive/T16_WITHDRAWN.md)；当前40新独立模型回放不等于Unity/最优或20个蛋关盲玩公平通过。
