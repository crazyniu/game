# T15 — 正面鸟接入游戏 + T11d 两处收尾

负责：Codex（gpt-6-astra / high）。验收：Claude。

## 背景
- 正面鸟素材 `art/raw/birds_front/`：bird_NN.png / _blink / _sleep / _fly，10 种，1024×1024 透明底，**脚底线 y=870**；manifest_front.json。bird_05 正在另行重画（T10e），文件名不变，完成后只需重新复制该 4 张。
- 前 5 关流程已按 UI_SPEC v1.3 完成（tasks/T11d_report.md）。
- 规范：docs/DESIGN.md、docs/UI_SPEC.md（v1.3 最高优先）。

## 执行纪律
照本任务书数值做，不得自行发挥；有歧义用 `orca orchestration ask` 问协调者。不改 Core 规则、关卡数据、art/ 原件；不提交 git；跑 Unity 前确认无其它 Unity 进程。

## A. 素材导入
1. 复制 40 张到 `PerchSort/Assets/Art/birds_front/`（写一个可重复运行的复制脚本 `scripts/sync_front_birds.sh`，T10e 完成后重跑即可）。
2. 导入设置：Sprite (Single)，pivot = (0.5, 154/1024)（即脚底线 y=870），Max Size 512，关闭 mipmap，压缩 High Quality。
3. 旧侧面鸟资源保留在工程里但不再被引用。

## B. 替换位置（全部改为正面鸟）
| 位置 | 用法 |
|---|---|
| 棋盘栖木 | stand；**左右两列都不镜像**（正面对称） |
| 待机眨眼 | 每只鸟独立随机 3–6s 切 blink 0.12s 再回 stand |
| 睡眠栖木 | sleep 图 + 现有 Z 特效 |
| 飞行（移动/归巢/揭箱浮空） | 起飞瞬间切 fly，落地切回 stand 并做现有挤压回弹 |
| 顶部鸟巢图标 | stand，坐在巢里（脚底线对齐巢内沿） |
| 通关页角色 | 本关最后归巢的那一种的 stand，放在原小鸟位置，画布高 340（可见鸟身约 260） |
| 主菜单树上三只鸟 | 依次 bird_01、bird_03、bird_02 的 stand，并使用各自 blink 做眨眼（替换原菜单鸟图） |
| 引导 / 关卡选择 | 若有鸟图，同样替换 |

## C. 尺寸与排布
- 栖木上每只鸟槽宽 = 栖木长度 / 容量；鸟 sprite 等比缩放到“画布宽 = 槽宽 × 1.15”（相邻画布可重叠，可见身体不重叠）。
- 脚底线对齐栖木上表面。
- 行距：两行栖木之间，上一行鸟头顶到下一行栖木上沿 ≥ 40px（设计坐标）；如不足，整体缩小鸟（不缩栖木）直到满足，并在报告写明最终鸟显示高度。
- 选中抬升 30px + 白描边保持不变（描边需适配新轮廓）。

## D. T11d 两处收尾
1. 通关：胜利面板弹出时，棋盘（栖木、鸟）和顶部鸟巢条 0.3s 淡出到 alpha 0，下一关入场时恢复（当前通关页横幅上方还能看到空鸟巢树枝）。
2. 引导：遮罩、镂空、手指、气泡必须在关卡入场动画（栖木滑入+鸟落位）**完成后**才出现，0.2s 淡入（当前入场第 2 帧就出现了空镂空框）。

## 交付
- `bash scripts/run_tests.sh` 全绿（EditMode 全过；PlayMode 0 失败）。
- 重新生成全部 `build/screens/flow/flow_*.png` 与 `anim_*_01..06.png`；新增 `flow_34_sleep_front.png`（有一根睡眠栖木的画面）和 `anim_blink_01..06.png`。
- 报告 `tasks/T15_report.md`：最终鸟显示高度/槽宽、改动文件、对应截图。完成后 worker_done。
