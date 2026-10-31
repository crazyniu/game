# 现行v2实现与同行依据v4差异

2026-10-31 · 只读实现盘点。当前工程Unity6000.6.7f1，运行JSON仍40关version2；本页不定义玩法，唯一规则[RULES](RULES.md)。此次未改代码/JSON/场景/素材，未跑Unity。

## 1. 从旧实现迁移需处理什么

| 已实现v2事实 | v4有效合同/差异 |
| --- | --- |
| 连续run、min(run,余量)、只到空或同种端 | 对应RULES R02，同行出处为Leaf+Simple；加token/蛋边界与稳定ID |
| 每枝4–6，篮容量1，只有六只完整才完成 | v4每关统一N4/6/8，K+2/两空；移除有效篮/混容量短枝；不能沿用硬编码6 |
| 前二目标，未来满组睡眠冻结，目标更新自动收 | 本轮精确来源不足，全部退出新规则；不拿旧代码当同行证据 |
| RevealAfter/RevealSpecies含睡眠完成事件 | 条件公式隔离；v4没有箱字段、完成数开箱链 |
| 归巢释放该枝继续可用 | SRC-BIRD2完成群飞走后失去该枝，v4仅在完成时Retired；普通搬空不退役 |
| List<int>物种，无egg token | 固定token ID/species/form；问号蛋身份隐藏，非提前纹样预告 |
| 旧星级由Undo+Restart计数，Hint每关终身3次 | 有源新版改免费无限帮助/最佳操作；旧政策仅历史，不得带入 |
| 无R07额外枝完整事务 | 新增AddBranch/末枝S(P+1)/辅助身份和分类记录，免费一条N座；旧40见证不算此事务测试 |
| 存档v4按关号回放From/To | v4设计campaign隔离，目标saveVersion6/schema4/rules4，完整稳定快照；详DATA_SAVE |
| 关卡网格/旧设置与手写动画现状 | 世界地图是USER新目标，UI图标/静音/开源优先约束待实施；不能因为现代码写法就豁免 |

T16纹样蛋、任意清空临时枝、schema3/save5、旧60纸面回放是已撤回的设计，**未在游戏实现**。现在新40只以独立文档模型重验，证明状态见LEVEL_SIMULATIONS；旧v2证书也不能证明新规则。

源码定位（只读）：[Rules](../../PerchSort/Assets/Scripts/Core/Rules.cs)、[BoardState](../../PerchSort/Assets/Scripts/Core/BoardState.cs)、[LevelData](../../PerchSort/Assets/Scripts/Core/LevelData.cs)、[Solver](../../PerchSort/Assets/Scripts/Core/Solver/Solver.cs)、[当前关卡](../../PerchSort/Assets/Resources/Levels/levels.json)。原T16审阅识别的L4/L6/L7、L9/L10换色等价仅是旧数据诊断，不改变代码。

## 2. 未来实施的唯一依赖顺序（需要另行授权）

1. 证据/规则/固定内容和未知边界独立审查；这不是代码授权。
2. 251隔离分支实现纯Core/schema4：N可变、token与隐藏egg、Home/Retired、R07 AddBranch与assistPolicyVersion、clone/key/Undo、固定数据校验；不混旧目标/箱字段。
3. 新Solver合法回放/证明状态，未知不无解；重新证明含孵化的启发式，不能沿旧块数下界宣称最优。
4. save6/campaign迁移、原子写档/SaveError、地图进度和回调代次。
5. 已授权后锁定/试验单一开源主补间库、批准新资产（需要振翅clip/帧、蛋孵化、完成枝退场）；无验收资产不能拿静图或近似重绘顶替，Core独立工作可继续。
6. UI/动画/世界地图接线，真实输入/后台/取消/移动平台/录屏；最后40关项目证书、盲玩、设备验证。

运行路径：Core/BoardState、Rules、Move、LevelData、GameSession；Core/Solver；UI/ProgressStore、HintService、TutorialState、TutorialOverlay；View/BoardView、PerchView、BirdView。列表是阅读入口，不是本次修改清单。

## 3. 历史数值不得冒充当前验证

旧Hint节点6000/UI等待1.5s、星级公式、163个EditMode/14个PlayMode通过/8Explicit跳过是旧记录，不是新版政策或本次测试结果。系统减动效读取/新动画库/地图/移动IL2CPP均未验证。新提示预算只影响Unknown，不能借历史值恢复收费或限次。
