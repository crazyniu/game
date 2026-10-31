# T4 — 游戏场景交付报告

日期：2026-10-31。工作目录：`/Users/apple/Documents/projects/game`。状态：实现及自动化已交付，待协调者视觉验收；无 Git 提交。

## 范围

完整读取 `tasks/T4_gameplay.md`、`docs/DESIGN.md`、`art/ASSETS.md` 和 `art/manifest.json`，基于既有 Core 与 40 关数据实现表现层。没有修改 Core、关卡数据、`art/` 原件或依赖配置，没有广告、内购或在线服务。导入的 14 张 PNG 与 `art/raw` 逐字节一致。

`SceneBuilder` 按 T4 要求将 Game 设为唯一 Build Settings 场景，因此生成 `ProjectSettings/EditorBuildSettings.asset` 的变化；协调者已通过 `msg_efc62ac504d0` 明确允许此项。其余 ProjectSettings 与开始时校验和一致。

## 文件清单

以下新增资源、目录均包含 Unity 自动生成的 `.meta`；已有 `Editor/LevelBuilder.cs`、Core 和 EditMode 测试未改。

| 路径（相对项目根） | 内容 |
| --- | --- |
| `PerchSort/Assets/Art/birds/bird_01.png`～`bird_10.png` | 10 种鸟的原图复制及 Sprite 导入参数 |
| `PerchSort/Assets/Art/env/{background,perch,tree_crown}.png` | 背景、栖木、树冠 |
| `PerchSort/Assets/Art/ui/icon_1024.png` | 图标原图复制；未改 Player Settings 图标配置 |
| `PerchSort/Assets/Scenes/Game.unity` | SceneBuilder 生成的场景，不手写 YAML |
| `PerchSort/Assets/Scripts/View/PerchSort.View.asmdef` | 表现程序集，引用 Core 与 uGUI |
| `PerchSort/Assets/Scripts/View/BirdView.cs` | 物种、贴图、脚部 pivot、缩放、层级、透明度 |
| `PerchSort/Assets/Scripts/View/PerchView.cs` | 栖木、槽位、整根栖木上方点击区域、选择高亮 |
| `PerchSort/Assets/Scripts/View/BoardView.cs` | 输入入口、Core 会话、表现同步、协程动画、操作锁、撤销/重开 |
| `PerchSort/Assets/Scripts/UI/PerchSort.UI.asmdef` | UI/流程程序集 |
| `PerchSort/Assets/Scripts/UI/GameController.cs` | 关卡加载、流程、稳定状态保存、恢复移动历史 |
| `PerchSort/Assets/Scripts/UI/GameHud.cs` | 顶部关卡/收集数、底部按钮、通关/死局/40 关结束提示 |
| `PerchSort/Assets/Scripts/UI/ProgressStore.cs` | v1 JSON、临时文件、替换前上一份 `.bak` |
| `PerchSort/Assets/Scripts/UI/SafeAreaLayout.cs` | 相机与 UI 同步适配屏幕、安全区、背景铺满 |
| `PerchSort/Assets/Scripts/UI/CaptureRoutine.cs` | 仅 Editor 编译的截图协程宿主，不进入 Player |
| `PerchSort/Assets/Editor/ArtImporter.cs` | 读取 manifest 的脚底 pivot，统一 Sprite/PPU/filter/mipmap/compression |
| `PerchSort/Assets/Editor/SceneBuilder.cs` | 一键生成场景与 Build Settings |
| `PerchSort/Assets/Editor/Screenshots.cs` | 菜单和 batchmode 截图入口、临时存档隔离、捕获期间锁定程序集重载 |
| `PerchSort/Assets/Editor/PerchCaptureSequence.cs` | 实际 Play Mode 输入/求解、相机 RenderTexture 读回 |
| `PerchSort/Assets/Tests/PlayMode/PerchSort.Gameplay.PlayModeTests.asmdef` | PlayMode 测试程序集 |
| `PerchSort/Assets/Tests/PlayMode/GameplaySmokeTests.cs` | 7 项验收烟测 |
| `scripts/run_tests.sh` | 先重建场景，再执行 EditMode + PlayMode 并校验 XML |
| `scripts/screenshots.sh` | 有图形设备的 batchmode 截图与 PNG 尺寸检查 |
| `PerchSort/ProjectSettings/EditorBuildSettings.asset` | 生成：仅启用 Game 场景（已获协调者授权） |
| `tasks/T4_report.md` | 本报告 |

## 场景与脚本结构

```text
Game
├─ Main Camera（Orthographic、Physics2DRaycaster）
├─ Forest Background
├─ Safe Playfield
│  ├─ Treetop Home
│  └─ Board / BoardView
│     ├─ PerchView × P（运行时生成）
│     └─ BirdView × 剩余鸟数（运行时生成）
├─ HUD Canvas（Screen Space Camera、GraphicRaycaster）
│  └─ Safe HUD 1080x1920
│     ├─ 品牌、关卡、收集数
│     ├─ 提示、撤销、重开
│     └─ Result Overlay / Result Card
├─ Game Flow（GameController、SafeAreaLayout）
└─ EventSystem（InputSystemUIInputModule）
```

