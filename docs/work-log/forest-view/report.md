# T10b — 表现层与玩法接入 v2

日期：2026-10-31。目录：`/Users/apple/Documents/projects/game`。依据：`docs/DESIGN.md`、`tasks/T10b_view2.md`、`tasks/T9_report.md`。

## 交付与验证

表现层 v2 已实现；`bash scripts/run_tests.sh` 最终退出 0，**EditMode 143/143、PlayMode 15/15，零失败**。`bash scripts/screenshots.sh` 最终退出 0，生成 **13 张 1080×1920** 截图；已打开检查中期关、选中态、十栖木关卡、睡眠、归巢和结算画面。等待 Claude 按任务验收，正式新美术由 T10c 接入。

- 测试证据：`build/test-results.xml`、`build/playmode-results.xml`、`build/editmode-tests.log`、`build/playmode-tests.log`。
- 截图证据：`build/screenshots.log`、`build/screens/*.png`。
- 未修改 Core、关卡数据、`art/`，未新增包，未提交 git。Unity 测试与截图进程均已正常退出。

## 实现

### 棋盘与输入

- `BoardView` 按 `PerchState.Side` 分左右两列，各列独立计数；右列镜像，逻辑 `Birds[0]` 靠墙，最后一只朝中央。宽度为 `Capacity × 0.72`，短枝贴墙放置；每列行距由行数计算，现有 5–10 栖木关卡落在 HUD 与篮子之间。
- `Perches` 仍仅表示栖木；新增 `Locations` 覆盖栖木和篮子，索引与 Core 一致。底部篮子使用与栖木相同的点击入口。
- 顶部目标显示全部尚未归巢目标；前两个放大、高亮，其余缩小排队，并叠加对应鸟图标。进度读取目标收集数，不再用鸟数除以 4。
- `Rules.GetRun` 控制整个外露连续段的抬升、白描边与星点；再次点击来源或空白取消。非法目标取消选择并轻抖目标；睡眠/箱体不可选。所有选择、移动、事件动画期间锁输入。
- `BlankBoardInput` 位于栖木碰撞体后方，用同一 EventSystem 接收空白点击。

### 事件与动画

唯一播放入口消费 **`result.Move.Events`**，没有引用旧 `Collected*` 摘要：

| Core 事件 | 实际表现 |
|---|---|
| `Moved` | 依事件 Count 搬运对应 BirdView，逐只间隔 60ms 沿弧线飞行，朝向目的地，落地压缩回弹；包含篮子和部分移动 |
| `Completed(Asleep)` | 全排压低、变暗，使用睡眠图或闭眼占位覆盖，间歇上飘 Z |
| `Completed(Homed)` / `AutoHomed` | 全排逐只飞向**该种对应的当前巢**，缩小淡出，巢口闪光、对勾和下落羽毛 |
| `TargetsAdvanced` | 已收集巢向上退出，未收集队列左移补位，新激活巢放大，使用事件携带的 ActiveTargets / CollectedCount |
| `CrateRevealed` | 箱体倾斜下落并淡出，被遮挡鸟排浮起后落稳 |

非移动事件后保留 110ms 可读停顿；整个链结束才同步最终状态、开放输入并发 `Settled` 存档。新增 `PlayingEvent` / `EventPlayed` 供截图和测试观察事件进度。

撤销/重开直接按 Core 快照快速重建鸟列表及位置，恢复睡眠、箱体、篮子和目标队列。通关沿用现有结果页，前置彩纸庆祝；死局沿用撤销/重开恢复页。GameController 已有的资源加载入口直接消费 T9 的 40 关 v2 JSON，无需另写关卡解析器。

Reduce motion 缩短飞行、选择、队列切换和事件间停顿，去弧线/倾斜/落地挤压/抖动/彩纸/羽毛；Z 静态显示，仍严格依事件顺序播放。

### 存档

`ProgressData.version = 3`。加载版本 1 **或 2** 时，丢弃旧操作和通关记录，写入第 1 关空进度；不把旧单鸟操作回放到 v2 关卡。沿用原子替换与 `.bak` 上一份快照机制；迁移这次替换保存旧文件，但后续正常存档仍会更新 `.bak`，它不是永久迁移档案。未知未来版本仍沿用明确报错，不做静默猜测。

### 分配与资源

协程内插值使用预先建立的数组/List，逐帧循环不创建集合、不 LINQ、不加载资源。鸟节点在选中、飞行、睡眠和移动之间复用，Sync 只增删需要变化的节点；占位 Sprite 以资源键缓存，选中轮廓共享材质。羽毛/庆祝节点按事件创建并及时销毁，并未引入每帧对象生成或新的对象池框架。本次通过代码检查确认自身更新路径无逐帧托管分配点；未进行设备 Profiler 或真机性能验收。

## 测试变化

- 原 6 个 v1 PlayMode 失败已迁移：5 个整关流程撤销后的鸟数从 4 改为 6；非法目标查找改用每枝 Capacity。
- 原 11 个 PlayMode 测试继续通过，包括 1/10/20/30/40 关解题、存档续玩、通关/解锁、死局、设置与 Reduce motion。
- 新增 4 个 PlayMode：整段选中/空白取消/整段移动/篮子部分移动及往返；睡眠→归巢→自动归巢→揭箱→再次归巢链与撤销；同链的 Reduce motion 顺序；旧 v1 存档带无效旧索引时仍从 v2 第 1 关启动。
- `MatchesState()` 核对实际 BirdView 数量、种类、位置，以及睡眠、箱体显隐、容量、篮子和激活目标；连锁测试撤销后另外比对完整 Core 状态键。
- EditMode 存档迁移改为 v1/v2 两例，检查 v3/第 1 关/空历史/空解锁及迁移时备份。

