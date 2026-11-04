# T2 — 求解器、生成器与 40 关交付报告

## 结果与验收证据

已完成 `tasks/T2_levels.md`，规则遵循 `docs/DESIGN.md`，未修改既有 BoardState / Rules / Move / GameSession 的行为或源码，未修改 `art/`，未提交 git。

- Unity **6000.6.7f1** 实际 batchmode 调用 `LevelBuilder.BuildAll`，退出 **0**，生成并求解验证全部 **40 关**；日志为 `build/levels-build.log`。
- 实际运行 `bash scripts/run_tests.sh`，退出 **0**；`build/test-results.xml` 为 **84/84 passed，0 failed、0 skipped、0 inconclusive**，包含原 T1 的 30 个用例。测试时间为 2026-10-31 13:19:31–13:19:42（台北），测试框架耗时 **10.449 秒**，不含 Unity 启动。
- 40 关分别从已写入的 JSON 重新读取、校验和求解，再用既有 `Rules.TryApply` 重放返回的所有 Move 到通关；每关最少步数均与文件一致。
- 40 次重新求解合计 **7.462 秒**；最慢为第 40 关 **1.307 秒**、13,113 个访问节点，全部低于 10 秒。构建日志的 seconds 包括候选生成及被拒候选的耗时，不等同于单次求解耗时。
- 另用 Python 标准库独立读取 JSON 检查全部 40 关的 ID、版本、每种鸟恰 4 只、鸟 ID 范围、栖木数量、空栖木数量及无开局满同种栖木，全部通过。
- 前 10 关平均最少步数 **4.4**，后 10 关 **14.0**；K 从 3 增至 10，前 10 关留 3 根空栖木，其后留 2 根；打散强度从 0 连续增至 39，实际反向步骤从 4 增至 171。

## 文件清单

新增：

- `PerchSort/Assets/Scripts/Core/Solver/Solver.cs`：最短解、三态结果及有界 BFS。
- `PerchSort/Assets/Scripts/Core/Generator/LevelGenerator.cs`：生成规格、固定种子反向生成、反向解答证据与求解复核。
- `PerchSort/Assets/Scripts/Core/LevelData.cs`：关卡/关卡包模型、纯 C# JSON 读写及校验。
- `PerchSort/Assets/Editor/LevelBuilder.cs`：编辑器菜单与 batchmode 入口。
- `PerchSort/Assets/Resources/Levels/levels.json`：40 关已验证数据。
- `PerchSort/Assets/Tests/EditMode/LevelTests.cs`：54 个新增用例，其中 40 个逐关复核。
- `tasks/T2_report.md`：本报告。
- 上述 Unity 资产的 `.meta` 及新目录的 Unity 自动生成 `.meta`。

修改：

- `scripts/run_tests.sh`：最低用例数从 25 提升至 40，仍同时检查进程退出码和 XML 全部通过。

既有 `CoreTests.cs`、Core asmdef 及 EditMode asmdef 均保持原样。Core 继续使用 `noEngineReferences: true`，新增运行时代码只使用 .NET BCL，不引用 UnityEngine。

## API、最短路径与预算

`Solver.Solve(BoardState state, int maxNodes = 200000)` 返回 `SolveResult`：

- `Status` 为 `Solved` / `Unsolvable` / `Unknown`。
- 仅 `Solved` 的 `MinMoves` 有效，其值等于 `Moves.Count`；其他结果为 -1、空 Move 序列。
- `VisitedNodes` 统计所有搜索轮次实际接纳的节点，达到预算返回 `Unknown`；不把预算耗尽当成不可解或可解。
- 输入不变，Move 索引针对原始栖木排列，可直接交给 `Rules.TryApply` 或 `GameSession`。

算法为逐步增加总步数上界的 FIFO BFS；每轮仅接纳 `depth + lowerBound <= horizon` 的节点。下界的依据是：一种鸟在收集前能够保持完全不动的，只可能是某根栖木底部连续同种前缀；其余鸟至少要移动一次。若一种鸟需收集 m 次，至多保留 m 个最长前缀，并且至少需要 m 次收集移动。因此该下界不会高估，依次递增上界、每轮按深度 BFS 得到的第一条解是全局最短解。

