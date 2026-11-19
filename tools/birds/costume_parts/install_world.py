#!/usr/bin/env python3
"""install_world.py <slot_bird>...: put finished world birds into the game resources as WebP.

Takes out/skin/<slot>_<bird>_<frame>.png (aligned by align.py with the world set, mirrored left frames included) and writes
PerchSort/assets/resources/birds/<bird>_<frame>.webp (quality 85) plus <bird>_face.webp, a 160x160 head portrait cut from the front frame.
Afterwards run `node tools/cocos-assets.mjs` and `python3 tools/cocos-meta.py`. Run from the repository root with pillow and numpy.
"""
import glob, os, sys
import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
BIRDS = os.path.abspath(os.path.join(HERE, "../../../PerchSort/assets/resources/birds"))
for key in sys.argv[1:]:
    slot, bird = key.split("_")
    frames = sorted(glob.glob("%s/out/skin/%s_%s_*.png" % (HERE, slot, bird)))
    assert len(frames) >= 30, "%s: only %d frames" % (key, len(frames))
    for f in frames:
        frame = os.path.basename(f)[len(slot) + len(bird) + 2:-4]
        Image.open(f).convert("RGBA").save("%s/%s_%s.webp" % (BIRDS, bird, frame), "WEBP", quality=85, method=4)
    front = Image.open("%s/out/skin/%s_%s_front.png" % (HERE, slot, bird)).convert("RGBA")
    a = np.asarray(front.getchannel("A")) > 24
    ys, xs = np.nonzero(a)
    top, bottom, left, right = ys.min(), ys.max(), xs.min(), xs.max()
    side = int((bottom - top) * 0.62)
    cx = (left + right) // 2
    face = front.crop((cx - side // 2, top, cx + side // 2 + side % 2, top + side)).resize((160, 160), Image.LANCZOS)
    face.save("%s/%s_face.webp" % (BIRDS, bird), "WEBP", quality=85, method=4)
    print("installed", bird, len(frames), "frames + face")
