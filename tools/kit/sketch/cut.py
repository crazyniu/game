"""Cut every alpha component (with its small attached bits) of a transparent sheet into its own PNG, ordered row-major."""
import sys
import numpy as np
from PIL import Image
from scipy import ndimage
src, prefix = sys.argv[1], sys.argv[2]
im = np.array(Image.open(src).convert("RGBA"))
mask = ndimage.binary_dilation(im[..., 3] > 16, iterations=6)
lab, n = ndimage.label(mask)
objs = ndimage.find_objects(lab)
items = []
for i, sl in enumerate(objs):
    area = (lab[sl] == i + 1).sum()
    if area < 4000:
        continue
    items.append((sl[0].start, sl[1].start, sl, i + 1))
items.sort(key=lambda t: (round(t[0] / 120), t[1]))
for k, (y, x, sl, idx) in enumerate(items):
    piece = im[sl].copy()
    piece[..., 3][lab[sl] != idx] = 0
    a = piece[..., 3] > 16
    ys, xs = np.where(a)
    piece = piece[ys.min():ys.max() + 1, xs.min():xs.max() + 1]
    piece[..., :3][piece[..., 3] == 0] = 0
    Image.fromarray(piece).save(f"{prefix}_{k}.png")
    print(k, piece.shape[1], piece.shape[0], "at", x, y)
