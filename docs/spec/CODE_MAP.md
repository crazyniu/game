## Current T21 bounded runtime implementation

The feature branch adds `Assets/Scripts/FirstLevel/` (Flow/Bird/Board/Entry/Hud/Assets), its isolated `FirstLevel.unity` and explicit Editor build/capture entries. It binds only adopted B: N4/K2/P3, AAA/BBBA/B, with real six-pose wings and Moved→Home→Retired→victory→Replay. [T21 current report](../../tasks/T21_report.md) owns named-commit evidence and limitations. Legacy code below remains the v2 map; this is not a full schema4/save6/catalog/Hint/ExtraBranch implementation.

# 现行v2实现与同行依据v4差异

2026-10-31 · 只读实现盘点。当前工程Unity6000.6.7f1，运行JSON仍40关version2；本页不定义玩法，唯一规则[RULES](RULES.md)。Production remains v2. T21 has an isolated bird-only Core foundation (18 focused Unity tests at f3813da, then only 2 added tests at 2d85437; no combined20-test rerun). LitMotion2.0.2 pin0b4c588ee75a07198841d92aab653e6b39445089 resolved and desktop compile/one smoke witness passed at a740b85. Production UI/asset binding, full v4/save6 and runtime acceptance remain incomplete.

## 1. 从旧实现迁移需处理什么

| 已实现v2事实 | v4有效合同/差异 |
| --- | --- |
| 连续run、min(run,余量)、只到空或同种端 | 对应RULES R02，同行出处为Leaf+Simple；加token/蛋边界与稳定ID |
| 每枝4–6，篮容量1，只有六只完整才完成 | v4每关统一N4/6/8，02–40为K+2/两空，仅L01为K2/P3/零空；移除有效篮/混容量短枝；不能沿用硬编码6 |
| 前二目标，未来满组睡眠冻结，目标更新自动收 | 本轮精确来源不足，全部退出新规则；不拿旧代码当同行证据 |
| RevealAfter/RevealSpecies含睡眠完成事件 | 条件公式隔离；v4没有箱字段、完成数开箱链 |
| 归巢释放该枝继续可用 | SRC-BIRD2完成群飞走后失去该枝，v4仅在完成时Retired；普通搬空不退役 |
| List<int>物种，无egg token | 固定token ID/species/form；问号蛋身份隐藏，非提前纹样预告 |
| 旧星级由Undo+Restart计数，Hint每关终身3次 | Current design keeps free Undo/Restart and replaces Hint with ECONOMY_SHOP free-per-attempt/inventory; old lifetime3-hint policy remains withdrawn, not implemented anew |
| 无R07额外枝完整事务 | 新增AddBranch/末枝S(P+1)/辅助身份和分类记录，免费一条N座；旧40见证不算此事务测试 |
| 存档v4按关号回放From/To | v4设计campaign隔离，目标saveVersion6/schema4/rules4，完整稳定快照；详DATA_SAVE |
| 关卡网格/旧设置与手写动画现状 | 世界地图是USER新目标，UI图标/静音/开源优先约束待实施；不能因为现代码写法就豁免 |

T16纹样蛋、任意清空临时枝、schema3/save5、旧60纸面回放是已撤回的设计，**未在游戏实现**。现在新40只以独立文档模型重验，证明状态见LEVEL_SIMULATIONS；旧v2证书也不能证明新规则。

源码定位（只读）：[Rules](../../PerchSort/Assets/Scripts/Core/Rules.cs)、[BoardState](../../PerchSort/Assets/Scripts/Core/BoardState.cs)、[LevelData](../../PerchSort/Assets/Scripts/Core/LevelData.cs)、[Solver](../../PerchSort/Assets/Scripts/Core/Solver/Solver.cs)、[当前关卡](../../PerchSort/Assets/Resources/Levels/levels.json)。原T16审阅识别的L4/L6/L7、L9/L10换色等价仅是旧数据诊断，不改变代码。

## 1.1 T21 adopted first-level checkpoint

Production remains v2. T21 has an isolated bird-only Core foundation (18 focused Unity tests at f3813da, then only 2 added tests at 2d85437; no combined20-test rerun). LitMotion2.0.2 pin0b4c588ee75a07198841d92aab653e6b39445089 resolved and desktop compile/one smoke witness passed at a740b85. Production UI/asset binding, full v4/save6 and runtime acceptance remain incomplete.

