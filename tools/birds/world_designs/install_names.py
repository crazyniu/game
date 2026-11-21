#!/usr/bin/env python3
"""install_names.py <world>: cut the painted name sheets of one world into labels and register them.

Reads out/names/job_<lang>_gold_world<world>/words.png + texts.json (paint_names.py), cuts one label per line with tools/island/cut_rows.py, scales all
labels of a sheet by one factor so the letters have the stored cap height (the same rule as tools/island/install_painted_words.py), writes
PerchSort/assets/resources/art/locale/<lang>/island_words/lettering_bird_<bird>.png and registers it in the locale manifest ("painted": lettering_bird_<bird>, "words": bird.<bird>.name). Then run
`node tools/cocos-assets.mjs` and `node tools/cocos/meta.mjs`. Run from the repository root.
"""
import json, os, sys
import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "../../.."))
sys.path.insert(0, ROOT + "/tools/island")
import cut_rows  # noqa: E402

CAP = {"en": 96.0, "zh": 80.0}


def body_height(piece):
    cover = (piece[..., 3] > 128).sum(axis=1).astype(float)
    rows = np.nonzero(cover >= 0.3 * cover.max())[0]
    return float(rows[-1] - rows[0] + 1)


world = sys.argv[1]
birds = [l.split("\t")[1] for l in open(HERE + "/names.tsv") if l.split("\t")[0] == world]
for lang in ("en", "zh"):
    job = "%s/out/names/job_%s_gold_world%s" % (HERE, lang, world)
    texts = json.load(open(job + "/texts.json"))
    assert len(texts) == len(birds)
    pieces = cut_rows.cut(job + "/words.png", len(texts))
    scale = CAP[lang] / sorted(body_height(p) for p in pieces)[len(pieces) // 2]
    folder = "%s/PerchSort/assets/resources/art/locale/%s" % (ROOT, lang)
    manifest = json.load(open(folder + "/manifest.json"))
    for bird, piece in zip(birds, pieces):
        image = Image.fromarray(piece, "RGBA")
        image = image.resize((max(1, round(image.width * scale)), max(1, round(image.height * scale))), Image.LANCZOS)
        rel = "island_words/lettering_bird_%s.png" % bird
        image.save("%s/%s" % (folder, rel))
        manifest["painted"]["lettering_bird_%s" % bird] = {"file": rel}
        manifest["words"]["bird.%s.name" % bird] = {"gold": {"file": rel, "cap": CAP[lang]}}
    json.dump(manifest, open(folder + "/manifest.json", "w"), ensure_ascii=False, indent=2)
    print(lang, "installed", len(birds), "labels")
