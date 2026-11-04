# T6 — 音效资产 + AudioManager（只做资产与 API，不接入游戏逻辑）

## 做什么
1. 用脚本（Python 标准库 + 可选 numpy，脚本放 `tools/audio/generate_sfx.py`，可重复运行）**原创合成**短音效 WAV（44.1kHz 16bit 单声道，每个 ≤ 1.2 秒，首尾有淡入淡出避免爆音，峰值归一到约 -3 dBFS）：
   `select`（选中小鸟，轻柔上扬）、`move`（落位，轻“噗”）、`collect`（一群鸟集齐回家，明亮的上行琶音，约 0.8–1.0 秒）、`win`（通关小号角式短旋律，约 1.2 秒）、`error`（非法操作，短促低沉但不刺耳）、`undo`（反向轻划音）、`click`（按钮）。风格：柔和、明亮、木质/风铃感，低疲劳（会重复听数百次）。
2. 可选：一个约 30–60 秒、可无缝循环的轻柔背景音乐 `bgm_loop.wav`（五声音阶，低音量、不抢戏）。若质量没把握就**不要做**，在报告里说明；宁缺毋滥。
3. 输出到 `PerchSort/Assets/Audio/`（导入设置脚本统一：SFX 用 Decompress On Load/ADPCM 合适值，BGM 用 Streaming/Vorbis）。
4. `PerchSort/Assets/Scripts/Audio/AudioManager.cs`（单例，DontDestroyOnLoad，自建 AudioSource 池）：
   - `enum Sfx { Select, Move, Collect, Win, Error, Undo, Click }`
   - `void Play(Sfx sfx)`；`void SetSfxEnabled(bool)`；`void SetMusicEnabled(bool)`；`void PlayMusic()/StopMusic()`；同一音效 50ms 内重复触发做节流；音效/音乐开关立即生效；资源通过 `Resources` 或序列化引用加载均可，但缺资源时不抛异常只记一次警告。
   - 配 EditMode 测试：开关语义、节流、缺资源不崩。
5. **不要修改** `Scripts/View`、`Scripts/UI`、场景或 T5 正在改的任何文件；接线由后续任务完成。

## 约束
- 全部原创合成，不使用任何第三方采样；不提交 git。
- 测试保持全绿：`bash scripts/run_tests.sh`（注意另一个任务可能并行改动 UI 相关代码；如测试因对方未完成的改动失败，在报告里说明，不要去改对方文件）。

## 验收（Claude 将检查）
- 用脚本分析每个 WAV：时长、采样率、峰值、首尾是否近零（防爆音）、无削波；我无法听音，因此你需在 `tasks/T6_report.md` 附上频谱/波形摘要与每个音效的音符/频率设计说明，并明确哪些需要真人试听。
- AudioManager 测试通过。
