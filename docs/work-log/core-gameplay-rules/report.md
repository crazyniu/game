# T9 — Core v2 交付报告

日期：2026-10-31。工作目录：`/Users/apple/Documents/projects/game`。规则依据：`docs/DESIGN.md` 核心规则 v2，以及协调者消息 `msg_d5ae01b0c205` 补充的每枝容量 4–6 / 短枝中转规则。

## 交付结论

Core v2、求解器、生成器、校验器、JSON v2 和 40 关已实现。Unity EditMode **142/142 通过**，其中 Core 49、Level 73，共 122 项（原 Core 30 + Level 54 = 84 项）；全部 40 关均由求解器重算最优步数并合法回放到所有鸟归巢。关卡 K=3→8、最少步数 4→26，K / 最少步数 / 被异种埋住鸟数均逐关非递减。

**完整测试脚本尚未全绿**：`bash scripts/run_tests.sh` 的 EditMode 全过，PlayMode 5/11 通过、6 失败，脚本退出码 1。失败均来自未迁移的 v1 PlayMode 容量假设，详见末节；协调者已在 `msg_eacfbb675f4b` 明确确认 PlayMode/View 留给 T10，不扩大本任务文件所有权。

未提交 git、未修改 art、View、UI、Audio 或 PlayMode 测试源码；没有添加表现层编译垫片。按要求向唯一打开的 PerchSort 主 Unity 进程 PID 42687 发送 TERM 并确认退出，未关闭其他项目，未重新打开编辑器。要求运行的原测试脚本自身会调用 `SceneBuilder.Build` 重建测试场景；没有手工编辑场景或修改 SceneBuilder。

## 实现内容

- 每枝 `Capacity=4..6`，默认 6；篮子容量固定 1。`Birds[0]` 靠墙、末端外露，左右列的逻辑顺序一致。
- 外露连续段移动、目标同种/空位限制、部分移动、睡眠/箱体冻结、篮子往返。非法操作在修改状态之前返回 false。
- 目标原始队列 + 已收集目标，支持先收集当前第二目标；完成时归巢或睡眠，目标补位后按栖木编号扫描自动归巢链，再统一揭箱并重复至稳定。
- 完整事件序列：Moved → Completed →（归巢时）TargetsAdvanced → 零或多组 AutoHomed / TargetsAdvanced → CrateRevealed；揭示导致的新完成会继续产生事件。
- 深克隆包括鸟、每枝容量、睡眠、箱条件、篮子、目标及进度。稳定键保留栖木顺序、所有规则状态，不用同构排序；哈希以精确键做 FNV-1a。求解器用完整字符串键去重，哈希碰撞不会合并状态。
- `GameSession` 原快照机制无需改变：新的深克隆使一次 Undo 还原整次操作及全部连锁，Restart 还原初始棋盘并清空历史。
- A* 搜索有节点预算、严格区分最优解 / 有合法见证的上界 / 已证明无解 / 预算内未知。无合法移动节点不扩展。没有沿用 v1 的“立即收集必然可交换到第一步”剪枝。
- 启发式为每种鸟的连续段数 r 的 `max(0, r-1)` 之和。一次操作至多合并一个同种段对；归巢/自动归巢只移除已经单段的完整鸟群，其贡献为零；拆段只会增加段数，因此下界可采纳，适用于睡眠、箱、短枝和篮子。
- 受控随机循环分层布局保持每种正好 6 只；箱体组先被前一组完成解锁；只返回预算内已证明最优的候选。每关最多 64 个候选、每次预算 60,000 个状态，失败不覆盖库；实际发布关卡最大 1,918 个入队状态。
- 生成入口：Unity 菜单 **Perch Sort/Build 40 Levels**；batchmode **LevelBuilder.BuildAll**。完整生成后才写文件，JSON 用 DataContractJsonSerializer。

## 公开 API 变化（T10 接入表）

