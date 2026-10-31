# T13a 音频素材交付报告

状态：素材与脚本已交付，脚本验收通过；等待 Claude 主观听感与后续游戏内混音验收。

## 交付

- `audio_src/`：18 个原创 WAV，44.1kHz / PCM 16-bit，总计约 26MB。
- 两首立体声 BGM：`bgm_menu.wav` 60.000s，64 BPM / 16 小节；`bgm_game.wav` 90.000s，96 BPM / 36 小节。均为 C 大调原创旋律，木琴、长笛和竖琴感合成音色。
- `chirp_01..05.wav`：0.210 / 0.250 / 0.290 / 0.320 / 0.360s，不同音高和走势。
- `flutter.wav` 0.300s、`sleep_z.wav` 0.500s、`nest_home.wav` 0.800s、`crate_open.wav` 0.600s。
- `star_1/2/3.wav` 各 0.350s，C6 → E6 → G6；`win_fanfare.wav` 2.000s；`hint.wav` 0.450s、`ui_tap.wav` 0.100s、`ui_back.wav` 0.180s。
- `audio_src/MANIFEST.md`：逐文件用途、时长、声道、峰值、LUFS、4× 真峰值估计与短音效量测补零长度。
- `audio_src/analysis.json`：最终 PCM 的完整测量值。
- `scripts/audio/generate.py`、`analyze.py`、`requirements.txt`、`README.md`：原创合成、独立文件验收与可复现环境。
- `docs/LICENSES.md`：追加原创音频来源说明，无第三方采样、录音、soundfont 或借用旋律。

## 协调者确认的细节

协调者消息 `msg_d46422eedb02` 确认未指定音效的上述时长、两首 BGM 的 BPM/曲式、C 大调原创旋律，以及短于 400ms 音效只在测量时尾补零至 400ms 的口径。没有任务范围偏离；没有修改 `PerchSort/`、游戏代码、旧 `tools/audio/` 素材流程，也没有提交 Git。

## 实际验证

执行环境为 Python 3.14.6，NumPy 2.5.4、SciPy 1.18.1、pyloudnorm 0.2.0，安装在 `/tmp/perchsort-t13a-audio-venv`，没有修改全局 Python 环境。执行：

```sh
/tmp/perchsort-t13a-audio-venv/bin/python scripts/audio/generate.py
/tmp/perchsort-t13a-audio-venv/bin/python scripts/audio/analyze.py
```

结果：`PASS: 18 WAV files; PCM format, duration, LUFS, peak, fades, clipping, and BGM seams.`

- 18 个文件全部符合 WAV/44.1kHz/16-bit；指定时长全部符合；无削波样本，端点为数字零，DC 偏移符合验收阈值。
- 每个最终 PCM 的响度均为 -16.00 LUFS（两位小数）；最高采样峰值是木箱音效的 -1.44 dBFS，最高 4× 真峰值估计为 -1.43 dBTP。
- `bgm_menu` 左/右头尾样本差 `[0, 0]` LSB；边界斜率 `[0, 0]` LSB/sample；首/尾 20ms 真实过零数 `[10, 10]` / `[11, 12]`。
- `bgm_game` 左/右头尾样本差 `[0, 0]` LSB；边界斜率 `[0, 0]` LSB/sample；首/尾 20ms 真实过零数 `[29, 27]` / `[13, 12]`。
- BGM 音符及混响尾音按循环周期环绕，文件端点各 3ms raised-cosine 淡化；无整段淡出静音。上述数值证明 PCM 接缝没有幅度跳变，且端点邻域确有音频与过零。

只做与交付要求直接相关的资产验证，没有运行无关的 Unity 测试或扩展游戏功能。

## 验收边界

短音效的 -16 LUFS 基于明确记录的 400ms 补零量测，不等同于无限循环时的响度；4× 真峰值是数值估计。未声称完成耳机/扬声器主观试听，也未集成或验收游戏内声音混合；这两项留给 Claude 后续素材验收/集成流程。复现只需 README 中的命令，无网络音频素材依赖。
