#!/usr/bin/env python3
"""Installs painted word sheets into the locale packs and registers every word by label id and style.

python3 tools/island/install_painted_words.py <jobs_dir>

<jobs_dir> holds one folder per generated sheet, named job_<lang>_<style>_<name>, each with words.png (one word or
phrase per line, top to bottom) and texts.json (the lines, same order). Folders whose name ends in "fix" are applied
last and win over earlier sheets of the same text. tools/island/word_catalog.json lists, per style (gold, brown,
caption, white), the label ids with their English and Chinese text, and "reuse": words that were painted earlier and
are only registered under their catalog id.

Every sheet is scaled by one factor so the letters of all words have the same height (the typical body height of the
sheet's lines becomes the style's cap height), cropped line by line and written to
PerchSort/art/locale/<lang>/island_words/<style>_<id>.png. The pack manifest gets
"words": {label id: {style: {"file": ..., "cap": ...}}}; the runtime scales a word by cap / its recorded cap.
"""
import json
import os
import subprocess
import sys

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import cut_rows  # noqa: E402

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
LOCALES = os.path.join(ROOT, "PerchSort", "art", "locale")
LANGS = {"en": 1, "zh": 2}    # language -> column in the catalog entries
CAP = {"en": 96.0, "zh": 80.0}  # stored cap height in pixels; Chinese glyph boxes are taller than Latin capitals
STYLES = ("gold", "brown", "caption", "white")


def body_height(piece):
    """Height of the dense band of rows (the letters without ascenders and descenders)."""
    cover = (piece[..., 3] > 128).sum(axis=1).astype(float)
    rows = np.nonzero(cover >= 0.3 * cover.max())[0]
    return float(rows[-1] - rows[0] + 1)


def main():
    jobs_dir = sys.argv[1]
    catalog = json.load(open(os.path.join(ROOT, "tools", "island", "word_catalog.json")))
    jobs = sorted(os.listdir(jobs_dir), key=lambda n: (n.endswith("fix"), n))
    cut = {}   # (lang, style) -> {text: piece scaled to the stored cap}
    for name in jobs:
        parts = name.split("_")
        folder = os.path.join(jobs_dir, name)
        if parts[0] != "job" or parts[1] not in LANGS or not os.path.exists(os.path.join(folder, "words.png")):
            continue
        lang, style = parts[1], parts[2]
        texts = json.load(open(os.path.join(folder, "texts.json")))
        pieces = cut_rows.cut(os.path.join(folder, "words.png"), len(texts))
        bodies = sorted(body_height(p) for p in pieces)
        scale = CAP[lang] / bodies[len(bodies) // 2]
        for text, piece in zip(texts, pieces):
            image = Image.fromarray(piece, "RGBA")
            image = image.resize((max(1, round(image.width * scale)), max(1, round(image.height * scale))), Image.LANCZOS)
            cut.setdefault((lang, style), {})[text] = image
    for lang, col in LANGS.items():
        folder = os.path.join(LOCALES, lang)
        mpath = os.path.join(folder, "manifest.json")
        manifest = json.load(open(mpath))
        painted = manifest.get("painted", {})
        words = {}
        written = {}   # (style, text) -> relative file, so equal words share one picture
        os.makedirs(os.path.join(folder, "island_words"), exist_ok=True)
        missing = []
        for style in STYLES:
            for entry in catalog[style]:
                wid, text = entry[0], entry[col]
                key = (style, text)
                if key not in written:
                    image = cut.get((lang, style), {}).get(text)
                    if image is not None:
                        rel = "island_words/%s_%s.png" % (style, wid.replace(".", "_"))
                        image.save(os.path.join(folder, rel))
                        written[key] = {"file": rel, "cap": CAP[lang]}
                    elif style + "." + wid in catalog["reuse"]:
                        written[key] = {"file": painted[catalog["reuse"][style + "." + wid]]["file"], "cap": None}
                    else:
                        missing.append("%s.%s %r" % (style, wid, text))
                        continue
                words.setdefault(wid, {})[style] = written[key]
        for key, painted_id in catalog["reuse"].items():   # earlier words that no sheet repeats
            style, wid = key.split(".", 1)
            if style not in words.get(wid, {}):
                words.setdefault(wid, {})[style] = {"file": painted[painted_id]["file"], "cap": None}
        # Words painted earlier by hand carry no recorded cap: infer it from the typical ink-to-cap ratio of the sheets.
        ratios = [im.height / CAP[lang] for sheet in cut.values() for im in sheet.values()]
        ratio = float(np.median(ratios))
        for styles in words.values():
            for info in styles.values():
                if info["cap"] is None:   # shared with every id of the same word
                    ink = Image.open(os.path.join(folder, info["file"])).getbbox()
                    info["cap"] = round((ink[3] - ink[1]) / ratio, 1)
        manifest["words"] = words
        with open(mpath, "w") as out:
            json.dump(manifest, out, indent=2, ensure_ascii=False)
            out.write("\n")
        print(lang, len(words), "label ids,", len(written), "pictures; missing:", len(missing))
        for m in missing:
            print("  missing", m)


main()
subprocess.run([sys.executable, os.path.join(os.path.dirname(os.path.abspath(__file__)), "tone_brown.py")], check=True)   # brown lettering matches the caption brown