去重时仅对 key 的栖木顺序排序，调用 Core 的 `GetStableHash()`；哈希桶内还逐项比较内容，避免碰撞误剪枝。搜索状态自身不重排，故父链 Move 索引保持可执行。空栖木的等价目标及仅交换单鸟栖木与空栖木的步骤会被去掉。

对于“全部剩余同种鸟恰 C 只、目标 C-1 只同种、最后一只位于另一根栖木顶端”的可立即收集步骤，优先收集不损失最优性：任何解可先移除这一种鸟并省去它后面的移动，其余鸟相对次序不变、空间只会增加。该剪枝不会套用到某种鸟仍有多于 C 只的局面。测试除手写已知最少步数外，还与不使用启发式或对称性的朴素 BFS 比较全部六种小局面排列。

## 生成、JSON 与复现

`LevelGenerator.Generate(LevelSpec spec, int seed)` 返回 `GeneratedLevel`，包含初始 `State`、独立求解的 `Solution` 和可直接正向重放到通关的 `ReverseWitness`。规格为 `SpeciesCount`、`Capacity=4`、`PerchCount`、`ScrambleStrength`、`MaxNodes`。

生成从真正的全空终态开始，逆转收集步骤：在空目标上恢复 C-1 只鸟，在供体顶端恢复最后一只。随后借助空栖木逆向轮转顶端鸟；每个候选轮转通过 Core 正向重放核对，拒绝无法逆转或出现满同种栖木的候选。强度表示附加三栖木轮转次数，每轮 4 个反向移动；强度 >=20 的第一次轮转使用双鸟块（8 步），加深混杂。最后对完整局面执行 Solver 复核。

使用固定 xorshift32 算法及局部 RNG，物种/栖木排列和轮转选择都只由 spec 与 seed 决定。构建器每关从 `20261031 + id*7919` 开始确定性尝试种子，预算为 20,000 节点；预算超限明确记录并尝试 seed+1，最多 64 个候选。任一关所有候选都失败则整个构建抛错，不替换已交付 JSON；`Unknown` 不会进入成品。

JSON 根对象为 `{ "levels": [...] }`，每关含任务要求的 `id/version/capacity/perches/solverMinMoves/difficulty/seed`，另含 `speciesCount/perchCount/scrambleStrength/reverseSteps`。鸟 ID 为 0 到 K-1，`perches` 是左到右排列的二维数组，末元素为可移动鸟。难度分数为 `100*solverMinMoves + 10*K + scrambleStrength`。

`LevelJson.Serialize/Deserialize` 使用 `DataContractJsonSerializer`，读写均执行 `LevelValidator.Validate`；校验必填字段、版本/关卡 ID、栖木数量与容量、鸟 ID 范围及每种恰 C 只、开局空位/满同种限制和元数据，关卡包还检查 ID 重复。`LevelData.ToBoardState()` 创建独立的 Core 状态。校验器不以元数据代替求解证明，逐关测试另行验证 `solverMinMoves`。

重新生成：

```bash
/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit \
  -projectPath /Users/apple/Documents/projects/game/PerchSort \
  -executeMethod LevelBuilder.BuildAll \
  -logFile /Users/apple/Documents/projects/game/build/levels-build.log
bash scripts/run_tests.sh
```

编辑器入口：`Perch Sort > Build 40 Levels`。复现单关：以文件中的 K、C、P、scrambleStrength 构造 `LevelSpec`，传入该关保存的 seed。

## 难度曲线（实际交付文件）

下表时间为测试中独立重新求解的时间，不含生成。