| v1 | v2 / 迁移说明 |
|---|---|
| `BoardState.Capacity = 4` | 默认 6，表示关卡鸟群完成数量元数据；布局/空位必须读取每根 `PerchState.Capacity`，不要用全局 Capacity 画所有树枝 |
| `BoardState.Perches` | 保留原名称；新增同列表别名 `Shelves`，不用复制列表 |
| `PerchState.Birds` 从左到右 | 改为靠墙→通道，末端始终外露；右列由 `Side == ShelfSide.Right` 镜像绘制 |
| `PerchState.Locked / Extra` 保留标记 | 删除；使用 `Asleep`、`Crate`、`Capacity`、`Side`，原 View/UI 没有引用这两个旧字段 |
| 无篮子 | `BoardState.Baskets : List<BasketState>`，每个 `.Birds` 最多 1 个，`.Capacity == 1` |
| 仅栖木编号的 From / To | 栖木仍为 `[0, Perches.Count)`；篮子为 `[Perches.Count, LocationCount)`，使用 `BasketIndex(i)` / `IsBasket(location)` / `BirdsAt(location)` / `CapacityAt(location)` |
| 无目标状态 | `Targets` 保存原顺序；`CollectedTargets` 记录已归巢种类；`CollectedCount` 为已收集数；`ActiveTargets` 每次返回未收集前 2 个的新列表；`CompletedCount` 累计第一次完成数 |
| `Move(from,to)` | 构造方式保留，作为操作请求；新增 `.Count` 与 `.Events`，成功的完整操作在 `MoveResult.Move` 中返回，不会回写传入的值类型 Move |
| `Rules.TryApply(..., out MoveResult)` | 签名保留，按 v2 规则修改棋盘；新增 `Rules.GetRun(state, location)`，睡眠/箱冻结由合法性判断负责，GetRun 只量内容 |
| `GetLegalMoves()` 每次单只 | 返回所有合法来源/目标及真实 `.Count`；执行前 `.Events` 为空，执行后由结果提供 |
| `MoveResult.Bird / Collected / CollectedPerchIndex / CollectedSpecies` | 为原 View 编译保留，反映直接目标的立即归巢；**不能表达自动连锁**。T10 必须消费 `result.Move` / `result.Events` / `result.Count` |
| 无事件 | `MoveEvent.Kind` 是 `Moved / Completed / TargetsAdvanced / AutoHomed / CrateRevealed`；移动带 `From/To/Count/Species`，完成带 `Shelf/Species/Completion(Asleep/Homed)`，目标刷新带不可变快照 `ActiveTargets/CollectedCount`，自动归巢带 `Shelf/Species`，揭箱带 `Shelf` |
| `IsWon` 判所有栖木空 | 判全部目标已收集；合法关卡中等价于所有鸟归巢；空盘但目标未收集不算赢 |
| `GetStableHash()` 旧固定向量 | 方法保留但键内容及哈希值改变；新增 `GetStateKey()` 提供精确等价键，不能拿 v1 哈希作存档兼容判断 |
| `GameSession.TryMove / Undo / Restart / State / UndoCount` | 签名保留；Undo 覆盖整个连锁，快照现在包含全部 v2 元数据 |
| `SolveResult.Status / MinMoves / Moves / VisitedNodes` | 保留；新增 `Solvable / Proven / Optimal / Bound`。`Bound.Exact` 才是最少步数；`UpperBound` 是合法解长度；`None` 时 MinMoves=-1；`Proven` 表示可解或无解已证明，**不是**最优标记 |
| `LevelData.version=1, capacity=4, perches:int[][]` | `version=2, capacity=6, shelves:ShelfData[]`；每 shelf 包含 `side:"left"/"right", capacity:4..6, birds:int[], crate` |
| JSON 无队列/篮子/箱 | `targets:int[]`、`baskets:int`（初始空篮子数量）；`crate` 省略表示无遮盖，否则只有 `revealAfter` 或 `revealSpecies` 一个字段 |
| `speciesCount / perchCount` JSON 字段 | 从 targets/shelves 推导的只读 C# 属性，v2 JSON 不再重复存储 |
| `scrambleStrength / reverseSteps` 关卡 JSON 字段 | 删除；保留 `id / solverMinMoves / difficulty / seed`，最短路径由 Solver 重算 |
| `LevelSpec.Capacity=4`、反向旋转强度 | 默认 6；新增 `Baskets / Crates / ShortShelves / Teaching`；ScrambleStrength 在非教学时决定分层数（2–6），教学时决定底层宽度 |
| `GeneratedLevel.ReverseWitness / ReverseSteps` | 删除；使用 `GeneratedLevel.State / Solution`，`Solution.Moves` 是已证明最短的合法见证 |
| `LevelValidator.Validate(level)` 仅结构检查 | 现在结构 + 求解 + 必须最优 + 核对 solverMinMoves；返回 SolveResult。`Validate(catalog)` 全量证明；可传 maxNodes |
| `LevelJson.Deserialize / Serialize` | 签名保留，检查结构与版本；为避免载入游戏同步重求所有关卡，不在普通 JSON I/O 中运行求解器。导入/入库验证请显式调用 `LevelValidator.Validate`；提供 `ValidateStructure` 供只检查结构使用 |

