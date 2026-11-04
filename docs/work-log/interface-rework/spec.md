# T11d — T11c 视觉返工（按 UI_SPEC v1.3）

负责：Codex（gpt-6-astra / high）。验收：Claude。

## 背景
T11c 已交付前 5 关完整流程（报告 tasks/T11c_report.md，截图 build/screens/flow/）。Claude 已复跑测试（EditMode 163/163，PlayMode 14 过 / 0 失败 / 8 Explicit）并逐张走查，主菜单、暂停、设置、HUD 图标化、select_and_fly、homing 动画已通过。剩余 10 处视觉问题写在 **docs/UI_SPEC.md 末尾“v1.3 修订”**，本任务只做这 10 条。

## 执行纪律
- 逐条照 v1.3 做，坐标、尺寸、颜色按规范数值，不得自行发挥或增删功能。
- 有歧义先 `orca orchestration ask` 问 Claude，不要猜。
- 不改 Core 逻辑、关卡数据、art/ 原件；不提交 git；鸟图保持现有旧版（正面鸟另行接入）。
- 跑 Unity batchmode 前确认没有其它 Unity 进程占用项目。

## 必做（详见 UI_SPEC v1.3 第 1–10 条）
1. 遮罩纯黑 alpha 0.5 全屏，盖住 HUD。
2. 弹窗打开 scale 0.85→1.05→1.0（0.25s OutBack），关闭 0.15s。
3. 通关页：面板 860×1120 + 无字 ribbon_title + 星坐横幅 + 规范坐标。
4. 死局：红圆钮内 icon_question 图示；撤销（绿）/重开（橙）同为 Ø200。
5. 关卡选择：删章节标题与藤蔓、连续 4 列；已解锁数字深棕 #5A3A22 字号 64；当前关 = 最小已解锁未通关关；底部 120px 渐隐。
6. 引导气泡固定 y=+420，不压任何栖木/鸟/巢。
7. Skip：木底圆钮 Ø130 + icon_next。
8. HUD 关卡牌 240×100。
9. 菜单→游戏转场无叠影：菜单淡出完毕后才入场。
10. 主菜单打开设置时菜单内容留在遮罩下。

## 交付
- `bash scripts/run_tests.sh` 全绿（EditMode 全过；PlayMode 0 失败）。
- 重新生成全部 `build/screens/flow/flow_*.png` 与 `anim_*_01..06.png`（同名覆盖），新增 `flow_33_menu_settings.png`（从主菜单打开设置）。
- 报告写 `tasks/T11d_report.md`：逐条列出 1–10 的改动文件与对应截图文件名。
- 完成后 worker_done。