| 关卡 | K | P | 空栖木 | 打散强度 | 反向步数 | 最少步数 | 难度分数 | 求解秒数 |
| ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 1 | 3 | 6 | 3 | 0 | 4 | 4 | 430 | 0.000 |
| 2 | 3 | 6 | 3 | 1 | 8 | 4 | 431 | 0.000 |
| 3 | 3 | 6 | 3 | 2 | 12 | 4 | 432 | 0.000 |
| 4 | 3 | 6 | 3 | 3 | 16 | 4 | 433 | 0.000 |
| 5 | 3 | 6 | 3 | 4 | 20 | 4 | 434 | 0.000 |
| 6 | 3 | 6 | 3 | 5 | 24 | 4 | 435 | 0.000 |
| 7 | 4 | 7 | 3 | 6 | 29 | 5 | 546 | 0.000 |
| 8 | 4 | 7 | 3 | 7 | 33 | 5 | 547 | 0.000 |
| 9 | 4 | 7 | 3 | 8 | 37 | 5 | 548 | 0.000 |
| 10 | 4 | 7 | 3 | 9 | 41 | 5 | 549 | 0.000 |
| 11 | 4 | 6 | 2 | 10 | 45 | 5 | 550 | 0.000 |
| 12 | 4 | 6 | 2 | 11 | 49 | 5 | 551 | 0.000 |
| 13 | 5 | 7 | 2 | 12 | 54 | 6 | 662 | 0.000 |
| 14 | 5 | 7 | 2 | 13 | 58 | 6 | 663 | 0.000 |
| 15 | 5 | 7 | 2 | 14 | 62 | 6 | 664 | 0.000 |
| 16 | 5 | 7 | 2 | 15 | 66 | 6 | 665 | 0.000 |
| 17 | 5 | 7 | 2 | 16 | 70 | 6 | 666 | 0.000 |
| 18 | 6 | 8 | 2 | 17 | 75 | 7 | 777 | 0.000 |
| 19 | 6 | 8 | 2 | 18 | 79 | 7 | 778 | 0.000 |
| 20 | 6 | 8 | 2 | 19 | 83 | 7 | 779 | 0.000 |
| 21 | 6 | 8 | 2 | 20 | 91 | 11 | 1180 | 0.064 |
| 22 | 6 | 8 | 2 | 21 | 95 | 11 | 1181 | 0.101 |
| 23 | 6 | 8 | 2 | 22 | 99 | 11 | 1182 | 0.076 |
| 24 | 7 | 9 | 2 | 23 | 104 | 12 | 1293 | 0.198 |
| 25 | 7 | 9 | 2 | 24 | 108 | 12 | 1294 | 0.179 |
| 26 | 7 | 9 | 2 | 25 | 112 | 13 | 1395 | 0.235 |
| 27 | 7 | 9 | 2 | 26 | 116 | 13 | 1396 | 0.330 |
| 28 | 7 | 9 | 2 | 27 | 120 | 13 | 1397 | 0.317 |
| 29 | 8 | 10 | 2 | 28 | 125 | 14 | 1508 | 0.530 |
| 30 | 8 | 10 | 2 | 29 | 129 | 13 | 1409 | 0.321 |
| 31 | 8 | 10 | 2 | 30 | 133 | 13 | 1410 | 0.376 |
| 32 | 8 | 10 | 2 | 31 | 137 | 13 | 1411 | 0.293 |
| 33 | 8 | 10 | 2 | 32 | 141 | 14 | 1512 | 0.478 |
| 34 | 8 | 10 | 2 | 33 | 145 | 13 | 1413 | 0.225 |
| 35 | 9 | 11 | 2 | 34 | 150 | 15 | 1624 | 0.759 |
| 36 | 9 | 11 | 2 | 35 | 154 | 14 | 1525 | 0.343 |
| 37 | 9 | 11 | 2 | 36 | 158 | 15 | 1626 | 0.746 |
| 38 | 9 | 11 | 2 | 37 | 162 | 14 | 1527 | 0.551 |
| 39 | 9 | 11 | 2 | 38 | 166 | 13 | 1428 | 0.033 |
| 40 | 10 | 12 | 2 | 39 | 171 | 16 | 1739 | 1.307 |

## 已知限制

- 难度总体递增，不保证每相邻两关最少步数都严格增加；不同种子有局部起伏，前期采用同类轮转结构的教程关存在结构相似性。生成器是有底部同种前缀的可控反向构造，并非覆盖全部随机排列；打散次数增加也不等价于最少步数必然增加。
- 求解节点预算不是硬性墙钟超时；复杂自定义局面可能返回 `Unknown`。本次打包的 40 关全部有实际最短解和 <10 秒的实测证据，不能据此保证任意 K=10 随机局面都能在同样预算内解出。
- 本任务验证 Unity EditMode 与纯 C# 逻辑；未进行表现层试玩、移动端 IL2CPP 打包或视觉验收。
- 未发现需修改或上报的既有 Core 规则错误。
