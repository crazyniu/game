# T11c — UI_SPEC v1.2 / 首五关流程

状态：已交付待验收；实现、功能检查及最终截图自检完成，待 Claude 视觉验收。工程 `/Users/apple/Documents/projects/game/PerchSort`，Unity `6000.6.7f1`。未提交 git；未修改 Core、Resources/Levels、art 原件。

## 当前口径

以协调者最新下发的 **UI_SPEC v1.2 > v1.1 > v1** 为准，围绕首五关验收，catalog 保留 40 关。末关、后期箱子/自动归巢规则链、100/300 关性能及三比例验收按指令延期，8 项 PlayMode 用例保留为 Explicit，未算作通过。

本 Dispatch 已确认：保留 Screen Space Camera，Canvas sortingOrder=100；第 2/3 关点镂空继续；Skip 改为右上 y=740、Ø130 的 icon_next 木圆钮；第 1 关由实际选择/移动自动完成。协调者 `msg_a9add273b066` 确认了以下布局和数字例外：

- 菜单 Play Ø260、y=-330；Levels Ø150、y=-565；右上齿轮。
- 通关 Next Ø220、y=-300；Replay/Home Ø150、(±140,-480)；删除横幅与步数行，保留三星/奖杯。
- 暂停木面板 760×760；继续 Ø220、(0,130)；重开/主页/声音 Ø150、(-220,-170)/(0,-170)/(220,-170)，右上红关闭。
- 死局原 680 高木面板，Undo Ø220、(-150,-100)，Restart Ø150、(150,-100)，上方空栖木+问号，无文字。
- 按钮无文字的明确例外：关卡格数字、Hint 角标数字；HUD 关卡号和星数胶囊数字亦保留。

## 实现

| 范围 | 最终行为 |
| --- | --- |
| 资源与主题 | manifest 驱动 Sprite FullRect/无压缩/九宫格；SceneBuilder 重建绑定主题与音频。仅圆形图标按钮，图标占直径 60%，绿色主、橙色菜单列表及通关 Replay/Home、木色普通、红关闭；禁用降饱和 60%，按压 0.92/回弹 1.06。允许的文字仍用规定字体、亮/暗两种样式。 |
| 菜单 | 无副标题/长按钮/版本号；Logo、树冠三鸟、进度木胶囊，Play/Levels/齿轮图标；Logo 落入、鸟眨眼/呼吸/周期飞出、Play 呼吸。 |
| 关卡列表 | 四列等间距，当前绿格/脉动光圈，完成格改用 open 圆角底+下方星条，移除拱顶完成格；章节、锁定反馈、滚动、底部渐隐；进入时定位当前章节，首五关同时可见。无 Levels 标题，仅返回与星图标/总数。 |
| HUD / 棋盘 | 胶囊只显示关卡数字，左暂停；底部三个 Ø130 图标，无文字，Hint 数字保留。两列同行、棋盘按行数居中，巢 y=700 水平居中，木架、激活光晕、等待透明度；睡眠 Z 放大 1.6 倍。 |
| 引导 | 柔边圆角透明洞覆盖整根栖木/鸟，洞外阻止点击，Skip 除外；指尖落于目标并循环点击/涟漪。第 1 课仅手指，无文字气泡；第 2/3 课分别为 Complete flocks fly home / Wait for matching nests（中文同色鸟群归巢 / 等待对应鸟巢），满足 6 词/10 汉字上限，不遮挡目标。 |
| 通关 | 全屏 50% 遮罩、底部 HUD 隐藏，木面板弹入；弧形三星依次弹出/旋转光芒、独立鸟和浮动奖杯；无标题横幅、Moves/Best、按钮文字。Next/Replay/Home 图标；120 片指定六色彩纸，约 2.2 秒。 |
| 设置 | 560×460 小木面板，无标题；仅中央 Ø200 声音圆钮和右上红关闭。声音同时作用于 BGM/SFX，图标 on/off 对应实际状态。删除滑条、独立音乐开关、语言、振动、减少动画、重看、重置、版本号等入口。 |
| 默认行为 | 振动开启；未接平台减少动画 API，默认关闭；语言跟随系统，中文系统简体中文，其余英文。旧存档字段保留，单声音开关持久化；不再通过 UI 暴露旧设置。 |
| 暂停/死局 | 采用已确认的纯图标布局，模态遮罩挡住 HUD/棋盘输入；暂停可继续、重开、Home、声音，死局可撤销/重开。 |
| 动画 | 页面淡入上移 40 / 0.25 秒，返回 0.18 秒；弹窗 0.7→1 / 0.35 秒及遮罩 0.2 秒；栖木依次滑入、鸟弹落、选择抬升 30 和描边闪烁、飞行扑动/落地挤压、归巢缩小/巢弹跳/星光/对勾/目标巢替换。Tween 起点单独显示一帧，避免前一帧的 PNG 编码等长帧吞掉入场。 |
| 音频 | 2 BGM + 16 生产 SFX，0.5 秒交叉淡化、五种 chirp 不连续重复、全部指定事件映射；失焦/暂停静音并恢复。保留原七种合成音回退；BGM Streaming/Vorbis、短音 ADPCM。 |

新素材状态：T11b5 的 manifest_d.json、六个图标与绿色/橙色圆钮及按压态已全部接入，临时映射已删除。

## UI_SPEC §6 逐项自检