## 截图

| 文件 | 内容 |
|---|---|
| `01-level-01.png` | 5 栖木教学关 |
| `02-level-20.png` | 实际第 20 关：箱、篮、短枝及目标队列 |
| `03-selected.png` | 第 20 关外露段选择反馈 |
| `04-win.png` | 第 20 关完成后的结果页及 5/5 进度 |
| `05-level-40.png` | 实际第 40 关：10 栖木、8 目标、2 篮、短枝 |
| `06-menu.png`～`11-settings-reduced-motion.png` | 主菜单、关卡选择、设置、暂停、死局、降低动态设置 |
| `12-sleeping.png` | 专用事件夹具：睡眠鸟排、Z、箱、篮、短枝 |
| `13-homing.png` | 同一夹具的实际协程归巢飞行中间帧 |

12/13 为可重复构造的规则事件夹具，HUD 的 LEVEL 01 沿用当前控制器关号，**不是发布第 1 关布局**。原截图脚本校验 11 张既有图，两个新增图由 `Screenshots.RunV2` 在同一次脚本运行前置捕获，另已读取 PNG 头核验全部 13 张尺寸。

## T10c 占位资源契约

以下是 `Resources.Load<Sprite>` 的精确资源键（不带扩展名），缺失时使用代码占位；新素材可放在任何 `Resources/` 下相应子路径，导入为 Sprite。

| 键 | 当前占位 / 接入说明 |
|---|---|
| `env/nest` | 编织椭圆巢；Sprite 居中，按约 1.55×1.4 的局部缩放绘制 |
| `env/basket` | 编织篮；居中，篮底位于站位下方 |
| `env/crate` | 木箱横板/斜撑；居中，宽度随栖木容量变化 |
| `fx/star` | 金色四角星，选中反馈 |
| `fx/flash` | 金色入口闪光 |
| `fx/feather` | 米色羽毛 |
| `ui/check` | 绿色圆底白色勾 |
| `fx/sleep_eyes` | 白色眼睑底/深色闭眼线，按现有鸟的眼睛锚点染色覆盖；有睡眠整图时不显示 |
| `birds/bird_01_sleep` … `birds/bird_10_sleep` | 可选睡眠整图；未提供时复用原鸟并压低/变暗/闭眼覆盖。应与 awake 图保持同画布、PPU、脚底 pivot |

睡眠 Z 当前为 TextMesh；彩纸沿用 `CelebrationBurst` 的代码粒子；枝条仍使用 SceneBuilder 配置的既有 `Assets/Art/env/perch.png` 并按容量缩放。T10c 若换用独立长短枝、Z、彩纸图，需要在对应组件接入，不会自动按尚未约定的文件名查找。新 env/fx/ui 资源建议按 1 世界单位的 Sprite bounds 导入，或同步调整 ViewArt 调用处的尺寸；当前占位为 128px / 128 PPU。

## 任务未指定处的决定

1. 非法目标统一采用**取消选择**，不自动换选；空白同样取消，符合任务给出的取消/换选二选一。
2. 撤销采用瞬时状态重建，不增加反向动画；确保一次撤销还原整次连锁。
3. 旧存档版本 2 同样作废，因为它仍然使用 v1 关卡/单鸟操作；不是只有编号 1 才属于旧规则。
4. 已收集目标在对勾与上退出动画后从可见队列移除；“显示全部目标”落实为显示全部**未收集**目标，与任务要求旧巢退出一致。
5. 揭箱使用下落/倾斜/淡出方案，不制造碎片物理体。
6. 睡眠整图尚未接入时，用每种现有图的眼睛锚点放置闭眼覆盖；真实睡眠素材存在时自动关闭覆盖。
7. 在授权的 `GameHud.cs` 内先替换两条 v1 英文提示，说明外露同种段与篮子；未扩大修改 `Strings.cs` 所有权，T11 可将新提示并入正式文案表。
8. 新睡眠/归巢截图用专门构造的事件夹具，避免把发布关卡求解路径是否碰巧睡眠作为截图前提；未修改入库关卡。
9. `ProgressStore.cs` 虽未列入开头目录清单，但任务第 6 条明确要求修改它并迁移 v3，按这一明确要求执行。
10. 去掉原场景中单一的大树冠主页饰物，给有规则意义的目标巢队列腾出顶部区域；结果页继续保留原树冠装饰。

## 修改文件

- `Assets/Scripts/View/BoardView.cs`、`BirdView.cs`、`PerchView.cs`。
- 新增 `Assets/Scripts/View/BlankBoardInput.cs`、`TargetQueueView.cs`、`ViewArt.cs`、`FallingFeathers.cs` 及 Unity `.meta`。
- `Assets/Scripts/UI/GameHud.cs`、`ProgressStore.cs`。
- `Assets/Editor/SceneBuilder.cs`、`Screenshots.cs`。
- `Assets/Tests/PlayMode/GameplaySmokeTests.cs`、`Assets/Tests/EditMode/PersistenceTests.cs`。
- 标准脚本重建 `Assets/Scenes/Game.unity`，生成测试日志/XML、截图；`tasks/T10b_report.md` 为本报告。

正式鸟巢/箱/篮/特效美术、结算页大奖杯角色和 T11 的新手引导/迁移提示不在本次新增美术实现中；本次完成的是任务授权的占位表现与现有结算页接入。最终视觉验收待 Claude，真机性能另行验收。
