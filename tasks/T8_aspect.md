# T8 — 画面比例固定为 9:16（手机竖屏）

用户在 Unity 编辑器里直接玩，要求游戏比例为 **9:16**。

## 前置
Unity 编辑器此刻正由我（协调者）打开着该项目。你开工前先**退出该编辑器进程**（只退出 `projectPath` 为 PerchSort 的那个 Unity，不要动别的），否则 batchmode 会抢锁。全部完成后，**重新启动编辑器**：
`/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity -projectPath /Users/apple/Documents/projects/game/PerchSort -openScene Assets/Scenes/Game.unity`（后台运行，日志写 build/unity-editor.log）。

## 做什么
1. **编辑器 Game 视图**：写 `[InitializeOnLoad]` 的 Editor 脚本，通过 GameViewSizes 反射添加并自动选中自定义尺寸 “Phone 9:16”（固定分辨率 1080×1920 或 AspectRatio 9:16），打开项目即生效；已存在则不重复添加。
2. **桌面独立播放器**：PlayerSettings 默认窗口 540×960，竖向，窗口可调但画面始终按 9:16 渲染：当窗口比 9:16 更宽时左右留黑边（pillarbox），比 9:16 更窄/更高（真机长屏）时保持铺满且内容仍在安全区内。相机视口与 Canvas 缩放都要一致，UI 不得被拉伸或裁切。
3. 检查并确认现有布局在 9:16（1080×1920）、更长的 9:19.5 与更宽的 3:4 下都正常（黑边或延展背景均可，但不得出现错位/重叠）；用截图脚本分别出这三种比例的游戏与菜单截图到 `build/screens/aspect/`。
4. 测试保持全绿（`bash scripts/run_tests.sh`），新增一个比例适配的测试（给定屏幕宽高 → 计算出的视口矩形）。

## 约束
不改 Core；不碰 art/ 原件；不提交 git；报告写 `tasks/T8_report.md`。

## 验收（Claude 亲自执行）
测试全绿；我看 aspect 截图；编辑器已按上述命令重新打开且 Game 视图为 9:16。
