#!/usr/bin/env python3
"""Writes the island board scenes: every world gets the island painting (day and night) in its own colour grade, the
palm-wood branch sprites (rows 0..5, left and right) and the slot file the board and the tutorial read.

python3 tools/island/build_board_scene.py <day_bg.png> <night_bg.png> <branch_left.png> <branch_right.png>
"""
import json
import sys
import os

import numpy as np
from PIL import Image

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "PerchSort", "world", "scene")
GRADES = [None, (150, 215, 170), (255, 170, 110), (200, 225, 255), (255, 175, 200), (190, 150, 235), (90, 200, 200),
          (150, 170, 200), (130, 220, 120), (200, 160, 255)]
LENGTH = 540
ROWS = 6


def grade(img, color, amount=0.22):
    if color is None:
        return img
    a = np.asarray(img.convert("RGB")).astype(np.float32)
    tint = np.array(color, np.float32)
    tint = tint / tint.mean()
    mixed = a * (1 - amount) + a * tint * amount
    return Image.fromarray(np.clip(mixed, 0, 255).astype(np.uint8), "RGB")


def branch(path, thick):
    img = Image.open(path).convert("RGBA")
    k = LENGTH / img.width
    return img.resize((LENGTH, max(1, round(img.height * k * thick))), Image.LANCZOS)


def wood_top(img):
    alpha = np.asarray(img)[..., 3]
    col = alpha[:, LENGTH // 2]
    return int(np.argmax(col > 128))


def main():
    day = Image.open(sys.argv[1]).convert("RGB").resize((1080, 1920), Image.LANCZOS)
    night = Image.open(sys.argv[2]).convert("RGB").resize((1080, 1920), Image.LANCZOS)
    left = branch(sys.argv[3], 1.25)
    right = branch(sys.argv[4], 1.25)
    top = wood_top(left)
    for world in range(1, 11):
        g = GRADES[world - 1]
        grade(day, g).save(os.path.join(OUT, "board_w%d_bg.png" % world))
        grade(night, g, 0.15).save(os.path.join(OUT, "board_w%d_night_bg.png" % world))
        slots = []
        for side, img in (("L", left), ("R", right)):
            for row in range(ROWS):
                name = "board_w%d_branch_%s%d" % (world, side, row)
                img.save(os.path.join(OUT, name + ".png"))
                slots.append({"side": side, "row": row, "name": name, "at": [0 if side == "L" else 1080 - LENGTH, 0],
                              "perch": float(top + 4), "tip": LENGTH if side == "L" else 1080 - LENGTH})
        json.dump({"world": world, "slots": slots}, open(os.path.join(OUT, "board_w%d_slots.json" % world), "w"), indent=1)
    print("wood top", top, left.size)


if __name__ == "__main__":
    main()
