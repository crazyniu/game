# T5 — UI、主菜单、选关与设置

日期：2026-10-31。目录：`/Users/apple/Documents/projects/game`。状态：实现、整套测试与真实运行截图已交付，待协调者逐张视觉验收。没有 Git 提交。

## 完成内容

1. 游戏内去掉常驻游戏名；关卡和收集进度放在顶部独立木色圆角牌上。树冠缩到顶部牌下方，文字不再叠在叶子上。
2. 面板、按钮统一使用本地生成的 9-slice PNG，暖奶油/森林绿/木色、描边和轻阴影；有按下、焦点和禁用状态。撤销使用折返箭头，重开使用环形箭头。胜利卡有小鸟/树冠装饰，死局卡有重试图标；二者均可回主菜单。
3. 使用 Fredoka Medium 的 TextMeshPro SDF 字体资源。TTF、OFL、静态 SDF 图集及 TMP 必需资源均已保存到 `Assets/Fonts`，无需联网生成或运行。来源与许可见 `docs/LICENSES.md`。
4. 鸟先按身体宽度适配，再把整个可见轮廓限制到 0.88 世界单位，并在槽位内按可见轮廓中心对齐。槽间距约 0.981，因此紫燕尾羽不再插进相邻鸟身体；只调整表现层，不改鸟图、脚部高度和 Core 槽位。
5. 启动显示主菜单：游戏名、开始/继续、选关、设置；有进度时显示继续。英文玩家字符串集中在 `Strings.cs` 的 locale 字典，游戏名只改 `title` 即可。
6. 40 关选关页使用 4×10 网格：木色勾选 DONE、绿色 PLAY、灰色锁定 LOCKED。顺序解锁，已完成关卡可重玩。进度文件升级到 v2，保留每关完成情况。
7. 设置页保存音效、音乐、震动开关；重置进度先确认，取消保留数据，确认回到初始主菜单；设置不随进度清空。本任务没有接音频或震动设备。
8. 单一 `Game.unity` 中切换 Menu/Game 页面，保持现有 Build Settings。暂停包含继续、重开、设置、回菜单。返回键统一调用 `Back()`：子页回上一页，游戏打开暂停，主菜单显示退出确认；暂停仅在当前动画稳定后打开。弹窗与暂停屏蔽背后按钮/棋盘输入，页面进入时设置可用按钮焦点。

## 文件与实现位置

| 路径 | 内容 |
| --- | --- |
| `PerchSort/Assets/Scripts/UI/GameController.cs` | 加载与恢复、完成记录、选关、设置持久化、重置 |
| `PerchSort/Assets/Scripts/UI/GameHud.cs` | 页面栈、HUD、菜单、40 关网格、设置、暂停、结果与确认页 |
| `PerchSort/Assets/Scripts/UI/UiTheme.cs` | TMP 文本、圆角 9-slice、按钮状态、图标、阴影 |
| `PerchSort/Assets/Scripts/UI/Strings.cs` | 可扩展 locale 表，当前英文 |
| `PerchSort/Assets/Scripts/UI/ProgressStore.cs` | v1→v2 迁移、顺序解锁、通关记录、原子保存及备份 |
| `PerchSort/Assets/Scripts/UI/SettingsStore.cs` | v1 JSON 与后续音频读取接口 |
| `PerchSort/Assets/Scripts/UI/PerchSort.UI.asmdef` | 添加 TMP 和已有 Input System 引用 |
| `PerchSort/Assets/Scripts/View/BirdView.cs` | 身体适配、完整轮廓宽度限制、可见中心偏移 |
| `PerchSort/Assets/Scripts/View/BoardView.cs` | UI 所有权下的输入开关，不改 Core 规则 |
| `PerchSort/Assets/Editor/UIAssetBuilder.cs` | UI Sprite border 与静态 TMP 字体资源生成 |
| `PerchSort/Assets/Editor/SceneBuilder.cs`、`Assets/Scenes/Game.unity` | 最终主题引用、树冠位置、场景重建；页面由 GameHud 运行时构建 |
| `PerchSort/Assets/Art/UI/*.png` | 新增圆角底板及图标；macOS 工作区该目录已有小写 `ui`，沿用同一目录 |
| `PerchSort/Assets/Fonts/**` | Fredoka、OFL、SDF 和 TMP 必需资源（带其许可） |
| `art/ui_sources/generate_ui.py`、`README.md` | 可复现的 UI PNG 生成脚本与字体制作说明 |
| `PerchSort/Assets/Tests/EditMode/PersistenceTests.cs` | 3 项迁移、解锁、设置测试 |
| `PerchSort/Assets/Tests/PlayMode/GameplaySmokeTests.cs` | 原 7 项保留，新增 1 项菜单/通关/解锁/暂停/设置/重置确认烟测 |
| `PerchSort/Assets/Editor/{Screenshots,PerchCaptureSequence}.cs` | 10 状态实际 Play Mode 截图；进度和设置使用隔离临时文件 |
| `scripts/{run_tests,screenshots}.sh` | 全套测试与 10 张 PNG 1080×1920 验证 |
| `docs/LICENSES.md`、`tasks/T5_report.md` | 字体/资源许可与本报告 |

