# T6 — 原创音效与 AudioManager

日期：2026-10-31。音效资产、合成/分析脚本、导入器、AudioManager API 已完成；7 个 WAV 的客观检查通过，音频 EditMode 测试 **12/12 通过**，最新隔离副本的整套脚本 **EditMode 99/99、PlayMode 8/8 全绿**。尚未接入游戏逻辑，尚未真人试听；不能据此宣称游戏内混音或低疲劳听感已验收。

## 交付文件与复现

- `tools/audio/generate_sfx.py`：只用 Python 标准库进行原创加法合成；没有第三方采样、音频下载或生成服务。噪声使用固定种子 601。
- `tools/audio/analyze_sfx.py`：从最终 WAV 重新读取 PCM，执行格式、时长、峰值、边界、削波检查，计算波形与频谱摘要。
- `tools/audio/analysis.json`：全部数值及逐文件 SHA256。
- `tools/audio/waveforms.svg`：七段波形包络与平均功率频谱图。
- `PerchSort/Assets/Audio/Resources/Audio/{select,move,collect,win,error,undo,click}.wav`。
- `PerchSort/Assets/Scripts/Audio/AudioManager.cs`、独立运行时程序集及 `Editor/SfxImporter.cs`。
- `PerchSort/Assets/Tests/EditMode/Audio/AudioManagerTests.cs`、独立测试程序集。
- `tools/audio/audio-tests.xml`：Unity 实际测试结果，12 passed / 0 failed，2026-10-31 05:46:29 UTC。
- `tools/audio/full-editmode-tests.xml`、`tools/audio/full-playmode-tests.xml`：最新快照整套回归的实际结果，99/99 与 8/8 通过。

在仓库根目录复现：

```sh
python3 tools/audio/generate_sfx.py
python3 tools/audio/analyze_sfx.py
```

已额外验证一次重复合成：七个 WAV 的 SHA256 全部不变。无需安装 numpy 或音频库。

## 每个音效的设计

统一以正弦泛音叠加构造声音。木质音色的频率比/初始振幅为 `1:1, 2:0.16, 3.01:0.055`；风铃为 `1:1, 2:0.20, 3:0.065, 4.02:0.025`；柔和号角为 `1:1, 2:0.23, 3:0.11, 4:0.035`。高次泛音额外快速衰减，避免持续尖锐高频。每个音符含 8 ms 正弦平方起音、35 ms 尾部包络，整个文件再叠加 8 ms / 20 ms 边界淡入淡出，最后统一峰值归一化至 -3 dBFS。

| 音效 | 时长 | 音符/频率与时间设计 | 预期听感，待真人确认 |
| --- | ---: | --- | --- |
| select | 0.240 s | E5→G5，659.255→783.991 Hz 线性滑音，木质泛音，指数衰减常数 85 ms | 轻柔上扬，短促选中反馈 |
| move | 0.180 s | 220→150 Hz 木质滑落，衰减 42 ms；叠加 0.20 振幅的 850 Hz 单极低通噪声，噪声衰减 30 ms | 柔软落位“噗”，少量气息，不用尖锐撞击 |
| collect | 0.940 s | 0/130/260/400 ms 起奏 C5/E5/G5/C6：523.251/659.255/783.991/1046.502 Hz；风铃泛音；末音延续 540 ms | 四级明亮上行琶音，集齐回家提示 |
| win | 1.200 s | 0/200/400/620 ms 起奏 C5/E5/G5/C6；柔和号角泛音；末音叠加低振幅 C5、E5 支撑至 1.2 s | 比 collect 更从容、有结尾感的短号角 |
| error | 0.220 s | G3→E3，196→164.814 Hz，木质泛音，衰减 65 ms | 低沉小幅下落，不使用刺耳蜂鸣或强烈不协和 |
| undo | 0.280 s | G5→C5，783.991→523.251 Hz，木质泛音，衰减 100 ms | 反向轻划，与 select 的上扬形成辨识 |
| click | 0.085011 s | A5，880 Hz 固定音高，木质泛音，衰减 20 ms | 简短圆润按钮反馈；长度按整采样取整 |

