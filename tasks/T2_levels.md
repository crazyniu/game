# T2 — 求解器 + 关卡生成器 + 40 关数据

规则以 `docs/DESIGN.md` 为准；Core 已完成并通过验收（`PerchSort/Assets/Scripts/Core`：BoardState/Rules/GameSession）。**复用 Core，不要改其行为**；如发现 Core 的 bug，只在报告里指出。

## 做什么
1. `PerchSort/Assets/Scripts/Core/Solver`（仍属 `PerchSort.Core`，纯 C#）：
   - `Solver.Solve(BoardState, maxNodes)` → 是否可解 + 最少步数（BFS，状态用 Core 的稳定哈希去重；注意栖木顺序对称性可做规范化以剪枝，但必须保证最少步数正确）+ 一条解答 Move 序列。超过节点预算返回“未知”，不得当作可解。
2. `Assets/Scripts/Core/Generator`：`LevelGenerator.Generate(spec, seed)`。spec：种类数 K、容量 C(=4)、栖木数 P、打散强度。做法：从“已排好”的终态出发反向随机打乱（保证可解），再用 Solver 复核；开局不得有已满同种栖木，不得已通关。确定性（同 seed 同结果）。
3. 编辑器工具 `Assets/Editor/LevelBuilder.cs`（菜单 + 可 batchmode 调用的静态方法 `LevelBuilder.BuildAll`）：按难度曲线生成 40 关写入 `Assets/Resources/Levels/levels.json`。难度曲线：K 3→10，空栖木数前 10 关 3 根，之后 2 根；打散强度和 solverMinMoves 总体递增。每关字段：id、version=1、capacity、perches（二维数组）、solverMinMoves、difficulty（自定义分数）、seed。
4. `Assets/Scripts/Core/LevelData.cs`（或同类）：JSON 读写与校验（`LevelValidator`：每种鸟恰 C 只、无非法 id、栖木数量一致）。
5. 测试（EditMode）新增：求解器对手工小局面给出已知最少步数；不可解局面返回不可解；生成器确定性；40 关逐一通过 Validator + Solver 重新验证可解且 minMoves 与文件一致；难度趋势检查（后 10 关平均 minMoves 大于前 10 关）。
6. `scripts/run_tests.sh` 保持可用；全部测试通过（含 T1 的 30 个）。

## 约束
- 不改 Core 既有行为；不碰 `art/`；不提交 git；运行时不引用 UnityEngine（Editor 工具除外）。
- Solver 性能：单关求解在合理时间内（< 10 秒/关）；超预算要有明确处理。

## 验收（Claude 将亲自运行并抽查）
- `bash scripts/run_tests.sh` 退出 0，用例数 ≥ 40，无失败。
- `levels.json` 40 关，我会用独立脚本复核每关鸟数、可解性。
- `tasks/T2_report.md`：文件清单、难度曲线表（每关 K/P/minMoves）、已知限制。
