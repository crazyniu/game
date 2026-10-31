"""Measure delivered PCM files; regenerate MANIFEST.md and analysis.json."""
import json
from pathlib import Path
import wave

import numpy as np
import pyloudnorm as pyln
from scipy.signal import resample_poly

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "audio_src"
SR = 44100
PURPOSES = {
    "bgm_menu": "主菜单；C 大调，64 BPM，16 小节，木琴/长笛/竖琴",
    "bgm_game": "游戏；C 大调，96 BPM，36 小节，轻快低干扰",
    **{f"chirp_{i:02}": f"选中/落地随机鸟鸣变体 {i}" for i in range(1, 6)},
    "flutter": "飞行振翅", "sleep_z": "轻柔下行入睡",
    "nest_home": "归巢上行三音与闪光铃", "crate_open": "木箱碎裂",
    "star_1": "第一颗星，C6", "star_2": "第二颗星，E6",
    "star_3": "第三颗星，G6", "win_fanfare": "通关庆祝",
    "hint": "提示出现", "ui_tap": "按钮按下", "ui_back": "返回",
}


def loudness(samples):
    # BS.1770 / EBU R128 has a 400 ms integration block. Padding is for
    # measurement only; it never extends the delivered short effect.
    padded = np.pad(samples, ((0, max(0, 17640-len(samples))), (0, 0)))
    return float(pyln.Meter(SR).integrated_loudness(padded))


def db(value):
    return float(20 * np.log10(max(float(value), 1e-12)))


def zero_crossings(samples):
    # Exclude exact digital zero so fade-to-zero alone cannot pass this check.
    return [int(np.count_nonzero(np.diff(np.signbit(c[c != 0]))))
            for c in samples.T]


def analyze():
    expected = {name + ".wav" for name in PURPOSES}
    assert {p.name for p in OUT.glob("*.wav")} == expected
    results = []
    for name, purpose in PURPOSES.items():
        with wave.open(str(OUT / (name + ".wav")), "rb") as wav:
            assert wav.getframerate() == SR and wav.getsampwidth() == 2
            assert wav.getnchannels() in (1, 2) and wav.getcomptype() == "NONE"
            pcm = np.frombuffer(wav.readframes(wav.getnframes()), dtype="<i2")
            pcm = pcm.reshape(-1, wav.getnchannels())
        samples = pcm.astype(np.float64) / 32768
        peak = np.max(np.abs(samples))
        result = dict(file=name + ".wav", purpose=purpose,
                      seconds=len(samples)/SR, sample_rate=SR, bits=16,
                      channels=samples.shape[1], lufs=loudness(samples),
                      sample_peak_dbfs=db(peak),
                      true_peak_4x_dbtp=db(np.max(np.abs(resample_poly(samples, 4, 1, axis=0)))),
                      clipped_samples=int(np.count_nonzero((pcm == -32768) | (pcm == 32767))),
                      dc_offset=float(np.max(np.abs(np.mean(samples, axis=0)))),
                      endpoint_max_lsb=int(np.max(np.abs(pcm[[0, -1]].astype(int)))),
                      loudness_padding_ms=max(0, (17640-len(samples))/SR*1000))
        assert abs(result["lufs"] + 16) < 0.5, result
        assert result["true_peak_4x_dbtp"] <= -1, result
        assert result["clipped_samples"] == 0 and result["endpoint_max_lsb"] <= 1, result
        assert result["dc_offset"] < 0.003, result
        duration = result["seconds"]
        fixed = {"bgm_menu": 60, "bgm_game": 90, "flutter": .3,
                 "sleep_z": .5, "nest_home": .8, "crate_open": .6,
                 "win_fanfare": 2, "star_1": .35, "star_2": .35,
                 "star_3": .35, "hint": .45, "ui_tap": .1, "ui_back": .18}
        if name in fixed:
            assert abs(duration-fixed[name]) <= 1/SR, result
        if name.startswith("chirp"):
            assert .15 <= duration <= .4, result
        if name.startswith("bgm"):
            window = round(.02*SR)
            loop = dict(
                boundary_delta_lsb=np.abs(pcm[0].astype(int)-pcm[-1]).tolist(),
                boundary_slope_lsb=np.max(np.abs(np.array([
                    pcm[1].astype(int)-pcm[0], pcm[-1].astype(int)-pcm[-2]])), axis=0).tolist(),
                head_zero_crossings_20ms=zero_crossings(samples[:window]),
                tail_zero_crossings_20ms=zero_crossings(samples[-window:]),
                head_rms_20ms_dbfs=db(np.sqrt(np.mean(samples[:window]**2))),
                tail_rms_20ms_dbfs=db(np.sqrt(np.mean(samples[-window:]**2))))
            assert max(loop["boundary_delta_lsb"]) <= 1, loop
            assert max(loop["boundary_slope_lsb"]) <= 2, loop
            assert min(loop["head_zero_crossings_20ms"] + loop["tail_zero_crossings_20ms"]) > 0, loop
            assert min(loop["head_rms_20ms_dbfs"], loop["tail_rms_20ms_dbfs"]) > -55, loop
            result["loop"] = loop
        results.append(result)
    (OUT / "analysis.json").write_text(json.dumps(results, ensure_ascii=False, indent=2) + "\n")
    lines = ["# Original audio manifest", "",
             "18 个项目原创合成音频；WAV / 44.1 kHz / signed PCM 16-bit。生成与分析脚本位于 `scripts/audio/`。",
             "", "响度：pyloudnorm 的 BS.1770 / EBU R128 integrated LUFS，400ms 分块。短于 400ms 的文件仅在测量时尾补零至 400ms，交付 WAV 时长不变；短音效 LUFS 不是持续播放的听感保证。峰值为最终 PCM 测量值；真峰值为 4 倍过采样估计。",
             "", "| 文件 | 秒 | 声道 | 用途 | 峰值 dBFS | LUFS | 真峰值 dBTP (4×) | 测量补零 ms |",
             "|---|---:|---:|---|---:|---:|---:|---:|"]
    for r in results:
        lines.append(f'| `{r["file"]}` | {r["seconds"]:.3f} | {r["channels"]} | {r["purpose"]} | {r["sample_peak_dbfs"]:.2f} | {r["lufs"]:.2f} | {r["true_peak_4x_dbtp"]:.2f} | {r["loudness_padding_ms"]:.0f} |')
    lines += ["", "## 循环与边界", "",
              "BGM 按整小节合成，音符与混响尾音环绕写入周期缓冲；仅在文件端点各用 3ms raised-cosine 淡入/淡出消除接缝跳变，无长静音。所有 SFX 同样带首尾淡化。", ""]
    for r in results[:2]:
        v = r["loop"]
        lines.append(f'- `{r["file"]}`：头尾样本差 {v["boundary_delta_lsb"]} LSB，边界斜率 {v["boundary_slope_lsb"]} LSB/sample；首/尾 20ms 真实过零数 {v["head_zero_crossings_20ms"]} / {v["tail_zero_crossings_20ms"]}；首/尾 RMS {v["head_rms_20ms_dbfs"]:.2f} / {v["tail_rms_20ms_dbfs"]:.2f} dBFS。')
    lines += ["", "全部文件削波样本数为 0，端点为数字零；完整测量数据见 `analysis.json`。脚本验收通过不代替耳机/扬声器的主观音色验收及游戏内混音验收。", ""]
    (OUT / "MANIFEST.md").write_text("\n".join(lines))
    print(f"PASS: {len(results)} WAV files; PCM format, duration, LUFS, peak, fades, clipping, and BGM seams.")


if __name__ == "__main__":
    analyze()
