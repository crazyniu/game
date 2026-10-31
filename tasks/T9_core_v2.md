# T9 — Core 规则 v2（逻辑层 + 求解器 + 生成器 + 40 关）

权威规则见 `docs/DESIGN.md` 的「核心规则 v2」。**只改逻辑层与关卡，不碰 View/UI/场景**（T10 另派）。目标：把 Core 从 v1（容量4/单只/任意同种清空）改成 v2（容量6/连续同种段整体移动/外露端/顶部目标队列/睡眠与自动归巢/板条箱/篮子）。

## 目录所有权
你可改：`PerchSort/Assets/Scripts/Core/**`、`PerchSort/Assets/Editor/LevelBuilder.cs`、`PerchSort/Assets/Resources/Levels/levels.json`、`PerchSort/Assets/Tests/EditMode/**` 中与 Core/关卡相关的测试、`tasks/T9_report.md`。
不要改 View/UI/Audio/场景。View/UI 此刻会编译失败是预期的——为让项目编译，**在 Core 中尽量保留旧 API 名称不现实时**，请在报告里列出所有对 View/UI 有影响的公开 API 变化（旧→新），T10 将据此改表现层。若 View/UI 编译错误会阻塞 EditMode 测试，可用最小垫片（仅让其编译，不实现功能）并在报告标明。

## 必须实现（Core，纯 C#，无 UnityEngine）
1. `BoardState`：capacity=6；栖木含 `Birds`(List/array，下标0靠墙、末端外露)、`Side`(Left/Right)、`Asleep`、`Crate`(null 或 {RevealAfter / RevealSpecies})；篮子（容量1，数量由关卡定）；`Targets` 有序队列 + 已收集数；`CompletedCount`（累计完成栖木数）；`Clone()`；稳定的哈希/等价键（供求解器去重，**睡眠、板条箱状态、目标进度都必须入键**，同构栖木排序规约可选）。
2. `Move`：来源、目标、实际移动数 n、以及执行后产生的**事件列表**（按顺序：Moved、Completed(asleep/homed, shelf, species)、TargetsAdvanced、AutoHomed(shelf)、CrateRevealed(shelf)），供表现层逐步播放。
3. `Rules`：`GetRun(state, shelf)`（外露端连续同种段长度）；`GetLegalMoves`；`TryApply`；`IsWon`；`IsDeadlock`。严格按 DESIGN 的合法性、部分移动 min(run, 空位)、睡眠冻结、激活目标=队列未收集前2个、集齐即归巢/睡眠、目标刷新后自动归巢连锁（按栖木编号稳定顺序）、完成后板条箱揭示检查（先结算自动归巢连锁，再统一检查揭示，直到稳定）。篮子：只能整体收 1 只（run 只取最外 1 只）、从篮子取出同理；篮子里的鸟可去空栖木或同种外露端栖木。无效/非法移动返回失败且不改状态。
4. `GameSession`：undo 栈（克隆快照）、Restart；撤销一次还原整次操作（含其引发的全部连锁）。
5. `Solver`：BFS/A* 有节点预算，返回 {Solvable, MinMoves（找到最优则标最优，否则标 upper bound 并区分）, Proven}。启发式必须可采纳。须有死局剪枝（无合法移动即死）。
6. `LevelGenerator`：从“已完成”倒推或受控随机+求解器验证，生成满足 v2 的关卡；`LevelValidator`：校验鸟总数（每种恰 6 只）、栖木数/空位、目标队列覆盖全部种类且无重复、板条箱引用合法、**求解器证明可解**。`LevelData` JSON v2（字段见 DESIGN），DataContractJsonSerializer；旧 v1 文件读取应明确报版本不符（不静默吞）。
7. 40 关（`levels.json` v2）：K 从 3 递增到 ≤8；教学关（1–3）无板条箱无篮子且目标队列与栖木上的种类自然对应；前 10 关无板条箱，之后逐步引入 `RevealAfter`/`RevealSpecies`；篮子 0→2；难度（求解器 MinMoves、种类数、被异种埋住程度）整体单调爬升且**无一关不可解**。每关写入 solverMinMoves。若某 K 的求解在预算内证明不了，降低 K 或收窄，别入库。LevelBuilder 提供 Unity 菜单与 batchmode 入口重新生成。

## 测试（EditMode，新增或改写；总数不得低于旧的 104 里属于 Core/关卡的部分）
至少覆盖：run 计算（含被异种隔开）、部分移动、非法移动不改状态、同种外露端目标、空栖木目标、篮子往返、睡眠冻结与占位、睡眠后目标刷新自动归巢、**连锁归巢**、归巢时队列补位、板条箱两种揭示、揭示时机（睡眠也计入）、撤销还原整个连锁、重开、死局检测、胜利、哈希区分睡眠/目标进度、求解器与朴素 BFS 在小盘面上的最少步数一致（随机若干小盘面）、Generator 产出必可解（多种子）、LevelValidator 各类拒绝、40 关全部通过校验且 solverMinMoves 与求解器重算一致（重算可限时抽样，但**全部 40 关必须至少证明可解**）、v1 文件拒绝。

## 约束与验证
- 不提交 git；不改 art/。
- 编辑器此刻由我打开着：开工前先退出 projectPath 为 PerchSort 的那个 Unity 进程（仅它），否则 batchmode 抢锁。完工后不必重开。
- 跑 `bash scripts/run_tests.sh` 并在报告贴结果与用例数。
- 报告 `tasks/T9_report.md`：做了什么、公开 API 变更表（旧→新）、关卡难度表（id/K/栖木数/篮子/箱/minMoves）、规则里我没明确而你自行决定的点（逐条列出，不许沉默决定）、未完成/风险。

## 验收（Claude 亲自）
重跑测试全绿；抽查 levels.json 结构；自己写脚本复核若干关；审阅规则歧义清单。
