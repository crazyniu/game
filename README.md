# 小鸟归巢 · Perch Sort

T21 feature-branch checkpoint: the isolated two-move first-level scene and real wingbeat/Home presentation are implemented and recorded. Open `PerchSort/Assets/Scenes/FirstLevel.unity`; see [current delivery evidence](tasks/T21_report.md). Legacy Game/default BuildSettings and the other39 runtime levels remain unchanged. The explicit local Player build is next; fullsave6/mobile acceptance is pending.

Unity6000.6.7f1竖屏排序益智项目。**Production remains v2/40; T21 isolated bird-only Core and the LitMotion desktop dependency checkpoint are implemented/tested, not completed production UI or full v4/save6.** First-level implementation is authorized; no automatic merge/deployment. [T21 evidence](tasks/T21_report.md).

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

每关初态固定；普通加枝已补同类来源与RULES R07完整合同，免费一条本关N座空枝。备用容器逐次扩高仍待参数；固定不代表禁止扩空间。已采规则为端部同类整理、容量内部分搬移、问号蛋、完成群飞走后失枝、免费无限回退/提示与最佳操作记录。具体数值/组合迁移和未证边界在证据表，不声称单一竞品原套装。

世界均为不同森林，主题候选阔叶林/竹林/秋日枫林/雪松林，不加环境机关。统一美术风格与树枝摆放原则已由用户确认，唯一约束见[UI_SPEC §4.1](docs/UI_SPEC.md#approved-forest-style)；小鸟允许依此重新设计，现有原素材不覆盖。具体生产素材/地图布局仍未验收，不能将风格确认当作实现授权。清晰图标不重复可见文字，规则形按钮、简单静音开关，无音量滑条。动画优先单一开源库，不能静图冒动画；现单张fly资源尚不等于真实振翅。

工程入口`PerchSort/Assets/Scenes/Game.unity`，Core为纯C#；原`PerchSort/Assets/Resources/Levels/levels.json`未更改。T21首关实现已授权，251开发、Mac测试；普通设计决定由团队推进，生产素材/布局须内部技术与独立视觉QA，不等待重复用户确认；后续关卡/商业化不因此获准。旧T16规则与60路径已[失效](docs/archive/T16_WITHDRAWN.md)；当前40新独立模型回放不等于Unity/最优或20个蛋关盲玩公平通过。

Adopted L01: N4/K2/P3, S1=AAA/S2=BBBA/S3=B, two Transfers, no empty branches; RULES owns its sole campaign/level exception. First-level presentation uses one upper receiving nest and staggered supported branches; production acceptance remains pending.
