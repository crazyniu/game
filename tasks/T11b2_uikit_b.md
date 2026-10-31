# T11b2 — UI 素材包 B：关卡格、星蛋、图标、手指（只改 art/ui_kit/，不碰 PerchSort/）
权威：`docs/UI_SPEC.md` §1、§2、§5。风格与 art/raw 现有素材一致（暖棕描边 #5A3A22、左上高光、右下柔影）。原创，image_gen，透明背景，无文字。
## 交付到 art/ui_kit/
level_cell_done / level_cell_current / level_cell_open / level_cell_locked（木牌 300×340，数字不画，done 带金边+三个空星槽位置，current 为绿牌，locked 灰木牌+小锁）、star_on / star_off（256）、egg_on / egg_off（128，HUD 进度蛋）、hand_pointer（卡通手套指向，256）、tap_ripple（涟漪环 256）、
图标（各 192×192，同一套圆润线性+填色风格，奶油/深棕两色可识别）：icon_pause、icon_undo、icon_restart、icon_hint（灯泡或羽毛）、icon_back、icon_home、icon_sound、icon_music、icon_vibrate、icon_lock、icon_globe、icon_close、icon_check。
再出 `art/ui_kit/manifest_b.json`、`art/ui_kit/contact_b_light.png`/`_dark.png`，并出一张 64px 缩放对照图验证图标可辨。
## 验收（Claude）
文件齐全、RGBA、无白边；风格统一；64px 可辨。报告 `tasks/T11b2_report.md`。不提交 git，不改已有 art 原件。
