# T11d — UI_SPEC v1.3 视觉返工

状态：已交付待验收；实现、测试、全部截图与动画证据完成，待 Claude 视觉验收。工程 `/Users/apple/Documents/projects/game/PerchSort`，Unity `6000.6.7f1`，1080×1920。

只处理 T11d 的 10 项及本 Dispatch 的明确补充。未改 Core、关卡 JSON、`art/` 原件或鸟资源，未提交 git。`docs/UI_SPEC.md` 的新增补充由协调者维护，本 worker 未编辑规范。

## 协调者确认

- `msg_355e03ab0747`：允许 RectMask2D 上下软裁切 120，顶部留至少 120px 内边距；无需另制遮罩材质。
- `msg_429560fb3459`：通关横幅 922×180，中心 `(0,520)`；三星 150/180/150，x=-190/0/190，y=540/560/540。已同步为规范第 12 条。
- `msg_3025834abd9e`：有撤销历史时保留绿色 Undo；无历史时隐藏 Undo，橙色 Restart 居中 `(0,-130)`、Ø200；增加 `flow_24b_deadlock_nohistory.png`。已同步为规范第 11 条。
- `msg_624f436c4f4e`：Linear 空间的遮罩明确改为纯黑 alpha **0.72**；暂停/设置/死局下 HUD 保留原色，仅由遮罩压暗；通关时 HUD 0.2s 淡出，下一关随入场淡入。已同步为规范第 13 条，覆盖原第 1 条的 0.5。

## 原任务 1–10 逐条交付

下面 UI 文件均在 `PerchSort/Assets/Scripts/UI/`；截图均在 `build/screens/flow/`。

| 条目 | 实现及改动文件 | 对应截图 |
| --- | --- | --- |
| 1 遮罩/HUD | `PanelMotion.cs` 将遮罩与面板的 alpha 分开，纯黑 0→0.72 / 0.2s；`GameHudPages.cs`、`GameHud.cs` 维持遮罩在棋盘/HUD 上方并阻断底层输入。`ButtonFeedback.cs` 让模态下 HUD 保持原色，通关 HUD controls 单独在 0.2s 内淡出。 | `flow_17_pause.png`、`flow_18_settings.png`、`flow_24_deadlock.png`、`flow_09_level1_win.png`、`anim_win_stars_01..06.png` |
| 2 弹窗动画 | `PanelMotion.cs` 使用 0.25s OutBack，scale 0.85→1.05→1.0，alpha 仅前 0.1s；退出 0.15s，scale 1→0.9 与 alpha→0。`GameHud.cs`、`SettingsPanel.cs`、`ScreenTransition.cs` 在返回/继续/重开/下一关/主页动作前完成关闭，且不会在后续页面淡入时重新激活刚关闭的弹窗。 | `anim_popup_open_01..06.png`、新增 `anim_popup_close_01..06.png` |
| 3 通关页 | `GameHudPages.cs`、`GameHud.cs`：面板 860×1120 居中；无字横幅及星星用协调者确认尺寸/位置；鸟+奖杯基准中心 y=180；Next Ø220 绿 `(0,-220)`；Replay/Home Ø150 橙 `(-150,-420)/(150,-420)`。 | `flow_09_level1_win.png`、`flow_11_level2_win.png`、`flow_13_level3_win.png`、`flow_15_level4_win.png`、`flow_25_level5_win.png` |
| 4 死局 | `GameHudPages.cs`：面板 760×640；单个不可点红圆底 Ø160 + 问号，中心 `(0,130)`；Undo/Restart 同为 Ø200，绿/橙、`(-150,-130)/(150,-130)`。`GameHud.cs` 按确认处理无历史时隐藏 Undo 并居中 Restart。 | `flow_24_deadlock.png`（真实移动后有历史）、新增 `flow_24b_deadlock_nohistory.png` |
| 5 关卡选择 | `LevelGrid.cs` 删除章节标题/藤蔓，统一连续四列；已解锁数字 #5A3A22、64、无描边/投影，锁定字保留白色描边；当前关从存档求最小已解锁未通关关。顶部/底部保留内边距，当前关已在可见区域时不强制滚动。`GameHudPages.cs` 保持视口底边 -760，去掉旧遮色条；`Assets/Shaders/UIDesaturated.shader` 支持 RectMask2D softness，锁定格也真实渐隐。 | `flow_27_levels_stars_1_to_5.png`、`flow_32_chinese_levels.png`（第 6 关绿格，1–5 完整可见） |
| 6 引导气泡 | `TutorialOverlay.cs` 将气泡固定在 `(0,420)`，不再依据目标栖木动态摆放。 | `flow_10_level2_tutorial.png`、`flow_12_level3_tutorial.png` |
| 7 Skip | `TutorialOverlay.cs` 原实现已是 `btn_round` 木底 Ø130 + `icon_next`，保持并重新截图验证；未改成橙色。 | `flow_02_level1_tutorial_select.png`、`flow_10_level2_tutorial.png`、`flow_12_level3_tutorial.png` |
| 8 HUD 关卡牌 | `GameHudPages.cs` 改为 240×100，文字区域 220×90，仅显示关卡数字。 | `flow_05_level1_hud.png`、`flow_14_level4_hud.png`、`flow_16_level5_hud.png` |
| 9 菜单→游戏 | `ScreenTransition.cs`、`GameHud.cs`：菜单先淡出 0.25s 并停用，再执行显示游戏/棋盘入场，HUD 淡入；无菜单 Logo 与棋盘交叉显示。 | 新增 `anim_menu_to_game_01..06.png`、`anim_level_start_01..06.png`、`flow_29_saved_resume.png` |
| 10 菜单设置 | `GameHud.cs` 保持菜单页活动并禁用其输入，设置遮罩/面板在其上；关闭后恢复菜单。 | 新增 `flow_33_menu_settings.png` |

