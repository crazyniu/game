# T11b4 — UI 素材包 C：横幅与彩色按钮（只改 art/ui_kit/）
依据 `docs/UI_SPEC.md` v1.1 修订 A 节。风格同 art/ui_kit 现有素材（看 contact_a_light.png）。image_gen 原创，透明背景，无文字。允许用 Pillow 做尺寸整理与按下态（下沉 6px、变暗 8%）。
## 交付（art/ui_kit/）
- `ribbon_title.png` 1024×200：标题横幅（绸带样式，主色暖红 #E0573E 或森林绿二选一——用**暖红**，两端折角，上缘高光），九宫格（中段可横向拉伸，两端折角不变形），manifest 标边距。
- `btn_orange.png` / `btn_orange_pressed.png` 512×132 胶囊，橙色 #FFA53A→#E87B1E，深棕描边，九宫格同 btn_primary。
- `btn_red.png` / `btn_red_pressed.png` 512×112，红 #E8604A→#C23F2C，九宫格。
- `btn_round_red.png` / `btn_round_red_pressed.png` 224×224 红色圆钮（放 icon_close）。
- `pill_wood.png` 512×120 木牌胶囊（HUD 关卡名、步数、星数），九宫格。
- `badge_red.png` 96×96 红色圆形角标（提示次数）。
- `glow_rays.png` 1024×1024 金色放射光芒（通关星背后旋转）。
- `manifest_c.json`、`contact_c_light.png`（按钮三宽度拉伸演示）。
报告 `tasks/T11b4_report.md`。不提交 git，不改已有素材。
