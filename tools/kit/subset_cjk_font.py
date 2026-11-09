#!/usr/bin/env python3
"""Cut the Simplified Chinese face of Noto Sans CJK Bold down to the characters the zh locale uses.

python3 tools/kit/subset_cjk_font.py /path/to/NotoSansCJK-Bold.ttc
Reads tools/kit/locales/zh.json and zh_docs.json, writes tools/kit/fonts/noto_sans_sc_bold.otf. Re-run it after adding
Chinese phrases. Needs fonttools. The font is used only by build_localized_labels.py (see fonts/README.md).
"""
import json
import sys
from pathlib import Path

from fontTools import subset
from fontTools.ttLib import TTCollection

ROOT = Path(__file__).resolve().parents[2]
locales = ROOT / "tools/kit/locales"
text = "".join(json.loads((locales / "zh.json").read_text(encoding="utf-8"))["labels"].values())
for doc in json.loads((locales / "zh_docs.json").read_text(encoding="utf-8")).values():
    text += doc["title"] + doc["body"]
text += "".join(map(chr, range(32, 127))) + "，。！？：；、（）《》“”…—·"
font = next(f for f in TTCollection(sys.argv[1]).fonts if f["name"].getDebugName(1) == "Noto Sans CJK SC")
options = subset.Options()
options.layout_features = ["*"]
subsetter = subset.Subsetter(options)
subsetter.populate(text=text)
subsetter.subset(font)
out = ROOT / "tools/kit/fonts/noto_sans_sc_bold.otf"
font.flavor = None
font.save(out)
print(out, out.stat().st_size, "bytes,", len(set(text)), "characters")
