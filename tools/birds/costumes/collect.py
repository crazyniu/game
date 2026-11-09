#!/usr/bin/env python3
"""Copy each finished Codex costume job's generated image to out/<species>_outfit_<n>_raw.png (matched by session id)."""
import glob, os, re, shutil
D = os.path.join(os.path.dirname(os.path.abspath(__file__)), "out")
for log in sorted(glob.glob(D + "/*_outfit_?.log")):
    m = re.search(r"session id: ([0-9a-f-]+)", open(log, errors="ignore").read())
    if not m:
        continue
    imgs = sorted(glob.glob(os.path.expanduser("~/.codex/generated_images/%s/*.png" % m.group(1))), key=os.path.getmtime)
    if imgs:
        shutil.copy(imgs[-1], log[:-4] + "_raw.png")
        print(os.path.basename(log)[:-4], len(imgs))
