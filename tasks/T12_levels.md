# T12 — 关卡扩量与难度曲线（100 关，逐关有差异）

规则权威：`docs/DESIGN.md` v2 与 `tasks/T9_report.md`。现状问题（我验收时发现）：40 关里 minMoves 出现大平台（第 4–10 关全是 7，11–17 全是 10，18–25 全是 17），同档关卡手感几乎一样。本任务按下面**精确规格**重做关卡生成，不得自行改规格。

## 工作方式（避免与正在运行的 UI 任务抢 Unity 工程锁）
**不要在 /Users/apple/Documents/projects/game/PerchSort 里运行 Unity。** 开工第一步：`cp -R PerchSort /Users/apple/Documents/projects/game_work_levels`（含 Library），在副本里开发、跑 batchmode、跑 EditMode 测试。完成后只把这些文件**回写**到主工程：`Assets/Scripts/Core/**`、`Assets/Editor/LevelBuilder.cs`、`Assets/Resources/Levels/levels.json`、`Assets/Tests/EditMode/` 里你新增/修改的 Core/Level 测试。**不得改其它目录**（UI/View 正由别的 worker 修改）。回写前先 diff 主工程这些文件，确保你的副本基线相同。

## 规格
1. **100 关**，JSON v2 格式不变（可新增可选字段，旧字段含义不变）。教学 1–3 关保持现有设计（K=3，无箱无篮，目标顺序 0,1,2）。
2. **章节结构**：每 10 关一章。章内难度逐关上升，第 10 关为本章峰值（“Boss 关”）；下一章第 1 关的难度回落到上章峰值的 70%±10% 并**引入一个新元素或新组合**，随后再逐关上升，且章峰值逐章抬高。引入表：第 8 关篮子、第 11 关 RevealAfter 箱、第 12 关短枝、第 20 关 RevealSpecies 箱（沿用 T9）；后续章：第 31 关起 2 个篮子+箱同时出现、第 41 关起双箱串联（一箱的揭示条件依赖另一箱揭示后的鸟）、第 51 关起 K=7、第 71 关起 K=8、第 81 关起短枝占比≥40%、第 91 关起三箱。
3. **难度度量**：沿用 T9 的 difficulty 公式，但新增硬性要求：**相邻两关的 (minMoves, buriedBirds, K, 箱数, 篮子数, 短枝数) 元组不得完全相同**；章内 difficulty **严格递增**（每关至少比上一关高 2%）；全 100 关没有任何两关初始布局相同或互为镜像/种类置换。
4. **minMoves 区间**（全部要求求解器给出精确最优，Exact）：第 1–10 关 4→14；11–20 为 10→24；之后每章峰值约 +4，到第 100 关峰值 ≥ 40。若求解器在预算内证不出，允许提高预算（单关 ≤ 3,000,000 节点）或换候选，**不得**降低上述区间；确实达不到的关在报告里列出实际值并说明。
5. **可玩性约束**（生成器必须拒绝违反者）：初始状态不得已有任何栖木集齐；至少 2 个初始合法移动；不得存在“第一手最优解即直接唯一可行”的线性关（要求第一步至少 2 个不同的合法且不立即死局的选择）；难关里“死路”比例：随机玩家（均匀选合法移动，深度 ≤ 60）能解的概率须随关卡递减（教学关 ≥ 40%，Boss 关 ≤ 5%）——用蒙特卡洛 2000 次估计并写进关卡数据 `randomSolveRate`（可选字段）。
6. 生成入口保持 `LevelBuilder.BuildAll`（batchmode 可跑），可复现（固定种子表），输出到 levels.json；同时输出 `docs/LEVEL_CURVE.md`（逐关表：id、K、栖木数、篮子、箱、短枝、minMoves、buried、difficulty、randomSolveRate）。
7. 测试：全部 100 关校验器证明可解且最优步数重算一致；章内递增、章峰值递增、相邻元组不同、无重复布局、randomSolveRate 单调趋势（章级）、各引入点关卡确含对应元素；总 EditMode 数量不得少于现有。

## 验收（Claude）
我用独立脚本读取 levels.json 复核结构/章节规则；重算抽样关最优步数；看 LEVEL_CURVE.md 曲线；回写后主工程跑全套测试。报告 `tasks/T12_report.md`（含偏离规格之处，须先问我）。不提交 git。
