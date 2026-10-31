# T13a — 音频素材（只产出文件，不碰 PerchSort/ 与代码）
目标：在 `audio_src/` 产出可上线的原创音频（用 Python/numpy 合成或 ffmpeg，不得使用第三方采样/受版权素材）；记录生成脚本到 `scripts/audio/` 以便复现；并更新 `docs/LICENSES.md` 的音频条目。
## 交付（WAV 44.1kHz 16-bit，单声道/立体声均可，响度统一 -16 LUFS 左右，无爆音、首尾淡入淡出）
1. BGM：`bgm_menu.wav`（约 60s 可无缝循环，温柔木琴/长笛/竖琴感，C 大调，慢速）、`bgm_game.wav`（约 90s 可无缝循环，轻快但不抢注意力，与 menu 同调性）。必须验证循环点无咔哒（头尾波形衔接）。
2. 鸟鸣类音效：`chirp_01..05.wav`（短，0.15–0.4s，音高各异，用于选中/落地随机）、`flutter.wav`（振翅，0.3s，飞行用）、`sleep_z.wav`（轻柔下行，0.5s）、`nest_home.wav`（归巢：上行三音+闪光铃，0.8s）、`crate_open.wav`（木箱碎裂，0.6s）、`star_1/2/3.wav`（通关星依次升调）、`win_fanfare.wav`（2s）、`hint.wav`、`ui_tap.wav`、`ui_back.wav`。
3. `audio_src/MANIFEST.md`：每个文件的时长、用途、峰值/LUFS 数据（脚本测出）。
## 验收（Claude）
用脚本复核时长/峰值/循环点衔接（头尾样本差、过零检查）；文件无削波。报告 `tasks/T13a_report.md`。不提交 git。
