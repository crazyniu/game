# 小鸟归巢：现行代码、差异与实施依赖

版本：2026-10-31。本文记录IMPLEMENTED_V2与DESIGNED差异，不定义规则；新规则唯一来源为[RULES](RULES.md)。

## 1. 现状与差异

Unity **6000.6.7f1**，工程`PerchSort/`，Core纯C#不引用UnityEngine。精确审阅版本与本轮验证记录见[T16报告](../../tasks/T16_report.md)。

| 维度 | 已从代码/数据确认的现状 | 新版需求 / 迁移影响 |
| --- | --- | --- |
| 搬移 | run连续段，空端/同种端，`min(run,目标空位)` | 保留；run不能越过未孵化蛋，退役枝不可操作 |
| 枝位 | `CapacityAt`按每枝4–6；篮固定1；只有6才完成 | 保留短枝语义；新增临时枝生命周期，固定ID不重排 |
| 鸟表示 | `List<int>`仅存物种 | 需支持稳定token/egg ID、预定物种、孵化状态和可辨纹样 |
| 满组/目标 | 激活种自动归巢，其他满组睡眠冻结；前二未收集目标可任意先收 | 保留流畅自动反馈，不引入手动Dispatch确认 |
| 连锁 | 目标刷新按现行Perches数组下标从0重新扫描自动归巢 | 保留，加入蛋揭示和退役事件，明确事件顺序 |
| 箱 | 完成数/指定种首次满组六只揭示，含睡眠 | 保留；阈值可有1/2/3，不只所有关固定1；临时枝不可同时盖箱 |
| 关卡 | 40关JSON均version2；L8篮、L11箱、L12短枝、L20种类箱、L24双篮 | 新目录重新排课程；不把旧教程节点称为新版已实现 |
| 求解 | A*、结构校验、最优值、回放；精确状态键去重 | 扩展蛋/临时枝状态；另验证可见信息下的公平释放顺序与内容同构 |
| 存档 | v4按关号和From/To历史回放，无完整布局hash | 新rules/catalog身份、状态迁移、内容hash；不能回放旧动作到新局 |
| 评级/提示 | 撤销+重开影响星级；每关终身最多3次提示 | 本稿建议保留现状入口，另提试错友好评级/分层提示供评审，不自动改实现 |

同质化证据：现行生成器用循环分层布局，精确`GetStateKey`去重排不掉换种类的等价题。种类标准化并保留枝位、容量和目标结构后，L4/L6/L7、L9/L10存在等价布局；L6对L9、L7对L10仅多一篮。L1–3同一环模板只换run宽，记录最优均4，L4–10均7。新版必须检查结构和主决策，不把新颜色、新seed或更多鸟等同新玩法。

实现证据：[Rules](../../PerchSort/Assets/Scripts/Core/Rules.cs)、[BoardState](../../PerchSort/Assets/Scripts/Core/BoardState.cs)、[LevelData](../../PerchSort/Assets/Scripts/Core/LevelData.cs)、[Generator](../../PerchSort/Assets/Scripts/Core/Generator/LevelGenerator.cs)、[Solver](../../PerchSort/Assets/Scripts/Core/Solver/Solver.cs)、[现行40关](../../PerchSort/Assets/Resources/Levels/levels.json)。历史测试报告只证明当时版本，不证明新版内容已通过。


## 2. 最小代码阅读与修改依赖

| 范围 | 现路径（相对工程） | 必须迁移 |
| --- | --- | --- |
| 状态/Clone/键 | Assets/Scripts/Core/BoardState.cs | token形态/ID、临时生命周期、精确深克隆与键 |
| 规则/事件 | Assets/Scripts/Core/Rules.cs、Move.cs | 锁run、孵化、稳定化末尾退役、有序事件、完整Undo |
| 关卡/校验 | Assets/Scripts/Core/LevelData.cs | 新schema、鸟数含蛋、位置/箱/临时约束、内容hash |
| 生成/求解 | Core/Generator/LevelGenerator.cs、Core/Solver/Solver.cs | 决策合同、零成本事件下界、预算三态、证书 |
| 历史/存档 | Core/GameSession.cs、UI/ProgressStore.cs | 类型化token状态重建、旧版迁移、备份、恢复整链 |
| 提示/教程 | UI/HintService.cs、TutorialState.cs、TutorialOverlay.cs | 新状态快照与过期保护，课程绑定内容hash |
| 表现 | View/BoardView.cs、PerchView.cs、BirdView.cs | 原鸟与壳替换/临时收枝、末尾同步，不靠动画改逻辑 |
| 质量 | Assets/Tests/EditMode、Assets/Tests/PlayMode | 新规则/证书/恢复/输入锁/资源/多比例 |

**实施依赖顺序唯一归本文件**：文档独立审查通过→新Core/schema/回放与夹具→存档隔离及失败恢复→样板关证书→新资源验收→表现/课程/提示接线与实机闭环→其余关卡/40份证书及真人观察。Core和资源盘点可独立推进，相关UI接线必须等待资源验收。实现地点按根AGENTS：251开发，Mac测试。本轮文档已获授权，代码阶段需先完成文档审稿并确认合法251通路；不直接在Mac改业务代码，不联系未授权外部代理。

## 3. 既有辅助基线（不是新设计规则）

- 当前星级：撤销+重开0次三星、1–3次两星、其余一星，提示不降星；最高星保留，不控制解锁。
- 当前提示：每关终身最多3次，重开/重玩不补；节点6000，界面等待1.5秒；旧结果失效/超时/未知不扣，后台搜索未结束不启动第二个。
- 当前进度v4只存From/To历史等字段，没有新版token/临时状态或内容hash；新旧映射见[DATA_SAVE](DATA_SAVE.md)。
- 当前减动画设置未接系统读取，不能宣称已支持。历史T15b记录EditMode163通过、PlayMode14通过/8Explicit跳过，不是本轮运行。

新版本实际应保留的星级/提示政策唯一见[INTERACTION§3](INTERACTION_ANIMATION.md#3-信息表达与辅助)；本节仅记录v2事实。效率奖章/不惩罚试错和分层提示仍OPTIONAL，不实现；无广告/内购范围不变。

旧Core的Settle已经先标记/揭示本批全部符合箱，再逐枝Complete；新案保持这个批次结构，在每枝Complete前增加外露蛋孵化，并在全部循环末尾增加空临时枝退役。不误写成旧代码逐箱递归完整连锁，也不改变自动事件固定数组顺序。
