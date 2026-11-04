# T15b — 棋盘鸟放大

状态：已交付待验收；实现、完整测试及截图完成，待 Claude 视觉验收。

工程 `/Users/apple/Documents/projects/game/PerchSort`；Unity `6000.6.7f1`；1080×1920 设计坐标。未修改 Core、关卡、art/ 原件；未提交 git。

## 尺寸与行距

- 栖木槽宽固定 **80px**，画布宽按 **80 × 1.7 = 136px**，画布为 **136×136px**；最终倍数 **1.7**，没有额外缩小。
- 篮子鸟也使用 **136×136px**，不再按篮子自身 150px 宽度放大。普通移动在栖木与篮子之间保持同一基础尺寸，扑翼及落地挤压沿用新 RestScale。
- 脚底 pivot 仍为 `(0.5,154/1024)`，站立脚底仍落在栖木表面：相对根节点 `-10 + 23/1024 × 栖木宽`，容量 6 / 栖木宽 480px 时为 **+0.78125px**。
- 每只鸟使用 SortingGroup；槽位从外墙端 0 到外露端递增，静止组序为 `10 + slot`，飞行组序为 `30 + slot`。鸟身、白描边、星点在同一组内排序，左右列均按逻辑槽位排列。
- 选中抬升仍为 30px；白描边、眨眼、睡眠、揭箱及挤压回弹使用同一放大后的鸟实例。鸟巢图标、通关鸟、菜单鸟的配置与实现未改。
- 协调者确认 `msg_b80827dec799`：保留归巢飞向巢时缩至 0.35 倍并消失的既有动画；起飞基础画布为 136px。

当前 40 关有 3–5 行，均已满足 ≥40px 净空，因此无需增大行距或重新布局。沿用 UI_SPEC 的 `min(260,1240/行数)` 行距及原棋盘居中位置；最高密度 5 行时，完整画布的上下范围仍在 y∈[-620,+620] 内。

| 关卡 | 行数 | 行距 | 完整画布脚底以上高度 | 保守行间净空 |
|---|---:|---:|---:|---:|
| 1–17 | 3 | 260px | 115.55px | ≥144.45px |
| 18–32 | 4 | 260px | 115.55px | ≥144.45px |
| 33–40 | 5 | 248px | 115.55px | ≥132.45px |

上表为同容量栖木的保守估算（完整画布而非可见头顶）。混合容量 4/5/6 的栖木表面高度差至多 3.59375px；计入该差值，5 行保守净空仍 **≥128.86px**，选中抬升 30px 后仍 **≥98.86px**。

可见高度按已接入 PNG 的 alpha>8 包围盒测量，再映射到136px画布；包括尾羽，不把透明留白计入：

| 鸟 | stand 可见总高 | 头顶至脚底线 |
|---|---:|---:|
| 01 | 102.80px | 102.66px |
| 02 | 100.94px | 100.80px |
| 03 | 100.80px | 100.67px |
| 04 | 97.75px | 93.50px |
| 05 | 102.80px | 102.66px |
| 06 | 109.17px | 109.04px |
| 07 | 100.94px | 100.80px |
| 08 | 101.07px | 100.94px |
| 09 | 100.80px | 100.67px |
| 10 | 101.07px | 100.94px |

## 验证与交付

首轮运行 `bash scripts/run_tests.sh`：EditMode 163 passed；PlayMode 13 passed / 1 failed，失败仅为旧首五关布局用例要求同栖木可见包围盒完全不相交。02 号鸟在136px画布的包围盒宽82.34px，因此同种相邻鸟会有约2.34px相交。

通过 orchestration ask 获协调者明确确认后，仅更新既有首五关布局用例：同栖木包围盒重叠 ≤6px、外露端渲染顺序递增；保留跨栖木不相交和原安全边界检查。未新增测试用例、未扩大测试集或改变跳过项。

最终重新运行 `bash scripts/run_tests.sh`，退出码 **0**：EditMode **163/163 passed，0 failed**；PlayMode **14 passed，0 failed，8 Explicit skipped / 22 total**。证据：`build/test-results.xml`、`build/playmode-results.xml`；跳过项沿用既有基线。

`bash scripts/screenshots.sh` 退出码 **0**，全部同名重生：**35 张 flow PNG + 54 张 anim PNG = 89 张**，全部1080×1920；9组动画各6帧，附对应时间采样记录。截图仍使用真实 Unity Camera / RenderTexture 与隔离临时存档；运行每次 Unity 前均确认无其它 Unity Editor 进程。

重点图均位于 `build/screens/flow/`：

- `flow_05_level1_hud.png`：放大后的站姿、脚底与栖木间距。
- `flow_03_level1_tutorial_target.png`：30px选中抬升与白描边。
- `anim_select_and_fly_01..06.png`、`anim_homing_01..06.png`：移动与归巢。
- `anim_blink_01..06.png`：眨眼。
- `flow_34_sleep_front.png`：睡眠与Z效果。
- 其余菜单、胜利及流程截图全部重新生成。

已目视核对HUD、选中、飞行与睡眠关键帧；剩余为 Claude 视觉验收，不宣称移动设备验收。

## 修改文件

- `PerchSort/Assets/Scripts/View/BoardView.cs`：共享80px槽宽与136px鸟画布；篮子同尺寸；按目的槽位更新飞行排序。
- `PerchSort/Assets/Scripts/View/BirdView.cs`：按槽位排序的 SortingGroup。
- `PerchSort/Assets/Tests/PlayMode/GameplaySmokeTests.cs`：按协调者确认更新已有布局用例的同栖木重叠规则。
- `PerchSort/Assets/Scenes/Game.unity`：既有 SceneBuilder / 截图脚本自动重建。
- 本报告与 `build/screens/flow/` 截图。
