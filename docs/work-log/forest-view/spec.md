# T10b — 表现层与玩法接入 v2

规则权威：`docs/DESIGN.md`；Core v2 已完成（见 `tasks/T9_report.md` 的 API 迁移表，务必消费 `result.Move.Events` 事件序列，不要依赖旧的 Collected* 字段）。

## 目录所有权
可改：`PerchSort/Assets/Scripts/View/**`、`Scripts/UI/GameController.cs`、`GameHud.cs`、`Editor/SceneBuilder.cs`、`Editor/Screenshots.cs`、`Tests/PlayMode/**`、`Tests/EditMode` 中与表现相关的测试。**不要改 Core**（有问题回报给我）。不碰 `art/`（另一个 worker T10a 正在产出新美术到 art/raw/env|fx|ui；你先用代码生成的占位 Sprite 或 `Resources` 里按名查找并带回退，最终由 T10c 接入真素材）。Unity 编辑器此刻没开；你独占该项目。

## 必须实现
1. **布局**：左右两列栖木，面向中央通道；每根栖木按自身 Capacity 画不同长度（树枝短/长）；`Side==Right` 镜像绘制，外露端朝通道。栖木数 5–10 在 9:16 内都能放下不重叠（按栖木数自适应缩放/分行）。篮子画在底部区域。顶部画目标鸟巢队列：显示全部目标，**激活的前两个高亮放大**，其余缩小排队，每个巢上叠对应鸟的小图标。
2. **输入**：点来源 → 外露端连续同种段（Rules.GetRun）整体抬升+描边+小星点；再点合法目标 → 执行；点非法目标/空白 → 取消或换选；睡眠与板条箱栖木不可选（轻抖反馈）；篮子可点。动画期间锁输入。
3. **移动动画**：run 内逐只错峰（间隔约 50–70ms）沿弧线飞到目标新位置，朝向转向、落地轻微压缩回弹；篮子同理。
4. **事件播放**：按 `Events` 顺序播放 Moved → Completed → TargetsAdvanced → AutoHomed → CrateRevealed，每类有独立动画：
   - Completed(Asleep)：整排压低+变暗+闭眼+间歇飘 Z；Completed(Homed)/AutoHomed：整排逐只弧线飞向对应激活鸟巢，入口闪光，对勾徽章，羽毛飘落；
   - TargetsAdvanced：旧巢上滑退出、队列左移补位、新激活巢放大进入；
   - CrateRevealed：箱体碎裂/掉落淡出，下方鸟排弹出落稳。
   - 自动连锁之间有短停顿，节奏可读。
5. **撤销/重开**：撤销瞬时或快速回放反向，状态必须与 Core 一致（可直接按状态重建视图）；睡眠/箱/目标进度正确还原。
6. **通关/死局**：通关彩纸+结算（沿用现有页面，接上新关卡数据）；死局弹撤销/重开。`GameController` 接入 40 关 v2 数据；`ProgressStore` 遇到 v1 存档：作废旧进度并从第 1 关开始（写迁移 v3 + 测试），不要崩溃。
7. **Reduce motion**：开启时动画缩短/去抖/去彩纸，仍按事件顺序。
8. 性能：无每帧 GC 分配热点（对象池或复用），动画用协程/插值，不引入新包。
9. **测试**：迁移并修复 PlayMode 6 个失败（旧 v1 容量假设）；新增：run 整体移动 UI 流程、睡眠、自动归巢连锁、箱揭示、撤销后视图与状态一致（通过读取视图节点数/位置与 Core 对比）、v1 存档迁移。`bash scripts/run_tests.sh` 必须全绿。
10. 用 `scripts/screenshots.sh` 出截图（新增：中期关卡含箱/篮/短枝、睡眠态、归巢瞬间若可抓、结算页）到 `build/screens/`。

报告 `tasks/T10b_report.md`：做了什么、占位资源清单（T10c 要替换的名字）、与我约定外的决定逐条列出。不提交 git。

## 验收（Claude）
我重跑 run_tests.sh 全绿；看截图；检查事件→动画覆盖；审阅决定清单。
