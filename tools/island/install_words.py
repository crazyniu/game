#!/usr/bin/env python3
"""Installs cut painted words into the locale packs.

python3 tools/island/install_words.py <stage_dir>

Files in <stage_dir> are named <lang>__<id>.png. Each is copied to PerchSort/art/locale/<lang>/island_words/<id>.png and
listed under "painted" in that locale's manifest.json (id -> {"file": ...}). Painted words keep their own colours and
are placed with UiText.painted(); the tint and outline shader of the older white labels are not applied.
"""
import json
import os
import shutil
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))


def main():
    stage = sys.argv[1]
    manifests = {}
    for f in sorted(os.listdir(stage)):
        if not f.endswith(".png") or "__" not in f:
            continue
        lang, word = f[:-4].split("__", 1)
        folder = os.path.join(ROOT, "PerchSort", "art", "locale", lang)
        mpath = os.path.join(folder, "manifest.json")
        if lang not in manifests:
            manifests[lang] = json.load(open(mpath))
            manifests[lang].setdefault("painted", {})
        os.makedirs(os.path.join(folder, "island_words"), exist_ok=True)
        shutil.copyfile(os.path.join(stage, f), os.path.join(folder, "island_words", word + ".png"))
        manifests[lang]["painted"][word] = {"file": "island_words/%s.png" % word}
    for lang, m in manifests.items():
        with open(os.path.join(ROOT, "PerchSort", "art", "locale", lang, "manifest.json"), "w") as out:
            json.dump(m, out, indent=2, ensure_ascii=False)
            out.write("\n")
        print(lang, len(m["painted"]), "painted words")


main()