**没有制作可选 BGM。** 30–60 秒音乐的旋律耐听度和循环衔接需要实际试听，此次没有这样的质量验证，不交付未经确认的音乐。`PlayMusic` API 与未来 BGM 的 Streaming/Vorbis 导入规则已保留；当前主动调用 `PlayMusic()` 会只输出一次缺少 `bgm_loop` 的警告，不抛异常。

## 波形与频谱实测

所有文件均为 **44,100 Hz / 16-bit PCM / mono**，峰值均为 **-3.0004 dBFS**。首采样和尾采样均为 0，削波采样数全部为 0；前 1 ms 最大幅度不超过 0.001221（约 -58.3 dBFS），后 1 ms 全部量化为 0。频谱采用 4096 点 Hann 窗、50% 重叠，对全片累积功率；频率分辨率约 10.77 Hz，因此谱峰是近似值，滑音也不是单一频率。

| 音效 | RMS dBFS | 功率谱重心 Hz | 主要谱峰 Hz | >5 kHz 能量占比 |
| --- | ---: | ---: | --- | ---: |
| select | -13.43 | 708.4 | 678.3 / 1356.6 / 1410.4 / 1453.5 | <0.00001% |
| move | -15.20 | 209.5 | 204.6 / 409.1 / 624.5 / 495.3 | 0.00751% |
| collect | -14.84 | 838.9 | 1044.4 / 786.0 / 656.8 / 527.6 | <0.00001% |
| win | -14.14 | 854.0 | 1044.4 / 786.0 / 656.8 / 527.6 | <0.00001% |
| error | -13.95 | 192.8 | 193.8 / 376.8 / 570.6 / 882.9 | <0.00001% |
| undo | -13.48 | 734.3 | 742.9 / 699.8 / 656.8 / 613.7 | <0.00001% |
| click | -14.60 | 903.6 | 882.9 / 1755.0 / 2648.6 / 484.5 | <0.00001% |

波形图中，select/undo 为单次指数衰减，move/error 的能量集中在低频，collect/win 可见四次起奏后平滑衰减。最大绝对 DC 偏置为 error 的 0.00043807（约 -67.2 dBFS）。这些数字证明信号格式、余量和边界达到目标；不等同于已通过听感、响度一致性或压缩音质验收。

## 导入与 API 语义

`SfxImporter` 只作用于 `Assets/Audio/`，SFX 使用 Decompress On Load、ADPCM、预加载、保留采样率、单声道；未来名为 `bgm_loop` 的文件使用 Streaming、Vorbis quality 0.7、后台加载。导入字段依据 [Unity AudioImporterSampleSettings 文档](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/audioimportersamplesettings)，七个实际导入对象的配置已由测试读取验证。

- 命名空间 `PerchSort.Audio`；`Sfx` 顺序为 Select / Move / Collect / Win / Error / Undo / Click。
- 后续任务在根 GameObject 挂一次 `AudioManager`；`Awake` 设置 `Instance` 并在运行时 `DontDestroyOnLoad`，重复实例销毁。没有自动创建场景对象或修改现有场景接线。
- 默认从 `Resources.Load<AudioClip>("Audio/<name>")` 加载；可按枚举顺序在 Inspector 覆盖七个序列化引用；音乐引用也可覆盖。
- 自建 8 个 2D SFX AudioSource 加 1 个独立循环音乐 AudioSource；优先使用空闲槽，池满后按轮转槽替换，播放不新增对象。
- `Play(Sfx)` 按音效独立节流：小于 50 ms 的同类触发丢弃，恰好 50 ms 可播放，不阻止不同音效。使用实时钟，不受游戏暂停的 timeScale 影响。
- `SetSfxEnabled(false)` 立即静音、停止并清空 SFX 音源；关闭期间不排队，重新开启可立即播放，不复活旧音效；不影响音乐。
- `SetMusicEnabled(false)` 立即静音并暂停音乐；重新开启恢复已请求的音乐；关闭期间 `PlayMusic()` 只记录播放意图。`StopMusic()` 停止并清除播放意图，之后开关开启不会自动重播。
- 缺资源按资源名每个管理器生命周期警告一次；没有广泛异常捕获、重试、备用资源或额外防御分支。
- SFX 每路音量 0.14（约 -17.1 dB），音乐 0.06（约 -24.4 dB）。原始 WAV 八路同相峰值理论上界约 0.793（-2.0 dBFS），为同时触发留余量；ADPCM 解码后的真实输出与未来 BGM 混音仍需运行时试听。