L01 is now adopted N4/K2/P3, S1=AAA/S2=BBBA/S3=B, zero empty, two Transfers. One upper receiving nest and staggered supported branches are selected for this first-level slice. Internal technical and independent visual QA remain required before UI integration; ordinary decisions are delegated, without repeated user confirmation. This does not waive asset acceptance or authorize broader campaign/payment work.

The isolated module is [FirstLevelV4](../../PerchSort/Assets/Scripts/Core/FirstLevelV4.cs); hidden eggs, assistance, full schema serialization/save6 and production catalog migration remain incomplete. L01 document adoption does not prove new-profile or scene tests passed. [T21 report](../../tasks/T21_report.md) owns exact test evidence; other39 are not migrated.

## 2. Dependency order (T21 first-level authorized; broader rollout separate)

1. 证据/规则/固定内容和未知边界独立审查；这不是代码授权。
2. 251隔离分支实现纯Core/schema4：N可变、token与隐藏egg、Home/Retired、R07 AddBranch与assistPolicyVersion、clone/key/Undo、固定数据校验；不混旧目标/箱字段。
3. 新Solver合法回放/证明状态，未知不无解；重新证明含孵化的启发式，不能沿旧块数下界宣称最优。
4. save6/campaign迁移、原子写档/SaveError、地图进度和回调代次。
5. 已授权后锁定/试验单一开源主补间库、批准新资产（需要振翅clip/帧、蛋孵化、完成枝退场）；无验收资产不能拿静图或近似重绘顶替，Core独立工作可继续。
6. UI/动画/世界地图接线，真实输入/后台/取消/移动平台/录屏；最后40关项目证书、盲玩、设备验证。

运行路径：Core/BoardState、Rules、Move、LevelData、GameSession；Core/Solver；UI/ProgressStore、HintService、TutorialState、TutorialOverlay；View/BoardView、PerchView、BirdView。列表是阅读入口，不是本次修改清单。

## 3. 历史数值不得冒充当前验证

旧Hint节点6000/UI等待1.5s、星级公式、163个EditMode/14个PlayMode通过/8Explicit跳过是旧记录，不是新版政策或本次测试结果。系统减动效/地图/移动IL2CPP仍未验证；LitMotion仅上述pin解析、desktop编译和1条smoke见证通过，不是运行补间或移动验收。新提示预算只影响Unknown，不能借历史值恢复旧收费/终身限次；当前ECONOMY_SHOP初始调参另有明确USER设计依据。


## 4. T22 reviewed first-level persistence handoff

[FirstLevelPersistence](../../PerchSort/Assets/Scripts/FirstLevel/FirstLevelPersistence.cs) is an isolated L01-only save6 adapter layered on the active T21 Core/session scaffold. T22 uses platform JSON parsing, whole-structure preflight before recoverable semantics, typed-purpose audio retry, normalized-path process-local serialization and post-staging conflict readback. The [T22 report](../../tasks/T22_report.md) records85 focused source tests and explicitly excludes production scene integration and cross-process same-path writing. Earlier54-case evidence did not establish composition safety and is superseded for this final source.

## Current economy/help design versus implementation (2026-11-01)

[ECONOMY_SHOP](ECONOMY_SHOP.md) and RULES §9 now define coin skins/shop, four help skills/three slots, consumable stock/upgrades, rewards/voluntary doubling and deterministic authored Shuffle candidates. ALL are DESIGN_ONLY in this update. No corresponding wallet/shop/ad SDK/IAP/save7 code or new certificate is claimed. Existing save6/T22 code and first-level scene/wingbeat/Home implementation remain unchanged and present. Current code evidence is not replaced by a conceptual mockup.

Future implementation requires separate Core/help actions, catalog/permutation data, save7 adapter, transaction/UI bindings and fresh tests under the approved workflow. This is a dependency description, not a new task queue or execution permission. Existing v2/save6 data cannot silently accept unknown economy fields. Visual selection, accepted production cutouts and the proposed side-trunk receiver remain pending; neither docs nor static images prove those implemented.
