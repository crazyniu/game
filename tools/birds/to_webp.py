#!/usr/bin/env python3
"""to_webp.py: convert every PNG of PerchSort/assets/resources/birds to WebP (quality 85) and delete the PNG and its .meta.

Run after the skins are published; then run `node tools/cocos-assets.mjs` and `python3 tools/cocos-meta.py` to refresh the manifest and
asset identities. Run from the repository root.
"""
import glob, os
from concurrent.futures import ProcessPoolExecutor
from PIL import Image

BIRDS = os.path.join(os.path.dirname(os.path.abspath(__file__)), "../../PerchSort/assets/resources/birds")


def convert(path):
    Image.open(path).convert("RGBA").save(path[:-4] + ".webp", "WEBP", quality=85, method=6)
    os.remove(path)
    if os.path.exists(path + ".meta"):
        os.remove(path + ".meta")
    return os.path.getsize(path[:-4] + ".webp")


if __name__ == "__main__":
    files = sorted(glob.glob(BIRDS + "/*.png"))
    before = sum(os.path.getsize(f) for f in files)
    with ProcessPoolExecutor(4) as pool:
        after = sum(pool.map(convert, files, chunksize=8))
    print("%d files: %.1f MB -> %.1f MB" % (len(files), before / 1048576, after / 1048576))
