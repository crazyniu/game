#!/usr/bin/env python3
"""install_names.py <world|skins1..skins5>: cut the painted name sheets of one world (or of nine skin designs) into labels and register them.

Reads out/names/job_<lang>_gold_world<world>/words.png + texts.json (paint_names.py), cuts one label per line with tools/island/cut_rows.py, scales all
labels of a sheet by one factor so the letters have the stored cap height (the same rule as tools/island/install_painted_words.py), writes
PerchSort/assets/resources/art/locale/<lang>/island_words/lettering_bird_<bird>.png and registers it in the locale manifest ("painted": lettering_bird_<bird>, "words": bird.<bird>.name). Then run
`node tools/cocos-assets.mjs` and `python3 tools/cocos-meta.py`. Run from the repository root.
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
if world.startswith("skins"):
    k = int(world[5:])
    rows = [l.split("\t") for l in open(HERE + "/skin_names.tsv")][(k - 1) * 9:k * 9]
    keys = ["outfit_%s_%s" % (r[0], r[1]) for r in rows]   # a skin design's title, shared by every bird that wears the design
    job_set = world
else:
    keys = ["bird_" + l.split("\t")[1] for l in open(HERE + "/names.tsv") if l.split("\t")[0] == world]
    job_set = "world" + world
for lang in ("en", "zh"):
    job = "%s/out/names/job_%s_gold_%s" % (HERE, lang, job_set)
    texts = json.load(open(job + "/texts.json"))
    assert len(texts) == len(keys)
    pieces = cut_rows.cut(job + "/words.png", len(texts))
    scale = CAP[lang] / sorted(body_height(p) for p in pieces)[len(pieces) // 2]
    folder = "%s/PerchSort/assets/resources/art/locale/%s" % (ROOT, lang)
    manifest = json.load(open(folder + "/manifest.json"))
    for key, piece in zip(keys, pieces):
        image = Image.fromarray(piece, "RGBA")
        image = image.resize((max(1, round(image.width * scale)), max(1, round(image.height * scale))), Image.LANCZOS)
        rel = "island_words/lettering_%s.png" % key
        image.save("%s/%s" % (folder, rel))
        manifest["painted"]["lettering_" + key] = {"file": rel}
        if key.startswith("bird_"):
            manifest["words"]["bird.%s.name" % key[5:]] = {"gold": {"file": rel, "cap": CAP[lang]}}
    json.dump(manifest, open(folder + "/manifest.json", "w"), ensure_ascii=False, indent=2)
    print(lang, "installed", len(keys), "labels")
