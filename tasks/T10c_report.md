# T10c — 正式美术接入与表现打磨

2026-10-31，工作目录 `/Users/apple/Documents/projects/game`。🟡 已交付待 Claude 视觉验收。

## 结果与证据

- 最终 `bash scripts/run_tests.sh` 退出 0：**EditMode 144/144、PlayMode 16/16，零失败**。
- 最终 `bash scripts/screenshots.sh` 退出 0：**15 张 1080×1920 PNG**，脚本逐张校验格式与尺寸，包含 1/20/40 关、睡眠、归巢、完成巢、彩纸和奖杯结算页。
- 已打开检查主要棋盘、选中、睡眠、归巢、巢口、彩纸、结算截图；第 40 关右列没有此前不同鸟种互相挤压的问题，箱体未越屏，篮子与底部按钮区分离。
- 日志/XML：`build/scene-build.log`、`build/editmode-tests.log`、`build/playmode-tests.log`、`build/test-results.xml`、`build/playmode-results.xml`、`build/screenshots.log`、`build/t10c-art-import.log`。
- 未修改 Core、关卡数据、`art/` 原件，未提交 git。任务开始时记录的 **1234 个输入文件 SHA-256 全部保持一致**，记录在 `build/t10c-input-hashes.json`；28 张导入 PNG 均逐字节匹配原素材。测试和截图使用原有临时存档隔离，Unity 批处理进程已退出。

## 美术接入

新增目录 `PerchSort/Assets/Art/Resources/`，沿用 `ArtImporter` 的 Single Sprite、FullRect、Bilinear、无 mipmap、无压缩设置。

| Resources 键 | 原素材 / 用途 |
|---|---|
| `env/crate`、`env/basket`、`env/nest`、`env/nest_done` | 同名正式环境图；完成后切换三蛋巢并隐藏鸟种图标 |
| `birds/bird_01_sleep`～`birds/bird_10_sleep` | 原始 `bird_01s.png`～`bird_10s.png`，仅复制后重命名以履行 T10b 契约 |
| `fx/zzz`、`fx/sparkle`、`fx/feather`、`fx/ring` | Z 气泡、选择/归巢闪光、飘落羽毛、巢口光环 |
| `ui/check` | 原 `raw/fx/check.png`，沿用契约键 |
| `fx/confetti_01`～`fx/confetti_03` | 三种正式彩纸 |
| `ui/trophy`、`ui/star` | 奖杯、结算两侧金星 |
| `fx/closed_eye` | 原闭眼弧线，资源可加载；实际睡眠使用整图 |
| `fx/star`、`fx/flash`、`fx/sleep_eyes` | 保留契约别名，分别复制 sparkle、sparkle、closed_eye |

睡眠鸟沿用 awake 图 **100 PPU、manifest 脚底 pivot**；不再染暗、不再叠占位眼睛。非鸟新图按最长边归一化：crate/trophy 1024 PPU，nest/nest_done/basket 512，常规 FX/star 256，closed_eye 128；crate 使用 manifest `(0.5, 0.05)` pivot，其余居中。导入器版本提升至 2，并执行既有 ImportAll 重导入；归一化值写入 `TextureImporterSettings`，避免被 `SetTextureSettings` 回写覆盖。代码占位图生成已移除，缺素材由资源测试明确失败。

无素材缺口。

## 布局数值与重叠修复

以下均为 Board 本地世界单位，9:16 对应 10.8×19.2（截图约 100px/单位）：

- **右列根因**：旧代码 flipX 后仍使用朝右的 pivot 补偿，鸟种 pivot 不同产生横向位移；现在 Art 与选中轮廓的补偿同镜像方向一起反转，槽位中心保持不变。
- 栖木名义宽度 `Capacity × 0.8`：6 格 4.8、5 格 4.0、4 格 3.2；旧 6 格为 4.32。左右外沿锚定 ±5.13，长枝之间名义空隙 **0.66**，每根长枝占画面宽度约 **44.4%**。
- 各容量、左右两侧的相邻站位间距统一 **0.752**。鸟不透明轮廓目标宽 **0.67**（旧 0.59）；睡眠横向压伸 1.03 后为 **0.6901**，仍小于站位间距；纵向睡眠比例 0.87，脚底下移 0.08。
- 每列围绕 **Y=-0.4** 独立居中；行距 `min(2.4, 8.4 / (最大行数-1))`，五行时 2.1。奇数栖木关采用左右错行，避免少一行的右列偏上。
- 测试使用棋盘安全范围 **X [-5.25, 5.25]、Y [-5.35, 5.1]**，检查栖木与鸟的实际不透明像素包围盒。
- 篮子中心 **Y=-5.9**；单篮 X=0、双篮 X=±1.0，素材整体宽 1.5、高 1.125，鸟站在 manifest 篮口锚点附近；提示区中心 Y=-7.1。
- 箱体按原图不透明宽 996/1024 补偿横向尺寸，实际左右边缘覆盖整枝；整体高度 1.7、局部 pivot Y=-0.4，不透明纵向区间约 **[-0.421, 1.151]**。既覆盖枝条下沿与鸟身，又保持最外沿 ±5.13；短枝也沿容量匹配宽度。
- 顶部巢队列中心 Y=6.5，前两巢保留 1.15 激活缩放；图标按 Sprite 中心校准至巢口，避免脚底 pivot 把图标顶到巢外。

