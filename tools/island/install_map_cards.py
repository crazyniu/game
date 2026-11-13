#!/usr/bin/env python3
"""Installs the world-card art of the map carousel from a Codex job directory:
card_w<N>.png (scenic painting per world) -> PerchSort/world/card_scene_w<N>.png (768 x 1152, cover crop),
plank.png -> art/island/island_name_plank.png, gold_badge.png -> art/island/island_badge_gold.png (both trimmed to
their visible pixels), backdrop.png -> art/island/island_map_backdrop.png (1080 x 1920, cover crop).

python3 tools/island/install_map_cards.py <job dir>
"""
import os
import sys

from PIL import Image

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "PerchSort")


def cover(img, size):
    k = max(size[0] / img.width, size[1] / img.height)
    img = img.resize((round(img.width * k), round(img.height * k)), Image.LANCZOS)
    left = (img.width - size[0]) // 2
    top = (img.height - size[1]) // 2
    return img.crop((left, top, left + size[0], top + size[1]))


def trimmed(path, width):
    img = Image.open(path).convert("RGBA")
    alpha = img.getchannel("A").point(lambda a: 255 if a > 12 else 0)
    img = img.crop(alpha.getbbox())
    return img.resize((width, round(img.height * width / img.width)), Image.LANCZOS)


def main():
    job = sys.argv[1]
    for w in range(1, 11):
        src = os.path.join(job, "card_w%d.png" % w)
        if os.path.exists(src):
            cover(Image.open(src).convert("RGB"), (768, 1152)).save(os.path.join(ROOT, "world", "card_scene_w%d.png" % w))
    island = os.path.join(ROOT, "art", "island")
    if os.path.exists(os.path.join(job, "plank.png")):
        trimmed(os.path.join(job, "plank.png"), 900).save(os.path.join(island, "island_name_plank.png"))
    if os.path.exists(os.path.join(job, "gold_badge.png")):
        trimmed(os.path.join(job, "gold_badge.png"), 256).save(os.path.join(island, "island_badge_gold.png"))
    if os.path.exists(os.path.join(job, "backdrop.png")):
        cover(Image.open(os.path.join(job, "backdrop.png")).convert("RGB"), (1080, 1920)).save(
            os.path.join(island, "island_map_backdrop.png"))


if __name__ == "__main__":
    main()
