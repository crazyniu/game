#!/usr/bin/env python3
"""Installs painted number glyphs (three colour styles) into every locale pack.

python3 tools/island/install_numbers.py <cut_dir>

<cut_dir>/<style>/ (cream, brown, gold) holds 0..9, slash, plus, x and minus as cut by cut_sheet.py. Within a style all
glyphs share one scale (digits become about 124 px tall), digits sit on a common baseline in a 128 px line, the signs
are centred on the digit middle line, and each glyph's cell is as wide as its ink plus a small margin. The pack
manifest gets "numbers": {cap, line_height, baseline, styles: {style: {glyphs, advances}}}.
"""
import glob
import json
import os
import subprocess
import sys

import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
LOCALES = os.path.join(ROOT, "PerchSort", "art", "locale")
CHARS = {**{str(d): str(d) for d in range(10)}, "/": "slash", "+": "plus", "x": "x", "-": "minus"}
LINE, BASELINE, DIGIT, MARGIN = 128, 126, 124.0, 5


def main():
    cut_dir = sys.argv[1]
    built = {}
    for style in ("cream", "brown", "gold"):
        src = {c: Image.open(os.path.join(cut_dir, style, name + ".png")).convert("RGBA") for c, name in CHARS.items()}
        heights = sorted(src[str(d)].height for d in range(10))
        k = DIGIT / heights[5]
        built[style] = {}
        for c, im in src.items():
            im = im.resize((max(1, round(im.width * k)), max(1, round(im.height * k))), Image.LANCZOS)
            cell = Image.new("RGBA", (im.width + 2 * MARGIN, LINE), (0, 0, 0, 0))
            if c.isdigit() or c == "/":
                top = BASELINE - im.height + 1
            else:
                top = round(BASELINE - DIGIT / 2 - im.height / 2)
            cell.alpha_composite(im, (MARGIN, max(0, top)))
            built[style][c] = cell
    for folder in sorted(glob.glob(os.path.join(LOCALES, "*", "manifest.json"))):
        base = os.path.dirname(folder)
        for old in glob.glob(os.path.join(base, "number_*")):
            os.remove(old)
        manifest = json.load(open(folder))
        styles = {}
        for style, glyphs in built.items():
            files, advances = {}, {}
            for c, cell in glyphs.items():
                name = "number_%s_%s.png" % (style, CHARS[c])
                cell.save(os.path.join(base, name))
                files[c], advances[c] = name, cell.width
            styles[style] = {"glyphs": files, "advances": advances}
        manifest["numbers"] = {"cap": LINE, "line_height": LINE, "baseline": BASELINE, "styles": styles}
        with open(folder, "w") as out:
            json.dump(manifest, out, indent=2, ensure_ascii=False)
            out.write("\n")
        print(base, "numbers installed")


main()
subprocess.run([sys.executable, os.path.join(os.path.dirname(os.path.abspath(__file__)), "tone_brown.py")], check=True)   # brown lettering matches the caption brown
