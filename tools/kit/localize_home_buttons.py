#!/usr/bin/env python3
"""Derive Chinese copies of the home and map tile pictures, whose English word is painted into the art.

uv run --no-project --isolated --with pillow --with numpy python tools/kit/localize_home_buttons.py
Reads PerchSort/art/home_button_*.png (never modified), erases the English word with the tile's own background and
letters the Chinese word with the same ink. Output: PerchSort/art/locale/zh/home_button_*.png.
"""
import numpy as np
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
from scipy.ndimage import binary_dilation

ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / "PerchSort/art"
FONT = str(ROOT / "tools/kit/fonts/zcool_kuaile.ttf")
WORDS = {"daily": "每日", "lucky_draw": "转盘", "map": "地图", "shop": "商店", "aviary": "鸟舍", "chest": "宝箱", "play": "开始"}
MAP_WORDS = {"help": "帮助"}


def tile(name, word, prefix="home_button_"):
    img = Image.open(ART / f"{prefix}{name}.png").convert("RGBA")
    a = np.array(img).astype(int)
    h, w = a.shape[:2]
    dark = (a[:, :, 0] < 110) & (a[:, :, 1] < 80) & (a[:, :, 2] < 60) & (a[:, :, 3] > 200)
    dark[:205] = False
    dark[:, :45] = False
    dark[:, w - 45:] = False
    ink = tuple(int(v) for v in np.median(a[dark][:, :3], axis=0))
    mask = binary_dilation(dark, iterations=5)
    for y in np.where(mask.any(axis=1))[0]:
        a[y, mask[y]] = np.median(a[y, 55:80], axis=0)
    img = Image.fromarray(a.astype("uint8"))
    ys, xs = np.where(dark)
    centre_x, base = (xs.min() + xs.max()) / 2, (264 if prefix == "home_button_" else ys.max() - 4)
    font = ImageFont.truetype(FONT, 66, layout_engine=ImageFont.Layout.RAQM)
    ImageDraw.Draw(img).text((centre_x, base), word, font=font, fill=ink + (255,), anchor="ms", language="zh")
    return img


def play(word):
    img = Image.open(ART / "home_button_play.png").convert("RGBA")
    a = np.array(img).astype(int)
    h, w = a.shape[:2]
    outline = (96, 58, 30)
    box = (250, 28, 735, 235)
    for y in range(box[1], box[3]):
        a[y, box[0]:box[2]] = np.median(a[y, 150:200], axis=0)
    img = Image.fromarray(a.astype("uint8"))
    font = ImageFont.truetype(FONT, 150, layout_engine=ImageFont.Layout.RAQM)
    ImageDraw.Draw(img).text((w / 2, 190), word, font=font, fill="white", anchor="ms", language="zh",
                             stroke_width=9, stroke_fill=outline)
    return img


def main():
    out = ART / "locale/zh"
    out.mkdir(parents=True, exist_ok=True)
    for name, word in MAP_WORDS.items():
        tile(name, word, "map_button_").save(out / f"map_button_{name}.png")
        print("wrote", name)
    for name, word in WORDS.items():
        (play(word) if name == "play" else tile(name, word)).save(out / f"home_button_{name}.png")
        print("wrote", name)


if __name__ == "__main__":
    main()
