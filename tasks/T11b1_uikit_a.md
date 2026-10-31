# T11b1 — UI 素材包 A：面板与控件（只改 art/ui_kit/，不碰 PerchSort/）
权威：`docs/UI_SPEC.md` §1（设计语言、色板）§2（组件库）§5（素材清单）。风格必须与 `art/raw/birds`、`art/raw/env/*`、`art/raw/ui/trophy.png` 一致：暖棕描边 #5A3A22 2–3px、左上高光、右下柔影、木/叶/奶油材质。全部原创，内置 image_gen，真实透明背景，无文字。
## 交付到 art/ui_kit/（每个文件 PNG RGBA）
panel_wood（九宫格底图 512×512，圆角 48，木纹外框+内嵌奶油纸；注明九宫格边距）、paper_inner、btn_primary / btn_primary_pressed（树叶绿，胶囊 512×132）、btn_secondary / btn_secondary_pressed（奶油，512×112）、btn_round / btn_round_pressed（木底圆钮 224×224）、toggle_track_on/off、toggle_knob（小蛋或小鸟头）、slider_track、slider_fill、slider_knob（叶片）、hud_bar（木牌 1024×160 九宫格）、tip_card（纸卷轴九宫格 768×512）、vine_divider（横向藤条 1024×48，可平铺）。
再出 `art/ui_kit/manifest_a.json`（尺寸、九宫格边距 L/R/T/B、用途）与 `art/ui_kit/contact_a_light.png`/`_dark.png`（合成预览）；按钮必须同时展示 3 种宽度的拉伸效果预览，证明九宫格不变形。
## 验收（Claude）
文件齐全、RGBA、边缘无白边/棋盘格；预览中材质统一、拉伸不糊。报告 `tasks/T11b1_report.md`。不提交 git，不改已有 art 原件。
