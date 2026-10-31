# T15 — 正面鸟接入及 T11d 收尾

状态：已交付待验收；实现、测试、截图完成，待 Claude 视觉验收。

工程 `/Users/apple/Documents/projects/game/PerchSort`；Unity `6000.6.7f1`；1080×1920 设计坐标。本轮没有修改 Core、关卡 JSON、art/ 原件或提交 git。

## 协调者确认

- `msg_2d93318a5113`：任务书 pivot 的 130 为笔误，使用 **154/1024 = 0.150390625**，与脚底 y=870 及 manifest 一致。
- 同一确认：通关角色使用 **340×340 画布**（可见鸟身约 260），脚底线落在原小鸟位置底边。相对 Win decoration 的脚底 y=-135，画布中心 y=-16.13；保留原有整体浮动。

## 接入

- `bash scripts/sync_front_birds.sh` 可重复复制 `art/raw/birds_front/bird_01..10{,_blink,_sleep,_fly}.png` 共 40 张到 `PerchSort/Assets/Art/birds_front/`，保留目标 `.meta` GUID；不写 art/。
- Sprite Single / Full Rect，pivot `(0.5,154/1024)`，Max Size 512，mipmap 关闭，CompressedHQ。
- 使用序列化 `BirdArt` 四态引用，不复制一套 Resources 贴图。重新生成场景核查：40 张正面贴图全部被引用，旧侧面鸟、旧 sleep 和旧菜单 blink/fly GUID 的场景引用为 0；旧文件保留。
- 棋盘两列均不镜像鸟，右列只保留逻辑槽位顺序及栖木朝向。每只清醒鸟独立随机 3–6s 眨眼，持续 0.12s；睡眠/飞行期间不眨眼。
- 移动与归巢采用 fly，单只抵达时恢复 stand；移动落地保留挤压回弹。揭箱浮空使用 fly，落稳恢复 stand 并回弹。sleep 图配原 Z 特效。
- 顶部巢使用 stand，脚底对齐巢内沿（原图 y≈320）。菜单按 01/03/02 使用各自 stand/blink/fly，不镜像。通关角色记录实际最后归巢种类，包含自动归巢；重载已通关状态用目标队列最后一种初始化。

## 尺寸

- 栖木长度 `capacity × 80px`，常规容量 6 → 480px，**槽宽 80px，鸟画布 92×92px**，比例为 1.15；移除旧槽位 0.94 系数。
- 脚底使用原栖木 standingYFromTop=105：`-10 + 23/1024 × 栖木宽`。480px 栖木表面为根节点 y+0.78125px，站立与睡眠脚底均对齐该线。
- 首五关均为 3 行，行距 260px。以完整画布脚底以上高度保守估计，行间净空至少 **181.84px**，无需缩鸟。普通 stand 可见总高度见下表，含尾羽；这与 92px 画布高度不同。
- 后续行数更密时仅限制鸟画布高度，使上方净空≥40px，栖木不缩放。篮子沿用其 150px 槽宽，对应画布172.5px（仍受行距上限约束）。
- 选中抬升 30px；白描边同步当前正面轮廓，沿用闪烁和星点。

| 鸟 | stand 可见高度（92px画布） |
|---|---:|
| 01 | 69.54px |
| 02 | 68.28px |
| 03 | 68.19px |
| 04 | 66.12px |
| 05 | 69.54px（T10e 新稿） |
| 06 | 73.85px |
| 07 | 68.28px |
| 08 | 68.37px |
| 09 | 68.19px |
| 10 | 68.37px |

## T11d 收尾

- 胜利面板出现时，Board 下的栖木、鸟、巢及支撑枝统一 0.3s 淡出至 alpha=0。下一关重新创建时恢复，Replay 同场景恢复原色；HUD 保留 v1.3 的独立 0.2s 淡出。
- `EntranceComplete` 在栖木滑入与所有鸟落位完成后置为 true，才允许引导显示；遮罩、镂空、手指、气泡及 Skip 在同一 CanvasGroup 下 0.2s 淡入。入场期间也不显示旧提示文字条。

## 修改文件