## 测试与边界

Unity 6000.6.7f1，隔离项目 `/tmp/perch-t6.aXgINB/PerchSort`，音频测试 12/12 通过：五项行为用例覆盖立即关闭/重新开启、49 ms 拒绝与 50 ms 接受、不同音效互不节流、音乐意图与停止、缺资源一次警告及固定池；另七个参数用例验证实际 Resources 资产与导入器结果。测试仅通过内部时钟控制触发时间，未增加面向用户的测试 API。

最初 EditMode 用 `SendMessage("Awake")` 触发 Unity 的 `ShouldRunBehaviour()` 断言；已改为测试中反射直接调用 C# 初始化方法，当前 12 项结果均通过。测试不依赖扬声器输出，检查组件配置、播放请求分配、静音/停止和节流，不宣称已测听觉输出。

音频测试复现命令（只在隔离副本运行，避免抢占 T5 的主项目 Unity）：

```sh
/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath /tmp/perch-t6.aXgINB/PerchSort \
  -runTests -testPlatform EditMode -testFilter PerchSort.Tests.Audio.AudioManagerTests \
  -testResults /tmp/perch-t6.aXgINB/build/audio-tests.xml \
  -logFile /tmp/perch-t6.aXgINB/build/audio-tests.log
```

整套 `bash scripts/run_tests.sh` 首次在隔离快照遇到 T5 中间态 `GameHud.cs(190,24): CS1061 BoardView.InputEnabled`，刷新后第二次遇到 `TMP_Settings.get_clearDynamicDataOnBuild` 空引用；未修改对方代码。最后同步最新 Assets 与 scripts 后，在 `/tmp/perch-t6.aXgINB` 运行 `bash /tmp/perch-t6.aXgINB/scripts/run_tests.sh`，退出码 0，**EditMode 99/99、PlayMode 8/8 全绿**。已逐字节核对该验证副本的音频 C#、程序集定义与 WAV 和最终交付完全一致。

操作记录：重跑时曾误在主目录启动一次脚本，导致代码生成的 Game.unity 被重建；已停止该测试并在收到协调者回复前恢复启动前副本，SHA256 为 `730b7c30d7bd74ef4747e0a41ca08301c5ef013b45482cc8c5746d8d4c7284cc`。该失误与恢复均已向协调者报告；协调者确认场景是可重建生成物，并要求后续不再操作场景、T5 独占主目录 Unity，已遵守。没有修改 View/UI 的源码、没有提交 git，后续未回滚 T5 的任何文件。

## 真人试听清单（全部待完成）

- [ ] 七个音效各试听原始 WAV 和 Unity ADPCM 导入版本，检查开头/末尾是否有可闻爆音、压缩砂感、毛刺。
- [ ] 耳机与手机外放分别确认 move/error 的低频可辨识、不闷；确认 click 不尖、select 不像错误提示。
- [ ] 以正常操作速度连续听 select / move / click 至少 100 次，确认重复疲劳和实际混音音量；数值频谱不能替代此项。
- [ ] 确认 collect 的“集齐回家”上行琶音与 win 的通关号角能够清楚区分，且 win 的高音与和声不刺耳。
- [ ] 确认 undo 的下划方向与 select 的上扬直觉一致，error 温和但可感知。
- [ ] 后续接线后，在游戏真实输出中验证同时触发、音源池覆盖、暂停场景、音效/音乐开关、跨场景单例保留和重复实例销毁。
- [ ] 如后续添加 BGM，另行验收 30–60 秒无缝循环、手机外放与耳机混音、静音后恢复；本次没有可试听的 BGM。