- 每行两根栖木，奇数最后一根居中；6～12 根对应 3～6 行。行距根据关卡自动变化，保留顶部树冠和底部按钮区域。
- 相机以 10.8×19.2 世界单位为参考；棋盘与固定 1080×1920 的 HUD 共用安全区域等比缩放。背景单独铺满，窗口尺寸/Safe Area 变化时更新。没有修改项目输入后端，使用已有 Input System，触屏和鼠标经 EventSystem 的同一 PointerClick 入口。
- Sprite 全部 PPU=100、Full Rect、Bilinear、无 mipmap、无压缩，鸟 pivot 直接从 manifest 读取。鸟以可见身体宽度缩放；长尾鹦鹉和剪尾燕采用身体宽度参数，避免按 1024 画布或长尾最低点落位。
- 选择只弹起最右鸟；点击自己取消；非法目标抖动并取消；空栖木不能选中。栖木 BoxCollider2D 为 4.65×1.55 世界单位，覆盖整根及上方区域。
- `BoardView.SelectPerch` 是真实输入、测试和截图共用入口。Core 先产生权威结果，表现层播放抛物线、落地挤压、错开收集及通关飞鸟；期间所有棋盘操作和 HUD 操作锁定。结束时逐槽强制校对状态、位置、缩放及透明度。
- 撤销通过 Core Undo 还原，再对同种鸟平滑重新落位；被收集的鸟从树冠返回。重开清除撤销历史；通关显示下一关/重玩；第 40 关显示全部完成并可从第 1 关重玩。死局只提供撤销/重开。

## 存档

正式路径：`Path.Combine(Application.persistentDataPath, "progress.json")`。

v1 JSON 保存 `version`、`levelId`、当前有效 `moves[{from,to}]`。从原关卡重放这些合法移动恢复 `GameSession`，同时恢复无限撤销历史；不序列化 Unity 对象，也不直接修改 Core 状态。仅在一段操作动画全部完成，或加载新关卡后保存；选择/取消不触发写盘。

先写 `.tmp`，再用 `File.Replace` 替换并保留上一份 `.bak`（第一次用 Move）。已有存档 version 不支持或记录不合法会明确抛错，不悄悄覆盖；不加入未要求的迁移/损坏恢复框架。测试和截图通过 `ProgressPathOverride` 使用临时存档，结束后清理，不碰玩家存档。

## 重建、运行与截图

使用已安装的 Unity **6000.6.7f1**。从仓库根执行：

```sh
bash scripts/run_tests.sh
bash scripts/screenshots.sh
```

可用 `UNITY_EDITOR` 环境变量覆盖 Unity 可执行文件路径。执行前关闭占用同一项目的其他 Unity Editor 实例。

在 Unity 菜单中：

- `Perch Sort/Import Art`：按 manifest 重导入素材。
- `Perch Sort/Rebuild Game Scene`：重建并打开 Game 场景，更新唯一 Build Settings 场景；按 Play 可运行。
- `Perch Sort/Capture Acceptance Screenshots`：重建场景、进入 Play Mode、截取验收状态，完成后退出 Play Mode。

仅重建场景的 CLI：

```sh
/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit -projectPath "$PWD/PerchSort" \
  -executeMethod SceneBuilder.Build -logFile "$PWD/build/scene-build.log"
```

截图必须有图形设备，脚本刻意不使用 `-nographics` 和 `-quit`；从真实 Game 场景运行，指定 1080×1920 RenderTexture，并等待渲染帧完成后读回 PNG，不是美术合成图。捕获期间锁住程序集重载，解决此 Unity 版本 batch 启动后自动重载导致协程停止的问题。

## 验证证据

`bash scripts/run_tests.sh`：**EditMode 84/84 通过，PlayMode 7/7 通过，0 失败**。

- XML：`build/test-results.xml`、`build/playmode-results.xml`。
- 日志：`build/scene-build.log`、`build/editmode-tests.log`、`build/playmode-tests.log`。
- 5 项指定关卡烟测：1、10、20、30、40，用 Solver 的解经 BoardView 输入入口通关。每次动画结束检查物种/槽位/位置与 Core 一致；飞行期间插入额外点击与撤销以确认输入锁；断言通关面板；再撤销一次，断言收集的四只鸟回来；重新通关并验证下一关，40 关回到第 1 关。
- 1 项输入/存档烟测：EventSystem PointerClick、空栖木、选中取消、非法满栖木、合法移动、备份存在、卸载重载实际 Game 场景、恢复棋盘/历史、撤销/重开。
- 1 项死局面板烟测：使用满栖木 Core 会话触发，无需改变规则。
- 测试结束及各动画阶段 `LogAssert.NoUnexpectedReceived()`，无游戏异常日志。

截图脚本输出如下，均为 **1080×1920**：

| 文件 | 内容 |
| --- | --- |
| `build/screens/01-level-01.png` | 第 1 关开局 |
| `build/screens/02-level-20.png` | 第 20 关开局 |
| `build/screens/03-selected.png` | 第 20 关最右鸟选中弹起及栖木高亮 |
| `build/screens/04-win.png` | 通过输入入口求解第 20 关后的通关面板 |
| `build/screens/05-level-40.png` | 额外第 40 关最密集布局检查 |

日志：`build/screenshots.log`。已逐张查看首关、第 20 关、选中、通关、第 40 关的实际输出；修复首次读回纹理过早的问题后重新生成，首关鸟/栖木/树冠均正常，按钮和面板文字清晰，脚部落在栖木上沿。

## 已知限制及验收边界

- 本轮验证为 macOS Unity Editor Play Mode 与相机实际渲染；未构建或安装 Android/iOS 包，未声称已完成真机触摸、刘海设备或所有屏幕比例验收。Safe Area 适配已实现。
- HUD 当前使用英文和 Unity 内置字体；没有引入外部字体/网络依赖。菜单、选关、设置、音效按任务边界留后续。
- 保留长尾鸟的自然尾羽，小部分尾尖可越过本槽进入邻槽的空隙；主体、脚部站位与栖木不严重重叠或出屏。最终美术/视觉判断交协调者逐张验收。
- Unity 启动日志可能包含许可证 token 刷新、在线遥测连接失败及原生线程清理信息；这不是游戏业务异常。游戏不依赖这些服务。