## 验证

已在确认无其它 Unity Editor 占用项目后运行：

```sh
bash scripts/run_tests.sh
bash scripts/screenshots.sh
```

- 最终运行代码的 EditMode：**163/163 passed，0 failed**，`build/test-results.xml`。
- 最终运行代码的 PlayMode：**14 passed，0 failed，8 Explicit skipped / 22 total**，`build/playmode-results.xml`。8 项沿用前序延期范围，未新增跳过、未当作通过。
- 只调整已有检查：`Assets/Tests/PlayMode/GameplaySmokeTests.cs` 等待关闭动画，设置入口按真实可交互按钮统计；`UiSpecTests.cs` 同样适配遮罩下保留菜单，并更新 HUD 分组路径。没有扩张测试套件。
- `Assets/Editor/FirstFiveCaptureSequence.cs` 的按钮操作先检查 `IsInteractable()`，再调用实际按钮回调；棋盘走真实选择/移动接口。死局从 `[0,1,0] / [1,0,1,0]` 执行右→左一次合法移动后进入，保留撤销历史；无历史版本另行初始化测试棋盘，不改关卡文件。
- 像素检查：`flow_16_level5_hud.png` 与 `flow_17_pause.png` 四处无 UI 的固定背景采样点，遮罩后 sRGB 各通道为原图约 **0.550–0.565 倍**，与协调者要求的视觉压暗程度一致。

## 截图交付

原 `flow_01..32` 与六组必需动画均同名重新生成；新增 `flow_33_menu_settings.png` 和协调者要求的 `flow_24b_deadlock_nohistory.png`。共 **34 张流程图 + 48 张动画帧**，全部 1080×1920。

六组必需动画：`menu_intro`、`popup_open`、`level_start`、`select_and_fly`、`homing`、`win_stars`，各 `01..06`。另外两组 `popup_close`、`menu_to_game` 各六帧，用于本轮新增关闭/顺序转场证据。

最终截图脚本退出码 0；82 张 PNG 尺寸检查全部通过，八组采样与计划时刻的最大偏差为 1.0ms。

每组实际采样时间保留在 `anim_<name>_times.txt`。帧间隔分别为 menu_intro 0.10s、popup_open 0.05s、level_start 0.13s、select_and_fly 0.25s、homing/win_stars 0.20s、popup_close 0.03s、menu_to_game 0.10s。弹窗开/关在 RenderTexture 准备完成后才触发，首帧在触发后约 0.01s，避免初始化消耗入场前几帧；六帧全部采完后才编码 PNG。

本轮证据是 Unity 实际 Camera/RenderTexture 和真实游戏状态，范围仍是首五关、1080×1920；未扩展为物理鼠标、移动设备、打包播放器或前序延期项目验收。运行脚本重建了 `Assets/Scenes/Game.unity` 及原有生成资产，未改生成器逻辑。

剩余：Claude 按本报告及原始 PNG 做最终视觉验收。
