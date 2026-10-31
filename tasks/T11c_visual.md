# T11c — 按 UI_SPEC 重建全部界面视觉 + 接入音频

唯一规范：`docs/UI_SPEC.md`（逐页坐标/尺寸/文案/动效，**照做，不得自行发挥**；没写到或有歧义的先用 orchestration ask 问我）。素材已验收：`art/ui_kit/`（控件包 A：panel_wood、paper_inner、btn_primary/secondary(+pressed)、btn_round(+pressed)、toggle_*、slider_*、hud_bar、tip_card、vine_divider；包 B：level_cell_{done,current,open,locked}（固定 300×340，非九宫格，运行时按 150×170 显示）、star_on/off、egg_on/off、hand_pointer、tap_ripple、13 个图标；菜单包：logo_perch_sort、menu_tree、blink_bird_01/03/05、fly_bird_01/03/05；各 manifest_*.json 含九宫格边距与站位坐标）；音频 `audio_src/*.wav`（见 MANIFEST.md）。Unity 编辑器未开，你独占主工程。T12 会在稍后向主工程回写 Core/关卡文件，你不要碰 `Scripts/Core/**`、`Resources/Levels/**`。

## 必须做（逐项，缺一不可）
1. **导入**：把 art/ui_kit 素材按 manifest 导入 `PerchSort/Assets/Art/ui_kit/`（Sprite，九宫格边距按 manifest 设置，Mesh Type FullRect，无压缩失真；导入脚本/约定放进现有 Editor 构建脚本，保证 SceneBuilder 重建后素材引用正确）。音频导入 `Assets/Audio/Resources/`：BGM 为流式/压缩 Vorbis，短音效 ADPCM 或 PCM。
2. **替换 UiTheme**：所有面板/按钮/开关/滑条/HUD 改用新素材；删除旧的色块/纯色 Sprite 用法；按下 0.08s 缩 0.94、抬起 1.04→1.0（0.18s）；禁用态降饱和 60%；文字描边与层级按 UI_SPEC §1。
3. **页面逐一按 UI_SPEC §3 重做**：3.1 主菜单（Logo、menu_tree+三只鸟眨眼/歪头/周期飞出（飞出用 fly_bird_*），Play 呼吸，进入动画；无大白卡片）；3.2 关卡选择（章节分隔藤蔓、滚动、星槽、当前关脉动；章节名按规范）；3.3 HUD（顶部 hud_bar：关卡名+蛋进度+暂停圆钮；底部三圆钮 Undo/Hint(角标)/Restart）；3.4 通关页（奖杯与鸟分开、三星槽依次弹出且音效递升、标题不与奖杯重叠、按钮 Next/Replay/Home、最终关只有 Home/Replay 与规定文案、两侧喷发彩纸铺满 ~2.2s）；3.5 设置/暂停（木面板、分组、行高 130、藤条分隔、危险操作二次确认）；3.6 死局小弹窗；3.7 引导（柔边镂空聚焦、手指+涟漪、tip_card 样式；**修复 T11 截图里聚焦区出现白色矩形的问题**；提示卡示意图换成有意义的素材（用 egg/鸟/星/箱等现有素材拼示意，不要一截栖木）；提示高亮 FROM/TO 的圈放大到覆盖整段/整根栖木并带脉动）。
4. **棋盘区域**：按 §3.3 限定在 y∈[-640,+700] 内整体垂直居中；睡眠 Z 气泡放大 1.6 倍。
5. **音频接入**：menu 与 game 两首 BGM 循环，切页面 0.5s 交叉淡入淡出；SFX 映射——选中/落地随机 chirp_01..05（同一连击内避免连续相同）、飞行 flutter、睡眠 sleep_z、归巢 nest_home、箱子揭示 crate_open、通关星 star_1/2/3 依次、通关 win_fanfare、提示 hint、按钮 ui_tap、返回 ui_back；保留现有 Select/Move/Collect/Win/Error/Undo/Click 的合成音作为缺文件回退。音乐/音效音量与开关沿用设置；失焦/应用暂停时静音，回来恢复。
6. **测试**：保持全绿并新增——资源完整性（所有素材/音频键可加载，缺失即失败）、各页面构建无空 Sprite 引用、字号下限 ≥32（版本号除外）的扫描测试、三种比例（1080×1920、1080×2340、1440×1920）下关键页面元素无重叠的布局断言、BGM 切换状态机、SFX 映射表完整。`bash scripts/run_tests.sh` 全绿。
7. **截图**：`scripts/screenshots.sh` 出 UI_SPEC §6 验收清单要求的全部页面（主菜单、关卡选择、游戏 HUD、通关三星各阶段、最终关通关、设置、暂停、二次确认、死局、引导各步、中文版、三种比例）。

## 约束
不改 Core 规则、关卡数据、art 原件；不提交 git；报告 `tasks/T11c_report.md`（含逐条对照 UI_SPEC §6 清单的自检结果、任何偏离都须先问我）。

## 验收（Claude）
我重跑测试，逐页对照 UI_SPEC 逐项勾选；任何一项不符就返工。