v1 JSON 会明确报 `Level version 1 is unsupported; expected version 2`，不做静默迁移。v1 存档的操作序列语义也发生变化，T10 必须决定存档版本迁移/重开策略；本任务没有越界改 UI 存档层。

## 逐条列明的规则决定

1. **新增原创规则（协调者明确授权）**：每根栖木独立容量 4–6，只有 6 只同种算完成；短枝填满不会睡眠、不会归巢、不会增加 CompletedCount。第 12 关起混用容量 4 或 5 的短枝，初始有鸟的栖木保持容量 6。
2. 第二激活目标可先收集。因此 Targets 保留原队列，用已收集种类集合语义跳过；不能用一个前缀计数表示进度。哈希中的已收集顺序不影响等价性。
3. 同一个鸟群从“首次完成并睡眠”到“自动归巢”只记一次 CompletedCount；自动归巢不再计一次完成。RevealSpecies 可被睡眠完成触发。
4. 每次目标刷新后从编号 0 重新查找第一个可自动归巢的睡眠栖木，整个自动归巢链完成后，按编号统一揭示满足条件的箱。AutoHomed 事件在该次 TargetsAdvanced 之前，按实际因果顺序播放。
5. 揭示后箱体置 null，不再保存冗余 revealed 标志。刚揭示的完整同种鸟群立即完成，再重复自动归巢/揭箱直到稳定；正常入库关卡禁止任何初始满 6 同种枝，因此此分支服务于直接构造/恢复的状态。
6. 初始棋盘不隐式结算；GameSession 构造只克隆。LevelValidator 拒绝预先完整同种枝，避免加载即完成且没有玩家操作事件。
7. 两种箱条件必须二选一。RevealAfter 范围 1..K（不接受 0 或负数），RevealSpecies 范围 0..K-1；触发不可达（如自己锁住自己的解锁种类）由求解验证拒绝，不只依赖字段合法性。
8. 篮子可与空篮子互转，遵循同一合法性；篮子一次最多移动 1 只，不成为目标完成位置。JSON 的 baskets 仅表示初始空篮子数量，不接受关卡里预填篮子。
9. “至少 2 个空位”按所有栖木与篮子的总剩余座位数解释，包含箱体盖住的容量；另要求至少有一根可操作的 6 座枝，并由求解器最终证明可解。没有沿用 v1 的“至少两个完全空栖木”校验；生成的发布关卡仍保留两根初始空枝。
10. 合法 species id 采用连续 0..K-1，目标队列必须覆盖每个 id 恰一次；初始每种总数严格为 6，K=3..8。
11. 关卡难度解释为逐关非递减、允许平台段：`difficulty = minMoves*100 + K*10 + buriedBirds`；buriedBirds 是每枝所有非外露 run 内的鸟数之和。另同时约束 minMoves、K、buriedBirds 各自不下降，避免只靠加权分掩盖实际降难度。
12. 教学 1–3 关目标顺序固定 0,1,2，墙侧鸟群依次对应目标，底层宽度从 1→2→3；无篮子/箱。第 8 关引入 1 个篮子，第 11 关引入 RevealAfter，第 20 关引入 RevealSpecies，第 24 关达到 2 篮子。
13. 精确重复的初始状态不入库；循环分层和分组生成会有同类布局，保证规则与难度进阶，未声称 40 种互不相似的关卡设计。
14. `Solver.MinMoves` 为兼容保留名称，但预算提前终止且已有见证时其数值只能是 upper bound；必须一起检查 Bound/Optimal。未知绝不标无解。发布与 Validate 都要求 Exact，不接收 upper bound。
15. Node budget 统计实际入队的状态（根节点也计入），包括更短路径重新打开的状态；被下界剪枝的候选、已知更短路径的重复状态不计入。公开 `VisitedNodes` 使用相同口径。

