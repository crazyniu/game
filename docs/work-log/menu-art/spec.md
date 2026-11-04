# T11b3 — 主菜单素材：Logo、树冠、背景层（只改 art/ui_kit/，不碰 PerchSort/）
权威：`docs/UI_SPEC.md` §3.1。风格与 art/raw 一致，原创，image_gen，透明背景。
## 交付到 art/ui_kit/
1. `logo_perch_sort.png`（1536×640）：木牌/树枝拼出 “Perch Sort” 描边大字（英文，字体圆润可爱，不得借用任何现有游戏 Logo），可带 1–2 片小叶和一只小鸟剪影装饰；字内无其他文字。另出 `logo_subtitle_none` 不需要。
2. `menu_tree.png`（1536×1280）：一棵大树冠+粗树枝，枝上留 3 个空位给鸟（鸟不画，代码叠 `art/raw/birds/bird_NN.png` 并做眨眼/歪头/飞出）；在 manifest 中给出三个鸟站位坐标（脚底点）。
3. 为眨眼动画提供 3 只常驻角色（bird_01、bird_03、bird_05）的闭眼版叠片 `blink_bird_01/03/05.png`（与原图同画布、同对齐，仅替换眼睛区域；若做不到完美，改为对每只提供完整的闭眼整图 `bird_NNb.png`）。
4. 飞行姿态：为菜单飞过动画提供 3 只鸟的展翅版 `fly_bird_01/03/05.png`（与站姿同风格，朝右）。
5. `manifest_menu.json` + `contact_menu.png` 预览（把 logo、树冠、三只鸟摆成菜单构图，放在 `art/raw/env/background.png` 上）。
## 验收（Claude）
合成预览整体好看、风格统一；闭眼/展翅与原鸟明显是同一只。报告 `tasks/T11b3_report.md`。不提交 git，不改已有 art 原件。