- `scripts/sync_front_birds.sh`、`scripts/screenshots.sh`
- `PerchSort/Assets/Art/birds_front/`：40 张素材及 Unity 导入 meta；目录 meta。
- `PerchSort/Assets/Editor/ArtImporter.cs`、`UIAssetBuilder.cs`、`SceneBuilder.cs`、`FirstFiveCaptureSequence.cs`
- `PerchSort/Assets/Scripts/View/BirdView.cs`、`BoardView.cs`、`PerchView.cs`、`TargetQueueView.cs`
- `PerchSort/Assets/Scripts/UI/UiTheme.cs`、`MenuAnimation.cs`、`GameHud.cs`、`GameHudPages.cs`、`TutorialOverlay.cs`
- `PerchSort/Assets/Tests/PlayMode/GameplaySmokeTests.cs`：已有布局检查改用正面原图，alpha bounds 归一化后映射到导入后的 sprite.rect，支持 Max Size 512；没有扩展测试套件。
- `PerchSort/Assets/Scenes/Game.unity`：脚本重建四态引用。
- 本报告及 `build/screens/flow/` 交付图。

## 验证与截图

已执行 `bash scripts/run_tests.sh`：EditMode **163/163 passed，0 failed**；PlayMode **14 passed，0 failed，8 Explicit skipped / 22 total**。延期项沿用现状，没有新增跳过。证据为 `build/test-results.xml`、`build/playmode-results.xml`。

`bash scripts/screenshots.sh` 已退出码 0：原流程图全部同名重生，新增 `flow_34_sleep_front.png` 与 `anim_blink_01..06.png`。共 **35 张流程图 + 54 张动画帧 = 89 张 PNG**，尺寸全部 1080×1920；九组动画均为 6 帧，保留 `anim_<name>_times.txt`。

关键证据：

| 项目 | 截图（均位于 build/screens/flow/） |
|---|---|
| 菜单正面 01/03/02 | `flow_01_menu.png`、`anim_menu_intro_01..06.png` |
| 栖木、脚底与选中 | `flow_02_level1_tutorial_select.png`、`flow_03_level1_tutorial_target.png` |
| 四态眨眼 | `anim_blink_01..06.png`，40ms 等间隔；左上第一只红鸟为被采样对象 |
| 入场结束才引导 | `anim_level_start_01..06.png` 与 `flow_02_level1_tutorial_select.png` |
| 飞行、归巢 | `anim_select_and_fly_01..06.png`、`anim_homing_01..06.png` |
| 通关角色与棋盘/鸟巢淡出 | `flow_09_level1_win.png`、`flow_11_level2_win.png`、`flow_13_level3_win.png`、`flow_15_level4_win.png`、`flow_25_level5_win.png`、`anim_win_stars_01..06.png` |
| 下一关恢复 | `flow_10_level2_tutorial.png`、`flow_12_level3_tutorial.png`、`flow_14_level4_hud.png`、`flow_16_level5_hud.png` |
| 睡眠正面与 Z | `flow_34_sleep_front.png` |

眨眼六帧已核对：01 睁眼、02/03/04 闭眼、05/06 恢复睁眼；改变只落在眼部区域。入场第 2 帧不再有空镂空、手指或气泡；通关稳定帧横幅上方已无空鸟巢树枝。

睡眠截图使用捕获脚本内临时棋盘，通过真实移动将非激活种类 2 凑齐六只产生一根睡眠栖木，未改关卡文件或玩家存档。所有截图使用 Unity Camera/RenderTexture、隔离临时存档；不等同移动设备验收。

T10e bird_05：已读取 `tasks/T10e_report.md`，重新运行同步脚本并导入四态新稿；最终 40/40 文件与当前 art/raw/birds_front 原件逐字节一致。站姿现为 774px 可见高度，对应92px画布时69.54px；首五关及新增睡眠捕获仅使用01/02/03，因此05替换不影响已生成截图。最终代码测试全绿后仅替换了这四张贴图，并额外执行 SceneBuilder.Build 验证最终素材导入，日志 `build/t15-final-art-import.log`。

剩余：Claude 视觉验收。