## 40 关难度表

箱列：`A=n` 为完成数触发，`S=s` 为种类完成触发。短枝列列出存在的短枝容量，其余均为 6；埋鸟为 buriedBirds。

| id | K | 栖木数 | 篮子 | 箱 | 短枝容量 | minMoves（最优） | 埋鸟 | difficulty |
|---:|---:|---:|---:|---|---|---:|---:|---:|
| 1 | 3 | 5 | 0 | — | — | 4 | 3 | 433 |
| 2 | 3 | 5 | 0 | — | — | 4 | 6 | 436 |
| 3 | 3 | 5 | 0 | — | — | 4 | 9 | 439 |
| 4 | 3 | 5 | 0 | — | — | 7 | 15 | 745 |
| 5 | 3 | 5 | 0 | — | — | 7 | 15 | 745 |
| 6 | 3 | 5 | 0 | — | — | 7 | 15 | 745 |
| 7 | 3 | 5 | 0 | — | — | 7 | 15 | 745 |
| 8 | 3 | 5 | 1 | — | — | 7 | 15 | 745 |
| 9 | 3 | 5 | 1 | — | — | 7 | 15 | 745 |
| 10 | 3 | 5 | 1 | — | — | 7 | 15 | 745 |
| 11 | 4 | 6 | 1 | A=1 | — | 10 | 16 | 1056 |
| 12 | 4 | 6 | 1 | A=1 | 4 | 10 | 16 | 1056 |
| 13 | 4 | 6 | 1 | A=1 | 5 | 10 | 18 | 1058 |
| 14 | 4 | 6 | 1 | A=1 | 4 | 10 | 18 | 1058 |
| 15 | 4 | 6 | 1 | A=1 | 4 | 10 | 18 | 1058 |
| 16 | 4 | 6 | 1 | A=1 | 5 | 10 | 20 | 1060 |
| 17 | 4 | 6 | 1 | A=1 | 4 | 10 | 20 | 1060 |
| 18 | 5 | 7 | 1 | A=1 | 4 | 17 | 22 | 1772 |
| 19 | 5 | 7 | 1 | A=1 | 5 | 17 | 25 | 1775 |
| 20 | 5 | 7 | 1 | A=1, S=4 | 5 | 17 | 25 | 1775 |
| 21 | 5 | 7 | 1 | A=1, S=3 | 4 | 17 | 25 | 1775 |
| 22 | 5 | 7 | 1 | A=1, S=4 | 4 | 17 | 25 | 1775 |
| 23 | 5 | 7 | 1 | A=1, S=4 | 4 | 17 | 25 | 1775 |
| 24 | 5 | 7 | 2 | A=1, S=2 | 5 | 17 | 25 | 1775 |
| 25 | 5 | 7 | 2 | A=1, S=4 | 5 | 17 | 25 | 1775 |
| 26 | 6 | 8 | 2 | A=1, S=1 | 4 | 20 | 30 | 2090 |
| 27 | 6 | 8 | 2 | A=1, S=3 | 5 | 20 | 30 | 2090 |
| 28 | 6 | 8 | 2 | A=1, S=5 | 5 | 20 | 30 | 2090 |
| 29 | 6 | 8 | 2 | A=1, S=4 | 4 | 20 | 30 | 2090 |
| 30 | 6 | 8 | 2 | A=1, S=4 | 4 | 20 | 30 | 2090 |
| 31 | 6 | 8 | 2 | A=1, S=2 | 5 | 20 | 30 | 2090 |
| 32 | 6 | 8 | 2 | A=1, S=2 | 5 | 20 | 30 | 2090 |
| 33 | 7 | 9 | 2 | A=1, S=5 | 5 | 23 | 30 | 2400 |
| 34 | 7 | 9 | 2 | A=1, S=2 | 4 | 23 | 35 | 2405 |
| 35 | 7 | 9 | 2 | A=1, S=6 | 4 | 23 | 35 | 2405 |
| 36 | 7 | 9 | 2 | A=1, S=6 | 5 | 23 | 35 | 2405 |
| 37 | 8 | 10 | 2 | A=1, S=7 | 5 | 26 | 38 | 2718 |
| 38 | 8 | 10 | 2 | A=1, S=0 | 5 | 26 | 40 | 2720 |
| 39 | 8 | 10 | 2 | A=1, S=5 | 5 | 26 | 40 | 2720 |
| 40 | 8 | 10 | 2 | A=1, S=6 | 4 | 26 | 40 | 2720 |