| 项 | 本轮结果 |
| --- | --- |
| 1. 材质一致、无白大卡片/无描边色块按钮 | 生产 UI 素材与圆钮；纯色 Graphic 仅用于遮罩、透明点击层和渐隐。最终运行截图已核对设置、暂停、通关、死局、列表与教学。 |
| 2. 字号/重叠/双语 | 全页面 en/zh 无空 Sprite 与字号下限扫描通过；按钮子树无文字扫描通过，关卡号/Hint 数字除外；已移除 v1.1 语言按钮与长文案。范围为 1080×1920，延期比例不声称通过。 |
| 3. 菜单 | 按 v1.2 圆钮坐标实现；6 帧开场证据；眨眼与周期飞出实现，0.5 秒开场帧本身不覆盖完整飞出周期。 |
| 4. 列表 | 首五关星级、章节/滚动/当前格；100/300 关性能延期。 |
| 5. HUD / 通关 | 依 v1.1 去蛋计数、依 v1.2 去按钮文案与通关文案；巢表达目标，三星/彩纸/奖杯保留。 |
| 6. 页面/弹窗/减少动画 | 六组各 6 帧记录指定动画；旧 reduce-motion 播放能力保留，v1.2 无用户开关，当前系统未接 API 时默认关。 |
| 7. 三比例 | 本轮仅 1080×1920；1080×2340、1440×1920 与相应布局断言按指令延期，未声明验收。 |

## 验证与证据

```sh
bash scripts/run_tests.sh
bash scripts/screenshots.sh
```

- EditMode：163/163 passed，0 failed，`build/test-results.xml`。
- PlayMode：14 passed / 22 total，0 failed，8 Explicit skipped，`build/playmode-results.xml`。
- 新检查：设置可见按钮只有 Close/Sound、无 Slider；全按钮无文字子节点，Number/Count 例外。原针对已删除设置控件的交互断言已移除；声音双通道静音、恢复和场景重载持久化继续测试。
- 首五关端到端：教学、合法选择/移动、Next、各关 3 星、Home/Levels、重进第二关、移动后重载场景与 Play 恢复棋盘/关卡状态。
- 8 项延期：求解 10/20/30/40、两项后期箱子/自动归巢事件链、100/300 关列表、三比例布局。Core 规则用例保持运行。
- 截图通过真实 Unity Camera / 1080×1920 RenderTexture 输出；使用隔离临时存档，按钮以 Button.onClick、棋盘以实际 BoardView 选择接口操作。此证据不是物理鼠标/移动设备触摸或打包播放器测试，也不等于设备扬声器听感验收。

最终截图目录 `build/screens/flow/`：32 张流程 PNG + 36 张动画 PNG，全部 1080×1920，截图脚本退出码 0。

| 编号 | 内容 |
| --- | --- |
| 01 | 初始菜单 |
| 02–05 | 第一课来源/目标/真实移动结束与 HUD |
| 06–09 | 通关三星各阶段、稳定通关 |
| 10–13 | 第 2/3 关教学及通关 |
| 14–16 | 第 4 关 HUD/通关、第 5 关 HUD |
| 17–20 | 暂停、极简设置、声音关、声音开 |
| 21–24 | 提示、撤销、重开、死局 |
| 25–27 | 第 5 关通关、Home、首五关星级 |
| 28–29 | 重进第二关、场景重载后的保存恢复 |
| 30–32 | 中文 HUD/菜单/列表（直接切换 locale 作为另一系统语言的表现检查，无语言 UI） |

第 5 关截图路径故意执行撤销和重开，因而第 1–4 关 3 星、第 5 关 2 星、总 14 星；独立测试另行证明无撤销解法的首五关全三星。死局截图使用单独测试棋盘，未修改关卡 JSON。

六组各 6 帧 `anim_<name>_01..06.png`：menu_intro 间隔 0.10 秒、popup_open 0.07、level_start 0.13、select_and_fly 0.25、homing 0.20、win_stars 0.20。同名 `_times.txt` 记录实际时间，本次六组实际时间均在记录精度 0.001 秒内与上述间隔一致；六帧采完后才编码 PNG，编码时间不混入帧间隔。`build/screens/review/` 为走查缩略图，验收以原始帧为准。

## 修改位置与剩余工作

- 导入/场景/截图：`Assets/Editor/UIAssetBuilder.cs`、`SceneBuilder.cs`、`Screenshots.cs`、`FirstFiveCaptureSequence.cs`、`PerchCaptureSequence.cs`、`AspectCaptureSequence.cs`；两条 scripts 验证脚本。
- UI：`UiTheme`、`GameHud`/`GameHudPages`、`SettingsPanel`、`LevelGrid`、`MenuAnimation`、`ResultAnimation`、`ButtonFeedback`、`PanelMotion`、`SoftFocusMask`、`TutorialOverlay`、`ScreenTransition`、`SafeAreaLayout`、`GameController`、`CaptureRoutine`、`CrateLabels`、`Strings`。
- 表现/音频：`BoardView`、`BirdView`、`PerchView`、`TargetQueueView`、`AudioManager`、`SfxImporter`；`UIConfetti`、`UIDesaturated`、`UIFocus` shaders。
- 检查：`UiKitTests`、`AudioManagerTests`、`UiSpecTests`、`GameplaySmokeTests`；规范同步 `docs/DESIGN.md`。
- 生成资产：导入 UI/音频副本、字体 SDF、Game 场景及新增资源 meta；未改 art 原件、Core、关卡。
- 剩余：Claude 视觉验收；延期项目不纳入此次完成范围。原鸟资产保留，未收到 T10d 新鸟接入许可。
