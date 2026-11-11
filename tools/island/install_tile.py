#!/usr/bin/env python3
"""Installs the shop tile painted by local Codex in the style of the home page's button tiles: island_tile (plain) and
island_tile_selected (green rim), both trimmed to the same box and 512 px wide, drawn as nine-patches (corners kept).
The plain tile is the selected one with its green rim replaced by the plain painting's rim, because the plain painting
came back with a see-through face.

python3 tools/island/install_tile.py <job dir>    (<job dir>/out/tile.png and tile_selected.png)
"""
import os
import sys

import numpy as np
from PIL import Image

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "PerchSort", "art", "island")


def main():
    out = os.path.join(sys.argv[1], "out")
    sel = np.asarray(Image.open(os.path.join(out, "tile_selected.png")).convert("RGBA")).astype(np.float32)
    plain = np.asarray(Image.open(os.path.join(out, "tile.png")).convert("RGBA")).astype(np.float32)
    green = (sel[..., 1] > sel[..., 0] + 25) & (sel[..., 1] > sel[..., 2] + 25)
    tile = sel.copy()
    keep = green & (plain[..., 3] > 200)
    tile[keep] = plain[keep]
    tile[green & ~keep, 3] = 0
    # Any green left on the plain tile's rim becomes the ivory of its face.
    left = (tile[..., 1] > tile[..., 0] + 10) & (tile[..., 3] > 0)
    tile[left, :3] = np.array([246, 232, 214], np.float32)
    box = Image.fromarray(sel.astype(np.uint8), "RGBA").getchannel("A").point(lambda a: 255 if a > 16 else 0).getbbox()
    for name, arr in [("island_tile", tile), ("island_tile_selected", sel)]:
        img = Image.fromarray(arr.astype(np.uint8), "RGBA").crop(box)
        img.resize((512, round(img.height * 512 / img.width)), Image.LANCZOS).save(os.path.join(OUT, name + ".png"))


if __name__ == "__main__":
    main()
