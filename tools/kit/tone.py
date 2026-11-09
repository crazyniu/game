"""Tone down the painted kit: the sketch-derived pieces came out candy-bright. Saturated pixels lose some
saturation and a little brightness; cream and white stay as they are (the change is weighted by saturation).

uv run --no-project --with pillow --with numpy python tools/kit/tone.py <png>... [--sat 0.8 --val 0.92] [--out DIR]
Without --out the files are rewritten in place (originals stay in Git history)."""
import argparse, os
import numpy as np
from PIL import Image

def tone(img, sat, val):
    rgba = np.asarray(img.convert("RGBA")).astype(np.float32) / 255.0
    hsv = np.asarray(img.convert("RGB").convert("HSV")).astype(np.float32) / 255.0
    s = hsv[..., 1]
    w = np.clip((s - 0.25) / 0.45, 0.0, 1.0)          # 0 for cream/white, 1 for strong colour
    hsv[..., 1] = s * (1.0 - w * (1.0 - sat))
    hsv[..., 2] = hsv[..., 2] * (1.0 - w * (1.0 - val))
    rgb = Image.fromarray((hsv * 255.0).round().astype(np.uint8), "HSV").convert("RGB")
    out = np.asarray(rgb).astype(np.float32) / 255.0
    return Image.fromarray((np.dstack([out, rgba[..., 3:]]) * 255.0).round().astype(np.uint8), "RGBA")

if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("files", nargs="+")
    ap.add_argument("--sat", type=float, default=0.8)
    ap.add_argument("--val", type=float, default=0.92)
    ap.add_argument("--out")
    a = ap.parse_args()
    for f in a.files:
        t = tone(Image.open(f), a.sat, a.val)
        dst = os.path.join(a.out, os.path.basename(f)) if a.out else f
        t.save(dst, optimize=True)
        print(dst)
