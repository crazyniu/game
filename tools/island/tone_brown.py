#!/usr/bin/env python3
"""Flattens the glossy orange-brown of the brown lettering and brown digits to the deep chocolate of the caption words.

python3 tools/island/tone_brown.py

Applies to every picture the locale manifests list in the brown style and to number_brown_* glyphs. Body pixels become
the caption brown (62, 25, 2) with a faint lift where the painted highlight was; the cream outline is kept.
"""
import glob
import json
import os

import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
BASE = np.array([62.0, 25.0, 2.0])
LIFT = np.array([30.0, 22.0, 14.0])


def tone(path):
    im = np.array(Image.open(path).convert("RGBA")).astype(float)
    rgb, alpha = im[..., :3], im[..., 3]
    lum = rgb.mean(axis=2)
    body = np.clip((200.0 - lum) / 60.0, 0.0, 1.0)[..., None]           # 1 on the lettering, 0 on the cream outline
    lift = np.clip((lum - 70.0) / 120.0, 0.0, 1.0)[..., None] * LIFT
    new = rgb * (1.0 - body) + (BASE + lift) * body
    out = np.concatenate([np.clip(new, 0, 255), alpha[..., None]], axis=2).astype(np.uint8)
    out[out[..., 3] == 0, :3] = 0
    Image.fromarray(out, "RGBA").save(path)


def main():
    done = set()
    for mpath in glob.glob(os.path.join(ROOT, "PerchSort", "art", "locale", "*", "manifest.json")):
        base = os.path.dirname(mpath)
        manifest = json.load(open(mpath))
        files = {v["brown"]["file"] for v in manifest["words"].values() if "brown" in v}
        files |= set(glob.glob(os.path.join(base, "number_brown_*.png")))
        for f in files:
            path = f if os.path.isabs(f) else os.path.join(base, f)
            if path not in done:
                tone(path)
                done.add(path)
    print(len(done), "pictures toned")


main()
