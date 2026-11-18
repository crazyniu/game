#!/usr/bin/env python3
"""pack_skin.py <species_slot>...: pack finished skins as WebP zips for transfer.

Takes out/skin/<species>_<slot>_<frame>.png and writes out/pack/<species>_<slot>.zip holding <species>_outfit_<slot>_<frame>.webp
(quality 85). install_skin.py on the game machine unpacks it into the game resources.
"""
import glob, io, os, sys, zipfile
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
os.makedirs(HERE + "/out/pack", exist_ok=True)
for key in sys.argv[1:]:
    species, slot = key.split("_")
    frames = sorted(glob.glob("%s/out/skin/%s_%s_*.png" % (HERE, species, slot)))
    assert len(frames) >= 60, "%s: only %d frames" % (key, len(frames))
    with zipfile.ZipFile("%s/out/pack/%s.zip" % (HERE, key), "w", zipfile.ZIP_STORED) as z:
        for f in frames:
            frame = os.path.basename(f)[len(species) + len(slot) + 2:-4]
            b = io.BytesIO(); Image.open(f).convert("RGBA").save(b, "WEBP", quality=85, method=4)
            z.writestr("%s_outfit_%s_%s.webp" % (species, slot, frame), b.getvalue())
    print("packed", key, len(frames))
