# T8 — 9:16 比例适配交付

日期：2026-10-31。实现与验证完成，截图供协调者最终视觉验收；没有提交 git。

## 实现

- `Assets/Editor/PhoneGameView.cs` 使用 `[InitializeOnLoad]` 和 GameViewSizes 反射，注册/复用 `Phone 9:16` 固定分辨率 1080×1920，并自动选中。等启动导入和布局加载完成后执行，避免启动时 `delayCall` 丢失。手动入口：Perch Sort → Phone 9:16 Game View。
- `Assets/Editor/ReleaseSettings.cs` 设置桌面窗口 540×960、Windowed、可调整大小、非原生全屏分辨率，保留 Portrait 方向；ProjectSettings 经该 Editor 脚本生成。
- `Assets/Scripts/UI/SafeAreaLayout.cs` 提供 `ViewportFor(width, height)`：宽于 9:16 时居中柱状黑边，等宽或更长时全屏视口。安全区取屏幕安全区与相机视口交集，HUD 和世界内容使用相同缩放与中心偏移；长屏延展背景。
- `Assets/Editor/SceneBuilder.cs` 添加只清黑色、不绘制对象的背景相机，确保缩放窗口后两侧仍是黑边。`run_tests.sh` / 截图脚本按既有流程重新生成 `Assets/Scenes/Game.unity`。
- `Assets/Tests/EditMode/AspectTests.cs` 添加一个参数化视口计算测试，覆盖三个要求比例。
- `Assets/Editor/AspectCaptureSequence.cs`、`Screenshots.cs` 和 `scripts/screenshots_aspect.sh` 添加三比例截图流程，直接抓取真实 Game 屏幕（含两个相机及 Canvas），沿用临时存档隔离。

没有修改 Core 或 `art/` 原件。Unity 自动生成新增脚本的 `.meta` 文件；场景及既有生成资产由原有 Editor 构建流程写入。

## 验证

`bash scripts/run_tests.sh` 成功退出：

- EditMode：104/104 通过，0 失败；`build/test-results.xml`。
- PlayMode：11/11 通过，0 失败；`build/playmode-results.xml`。
- 日志：`build/scene-build.log`、`build/editmode-tests.log`、`build/playmode-tests.log`。

`bash scripts/screenshots_aspect.sh` 成功退出；六张 PNG 的实际尺寸均通过脚本校验。逐张查看菜单和最密集的第 40 关，标题、按钮、棋盘没有比例拉伸、裁切、错位或重叠；9:19.5 上下延展背景，3:4 左右柱状黑边。截图日志记录实际 Screen 尺寸与 Camera.rect：`build/screenshots-aspect.log`。

| 比例 | 分辨率 | 视口 x/y/w/h | 菜单 | 游戏（第 40 关） |
|---|---|---|---|---|
| 9:16 | 1080×1920 | 0 / 0 / 1 / 1 | [菜单](../build/screens/aspect/1080x1920-menu.png) | [游戏](../build/screens/aspect/1080x1920-game.png) |
| 9:19.5 | 1080×2340 | 0 / 0 / 1 / 1 | [菜单](../build/screens/aspect/1080x2340-menu.png) | [游戏](../build/screens/aspect/1080x2340-game.png) |
| 3:4 | 1440×1920 | 0.125 / 0 / 0.75 / 1 | [菜单](../build/screens/aspect/1440x1920-menu.png) | [游戏](../build/screens/aspect/1440x1920-game.png) |

## 编辑器最终状态

只退出了 projectPath 为 PerchSort 的原编辑器（PID 39601），未操作其他项目。完成 batchmode 工作后按任务指定路径和场景参数重启，最后编辑器 PID 为 **42687**：

```text
/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -projectPath /Users/apple/Documents/projects/game/PerchSort -openScene Assets/Scenes/Game.unity -logFile /Users/apple/Documents/projects/game/build/unity-editor.log
```

冷启动日志确认 `Game view selected: Phone 9:16 (1080x1920), index 10`，并通过原生窗口截图亲眼核实工具栏所选尺寸。随后进入 Play 模式，实际菜单完整显示于 9:16 画面；编辑器保持打开，停留在菜单等待协调者验收。首次冷启动发现自动选择时序问题后只修改了 Editor 启动回调，并以再次冷启动和真实 Play 画面验证；运行时及测试代码未再变化。

当前三比例证据来自 Unity 6000.6.7f1 Editor 实际 Play 渲染；未另行打包独立播放器或进行带刘海真机验证。

反射接口核对参考：[Unity 官方 GameView 源码](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/GameView/GameView.cs)、[GameViewSizes 源码](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/GameView/GameViewSizes.cs)。