所有新 Unity 资源附 `.meta`。没有修改 Core、关卡数据、原始美术、ProjectSettings、Packages 或音频 worker 的资源/代码/测试；SceneBuilder 不再重导入旧美术或改写 Build Settings。

## 存档与后续音频接口

进度路径仍为 `Application.persistentDataPath/progress.json`。v2 字段：`version`, `levelId`, `moves[{from,to}]`, `completedLevels[int]`。

- v1 的当前关卡和移动保留，`1..levelId-1` 推导为已通关，写入 v2，原 v1 保留在 `.bak`；当前关卡保存了胜局移动时，在恢复 Core 会话后补记当前关通关。
- 旧格式没有记录历史通关集合，因此旧版完成 40 关后回到第 1 关的存档无法推断历史全通关；不会凭空填满 40 关。
- v2 重玩不会删除通关记录。每次动画落定后保存当前移动；迁移后保留撤销历史。
- 设置路径 `SettingsStore.DefaultPath` = `Application.persistentDataPath/settings.json`。`SettingsData` 为 `version=1` 和 `soundEnabled`, `musicEnabled`, `vibrationEnabled`（默认均 true）。
- `new SettingsStore(path).Load()` / `.Save(data)`；保存完成触发 `SettingsStore.Changed` (`Action<SettingsData>`)。`GameController.SettingsPathOverride` 为测试/截图隔离路径。后续音频接线可读取默认文件并订阅 Changed，本任务不实例化 AudioManager。

## 验证

最终命令（Unity 6000.6.7f1）：

```sh
bash scripts/run_tests.sh
bash scripts/screenshots.sh
```

最终执行结果：**EditMode 99/99 通过（Core/UI 87 + T6 音频 12），PlayMode 8/8 通过，0 失败**；随后 `bash scripts/screenshots.sh` 成功，10 张 PNG 的签名和 1080×1920 尺寸全部通过脚本检查。最初 T6 音频测试初始化断言曾阻断整套运行，已由音频 worker 修复；上述数字来自修复后的主工作区完整重跑。

本轮新增测试仅针对任务要求：v1 存档迁移及备份、顺序解锁与重玩保存、设置开关存取与变更事件；真实场景的菜单→选关→游戏→通关→菜单→解锁下一关，暂停输入阻隔、设置保存、返回路径，以及重置的取消与确认。原有指定关卡通关、撤销、重开、恢复和死局烟测保留。

截图由真实 Game 场景在 Play Mode 下渲染，通过相机 RenderTexture 读回，并非拼图；每张 1080×1920：

| 文件（`build/screens/`） | 状态 |
| --- | --- |
| `01-level-01.png` | 第 1 关 |
| `02-level-20.png` | 第 20 关 |
| `03-selected.png` | 第 20 关选择最右鸟 |
| `04-win.png` | 通过真实棋盘输入求解第 20 关后的胜利卡 |
| `05-level-40.png` | 10 种鸟、6 行布局与长尾间距 |
| `06-menu.png` | 主菜单 |
| `07-levels.png` | 真实完成第 1 关后的 DONE / PLAY / LOCKED 混合选关页 |
| `08-settings.png` | 音乐关闭、其他开关开启 |
| `09-pause.png` | 第 40 关暂停 |
| `10-deadlock.png` | 固定满栖木 Core 会话触发死局，展示无撤销历史的禁用按钮 |

已逐张查看首轮 10 张图，最终样式修正后复查第 40 关与死局卡；牌中文字没有压树冠、紫燕长尾与邻鸟分开、字体与按钮一致，保留最终视觉验收给协调者。

日志与结果：`build/scene-build.log`, `build/test-results.xml`, `build/editmode-tests.log`, `build/playmode-results.xml`, `build/playmode-tests.log`, `build/screenshots.log`。

## 验收边界

验证环境为 macOS Unity Editor 的实际 Play Mode、1080×1920 相机渲染与现有安全区缩放逻辑；没有构建或安装 Android/iOS，未声称真机返回手势、触摸、刘海或所有宽高比已验收。返回键通过 Input System 的 Escape 路径进入页面栈；Unity 的 Android Back/Escape 语义参考 https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/input/backbuttonleavesapp 。

死局截图使用同 T4 烟测方式的固定 Core 会话，只用于展示面板，不改正常关卡或玩家存档。测试和截图均隔离并清理进度/设置文件。字体与 UI 图形生成所需下载/工具仅用于制作，最终游戏与脚本运行无需联网下载资源。