## 验证证据与剩余风险

1. `bash scripts/run_tests.sh` 已执行：场景构建成功；**EditMode 142/142 passed, 0 failed**。证据：`build/test-results.xml`、`build/editmode-tests.log`。Core 49、Level 73、其他已有测试 20；全部 40 关都实际重算、核对 solverMinMoves 并逐步回放，不是抽样或仅读取入库标记。
2. 小盘面 4 个随机种子与不使用启发式的朴素 BFS 对照，另有短枝+篮子的 BFS 对照；包含真实无解样本。多种子生成并复核、JSON v1 拒绝、结构错误/锁死/预算不足/最短步数造假拒绝均通过。
3. 使用 Unity 自带 Mono 独立编译全部 Core，未引用 UnityEngine/UnityEditor，40 关完整生成成功；Core 源码中也无 Unity 引用。
4. **实际 Unity batchmode 入口** `-executeMethod LevelBuilder.BuildAll` 已执行成功（退出 0），重新生成 40 关，日志 `build/t9-level-build.log` 最终为 `Built all 40 v2 levels with proven shortest solutions.`；最大状态数为第 38 关的 1,918，最低至 20。最终 JSON 确认 40 关、version 全为 2、容量集合为 4/5/6。
5. **PlayMode 5/11 passed，6 failed**：证据 `build/playmode-results.xml`、`build/playmode-tests.log`。以下文件留给 T10 按协调者要求迁移：
   - `GameplaySmokeTests.Level01 / Level10 / Level20 / Level30 / Level40`：在 `Assets/Tests/PlayMode/GameplaySmokeTests.cs:77` 断言撤销最终归巢后有 4 只；v2 正确返回 6 只，五项均 Expected 4 / Actual 6。在此失败前已经完成整关求解、表现同步检查和通关。
   - `PointerCancelIllegalMoveAndResumePreserveState`：同文件约 96 行通过 `Birds.Count == 4` 找满枝，v2 盘面没有该满枝，结果 -1 被传入旧 BoardView，最终在 `BoardView.cs:141` 的 Reject 动画索引越界。应将测试目标和表现层同步迁移到每枝 Capacity；不要把 Core 改回容量 4。
6. 没有实现或验收 v2 表现：旧 View 仍只飞单只、不消费事件链，不画目标/箱/篮子/短枝/左右镜像，也未处理睡眠冻结的选中反馈。虽然可编译并在 Sync 后显示最终鸟列表，这不证明 v2 交互和动画已交付。T10 必须按上表迁移。
7. 本任务未修改存档 schema / 旧操作回放迁移；旧操作相同 From/To 在 v2 会搬整个 run，T10 需要显式处理旧存档版本，不能直接复用为 v2 历史。
8. 规则实现、纯逻辑、数据及最优解验证已完成；完整产品验收与 PlayMode 全绿仍待 T10。本报告保留原脚本失败状态，不将 Core 全绿写成全项目全绿。

## 修改文件

- `PerchSort/Assets/Scripts/Core/BoardState.cs`
- `PerchSort/Assets/Scripts/Core/Move.cs`
- `PerchSort/Assets/Scripts/Core/Rules.cs`
- `PerchSort/Assets/Scripts/Core/Solver/Solver.cs`
- `PerchSort/Assets/Scripts/Core/Generator/LevelGenerator.cs`
- `PerchSort/Assets/Scripts/Core/LevelData.cs`
- `PerchSort/Assets/Editor/LevelBuilder.cs`
- `PerchSort/Assets/Resources/Levels/levels.json`
- `PerchSort/Assets/Tests/EditMode/CoreTests.cs`
- `PerchSort/Assets/Tests/EditMode/LevelTests.cs`
- `tasks/T9_report.md`

`GameSession.cs` 已审阅并由撤销/重开测试验证，原实现无需修改。未提交 git。