| 关卡 | 左/右栖木数 | 行距 | 左列 Y | 右列 Y | 篮子数 |
|---|---:|---:|---|---|---:|
| 1 | 3/2 | 2.4 | 2.0, -0.4, -2.8 | 0.8, -1.6 | 0 |
| 10 | 3/2 | 2.4 | 2.0, -0.4, -2.8 | 0.8, -1.6 | 1 |
| 20 | 4/3 | 2.4 | 3.2, 0.8, -1.6, -4.0 | 2.0, -0.4, -2.8 | 1 |
| 30 | 4/4 | 2.4 | 3.2, 0.8, -1.6, -4.0 | 同左 | 2 |
| 40 | 5/5 | 2.1 | 3.8, 1.7, -0.4, -2.5, -4.6 | 同左 | 2 |

## 动画

- 选择抬升仍为 0.25，加入轻微 Back easing 弹性；取消平滑回落。
- 落地 0.16 秒内先压缩、再轻微反向回弹，最后恢复 RestScale。
- 正式 Z 气泡缓慢左右漂移、上浮、淡入淡出；睡眠整图保留原色。
- 归巢切换 nest_done，光环向外扩散，sparkle 淡出，对勾从零缩放弹出；羽毛使用正式图并保留旋转与下落。
- 已收集巢上滑时逐渐淡出，后续目标平滑补位和放大。
- 彩纸为 27 片，随机选三种图、初速度、大小和旋转速度；受重力下落，尾段淡出，并有轻度暗场。结算卡合成现有鸟角色、正式奖杯和金星。
- Reduce motion 保留：取消选择弹性、落地挤压/倾斜/飞行弧线、彩纸暗场、羽毛、Z 漂移；巢口特效保持固定大小，对勾直接显示，目标切换短时完成。既有 Reduce motion 连锁回归通过。

## 新增测试与截图

仅增加任务要求的两项验证：

1. `ArtResourceTests.RequiredProductionSpritesLoad`：检查 28 个关键资源及契约别名，资源缺失即失败。
2. `GameplaySmokeTests.RepresentativeLayoutsHaveNoOverlapsAndStayInSafeArea`：真实加载 1/10/20/30/40 关，从原 PNG 扫描 alpha>8 的轮廓，经过实际 SpriteRenderer 的 pivot、flipX、Transform 计算包围盒；断言同侧所有鸟互不相交、所有枝鸟组合位于安全区、箱体完整包住枝鸟且未越边界。它不使用槽位公式代替画面几何。

截图均位于 `build/screens/`：

| 文件 | 内容 |
|---|---|
| `01-level-01.png` | 第 1 关五枝居中错行布局 |
| `02-level-20.png` | 第 20 关正式箱体、短枝、篮子 |
| `03-selected.png` | 选择抬升、白描边与正式 sparkle |
| `04-win.png` | 第 20 关完成，角色/奖杯/金星结算卡 |
| `05-level-40.png` | 第 40 关十枝、两篮、两箱及右侧混合鸟种 |
| `06-menu.png`～`11-settings-reduced-motion.png` | 主菜单、选关、设置、暂停、死局、降低动态设置 |
| `12-sleeping.png` | 闭眼睡眠整图与 Z 气泡 |
| `13-homing.png` | 逐只归巢弧线中间帧 |
| `14-home-entry.png` | 完成巢、对勾、光环、sparkle、羽毛 |
| `15-celebration.png` | 实际第 20 关最后一次归巢后的暗场三色彩纸 |

12/13/14 沿用 T10b 的可重复事件夹具，HUD LEVEL 01 不代表发布第 1 关布局。15 在真实第 20 关最终事件链中捕获。截图脚本现在清理并校验全部 15 张图，避免旧图混入。

## 自行决定与修改范围

- 使用契约名复制睡眠图，不改源文件名；保留旧 fx 键别名，不引入自动兜底图。
- 五枝/七枝采用左右列分别居中；十枝维持五行对齐。鸟宽按不透明轮廓约束，避免长尾和透明画布干扰排布。
- 箱体使用统一高度和按容量调整的宽度；无需新画不同容量的箱子。
- 结算角色与奖杯由已有素材合成；没有新增原画或改变结算流程。
- 新增两张事件截图用于直接检查巢口和庆祝，不增加发布玩法或新测试框架。

修改：`Assets/Editor/ArtImporter.cs`、`Screenshots.cs`、`PerchCaptureSequence.cs`；`Assets/Scripts/View/BoardView.cs`、`BirdView.cs`、`PerchView.cs`、`TargetQueueView.cs`、`ViewArt.cs`、`CelebrationBurst.cs`；`Assets/Scripts/UI/GameHud.cs`；`Assets/Tests/EditMode/ArtResourceTests.cs`、`Assets/Tests/PlayMode/GameplaySmokeTests.cs`；`Assets/Art/Resources/` 与相应 `.meta`；重建 `Assets/Scenes/Game.unity`；`scripts/screenshots.sh`；本报告与 build 验证产物。

实现与本地验证完成；最终美术比例/风格验收由 Claude 逐张检查。本次未做移动设备性能或真机验收。
