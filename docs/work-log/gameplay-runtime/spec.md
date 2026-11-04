# T4 — 游戏场景：美术导入、棋盘表现、输入、动画、HUD、关卡流程

规则以 `docs/DESIGN.md` 为准。已完成并验收：`PerchSort/Assets/Scripts/Core`（BoardState/Rules/GameSession/Solver/Generator/LevelData，84 个测试）、`PerchSort/Assets/Resources/Levels/levels.json`（40 关）、`art/`（素材清单见 `art/ASSETS.md`、`art/manifest.json`，含各鸟脚部锚点）。**Core 行为不得改动**；表现层只读 Core 状态，动画与逻辑分离，动画期间锁输入。

## 做什么
1. **素材导入**：把 `art/raw` 的成品复制到 `PerchSort/Assets/Art/`（保留 `art/` 原件不动），用 Editor 脚本统一设置导入参数（Sprite 2D、合适的压缩与 mipmap、Pixels Per Unit 一致、鸟的 pivot 取 manifest 的脚底锚点）。
2. **场景生成**：写 Editor 脚本 `Assets/Editor/SceneBuilder.cs`，一键生成 `Assets/Scenes/Game.unity`（同时设为唯一的 Build Settings 场景）；不要手工编辑 .unity。场景含相机（竖屏 1080×1920 参考，正交，适配不同长宽比与刘海 Safe Area）、背景、顶部树冠、棋盘根节点、Canvas HUD、EventSystem。
3. **棋盘表现** `Assets/Scripts/View`：
   - 根据关卡栖木数 P 自动排布成 N 行（建议每行 2–3 根栖木，竖向居中，留出顶部树冠与底部按钮区），栖木按容量 4 均分槽位；鸟统一缩放到槽宽内（尾羽长的鸟按身体而不是整图包围盒适配，不得互相严重重叠），脚落在栖木上沿。
   - `BirdView`/`PerchView`/`BoardView`：由 `GameSession.State` 驱动；每次逻辑变化后 `BoardView` 对比并播放动画，结束后与 Core 状态强制校对一致。
4. **输入**（触屏与鼠标均可）：点栖木 → 其最右端鸟被选中并上跳；再点另一根栖木 → 若合法则移动，否则抖动并取消；再点自己取消选中。空栖木不可选。点击判定区要大（整根栖木及其上方区域），小屏易按。
5. **动画**（自写轻量协程补间，不引入第三方）：选中弹起、飞行抛物线落位（带轻微挤压回弹）、非法抖动、集齐时 4 只鸟飞向树冠（错开起飞）并栖木清空、通关庆祝。动画可快速连点而不乱序（输入锁或队列，二选一并保证状态一致）。撤销时直接平滑还原（可以简化为快速反向动画），被收集的鸟要还回栖木。
6. **HUD 与流程**（uGUI）：顶部关卡号；底部撤销、重开按钮（撤销在无历史时置灰）；通关面板（下一关/重玩）；死局面板（撤销/重开，不诱导购买）。读取 `Resources/Levels/levels.json`；`ProgressStore` 版本化 JSON 存档到 `Application.persistentDataPath`，仅在稳定状态写入，覆盖前保留上一份快照；重进游戏继续当前关；40 关通关后有结束提示并可重玩。
7. **自动化验收**：
   - 新增 PlayMode 或 EditMode 烟测：用 `Solver` 的解答序列驱动 `BoardView` 的输入入口，对关卡 1、10、20、30、40 跑到通关，断言通关面板/状态、动画结束后表现与 Core 一致、无异常日志。
   - Editor 工具 `Assets/Editor/Screenshots.cs`（batchmode 可调）截取：第 1 关开局、第 20 关开局、选中状态、通关面板，输出到 `build/screens/*.png`（1080×1920）。
   - `scripts/run_tests.sh` 保持可用并覆盖新增测试；新增 `scripts/screenshots.sh`。

## 约束
- 不改 Core 行为；不碰 `art/` 原件；不提交 git；不引入付费或需联网的依赖；无广告、无内购。
- 代码简洁，匹配现有风格；公共 API 小；不写与上述无关的功能（菜单/选关/设置/音效留给后续任务）。

## 验收（Claude 将亲自执行）
- `bash scripts/run_tests.sh` 全绿；`bash scripts/screenshots.sh` 产出截图，我会逐张看：鸟不重叠不出界、栖木整齐、HUD 清晰。
- `tasks/T4_report.md`：文件清单、场景/脚本结构、如何重建场景与截图、已知问题。
